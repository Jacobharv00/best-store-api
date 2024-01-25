using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;

        public AccountController(IConfiguration configuration, ApplicationDbContext context)
        {
            this.configuration = configuration;
            this.context = context;
        }

        // [HttpGet("TestToken")]
        // public IActionResult TestToken()
        // {
        //     User user = new User() { Id = 2, Role = "admin" };
        //     string jwt = CreateJWToken(user);
        //     var response = new { JwtToken = jwt };

        //     return Ok(response);
        // }

        [HttpPost("Register")]
        public IActionResult Register(UserDto user)
        {
            var emailCount = context.Users.Count(u => u.Email == user.Email);

            if (emailCount > 0)
            {
                ModelState.AddModelError("Email", "This email address is already is use");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), user.Pasword);

            User newUser = new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone ?? "",
                Address = user.Address,
                Pasword = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now
            };

            context.Users.Add(newUser);
            context.SaveChanges();

            var jwt = CreateJWToken(newUser);

            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = newUser.Id,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Phone = newUser.Phone,
                Address = newUser.Address,
                Role = newUser.Role,
                CreatedAt = newUser.CreatedAt,
            };

            var response = new { Token = jwt, User = userProfileDto };

            return Ok(response);
        }

        [HttpPost("Login")]
        public IActionResult Login(string email, string password)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Wrong Information");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Pasword, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Wrong Information");
                return BadRequest(ModelState);
            }

            var jwt = CreateJWToken(user);

            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
            };

            var response = new { Token = jwt, User = userProfileDto };

            return Ok(response);
        }

        private string CreateJWToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", "" + user.Id),
                new Claim("role", user.Role)
            };

            string strKey = configuration["JwtSettings:Key"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
