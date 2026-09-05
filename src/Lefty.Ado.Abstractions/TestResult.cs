namespace Lefty.Ado.Model;

/// <summary>
/// The result of one test case's execution within a test run.
/// </summary>
public record TestResult
{
    /// <summary />
    public required int Id { get; set; }

    /// <summary />
    public required int TestCaseWorkItemId { get; set; }

    /// <summary />
    public required string TestCaseTitle { get; set; }

    /// <summary />
    public required string Outcome { get; set; }

    /// <summary />
    public required string State { get; set; }

    /// <summary />
    public DateTime? MomentStarted { get; set; }

    /// <summary />
    public DateTime? MomentCompleted { get; set; }

    /// <summary />
    public double? DurationMs { get; set; }

    /// <summary />
    public User? RunBy { get; set; }

    /// <summary />
    public string? ErrorMessage { get; set; }

    /// <summary />
    public string? Comment { get; set; }

    /// <summary />
    public int? TestPointId { get; set; }
}