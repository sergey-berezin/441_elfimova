using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


public class ImagesTable
{
    [Key]
    public int imageId { get; set; }
    public string fileName { get; set; }
    public string imgPath { get; set; }
    public int hashCode { get; set; }
    public byte[] blob { get; set; }
    public ImagesTable() { }
    public ImagesTable(string name, string path, int hash)
    {
        fileName = name;
        imgPath = path;
        hashCode = hash;
    }
}

public class EmotionsTable
{
    [Key]
    public int imageId { get; set; }
    //public Dictionary<string, float> emotions { get; set; }
    public float neutral { get; set; }
    public float happiness { get; set; }
    public float surprise { get; set; }
    public float sadness { get; set; }
    public float anger { get; set; }
    public float disgust { get; set; }
    public float fear { get; set; }
    public float contempt { get; set; }
    public EmotionsTable() { }
    public EmotionsTable(int imageId, Dictionary<string, float> emotions)
    {
        this.imageId = imageId;
        neutral = emotions["neutral"];
        happiness = emotions["happiness"];
        surprise = emotions["surprise"];
        sadness = emotions["sadness"];
        anger = emotions["anger"];
        disgust = emotions["disgust"];
        fear = emotions["fear"];
        contempt = emotions["contempt"];
        //this.emotions = emotions;
    }
}

public class ImagesContext : DbContext
{
    public DbSet<ImagesTable> Images { get; set; }
    public DbSet<EmotionsTable> Emotions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlite("Data Source=images.db");
    }
}