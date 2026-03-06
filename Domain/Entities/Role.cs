using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Role
{
    public int RecordId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
