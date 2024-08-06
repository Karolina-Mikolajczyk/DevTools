using System.IO.Compression;
using DevTools.ConsoleUtils;
using Spectre.Console.Cli;

namespace DevTools.Bots;

public class ExtractBotsSettings: CommandSettings
{
    [CommandArgument(0, "[path]")]
    public required string Path { get; set; }
    
    [CommandOption("-d|--destination <DESTINATION>")]
    public string? Destination { get; set; }
}
public class ExtractBotsCommand : Command<ExtractBotsSettings>
{
    public override int Execute(CommandContext context, ExtractBotsSettings settings)
    {
        if (!Path.Exists(settings.Path))
        {
            AppConsole.WriteMissingPath(settings.Path);
            return 1;
        }
        AppConsole.WriteInfo($"Extracting bots from '{settings.Path}'...");

        var files = Directory.GetFiles(settings.Path);
        AppConsole.WriteInfo($"Found {files.Length} files.");

        var destDirectory = string.Empty;
        if (string.IsNullOrWhiteSpace(settings.Destination) || !Path.Exists(settings.Destination))
        {
            AppConsole.WriteError($"The destination '{settings.Destination}' does not exist.");
            
            destDirectory = Directory.CreateDirectory(Path.Combine(settings.Path, "extracted")).FullName;
            AppConsole.WriteInfo($"Extract files to {destDirectory}");
        }
        else
        {
            destDirectory = settings.Destination;
        }
        
        
        try
        {
            foreach (var file in files)
            {
                AppConsole.WriteInfo($"Extracting '{file}'...");
                ZipFile.ExtractToDirectory(file, destDirectory, true);
            }
            AppConsole.WriteSuccess("Extraction complete.");
        }
        catch (Exception e)
        {
            AppConsole.WriteError($"An error occurred: {e.Message}");
            return 1;
        }
        
        return 0;
    }
}