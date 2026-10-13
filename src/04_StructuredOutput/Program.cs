#pragma warning disable GHCP001 // experimental API
using GitHub.Copilot;

// 構造化出力 (experimental): .NET 型から JSON Schema を生成し、結果を型として受け取る
Console.OutputEncoding = System.Text.Encoding.UTF8;
await using var client = new CopilotClient();
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

var review = await session.SendAndWaitAsync<CodeReview>(
    "次のコードをレビューしてください: for (int i = 0; i <= list.Count; i++) Console.WriteLine(list[i]);");

Console.WriteLine($"Severity: {review.Severity}");
Console.WriteLine($"Summary : {review.Summary}");
foreach (var s in review.Suggestions) Console.WriteLine($" - {s}");

public sealed class CodeReview
{
    public required string Severity { get; set; }
    public required string Summary { get; set; }
    public required List<string> Suggestions { get; set; }
}
