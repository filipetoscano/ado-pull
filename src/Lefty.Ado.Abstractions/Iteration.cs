namespace Lefty.Ado.Model;

/// <summary>
/// Iteration / Sprint.
/// </summary>
public record Iteration
{
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