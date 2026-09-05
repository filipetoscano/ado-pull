namespace Lefty.Ado.Model;

/// <summary>
/// Test plan.
/// </summary>
public record TestPlan
{
    /// <summary />
    public required int Id { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary />
    public string? AreaPath { get; set; }

    /// <summary />
    public string? Iteration { get; set; }

    /// <summary />
    public required string State { get; set; }

    /// <summary />
    public DateOnly? DateStart { get; set; }

    /// <summary />
    public DateOnly? DateEnd { get; set; }

    /// <summary />
    public required User Owner { get; set; }

    /// <summary>
    /// All suites in the plan's suite tree, flattened (see <see cref="TestSuite.ParentSuiteId" />).
    /// </summary>
    public required IReadOnlyList<TestSuite> Suites { get; set; }
}