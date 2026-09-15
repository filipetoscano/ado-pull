namespace Lefty.Ado.Model;

/// <summary />
public class WorkItemIteration
{
    /// <summary />
    public required Iteration? From { get; set; }

    /// <summary />
    public required Iteration? To { get; set; }

    /// <summary />
    public required User By { get; set; }

    /// <summary />
    public required DateTime Moment { get; set; }
}