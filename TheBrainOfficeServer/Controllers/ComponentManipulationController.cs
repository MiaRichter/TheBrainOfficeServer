using Microsoft.AspNetCore.Mvc;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComponentManipulationController : ControllerBase
    {
        private readonly ComponentRepo _componentRepo;
        private readonly ILogger<ComponentManipulationController> _logger;

        public ComponentManipulationController(
            ComponentRepo componentRepo,
            ILogger<ComponentManipulationController> logger)
        {
            _componentRepo = componentRepo;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ComponentModel>> GetAllComponents()
        {
            try
            {
                var components = _componentRepo.ShowComponents();
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{componentId}")]
        public ActionResult<ComponentModel> GetComponent(string componentId)
        {
            try
            {
                var component = _componentRepo.GetComponentById(componentId);
                return component != null ? Ok(component) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting component {componentId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public ActionResult<ComponentModel> CreateComponent([FromBody] ComponentModel component)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newId = _componentRepo.CreateComponent(component);
                component.Id = newId;
                return CreatedAtAction(nameof(GetComponent), new { componentId = component.ComponentId }, component);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating component");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{componentId}")]
        public IActionResult UpdateComponent(string componentId, [FromBody] ComponentModel component)
        {
            if (componentId != component.ComponentId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return _componentRepo.UpdateComponent(component) ? NoContent() : StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating component {componentId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{componentId}")]
        public IActionResult DeleteComponent(string componentId, [FromQuery] bool permanent = false)
        {
            try
            {
                var success = permanent
                    ? _componentRepo.HardDeleteComponent(componentId)
                    : _componentRepo.SoftDeleteComponent(componentId);

                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting component {componentId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}