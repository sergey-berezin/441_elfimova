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
using Polly;
using Polly.Retry;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Windows.Markup;
using System.Net;
using Contracts;

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
        private string url = "http://localhost:5001";
        private AsyncRetryPolicy _retryPolicy;
        private int maxRetries = 3;
        List<int> ids = new List<int>();

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
            Check.ItemsSource = ids;
            semaphore = new SemaphoreSlim(1, 1);
            cts = new CancellationTokenSource();
            _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(maxRetries, times =>
               TimeSpan.FromMilliseconds(3000));
        }
        private void Clear_Collections()
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
            ids = new List<int>();
            Check.ItemsSource = ids;
        }
        private async void Clear(object sender, RoutedEventArgs e)
        {
            try
            {
                Clear_Collections();
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.DeleteAsync($"{url}/images");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Images are deleted");
                    }
                    else
                    {
                        MessageBox.Show(response.ReasonPhrase);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
        private string max(ImgDataAndEmos fi)
        {
            string max_emotion = "";
            float max_value = 0;
            if(fi.neutral > max_value)
            {
                max_value = fi.neutral;
                max_emotion = "neutral";
            }
            if (fi.happiness > max_value)
            {
                max_value = fi.happiness;
                max_emotion = "happiness";
            }
            if (fi.surprise > max_value)
            {
                max_value = fi.surprise;
                max_emotion = "surprise";
            }
            if (fi.sadness > max_value)
            {
                max_value = fi.sadness;
                max_emotion = "sadness";
            }
            if (fi.anger > max_value)
            {
                max_value = fi.anger;
                max_emotion = "anger";
            }
            if (fi.disgust > max_value)
            {
                max_value = fi.disgust;
                max_emotion = "disgust";
            }
            if (fi.fear > max_value)
            {
                max_value = fi.fear;
                max_emotion = "fear";
            }
            if (fi.contempt > max_value)
            {
                max_value = fi.contempt;
                max_emotion = "contempt";
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

        private async void Run(object sender, RoutedEventArgs e)
        {
            Start_button.IsEnabled = false;
            Folder_button.IsEnabled = false;
            Clear_button.IsEnabled = false;
            Ids_button.IsEnabled = false;
            Progress_Bar = 0.0;
            double step = 100.0 / files.Length;
            pbStatus.Foreground = Brushes.Lime;
            result_dict = new Dictionary<string, Dictionary<string, float>>(files.Length);
            List<int> chosen_files = chooseFiles();
            for (int i = 0; i < files.Length && !cts.IsCancellationRequested; i++)
            {
                try
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        var httpClient = new HttpClient();
                        httpClient.BaseAddress = new Uri($"{url}/images");
                        byte[] blobFile = await File.ReadAllBytesAsync(files[i], cts.Token);
                        var img_data = new ImgPost(blobFile, files[i]);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await HttpClientJsonExtensions.PostAsJsonAsync(httpClient, "", img_data, cts.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Success");
                            if (response.StatusCode == HttpStatusCode.Created)
                            {
                                MessageBox.Show("Created");
                            }
                            else if (response.StatusCode == HttpStatusCode.NoContent)
                                throw new OperationCanceledException("No images");
                        }
                        else
                        {
                            MessageBox.Show("Not Success: " + response.ReasonPhrase);
                        }
                    });
                    Progress_Bar += step;
                }
                catch(OperationCanceledException ex)
                {
                    cts = new CancellationTokenSource();
                    pbStatus.Foreground = Brushes.OrangeRed;
                    MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    cts = new CancellationTokenSource();
                    pbStatus.Foreground = Brushes.OrangeRed;
                    MessageBox.Show(ex.Message);
                }
            }
            Start_button.IsEnabled = true;
            Folder_button.IsEnabled = true;
            Clear_button.IsEnabled = true;
            Ids_button.IsEnabled = true;
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
        private Image_info make_image_info(ImgDataAndEmos fi)
        {
            Dictionary<string, float> result_emos = new Dictionary<string, float>();
            result_emos["neutral"] = fi.neutral;
            result_emos["happiness"] = fi.happiness;
            result_emos["surprise"] = fi.surprise;
            result_emos["sadness"] = fi.sadness;
            result_emos["anger"] = fi.anger;
            result_emos["disgust"] = fi.disgust;
            result_emos["fear"] = fi.fear;
            result_emos["contempt"] = fi.contempt;
            return new Image_info(fi.fileName, result_emos);
        }
        private async void Ids_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync($"{url}/images");
                    Clear_Collections();
                    if (response.IsSuccessStatusCode)
                    {
                        ids = await response.Content.ReadFromJsonAsync<List<int>>();
                        Check.ItemsSource = ids;
                        if (ids.Count ==0)
                        {
                            MessageBox.Show("No images");
                        }
                        foreach (int id in ids)
                        {
                            var image_info = await httpClient.GetAsync($"{url}/images/{id}");
                            ImgDataAndEmos info = await image_info.Content.ReadFromJsonAsync<ImgDataAndEmos>();
                            string max_emotion = max(info);
                            Image_info new_item = make_image_info(info);
                            if(max_emotion == "neutral")
                            {
                                neutralCollection.Add(new_item);
                            }
                            else if (max_emotion == "happiness")
                            {
                                happinessCollection.Add(new_item);
                            }
                            else if (max_emotion == "surprise")
                            {
                                surpriseCollection.Add(new_item);
                            }
                            else if (max_emotion == "sadness")
                            {
                                sadnessCollection.Add(new_item);
                            }
                            else if (max_emotion == "anger")
                            {
                                angerCollection.Add(new_item);
                            }
                            else if (max_emotion == "disgust")
                            {
                                disgustCollection.Add(new_item);
                            }
                            else if (max_emotion == "fear")
                            {
                                fearCollection.Add(new_item);
                            }
                            else if (max_emotion == "contempt")
                            {
                                contemptCollection.Add(new_item);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(response.ReasonPhrase);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
