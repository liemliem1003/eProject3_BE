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
    public class UsersController : ControllerBase
    {
        private readonly InsuranceContext _context;

        public UsersController(InsuranceContext context)
        {
            _context = context;
        }

        //cac API thuc thi

        //login
        [HttpGet("login")]
        public async Task<User> Login(string Username, string Password)
        {
            var user = _context.Users.SingleOrDefault(em => em.Username.Equals(Username) && em.Password.Equals(Password));
            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        //Get all
        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        //get one
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var employee = await _context.Users.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            else
            {
                return employee;
            }
        }

        //create
        [HttpPost]
        public async Task<ActionResult<User>> PostEmployee(User employee)
        {
            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetEmployee", new { id = employee.UserId }, employee);
        }

        //update
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> PostEmpployee(User employee, int id)
        {
            if (id != employee.UserId)
            {
                return BadRequest();
            }
            else
            {
                _context.Entry(employee).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Users.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            else
            {
                _context.Users.Remove(employee);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}