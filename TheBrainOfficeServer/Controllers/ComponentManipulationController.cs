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
        
        [HttpGet("StateLed")]
        public IActionResult StateLed()
        {
            bool state = false;
            if (state == false)
            {
                state = true;
            }
            else
            {
                state = false;
            }

            return Ok(componentRepo.SwitchState(state));
        }
        
        [HttpGet("DhtState")]
        public IActionResult DhtState() 
        {
            var reading = componentRepo.DHTState();
    
            if (reading.IsSuccessful)
            {
                return Ok(new
                {
                    success = true,
                    temperatureC = reading.TemperatureC,
                    temperatureF = reading.TemperatureF,
                    humidity = reading.Humidity,
                    heatIndexC = reading.HeatIndexC,
                    dewPointC = reading.DewPointC,
                    timestamp = DateTime.UtcNow
                });
            }
    
            return BadRequest(new
            {
                success = false,
                error = reading.ErrorMessage
            });
        }
        
    }
}