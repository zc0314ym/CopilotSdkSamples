using System.ComponentModel;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

// カスタムツール: モデルが必要に応じて自前の C# 関数を呼び出す
Console.OutputEncoding = System.Text.Encoding.UTF8;
await using var client = new CopilotClient();
await client.StartAsync();

var getWeather = CopilotTool.DefineTool(
    ([Description("都市名")] string city) =>
        new { city, temperatureC = 22, condition = "晴れ" }, // 実際は外部 API 等を呼ぶ
    factoryOptions: new AIFunctionFactoryOptions
    {
        Name = "get_weather",
        Description = "指定した都市の現在の天気を返す",
    },
    toolOptions: new CopilotToolOptions { SkipPermission = true }); // 読み取り専用なので確認不要

await using var session = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5",
    Tools = [getWeather],
    OnPermissionRequest = PermissionHandler.ApproveAll,
});

var reply = await session.SendAndWaitAsync(new MessageOptions { Prompt = "東京の天気を教えてください。" });
Console.WriteLine(reply?.Data.Content);
