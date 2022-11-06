using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using EmotionsLibrary;

namespace Grid
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
         protected void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        Emotions emo;
        CancellationTokenSource cts; 
        string[] files;
        private double progress_bar = 0;
        public double Progress_Bar
        {
            get { return progress_bar; }
            set 
            {
                progress_bar = value;
                RaisePropertyChanged("Progress_Bar");
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            emo = new Emotions();
            cts = new CancellationTokenSource();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dlg.Multiselect = true;
            var result = dlg.ShowDialog();
            if (result == true)
            {
                files = dlg.FileNames;
            }
                
        }
        private string max(Dictionary<string, float> result_dict)
        {
            string max_emotion = "";
            float max_value = 0;
            foreach(var emotion in result_dict)
            {
                if (emotion.Value > max_value)
                {
                    max_value = emotion.Value;
                    max_emotion = emotion.Key;
                }
            }
            return max_emotion;
        }
        private async Task Process_image(string file, CancellationToken ct)
        { 
            Dictionary<string, float> result_dict;
            var task0 = Task.Run(async () => {
                result_dict = await emo.EFP(file, ct);
                var name = max(result_dict);
                if(name == "neutral")
                    neutral.DataContext = file;
                else if (name == "happiness")
                    happiness.DataContext = file;
                else if (name == "surprise")
                    surprise.DataContext = file;
                else if (name == "sadness")
                    sadness.DataContext = file;
                else if (name == "anger")
                    anger.DataContext = file;
                else if (name == "disgust")
                    disgust.DataContext = file;
                else if (name == "fear")
                    fear.DataContext = file;  
                else if (name == "contempt")
                    contempt.DataContext = file;
            });
            await task0;
            return task0;
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
            Start_Button.IsEnabled = false;
            Folder_Button.IsEnabled = false;
            Progress_Bar = 0.0;
            double step = 100.0 / files.Length;
            pbStatus.Foreground = Brushes.Lime;
            try
            {
                for(int i = 0; i < files.Length; i++)
                {
                    await Process_image(files[i], cts.Token);
                    Progress_Bar += step;
                }
            }
            catch(OperationCanceledException)
            {
                cts = new CancellationTokenSource();
                pbStatus.Foreground = Brushes.OrangeRed;
            }
            Start_Button.IsEnabled = true;
            Folder_Button.IsEnabled = true;
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
    }
}