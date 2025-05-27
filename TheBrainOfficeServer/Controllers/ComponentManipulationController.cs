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
        
        [HttpPost("register")]
        public IActionResult RegisterComponent([FromBody] ComponentModel request)
        {
            try
            {
                // Проверяем существование компонента
                var existing = componentRepo.GetComponentById(request.ComponentId);
            
                if (existing == null)
                {
                    // Создаем новый компонент
                    var component = new ComponentModel
                    {
                        ComponentId = request.ComponentId,
                        Name = request.Name,
                        Description = request.Description,
                        ComponentType = request.ComponentType,
                        Location = request.Location
                    };
                
                    componentRepo.CreateComponent(component);
                    return Ok(existing);
                }
                else
                {
                    // Обновляем существующий компонент
                    existing.Name = request.Name;
                    existing.Description = request.Description;
                    existing.ComponentType = request.ComponentType;
                    existing.Location = request.Location;
                
                    componentRepo.UpdateComponent(existing);
                    return Ok($"{existing}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error registering component {request.ComponentId}");
            }
        }
    }
}