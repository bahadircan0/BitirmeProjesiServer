using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<int> SaveAsync();
        Task AddTeacherStudentAsync(TeacherStudent relation);
        Task<List<User>> GetStudentsByTeacherIdAsync(int teacherId);
        Task<bool> IsApprovedTeacherAsync(string email);

        Task<List<User>> GetTeachersOfStudentAsync(int studentId);
        Task<bool> CheckTeacherStudentRelationExistsAsync(int studentId, int teacherId);

        Task<bool> UpdateProfileByIdAsync(int userId, UpdateProfileDto request);
    }
}
