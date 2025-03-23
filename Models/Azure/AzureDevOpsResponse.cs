using System.Text.Json.Serialization;

namespace doc_easy.Models.Azure;

public class AzureDevOpsResponse
{
    [JsonPropertyName("count")]
    public int Contador { get; set; }
    
    [JsonPropertyName("value")]
    public List<AzureDevOpsWorkItem> Value { get; set; }
}

public class AzureDevOpsWorkItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fields")]
    public AzureDevOpsCampos Campos { get; set; }

    [JsonPropertyName("relations")]
    public List<AzureDevOpsRelacionamentos> Relacionamentos { get; set; }
}

public class AzureDevOpsCampos
{
    [JsonPropertyName("System.Title")]
    public string Titulo { get; set; }

    [JsonPropertyName("System.Description")]
    public string Descricao { get; set; }
    
    [JsonPropertyName("System.BoardColumnDone")]
    public bool ColunaFechado { get; set; }
    
    [JsonPropertyName("System.WorkItemType")]
    public string TipoWorkItem { get; set; }
}

public class AzureDevOpsRelacionamentos
{
    [JsonPropertyName("rel")]
    public string Relacionamento { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}