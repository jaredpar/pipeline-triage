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
        var items = await helix.GetHelixWorkItemsForBuildAsync(owner, repository, buildNumber, includeAll);
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

    [McpServerTool(Name = "helix_console_for_build"), Description("Get console output for Helix work items in an AzDo build")]
    public static async Task<string> GetHelixConsoleForBuild(
        HelixClient helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The AzDo build number")] int buildNumber,
        [Description("If true, include all work items including succeeded ones. Default returns only failed items.")] bool includeAll = false)
    {
        var items = await helix.GetHelixWorkItemsForBuildAsync(owner, repository, buildNumber, includeAll);
        var consoles = await helix.GetConsolesAsync(items);
        return JsonSerializer.Serialize(consoles, s_jsonOptions);
    }

    [McpServerTool(Name = "helix_console_for_pr"), Description("Get console output for Helix work items in a pull request")]
    public static async Task<string> GetHelixConsoleForPullRequest(
        HelixClient helix,
        [Description("The repository owner (e.g. dotnet)")] string owner,
        [Description("The repository name (e.g. roslyn)")] string repository,
        [Description("The pull request number")] int prNumber,
        [Description("If true, include all work items including succeeded ones. Default returns only failed items.")] bool includeAll = false)
    {
        var items = await helix.GetHelixWorkItemsForPullRequestAsync(owner, repository, prNumber, includeAll);
        var consoles = await helix.GetConsolesAsync(items);
        return JsonSerializer.Serialize(consoles, s_jsonOptions);
    }

    [McpServerTool(Name = "helix_console_for_work_item"), Description("Get console output for a specific Helix work item by job ID and work item ID")]
    public static async Task<string> GetHelixConsoleForWorkItem(
        HelixClient helix,
        [Description("The Helix job ID")] long jobId,
        [Description("The Helix work item ID")] long workItemId)
    {
        var workItem = await helix.GetHelixWorkItemAsync(jobId, workItemId);
        var console = await helix.GetConsoleAsync(workItem);
        return JsonSerializer.Serialize(console, s_jsonOptions);
    }
}
