using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinterSystem.Database;
using PrinterSystem.Models;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeniorController : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetSeniors()
        {
            using (var sql = new SQL())
            {
                var senior = await sql.Users.Where(u => u.Role == Role.Senior).ToListAsync();
                return Ok(senior);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSenior(int id, [FromBody] User updatedSenior)
        {
            APIResponse response = new APIResponse();

            using (var sql = new SQL())
            {
                var senior = await sql.Users.FindAsync(id);
                if (senior == null || senior.Role != Role.Senior) return NotFound("Nem található Senior.");

                if (senior.Name != updatedSenior.Name)
                {
                    senior.Name = updatedSenior.Name;
                }
                if (senior.Username != updatedSenior.Username)
                {
                    senior.Username = updatedSenior.Username;
                }
                senior.Role = Role.Senior;

                await sql.SaveChangesAsync();

                response.StatusCode = 200;
                response.Date = new DateTime();

                return Ok(response);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSenior(int id)
        {
            APIResponse response = new APIResponse();
            using (var sql = new SQL())
            {
                var senior = await sql.Users.FindAsync(id);
                if (senior == null || senior.Role != Role.Senior) return NotFound("A megadott felhasználó nem található, vagy nem Senior.");

                sql.Users.Remove(senior);
                await sql.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Senior sikeresen törölve!";
                return Ok(response);
            }
        }
    }
}
