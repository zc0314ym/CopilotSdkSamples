using System.ComponentModel;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

// フック: ツール実行前後・プロンプト送信・セッション開始/終了などに処理を差し込む
Console.OutputEncoding = System.Text.Encoding.UTF8;

await using var client = CopilotAuth.CreateClient();
await client.StartAsync();

var readNote = CopilotTool.DefineTool(
    ([Description("メモのタイトル")] string title) => new { title, body = "これはデモ用のメモ本文です。" },
    factoryOptions: new AIFunctionFactoryOptions { Name = "read_note", Description = "メモを読む" },
    toolOptions: new CopilotToolOptions { SkipPermission = true });

var deleteNote = CopilotTool.DefineTool(
    ([Description("メモのタイトル")] string title) => new { deleted = title },
    factoryOptions: new AIFunctionFactoryOptions { Name = "delete_note", Description = "メモを削除する" },
    toolOptions: new CopilotToolOptions { SkipPermission = true });

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    Tools = [readNote, deleteNote],
    OnPermissionRequest = PermissionHandler.ApproveAll,
    Hooks = new SessionHooks
    {
        OnSessionStart = (input, _) =>
        {
            Console.WriteLine($"[hook] セッション開始 ({input.Source})");
            return Task.FromResult<SessionStartHookOutput?>(null);
        },

        OnUserPromptSubmitted = (input, _) =>
        {
            Console.WriteLine($"[hook] プロンプト: {input.Prompt}");
            return Task.FromResult<UserPromptSubmittedHookOutput?>(null);
        },

        // ツール実行前: ポリシー違反 (delete_*) は拒否する
        OnPreToolUse = (input, _) =>
        {
            Console.WriteLine($"[hook] 実行前: {input.ToolName}");
            var output = input.ToolName.StartsWith("delete_", StringComparison.Ordinal)
                ? new PreToolUseHookOutput { PermissionDecision = "deny", PermissionDecisionReason = "削除系ツールはポリシーで禁止されています" }
                : new PreToolUseHookOutput { PermissionDecision = "allow" };
            return Task.FromResult<PreToolUseHookOutput?>(output);
        },

        OnPostToolUse = (input, _) =>
        {
            Console.WriteLine($"[hook] 実行後: {input.ToolName}");
            return Task.FromResult<PostToolUseHookOutput?>(null);
        },

        OnSessionEnd = (input, _) =>
        {
            Console.WriteLine($"[hook] セッション終了 ({input.Reason})");
            return Task.FromResult<SessionEndHookOutput?>(null);
        },
    },
});

var reply = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "メモ「会議」を読んで内容を教えてください。その後、メモ「会議」を削除してください。",
});
Console.WriteLine(reply?.Data.Content);
