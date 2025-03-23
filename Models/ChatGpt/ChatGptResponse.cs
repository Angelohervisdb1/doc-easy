using System.Text.Json.Serialization;

namespace doc_easy.Models.ChatGpt;

public class ChatGptResponse
{
    [JsonPropertyName("choices")]
    public List<ChatGptChoice> Choices { get; set; }
}

public class ChatGptChoice
{
    [JsonPropertyName("message")]
    public ChatGptMessage Message { get; set; }
}

public class ChatGptMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
}