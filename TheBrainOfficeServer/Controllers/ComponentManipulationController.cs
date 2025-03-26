using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;
using TheBrainOfficeServer.Services;
namespace TheBrainOfficeServer.Controllers
{
    [Route("[controller]")]
    public class ComponentManipulationController : ControllerBase
    {
        private readonly InitializeRepo _initializeRepo;
        private readonly AppDBService _db;
        public ComponentManipulationController(InitializeRepo initializeRepo, AppDBService db)
        {
            _initializeRepo = initializeRepo;
            _db = db;
        }

        [HttpGet("Initialization")]
        public ActionResult ListComponents()
        {
            return Ok(_initializeRepo.TestRepo(_db));
        }
    }
}
