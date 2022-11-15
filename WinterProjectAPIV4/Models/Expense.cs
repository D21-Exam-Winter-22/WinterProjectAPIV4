using System;
using System.Collections.Generic;

namespace WinterProjectAPIV4.Models;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public int? UserGroupId { get; set; }

    public double? Amount { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual UserGroup? UserGroup { get; set; }
}
