namespace Lefty.Ado.Model;

/// <summary />
public class ItemTransition
{
    /// <summary />
    public required string From { get; set; }

    /// <summary />
    public required string To { get; set; }

    /// <summary />
    public required User By { get; set; }

    /// <summary />
    public required DateTime Moment { get; set; }
}