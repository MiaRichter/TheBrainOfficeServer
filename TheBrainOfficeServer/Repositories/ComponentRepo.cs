using System.Device.Gpio;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Services;

namespace TheBrainOfficeServer.Repositories
{
    public class ComponentRepo
    {
        private readonly AppDBService _db;
        private readonly GpioController _gpio;
        public ComponentRepo(AppDBService db)
        {
            _db = db;
            _gpio = new GpioController(PinNumberingScheme.Logical);
        }

        public List<ComponentModel> ShowComponents() //показать доступные компоненты
        {

            string query = @"
                SELECT 
                    id, 
                    component_id AS ComponentId,
                    name,
                    description,
                    component_type AS ComponentType,
                    location,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    is_active AS IsActive
                FROM components
                WHERE is_active = true
                ORDER BY created_at DESC";

            return _db.GetList<ComponentModel>(query);
        }

        public string CreateComponent(ComponentModel component)
        {
            const string query = @"
                INSERT INTO components 
                    (component_Id, name, description, component_type, location)
                VALUES 
                    (@ComponentId, @Name, @Description, @ComponentType, @Location)
                RETURNING id";

            return _db.GetScalar<int>(query, new
            {
                component.ComponentId,
                component.Name,
                component.Description,
                component.ComponentType,
                component.Location
            }).ToString(); //создать компонент
        }

        public bool UpdateComponent(ComponentModel component)  //обновить компонент
        {
            const string query = $@"
                UPDATE components
                SET 
                    name = @Name,
                    description = @Description,
                    component_type = @ComponentType,
                    location = @Location,
                    updated_at = NOW()
                WHERE component_id = @ComponentId";
            var parameters = new
            {
                component.Name,
                component.Description,
                component.ComponentType,
                component.Location,
                component.ComponentId
            };

            return _db.Execute(query, parameters);
        }

        public bool DeleteComponent(string componentId) //удалить компонент
        {
            string query = $@"
                DELETE FROM components
                WHERE component_id = '{componentId}'";

            return _db.Execute(query);
        }

        public void TurnOnLight() // метод для управления освещением в доме а точнее включением света
        {
            const int pin = 17; // GPIO17 (BCM)
            _gpio.OpenPin(pin, PinMode.Output);
            _gpio.Write(pin, PinValue.High);
        }

        public void TurnOffLight() // выключение света
        {
            const int pin = 17;
            _gpio.Write(pin, PinValue.Low);
            _gpio.ClosePin(pin);
        }
    }
}