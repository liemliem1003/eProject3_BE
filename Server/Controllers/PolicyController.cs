using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        public async Task<IActionResult> GetPolicies(int limit, int page, string sortOrder = "asc")
        {
            // Calculate skip count based on page and limit
            int skip = (page - 1) * limit;

            // Set the default sort direction if not provided
            if (sortOrder != "asc" && sortOrder != "desc")
            {
                sortOrder = "asc";
            }

            // Query data using Skip() and Take() methods to implement paging
            var policiesQuery = _context.Policies.AsQueryable();

            if (sortOrder == "asc")
            {
                policiesQuery = policiesQuery.OrderBy(c => c.PolicyName);
            }
            else
            {
                policiesQuery = policiesQuery.OrderByDescending(c => c.PolicyName);
            }

            var policies = await policiesQuery
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.Policies.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Policies = policies,
                SortOrder = sortOrder
            };

            return Ok(response);
        }

        //Sort by company api/policies/company/{companyId}
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<object>> GetPoliciesByCompany(int companyId, int page = 1, int limit = 10, string sortOrder = "asc")
        {
            var company = await _context.Companies.FindAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            var policiesQuery = _context.Policies
                .Where(p => p.CompanyId == companyId);

            // Get the total count of items in the database
            int totalPolicies = await policiesQuery.CountAsync();

            var sortedPoliciesQuery = sortOrder == "asc"
            ? policiesQuery.OrderBy(c => c.PolicyName)
            : policiesQuery.OrderByDescending(c => c.PolicyName);

            var policies = await policiesQuery
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var response = new
            {
                TotalCount = totalPolicies,
                Policies = policies,
                SortOrder = sortOrder
            };
            Response.Headers.Add("X-Total-Count", totalPolicies.ToString());

            var serializedResponse = System.Text.Json.JsonSerializer.Serialize(response, jsonOptions);

            return new ContentResult
            {
                Content = serializedResponse,
                ContentType = "application/json",
                StatusCode = 200
            };
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

        private bool PolicyExists(int id)
        {
            return _context.Policies.Any(e => e.PolicyId == id);
        }

        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Policy>> UpdatePolicy(int id, [FromBody] Policy policy)
        {
            try
            {
                if (id != policy.PolicyId)
                {
                    return BadRequest();
                }

                var existingPolicy = await _context.Policies.FindAsync(id);

                if (existingPolicy == null)
                {
                    return NotFound();
                }

                // Check if the updated CompanyName already exists (excluding the current company being updated)
                var companyNameExists = await _context.Policies
                    .AnyAsync(p => p.PolicyName == policy.PolicyName && p.PolicyId != id);

                if (companyNameExists)
                {
                    ModelState.AddModelError("CompanyName", "Policy name already exists.");
                    return BadRequest(ModelState);
                }

                if (existingPolicy.Banner != policy.Banner)
                {
                    // If the logo has changed, decode base64 image data and save to server
                    var logoBytes = Convert.FromBase64String(policy.Banner);
                    var fileName = Guid.NewGuid().ToString() + ".png"; // Generate a new unique filename
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Update company details including the new logo link
                    existingPolicy.Banner = "images/" + fileName;
                }
                else
                {
                    // If the logo hasn't changed, keep the existing logo link
                    policy.Banner = existingPolicy.Banner;
                }

                // Update properties from the viewModel
                existingPolicy.PolicyName = policy.PolicyName;
                existingPolicy.Desciption = policy.Desciption;
                existingPolicy.TotalAmount = policy.TotalAmount;
                existingPolicy.Duration = policy.Duration;
                existingPolicy.CompanyId = policy.CompanyId;
                existingPolicy.Status = policy.Status;

                _context.Update(existingPolicy);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PolicyExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { message = "Policy updated successfully." });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
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
