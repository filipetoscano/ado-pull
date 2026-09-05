namespace Lefty.Ado.Model;

/// <summary>
/// Test suite (static, dynamic, or requirement-based) within a test plan.
/// </summary>
public record TestSuite
{
    /// <summary />
    public required int Id { get; set; }

    /// <summary />
    public required string Name { get; set; }

    /// <summary>
    /// Raw ADO suite type: "staticTestSuite", "dynamicTestSuite", or "requirementTestSuite".
    /// </summary>
    public required string SuiteType { get; set; }

    /// <summary>
    /// Id of the parent suite, or null for the plan's root suite.
    /// </summary>
    public int? ParentSuiteId { get; set; }

    /// <summary />
    public required IReadOnlyList<TestCase> TestCases { get; set; }
}