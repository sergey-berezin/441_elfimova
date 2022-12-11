using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.ComponentModel;
using EmotionsLibrary;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection.Metadata;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

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
        private SemaphoreSlim semaphore;
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
            semaphore = new SemaphoreSlim(1, 1);
            cts = new CancellationTokenSource();
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            neutralCollection = new ObservableCollection<Image_info>();
            happinessCollection = new ObservableCollection<Image_info>();
            surpriseCollection = new ObservableCollection<Image_info>();
            sadnessCollection = new ObservableCollection<Image_info>();
            angerCollection = new ObservableCollection<Image_info>();
            disgustCollection = new ObservableCollection<Image_info>();
            fearCollection = new ObservableCollection<Image_info>();
            contemptCollection = new ObservableCollection<Image_info>();
            neutral.ItemsSource = neutralCollection;
            happiness.ItemsSource = happinessCollection;
            surprise.ItemsSource = surpriseCollection;
            sadness.ItemsSource = sadnessCollection;
            anger.ItemsSource = angerCollection;
            disgust.ItemsSource = disgustCollection;
            fear.ItemsSource = fearCollection;
            contempt.ItemsSource = contemptCollection;
        }
        private void Choose_folder(object sender, RoutedEventArgs e)
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
        List<int> chooseFiles()
        {
            List<int> res = new List<int>();
            for (int i = 0; i < files.Length; i++)
            {
                byte[] blobFile = File.ReadAllBytes(files[i]);
                HashAlgorithm sha = SHA256.Create();
                var hashCode = sha.ComputeHash(blobFile);
                using (var db = new ImagesContext())
                {
                    if ((db.Images.Any(x => x.hashCode == hashCode)) & (db.Images.Any(x => x.blob == blobFile)))
                    {
                        res.Add(1);
                    }
                    else
                    {
                        res.Add(0);
                    }
                }
            }
            return res;
        }
        private void AddToDb(int i, byte[] blobFile)
        {
            using (var db = new ImagesContext())
            {
                HashAlgorithm sha = SHA256.Create();
                db.Add(new ImagesTable
                {
                    fileName = files[i],
                    imgPath = files[i],
                    blob = blobFile,
                    hashCode = sha.ComputeHash(blobFile)
                });
                db.Add(new EmotionsTable
                {
                    fileName = files[i],
                    neutral = result_dict[files[i]]["neutral"],
                    happiness = result_dict[files[i]]["happiness"],
                    surprise = result_dict[files[i]]["surprise"],
                    sadness = result_dict[files[i]]["sadness"],
                    anger = result_dict[files[i]]["anger"],
                    disgust = result_dict[files[i]]["disgust"],
                    fear = result_dict[files[i]]["fear"],
                    contempt = result_dict[files[i]]["contempt"],
                });
                db.SaveChanges();
            }
        }
        private async void Run(object sender, RoutedEventArgs e)
        {
            Start_button.IsEnabled = false;
            Folder_button.IsEnabled = false;
            Clear_button.IsEnabled = false;
            Progress_Bar = 0.0;
            double step = 100.0 / files.Length;
            pbStatus.Foreground = Brushes.Lime;
            result_dict = new Dictionary<string, Dictionary<string, float>>(files.Length);
            List<int> chosen_files = chooseFiles();
            for (int i = 0; i < files.Length && !cts.IsCancellationRequested; i++)
            {
                try
                {
                    result_dict[files[i]] = new Dictionary<string, float>();
                    if (chosen_files[i] == 0)
                    {
                        result_dict[files[i]] = await emo.EFP(files[i], cts.Token);
                    }
                    else
                    {
                        await semaphore.WaitAsync();
                        using (var db = new ImagesContext())
                        {
                            var query2 = db.Emotions.Where(x => x.fileName == files[i]);
                            foreach (EmotionsTable q2 in query2)
                            {
                                result_dict[files[i]]["neutral"] = q2.neutral;
                                result_dict[files[i]]["happiness"] = q2.happiness;
                                result_dict[files[i]]["surprise"] = q2.surprise;
                                result_dict[files[i]]["sadness"] = q2.sadness;
                                result_dict[files[i]]["anger"] = q2.anger;
                                result_dict[files[i]]["disgust"] = q2.disgust;
                                result_dict[files[i]]["fear"] = q2.fear;
                                result_dict[files[i]]["contempt"] = q2.contempt;
                            }
                        }
                        semaphore.Release();
                    }
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
                    byte[] blobFile = File.ReadAllBytes(files[i]);
                    if (name == "neutral")
                    {
                        AddToDb(i, blobFile);
                        neutralCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "happiness")
                    {
                        AddToDb(i, blobFile);
                        happinessCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "surprise")
                    {
                        AddToDb(i, blobFile);
                        surpriseCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "sadness")
                    {
                        AddToDb(i, blobFile);
                        sadnessCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "anger")
                    {
                        AddToDb(i, blobFile);
                        angerCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "disgust")
                    {
                        AddToDb(i, blobFile);
                        disgustCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "fear")
                    {
                        AddToDb(i, blobFile);
                        fearCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                    else if (name == "contempt")
                    {
                        AddToDb(i, blobFile);
                        contemptCollection.Add(new Image_info(files[i], result_dict[files[i]]));
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            Start_button.IsEnabled = true;
            Folder_button.IsEnabled = true;
            Clear_button.IsEnabled = true;
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
    }
}
