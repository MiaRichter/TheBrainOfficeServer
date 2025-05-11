using System.Device.Gpio;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Services;
using Iot.Device.DHTxx;

namespace TheBrainOfficeServer.Repositories
{
    public class ComponentRepo
    {
        private readonly AppDBService _db;
        public ComponentRepo(AppDBService db)
        {
            _db = db;
        }

        public List<ComponentModel> ShowComponents()
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

            return _db.Execute(query, parameters);
        }

        public bool DeleteComponent(string componentId)
        {
            string query = $@"
                DELETE FROM components
                WHERE component_id = '{componentId}'";

            return _db.Execute(query);
        }

        
    }
}