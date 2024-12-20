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
        [HttpGet("statistics")]
        [Authorize(Policy = "SeniorPolicy")]
        public IActionResult GetStatistics()
        {
            try
            {
                using (SQL sql = new SQL())
                {
                    var prints = sql.Prints.ToList();

                    var totalBlackWhite = prints.Sum(p => p.FDb);
                    var totalDoubleSidedBlackWhite = prints.Sum(p => p.FFDb);
                    var totalColored = prints.Sum(p => p.SzDb);
                    var totalDoubleSidedColored = prints.Sum(p => p.SzszDb);
                    var totalScanning = prints.Sum(p => p.ScDb);

                    var totalCash = (totalBlackWhite * 15) +
                                    (totalDoubleSidedBlackWhite * 25) +
                                    (totalColored * 80) +
                                    (totalDoubleSidedColored * 150) +
                                    (totalScanning * 10);

                    var result = new
                    {
                        TotalBlackWhite = totalBlackWhite,
                        TotalDoubleSidedBlackWhite = totalDoubleSidedBlackWhite,
                        TotalColored = totalColored,
                        TotalDoubleSidedColored = totalDoubleSidedColored,
                        TotalScanning = totalScanning,
                        TotalCash = totalCash
                    };

                    return Ok(new APIResponse
                    {
                        StatusCode = 200,
                        Message = "Statistics fetched successfully.",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse
                {
                    StatusCode = 500,
                    Message = "An error occurred while processing the request.",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPrintStats([FromQuery] int? userId = null)
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    var query = sql.Prints.AsQueryable();

                    // Apply filter if userId is specified
                    if (userId.HasValue)
                    {
                        query = query.Where(p => p.UserId == userId.Value);
                    }

                    var stats = await query
                        .GroupBy(p => p.UserId)
                        .Select(group => new
                        {
                            UserId = group.Key,
                            Prints = group.Count()
                        })
                        .ToListAsync();

                    // Include user details
                    var userStats = stats.Select(s => new
                    {
                        UserId = s.UserId,
                        Name = sql.Users.FirstOrDefault(u => u.Id == s.UserId)?.Username ?? "Unknown",
                        Prints = s.Prints
                    }).ToList();

                    response.StatusCode = 200;
                    response.Message = "Statistics fetched successfully!";
                    response.Data = userStats;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = new { ex.Message, ex.StackTrace };
                return BadRequest(response);
            }
        }

    }
}
