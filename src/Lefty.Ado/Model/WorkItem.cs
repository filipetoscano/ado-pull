namespace Lefty.Ado.Model;

/// <summary />
public record WorkItem
{
    /// <summary />
    public int Id { get; set; }

    /// <summary />
    public required string Title { get; set; }

    /// <summary />
    public required string Description { get; set; }

    /// <summary />
    public required string State { get; set; }

    /// <summary />
    public required User CreatedBy { get; set; }

    /// <summary />
    public required DateTime MomentCreated { get; set; }

    /// <summary />
    public required DateTime MomentActivity { get; set; }

    /// <summary />
    public required User? AssignedTo { get; set; }

    /// <summary />
    public required IReadOnlyList<string> Tags { get; set; }

    /// <summary />
    public required Iteration? Iteration { get; set; }


    /// <summary />
    public required string? IssueType { get; set; }

    /// <summary />
    public required string? Component { get; set; }

    /// <summary />
    public required string? Severity { get; set; }


    /// <summary />
    public required IReadOnlyList<WorkItemTransition> Transitions { get; set; }

    /// <summary />
    public required IReadOnlyList<WorkItemRemark> Remarks { get; set; }
}