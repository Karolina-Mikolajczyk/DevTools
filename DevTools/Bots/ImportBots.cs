using Amazon;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using DevTools.Common;
using DevTools.ConsoleUtils;
using DevTools.Lambda.Permissions;
using Spectre.Console.Cli;

namespace DevTools.Bots;


public class ImportBotsSettings : AwsCommandSettings
{
    [CommandOption("-p|--path <BOTS_PATH>")]
    public required string Path { get; set; }
}

public class ImportBots : AwsAsyncCommand<ImportBotsSettings>
{
    protected override async Task<int> ExecuteCommandAsync(CommandContext context, ImportBotsSettings settings)
    {
        if (!Path.Exists(settings.Path))
        {
            AppConsole.WriteMissingPath(settings.Path);
            return 1;
        }

        var client = new AmazonLexModelBuildingServiceClient(RegionEndpoint.USWest2);

        var zipFiles = Directory.GetFiles(settings.Path, "*.zip");

        await AppConsole.StatusAsync("Import bots...", async () => await ImportBotsAsync(zipFiles, client));

        AppConsole.WriteSuccess("Import complete.");
        return 0;
    }

    private static async Task ImportBotsAsync(string[] zipFiles, AmazonLexModelBuildingServiceClient client)
    {
        foreach (var (zipFile, index) in zipFiles.Select((zipFile, index) => (zipFile, index)))
        {
            AppConsole.WriteInfo("Importing bot {index} of {zipFiles.Length} from {zipFile}...");

            var importId = await StartImportBot(client, zipFile);
            if (string.IsNullOrWhiteSpace(importId))
            {
                AppConsole.WriteError($"Error importing bot from {zipFile}");
                return;

            }

            await WaitForImportToComplete(client, importId);
        }
    }

    private static async Task<string> StartImportBot(AmazonLexModelBuildingServiceClient client, string filePath)
    {
        try
        {
            var zipBytes = await File.ReadAllBytesAsync(filePath);
            var importRequest = new StartImportRequest
            {
                Payload = new MemoryStream(zipBytes),
                ResourceType = ResourceType.BOT,
                MergeStrategy = MergeStrategy.OVERWRITE_LATEST,
            };

            var response = await client.StartImportAsync(importRequest);
            return response.ImportId;
        }
        catch (Exception ex)
        {
            AppConsole.WriteError($"Error importing bot from {filePath}: {ex.Message}");
            return string.Empty;
        }
    }

    private static async Task WaitForImportToComplete(AmazonLexModelBuildingServiceClient client, string importId)
    {
        string status;
        do
        {
            var (importStatus, failureReason) = await CheckImportStatus(client, importId);
            status = importStatus;
            switch (status)
            {
                case "COMPLETE":
                    AppConsole.WriteSuccess("Complete.");
                    break;
                case "FAILED":
                    AppConsole.WriteError("Failed.");
                    foreach (var fail in failureReason)
                    {
                        AppConsole.WriteError($"{fail}");
                    }

                    break;
            }

            if (status == "IN_PROGRESS")
            {
                await Task.Delay(5000);
            }
        } while (status == "IN_PROGRESS");
    }

    private static async Task<(string ImportStatus, List<string> FailureReason)> CheckImportStatus(
        AmazonLexModelBuildingServiceClient client, string importId)
    {
        var request = new GetImportRequest
        {
            ImportId = importId
        };

        var response = await client.GetImportAsync(request);
        return (response.ImportStatus, response.FailureReason); // IN_PROGRESS, COMPLETE, FAILED
    }
}