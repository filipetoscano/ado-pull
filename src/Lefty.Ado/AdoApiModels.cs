using System.Text.Json;

namespace Lefty.Ado;

// Internal DTOs mirroring the Azure DevOps REST API JSON shapes. These are
// intentionally separate from Lefty.Ado.Model, which is the library's public,
// ADO-agnostic surface.

internal sealed class IdentityRefDto
{
    public string? DisplayName { get; set; }
    public string? UniqueName { get; set; }
}


internal sealed class ClassificationNodeAttributesDto
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? FinishDate { get; set; }
}


internal sealed class ClassificationNodeDto
{
    public string Name { get; set; } = "";
    public ClassificationNodeAttributesDto? Attributes { get; set; }
    public List<ClassificationNodeDto>? Children { get; set; }
}


internal sealed class WiqlWorkItemRefDto
{
    public int Id { get; set; }
}


internal sealed class WiqlResultDto
{
    public List<WiqlWorkItemRefDto> WorkItems { get; set; } = new();
}


internal sealed class WorkItemBatchItemDto
{
    public int Id { get; set; }
    public Dictionary<string, JsonElement> Fields { get; set; } = new();
}


internal sealed class WorkItemBatchResultDto
{
    public List<WorkItemBatchItemDto> Value { get; set; } = new();
}


internal sealed class WorkItemFieldChangeDto
{
    public JsonElement? OldValue { get; set; }
    public JsonElement? NewValue { get; set; }
}


internal sealed class WorkItemUpdateDto
{
    public IdentityRefDto? RevisedBy { get; set; }
    public DateTimeOffset? RevisedDate { get; set; }
    public Dictionary<string, WorkItemFieldChangeDto>? Fields { get; set; }
}


internal sealed class WorkItemUpdatesResultDto
{
    public List<WorkItemUpdateDto> Value { get; set; } = new();
}


internal sealed class WorkItemCommentDto
{
    public string Text { get; set; } = "";
    public IdentityRefDto? CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
}


internal sealed class WorkItemCommentsResultDto
{
    public List<WorkItemCommentDto> Comments { get; set; } = new();
}
