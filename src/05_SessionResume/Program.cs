using GitHub.Copilot;

// セッション再開: セッション ID を指定して作成し、別のクライアントから文脈を引き継ぐ
const string sessionId = "sample-resume-session";

async Task<string?> AskAsync(CopilotSession session, string prompt)
{
    var reply = await session.SendAndWaitAsync(new MessageOptions { Prompt = prompt });
    return reply?.Data.Content;
}

Console.OutputEncoding = System.Text.Encoding.UTF8;
await using (var client = new CopilotClient())
{
    await client.StartAsync();
    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
        SessionId = sessionId,
        Model = "gpt-5",
        OnPermissionRequest = PermissionHandler.ApproveAll,
    });
    Console.WriteLine(await AskAsync(session, "私の好きな言語は C# です。覚えておいてください。"));
}

await using (var client = new CopilotClient())
{
    await client.StartAsync();
    await using var session = await client.ResumeSessionAsync(sessionId, new ResumeSessionConfig
    {
        OnPermissionRequest = PermissionHandler.ApproveAll,
    });
    Console.WriteLine(await AskAsync(session, "私の好きな言語は何ですか?"));

    foreach (var s in await client.ListSessionsAsync())
        Console.WriteLine($"session: {s.SessionId}");

    await client.DeleteSessionAsync(sessionId); // 後始末
}
