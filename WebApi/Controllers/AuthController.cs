using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
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
                // 1. E-posta kontrolü
                var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("Bu e-posta adresi zaten kullanılıyor.");
                }

                // 2. Yeni kullanıcıyı oluştur (Şifre düz metin)
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

                // 3. Veritabanına kaydet
                await _userRepository.AddUserAsync(newUser);
                await _userRepository.SaveAsync();

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
            // 1. Kullanıcıyı veritabanından çekiyoruz (UserRepository'de Include(u => u.Role) var!)
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            // 2. Kullanıcı var mı ve şifresi doğru mu?
            if (user == null || user.Password != loginDto.Password)
            {
                return BadRequest("E-posta veya şifre hatalı.");
            }

            // --- TOKEN (BİLET) ÜRETME AŞAMASI ---

            // appsettings.json'daki gizli anahtarımızı alıyoruz
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Token içine basacağımız bilgiler (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.RecordId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
                
                // EN KRİTİK YER: Kullanıcının rol adını Token'a basıyoruz! (User.Role null gelmemeli)
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            // Token ayarlarını yapıyoruz
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3), // Token 3 saat geçerli
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Kullanıcıya Token'ı veriyoruz
            return Ok(new { Token = tokenString, Message = "Giriş başarılı!" });
        }
    }
}
