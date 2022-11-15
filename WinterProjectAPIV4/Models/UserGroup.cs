using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinterProjectAPIV4.Models;

public partial class UserGroup
{
    public int UserGroupId { get; set; }

    public int? UserId { get; set; }

    public int? GroupId { get; set; }

    public bool? IsOwner { get; set; }

    [JsonIgnore]
    public virtual ICollection<Expense> Expenses { get; } = new List<Expense>();

    public virtual ShareGroup? Group { get; set; }

    [JsonIgnore]
    public virtual ICollection<InPayment> InPayments { get; } = new List<InPayment>();

    public virtual ShareUser? User { get; set; }
}
