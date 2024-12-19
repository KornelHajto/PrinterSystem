using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinterSystem.Database;
using PrinterSystem.Models;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login lgm)
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    var user = await sql.Users.FirstOrDefaultAsync(u => u.Username == lgm.Username);
                    if (user == null)
                    {
                        response.StatusCode = 401;
                        response.Message = "Invalid username or password.";
                        return Unauthorized(response);
                    }

                    var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, lgm.Password);

                    if (verificationResult == PasswordVerificationResult.Failed)
                    {
                        response.StatusCode = 401;
                        response.Message = "Invalid username or password.";
                        return Unauthorized(response);
                    }

                    response.StatusCode = 200;
                    response.Message = "Login successful!";
                    response.Data = new { UserId = user.Id, Username = user.Username, Role = user.Role };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = ex;
            }
            return BadRequest(response);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            APIResponse response = new APIResponse();
            try
            {
                using (SQL sql = new SQL())
                {
                    user.Password = _passwordHasher.HashPassword(user, user.Password);

                    await sql.Users.AddAsync(user);
                    await sql.SaveChangesAsync();
                    response.StatusCode = 200;
                    response.Message = "User added successfully!";
                    response.Data = user;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = "An error occurred while processing your request.";
                response.Data = ex;
            }
            return BadRequest(response);
        }
    }
}
