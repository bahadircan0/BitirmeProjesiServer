using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly IMeetingRepository _meetingRepository; 
        private readonly IUserRepository _userRepository;

        public MeetingController(IMeetingRepository meetingRepository, IUserRepository userRepository)
        {
            _meetingRepository = meetingRepository;
            _userRepository = userRepository;
        }

        [HttpPost("AddMeeting")]
        public async Task<IActionResult> AddMeeting([FromBody] CreateMeetingDto dto)
        {
            var teacherIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (teacherIdClaim == null) return Unauthorized("Hoca kimliği bulunamadı.");

            int teacherId = int.Parse(teacherIdClaim.Value);

            
            using var transaction = await _meetingRepository.BeginTransactionAsync();

            try
            {
                var newMeeting = new Meeting
                {
                    TeacherId = teacherId,
                    Title = dto.Title,
                    Description = dto.Description,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Status = "Active",
                    CreatedDatetime = DateTime.Now,
                    Deleted = false
                };

                await _meetingRepository.AddAsync(newMeeting);
                await _meetingRepository.SaveAsync(); 

               
                if (dto.ParticipantIds != null && dto.ParticipantIds.Any())
                {
                    foreach (var studentId in dto.ParticipantIds)
                    {
                        var participant = new MeetingParticipant
                        {
                            MeetingId = newMeeting.RecordId, 
                            UserId = studentId,
                            CreatedDatetime = DateTime.Now,
                            Deleted = false
                        };
                        await _meetingRepository.AddParticipantAsync(participant);
                    }
                    await _meetingRepository.SaveAsync();
                }

              
                await transaction.CommitAsync();

                return Ok(new { Message = "Toplantı ve katılımcılar başarıyla oluşturuldu!", MeetingId = newMeeting.RecordId });
            }
            catch (Exception ex)
            {
                
                await transaction.RollbackAsync();
                return BadRequest($"Toplantı oluşturulurken hata: {ex.Message}");
            }
        }


        [HttpGet("GetMeetings")]
        public async Task<IActionResult> GetMeetings()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var role = User.FindFirst(ClaimTypes.Role)!.Value;

                var meetings = await _meetingRepository.GetMeetingsByUserIdAsync(userId, role);

                // FullCalendar'ın beklediği JSON formatına dönüştürüyoruz
                var eventList = meetings.Select(m => new
                {
                    id = m.RecordId,
                    title = m.Title,
                    start = m.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = m.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = m.Description,
                    // İsteğe bağlı: Hoca ve öğrenciyi ayırmak için renk ekleyebilirsin
                    backgroundColor = role == "Teacher" ? "#7367F0" : "#28C76F"
                });

                return Ok(eventList);
            }
            catch (Exception ex)
            {
                return BadRequest($"Toplantılar yüklenirken hata: {ex.Message}");
            }
        }


    }
}
