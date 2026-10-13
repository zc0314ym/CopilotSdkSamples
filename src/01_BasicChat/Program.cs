using GitHub.Copilot;

// 基本: クライアント起動 -> セッション作成 -> 送信 -> 応答待ち
Console.OutputEncoding = System.Text.Encoding.UTF8;
await using var client = CopilotAuth.CreateClient();
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    // サンプル用に全ツール実行を自動承認。本番では個別に判定すること。
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

var done = new TaskCompletionSource();
session.On<SessionEvent>(evt =>
{
    switch (evt)
    {
        case AssistantMessageEvent msg:
            Console.WriteLine(msg.Data.Content);
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

await session.SendAsync(new MessageOptions { Prompt = "C# の record 型を 3 行で説明してください。" });
await done.Task;
