namespace Lefty.Ado.Model;

/// <summary>
/// Iteration / Sprint.
/// </summary>
public record Iteration
{
    /// <summary>
    /// ADO's stable identifier for the iteration (classification node <c>identifier</c>).
    /// For an iteration no longer in the project's iteration tree (renamed or
    /// deleted, or the root), an identifier derived from its path instead.
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