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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Claim = Server.Models.Claim;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly InsuranceContext _context; private readonly IWebHostEnvironment environment;

        public ClaimController(InsuranceContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            environment = webHostEnvironment;
        }

        //Claim API

        //Get all
        [HttpGet]
        public async Task<IActionResult> GetClaims(int limit, int page)
        {
            // Calculate skip count based on page and limit
            int skip = (page - 1) * limit;            

            // Query data using Skip() and Take() methods to implement paging
            var claimsQuery = _context.Claims.AsQueryable();

            var claims = await claimsQuery
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await _context.Claims.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Claims = claims
            };

            return Ok(response);
        }

        //Get one
        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetClaimByUser(int id)
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
            int totalCount = await _context.Claims.CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Claims = claims
            };

            return Ok(response);
        }

        //Get one
        [HttpGet("claimimage")]
        public async Task<ActionResult<ClaimImage>> GetClaimImage(int id)
        {
            var claimImage = await _context.ClaimImages.FindAsync(id);
            if (claimImage == null)
            {
                return NotFound();
            }
            else
            {
                return claimImage;
            }
        }

        //search by user name
        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Claim>>> SearchClaim(string name, int limit, int page, string sortOrder = "asc")
        {
            int skip = (page - 1) * limit;

            // Set the default sort direction if not provided
            if (sortOrder != "asc" && sortOrder != "desc")
            {
                sortOrder = "asc";
            }

            var claimsQuery = _context.Claims
                .Include(c => c.User)
                .Where(c => c.User.Name.Contains(name));

            if (sortOrder == "asc")
            {
                claimsQuery = claimsQuery.OrderBy(c => c.CreateDate);
            }
            else
            {
                claimsQuery = claimsQuery.OrderByDescending(c => c.CreateDate);
            }


            var claims = await claimsQuery
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Get the total count of items in the database
            int totalCount = await claimsQuery
                .CountAsync();

            // Create a response object containing the paginated data and total count
            var response = new
            {
                TotalCount = totalCount,
                Claims = claims,
                SortOrder = sortOrder
            };

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var jsonString = System.Text.Json.JsonSerializer.Serialize(response, options);

            return Ok(jsonString);
        }

        //get image by claim id
        [HttpGet("getimagebyclaimid")]
        public async Task<ActionResult<ClaimImage>> GetImageByClaimId(int id)
        {
            var claimImage = await _context.ClaimImages
                                .Where(c => c.ClaimId.Equals(id)).ToListAsync();
            if (claimImage == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(claimImage);
            }
        }

        //upload claim image
        [HttpPost("uploadimage")]
        public async Task<ActionResult<ClaimImage>> CreateClaimImage([FromBody] ClaimImage claimImage)
        {
            try
            { 
                var claimid = await _context.Claims.SingleOrDefaultAsync(c => c.ClaimId.Equals(claimImage.ClaimId));
                if (claimid == null)
                {
                    ModelState.AddModelError(string.Empty, "No Claim Found");
                }
                else
                {
                    // Decode base64 image data
                    var logoBytes = Convert.FromBase64String(claimImage.Url);

                    // Save the image file to the server
                    //var fileExtension = Path.GetExtension(company.Logo);
                    var fileName = Guid.NewGuid().ToString() + /*fileExtension*/ ".png";
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(logoBytes);
                    }

                    // Create a new company entity
                    var newclaimImage = new ClaimImage
                    {
                        Url = "images/" + fileName, // Store the link to the image file
                        ClaimId = claimImage.ClaimId
                    };

                    _context.ClaimImages.Add(newclaimImage);
                    await _context.SaveChangesAsync();

                    return Ok("Add Image Successfully");
                }
                
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }   

        return BadRequest(ModelState);
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
            try
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
                var policy = _context.PolicyOnUsers.FirstOrDefault(p => p.PolicyId == claim.PolicyId && p.UserId == claim.UserId);

                if (policy == null)
                {
                    return NotFound("Policy not found");
                }

                if (policy.AvaibleAmount < claim.AppAmount)
                {
                    return BadRequest("Insufficient available amount on the policy");
                }

                policy.AvaibleAmount -= claim.AppAmount;
                _context.Update(policy);
                _context.SaveChanges();

                return Ok("Claim approved and policy updated");
                //return Ok(policy.AvaibleAmount);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, "An error occurred while processing the claim: " + ex.Message);
            }
            
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

        //print invoice ver2
        [HttpGet("print/{id}")]
        public async Task<IActionResult> GeneratePDF(int id)
        {
            var document = new PdfDocument();

            var claim = await _context.Claims.FindAsync(id);

            if (claim != null)
            {
                if (claim.Status == true)
                {
                    var user = await _context.Users.FindAsync(claim.UserId);
                    var policy = await _context.Policies.FindAsync(claim.PolicyId);
                    var company = await _context.Companies.FindAsync(policy.CompanyId);

                    //show logo company
                    string filepath = environment.WebRootPath + "\\" + company.Logo;
                    byte[] imgarray = System.IO.File.ReadAllBytes(filepath);
                    string base64 = Convert.ToBase64String(imgarray);
                    string imgeurl = "data:image/png;base64, " + base64 + "";
                    string htmlcontent = "<div style='width:100%; text-align:center'>";
                    //Console.WriteLine("Image URL: " + imgeurl); // If using Console.WriteLine

                    htmlcontent += "<img style='width:80px;height:80%' src='" + imgeurl + "'   />";

                    htmlcontent += "<h2>Welcome to " + company.CompanyName + " </h2>";
                    htmlcontent += "<h2> Invoice No:" + claim.ClaimId + " & Invoice Date:" + claim.CreateDate + "</h2>";
                    htmlcontent += "<h3> Customer : " + user.Name + "</h3>";
                    htmlcontent += "<p>" + user.Address + "</p>";
                    htmlcontent += "<h3> Phone : " + user.Phone + " & Email : " + user.Email + "</h3>";
                    htmlcontent += "<div>";
                    htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
                    htmlcontent += "<thead style='font-weight:bold'>";
                    htmlcontent += "<tr>";
                    htmlcontent += "<td style='border:1px solid #000'> Policy Name </td>";
                    htmlcontent += "<td style='border:1px solid #000'> Description </td>";
                    //htmlcontent += "<td style='border:1px solid #000'>Quantity</td>";
                    htmlcontent += "<td style='border:1px solid #000'>Approve Amount</td >";
                    htmlcontent += "<td style='border:1px solid #000'>Total</td>";
                    htmlcontent += "</tr>";
                    htmlcontent += "</thead >";

                    htmlcontent += "<tbody>";
                    htmlcontent += "<tr>";
                    htmlcontent += "<td>" + policy.PolicyName + "</td>";
                    htmlcontent += "<td>" + claim.Description + "</td>";
                    htmlcontent += "<td>" + claim.AppAmount + "</td>";
                    htmlcontent += "<td> " + claim.AppAmount + "</td >";
                    htmlcontent += "</tr>";
                    htmlcontent += "</tbody>";

                    htmlcontent += "</table>";
                    htmlcontent += "</div>";

                    htmlcontent += "<div style='text-align:right'>";
                    htmlcontent += "<h1> Summary Info </h1>";
                    htmlcontent += "<table style='border:1px solid #000;float:right' >";
                    htmlcontent += "<tr>";
                    htmlcontent += "<td style='border:1px solid #000'> Summary Total </td>";
                    //htmlcontent += "<td style='border:1px solid #000'> Summary Tax </td>";
                    //htmlcontent += "<td style='border:1px solid #000'> Summary NetTotal </td>";
                    htmlcontent += "</tr>";

                    htmlcontent += "<tr>";
                    htmlcontent += "<td style='border: 1px solid #000'> " + claim.AppAmount + " </td>";

                    htmlcontent += "</tr>";

                    htmlcontent += "</table>";
                    htmlcontent += "</div>";

                    htmlcontent += "</div>";
                    PdfGenerator.AddPdfPages(document, htmlcontent, PageSize.A4);
                    byte[]? response = null;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        document.Save(ms);
                        response = ms.ToArray();
                    }
                    string Filename = "Invoice_" + id + ".pdf";
                    return File(response, "application/pdf", Filename);
                }
                else
                {
                    return BadRequest("Claim did not accepted !");
                }
            }
            else
            {
                return NotFound("Claim not found !");
            }
        }

    }
}
