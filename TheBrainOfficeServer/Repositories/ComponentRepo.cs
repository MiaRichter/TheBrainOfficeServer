﻿using System.Collections.Generic;
 using System.Device.Gpio;
 using System.Device.I2c;
 using System.IO.Ports;
 using System.Text.Json;
 using Iot.Device.Common;
 using Iot.Device.DHTxx;
 using TheBrainOfficeServer.Models;
using Microsoft.Extensions.Logging;
using TheBrainOfficeServer.Services;
using UnitsNet;

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
        
        public SensorData DhtState(string portNumber)
        {
            Console.WriteLine("Сервер запущен. Ожидание данных...");
            SensorData data;
            try
            {
                
                using var serialPort = new SerialPort($"/dev/ttyUSB{portNumber}", 115200)
                {
                    ReadTimeout = 1500,
                    WriteTimeout = 1500,
                    Encoding = System.Text.Encoding.UTF8
                };

                serialPort.Open();
                serialPort.DiscardInBuffer(); // Очистка буфера

                while (DateTime.Now.Day <= 30)
                {
                    try
                    {
                        string jsonData = serialPort.ReadLine().Trim();

                        // Пропускаем пустые или некорректные данные
                        if (string.IsNullOrEmpty(jsonData) || !jsonData.StartsWith("{"))
                            continue;

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        data = JsonSerializer.Deserialize<SensorData>(jsonData, options);

                        Console.WriteLine($"Влажность: {data?.Humidity ?? 0:F1}%");
                        Console.WriteLine($"Температура: {data?.Temperature ?? 0:F1}°C");
                        Console.WriteLine($"RFID: {data?.Rfid ?? "none"}");
                        Console.WriteLine($"Сервопривод: {(data?.Servo > 0 ? "ВКЛ" : "ВЫКЛ")}");
                        Console.WriteLine("---------------------");
                        return data;
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("Таймаут чтения. Проверьте подключение.");
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Ошибка формата данных. Получена некорректная строка JSON.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Ошибка доступа к порту. Попробуйте:");
                Console.WriteLine("1. sudo usermod -a -G dialout $USER");
                Console.WriteLine("2. sudo chmod 666 /dev/ttyUSB*");
                Console.WriteLine("3. Перезагрузите Raspberry Pi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
            }

            return null;
        }
    }
}