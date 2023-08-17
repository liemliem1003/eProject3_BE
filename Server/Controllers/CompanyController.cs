using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public CompanyController(InsuranceContext context) {
            _context = context;
        }

        //Company API

        //get all
        [HttpGet]
        //[Authorize(Roles = "admin")]
        //[Authorize]
        public async Task<IActionResult> GetCompanies(int limit, int page, string sortOrder = "asc")
        {
            var token = HttpContext.Request.Headers["Authorization"];
            Console.WriteLine("Received Token: " + token);
            // Calculate skip count based on page and limit
            int skip = (page - 1) * limit;

            // Set the default sort direction if not provided
            if (sortOrder != "asc" && sortOrder != "desc")
            {
                sortOrder = "asc";
            }

            // Query data using Skip() and Take() methods to implement paging
            var companiesQuery = _context.Companies.AsQueryable();

            if (sortOrder == "asc")
            {
                companiesQuery = companiesQuery.OrderBy(c => c.CompanyName);
            }
            else
            {
                companiesQuery = companiesQuery.OrderByDescending(c => c.CompanyName);
            }

            var companies = await companiesQuery
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.Companies.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Companies = companies,
                SortOrder = sortOrder
            };

            return Ok(response);
        }

        //Get one
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                return company;
            }
        }

        //get active
        [HttpGet("getactive")]
        public async Task<IActionResult> GetCompaniesByStatus(string sortOrder = "asc")
        {
            // Set the default sort direction if not provided
            if (sortOrder != "asc" && sortOrder != "desc")
            {
                sortOrder = "asc";
            }

            var companiesQuery = _context.Companies.AsQueryable();

            if (sortOrder == "asc")
            {
                companiesQuery = companiesQuery.OrderBy(c => c.CompanyName);
            }
            else
            {
                companiesQuery = companiesQuery.OrderByDescending(c => c.CompanyName);
            }

            var companies = await companiesQuery.Where(c => c.Status == true).ToListAsync();

            return Ok(companies);
        }

        //Search by name
        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Company>>> SearchCompany(string name)
        {
            IQueryable<Company> query = _context.Companies;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.CompanyName.Contains(name));
            }

            var result = await query.ToListAsync();

            if (result.Any())
            { 
                return Ok(result);
            }else return NotFound();
        }

        //save on database as code64
        ////create
        //[HttpPost("create")]
        //public async Task<ActionResult<Company>> CreateCompany([FromBody] Company company)
        //{
        //    try
        //    {
        //        var cname = _context.Companies.SingleOrDefault(c => c.CompanyName.Equals(company.CompanyName));
        //        if (cname == null)
        //        {
        //            // Convert base64 to byte array
        //            byte[] logoBytes = Convert.FromBase64String(company.Logo);

        //            // Generate a unique file name
        //            string fileName = Guid.NewGuid().ToString() + ".png";
        //            var filePath = Path.Combine("wwwroot/images", fileName);

        //            // Save the image to the server
        //            await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await fileStream.WriteAsync(logoBytes);
        //            }

        //            // Create a new company entity
        //            var newCompany = new Company
        //            {
        //                CompanyName = company.CompanyName,
        //                CompanyPhone = company.CompanyPhone,
        //                Address = company.Address,
        //                Logo = "images/" + fileName,
        //                Url = company.Url
        //            };

        //            _context.Companies.Add(company);
        //            await _context.SaveChangesAsync();
        //            return CreatedAtAction("GetCompany", new { id = newCompany.CompanyId }, newCompany);
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Company already exists.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ModelState.AddModelError(string.Empty, e.Message);
        //    }
        //    return BadRequest(ModelState);
        //}

        //save on database as link to file
        [HttpPost("create")]
        public async Task<ActionResult<Company>> CreateCompany([FromBody] Company company)
        {
            try
            {
                var cname = await _context.Companies.SingleOrDefaultAsync(c => c.CompanyName.Equals(company.CompanyName));
                if (cname == null)
                {
                    // Decode base64 image data
                    var logoBytes = Convert.FromBase64String(company.Logo);

                    // Save the image file to the server
                    //var fileExtension = Path.GetExtension(company.Logo);
                    var fileName = Guid.NewGuid().ToString() + /*fileExtension*/ ".png";
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Create a new company entity
                    var newCompany = new Company
                    {
                        CompanyName = company.CompanyName,
                        Logo = "images/" + fileName, // Store the link to the image file
                        CompanyPhone = company.CompanyPhone,
                        Url = company.Url,
                        Address = company.Address,
                        Status = company.Status,
                    };

                    _context.Companies.Add(newCompany);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetCompany", new { id = newCompany.CompanyId }, newCompany);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Company already exists.");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.CompanyId == id);
        }


        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Company>> UpdateCompany(int id, [FromBody] Company company)
        {
            try
            {
                if (id != company.CompanyId)
                {
                    return BadRequest();
                }

                var existingCompany = await _context.Companies.FindAsync(id);

                if (existingCompany == null)
                {
                    return NotFound();
                }

                // Check if the updated CompanyName already exists (excluding the current company being updated)
                var companyNameExists = await _context.Companies
                    .AnyAsync(c => c.CompanyName == company.CompanyName && c.CompanyId != id);

                if (companyNameExists)
                {
                    ModelState.AddModelError("CompanyName", "Company name already exists.");
                    return BadRequest(ModelState);
                }

                if (existingCompany.Logo != company.Logo)
                {
                    // If the logo has changed, decode base64 image data and save to server
                    var logoBytes = Convert.FromBase64String(company.Logo);
                    var fileName = Guid.NewGuid().ToString() + ".png"; // Generate a new unique filename
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Update company details including the new logo link
                    existingCompany.Logo = "images/" + fileName;
                }
                else
                {
                    // If the logo hasn't changed, keep the existing logo link
                    company.Logo = existingCompany.Logo;
                }

                // Update properties from the viewModel
                existingCompany.CompanyName = company.CompanyName;
                existingCompany.CompanyPhone = company.CompanyPhone;
                existingCompany.Address = company.Address;
                existingCompany.Url = company.Url;
                existingCompany.Status = company.Status;

                //if (!string.IsNullOrEmpty(ucompany.Logo))
                //{
                //    // Convert base64 to byte array
                //    byte[] logoBytes = Convert.FromBase64String(ucompany.Logo);

                //    // Generate a unique file name
                //    string fileName = Guid.NewGuid().ToString() + ".png";
                //    var filePath = Path.Combine("wwwroot/images", fileName);

                //    // Save the new image to the server
                //    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await fileStream.WriteAsync(logoBytes);
                //    }

                //    // Update the logo path
                //    company.Logo = "images/" + fileName;
                //}

                _context.Update(existingCompany);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { message = "Company updated successfully." });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
            //if (id != company.CompanyId)
            //{
            //    return BadRequest();
            //}
            //else
            //{
            //    _context.Entry(company).State = EntityState.Modified;
            //    await _context.SaveChangesAsync();
            //    return NoContent();
            //}
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}
