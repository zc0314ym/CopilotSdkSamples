using System.Diagnostics;
using GitHub.Copilot;

// 認証トークンの解決順: 環境変数 -> `gh auth token` -> (null の場合は Copilot CLI のログイン情報を使用)
static class CopilotAuth
{
    public static CopilotClient CreateClient() =>
        new(new CopilotClientOptions { GitHubToken = ResolveToken() });

    static string? ResolveToken()
    {
        foreach (var name in new[] { "COPILOT_GITHUB_TOKEN", "GH_TOKEN", "GITHUB_TOKEN" })
        {
            var v = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        }

        try
        {
            using var p = Process.Start(new ProcessStartInfo("gh", "auth token")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            })!;
            var token = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit(5000);
            if (p.ExitCode == 0 && token.Length > 0) return token;
        }
        catch { /* gh 未インストール */ }

        Console.Error.WriteLine("GitHub トークンが見つかりません。`gh auth login` を実行するか、環境変数 GH_TOKEN を設定してください。");
        return null;
    }
}
