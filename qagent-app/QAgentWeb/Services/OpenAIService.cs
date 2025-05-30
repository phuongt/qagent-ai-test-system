using OpenAI;
using OpenAI.Chat;

namespace QAgentWeb.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<OpenAIService> _logger;
        private readonly IConfiguration _configuration;

        public OpenAIService(ILogger<OpenAIService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("OpenAI API key not configured");
            }
            
            _client = new OpenAIClient(apiKey);
        }

        public async Task<string> GenerateTextAsync(string prompt, int maxTokens = 1000)
        {
            try
            {
                var chatClient = _client.GetChatClient("gpt-3.5-turbo");
                var completion = await chatClient.CompleteChatAsync(prompt);
                
                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with OpenAI");
                throw;
            }
        }

        public async Task<string> ChatCompletionAsync(string message, string? systemMessage = null)
        {
            try
            {
                var chatClient = _client.GetChatClient("gpt-3.5-turbo");
                var messages = new List<ChatMessage>();
                
                if (!string.IsNullOrEmpty(systemMessage))
                {
                    messages.Add(ChatMessage.CreateSystemMessage(systemMessage));
                }
                
                messages.Add(ChatMessage.CreateUserMessage(message));
                
                var completion = await chatClient.CompleteChatAsync(messages);
                
                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat completion with OpenAI");
                throw;
            }
        }

        public async Task<OpenAIResponse> AnalyzeImageAsync(string imageUrl, string prompt)
        {
            try
            {
                // For demo purposes, return mock analysis
                _logger.LogInformation("Mock image analysis for: {ImageUrl}", imageUrl);
                
                var mockResponse = @"[
                    {
                        ""type"": ""input"",
                        ""text"": ""Tên người dùng"",
                        ""position"": {""x"": 100, ""y"": 150, ""width"": 200, ""height"": 30}
                    },
                    {
                        ""type"": ""button"",
                        ""text"": ""Đăng nhập"",
                        ""position"": {""x"": 150, ""y"": 200, ""width"": 100, ""height"": 35}
                    }
                ]";

                return new OpenAIResponse
                {
                    Success = true,
                    Content = mockResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image with OpenAI");
                return new OpenAIResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OpenAIResponse> GenerateTextAsync(string prompt)
        {
            try
            {
                var chatClient = _client.GetChatClient("gpt-3.5-turbo");
                var completion = await chatClient.CompleteChatAsync(prompt);
                
                return new OpenAIResponse
                {
                    Success = true,
                    Content = completion.Value.Content[0].Text
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with OpenAI");
                return new OpenAIResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                // For demo purposes, always return true
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI service is not available");
                return false;
            }
        }
    }
} 