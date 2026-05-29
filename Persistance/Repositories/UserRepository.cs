using Application.DTOs;
using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTeacherStudentAsync(TeacherStudent relation)
        {
            await _context.Set<TeacherStudent>().AddAsync(relation);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> CheckTeacherStudentRelationExistsAsync(int studentId, int teacherId)
        {
            return await _context.TeacherStudents
        .AnyAsync(ts => ts.StudentId == studentId && ts.TeacherId == teacherId && !ts.Deleted);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async  Task<List<User>> GetStudentsByTeacherIdAsync(int teacherId)
        {
            return await _context.TeacherStudents
        .Where(ts => ts.TeacherId == teacherId && !ts.Deleted) 
        .Select(ts => ts.Student) 
        .Where(s => !s.Deleted) 
        .ToListAsync();
        }

        public async Task<List<User>> GetTeachersOfStudentAsync(int studentId)
        {
            
            var teacherIds = await _context.TeacherStudents
                .Where(ts => ts.StudentId == studentId && !ts.Deleted)
                .Select(ts => ts.TeacherId)
                .ToListAsync();

            return await _context.Users
                .Where(u => teacherIds.Contains(u.RecordId) && !u.Deleted)
                .ToListAsync();
        }

        public async Task<bool> IsApprovedTeacherAsync(string email)
        {
            return await _context.ApprovedTeachers
                .AnyAsync(x => x.Email == email && !x.IsDeleted);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProfileByIdAsync(int userId, UpdateProfileDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RecordId == userId && !u.Deleted);

            if (user == null)
                return false;

            user.Name = request.Name;
            user.Surname = request.Surname;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
