using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Pipeline.Core;

namespace Pipeline.Mcp;

[McpServerToolType]
public class AzdoMcpTools
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "azdo_builds_for_repo"), Description("Get AzDO builds for a repository. Supports both GitHub repositories and AzDO Git (TfsGit) repositories.")]
    public static async Task<string> GetBuildsForRepository(
        AzdoClient azdoClient,
        [Description("The repository identifier. For GitHub repos use owner/repo format (e.g. dotnet/roslyn). For AzDO Git repos use the repo name (e.g. razor-toolset-tests).")] string repository,
        [Description("Maximum number of builds to return (default 10)")] int top = 10,
        [Description("Filter builds: 'pr' for pull request builds only, 'ci' for post-merge builds only, 'all' for both (default)")] string filter = "all",
        [Description("Repository type: 'GitHub' for GitHub repos (default), 'TfsGit' for AzDO Git repos")] string repositoryType = "GitHub",
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        string? reasonFilter = filter.ToLowerInvariant() switch
        {
            "pr" => "pullRequest",
            "ci" => "individualCI,batchedCI",
            _ => null,
        };

        var builds = await azdoClient.GetBuildsForRepositoryAsync(repository, top, reasonFilter, repositoryType, organization, project);
        return JsonSerializer.Serialize(builds, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_recent_builds"), Description("Get recent AzDO builds, optionally filtered by pipeline definition ID.")]
    public static async Task<string> GetRecentBuilds(
        AzdoClient azdoClient,
        [Description("Optional pipeline definition ID to filter by")] int? definitionId = null,
        [Description("Maximum number of builds to return (default 10)")] int top = 10,
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var builds = await azdoClient.GetRecentBuildsAsync(definitionId, top, organization, project);
        return JsonSerializer.Serialize(builds, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_pr_builds"), Description("Get AzDO builds for a specific pull request. Supports both GitHub and AzDO Git (TfsGit) repositories.")]
    public static async Task<string> GetBuildsForPullRequest(
        AzdoClient azdoClient,
        [Description("The repository identifier. For GitHub repos use owner/repo format (e.g. dotnet/roslyn). For AzDO Git repos use the repo name (e.g. razor-toolset-tests).")] string repository,
        [Description("The pull request number")] int prNumber,
        [Description("Maximum number of builds to return (default 10)")] int top = 10,
        [Description("Repository type: 'GitHub' for GitHub repos (default), 'TfsGit' for AzDO Git repos")] string repositoryType = "GitHub",
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var builds = await azdoClient.GetBuildsForPullRequestAsync(repository, prNumber, top, repositoryType, organization, project);
        return JsonSerializer.Serialize(builds, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_test_failures"), Description("Get test failures for an AzDO build.")]
    public static async Task<string> GetTestFailures(
        AzdoClient azdoClient,
        [Description("The AzDO build ID")] int buildId,
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var failures = await azdoClient.GetTestFailuresAsync(buildId, organization, project);
        return JsonSerializer.Serialize(failures, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_timeline"), Description("Get the timeline (all records) for an AzDO build.")]
    public static async Task<string> GetTimeline(
        AzdoClient azdoClient,
        [Description("The AzDO build ID")] int buildId,
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var timeline = await azdoClient.GetTimelineAsync(buildId, organization, project);
        return JsonSerializer.Serialize(timeline, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_artifacts"), Description("Get build artifacts for an AzDO build.")]
    public static async Task<string> GetArtifacts(
        AzdoClient azdoClient,
        [Description("The AzDO build ID")] int buildId,
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var artifacts = await azdoClient.GetArtifactsAsync(buildId, organization, project);
        return JsonSerializer.Serialize(artifacts, s_jsonOptions);
    }

    [McpServerTool(Name = "azdo_jobs"), Description("Get job records from an AzDO build timeline.")]
    public static async Task<string> GetJobs(
        AzdoClient azdoClient,
        [Description("The AzDO build ID")] int buildId,
        [Description("AzDO organization (default: dnceng-public). Use 'dnceng' for internal repos.")] string? organization = null,
        [Description("AzDO project (default: public). Use 'internal' for internal repos.")] string? project = null)
    {
        var timeline = await azdoClient.GetTimelineAsync(buildId, organization, project);
        var jobs = timeline.Records
            .Where(r => r.RecordType == "Job")
            .OrderBy(r => r.Order)
            .ToList();
        return JsonSerializer.Serialize(jobs, s_jsonOptions);
    }
}
