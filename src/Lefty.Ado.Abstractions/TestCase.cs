namespace Lefty.Ado.Model;

/// <summary>
/// A test case's membership within a suite. The test case itself is a "Test Case" work item.
/// </summary>
public record TestCase
{
    /// <summary />
    public required int WorkItemId { get; set; }

    /// <summary />
    public required string Title { get; set; }

    /// <summary />
    public required IReadOnlyList<TestPoint> Points { get; set; }
}