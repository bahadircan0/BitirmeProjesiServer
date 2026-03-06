using System;
using System.Collections.Generic;

namespace Domain.Entities;
public partial class MeetingParticipant
{
    public int RecordId { get; set; }

    public int MeetingId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public bool Deleted { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
