using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user is null)
            {
                return NotFound();
            }

            var oldPasswordReset = context.PasswordResets.FirstOrDefault(r => r.Email == email);
            if (oldPasswordReset is not null)
            {
                context.Remove(oldPasswordReset);
            }

            string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            var passwordReset = new PasswordReset()
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now
            };

            context.PasswordResets.Add(passwordReset);
            context.SaveChanges();

            // ========== Start of sending a user the email flow ==========
            // string emailSubject = "Password Reset";
            // string username = user.FirstName + " " + user.LastName;
            // string emailMessage =
            //     "Dear "
            //     + username
            //     + "\n"
            //     + "We received your password reset request.\n"
            //     + "Please copy the following token and paste it in the Password Reset Form:\n"
            //     + token
            //     + "\n\n"
            //     + "Best Regards\n";

            // Send Email but I didnot signup for 3rd party service or implement
            //  the EmailSender class & method
            // ========== What the code would be to send user token ==========
            // emailSender.SendEmail(emailSubject, email, username, emailMessage).Wait();

            return Ok();
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string token, string password)
        {
            var pwdReset = context.PasswordResets.FirstOrDefault(r => r.Token == token);
            if (pwdReset is null)
            {
                ModelState.AddModelError("Token", "Wrong Information");
                return BadRequest(ModelState);
            }

            var user = context.Users.FirstOrDefault(u => u.Email == pwdReset.Email);
            if (user is null)
            {
                ModelState.AddModelError("Token", "Wrong Information");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            string encryptedPassword = passwordHasher.HashPassword(new User(), password);

            user.Pasword = encryptedPassword;
            context.PasswordResets.Remove(pwdReset);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpGet("Profile")]
        public IActionResult GetProfile()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity is null)
            {
                return Unauthorized();
            }

            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
            if (claim is null)
            {
                return Unauthorized();
            }

            int id;
            try
            {
                id = int.Parse(claim.Value);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            var user = context.Users.Find(id);
            if (user is null)
            {
                return Unauthorized();
            }

            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }

        // ========== Get Claim Information ==========
        // [Authorize]
        // [HttpGet("GetTokenClaims")]
        // public IActionResult GetTokenClaims()
        // {
        //     var identity = User.Identity as ClaimsIdentity;

        //     if (identity != null)
        //     {
        //         Dictionary<string, string> claims = new Dictionary<string, string>();

        //         foreach (Claim claim in identity.Claims)
        //         {
        //             claims.Add(claim.Type, claim.Value);
        //         }

        //         return Ok(claims);
        //     }

        //     return Ok();
        // }

        // ========== Testing Authorized Routes ==========
        // [Authorize]
        // [HttpGet("AuthorizedUser")]
        // public IActionResult AuthorizedUser()
        // {
        //     return Ok("You are a user and are authorized!");
        // }

        // [Authorize(Roles = "admin")]
        // [HttpGet("AuthorizeAdminUser")]
        // public IActionResult AuthorizeAdminUser()
        // {
        //     return Ok("You are a admin and are authorized");
        // }

        // [Authorize(Roles = "admin, seller")]
        // [HttpGet("AuthorizeAdminAndSeller")]
        // public IActionResult AuthorizeAdminAndSeller()
        // {
        //     return Ok("You are either a admin or a seller and are authorized");
        // }

        // ========== Helpers ==========
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
