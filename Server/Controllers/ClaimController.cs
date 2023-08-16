using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
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
        public async Task<ActionResult<Claim>> GetClaimByUser(int id, int limit, int page, string sortOrder = "asc")
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

        //get claim by userid
        [HttpGet("claimbyuser/{id}")]
        public async Task<ActionResult<Claim>> GetClaim(int id, int limit, int page)
        {
            // Calculate skip count based on page and limit
            int skip = (page - 1) * limit;

            // Query data using Skip() and Take() methods to implement paging
            var claimsQuery = _context.Claims.AsQueryable();

            var claims = await claimsQuery.Where(c => c.UserId.Equals(id))
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.Policies.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Policies = claims
            };

            return Ok(response);
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

            if (claim.Status ?? false)
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
