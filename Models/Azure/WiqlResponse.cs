using System.Text.Json.Serialization;

namespace doc_easy.Models.Azure;

public class WiqlResponse
{
    [JsonPropertyName("workItems")]
    public WorkItemReference[] WorkItems { get; set; }
}

public class WorkItemReference
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
