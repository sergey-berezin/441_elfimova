using emotions;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main(string[] args)
    {
        string[] Images = {
            "face1.png",
            "face2.png"
        };

        Emotions e = new Emotions();
        for(int i = 0; i < testImages.GetLength(); i++)
        {
            var res = e.EFP(Images[i]);
            Console.WriteLine("Image: ", i);
            foreach(var picture in res)
            {
                Console.WriteLine($"emotion: {picture.Key}  value: {picture.Value}");
            }
        }
    }
}
