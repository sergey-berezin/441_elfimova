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
        async void Main(string[] args)
        {
            string[] Images = {
                "face1.png",
                "face2.png"
            };

            Emotions e = new Emotions();
            CancellationToken ct = new CancellationToken();
            Dictionary<string, Dictionary<string, float>> res = new Dictionary<string, Dictionary<string, float>>();
            Dictionary<string, float> result_dict;
            for(int i = 0; i < Images.Length; i++)
            {
                var task0 = Task.Run(async () => {
                    result_dict = await e.EFP(Images[i], ct);
                    res[Images[i]] = result_dict;
                });
                await task0;
            }
            
            foreach( KeyValuePair<string, Dictionary<string, float>> kvp in res)
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

