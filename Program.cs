using System.Text.Json;

namespace doc_easy;

public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private const string DocsPath = "C:/Users/angelo.hervis/Desktop/doc-easy/doc_easy/docs";

    private static async Task Main()
    {
        Console.WriteLine("🔍 Iniciando geração de documentação...");
        Console.WriteLine("ℹ️ Informe o nome da organização do Azure DevOps:");
        var nomeOrganizacao = Console.ReadLine();
        
        Console.WriteLine("ℹ️ Informe o nome do projeto do Azure DevOps:");
        var nomeProjeto = Console.ReadLine();
        
        Console.WriteLine("ℹ️ Informe o token de acesso ao Azure DevOps:");
        var tokenAzure = Console.ReadLine();
        
        Console.WriteLine("ℹ️ Informe o token de acesso ao ChatGPT:");
        var tokenGpt = Console.ReadLine();
        
        var azureDevOpsService = new AzureDevOpsService(nomeOrganizacao, nomeProjeto, tokenAzure);
        var deliverables = await azureDevOpsService.GetDeliverablesAsync();
        var chatGptService = new ChatGptService(tokenGpt);
        
//         if (!Directory.Exists(DocsPath))
//             Directory.CreateDirectory(DocsPath);
//         
//         Console.WriteLine("🔍 Iniciando geração de documentação...");
//         
//
//         var azureDevOpsService = new AzureDevOpsService("angelohervis", "Delivery", "4VkUbwqgbcd16rCRmVe2MEjJHN58R7Sr6i5fyY4yi9NxSDCH83MzJQQJ99BCACAAAAAAAAAAAAASAZDO3g5u");
//         var deliverables = await azureDevOpsService.GetDeliverablesAsync();
//         var chatGptService = new ChatGptService("7e21c30eddab4017a3d6befef943e880");
//
         foreach (var parent in deliverables)
         {
             var prompt = $"""
                               Analise os seguintes requisitos do projeto e gere uma documentação detalhada e formatada em Markdown para publicação no Docusaurus.  
                               Cada deliverable pode conter subdeliverables (children). Estruture a documentação de forma hierárquica, detalhando cada item conforme necessário.  
                               Seguem os dados extraídos do Azure DevOps:
                           
                               ```json
                               {JsonSerializer.Serialize(parent, JsonOptions)}
                               ```
                           
                               Gere uma documentação completa baseada nesses dados.
                           """;

             var documentation = await chatGptService.AnalisarRequisitosAsync(prompt);

             var fileName = $"{parent.IdDeliverable}-{SanitizeFileName(parent.Title)}.mdx";
             var filePath = Path.Combine(DocsPath, fileName);

             var mdxContent = $"""
                               ---
                               id: {parent.IdDeliverable}
                               title: "{parent.Title}"
                               sidebar_label: "{parent.Title}"
                               ---

                               {documentation}
                               """;

             await File.WriteAllTextAsync(filePath, mdxContent);
         }
        
        Console.WriteLine("📜 Criando commit...");
        var files = Directory.GetFiles(DocsPath).ToList();
        var urlAzure = "https://dev.azure.com/angelohervis/Delivery/_apis/git/repositories/Delivery/pullrequests?api-version=7.1";
        // await GitService.CommitAndCreatePrAsync(files, tokenAzure,
        //     $"https://dev.azure.com/{nomeOrganizacao}/{nomeProjeto}/_apis/git/repositories/{nomeProjeto}/pullrequests?api-version=7.1");
        
        await GitService.CommitAndCreatePrAsync(files, "4VkUbwqgbcd16rCRmVe2MEjJHN58R7Sr6i5fyY4yi9NxSDCH83MzJQQJ99BCACAAAAAAAAAAAAASAZDO3g5u", urlAzure);
    }
    
    private static string SanitizeFileName(string title)
    {
        title = Path.GetInvalidFileNameChars().Aggregate(title, (current, c) => current.Replace(c, '-'));
        return title.ToLower().Replace(" ", "-");
    }
}