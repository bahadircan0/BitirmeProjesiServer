using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration; // appsettings.json'daki JWT ayarlarına ulaşmak için

        public AuthController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("Bu e-posta adresi zaten kullanılıyor.");
                }
                User? teacher = null;

                if (registerDto.RoleId == (int)UserRoles.Student)
                {
                    if (string.IsNullOrEmpty(registerDto.TeacherEmail))
                    {
                        return BadRequest("Öğrenciler kayıt olurken bir hoca e-postası girmek zorundadır!");
                    }

                    teacher = await _userRepository.GetByEmailAsync(registerDto.TeacherEmail);

                    if (teacher == null || teacher.RoleId != (int)UserRoles.Teacher)
                    {
                        return BadRequest("Girilen e-posta adresine ait geçerli bir hoca bulunamadı.");
                    }
                }

                var newUser = new User
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password,
                    Name = registerDto.Name,
                    Surname = registerDto.Surname,
                    RoleId = registerDto.RoleId,
                    CreatedDatetime = DateTime.Now,
                    Deleted = false
                };

                await _userRepository.AddUserAsync(newUser);
                await _userRepository.SaveAsync();

                if (registerDto.RoleId == (int)UserRoles.Student && teacher != null)
                {
                    var relation = new TeacherStudent
                    {
                        TeacherId = teacher.RecordId,
                        StudentId = newUser.RecordId,
                        CreatedDatetime = DateTime.Now,
                        Deleted = false
                    };

                    await _userRepository.AddTeacherStudentAsync(relation);
                    await _userRepository.SaveAsync();
                }


                return Ok("Kullanıcı başarıyla kaydedildi! 🎉");
            }
            catch (Exception ex)
            {
                return BadRequest($"Kayıt olurken bir hata oluştu: {ex.Message}");
            }
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);

                if (user == null || user.Password != loginDto.Password)
                {
                    return BadRequest("E-posta veya şifre hatalı.");
                }


                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.RecordId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(3), // Token 3 saat geçerli
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { Token = tokenString, Message = "Giriş başarılı!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Giriş yaparken bir hata oluştu: {ex.Message}");
            }
        }
    }
}
