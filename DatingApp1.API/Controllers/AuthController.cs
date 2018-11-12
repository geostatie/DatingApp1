using Microsoft.AspNetCore.Mvc;
using DatingApp1.API.Data;
using DatingApp1.API.Models;
using System.Threading.Tasks;
using DatingApp1.API.Dtos;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp1.API.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this._repo=repo;
            this._config=config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate input

            userForRegisterDto.Name= userForRegisterDto.Name.ToLower();

            if(await _repo.UserExists(userForRegisterDto.Name))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate= new User() {
                Name=userForRegisterDto.Name
            };

            var userCreated=await _repo.Register(userToCreate, userForRegisterDto.Password);

            //return CreatedAtRoute(...);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login( userForLoginDto.Name.ToLower(), userForLoginDto.Password);
            
            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim( ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim( ClaimTypes.Name, userFromRepo.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

                var creds=new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject=new ClaimsIdentity(claims),
                    Expires=DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenHandler= new JwtSecurityTokenHandler();

                var token=tokenHandler.CreateToken(tokenDescriptor);

                return Ok( new {
                    token=tokenHandler.WriteToken(token)
                });
        }
    }
}