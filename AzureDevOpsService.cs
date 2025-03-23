using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using doc_easy.Models.Azure;

namespace doc_easy;

public class AzureDevOpsService
{
    private readonly string _organization;
    private readonly string _project;
    private readonly HttpClient _httpClient;

    public AzureDevOpsService(string organization, string project, string personalAccessToken)
    {
        _organization = organization;
        _project = project;
        _httpClient = new HttpClient();

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
    }

    public async Task<List<ParentDeliverable>> GetDeliverablesAsync()
    {
        var workItems = await ObterWorkItemsAsync();
        var detalhes = await ObterDetalhesWorkItemsAsync(workItems);

        return OrganizeDeliverables(detalhes);
    }

    private async Task<List<int>> ObterWorkItemsAsync()
    {
        var url = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/wiql?api-version=6.0";
        var query = new
        {
            query =
                $"SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.TeamProject] = '{_project}' ORDER BY [System.Id] ASC"
        };

        var requestBody = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WiqlResponse>(responseBody);

        return result.WorkItems.Select(wi => wi.Id).ToList();
    }

    private async Task<List<AzureDevOpsWorkItem>> ObterDetalhesWorkItemsAsync(List<int> ids)
    {
        var url =
            $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems?ids={string.Join(',', ids)}&api-version=7.1&$expand=relations";

        var response = await _httpClient.GetAsync(url);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AzureDevOpsResponse>(jsonResponse);

        return result?.Value ?? [];
    }

    private static List<ParentDeliverable> OrganizeDeliverables(List<AzureDevOpsWorkItem> workItems)
    {
        var parents = new Dictionary<int, ParentDeliverable>();

        foreach (var workItem in workItems)
        {
            var parentId = GetParentId(workItem, out var isChild);

            if (isChild)
            {
                AddChildDeliverable(parents, parentId, workItem);
            }
            else
            {
                AddParentDeliverable(parents, workItem);
            }
        }

        return parents.Values.ToList();
    }

    private static int GetParentId(AzureDevOpsWorkItem workItem, out bool isChild)
    {
        isChild = false;
        var parentId = 0;

        if (workItem.Relacionamentos == null) return parentId;
        foreach (var relation in workItem.Relacionamentos)
        {
            if (relation.Relacionamento != "System.LinkTypes.Hierarchy-Reverse") continue;
            parentId = int.TryParse(relation.Url.Split('/')[1], out var id) ? id : 0;
            isChild = true;
            break;
        }

        return parentId;
    }

    private static void AddChildDeliverable(Dictionary<int, ParentDeliverable> parents, int parentId,
        AzureDevOpsWorkItem workItem)
    {
        if (!parents.TryGetValue(parentId, out var value))
        {
            value = new ParentDeliverable
            {
                IdDeliverable = parentId,
                Title = $"[Pendente: {parentId}]",
                Children = []
            };
            parents[parentId] = value;
        }

        value.Children.Add(new Deliverable
        {
            Id = workItem.Id,
            Titulo = workItem.Campos.Titulo,
            Descricao = workItem.Campos.Descricao,
            ParentId = parentId.ToString(),
            Parent = false,
            Tipo = Enum.TryParse<TipoWorkItem>(workItem.Campos.TipoWorkItem, out var tipo)
                ? tipo
                : TipoWorkItem.NaoMapeado,
            Finalizado = workItem.Campos.ColunaFechado
        });
    }

    private static void AddParentDeliverable(Dictionary<int, ParentDeliverable> parents,
        AzureDevOpsWorkItem workItem)
    {
        if (!parents.ContainsKey(workItem.Id))
        {
            parents[workItem.Id] = new ParentDeliverable
            {
                IdDeliverable = workItem.Id,
                Title = workItem.Campos.Titulo,
                Children = []
            };
        }
    }
}