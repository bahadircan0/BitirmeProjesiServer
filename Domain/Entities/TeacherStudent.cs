using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TeacherStudent
{
    public int RecordId { get; set; }

    public int TeacherId { get; set; }

    public int StudentId { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public bool Deleted { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
