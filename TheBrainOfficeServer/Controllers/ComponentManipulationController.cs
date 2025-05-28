using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer.Controllers
{
    [Route("api/[controller]")]
    public class ComponentManipulationController(ComponentRepo componentRepo) : ControllerBase
    {
        [HttpPost("Create")]
        public ActionResult CreateComponent() => Ok(componentRepo.CreateComponent());
    }
}