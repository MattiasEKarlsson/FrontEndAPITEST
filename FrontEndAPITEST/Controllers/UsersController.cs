using FrontEndAPITEST.Data;
using FrontEndAPITEST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FrontEndAPITEST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SqlDbContext _context;
        private IConfiguration Configuration { get; }

        public UsersController(SqlDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUp model)
        {
            try
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserRole = model.Role 
                };
                user.CreatePassword(model.Password);

                if (!_context.Users.Any(u => u.Email == user.Email))
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    return new OkResult();
                }
                return new BadRequestResult();
            }
            catch
            {
                return new BadRequestResult();
            }
        }
        
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignIn user)
        {
            try
            {
                var _user = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                int id = _user.Id;
                string role = _user.UserRole;

                if (_user != null)
                {
                    if (_user.ValidatePassword(user.Password))
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var expiresDate = DateTime.Now.AddMinutes(60);

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim("UserId", _user.Id.ToString()),
                                new Claim("Expires", expiresDate.ToString())
                            }),
                            Expires = expiresDate,
                            SigningCredentials = new SigningCredentials
                            (new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("SecretKey").Value)), SecurityAlgorithms.HmacSha512Signature)
                        };

                        var _accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                        return new OkObjectResult(new { AccessToken = _accessToken, Role = role, Id = id});
                    }

                }

            }
            catch { }

            return new BadRequestResult();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAllUsers()
        {
            var all = from c in _context.Users select c;

            _context.Users.RemoveRange(all);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }

}

