using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
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
    public struct ImgDataAndEmos
    {
        public int fileId { get; set; }
        public string fileName { get; set; }
        public string imgPath { get; set; }
        public byte[] blob { get; set; }
        public float neutral { get; set; }
        public float happiness { get; set; }
        public float surprise { get; set; }
        public float sadness { get; set; }
        public float anger { get; set; }
        public float disgust { get; set; }
        public float fear { get; set; }
        public float contempt { get; set; }
    }
    public class ImgPost
    {
        public byte[] blob { get; set; }
        public string path { get; set; }

        public ImgPost() { }

        public ImgPost(byte[] blob, string path)
        {
            this.blob = blob;
            this.path = path;
        }
    }
}