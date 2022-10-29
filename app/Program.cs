using EmotionsLibrary;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] Images = {
                "face1.png",
                "face2.png"
            };

            Emotions e = new Emotions();
            CancellationToken ct = new CancellationToken();
            var res = e.RunTasks(Images, ct);
            foreach( KeyValuePair<string, Dictionary<string, float>> kvp in res.Result)
            {
                Console.WriteLine("Image: ", kvp.Key);
                foreach(var picture in kvp.Value)
                {
                    Console.WriteLine($"emotion: {picture.Key}  value: {picture.Value}");
                }
            }
        }
    }

}

