using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer.Controllers
{
    [Route("api/[controller]")]
    public class ComponentManipulationController(ComponentRepo componentRepo) : ControllerBase
    {
        [HttpGet("ShowComponents")]
        public ActionResult<IEnumerable<ComponentModel>> GetAllComponents() => Ok(componentRepo.ShowComponents());

        [HttpPost("Create")]
        public ActionResult CreateComponent([FromBody] ComponentModel component) => Ok(componentRepo.CreateComponent(component));

        [HttpPut("Update")]
        public IActionResult UpdateComponent([FromBody] ComponentModel component) => Ok(componentRepo.UpdateComponent(component));

        [HttpDelete("Delete/{componentId}")]
        public IActionResult DeleteComponent([FromRoute] string componentId) => Ok(componentRepo.DeleteComponent(componentId));[HttpDelete("Delete/{componentId}")]
        
        [HttpGet("BrainHome/{PortId}")]
        public IActionResult StateLed([FromRoute] string PortId)
        {
            return Ok(componentRepo.DhtState(PortId));
        }
    }
}