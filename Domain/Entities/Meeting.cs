using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Meeting
{
    public int RecordId { get; set; }

    public int TeacherId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? DailyRoomName { get; set; }

    public string? DailyRoomUrl { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<MeetingParticipant> MeetingParticipants { get; set; } = new List<MeetingParticipant>();

    public virtual User Teacher { get; set; } = null!;
}
