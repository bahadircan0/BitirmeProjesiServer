using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly IMeetingRepository _meetingRepository; 
        private readonly IUserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;


        public MeetingController(IMeetingRepository meetingRepository, IUserRepository userRepository, IHttpClientFactory httpClientFactory)
        {
            _meetingRepository = meetingRepository;
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
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
                var dailyClient = _httpClientFactory.CreateClient("DailyClient");

                var roomPayload = new
                {
                    name = $"meeting-{Guid.NewGuid():N}",
                    privacy = "private",
                    properties = new
                    {
                        exp = new DateTimeOffset(dto.EndTime).ToUnixTimeSeconds(),
                        enable_chat = false,
                        enable_screenshare = false
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(roomPayload),
                    Encoding.UTF8,
                    "application/json");

                var dailyResponse = await dailyClient.PostAsync("rooms", content);

                if (!dailyResponse.IsSuccessStatusCode)
                {
                    var err = await dailyResponse.Content.ReadAsStringAsync();
                    return BadRequest($"Daily.co oda oluşturma hatası: {err}");
                }

                var dailyJson = await dailyResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(dailyJson);
                var root = doc.RootElement;

                string dailyRoomName = root.GetProperty("name").GetString()!;
                string dailyRoomUrl = root.GetProperty("url").GetString()!;



                var newMeeting = new Meeting
                {
                    TeacherId = teacherId,
                    Title = dto.Title,
                    Description = dto.Description,
                    DailyRoomName = dailyRoomName,
                    DailyRoomUrl = dailyRoomUrl,
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

                var eventList = meetings.Select(m => new
                {
                    id = m.RecordId,
                    title = m.Title,
                    start = m.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = m.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = m.Description,
                    dailyRoomUrl = m.DailyRoomUrl,
                    backgroundColor = role == "Teacher" ? "#7367F0" : "#28C76F"
                });

                return Ok(eventList);
            }
            catch (Exception ex)
            {
                return BadRequest($"Toplantılar yüklenirken hata: {ex.Message}");
            }
        }

        [HttpGet("GetMeetingToken/{meetingId}")]
        public async Task<IActionResult> GetMeetingToken(int meetingId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var role = User.FindFirst(ClaimTypes.Role)!.Value;
                var isTeacher = role == "Teacher";

                var meeting = await _meetingRepository.GetByIdAsync(meetingId);
                if (meeting == null) return NotFound("Toplantı bulunamadı.");

                if (!isTeacher)
                {
                    var isParticipant = await _meetingRepository.IsParticipantAsync(meetingId, userId);
                    if (!isParticipant) return Forbid();
                }

                var dailyClient = _httpClientFactory.CreateClient("DailyClient");

                var tokenPayload = new
                {
                    properties = new
                    {
                        room_name = meeting.DailyRoomName,
                        is_owner = isTeacher,  
                        exp = new DateTimeOffset(meeting.EndTime).ToUnixTimeSeconds()
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(tokenPayload),
                    Encoding.UTF8,
                    "application/json");

                var tokenResponse = await dailyClient.PostAsync("meeting-tokens", content);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var err = await tokenResponse.Content.ReadAsStringAsync();
                    return BadRequest($"Token oluşturma hatası: {err}");
                }

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(tokenJson);
                var meetingToken = doc.RootElement.GetProperty("token").GetString();

                var joinUrl = $"{meeting.DailyRoomUrl}?t={meetingToken}";

                return Ok(new { joinUrl });
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata: {ex.Message}");
            }
        }

        [HttpPost("RunCode")]
        public async Task<IActionResult> RunCode([FromBody] RunCodeDto dto)
        {
            try
            {
                var judge0Client = _httpClientFactory.CreateClient("Judge0Client");

                var payload = new
                {
                    source_code = dto.Code,
                    language_id = dto.LanguageId,
                    stdin = ""
                };

                var response = await judge0Client.PostAsJsonAsync("submissions?base64_encoded=false&wait=true", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { message = $"Judge0 Hatası ({response.StatusCode}): {errorContent}" });
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                using var resultDoc = JsonDocument.Parse(resultJson);
                var root = resultDoc.RootElement;

                string output = "Çıktı yok.";

                if (root.TryGetProperty("stdout", out var stdout) && stdout.ValueKind != JsonValueKind.Null)
                    output = stdout.GetString()!;
                else if (root.TryGetProperty("stderr", out var stderr) && stderr.ValueKind != JsonValueKind.Null)
                    output = "Hata:\n" + stderr.GetString()!;
                else if (root.TryGetProperty("compile_output", out var compile) && compile.ValueKind != JsonValueKind.Null)
                    output = "Derleme hatası:\n" + compile.GetString()!;

                return Ok(new { output });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Çalıştırma hatası: {ex.Message}" });
            }
        }


    }
}
