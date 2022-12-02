using System;
using SixLabors.ImageSharp; 
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;

namespace EmotionsLibrary
{
    public class Emotions
    {
        private InferenceSession session;
        private SemaphoreSlim sessionLock;
        public Emotions()
        {
            using var modelStream = typeof(Emotions).Assembly.GetManifestResourceStream("EmotionFerPlus.emotion-ferplus-8.onnx");
            using var memoryStream = new MemoryStream();
            if (modelStream is not null)
                modelStream.CopyTo(memoryStream);
            else
                throw new Exception("modelStream error");
            this.session = new InferenceSession(memoryStream.ToArray());
            this.sessionLock = new SemaphoreSlim(1, 1);
        }

        public async Task<Dictionary<string, Dictionary<string, float>>> RunTasks(string[] Images, CancellationToken ct)
        {
            var res = new Dictionary<string, Dictionary<string, float>>();
            var task0 = Task.Run(async () => {
                var s = await EFP(Images[0], ct);
                res.Add(Images[0], s);
            });
            var task1 = Task.Run(async () => {
                var s = await EFP(Images[1], ct);
                res.Add(Images[1], s);
            });
            await Task.WhenAll(task0, task1);
            return res;
        }
        public async Task<Dictionary<string, float>> EFP(string arg,  CancellationToken ct)
        {
            using Image<Rgb24> image = Image.Load<Rgb24>(arg);
            image.Mutate(ctx => {
                ctx.Resize(new Size(64,64));
            });
           
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
            await sessionLock.WaitAsync();
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = this.session.Run(inputs);
            sessionLock.Release();
            var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

            string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            var emotions_dict = new Dictionary<string, float>();
            if(!ct.IsCancellationRequested)
            {
                for(int i = 0; i < keys.Count(); i++)
                {
                    emotions_dict[keys[i]] = emotions[i];
                }
            }
            return emotions_dict;
        }

        public DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa => 
            {
                for (int y = 0; y < h; y++)
                {           
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R; 
                    }
                }
            });
            
            return t;
        }

        public string MetadataToString(NodeMetadata metadata)
            => $"{metadata.ElementType}[{String.Join(",", metadata.Dimensions.Select(i => i.ToString()))}]";

        public float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
    }
}