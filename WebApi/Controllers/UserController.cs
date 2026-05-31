using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

                int teacherId = int.Parse(userIdClaim.Value);

                var students = await _userRepository.GetStudentsByTeacherIdAsync(teacherId);

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


        [HttpGet("my-teachers")]
        public async Task<IActionResult> GetMyTeachers()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("Oturum bilgisi bulunamadı.");

                int studentId = int.Parse(userIdClaim.Value);

                var teachers = await _userRepository.GetTeachersOfStudentAsync(studentId);

                var result = teachers.Select(t => new {
                    t.RecordId,
                    t.Name,
                    t.Surname,
                    t.Email
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Hocalar getirilirken hata oluştu: {ex.Message}");
            }
        }

        [HttpPost("add-new-teacher")]
        public async Task<IActionResult> AddNewTeacher([FromBody] AddTeacherDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("Oturum bilgisi bulunamadı.");

                int studentId = int.Parse(userIdClaim.Value);

                var teacher = await _userRepository.GetByEmailAsync(dto.TeacherEmail);

                if (teacher == null || teacher.RoleId != (int)UserRoles.Teacher)
                {
                    return BadRequest("Girilen e-posta adresine ait geçerli bir hoca bulunamadı.");
                }

                bool exists = await _userRepository.CheckTeacherStudentRelationExistsAsync(studentId, teacher.RecordId);
                if (exists)
                {
                    return BadRequest("Bu hoca zaten listenizde ekli!");
                }

                var relation = new TeacherStudent
                {
                    TeacherId = teacher.RecordId,
                    StudentId = studentId, 
                    CreatedDatetime = DateTime.Now,
                    Deleted = false
                };

                await _userRepository.AddTeacherStudentAsync(relation);
                await _userRepository.SaveAsync();

                return Ok("Hoca başarıyla listenize eklendi! 🎉");
            }
            catch (Exception ex)
            {
                return BadRequest($"Hoca eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            try
            {
              
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Kullanıcı bilgisi token içerisinde bulunamadı.");

                int userId = int.Parse(userIdClaim.Value);

                
                var isSuccess = await _userRepository.UpdateProfileByIdAsync(userId, request);

                if (!isSuccess)
                {
                    return NotFound("Güncellenmek istenen kullanıcı sistemde bulunamadı.");
                }

                return Ok(new { message = "Profil bilgileriniz başarıyla güncellendi! ✅" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Profil güncellenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}