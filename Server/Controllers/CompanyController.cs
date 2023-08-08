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
    public class CompanyController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public CompanyController(InsuranceContext context) {
            _context = context;
        }

        //Company API

        //Get all
        [HttpGet]
        public async Task<IEnumerable<Company>> GetCompanies()
        { 
            return await _context.Companies.ToListAsync();
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

        //create
        [HttpPost]
        public async Task<ActionResult<Company>> CreateCompany(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCompany", new { id = company.CompanyId }, company);
        }

        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Company>> UpdateCompany(Company company, int id)
        {
            if (id != company.CompanyId)
            {
                return BadRequest();
            }
            else
            {
                _context.Entry(company).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
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
