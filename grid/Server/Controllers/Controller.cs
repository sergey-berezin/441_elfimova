using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Contracts;

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
        public async Task<ActionResult<int>> PostImage([FromBody]ImgPost data, CancellationToken ct)
        {
            try
            {
                return await db.PostImage(data.blob, data.path, ct);
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(404, "Error occured while adding an image: "+ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(404, "Error occured while adding an image: "+ex.Message);
            }
        }
        [HttpGet]
        public List<int> GetAllImagesId()
        {
            return db.GetAllImagesId();
        }
        [HttpGet("{id}")]
        public ActionResult<ImgDataAndEmos> GetImageById(int id)
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