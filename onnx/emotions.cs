using System;
using SixLabors.ImageSharp; 
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.IO;


namespace emotions
{
    public class Emotions
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            using var imageStream = File.OpenRead(args.FirstOrDefault() ?? "sample.jpg");
            var task = Task.Factory.StartNew(async () => {
                while(!cts.Token.IsCancellationRequested) {
                    await EFP(args.FirstOrDefault() ?? "sample.jpg");
                }
            }, cts.Token);
            await Task.WhenAll(task);
            cts.Cancel();
        }
        public static async Task<IEnumerable<(string First, float Second)>> EFP(string arg)
        {
            var r = new TaskCompletionSource<IEnumerable<(string First, float Second)>>();
            using Image<Rgb24> image = Image.Load<Rgb24>(arg);
            image.Mutate(ctx => {
                ctx.Resize(new Size(64,64));
            });
            using var modelStream = typeof(Emotions).Assembly.GetManifestResourceStream("emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            /*foreach(var kv in session.InputMetadata)
                Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}");
            foreach(var kv in session.OutputMetadata)
                Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}]");*/
            using var session = new InferenceSession(memoryStream.ToArray());
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

            string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            keys.Zip(emotions);
            return await r.Task;
        }

        public static DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
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

        public static string MetadataToString(NodeMetadata metadata)
            => $"{metadata.ElementType}[{String.Join(",", metadata.Dimensions.Select(i => i.ToString()))}]";

        public static float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
    }
}