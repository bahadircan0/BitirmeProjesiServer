using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public int RecordId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<MeetingParticipant> MeetingParticipants { get; set; } = new List<MeetingParticipant>();

    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TeacherStudent> TeacherStudentStudents { get; set; } = new List<TeacherStudent>();

    public virtual ICollection<TeacherStudent> TeacherStudentTeachers { get; set; } = new List<TeacherStudent>();
}
