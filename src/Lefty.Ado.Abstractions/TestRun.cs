namespace Lefty.Ado.Model;

/// <summary>
/// A test run: one execution pass of some test points.
/// </summary>
public record TestRun
{
    /// <summary />
    public required int Id { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public required bool IsAutomated { get; set; }

    /// <summary />
    public required string State { get; set; }

    /// <summary />
    public DateTime? MomentStarted { get; set; }

    /// <summary />
    public DateTime? MomentCompleted { get; set; }

    /// <summary />
    public required User Owner { get; set; }

    /// <summary />
    public required int TotalTests { get; set; }

    /// <summary />
    public required int PassedTests { get; set; }

    /// <summary />
    public required IReadOnlyList<TestResult> Results { get; set; }
}
