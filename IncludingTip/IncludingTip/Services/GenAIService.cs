using System.ClientModel;
using System.Text;
using IncludingTip.Model;
using Microsoft.EntityFrameworkCore;
using OpenAI;

namespace IncludingTip.Services;

public class GenAIService
{
    private OpenAIClient _aiClient;
    private readonly IDbContextFactory<TipApplicationContext> _dbContextFactory;
    public const string DefaultModel = "gemini-2.0-flash";

    public GenAIService(IDbContextFactory<TipApplicationContext> dbContextFactory)
    {
        this._dbContextFactory = dbContextFactory;
        _aiClient = new OpenAIClient(
            new ApiKeyCredential(
                Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? ""
            ),
            new OpenAIClientOptions()
            {
                Endpoint = new(@"https://generativelanguage.googleapis.com/v1beta/openai/"),
            });
    }

    private async Task<string> SummarizeTips(ICollection<Tip> tips)
    {
        StringBuilder sb = new();
        
        foreach (var tip in tips)
        {
            string experience = $"#{tip.Title}\n{tip.Experience}\n**Tip**: {tip.Percent}%\n**Place name**: {tip.Place.Name}\n";

            sb.Append(experience);
        }

        var experiences = sb.ToString();

        // Kowalski, analysis
        var result =
            await _aiClient
                .GetChatClient(DefaultModel)
                .CompleteChatAsync($"Summarize following experiences from tip places and assume what influences tipping in this area. Output should be in english:\n${experiences}");

        return result.Value.Content[0].Text;
    }


    public async Task<string> GetRecentCountryTippingSummary(Country country)
    {
        // Get up to 10 most recent tips in places inside country, Get their content and send it to AI model
        var database = await _dbContextFactory.CreateDbContextAsync();

        var ids = country.Places.Select(p => p.Id).ToArray();

        var tips = database.Tips.Where(p => ids.Contains(p.PlaceId)).OrderByDescending(t=>t.Id).Take(10);


        return await SummarizeTips(tips.ToList());
    }
    
}