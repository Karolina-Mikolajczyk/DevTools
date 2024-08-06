using DevTools.STS;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DevTools.Common;

public abstract class AwsCommandSettings : CommandSettings
{
    [CommandOption("-s|--skip-check-identity")]
    public bool SkipCheckIdentity { get; set; } = false;
}

public abstract class AwsAsyncCommand<TSettings> : AsyncCommand<TSettings> where TSettings : AwsCommandSettings
{
    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        if (!settings.SkipCheckIdentity)
        {
            var isProductionAccount = await CheckAwsIdentity.IsProductionAccount();
            if (isProductionAccount)
            {
                AnsiConsole.MarkupLine("[red]This is a production account. Use -s|--skip-check-identity flag to skip this check.[/]");
                return 1;
            }
        }

        return await ExecuteCommandAsync(context, settings);
    }

    protected abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
}