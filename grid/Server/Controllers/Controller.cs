using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Net;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Database;
using System.Threading;

namespace Server.Controllers
{
    [ApiController]
    [Route("images")]
    [Produces("application/json")]
    public class ServerController : ControllerBase
    {
        private IDatabase db;
        public ServerControllers(IDatabase db, ILogger<ServerControllers> logger)
        {
            this.db = db;
        }
        [HttpPost]
        public async Task<(int, bool)> AddPhoto((byte[], string) obj, CancellationToken ct)
        {
            var img = obj.Item1;
            var name = obj.Item2;
            return await dB.PostImage(img, ct, name);
        }
        [HttpGet]
        public string Get(string name)
        {
            return String.Concat("Hello, ", name, "!");
        }
    }
}