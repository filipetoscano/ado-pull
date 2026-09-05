using System.Text.Json;

namespace Lefty.Ado;

// Internal DTOs mirroring the Azure DevOps REST API JSON shapes. These are
// intentionally separate from Lefty.Ado.Model, which is the library's public,
// ADO-agnostic surface.

internal sealed class IdentityRefDto
{
    public Guid Id { get; set; }
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
    public Guid Identifier { get; set; }
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


internal sealed class TestSuiteReferenceDto
{
    public int Id { get; set; }
}


internal sealed class TestSuiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string SuiteType { get; set; } = "";
    public TestSuiteReferenceDto? ParentSuite { get; set; }
}


internal sealed class TestSuiteListResultDto
{
    public List<TestSuiteDto> Value { get; set; } = new();
}


internal sealed class TestPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? AreaPath { get; set; }
    public string? Iteration { get; set; }
    public string State { get; set; } = "";
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public IdentityRefDto? Owner { get; set; }
}


internal sealed class TestPlanListResultDto
{
    public List<TestPlanDto> Value { get; set; } = new();
}


internal sealed class TestPointConfigurationRefDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}


internal sealed class TestPointRunRefDto
{
    public string? Id { get; set; }
}


internal sealed class TestPointResultRefDto
{
    public string? Id { get; set; }
}


internal sealed class TestPointTestCaseRefDto
{
    public string Id { get; set; } = "";
}


internal sealed class WorkItemPropertyValueDto
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}


internal sealed class WorkItemPropertyDto
{
    public WorkItemPropertyValueDto? WorkItem { get; set; }
}


internal sealed class TestPointDto
{
    public int Id { get; set; }
    public IdentityRefDto? AssignedTo { get; set; }
    public TestPointConfigurationRefDto? Configuration { get; set; }
    public string? Outcome { get; set; }
    public TestPointRunRefDto? LastTestRun { get; set; }
    public TestPointResultRefDto? LastResult { get; set; }
    public TestPointTestCaseRefDto TestCase { get; set; } = new();
    public List<WorkItemPropertyDto>? WorkItemProperties { get; set; }
}


internal sealed class TestPointListResultDto
{
    public List<TestPointDto> Value { get; set; } = new();
}