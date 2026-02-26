using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Azure.Core;
using Kusto.Data;
using Kusto.Data.Net.Client;
using Microsoft.Identity.Client.NativeInterop;

namespace Pipeline.Core;

public class HelixWorkItem
{
    [JsonPropertyName("friendlyName")]
    public required string FriendlyName { get; init; }
    [JsonPropertyName("executionTime")]
    public int ExecutionTime { get; init; }
    [JsonPropertyName("queueName")]
    public required string QueueName { get; init; }
    [JsonPropertyName("queuedTime")]
    public int QueuedTime { get; init; }
    [JsonPropertyName("azdoBuildId")]
    public int AzdoBuildId { get; init; }
    [JsonPropertyName("azdoPhaseName")]
    public required string AzdoPhaseName { get; init; }
    [JsonPropertyName("azdoAttempt")]
    public int AzdoAttempt { get; init; }
    [JsonPropertyName("machineName")]
    public required string MachineName { get; init; }
    [JsonPropertyName("exitCode")]
    public int ExitCode { get; init; }
    [JsonPropertyName("consoleUri")]
    public required string ConsoleUri { get; init; }
    [JsonPropertyName("jobId")]
    public long JobId { get; init; }
    [JsonPropertyName("jobName")]
    public required string JobName { get; init; }
    [JsonPropertyName("finished")]
    public DateTime Finished { get; init; }
    [JsonPropertyName("workItemId")]
    public long WorkItemId { get; init; }
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}

public class HelixWorkItemConsole
{
    [JsonPropertyName("jobId")]
    public long JobId { get; init; }
    [JsonPropertyName("workItemId")]
    public long WorkItemId { get; init; }
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

public sealed class HelixClient
{
    private const string ClusterUrl = "https://engsrvprod.kusto.windows.net";
    private const string DatabaseName = "engineeringdata";

    private AccessToken AccessToken { get; } 
    private KustoConnectionStringBuilder KustoConnectionStringBuilder { get; }

    private HelixClient(AccessToken accessToken)
    {
        AccessToken = accessToken;
        KustoConnectionStringBuilder = new KustoConnectionStringBuilder(ClusterUrl, DatabaseName)
            .WithAadTokenProviderAuthentication(() => AccessToken.Token);
    }

    public static async Task<HelixClient> CreateAsync(TokenCredential tokenCredential)
    {
        var tokenRequestContext = new TokenRequestContext(["https://kusto.kusto.windows.net/.default"]);
        var token = await tokenCredential.GetTokenAsync(tokenRequestContext, default);

        return new HelixClient(token);
    } 

    public Task<List<HelixWorkItem>> GetHelixWorkItemsForBuildAsync(string owner, string repository, int buildNumber, bool includeAll = false)
    {
        var failedFilter = includeAll ? "" : "| where ExitCode != 0";
        string query = $"""
            Jobs
            | where Repository == "{owner}/{repository}"
            | project-away Started, Finished
            | join kind=inner WorkItems on JobId
            | extend p = parse_json(Properties)
            | extend AzdoBuildId = toint(p["BuildId"])
            | where AzdoBuildId == {buildNumber}
            | extend AzdoPhaseName = tostring(p["System.PhaseName"])
            | extend AzdoAttempt = tostring(p["System.JobAttempt"])
            | extend ExecutionTime = (Finished - Started) / 1s
            | extend QueuedTime = (Started - Queued) / 1s
            {failedFilter}
            | project FriendlyName, ExecutionTime, QueuedTime, AzdoBuildId, AzdoPhaseName, AzdoAttempt, MachineName, ExitCode, ConsoleUri, JobId, JobName, QueueName, Finished, WorkItemId, Status
            """;

        return QueryHelixWorkItem(query);
    }

    public Task<List<HelixWorkItem>> GetHelixWorkItemsForPullRequestAsync(string owner, string repository, int prNumber, bool includeAll = false)
    {
        var failedFilter = includeAll ? "" : "| where ExitCode != 0";
        string query = $"""
            Jobs
            | where Repository == "{owner}/{repository}"
            | where Branch == "refs/pull/{prNumber}/merge"
            | project-away Started, Finished
            | join kind=inner WorkItems on JobId
            | extend p = parse_json(Properties)
            | extend AzdoPhaseName = tostring(p["System.PhaseName"])
            | extend AzdoAttempt = tostring(p["System.JobAttempt"])
            | extend AzdoBuildId = toint(p["BuildId"])
            | extend ExecutionTime = (Finished - Started) / 1s
            | extend QueuedTime = (Started - Queued) / 1s
            {failedFilter}
            | project FriendlyName, ExecutionTime, QueuedTime, AzdoBuildId, AzdoPhaseName, AzdoAttempt, MachineName, ExitCode, ConsoleUri, JobId, JobName, QueueName, Finished, WorkItemId, Status
            """;

        return QueryHelixWorkItem(query);
    }

    public async Task<HelixWorkItem> GetHelixWorkItemAsync(long jobId, long workItemId)
    {
        string query = $"""
            WorkItems
            | where JobId == {jobId}
            | where WorkItemId == {workItemId}
            | join kind=inner Jobs on JobId
            | extend p = parse_json(Properties)
            | extend AzdoPhaseName = tostring(p["System.PhaseName"])
            | extend AzdoAttempt = tostring(p["System.JobAttempt"])
            | extend AzdoBuildId = toint(p["BuildId"])
            | extend ExecutionTime = (Finished - Started) / 1s
            | extend QueuedTime = (Started - Queued) / 1s
            | project FriendlyName, ExecutionTime, QueuedTime, AzdoBuildId, AzdoPhaseName, AzdoAttempt, MachineName, ExitCode, ConsoleUri, JobId, JobName, QueueName, Finished, WorkItemId, Status
            """;

        var items = await QueryHelixWorkItem(query);
        return items.Single();
    }

    private async Task<List<HelixWorkItem>> QueryHelixWorkItem(string query)
    {
        try
        {
            using var kustoQueryClient = KustoClientFactory.CreateCslQueryProvider(KustoConnectionStringBuilder);
            var reader = kustoQueryClient.ExecuteQuery(query);
            var list = new List<HelixWorkItem>();

            // Read and print results
            while (reader.Read())
            {
                var friendlyName = reader.GetString(0);
                var executionTime = TimeSpan.FromSeconds(reader.GetDouble(1));
                var queuedTime = TimeSpan.FromSeconds(reader.GetDouble(2));
                var azdoBuildId = reader.GetInt32(3);
                var azdoPhaseName = reader.GetString(4);
                var azdoAttempt = int.Parse(reader.GetString(5));
                var machineName = reader.GetString(6);
                var exitCode = reader.GetInt32(7);
                var consoleUri = reader.GetString(8);
                var jobId = reader.GetInt64(9);
                var jobName = reader.GetString(10);
                var queueName = reader.GetString(11);
                var finished = reader.GetDateTime(12);
                var workItemId = reader.GetInt64(13);
                var status = reader.GetString(14);

                list.Add(new HelixWorkItem
                {
                    FriendlyName = friendlyName,
                    ExecutionTime = (int)executionTime.TotalSeconds,
                    QueuedTime = (int)queuedTime.TotalSeconds,
                    AzdoBuildId = azdoBuildId,
                    AzdoPhaseName = azdoPhaseName,
                    AzdoAttempt = azdoAttempt,
                    MachineName = machineName,
                    ExitCode = exitCode,
                    ConsoleUri = consoleUri,
                    JobId = jobId,
                    JobName = jobName,
                    QueueName = queueName,
                    Finished = finished,
                    WorkItemId = workItemId,
                    Status = status
                });
            }

            return list;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Error reading Kusto, are you connected to the VPN?");
            throw;
        }
    }

    public async Task<HelixWorkItemConsole> GetConsoleAsync(HelixWorkItem workItem)
    {
        using var httpClient = new HttpClient();
        var text = await httpClient.GetStringAsync(workItem.ConsoleUri);
        return new HelixWorkItemConsole
        {
            JobId = workItem.JobId,
            WorkItemId = workItem.WorkItemId,
            Text = text
        };
    }

    public async Task<List<HelixWorkItemConsole>> GetConsolesAsync(List<HelixWorkItem> workItems)
    {
        using var httpClient = new HttpClient();
        var list = new List<HelixWorkItemConsole>();
        foreach (var workItem in workItems)
        {
            var text = await httpClient.GetStringAsync(workItem.ConsoleUri);
            list.Add(new HelixWorkItemConsole
            {
                JobId = workItem.JobId,
                WorkItemId = workItem.WorkItemId,
                Text = text
            });
        }
        return list;
    }
}