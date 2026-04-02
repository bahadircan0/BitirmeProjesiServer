using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {

        private readonly AppDbContext _context;

        public MeetingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Meeting meeting)
        {
            await _context.Meetings.AddAsync(meeting);
        }

        public async Task AddParticipantAsync(MeetingParticipant participant)
        {
            await _context.MeetingParticipants.AddAsync(participant);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<Meeting?> GetByIdAsync(int meetingId)
        {
            return await _context.Meetings
                .FirstOrDefaultAsync(m => m.RecordId == meetingId && !m.Deleted);
        }

        public async Task<List<Meeting>> GetMeetingsByUserIdAsync(int userId, string role)
        {

            if (role == "Teacher" || role == "1")
            {
                return await _context.Meetings
                    .Where(m => m.TeacherId == userId && !m.Deleted)
                    .ToListAsync();
            }
            else
            {
                return await _context.MeetingParticipants
                    .Where(mp => mp.UserId == userId && !mp.Deleted)
                    .Include(mp => mp.Meeting) 
                    .Select(mp => mp.Meeting)
                    .Where(m => !m.Deleted)
                    .ToListAsync();
            }
        }

        public async Task<bool> IsParticipantAsync(int meetingId, int userId)
        {
            return await _context.MeetingParticipants
                    .AnyAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId && !mp.Deleted);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
