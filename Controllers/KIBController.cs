using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinterSystem.Database;
using PrinterSystem.Models;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KIBController : ControllerBase
    {

        [HttpGet("")]
        public async Task<IActionResult> GetKIBUsers()
        {
            using (var sql = new SQL())
            {
                var kibes = await sql.Users.Where(u => u.Role == Role.KIB).ToListAsync();
                return Ok(kibes);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKIB(int id, [FromBody] User updatedKIB)
        {
            APIResponse response = new APIResponse();

            using (var sql = new SQL())
            {
                var kib = await sql.Users.FindAsync(id);
                if (kib == null || kib.Role != Role.KIB) return NotFound("KIB not found.");

                if(kib.Name != updatedKIB.Name)
                {
                    kib.Name = updatedKIB.Name;
                }
                if (kib.Username != updatedKIB.Username)
                {
                    kib.Username = updatedKIB.Username;
                }
                kib.Role = Role.KIB;

                await sql.SaveChangesAsync();

                response.StatusCode = 200;
                response.Date = new DateTime();

                return Ok(response);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKIB(int id)
        {
            APIResponse response = new APIResponse();
            using (var sql = new SQL())
            {
                var kib = await sql.Users.FindAsync(id);
                if (kib == null || kib.Role != Role.KIB) return NotFound("KIB not found.");

                sql.Users.Remove(kib);
                await sql.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "KIB deleted successfully!";
                return Ok(response);
            }
        }

    }
}
