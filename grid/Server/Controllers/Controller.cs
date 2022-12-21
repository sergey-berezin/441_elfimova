using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace Server.Controllers
{
    [ApiController]
    [Route("images")]
    public class ServerController : ControllerBase
    {
        private IImagesInterface db;
        public ServerController(IImagesInterface db)
        {
            this.db = db;
        }
        [HttpPost]
        public async Task<ActionResult<int>> PostImage(byte[] img, string local_fileName, CancellationToken ct)
        {
            try
            {
                return await db.PostImage(img, local_fileName, ct);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(404, "Error occured while adding an image");
            }
            catch (Exception)
            {
                return StatusCode(404, "Error occured while adding an image");
            }
        }
        [HttpGet]
        public List<int> GetAllImagesId()
        {
            return db.GetAllImagesId();
        }
        [HttpGet("{id}")]
        public ActionResult<FileInfo> GetImageById(int id)
        {
            var result = db.GetImageById(id);
            if (result == null)
            {
                return StatusCode(404, "Id is not found");
            }
            return result;
        }
        [HttpDelete]
        public async Task DeleteAllImages()
        {
            await db.DeleteAllImages();
        }
    }
}