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

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async  Task<List<User>> GetStudentsByTeacherIdAsync(int teacherId)
        {
            return await _context.TeacherStudents
        .Where(ts => ts.TeacherId == teacherId && !ts.Deleted) // Hocaya ait ve silinmemiş kayıtlar
        .Select(ts => ts.Student) // Ara tablodan "Student" (User) nesnesine geçiş yapıyoruz
        .Where(s => !s.Deleted) // Öğrenci de silinmemiş olmalı
        .ToListAsync();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
