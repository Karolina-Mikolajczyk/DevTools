using System.Text.Json.Nodes;
using DevTools.ConsoleUtils;
using Spectre.Console.Cli;

namespace DevTools.Bots;


public class SetArnsCommandSettings : CommandSettings
{
    [CommandOption("-p|--path <PATH>")]
    public required string Path { get; set; }
    
    [CommandOption("-n|--new-arn <NEW_ARN>")]
    public required string NewArn { get; set; }
}
public class SetArnsCommand : Command<SetArnsCommandSettings>
{
     public override int Execute(CommandContext context, SetArnsCommandSettings commandSettings)
    {
        if (!Path.Exists(commandSettings.Path))
        {
            AppConsole.WriteMissingPath(commandSettings.Path);
            return 1;
        }
        
        if (string.IsNullOrWhiteSpace(commandSettings.NewArn))
        {
            AppConsole.WriteError("The new ARN is required.");
            return 1;
        }

        AppConsole.WriteInfo($"Setting ARNs in '{commandSettings.Path}'...");

        var allFiles = DirectoryHelper.ToListRecursively(commandSettings.Path);

        var botFiles = allFiles.Where(name => Path.GetExtension(name) == ".json")
            .ToList();
        
        AppConsole.WriteInfo($"Found {botFiles.Count} bot files.");
        
        foreach (var (botFile, index) in botFiles.Select((file, index) => (file, index)))
        {
            AppConsole.WriteInfo($"Processing {index}/{botFiles.Count} file...");
            var json = File.ReadAllText(botFile);
            var bot = JsonNode.Parse(json);
            if (bot is null)
            {
                AppConsole.WriteWarning("No JSON found.");
                continue;
            }
            
            if (bot["resource"]is null || bot["resource"]!["intents"] is null || bot["resource"]?["intents"]?.AsArray().Count == 0)
            {
                AppConsole.WriteWarning("No intents found.");
                continue;
            }
            
            foreach (var jsonNode in bot["resource"]!["intents"]!.AsArray())
            {
                if (jsonNode!["dialogCodeHook"] is null || jsonNode!["dialogCodeHook"]!["uri"] is null)
                {
                   continue;
                }
        
                
                jsonNode["dialogCodeHook"]!["uri"] = commandSettings.NewArn;
                AppConsole.WriteSuccess($"Set ARN in {botFile}");
            }
            
            
            File.WriteAllText(botFile, bot.ToString());
        }

        return 0;
    }
}
