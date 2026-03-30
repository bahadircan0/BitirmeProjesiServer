using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("GetMyStudents")]
        public async Task<IActionResult> GetMyStudents()
        {
            try
            {
                // 1. Token içindeki NameIdentifier claim'inden Hoca ID'sini alıyoruz
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

                int teacherId = int.Parse(userIdClaim.Value);

                // 2. Repository üzerinden bu hocaya bağlı öğrencileri çekiyoruz
                // (Repository'ye bu metodu birazdan ekleyeceğiz)
                var students = await _userRepository.GetStudentsByTeacherIdAsync(teacherId);

                // 3. Frontend'e sadece ihtiyacı olanı gönderelim (şifreleri göndermeyelim!)
                var response = students.Select(s => new
                {
                    s.RecordId,
                    s.Name,
                    s.Surname,
                    s.Email
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Öğrenciler getirilirken bir hata oluştu: {ex.Message}");
            }

        }
    }
}
