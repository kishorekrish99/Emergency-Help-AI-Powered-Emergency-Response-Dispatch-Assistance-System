namespace EmergencyHelp.Services
{
    public interface IOpenAiService
    {
        Task<string> CompleteSentence(string prompt);
    }
}
