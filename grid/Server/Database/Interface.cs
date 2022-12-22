using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using EmotionsLibrary;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using Contracts;

namespace Server
{
    public interface IImagesInterface
    {
        Task<int> PostImage(byte[] img, string local_fileName, CancellationToken ct);
        List<int> GetAllImagesId();
        ImgDataAndEmos? GetImageById(int id);
        Task DeleteAllImages();
    }
    public class DbWorker : IImagesInterface
    {
        private Emotions emo = new Emotions();
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public async Task<int> PostImage(byte[] img, string local_fileName, CancellationToken ct)
        {
            try
            {
                await semaphore.WaitAsync();
                int id = -1;
                using (var db = new ImagesContext())
                {
                    HashAlgorithm sha = SHA256.Create();
                    var local_hashCode = sha.ComputeHash(img);
                    if (db.Images.Any(x => x.fileName == local_fileName))
                    {
                        var query = db.Images.Where(x => x.blob == img && x.hashCode == local_hashCode).FirstOrDefault();
                        if(query != null)
                        {
                            id = query.fileId;
                        }
                    }
                    else
                    {
                        Dictionary<string, float> result_dict = await emo.EFP(local_fileName, ct);
                        ImagesTable image = new ImagesTable
                        (
                            local_fileName,
                            local_fileName,
                            img,
                            local_hashCode
                        );
                        db.Add(image);
                        db.Add(new EmotionsTable
                        (
                            image.fileId,
                            local_fileName,
                            result_dict
                        ));
                        db.SaveChanges();
                        id = image.fileId;
                    }
                }
                semaphore.Release();
                return id;
            }
            catch(OperationCanceledException)
            {
                throw new OperationCanceledException("Data processing has been canceled.");
            }
            catch (Exception)
            {
                throw new Exception("Caught an error while processing image.");
            }
        }
        public List<int> GetAllImagesId()
        {
            try
            {
                using (var db = new ImagesContext())
                {
                    var images = db.Images.Select(item => item.fileId).ToList();
                    return images;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Caught an error while getting images' ids: "+ex.Message);
            }
        }
        public ImgDataAndEmos? GetImageById(int id)
        {
            try
            {
                ImgDataAndEmos result = new ImgDataAndEmos();
                using (var db = new ImagesContext())
                {
                    var query1 = db.Images.Where(x => x.fileId == id).FirstOrDefault();
                    if(query1 != null)
                    {
                        result.fileName = query1.fileName;
                        result.imgPath = query1.imgPath;
                        result.blob = query1.blob;
                    }
                    var query2 = db.Emotions.Where(x => x.fileId == id).FirstOrDefault();
                    if (query2 != null)
                    {
                        result.neutral = query2.neutral;
                        result.happiness = query2.happiness;
                        result.surprise = query2.surprise;
                        result.sadness = query2.sadness;
                        result.anger = query2.anger;
                        result.disgust = query2.disgust;
                        result.fear = query2.fear;
                        result.contempt = query2.contempt;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw new Exception("Caught an error while getting image.");
            }
        }
        public async Task DeleteAllImages()
        {
            try
            {
                await semaphore.WaitAsync();
                using (var db = new ImagesContext())
                {
                    await db.Database.ExecuteSqlRawAsync("DELETE FROM [Images]");
                    await db.Database.ExecuteSqlRawAsync("DELETE FROM [Emotions]");
                }
                semaphore.Release();
            }
            catch (Exception)
            {
                throw new Exception("Caught an error while delliting.");
            }
        }
    }

}