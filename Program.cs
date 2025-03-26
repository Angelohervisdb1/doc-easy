using System.Text.Json;

namespace doc_easy;

public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static async Task Main()
    {
        Console.WriteLine("🔍 Iniciando geração de documentação...");

        Console.WriteLine("ℹ️ Informe o contexto do projeto:");
        var contextoProjeto = Console.ReadLine();

        Console.WriteLine("ℹ️ Informe o nome da organização do Azure DevOps:");
        var nomeOrganizacao = Console.ReadLine();
        // angelohervis

        Console.WriteLine("ℹ️ Informe o nome do projeto do Azure DevOps:");
        var nomeProjeto = Console.ReadLine();
        // Delivery

        Console.WriteLine("ℹ️ Informe o token de acesso ao Azure DevOps:");
        var tokenAzure = Console.ReadLine();
        // 4VkUbwqgbcd16rCRmVe2MEjJHN58R7Sr6i5fyY4yi9NxSDCH83MzJQQJ99BCACAAAAAAAAAAAAASAZDO3g5u

        Console.WriteLine("ℹ️ Informe o token de acesso ao ChatGPT:");
        var tokenGpt = Console.ReadLine();
        // 7e21c30eddab4017a3d6befef943e880

        var azureDevOpsService = new AzureDevOpsService(nomeOrganizacao, nomeProjeto, tokenAzure);
        var deliverables = await azureDevOpsService.GetDeliverablesAsync();
        var chatGptService = new ChatGptService(tokenGpt);
        
        var docusaurusDocsPath = Path.Combine("my-docs", "docs");
        
        if (Directory.Exists(docusaurusDocsPath))
        {
            Console.WriteLine("🧹 Limpando diretório de documentação...");
            Directory.Delete(docusaurusDocsPath, true);
        }

        Directory.CreateDirectory(docusaurusDocsPath);

        foreach (var parent in deliverables)
        {
            var prompt = $"""
                              **Contexto do Projeto:**  
                              {contextoProjeto}  
                          
                              **Objetivo:**  
                              Com base no contexto acima, analise os requisitos extraídos do Azure DevOps e gere uma documentação clara e objetiva no formato Markdown, seguindo a estrutura abaixo:  
                          
                              - **Título do Deliverable**  
                              - **Descrição geral** (Explicação objetiva do propósito e escopo do item)  
                              - **Objetivo e impacto no sistema**  
                              - **Pré-requisitos e dependências**  
                              - **Critérios de aceitação** (em formato de tabela Markdown)  
                              - **Fluxo esperado** (passo a passo claro)  
                          
                              **Formato:** Use `#` para títulos e `##` para subtítulos. Utilize `<details><summary>` para expandir critérios de aceitação.  
                          
                              **Requisitos extraídos do Azure DevOps:**  
                          
                              ```json
                              {JsonSerializer.Serialize(parent, JsonOptions)}
                              ```
                          
                              Gere a documentação de forma objetiva e técnica, sem adicionar informações fictícias, exemplos de código ou explicações desnecessárias.
                          """;

            var documentation = await chatGptService.AnalisarRequisitosAsync(prompt);
            var fileName = $"{parent.IdDeliverable}-{SanitizeFileName(parent.Title)}.mdx";

            var mdxContent = $"""
                              ---
                              id: {parent.IdDeliverable}
                              title: "{parent.Title}"
                              sidebar_label: "{parent.Title}"
                              ---

                              {documentation}
                              """;
            
            Task.Delay(5000).Wait();

            var filePath = Path.Combine(docusaurusDocsPath, fileName);
            await File.WriteAllTextAsync(filePath, mdxContent);
        }

        Console.WriteLine("✅ Documentação gerada com sucesso!");
        Console.WriteLine($"📂 Arquivos salvos em: {docusaurusDocsPath}");
        Console.WriteLine("🚀 Para visualizar, inicie o Docusaurus com `npm run start` dentro do diretório `my-docs`.");
    }

    private static string SanitizeFileName(string title)
    {
        title = Path.GetInvalidFileNameChars().Aggregate(title, (current, c) => current.Replace(c, '-'));
        return title.ToLower().Replace(" ", "-");
    }
}