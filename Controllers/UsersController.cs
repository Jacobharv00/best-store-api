using Azure;
using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public UsersController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult GetUsers(int? page)
        {
            if (page is null || page < 1)
            {
                page = 1;
            }

            var pageSize = 5;
            var totalPages = 0;

            decimal count = context.Users.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            var users = context.Users
                .Skip((int)(page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(u => u.Id)
                .ToList();

            List<UserProfileDto> userProfiles = new List<UserProfileDto>();
            foreach (var user in users)
            {
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

                userProfiles.Add(userProfileDto);
            }

            var response = new
            {
                Users = userProfiles,
                TotalPage = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = context.Users.Find(id);

            if (user is null)
            {
                return NotFound();
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
    }
}
