using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;


// В проектах SDK, таких как этот, некоторые атрибуты сборки, которые ранее определялись
// в этом файле, теперь автоматически добавляются во время сборки и заполняются значениями,
// заданными в свойствах проекта. Подробные сведения о том, какие атрибуты включены
// и как настроить этот процесс, см. на странице: https://aka.ms/assembly-info-properties.


// При установке значения false для параметра ComVisible типы в этой сборке становятся
// невидимыми для компонентов COM. Если вам необходимо получить доступ к типу в этой
// сборке из модели COM, установите значение true для атрибута ComVisible этого типа.

[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов typelib, если этот проект
// будет видимым для COM.

[assembly: Guid("0356eb2d-231c-42bf-9ca3-5b263ebba0c6")]

namespace Server
{
    public class ImagesTable
    {
        [Key]
        public int fileId { get; set; }
        public string fileName { get; set; }
        public string imgPath { get; set; }
        public byte[] hashCode { get; set; }
        public byte[] blob { get; set; }
        public ImagesTable() {}
        public ImagesTable(string name, string path, byte[] hash, byte[] blobFile)
        {
            fileName = name;
            imgPath = path;
            hashCode = hash;
            blob = blobFile;
        }
    }

    public class EmotionsTable
    {
        [Key]
        [ForeignKey(nameof(ImagesTable))]
        public int fileId { get; set; }
        public string fileName { get; set; }
        public float neutral { get; set; }
        public float happiness { get; set; }
        public float surprise { get; set; }
        public float sadness { get; set; }
        public float anger { get; set; }
        public float disgust { get; set; }
        public float fear { get; set; }
        public float contempt { get; set; }
        public EmotionsTable() {}
        public EmotionsTable(int imageId, string name, Dictionary<string, float> emotions)
        {
            fileId = imageId;
            fileName = name;
            neutral = emotions["neutral"];
            happiness = emotions["happiness"];
            surprise = emotions["surprise"];
            sadness = emotions["sadness"];
            anger = emotions["anger"];
            disgust = emotions["disgust"];
            fear = emotions["fear"];
            contempt = emotions["contempt"];
        }
    }

    public class ImagesContext : DbContext
    {
        public DbSet<ImagesTable> Images { get; set; }
        public DbSet<EmotionsTable> Emotions { get; set; }
        public ImagesContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=images.db");
        }
    }
}
