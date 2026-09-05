namespace Lefty.Ado.Model;

/// <summary>
/// One (configuration, tester) assignment for a test case within a suite.
/// </summary>
public record TestPoint
{
    /// <summary />
    public required int Id { get; set; }

    /// <summary />
    public required string ConfigurationName { get; set; }

    /// <summary />
    public User? Tester { get; set; }
}
