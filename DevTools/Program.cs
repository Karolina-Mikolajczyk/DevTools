// See https://aka.ms/new-console-template for more information

using DevTools.Bots;
using DevTools.Lambda.Permissions;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch("bots" , bots =>
    {
        bots.AddCommand<ExtractBotsCommand>("extract")
            .WithDescription("Extracts amazon lex bots from a given path.");

        bots.AddCommand<SetArnsCommand>("set-arns")
            .WithDescription("Sets the ARNs in the bot files.");
        
        bots.AddCommand<ImportBots>("import")
            .WithDescription("Imports the bots to Amazon Lex.");
        
        bots.AddCommand<SetAliasesCommand>("set-aliases")
            .WithDescription("Sets the aliases for the bots.");
    });

    config.AddBranch("lambda", lambdaFunctions =>
    {
        lambdaFunctions.AddCommand<AddBotsPermissionsCommand>("add-bot-permissions")
            .WithDescription("Adds bot permissions to the lambda function.");
    });
    AnsiConsole.Write(
        new FigletText("Aws DevTools")
            .LeftJustified()
            .Color(Color.Red));
});
app.Run(args);