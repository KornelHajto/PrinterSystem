using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrinterSystem.Database;
using PrinterSystem.Models;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                    var prints = sql.Prints.OrderBy(x => x.Id).ToList();
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
                response.Data = ex;
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
                response.Data = ex;
            }
            return BadRequest(response);
        }
    }

}
