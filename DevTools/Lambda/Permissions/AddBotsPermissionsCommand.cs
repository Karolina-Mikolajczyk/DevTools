using System.Text.Json.Nodes;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using DevTools.Common;
using DevTools.ConsoleUtils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DevTools.Lambda.Permissions;

public class AddBotsPermissionsCommandSettings : AwsCommandSettings
{
    [CommandOption("-p|--path <BOTS_PERMISSIONS_PATH>")]
    public required string Path { get; set; }
    
    [CommandOption("-l|--lambda <LAMBDA_FUNCTION_NAME>")]
    public required string LambdaFunctionName { get; set; }
}


public class AddBotsPermissionsCommand : AwsAsyncCommand<AddBotsPermissionsCommandSettings>
{
    protected override async Task<int> ExecuteCommandAsync(CommandContext context,
        AddBotsPermissionsCommandSettings settings)
    {
        if (!Path.Exists(settings.Path))
        {
            AppConsole.WriteMissingPath(settings.Path);
            return 1;
        }

        if (string.IsNullOrWhiteSpace(settings.LambdaFunctionName))
        {
            AppConsole.WriteError("The lambda function name is required.");
            return 1;
        }


        var region = Amazon.RegionEndpoint.USWest2;

        var statements = GetPermissions(settings.Path);
        if (statements is null)
        {
            return 1;
        }

        using var lambdaClient = new AmazonLambdaClient(region);
        var addPermissionRequests = statements.Select(node => new AddPermissionRequest
        {
            Action = "lambda:InvokeFunction",
            FunctionName = settings.LambdaFunctionName,
            Principal = "lex.amazonaws.com",
            StatementId = node["Sid"].ToString(),
            SourceArn = node["Condition"]!["ArnLike"]!["AWS:SourceArn"]!.ToString()
        }).ToList();

        await AnsiConsole.Status().StartAsync("Adding permissions...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("green"));
            ctx.Status("Adding permissions...");
            foreach (var request in addPermissionRequests)
            {
                AnsiConsole.MarkupLine($"Processing permission for {request.StatementId}...");

                try
                {
                    await lambdaClient.AddPermissionAsync(request);

                }
                catch (ResourceConflictException ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Permission already exists: {ex.Message}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to add permission: {ex.Message}[/]");
                }

            }
        });
        
        return 0;
    }

    static JsonArray? GetPermissions(string path)
    {
        var json = File.ReadAllText(path);
        var bot = JsonNode.Parse(json);
        
        if (bot is null)
        {
            AnsiConsole.MarkupLine($"[yellow]No JSON found.[/]");
            return null;
        }

        if(bot["Statement"] is null || bot["Statement"]!.AsArray().Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No statements found.'[/]");
            return null;
        }

        return bot["Statement"]!.AsArray();
    }
}