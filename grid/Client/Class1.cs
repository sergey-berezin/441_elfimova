using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Grid
{
    public class Image_info
    {
        public string ImgPath { get; set; }
        public Dictionary<string, float> Emos { get; set; }
        public Image_info(string i, Dictionary<string, float> e)
        {
            ImgPath = i;
            Emos = new Dictionary<string, float>();
            Emos = e;
        }
    }
}
