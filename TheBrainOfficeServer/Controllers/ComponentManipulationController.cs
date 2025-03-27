using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer.Controllers
{
    [Route("api/[controller]")]
    public class ComponentManipulationController : ControllerBase
    {
        private readonly ComponentRepo _componentRepo;
        private readonly ILogger<ComponentManipulationController> _logger;

        public ComponentManipulationController(ComponentRepo componentRepo, ILogger<ComponentManipulationController> logger)
        {
            _componentRepo = componentRepo;
            _logger = logger;
        }

        [HttpGet("ShowComponents")]
        public ActionResult<IEnumerable<ComponentModel>> GetAllComponents()
        {
            try
            {
                var components = _componentRepo.ShowComponents();
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error getting components");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Create")]
        public ActionResult CreateComponent([FromBody] ComponentModel component)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(_componentRepo.CreateComponent(component));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating component");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("Update/{componentId}")]
        public IActionResult UpdateComponent(string componentId, [FromBody] ComponentModel component)
        {
            if (componentId != component.ComponentId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(_componentRepo.UpdateComponent(component));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error updating component {componentId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("Delete/{componentId}")]
        public IActionResult DeleteComponent([FromRoute] string componentId)
        {
            try
            {
                return Ok(_componentRepo.DeleteComponent(componentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error deleting component {componentId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}