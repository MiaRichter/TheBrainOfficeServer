using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer.Controllers
{
    [Route("api/[controller]")]
    public class ComponentManipulationController : ControllerBase
    {
        private readonly ComponentRepo _componentRepo;

        public ComponentManipulationController(ComponentRepo componentRepo)
        {
            _componentRepo = componentRepo;
        }

        [HttpGet("ShowComponents")]
        public ActionResult<IEnumerable<ComponentModel>> GetAllComponents() => Ok(_componentRepo.ShowComponents());

        [HttpPost("Create")]
        public ActionResult CreateComponent([FromBody] ComponentModel component) => Ok(_componentRepo.CreateComponent(component));

        [HttpPut("Update/{componentId}")]
        public IActionResult UpdateComponent(string componentId, [FromBody] ComponentModel component) => Ok(_componentRepo.UpdateComponent(component));

        [HttpDelete("Delete/{componentId}")]
        public IActionResult DeleteComponent([FromRoute] string componentId) => Ok(_componentRepo.DeleteComponent(componentId));
    }
}