using System;
using System.Collections.Generic;
using System.Linq;
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
        [HttpPost("create")]
        public async Task<ActionResult<User>> PostEmployee([FromBody] User employee)
        {
            try
            {
                var ename = await _context.Users.SingleOrDefaultAsync(c => c.Name.Equals(employee.Name));
                if (ename == null)
                {
                    // Decode base64 image data
                    var logoBytes = Convert.FromBase64String(employee.Avatar);

                    // Save the image file to the server
                    //var fileExtension = Path.GetExtension(company.Logo);
                    var fileName = Guid.NewGuid().ToString() + /*fileExtension*/ ".png";
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Create a new company entity
                    var newEmployee = new User
                    {
                        Username = employee.Username,
                        Password = employee.Password,
                        Name = employee.Name,
                        Dob = employee.Dob,
                        Email = employee.Email,
                        Phone = employee.Phone,
                        Address = employee.Address,
                        Avatar = "images/" + fileName, // Store the link to the image file
                        Role = employee.Role
                    };

                    _context.Users.Add(newEmployee);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetUser", new { id = newEmployee.UserId }, newEmployee);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Employee already exists.");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
            //_context.Users.Add(employee);
            //await _context.SaveChangesAsync();
            //return CreatedAtAction("GetEmployee", new { id = employee.UserId }, employee);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        //update
        [HttpPut("update/{id}")]
        public async Task<ActionResult<User>> PostEmpployee(int id, [FromBody] User employee)
        {
            try
            {
                if (id != employee.UserId)
                {
                    return BadRequest();
                }

                var existingEmployee = await _context.Users.FindAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound();
                }

                if (existingEmployee.Avatar != employee.Avatar)
                {
                    // If the logo has changed, decode base64 image data and save to server
                    var logoBytes = Convert.FromBase64String(employee.Avatar);
                    var fileName = Guid.NewGuid().ToString() + ".png"; // Generate a new unique filename
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Update company details including the new logo link
                    existingEmployee.Avatar = "images/" + fileName;
                }
                else
                {
                    // If the logo hasn't changed, keep the existing logo link
                    employee.Avatar = existingEmployee.Avatar;
                }

                // Update properties from the viewModel
                existingEmployee.Name = employee.Name;
                existingEmployee.Dob = employee.Dob;
                existingEmployee.Email = employee.Email;
                existingEmployee.Phone = employee.Phone;
                existingEmployee.Address = employee.Address;

                _context.Update(employee);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
            //if (id != employee.UserId)
            //{
            //    return BadRequest();
            //}
            //else
            //{
            //    _context.Entry(employee).State = EntityState.Modified;
            //    await _context.SaveChangesAsync();
            //    return NoContent();
            //}
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