using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Pipeline.Core;

namespace Pipeline.Mcp;

[McpServerToolType]
public class HelixTools
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "helix_work_items_for_build"), Description("Get Helix work items for an AzDo build number")]
    public static async Task<string> GetHelixWorkItemsForBuild(
        Helix helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The AzDo build number")] int buildNumber)
    {
        var items = await helix.GetHelixWorkItemsForBuild(owner, repository, buildNumber);
        return JsonSerializer.Serialize(items, s_jsonOptions);
    }

    [McpServerTool(Name = "helix_work_items_for_pr"), Description("Get Helix work items for a pull request")]
    public static async Task<string> GetHelixWorkItemsForPullRequest(
        Helix helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The pull request number")] int prNumber)
    {
        var items = await helix.GetHelixWorkItemsForPullRequestAsync(owner, repository, prNumber);
        return JsonSerializer.Serialize(items, s_jsonOptions);
    }
}
