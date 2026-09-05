namespace Lefty.Ado.Model;

/// <summary />
public record User
{
    /// <summary>
    /// ADO's stable identifier for the identity (IdentityRef <c>id</c>).
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary />
    public required string DisplayName { get; init; }

    /// <summary />
    public required string Upn { get; init; }
}