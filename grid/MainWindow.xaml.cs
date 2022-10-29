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
    public partial class MainWindow : Window
    {
        Emotions emo;
        CancellationTokenSource cts; 
        string[] files;
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

        private async void Process_image(string file, CancellationToken ct)
        { 
            Dictionary<string, float> result_dict;
            var task0 = Task.Run(async () => {
                result_dict = await emo.EFP(file, ct);
            });
        }

        private async void Run(object sender, RoutedEventArgs e)
        {
            //wpf progress bar
            Star_Button.IsEnabled = false;
            Folder_Buttom.IsEnabled = false;
            for(int i = 0; i < files.Length; i++)
            {
                Process_image(files[i], cts.Token);
                pbStatus.Value++;
            }
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
    }
}