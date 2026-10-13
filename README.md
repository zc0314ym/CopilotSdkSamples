# GitHub Copilot SDK for .NET サンプル集

C# から GitHub Copilot をプログラム制御する [`GitHub.Copilot.SDK`](https://www.nuget.org/packages/GitHub.Copilot.SDK) の機能解説とサンプルです。

## 概要

SDK は同梱の **Copilot CLI ランタイム** を子プロセスとして起動し、JSON-RPC で通信します。
アプリ側は `CopilotClient` → `CopilotSession` を介してエージェントと会話し、ツール実行・ファイル操作などを任せられます。

```
あなたのアプリ (C#) ── SDK ── Copilot CLI ランタイム ── Copilot / BYOK プロバイダー
```

## 前提条件

- .NET 10 SDK（SDK 自体は .NET Standard 2.0 互換）
- GitHub Copilot のサブスクリプション
- 認証（必須）: 未認証だと `No GitHub OAuth token or Copilot HMAC key provided` になります。
  `src/CopilotAuth.cs` が次の順でトークンを解決します。
  1. 環境変数 `COPILOT_GITHUB_TOKEN` / `GH_TOKEN` / `GITHUB_TOKEN`
  2. `gh auth token`（事前に `gh auth login` を実行）
  3. 上記が無ければ Copilot CLI のログイン情報（`CopilotClientOptions.UseLoggedInUser`）

  Visual Studio から実行する場合は、`gh auth login` を済ませるか、環境変数を設定して VS を再起動してください。

## 主な機能

| 機能 | 主な API | サンプル |
|---|---|---|
| クライアント/接続管理 | `CopilotClient`, `CopilotClientOptions`, `RuntimeConnection.ForStdio/ForTcp/ForUri` | 全サンプル |
| 会話セッション | `CreateSessionAsync`, `SendAsync`, `SendAndWaitAsync`, `On<SessionEvent>` | [01_BasicChat](src/01_BasicChat) |
| ストリーミング | `SessionConfig.Streaming`, `AssistantMessageDeltaEvent` | [02_Streaming](src/02_Streaming) |
| カスタムツール | `CopilotTool.DefineTool`, `CopilotToolOptions`（`SkipPermission` / `OverridesBuiltInTool`） | [03_CustomTool](src/03_CustomTool) |
| 構造化出力 (experimental) | `SendAndWaitAsync<T>`, `MessageOptions.ResponseSchema` | [04_StructuredOutput](src/04_StructuredOutput) |
| セッション永続化/再開 | `ResumeSessionAsync`, `ListSessionsAsync`, `DeleteSessionAsync` | [05_SessionResume](src/05_SessionResume) |
| 権限制御 | `OnPermissionRequest`, `PermissionHandler.ApproveAll` | 全サンプル |
| Infinite Sessions | `InfiniteSessionConfig`（コンテキスト自動圧縮） | – |
| システムプロンプト調整 | `SystemMessageConfig`（Append / Customize / Replace） | – |
| 添付 | `AttachmentFile`, `AttachmentBlob`（画像対応） | – |
| スラッシュコマンド | `SessionConfig.Commands` | – |
| UI Elicitation | `session.Ui.ConfirmAsync/SelectAsync/InputAsync` | – |
| BYOK | `SessionConfig.Provider`（OpenAI 互換など自前キー） | – |
| フック / MCP / カスタムエージェント | `Hooks`, `McpServers`, `CustomAgents` | – |
| メモリ | `MemoryConfiguration` | – |
| テレメトリ | `CopilotClientOptions.Telemetry`（OpenTelemetry） | – |

### イベントモデル
セッションは `UserMessageEvent` / `AssistantMessageEvent` / `ToolExecutionStartEvent` / `ToolExecutionCompleteEvent` / `SessionIdleEvent` / `SessionErrorEvent` などを発行します。
`SessionIdleEvent` が「処理完了」の合図です。単純な往復は `SendAndWaitAsync` が便利です。

### 権限制御
ファイル書き込みやシェル実行などは `OnPermissionRequest` で承認/拒否します。サンプルでは簡略化のため `ApproveAll` を使っていますが、**本番では必ず内容を検査**してください。

## 実行方法

```powershell
dotnet build
dotnet run --project src\01_BasicChat
dotnet run --project src\02_Streaming
dotnet run --project src\03_CustomTool
dotnet run --project src\04_StructuredOutput
dotnet run --project src\05_SessionResume
```

Visual Studio では `CopilotSdkSamples.sln` を開き、起動プロジェクトを選んで実行します。

## 参考

- <https://github.com/github/copilot-sdk>
- <https://github.com/github/copilot-sdk/blob/main/dotnet/README.md>
