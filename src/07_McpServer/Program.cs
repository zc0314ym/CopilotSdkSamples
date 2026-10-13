using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

// 自作 MCP サーバー (stdio)。08_McpClient から Copilot SDK 経由で呼び出される。
// stdout は MCP プロトコル専用のため、ログは必ず stderr に出す。
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SampleTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public sealed class SampleTools
{
    [McpServerTool(Name = "add_numbers"), Description("2 つの数値を足し算する")]
    public static double Add([Description("1 つ目の数値")] double a, [Description("2 つ目の数値")] double b) => a + b;

    [McpServerTool(Name = "get_server_time"), Description("MCP サーバーのマシンの現在時刻 (ISO 8601) を返す")]
    public static string GetServerTime() => DateTimeOffset.Now.ToString("O");

    [McpServerTool(Name = "lookup_employee"), Description("社員番号から社員情報を返す (デモ用の固定データ)")]
    public static object LookupEmployee([Description("社員番号 (例: E001)")] string id) => id switch
    {
        "E001" => new { id, name = "山田 太郎", department = "開発部" },
        "E002" => new { id, name = "鈴木 花子", department = "営業部" },
        _ => new { id, name = "(該当なし)", department = "" },
    };
}
