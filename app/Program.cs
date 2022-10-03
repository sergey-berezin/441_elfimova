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
            "images/chan1.jpg",
            "images/chan2.jpg",
            "images/depp1.jpg",
            "images/depp2.jpg",
            "images/dicaprio1.jpg",
            "images/dicaprio2.jpg"
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
