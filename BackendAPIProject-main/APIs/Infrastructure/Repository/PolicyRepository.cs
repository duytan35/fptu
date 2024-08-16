﻿using Application.InterfaceRepository;
using Application.InterfaceService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class PolicyRepository : GenericRepository<Policy>,IPolicyRepository
    {
        public PolicyRepository(AppDbContext appDbContext, IClaimService claimService, ICurrentTime currentTime) : base(appDbContext, claimService, currentTime)
        {
        }
    }
}
