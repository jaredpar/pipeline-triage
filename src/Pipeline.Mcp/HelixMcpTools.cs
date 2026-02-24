using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Pipeline.Core;

namespace Pipeline.Mcp;

[McpServerToolType]
public class HelixMcpTools
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "helix_work_items_for_build"), Description("Get Helix work items for an AzDo build number")]
    public static async Task<string> GetHelixWorkItemsForBuild(
        HelixClient helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The AzDo build number")] int buildNumber,
        [Description("If true, include all work items including succeeded ones. Default returns only failed items.")] bool includeAll = false)
    {
        var items = await helix.GetHelixWorkItemsForBuild(owner, repository, buildNumber, includeAll);
        return JsonSerializer.Serialize(items, s_jsonOptions);
    }

    [McpServerTool(Name = "helix_work_items_for_pr"), Description("Get Helix work items for a pull request")]
    public static async Task<string> GetHelixWorkItemsForPullRequest(
        HelixClient helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The pull request number")] int prNumber,
        [Description("If true, include all work items including succeeded ones. Default returns only failed items.")] bool includeAll = false)
    {
        var items = await helix.GetHelixWorkItemsForPullRequestAsync(owner, repository, prNumber, includeAll);
        return JsonSerializer.Serialize(items, s_jsonOptions);
    }
}
