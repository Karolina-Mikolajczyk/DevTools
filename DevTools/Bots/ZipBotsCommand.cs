using System.IO.Compression;
using DevTools.ConsoleUtils;
using Spectre.Console.Cli;

namespace DevTools.Bots;

public class ZipBotsSettings : CommandSettings
{
    [CommandOption("-p|--path <BOTS_PATH>")]
    public required string Path { get; set; }
}

public class ZipBotsCommand : Command<ZipBotsSettings>
{

    public override int Execute(CommandContext context, ZipBotsSettings settings)
    {
        if (!Path.Exists(settings.Path))
        {
            AppConsole.WriteMissingPath(settings.Path);
            return 1;
        }

        var filesRecursively = DirectoryHelper.ToListRecursively(settings.Path);
        if (!filesRecursively.Any())
        {
            AppConsole.WriteError("No files found in '{settings.Path}'");
            return 1;
        }


        AppConsole.WriteInfo($"Importing bots from '{settings.Path}'...");

        var directories = Directory.GetDirectories(settings.Path);
        var files = Directory.GetFiles(settings.Path);

        AppConsole.Status("Creating zip files...", () =>
        {
            CreateZipFromDirectories(settings, directories);
            ZipFromFiles(settings, files);
        });
        return 0;
    }

    private static void ZipFromFiles(ZipBotsSettings settings, string[] files)
    {
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var zipFilePath = Path.Combine(settings.Path, $"{fileName}.zip");
            using var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(filePath, Path.GetFileName(fileName));
        }
    }

    private static void CreateZipFromDirectories(ZipBotsSettings settings, string[] directories)
    {
        foreach (var directory in directories)
        {
            var directoryName = Path.GetDirectoryName(directory);
            ZipFile.CreateFromDirectory(directory, Path.Combine(settings.Path, $"{directoryName}.zip"));
        }
    }
}



    