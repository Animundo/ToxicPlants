﻿using Function.Interfaces;
using Function.MiddleWare.ExceptionHandler;
using Function.Models;
using Function.Models.Request;
using Function.Utilities;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Function.UseCases
{
    internal class HandleRequestDataPlantCheck : IHandleRequestData
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IPlantService _plantService;
        private readonly IToxicPlantAnimalRepository _toxicPlantAnimalRepository;

        public HandleRequestDataPlantCheck(IPlantRepository plantRepository, IAnimalRepository animalRepository, IPlantService plantService, IToxicPlantAnimalRepository toxicPlantAnimalRepository)
        {
            _plantRepository = plantRepository;
            _animalRepository = animalRepository;
            _plantService = plantService;
            _toxicPlantAnimalRepository = toxicPlantAnimalRepository;
        }

        public async Task<string> HandleRequest(HttpRequestData request)
        {
            var parsedData = await RequestParser.Parse(request.Body);
            var addPlants = AddPlants(parsedData);
            AddAnimals(parsedData);
            await Task.WhenAll(addPlants);
            var result = MatchToxicPlantsForAnimals();
            var json = JsonSerializer.Serialize(result);
            return json;
        }

        private void AddAnimals(RequestData data)
        {
            var animals = data.Parameters.Where(x => x.Name == "animal").ToList();

            if (animals.Count == 0)
            {
                throw new RequestException("No animal received");
            }

            foreach (var animal in animals)
            {
                if (Enum.TryParse(animal.Data, true, out Animal animalEnum))
                {
                    _animalRepository.Add(animalEnum);
                }
                else
                {
                    throw new RequestException("Animal not supported");
                }
            }
        }

        private async Task AddPlants(RequestData data)
        {
            var responseContent = await _plantService.GetPlantsAsync(data);

            var json = JsonSerializer.Deserialize<JsonElement>(responseContent);
            json.TryGetProperty("results", out var results);

            foreach (var result in results.EnumerateArray())
            {
                result.TryGetProperty("species", out var species);
                species.TryGetProperty("scientificName", out var scientificName);

                result.TryGetProperty("score", out var score);

                var plant = new Plant
                {
                    ScientificName = scientificName.GetString(),
                    Score = score.GetDouble()
                };
                _plantRepository.Add(plant);
            }
        }

        private List<ToxicPlantAnimal> MatchToxicPlantsForAnimals()
        {
            var toxicPlantsAnimal = new List<ToxicPlantAnimal>();
            foreach (var animal in _animalRepository.Get())
            {
                foreach (var plant in _plantRepository.Get())
                {
                    var ToxicPlant = _toxicPlantAnimalRepository.GetbyAnimalAndPlantName(animal, plant);
                    if (ToxicPlant != null)
                    {
                        toxicPlantsAnimal.Add(ToxicPlant);
                    }
                }
            }

            return toxicPlantsAnimal;
        }


    }
}
