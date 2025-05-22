using System.Device.Gpio;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Services;

namespace TheBrainOfficeServer.Repositories;

public abstract class ComponentRepo(AppDbService db)
{
    public List<ComponentModel> ShowComponents()
    {
        const string query = $@"
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

        return db.GetList<ComponentModel>(query);
    }

    public string CreateComponent(ComponentModel component) //проверка
    {
        const string query = @"
                INSERT INTO components 
                    (component_Id, name, description, component_type, location)
                VALUES 
                    (@ComponentId, @Name, @Description, @ComponentType, @Location)
                RETURNING id";

        return db.GetScalar<int>(query, new
        {
            component.ComponentId,
            component.Name,
            component.Description,
            component.ComponentType,
            component.Location
        }).ToString();
    }

    public bool UpdateComponent(ComponentModel component)
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

        return db.Execute(query, parameters);
    }

    public bool DeleteComponent(string componentId)
    {
        var query = $@"
                DELETE FROM components
                WHERE component_id = '{componentId}'";

        return db.Execute(query);
    }

    public bool SwitchState(bool isActive)
    {
        try
        {
            int ledPin = 24; //GPIO24 is pin 18 on RPi
            int ledOnTime = 1000; //led on time in ms
            int ledOffTime = 500; //led off time in ms
 
            using GpioController controller = new();
            controller.OpenPin(ledPin, PinMode.Output);
 
            Console.CancelKeyPress += (s, e) =>
            {
                controller.Dispose();
            };
            if (isActive == true)
                controller.Write(ledPin, PinValue.High);
            else 
                controller.Write(ledPin, PinValue.Low);
        
            return isActive;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}