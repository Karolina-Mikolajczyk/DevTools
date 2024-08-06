using Spectre.Console;

namespace DevTools.ConsoleUtils;

public static class AppConsole
{
    public static void WriteMissingPath(string path) => WriteError($"The path '{path}' does not exist.");
    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }
    public static void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine($"{message}");
    }
    
    public static void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }
    
    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");
    }

    public static async Task StatusAsync(string status, Func<Task> func)
    {
        await AnsiConsole.Status()
            .StartAsync(string.Empty, async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                ctx.Status(status);
                await func();
            });
    }

    
    public static void Status(string status, Action func)
    {
        AnsiConsole.Status()
            .Start(string.Empty, ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                ctx.Status(status);
                func();
            });
    }
}
