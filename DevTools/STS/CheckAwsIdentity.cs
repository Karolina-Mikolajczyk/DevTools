using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Spectre.Console;

namespace DevTools.STS;

public class CheckAwsIdentity
{
    private static string[] ProductionAccounts = [];
    public static async Task<bool> IsProductionAccount()
    {
        using var client = new AmazonSecurityTokenServiceClient();
        try
        {
            var response = await client.GetCallerIdentityAsync(new GetCallerIdentityRequest());
            AnsiConsole.MarkupLine($"[green]Account: {response.Account}, ARN: {response.Arn}, UserID: {response.UserId}[/]");

            var contains = ProductionAccounts.Contains(response.Account);
            return contains;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error in retrieving caller identity: {ex.Message}[/]");
            return false;
        }
    }
}
