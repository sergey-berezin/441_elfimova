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
        var res = e.RunTasks(Images);
        for(int i = 0; i < res.GetLength(); i++)
        {
            Console.WriteLine("Image: ", i);
            foreach(var picture in res[i])
            {
                Console.WriteLine($"emotion: {picture.Key}  value: {picture.Value}");
            }
        }
    }
}
