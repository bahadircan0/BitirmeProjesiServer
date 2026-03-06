using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;

        // Dependency Injection ile sadece Interface'i istiyoruz. Temiz mimari!
        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpPost("add-test-role")]
        public async Task<IActionResult> AddTestRole()
        {
            try
            {
                var newRole = new Role
                {
                    Name = "Test Role",
                    Description = "Bu rol, temiz mimariye uygun olarak eklenmiştir.",
                    CreatedDatetime = DateTime.Now,
                    Deleted = false
                };

                await _roleRepository.AddAsync(newRole);
                await _roleRepository.SaveAsync();

                return Ok("Temiz mimariye uygun olarak Rol eklendi! 🚀");
            }
            catch (Exception ex)
            {
                return BadRequest($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
