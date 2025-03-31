using TheBrainOfficeServer.Exceptions;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Services;

namespace TheBrainOfficeServer.Repositories
{
    public class ComponentRepo
    {
        private readonly AppDBService _db;
        private readonly ILogger<ComponentRepo> _logger;

        public ComponentRepo(AppDBService db, ILogger<ComponentRepo> logger)
        {
            _db = db;
            _logger = logger;
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

            return _db.GetList<ComponentModel>(query)
                ?? throw new RepositoryException(
                    "получение списка компонентов",
                    "Query returned null");
        }

        public string CreateComponent(ComponentModel component)
        {
            const string query = @"
        INSERT INTO components 
            (component_Id, name, description, component_type, location)
        VALUES 
            (@ComponentId, @Name, @Description, @ComponentType, @Location)
        RETURNING id";

            var parameters = new
            {
                component.ComponentId,
                component.Name,
                component.Description,
                component.ComponentType,
                component.Location
            };

            return _db.GetScalar<int>(query, parameters).ToString()
                ?? throw new RepositoryException(
                    "создание компонента",
                    $"Failed to create component {component.ComponentId}");
        }

        public bool UpdateComponent(ComponentModel component)
        {
            string query = $@"
                UPDATE components
                SET 
                    name = '{component.Name}',
                    description = '{component.Description}',
                    component_type = '{component.ComponentType}',
                    location = '{component.Location}',
                    updated_at = NOW()
                WHERE 
                    component_id = '{component.ComponentId}'
                AND 
                    is_active = true";

            if (!_db.Execute(query))
            {
                throw new RepositoryException(
                    "обновление компонента",
                    $"Failed to update component {component.ComponentId}");
            }
            return true;
        }

        public bool DeleteComponent(string componentId)
        {
            string query = $@"
                DELETE FROM components
                WHERE 
                    component_id = '{componentId}'";

            if (!_db.Execute(query))
            {
                throw new RepositoryException(
                    "удаление компонента",
                    $"Failed to delete component {componentId}");
            }
            return true;
        }
    }
}