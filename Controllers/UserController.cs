﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrinterSystem.Database;
using PrinterSystem.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace PrinterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            APIResponse response = new APIResponse();

            try
            {
                using (SQL sql = new SQL())
                {
                    var users = await sql.Users.OrderBy(x => x.Id).ToListAsync();
                    response.StatusCode = 200;
                    response.Message = "Sikeres lekérdezés!";
                    response.Data = users;
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

        //TODO: Megcsinálni a logikát az updatere, és a deletere
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromHeader] int id)
        {
            APIResponse response = new APIResponse();
            response.StatusCode = 200;
            response.Message = "User updated successfully!";
            try
            {
                using (SQL sql = new SQL())
                {
                    var user = await sql.Users.FirstOrDefaultAsync(x => x.Id == id);
                    if (null == user)
                    {
                        response.StatusCode = 404;
                        response.Message = "User not found!";
                        return NotFound(response);
                    }
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
