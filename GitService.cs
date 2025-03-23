using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace doc_easy;

public static class GitService
{
    public static async Task CommitAndCreatePrAsync(List<string> files, string tokenAzure, string urlAzure)
    {
        Console.WriteLine("📂 Adicionando arquivos ao Git...");
        
        ExecuteGitCommand("checkout develop");
        ExecuteGitCommand("pull");
        
        Task.Delay(5000).Wait();

        var branchName = $"doc-update-{DateTime.Now:yyyyMMddHHmmss}";
        ExecuteGitCommand($"checkout -b {branchName}");
        
        Task.Delay(5000).Wait();
        
        CreateGitIgnore();

        ExecuteGitCommand("add -A");
        
        Task.Delay(5000).Wait();

        Console.WriteLine("📜 Criando commit...");
        ExecuteGitCommand("commit -m \"📄 Criando documentação automática\"");
        
        Task.Delay(5000).Wait();

        Console.WriteLine("⬆️ Subindo alterações...");
        ExecuteGitCommand($"push origin {branchName}");
        
        Task.Delay(5000).Wait();

        Console.WriteLine("🔀 Criando Pull Request...");
        await CreatePullRequestAsync(tokenAzure, urlAzure, branchName);
    }
    
    private static void CreateGitIgnore()
    {
        Console.WriteLine("📝 Criando .gitignore...");
        const string gitIgnoreContent = """
                                        node_modules/
                                        .build/
                                        .DS_Store
                                        .idea/
                                        .vscode/
                                        docusaurus/.cache/
                                        docusaurus/build/
                                        obj/
                                        bin/
                                        *.log
                                        *.lock
                                        *.user
                                        *.suo
                                        *.db
                                        .env
                                        .env.local
                                        """;
        
        File.WriteAllText(".gitignore", gitIgnoreContent);
    }

    private static void ExecuteGitCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
        {
            Console.WriteLine($"❌ Erro ao executar o comando Git: {error}");
            throw new InvalidOperationException($"Git command failed: {error}");
        }
        Console.WriteLine(output);
    }

    private static async Task CreatePullRequestAsync(string tokenAzure, string azureDevOpsUrl, string branchName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{tokenAzure}")));

        var prRequest = new
        {
            sourceRefName = $"refs/heads/{branchName}",
            targetRefName = "refs/heads/develop",
            title = $"{DateTime.Now.Date} - Criação da documentação automática",
            description = "Este PR inclui a documentação gerada."
        };

        var json = JsonSerializer.Serialize(prRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(azureDevOpsUrl, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("✅ Pull Request criado com sucesso!");
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Falha ao criar Pull Request: {errorResponse}");
        }
    }
}
