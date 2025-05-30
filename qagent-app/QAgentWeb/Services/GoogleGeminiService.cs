using Microsoft.Extensions.Options;
using QAgentWeb.Models;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace QAgentWeb.Services
{
    public class GoogleGeminiService : IGoogleGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleGeminiService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";

        public GoogleGeminiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleGeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["GoogleGemini:ApiKey"] ?? throw new ArgumentNullException("GoogleGemini:ApiKey not configured");
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/models?key={_apiKey}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Google Gemini service availability");
                return false;
            }
        }

        public async Task<TextExtractionResult> ExtractTextFromImageAsync(string imagePath)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting text extraction with Google Gemini for: {ImagePath}", imagePath);

                var imageBase64 = await ConvertImageToBase64(imagePath);
                var prompt = @"Phân tích ảnh này và trích xuất tất cả văn bản có thể nhìn thấy. 
                Hãy liệt kê từng đoạn text theo thứ tự từ trên xuống dưới, từ trái sang phải.
                Chỉ trả về văn bản thuần túy, không thêm giải thích.";

                var request = CreateVisionRequest(prompt, imageBase64);
                var response = await CallGeminiAPI(request);

                if (response.Success && !string.IsNullOrEmpty(response.Content))
                {
                    stopwatch.Stop();
                    return new TextExtractionResult
                    {
                        Success = true,
                        ExtractedText = response.Content,
                        ConfidenceScore = 0.9,
                        ProcessingTime = stopwatch.Elapsed
                    };
                }

                stopwatch.Stop();
                return new TextExtractionResult
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage ?? "No text extracted",
                    ProcessingTime = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error extracting text from image: {ImagePath}", imagePath);
                return new TextExtractionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<ElementDetectionResult> AnalyzeUIElementsAsync(string imagePath, string? businessDescription = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting UI element analysis with Google Gemini for: {ImagePath}", imagePath);

                var imageBase64 = await ConvertImageToBase64(imagePath);
                var prompt = $@"Phân tích giao diện người dùng trong ảnh này và nhận diện các thành phần UI.

                {(string.IsNullOrEmpty(businessDescription) ? "" : $"Ngữ cảnh nghiệp vụ: {businessDescription}")}

                Hãy trả về kết quả dạng JSON với format sau:
                {{
                  ""screen_type"": ""form|grid|search|dashboard|workflow|report"",
                  ""confidence_score"": 0.0-1.0,
                  ""ui_elements"": [
                    {{
                      ""element_id"": ""unique_id"",
                      ""element_type"": ""input|button|dropdown|checkbox|radio|table|label|image"",
                      ""element_name"": ""tên thành phần"",
                      ""element_label"": ""nhãn hiển thị"",
                      ""is_required"": true/false,
                      ""position"": {{""x"": 0, ""y"": 0, ""width"": 0, ""height"": 0}}
                    }}
                  ],
                  ""business_functions"": [
                    {{
                      ""function_name"": ""tên chức năng"",
                      ""function_type"": ""create|read|update|delete|search|export"",
                      ""description"": ""mô tả chức năng""
                    }}
                  ]
                }}

                Chỉ trả về JSON, không thêm giải thích.";

                var request = CreateVisionRequest(prompt, imageBase64);
                var response = await CallGeminiAPI(request);

                if (response.Success && !string.IsNullOrEmpty(response.Content))
                {
                    var analysisResult = ParseAnalysisResult(response.Content);
                    stopwatch.Stop();
                    
                    return new ElementDetectionResult
                    {
                        Success = true,
                        DetectedElements = analysisResult.UIElements,
                        ConfidenceScore = analysisResult.ConfidenceScore,
                        ScreenType = analysisResult.ScreenType,
                        ProcessingTime = stopwatch.Elapsed
                    };
                }

                stopwatch.Stop();
                return new ElementDetectionResult
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage ?? "Failed to analyze UI elements",
                    ProcessingTime = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error analyzing UI elements: {ImagePath}", imagePath);
                return new ElementDetectionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<string> GenerateScreenDescriptionAsync(string imagePath, string? context = null)
        {
            try
            {
                var imageBase64 = await ConvertImageToBase64(imagePath);
                var prompt = $@"Mô tả ngắn gọn chức năng chính của giao diện này.
                {(string.IsNullOrEmpty(context) ? "" : $"Bối cảnh: {context}")}
                
                Trả về mô tả 1-2 câu, tập trung vào mục đích sử dụng của màn hình.";

                var request = CreateVisionRequest(prompt, imageBase64);
                var response = await CallGeminiAPI(request);

                return response.Success ? response.Content : "Không thể tạo mô tả cho màn hình này.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating screen description: {ImagePath}", imagePath);
                return "Lỗi khi tạo mô tả màn hình.";
            }
        }

        public async Task<List<BusinessFunction>> InferBusinessFunctionsAsync(string imagePath, List<UIElement> elements, string? description = null)
        {
            try
            {
                var elementsJson = JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true });
                var imageBase64 = await ConvertImageToBase64(imagePath);
                
                var prompt = $@"Dựa vào ảnh giao diện và danh sách UI elements, hãy suy luận các chức năng nghiệp vụ:

                UI Elements đã phát hiện:
                {elementsJson}

                {(string.IsNullOrEmpty(description) ? "" : $"Mô tả bổ sung: {description}")}

                Trả về JSON array các business functions:
                [
                  {{
                    ""function_name"": ""tên chức năng"",
                    ""function_type"": ""create|read|update|delete|search|export"",
                    ""description"": ""mô tả chi tiết chức năng"",
                    ""related_elements"": [""element_id1"", ""element_id2""]
                  }}
                ]";

                var request = CreateVisionRequest(prompt, imageBase64);
                var response = await CallGeminiAPI(request);

                if (response.Success)
                {
                    var functions = JsonSerializer.Deserialize<List<BusinessFunction>>(response.Content);
                    return functions ?? new List<BusinessFunction>();
                }

                return new List<BusinessFunction>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inferring business functions: {ImagePath}", imagePath);
                return new List<BusinessFunction>();
            }
        }

        private async Task<string> ConvertImageToBase64(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
            {
                var imageData = await _httpClient.GetByteArrayAsync(imagePath);
                return Convert.ToBase64String(imageData);
            }
            else
            {
                var fullPath = Path.IsPathRooted(imagePath) ? imagePath : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
                var imageData = await File.ReadAllBytesAsync(fullPath);
                return Convert.ToBase64String(imageData);
            }
        }

        private object CreateVisionRequest(string prompt, string imageBase64)
        {
            return new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = imageBase64
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    topK = 32,
                    topP = 1,
                    maxOutputTokens = 8192
                }
            };
        }

        private async Task<GeminiResponse> CallGeminiAPI(object request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/models/gemini-pro-vision:generateContent?key={_apiKey}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (geminiResponse.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var firstCandidate = candidates[0];
                        if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                            contentElement.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            var firstPart = parts[0];
                            if (firstPart.TryGetProperty("text", out var textElement))
                            {
                                return new GeminiResponse { Success = true, Content = textElement.GetString() ?? "" };
                            }
                        }
                    }
                    
                    return new GeminiResponse { Success = false, ErrorMessage = "Invalid response format" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new GeminiResponse { Success = false, ErrorMessage = $"API call failed: {response.StatusCode} - {errorContent}" };
                }
            }
            catch (Exception ex)
            {
                return new GeminiResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        private GeminiAnalysisResult ParseAnalysisResult(string jsonContent)
        {
            try
            {
                // Clean up JSON content (remove markdown formatting if present)
                var cleanJson = jsonContent.Trim();
                if (cleanJson.StartsWith("```json"))
                {
                    cleanJson = cleanJson.Substring(7);
                }
                if (cleanJson.EndsWith("```"))
                {
                    cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
                }

                var result = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                
                var analysisResult = new GeminiAnalysisResult
                {
                    Success = true,
                    ScreenType = result.TryGetProperty("screen_type", out var screenType) ? screenType.GetString() ?? "unknown" : "unknown",
                    ConfidenceScore = result.TryGetProperty("confidence_score", out var confidence) ? confidence.GetDouble() : 0.8
                };

                // Parse UI Elements
                if (result.TryGetProperty("ui_elements", out var uiElements))
                {
                    foreach (var element in uiElements.EnumerateArray())
                    {
                        var uiElement = new UIElement
                        {
                            ElementId = element.TryGetProperty("element_id", out var id) ? id.GetString() ?? "" : Guid.NewGuid().ToString(),
                            ElementType = element.TryGetProperty("element_type", out var type) ? type.GetString() ?? "unknown" : "unknown",
                            ElementName = element.TryGetProperty("element_name", out var name) ? name.GetString() ?? "" : "",
                            ElementLabel = element.TryGetProperty("element_label", out var label) ? label.GetString() ?? "" : "",
                            IsRequired = element.TryGetProperty("is_required", out var required) && required.GetBoolean()
                        };

                        if (element.TryGetProperty("position", out var position))
                        {
                            uiElement.Position = new ElementPosition
                            {
                                X = position.TryGetProperty("x", out var x) ? x.GetDouble() : 0,
                                Y = position.TryGetProperty("y", out var y) ? y.GetDouble() : 0,
                                Width = position.TryGetProperty("width", out var width) ? width.GetDouble() : 0,
                                Height = position.TryGetProperty("height", out var height) ? height.GetDouble() : 0
                            };
                        }

                        analysisResult.UIElements.Add(uiElement);
                    }
                }

                // Parse Business Functions
                if (result.TryGetProperty("business_functions", out var businessFunctions))
                {
                    foreach (var function in businessFunctions.EnumerateArray())
                    {
                        var businessFunction = new BusinessFunction
                        {
                            FunctionName = function.TryGetProperty("function_name", out var funcName) ? funcName.GetString() ?? "" : "",
                            FunctionType = function.TryGetProperty("function_type", out var funcType) ? funcType.GetString() ?? "read" : "read",
                            Description = function.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : ""
                        };

                        analysisResult.BusinessFunctions.Add(businessFunction);
                    }
                }

                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini analysis result: {Content}", jsonContent);
                return new GeminiAnalysisResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to parse analysis result: {ex.Message}"
                };
            }
        }

        private class GeminiResponse
        {
            public bool Success { get; set; }
            public string Content { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
} 