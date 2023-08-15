using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IConfiguration _configuration;
        

        public UsersController(InsuranceContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
        }

        //cac API thuc thi

        //login
        [HttpGet("login")]
        public /*async Task<User>*/ IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(em => em.Username.Equals(username) && em.Password.Equals(password));
            if (user != null)
            {
                var claims = new[]
                {
                    new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new System.Security.Claims.Claim(ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim(ClaimTypes.Role, user.Role == 1 ? "admin" : "employee")
                };
                //var claims = new[]
                //{
                //    new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.UserId.ToString()),
                //    new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.Username),
                //    new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", user.Role == 1 ? "admin" : "employee")
                //};

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtIssuer"],
                    audience: _configuration["JwtAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(1), // Token expiration time
                    signingCredentials: creds
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Return the token as part of the response
                return Ok(new { Token = tokenString,
                    User = new { 
                        user.UserId,
                        user.Username,
                        user.Role
                    }
                });
                
                //return user;
            }
            else
            {
                return Unauthorized(); // Invalid login credentials
                //return null;
            }
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        //Get all
        [HttpGet]
        //[Authorize(Roles = "admin")]
        //[Authorize]
        public async Task<IActionResult> GetUsers(int limit, int page, string sortOrder = "asc")
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
            var employeesQuery = _context.Users.AsQueryable();

            if (sortOrder == "asc")
            {
                employeesQuery = employeesQuery.OrderBy(c => c.Name);
            }
            else
            {
                employeesQuery = employeesQuery.OrderByDescending(c => c.Name);
            }

            var employees = await employeesQuery
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.Users.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Employees = employees,
                SortOrder = sortOrder
            };

            return Ok(response);
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
                        Role = employee.Role,
                        Status = employee.Status,
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
                existingEmployee.Password = employee.Password;
                existingEmployee.Name = employee.Name;
                existingEmployee.Dob = employee.Dob;
                existingEmployee.Email = employee.Email;
                existingEmployee.Phone = employee.Phone;
                existingEmployee.Address = employee.Address;
                existingEmployee.Status = employee.Status;

                _context.Update(existingEmployee);
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

                return Ok(new { message = "Employee updated successfully." });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return BadRequest(ModelState);
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