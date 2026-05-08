
using EmergencyHelp.Configuration;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;

namespace EmergencyHelp.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAi openAi;

        public OpenAiService(IOptionsMonitor<OpenAi> optionsMonitor)
        {
            openAi = optionsMonitor.CurrentValue;
        }

        public OpenAiService(OpenAi openAi)
        {
            this.openAi = openAi;
        }

        public async Task<string> CompleteSentence(string prompt)
        {
            var api = new OpenAI_API.OpenAIAPI(openAi.ApiKey);

            var chatRequest = new ChatRequest
            {
                Model = "gpt-3.5-turbo", 
                Messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatMessageRole.System, "You are an assistant."),
                    new ChatMessage(ChatMessageRole.User, prompt)
                }
            };
            var chatResponse = await api.Chat.CreateChatCompletionAsync(chatRequest);
            return chatResponse.Choices.FirstOrDefault()?.Message.Content ?? "No response generated.";
            //var result = await api.Completions.GetCompletion(prompt);
            //return result;
        }
    }
}
