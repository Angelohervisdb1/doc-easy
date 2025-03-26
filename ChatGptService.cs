using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using doc_easy.Models.ChatGpt;

namespace doc_easy;

public class ChatGptService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://api.aimlapi.com/v1/chat/completions";

    private const string InstrucaoSistema =
        "Você é um assistente especializado em análise de requisitos e geração de documentação técnica para software. " +
        "Sua tarefa é transformar requisitos extraídos do Azure DevOps em documentação detalhada e estruturada, " +
        "formatada para publicação no **Docusaurus**.\n\n" +
        "**Diretrizes:**\n" +
        "- Use **Markdown** (`#` para títulos, `##` para subtítulos, `###` para seções internas).\n" +
        "- Utilize `<details><summary>` para seções colapsáveis quando necessário.\n" +
        "- Apresente **tabelas bem formatadas** para critérios de aceitação e dependências.\n" +
        "- Mantenha a estrutura hierárquica entre deliverables e subdeliverables.\n\n" +
        "**Seções esperadas:**\n" +
        "1. **Título do deliverable**\n" +
        "2. **Descrição detalhada**\n" +
        "3. **Objetivo e impacto no sistema**\n" +
        "4. **Pré-requisitos ou dependências**\n" +
        "5. **Critérios de aceitação**\n" +
        "6. **Fluxo esperado**\n\n" +
        "**Exemplo de Formatação:**\n" +
        "```md\n" +
        "# Título do Deliverable\n" +
        "## Descrição\n" +
        "Aqui está a descrição detalhada...\n\n" +
        "<details>\n" +
        "<summary>Critérios de Aceitação</summary>\n\n" +
        "| Critério  | Descrição |\n" +
        "|-----------|------------|\n" +
        "| 1  | O sistema deve validar X antes de permitir Y |\n" +
        "</details>\n\n" +
        "## Fluxo Esperado\n" +
        "1. O usuário faz isso.\n" +
        "2. O sistema responde assim.\n" +
        "```\n" +
        "Mantenha a documentação **clara, objetiva e bem estruturada** para facilitar o uso por desenvolvedores e stakeholders.";
    
    public ChatGptService(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string> AnalisarRequisitosAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = InstrucaoSistema },
                new { role = "user", content = prompt }
            },
            temperature = 0.4
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro na API do ChatGPT: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var parsedResponse = JsonSerializer.Deserialize<ChatGptResponse>(jsonResponse);

        return parsedResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "Erro ao processar resposta.";
    }
}