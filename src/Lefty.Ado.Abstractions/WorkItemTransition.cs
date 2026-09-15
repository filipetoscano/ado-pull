using System.Text.Json.Serialization;

namespace Lefty.Ado.Model;

/// <summary />
public class WorkItemTransition
{
    /// <summary>
    /// Previous state, or null when the transition is the work item's creation
    /// (into its initial state).
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public required string? From { get; set; }

    /// <summary />
    public required string To { get; set; }

    /// <summary />
    public required User By { get; set; }

    /// <summary />
    public required DateTime Moment { get; set; }
}