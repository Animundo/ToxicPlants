﻿using Function.Models;
using Function.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Function.Repository
{
    public interface IPlantRepository
    {
        public List<Plant> GetAll();
        public Task AddAllAsync(RequestData data);
    }
}
