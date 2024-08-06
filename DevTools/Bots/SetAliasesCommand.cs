using Amazon;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using DevTools.Common;
using DevTools.ConsoleUtils;
using DevTools.Lambda.Permissions;
using Spectre.Console.Cli;

namespace DevTools.Bots;

public class SetAliasesCommandSettings : AwsCommandSettings
{
    [CommandArgument(0, "<ALIAS_NAME>")]
    public required string AliasName { get; set; }
}

public class SetAliasesCommand : AwsAsyncCommand<SetAliasesCommandSettings>
{
     protected override async Task<int> ExecuteCommandAsync(CommandContext context, SetAliasesCommandSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AliasName))
        {
            AppConsole.WriteError("The alias name is required.");
            return 1;
        }
        
        var client = new AmazonLexModelBuildingServiceClient(RegionEndpoint.USWest2);

        var botsResponse = await GetAllBotsAsync(client);
        
        AppConsole.WriteInfo("Found {botsResponse.Count} bots.");
        var botAliasesRequests = botsResponse.Select(bot => bot)
            .ToDictionary(bot => bot.Name, bot => new GetBotAliasesRequest
            {
                BotName = bot.Name
            });

        var botsToUpdate = new List<BotMetadata>();
        
        foreach (var (botName, botRequest) in botAliasesRequests)
        {
            var aliasesResponse = await client.GetBotAliasesAsync(botRequest);
            var aliases = aliasesResponse.BotAliases.Select(alias => alias.Name).ToList();

            if (aliases.Contains(settings.AliasName))
            {
                AppConsole.WriteWarning($"The alias {settings.AliasName} already exists for bot {botName}.");
                continue;
            }
            
            botsToUpdate.Add(botsResponse.First(x=>x.Name == botName));
        }
        
        AppConsole.WriteInfo($"Found {botsToUpdate.Count} bots without {settings.AliasName} aliases.");
        var putBotAliasRequests = botsToUpdate.Select(bot => new PutBotAliasRequest
        {
            BotName = bot.Name,
            BotVersion = "$LATEST",
            Name = settings.AliasName,
        });

        var botAliasRequests = putBotAliasRequests.ToList();

        await AppConsole.StatusAsync("Creating aliases",async () => await CreateAlias(botAliasRequests, client));
        
        return 0;
    }

    private static async Task CreateAlias(List<PutBotAliasRequest> botAliasRequests, AmazonLexModelBuildingServiceClient client)
    {
        foreach (var (request, index) in botAliasRequests.Select((request, index)=>(request, index)))
        {
            AppConsole.WriteInfo($"Process for bot: {request.BotName}...");
            await client.PutBotAliasAsync(request);
        }
    }

    private static async Task<List<BotMetadata>> GetAllBotsAsync(AmazonLexModelBuildingServiceClient client)
    {
        var allBots = new List<BotMetadata>();
        string nextToken = null;

        do
        {
            var request = new GetBotsRequest
            {
                NextToken = nextToken,
                MaxResults = 50
            };

            GetBotsResponse response = await client.GetBotsAsync(request);
            allBots.AddRange(response.Bots);
            nextToken = response.NextToken;

        } while (nextToken != null);

        return allBots;
    }

}