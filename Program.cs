// See https://aka.ms/new-console-template for more information
using GitHub.Copilot.SDK;

Console.WriteLine("Hello, World!");

CopilotClient? client = null;
try
{
    client = await CreateClientAsync();

    client.CreateSessionAsync()
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
}
finally
{
    if (client is not null)
    {
        await client.StopAsync();
    }
}

async Task<CopilotClient> CreateClientAsync()
{
    try
    {
        var client = new CopilotClient();
        await client.StartAsync();
        return client;
    }
    catch (FileNotFoundException e)
    {
        throw new Exception("Copilot CLI not found. Please install it first.", e);
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("connection"))
    {
        throw new Exception("Could not connect to Copilot CLI server.", ex);
    }
}
