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
                string query = @"
                    INSERT INTO components 
                        (component_id, name, description, component_type, location)
                    VALUES 
                        (@ComponentId, @Name, @Description, @ComponentType, @Location)
                    RETURNING id";

                return _db.GetScalar<int>(query, component);
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
                string query = @"
                    UPDATE components
                    SET 
                        name = @Name,
                        description = @Description,
                        component_type = @ComponentType,
                        location = @Location,
                        updated_at = NOW()
                    WHERE 
                        component_id = @ComponentId
                    AND 
                        is_active = true";

                return _db.Execute(query, component);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating component {component.ComponentId}");
                throw;
            }
        }

        public bool SoftDeleteComponent(string componentId)
        {
            try
            {
                string query = @"
                    UPDATE components
                    SET 
                        is_active = false,
                        updated_at = NOW()
                    WHERE 
                        component_id = @componentId";

                return _db.Execute(query, new { componentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft-deleting component {componentId}");
                throw;
            }
        }

        public bool HardDeleteComponent(string componentId)
        {
            try
            {
                string query = @"
                    DELETE FROM components
                    WHERE 
                        component_id = @componentId";

                return _db.Execute(query, new { componentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error hard-deleting component {componentId}");
                throw;
            }
        }

        public ComponentModel GetComponentById(string componentId)
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
                    WHERE 
                        component_id = @componentId
                    AND 
                        is_active = true";

                return _db.GetScalar<ComponentModel>(query, new { componentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving component {componentId}");
                throw;
            }
        }
    }
}