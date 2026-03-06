using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistance.Contexts;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Meeting> Meetings { get; set; }

    public virtual DbSet<MeetingParticipant> MeetingParticipants { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TeacherStudent> TeacherStudents { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Database=BitirmeProjesi_DB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MEETING__E2DA602F1DCD066C");

            entity.ToTable("MEETING");

            entity.Property(e => e.RecordId).HasColumnName("RECORD_ID");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DATETIME");
            entity.Property(e => e.DailyRoomName)
                .HasMaxLength(255)
                .HasColumnName("DAILY_ROOM_NAME");
            entity.Property(e => e.DailyRoomUrl)
                .HasMaxLength(500)
                .HasColumnName("DAILY_ROOM_URL");
            entity.Property(e => e.Deleted).HasColumnName("DELETED");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("END_TIME");
            entity.Property(e => e.ModifiedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFIED_DATETIME");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("START_TIME");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("STATUS");
            entity.Property(e => e.TeacherId).HasColumnName("TEACHER_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("TITLE");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEETING_TEACHER");
        });

        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MEETING___E2DA602F92CB4CF1");

            entity.ToTable("MEETING_PARTICIPANT");

            entity.HasIndex(e => new { e.MeetingId, e.UserId }, "UQ_MEETING_USER").IsUnique();

            entity.Property(e => e.RecordId).HasColumnName("RECORD_ID");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DATETIME");
            entity.Property(e => e.Deleted).HasColumnName("DELETED");
            entity.Property(e => e.MeetingId).HasColumnName("MEETING_ID");
            entity.Property(e => e.ModifiedDatetime).HasColumnName("MODIFIED_DATETIME");
            entity.Property(e => e.UserId).HasColumnName("USER_ID");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingParticipants)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEETINGPARTICIPANT_MEETING");

            entity.HasOne(d => d.User).WithMany(p => p.MeetingParticipants)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MEETINGPARTICIPANT_USER");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__ROLE__E2DA602F5A184CB6");

            entity.ToTable("ROLE");

            entity.Property(e => e.RecordId).HasColumnName("RECORD_ID");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DATETIME");
            entity.Property(e => e.Deleted).HasColumnName("DELETED");
            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.ModifiedDatetime).HasColumnName("MODIFIED_DATETIME");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<TeacherStudent>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__TEACHER___E2DA602FD93B6CC6");

            entity.ToTable("TEACHER_STUDENT");

            entity.HasIndex(e => new { e.TeacherId, e.StudentId }, "UQ_TEACHER_STUDENT").IsUnique();

            entity.Property(e => e.RecordId).HasColumnName("RECORD_ID");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DATETIME");
            entity.Property(e => e.Deleted).HasColumnName("DELETED");
            entity.Property(e => e.ModifiedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("MODIFIED_DATETIME");
            entity.Property(e => e.StudentId).HasColumnName("STUDENT_ID");
            entity.Property(e => e.TeacherId).HasColumnName("TEACHER_ID");

            entity.HasOne(d => d.Student).WithMany(p => p.TeacherStudentStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEACHER_STUDENT_STUDENT");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherStudentTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEACHER_STUDENT_TEACHER");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__USER__E2DA602F4F51B7DF");

            entity.ToTable("USER");

            entity.HasIndex(e => e.Email, "UQ_USER_EMAIL").IsUnique();

            entity.Property(e => e.RecordId).HasColumnName("RECORD_ID");
            entity.Property(e => e.CreatedDatetime)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DATETIME");
            entity.Property(e => e.Deleted).HasColumnName("DELETED");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("EMAIL");
            entity.Property(e => e.ModifiedDatetime).HasColumnName("MODIFIED_DATETIME");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("NAME");
            entity.Property(e => e.Password)
                .HasMaxLength(300)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.Surname)
                .HasMaxLength(255)
                .HasColumnName("SURNAME");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
