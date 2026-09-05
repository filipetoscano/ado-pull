namespace Lefty.Ado.Model;

/// <summary>
/// Iteration / Sprint.
/// </summary>
public record Iteration
{
    /// <summary>
    /// ADO's stable identifier for the iteration (classification node <c>identifier</c>).
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Name of the iteration.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Date when the iteration starts.
    /// </summary>
    public DateOnly? DateStart { get; set; }

    /// <summary>
    /// Date when the iteration ends.
    /// </summary>
    public DateOnly? DateEnd { get; set; }
}