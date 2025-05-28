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
        private readonly AppDbService _db;
        private readonly ILogger<ComponentRepo> _logger;

        public ComponentRepo(AppDbService db, ILogger<ComponentRepo> logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<int> CreateComponent()
        {
            try
            {
                string query = $@" CREATE TABLE users (
                                    id SERIAL PRIMARY KEY,
                                    username VARCHAR(50) UNIQUE NOT NULL,
                                    password_hash TEXT NOT NULL);";

                return await _db.GetScalarAsync<int>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating component");
                throw;
            }
        }
        
    }
}