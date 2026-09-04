namespace Lefty.Ado;

/// <summary />
public class AdoServiceOptions
{
    /// <summary />
    public required string Organization { get; set; }

    /// <summary />
    public required string DefaultProject { get; set; }

    /// <summary />
    public required string PersonalAccessToken { get; set; }
}