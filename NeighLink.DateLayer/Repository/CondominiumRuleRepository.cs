﻿using Microsoft.EntityFrameworkCore;
using NeighLink.DateLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeighLink.DateLayer.Repository
{
    public class CondominiumRuleRepository : CrudRepository<Condominiumrule>
    {
        public CondominiumRuleRepository(neighlinkdbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Condominiumrule>> GetByCondominium(int condominiumId)
        {
            return await _dbContext.Condominiumrule.Where( x => x.CondominiumId == condominiumId ).ToListAsync();
        }
    }
}
