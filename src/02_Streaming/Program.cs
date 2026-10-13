using GitHub.Copilot;

// ストリーミング: 応答を逐次表示する
Console.OutputEncoding = System.Text.Encoding.UTF8;
await using var client = CopilotAuth.CreateClient();
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    Streaming = true,
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

var done = new TaskCompletionSource();
session.On<SessionEvent>(evt =>
{
    switch (evt)
    {
        case AssistantMessageDeltaEvent delta:
            Console.Write(delta.Data.DeltaContent);
            break;
        case AssistantMessageEvent:
            Console.WriteLine();
            break;
        case SessionErrorEvent err:
            Console.Error.WriteLine($"Error: {err.Data.Message}");
            done.TrySetResult();
            break;
        case SessionIdleEvent:
            done.TrySetResult();
            break;
    }
});

await session.SendAsync(new MessageOptions { Prompt = "async/await の仕組みを短く説明してください。" });
await done.Task;
