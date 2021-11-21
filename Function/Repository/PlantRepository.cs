﻿using System;
using Function.Interfaces;
using Function.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Function.MiddleWare.ExceptionHandler;
using Microsoft.Extensions.Logging;

namespace Function.Repository
{
    internal class PlantRepository : IPlantRepository
    {
        private readonly List<Plant> _plants;
        private readonly ILogger<PlantRepository> _logger;

        public PlantRepository(ILogger<PlantRepository> logger)
        {
            _plants = new();
            _logger = logger;
        }

        public List<Plant> Get() => _plants;

        public void Add(Plant plant)
        {
            var plantsInRepo = Get();
            if (!plantsInRepo.Exists(x => x.ScientificName == plant.ScientificName))
            {
                _plants.Add(plant);
            }
            else
            {
                _logger.LogCritical("Plant with same Scenttific name was not added to the Plant repository. Plantname = {_}", plant.ScientificName);
            }

        }
    }
}
