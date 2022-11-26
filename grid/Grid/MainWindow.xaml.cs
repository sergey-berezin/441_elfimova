using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.ComponentModel;
using EmotionsLibrary;
using System.Linq;
using System.Collections.ObjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Grid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        CancellationTokenSource cts;
        string[] files;
        private double progress_bar = 0;
        Dictionary<string, Dictionary<string, float>> result_dict;
        private static Emotions emo = new Emotions();

        public ObservableCollection<Image_info> neutralCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> happinessCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> surpriseCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> sadnessCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> angerCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> disgustCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> fearCollection = new ObservableCollection<Image_info>();
        public ObservableCollection<Image_info> contemptCollection = new ObservableCollection<Image_info>();

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
            neutral.ItemsSource = neutralCollection;
            happiness.ItemsSource = happinessCollection;
            surprise.ItemsSource = surpriseCollection;
            sadness.ItemsSource = sadnessCollection;
            anger.ItemsSource = angerCollection;
            disgust.ItemsSource = disgustCollection;
            fear.ItemsSource = fearCollection;
            contempt.ItemsSource = contemptCollection;
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
            Check.ItemsSource = files;
        }
        private string max(Dictionary<string, float> result_dict)
        {
            string max_emotion = "";
            float max_value = 0;
            foreach (var emotion in result_dict)
            {
                if (emotion.Value > max_value)
                {
                    max_value = emotion.Value;
                    max_emotion = emotion.Key;
                }
            }
            return max_emotion;
        }
        private async Task Process_image(string file, Dictionary<string, float> result_emotions, CancellationToken ct)
        {
            var task0 = Task.Run(async () => {
                result_emotions = await emo.EFP(file, ct);
            });
            await task0;
            return;
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
            Start_Button.IsEnabled = false;
            Folder_button.IsEnabled = false;
            Progress_Bar = 0.0;
            double step = 100.0 / files.Length;
            pbStatus.Foreground = Brushes.Lime;
            //Dictionary<string, float> result_emotions = new Dictionary<string, float>();
            result_dict = new Dictionary<string, Dictionary<string, float>>(files.Length);
            for (int i = 0; i < files.Length && !cts.IsCancellationRequested; i++)
            {
                try
                {
                    result_dict[files[i]] = new Dictionary<string, float>();
                    await Process_image(files[i], result_dict[files[i]], cts.Token);
                    Progress_Bar += step;
                }
                catch (OperationCanceledException)
                {
                    cts = new CancellationTokenSource();
                    pbStatus.Foreground = Brushes.OrangeRed;
                }
            }
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var name = max(result_dict[files[i]]);
                    if (name == "neutral")
                    {
                        neutralCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "happiness")
                    {
                        happinessCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "surprise")
                    {
                        surpriseCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "sadness")
                    {
                        sadnessCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "anger")
                    {
                        angerCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "disgust")
                    {
                        disgustCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "fear")
                    {
                        fearCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "contempt")
                    {
                        contemptCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    neutral.ItemsSource = neutralCollection;
                    happiness.ItemsSource = happinessCollection;
                    surprise.ItemsSource = surpriseCollection;
                    sadness.ItemsSource = sadnessCollection;
                    anger.ItemsSource = angerCollection;
                    disgust.ItemsSource = disgustCollection;
                    fear.ItemsSource = fearCollection;
                    contempt.ItemsSource = contemptCollection;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            Start_Button.IsEnabled = true;
            Folder_button.IsEnabled = true;
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
    }
}
