
using EmergencyHelp.Configuration;
using EmergencyHelp.Models.Entities;
using EmergencyHelp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;


namespace EmergencyHelp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly ILogger<OpenAIController> _logger;
        private readonly IOpenAiService _openAiService;
        private readonly OpenAi openAi;

        public OpenAIController(ILogger<OpenAIController> logger, IOpenAiService openAiService, IOptionsMonitor<OpenAi> optionsMonitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
            openAi = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        [HttpPost]
        [Route("CompleteSentence")]
        public async Task<IActionResult> CompleteSentence([FromBody] CompleteSentenceRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Prompt))
            {
                return BadRequest("Prompt cannot be null or empty.");
            }

            try
            {
                var result = await _openAiService.CompleteSentence(request.Prompt);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sentence");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        [Route("audio-to-text")]
        public async Task<IActionResult> AudioToText(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest("Audio file is required.");
            }

            try
            {
                var openAiKey = openAi.ApiKey;

                using var content = new MultipartFormDataContent();
                using var stream = audio.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");

                content.Add(streamContent, "file", audio.FileName);
                content.Add(new StringContent("whisper-1"), "model");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiKey);

                var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error from OpenAI: {error}");
                    return StatusCode((int)response.StatusCode, "Error transcribing audio");
                }

                var result = await response.Content.ReadAsStringAsync();
                string prompt = "Given the audio clip (Based on the given conversation generate a the following enitites with different font for the heading of each entity. Each category should be in a new line and single line width to the next heading without causing clustering of data), Name, Location, Contact Number, Description of Case, severity (If the person called is not stable make sevearity as high else Low. If the audio clip doesn't have the required parameters to process the given query output: THE GIVEN AUDIO DOESNT CONTAIN ANY CONVERSTION WHERE ANY REQUIRED INFORMATION COULD BE EXTRACTED), Description of the Offender if any";
                string query = prompt + "\n" + result;
                var answer = await GetEmergencyInfo(query);
                return Ok(answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing audio");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private async Task<string> GetEmergencyInfo(string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(request));
            }

            try
            {
                var result = await _openAiService.CompleteSentence(request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sentence");
                throw;
            }
        }
    }
}
