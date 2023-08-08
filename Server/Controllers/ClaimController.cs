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
    public class ClaimController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public ClaimController(InsuranceContext context)
        {
            _context = context;
        }

        //Claim API

        //Get all
        [HttpGet]
        public async Task<IEnumerable<Claim>> GetClaims()
        {
            return await _context.Claims.ToListAsync();
        }

        //Get one
        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }
            else
            {
                return claim;
            }
        }

        //Create
        [HttpPost("create")]
        public async Task<ActionResult<Claim>> CreatePolicy(Claim claim)
        {
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetClaim", new { id = claim.ClaimId }, claim);
        }

        //Update

    }
}
