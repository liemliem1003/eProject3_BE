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
        public async Task<ActionResult<Claim>> CreateClaim(Claim claim)
        {
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetClaim", new { id = claim.ClaimId }, claim);
        }

        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Claim>> UpdateClaim(Claim claim, int id)
        {
            if (id != claim.ClaimId)
            {
                return BadRequest();
            }
            else
            {
                _context.Entry(claim).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        //approve
        [HttpPost("approve/{claimId}")]
        public IActionResult ApproveClaim(int claimId)
        {
            // Get the claim from the database
            var claim = _context.Claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            if (claim.Status)
            {
                return BadRequest("Claim is already approved");
            }

            // Update the claim status to approved
            claim.Status = true;

            // Update the available amount on the associated policy on user
            var policy = _context.PolicyOnUsers.FirstOrDefault(p => p.PolicyId == claim.PolicyId);

            if (policy == null)
            {
                return NotFound("Policy not found");
            }

            if (policy.AvaibleAmount < claim.AppAmount)
            {
                return BadRequest("Insufficient available amount on the policy");
            }

            policy.AvaibleAmount -= claim.AppAmount;

            _context.SaveChanges();

            return Ok("Claim approved and policy updated");
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _context.Policies.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }
            else
            {
                _context.Policies.Remove(claim);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}
