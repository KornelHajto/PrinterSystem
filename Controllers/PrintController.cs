using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinterSystem.Database;
using PrinterSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "KIBPolicy")] // Minimum KIB policy for all actions
    public class PrintController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPrints()
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    // Get user role and ID from JWT claims
                    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        response.StatusCode = 400;
                        response.Message = "User ID claim not found.";
                        return BadRequest(response);
                    }

                    int userId = int.Parse(userIdClaim);

                    if (role == nameof(Role.KIB))
                    {
                        // Fetch only the user's own prints
                        var prints = await sql.Prints.Where(p => p.UserId == userId).ToListAsync();
                        response.StatusCode = 200;
                        response.Message = "Sikeres lekérdezés!";
                        response.Data = prints;
                    }
                    else if (role == nameof(Role.Senior) || role == nameof(Role.Admin))
                    {
                        // Fetch all prints for senior or admin roles
                        var prints = await sql.Prints.ToListAsync();
                        response.StatusCode = 200;
                        response.Message = "Sikeres lekérdezés!";
                        response.Data = prints;
                    }
                    else
                    {
                        response.StatusCode = 403;
                        response.Message = "Access denied.";
                        return Forbid(response.Message);
                    }
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = new { ex.Message, ex.StackTrace };
            }
            return BadRequest(response);
        }

        [HttpGet("{user_id}")]
        public async Task<IActionResult> GetPrintByUser(int user_id)
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    var prints = await sql.Prints.Where(x => x.UserId == user_id).ToListAsync();
                    response.StatusCode = 200;
                    response.Message = "Sikeres lekérdezés!";
                    response.Data = prints;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = new { ex.Message, ex.StackTrace };
            }
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrint([FromBody] Print print)
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    // Automatically set the UserId based on the JWT token
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        response.StatusCode = 400;
                        response.Message = "User ID claim not found.";
                        return BadRequest(response);
                    }

                    print.UserId = int.Parse(userIdClaim);

                    await sql.Prints.AddAsync(print);
                    await sql.SaveChangesAsync();
                    response.StatusCode = 200;
                    response.Message = "Print added successfully!";
                    response.Data = print;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = new { ex.Message, ex.StackTrace };
            }
            return BadRequest(response);
        }
    }
}
