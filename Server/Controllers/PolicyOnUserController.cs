using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyOnUserController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public PolicyOnUserController(InsuranceContext context)
        {
            _context = context;
        }

        //PolicyOnUser API

        //Get all
        [HttpGet]
        public async Task<IEnumerable<PolicyOnUser>> GetPolicyOnUsers()
        {
            return await _context.PolicyOnUsers.ToListAsync();
        }

        //add


        //delete


    }
}
