﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Function.Interfaces;
using Function.MiddleWare.ExceptionHandler;
using Function.Models.Response;
using Microsoft.Extensions.Logging;

namespace Function.UseCases
{
    internal class Matcher : IMatcher
    {
        private readonly ILogger<HandleRequest> _logger;
        private readonly IPlantRepository _plantRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IToxicPlantAnimalRepository _toxicPlantAnimalRepository;
        private readonly IToxicPlantAnimalService _toxicPlantAnimalService;

        public Matcher(ILogger<HandleRequest> logger, IPlantRepository plantRepository, IAnimalRepository animalRepository, IPlantService plantService, IToxicPlantAnimalRepository toxicPlantAnimalRepository, IToxicPlantAnimalService toxicPlantAnimalService)
        {
            _logger = logger;
            _plantRepository = plantRepository;
            _animalRepository = animalRepository;
            _toxicPlantAnimalRepository = toxicPlantAnimalRepository;
            _toxicPlantAnimalService = toxicPlantAnimalService;
        }

        public List<ToxicPlantResponse> MatchToxicPlantsForAnimals()
        {
            _toxicPlantAnimalService.LoadToxicPlantAnimalData();

            var plantResponseList = new List<ToxicPlantResponse>();
            foreach (var animal in _animalRepository.Get())
            {
                foreach (var plant in _plantRepository.Get())
                {
                    var plantResponse = new ToxicPlantResponse
                    {
                        Animal = animal,
                        PlantName = plant.ScientificName,
                        PlantDetail = plant.PlantDetail
                    };

                    var toxicPlantList = _toxicPlantAnimalRepository.GetbyAnimalAndPlantName(animal, plant);
                    switch (toxicPlantList.Count)
                    {
                        case 1:
                        {
                            var toxicPlant = toxicPlantList.First();
                            plantResponse.HowToxic = toxicPlant.HowToxic;
                            plantResponse.Reference = toxicPlant.Reference;
                            plantResponse.ExtraInformation = toxicPlant.ExtraInformation;
                            plantResponseList.Add(plantResponse);
                            break;
                        }
                        case 0:
                            plantResponse.HowToxic = 0;
                            plantResponse.ExtraInformation = "There is no information that this plant is toxic for this animal";
                            plantResponseList.Add(plantResponse);
                            break;
                        default:
                            ProgramError.CreateProgramError(HttpStatusCode.Conflict, "Multiple hits on same toxic plant.");
                            break;
                    }
                }
            }

            var nrOfToxicPlants = plantResponseList.Where(x => x.HowToxic > 0);
            _logger.LogInformation($"Found {nrOfToxicPlants.Count()} posible toxic plants hits for sent animals");
            return plantResponseList;
        }
    }
}
