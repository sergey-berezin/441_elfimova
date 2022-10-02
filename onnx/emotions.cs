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
    public static class Emotions
    {
        public static void EFP(Image<Rgb24> image)
        {
            using var modelStream = typeof(Emotions).Assembly.GetManifestResourceStream("emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            using var session = new InferenceSession(memoryStream.ToArray()); 
            foreach(var kv in session.InputMetadata)
                Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}");
            foreach(var kv in session.OutputMetadata)
                Console.WriteLine($"{kv.Key}: {MetadataToString(kv.Value)}]");

            image.Mutate(ctx => {
                ctx.Resize(new Size(64,64));
            });

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

            string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            foreach(var i in keys.Zip(emotions))
                Console.WriteLine($"{i.First}: {i.Second}");
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