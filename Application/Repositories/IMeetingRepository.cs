using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IMeetingRepository
    {
         Task AddAsync(Meeting meeting);
        Task AddParticipantAsync(MeetingParticipant participant);
        Task SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<List<Meeting>> GetMeetingsByUserIdAsync(int userId, string role);

    }
}
