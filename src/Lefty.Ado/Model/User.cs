namespace Lefty.Ado.Model;

/// <summary />
public record User
{
    /// <summary />
    public required string DisplayName { get; init; }

    /// <summary />
    public required string Upn { get; init; }
}