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

        //Get one
        [HttpGet("{id}")]
        public async Task<ActionResult<PolicyOnUser>> GetPolicyOnUser(int id)
        {
            var policyonuser = await _context.PolicyOnUsers.FindAsync(id);
            if (policyonuser == null)
            {
                return NotFound();
            }
            else
            {
                return policyonuser;
            }
        }

        //get by userid
        [HttpGet("getbyuserid/{id}")]
        public async Task<ActionResult<Claim>> GetPolicyOnUsers(int id, int limit, int page)
        {
            // Calculate skip count based on page and limit
            int skip = (page - 1) * limit;

            // Query data using Skip() and Take() methods to implement paging
            var policyonusersQuery = _context.PolicyOnUsers.AsQueryable();

            var policies = await policyonusersQuery.Where(c => c.UserId.Equals(id))
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.PolicyOnUsers.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                PoliciesOnUser = policies
            };

            return Ok(response);
        }

        //add
        [HttpPost("create")]
        public async Task<ActionResult<PolicyOnUser>> CreatePolicyOnUser(PolicyOnUser policyonuser)
        {
            _context.PolicyOnUsers.Add(policyonuser);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPolicyOnUsers", new { id = policyonuser.Id }, policyonuser);
        }

        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<PolicyOnUser>> UpdatePolicyOnUser(PolicyOnUser policyonuser, int id)
        {
            if (id != policyonuser.PolicyId)
            {
                return BadRequest();
            }
            else
            {
                _context.Entry(policyonuser).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicyOnUser(int id)
        {
            var policyonuser = await _context.PolicyOnUsers.FindAsync(id);
            if (policyonuser == null)
            {
                return NotFound();
            }
            else
            {
                _context.PolicyOnUsers.Remove(policyonuser);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

    }
}
