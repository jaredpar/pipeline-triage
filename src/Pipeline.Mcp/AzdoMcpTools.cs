using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Pipeline.Core;

namespace Pipeline.Mcp;

[McpServerToolType]
public class AzdoMcpTools
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "azdo_builds_for_repo"), Description("Get AzDO builds for a GitHub repository. Returns both PR and CI builds by default.")]
    public static async Task<string> GetBuildsForRepository(
        AzdoClient azdoClient,
        [Description("The GitHub repository in owner/repo format (e.g. dotnet/roslyn)")] string repository,
        [Description("Maximum number of builds to return (default 10)")] int top = 10,
        [Description("Filter builds: 'pr' for pull request builds only, 'ci' for post-merge builds only, 'all' for both (default)")] string filter = "all")
    {
        string? reasonFilter = filter.ToLowerInvariant() switch
        {
            "pr" => "pullRequest",
            "ci" => "individualCI,batchedCI",
            _ => null,
        };

        var builds = await azdoClient.GetBuildsForRepositoryAsync(repository, top, reasonFilter);
        return JsonSerializer.Serialize(builds, s_jsonOptions);
    }
}
