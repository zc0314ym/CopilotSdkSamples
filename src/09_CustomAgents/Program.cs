using GitHub.Copilot;

// カスタムエージェント: 専用プロンプト・ツール制限を持つサブエージェントを定義する
Console.OutputEncoding = System.Text.Encoding.UTF8;

await using var client = CopilotAuth.CreateClient();
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    WorkingDirectory = Directory.GetCurrentDirectory(),
    CustomAgents = new List<CustomAgentConfig>
    {
        new()
        {
            Name = "researcher",
            DisplayName = "調査エージェント",
            Description = "コードを読んで質問に答える。読み取り専用で、ファイルは変更しない",
            Tools = new List<string> { "grep", "glob", "view" }, // 読み取り系ツールのみ許可
            Prompt = "あなたは調査担当です。コードを分析して日本語で簡潔に回答します。ファイルは決して変更しません。",
        },
        new()
        {
            Name = "reviewer",
            DisplayName = "レビューエージェント",
            Description = "C# コードのバグ・可読性・パフォーマンスの問題をレビューする",
            Tools = new List<string> { "view", "grep" },
            Prompt = "あなたは厳格な C# コードレビュアーです。問題点を重要度順に箇条書きで日本語で報告します。",
        },
    },
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

// どのエージェントが選ばれたか表示する
session.On<SessionEvent>(evt =>
{
    switch (evt)
    {
        case SubagentStartedEvent s:
            Console.WriteLine($"[agent 開始] {s.Data.AgentDisplayName}");
            break;
        case SubagentCompletedEvent c:
            Console.WriteLine($"[agent 完了] {c.Data.AgentDisplayName}");
            break;
    }
});

var reply = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "調査エージェントを使って、このフォルダにある .cs ファイルの一覧と、それぞれの役割を教えてください。",
});
Console.WriteLine(reply?.Data.Content);
