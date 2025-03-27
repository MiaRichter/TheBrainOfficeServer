using System.Collections.Generic;
using TheBrainOfficeServer.Models;
using Microsoft.Extensions.Logging;
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving components list");
                throw;
            }
        }

        public int CreateComponent(ComponentModel component)
        {
            try
            {
                string query = $@"
                    INSERT INTO components 
                        (component_Id, name, description, component_type, location)
                    VALUES 
                        ('{component.ComponentId}', '{component.Name}', '{component.Description}', '{component.ComponentType}', '{component.Location}')
                    RETURNING id";

                return _db.GetScalar<int>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating component {component.ComponentId}");
                throw;
            }
        }

        public bool UpdateComponent(ComponentModel component)
        {
            try
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

                return _db.Execute(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error updating component {component.ComponentId}");
                throw;
            }
        }

        public bool DeleteComponent(string componentId)
        {
            try
            {
                string query = $@"
                    DELETE FROM components
                    WHERE 
                        component_id = '{componentId}'";

                return _db.Execute(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error hard-deleting component {componentId}");
                throw;
            }
        }
    }
}