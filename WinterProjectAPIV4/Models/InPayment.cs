using System;
using System.Collections.Generic;

namespace WinterProjectAPIV4.Models;

public partial class InPayment
{
    public int TransactionId { get; set; }

    public int? UserGroupId { get; set; }

    public double? Amount { get; set; }

    public virtual UserGroup? UserGroup { get; set; }
}
