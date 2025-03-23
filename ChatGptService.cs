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
        "Você é um assistente especializado em análise de requisitos e geração de documentação técnica. " +
        "Sua tarefa é transformar requisitos de software em documentação detalhada e estruturada em Markdown, " +
        "adequada para ser publicada no Docusaurus.\n\nA documentação gerada deve conter:\n- **Título do deliverable**" +
        "\n- **Descrição detalhada** do requisito\n- **Objetivo e impacto no sistema**\n- **Pré-requisitos ou dependências**, " +
        "caso existam\n- **Critérios de aceitação**, se aplicáveis\n- **Fluxo de funcionamento esperado**\n- **Exemplo de uso**, " +
        "se relevante\n- **Possíveis erros e como evitá-los**\n\n**Formato Markdown:**\n- Use `#` para títulos, `##` para subtítulos e " +
        "`###` para seções internas.\n- Utilize listas e tabelas sempre que possível.\n- O resultado deve ser um documento pronto para ser salvo como " +
        "`.md` e publicado no Docusaurus.\n\nMantenha a documentação **clara, objetiva e bem estruturada**, " +
        "garantindo que seja útil para desenvolvedores e stakeholders.";

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