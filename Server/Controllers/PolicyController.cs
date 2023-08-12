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
    public class PolicyController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public PolicyController(InsuranceContext context)
        {
            _context = context;
        }

        //Policy API

        //Get all
        [HttpGet]
        public async Task<IEnumerable<Policy>> GetPolicies()
        {
            return await _context.Policies.ToListAsync();
        }

        //Get one
        [HttpGet("{id}")]
        public async Task<ActionResult<Policy>> GetPolicy(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            else
            {
                return policy;
            }
        }

        //Search by name
        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Policy>>> SearchPolicy(string name)
        {
            IQueryable<Policy> query = _context.Policies;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.PolicyName.Contains(name));
            }

            var result = await query.ToListAsync();

            if (result.Any())
            {
                return Ok(result);
            }
            else return NotFound();
        }

        //create
        //[HttpPost("create")]
        //public async Task<ActionResult<Policy>> CreatePolicy(Policy policy)
        //{
        //    _context.Policies.Add(policy);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction("GetPolicy", new { id = policy.PolicyId }, policy);
        //}
        [HttpPost("create")]
        public async Task<ActionResult<Policy>> CreatePolicy([FromBody] Policy policy)
        {
            try
            {
                var pname = await _context.Policies.SingleOrDefaultAsync(c => c.PolicyName.Equals(policy.PolicyName));
                if (pname == null)
                {
                    // Decode base64 image data
                    var logoBytes = Convert.FromBase64String(policy.Banner);

                    // Save the image file to the server
                    //var fileExtension = Path.GetExtension(company.Logo);
                    var fileName = Guid.NewGuid().ToString() + /*fileExtension*/ ".png";
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Create a new company entity
                    var newPolicy = new Policy
                    {
                        PolicyId = policy.PolicyId,
                        PolicyName = policy.PolicyName,
                        Desciption = policy.Desciption,
                        TotalAmount = policy.TotalAmount,
                        Duration = policy.Duration,
                        CompanyId = policy.CompanyId,
                        Banner = "images/" + fileName, // Store the link to the image file
                    };

                    _context.Policies.Add(newPolicy);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetPolicy", new { id = newPolicy.PolicyId }, newPolicy);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Policy already exists.");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
        }


        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Policy>> UpdatePolicy(Policy policy, int id)
        {
            if (id != policy.PolicyId)
            {
                return BadRequest();
            }
            else
            {
                _context.Entry(policy).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            else
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}
