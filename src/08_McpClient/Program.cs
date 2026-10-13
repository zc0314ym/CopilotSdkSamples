using GitHub.Copilot;

// MCP クライアント: 07_McpServer を stdio の MCP サーバーとして Copilot セッションに登録する
Console.OutputEncoding = System.Text.Encoding.UTF8;

// bin\<構成>\<TFM>\ から 07_McpServer のビルド成果物を探す
var baseDir = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd('\\', '/'));
var tfm = baseDir.Name;
var config = baseDir.Parent!.Name;
var srcDir = baseDir.Parent.Parent!.Parent!.Parent!.FullName;
var serverDll = Path.Combine(srcDir, "07_McpServer", "bin", config, tfm, "07_McpServer.dll");
if (!File.Exists(serverDll))
{
    Console.Error.WriteLine($"MCP サーバーが見つかりません: {serverDll}");
    return;
}

await using var client = CopilotAuth.CreateClient();
await client.StartAsync();

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    McpServers = new Dictionary<string, McpServerConfig>
    {
        ["demo"] = new McpStdioServerConfig
        {
            Command = "dotnet",
            Args = new List<string> { serverDll },
            Tools = new List<string> { "*" }, // "*" = 全ツール、[] = なし、または名前を列挙
        },

        // リモート (HTTP) MCP サーバーの例。使う場合はコメントを外してトークンを設定する。
        // ["github"] = new McpHttpServerConfig
        // {
        //     Url = "https://api.githubcopilot.com/mcp/",
        //     Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer <token>" },
        //     Tools = new List<string> { "*" },
        // },
    },
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

// ツール名は "<サーバー名>-<ツール名>" になるため、実行されたツールを表示する
session.On<SessionEvent>(evt =>
{
    if (evt is ToolExecutionStartEvent start)
        Console.WriteLine($"[tool] {start.Data.ToolName}");
});

var reply = await session.SendAndWaitAsync(new MessageOptions
{
    Prompt = "MCP の demo サーバーのツールを使って、(1) 12.5 と 30 の合計、(2) 社員番号 E001 の社員情報、(3) サーバーの現在時刻 を教えてください。",
});
Console.WriteLine(reply?.Data.Content);
