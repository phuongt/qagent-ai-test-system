namespace QAgentWeb.Services
{
    public interface IOpenAIService
    {
        Task<string> GenerateTextAsync(string prompt, int maxTokens = 1000);
        Task<string> ChatCompletionAsync(string message, string? systemMessage = null);
        Task<OpenAIResponse> AnalyzeImageAsync(string imageUrl, string prompt);
        Task<OpenAIResponse> GenerateTextAsync(string prompt);
        Task<bool> IsServiceAvailableAsync();
    }

    public class OpenAIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
} 