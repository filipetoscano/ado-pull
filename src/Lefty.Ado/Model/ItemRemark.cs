namespace Lefty.Ado.Model;

/// <summary />
public class ItemRemark
{
    /// <summary />
    public required string Text { get; set; }

    /// <summary />
    public required User By { get; set; }

    /// <summary />
    public required DateTime Moment { get; set; }
}