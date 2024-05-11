using System.Text.Json.Serialization;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Github.Records;

public record GithubReleaseAsset {
    public string Name { get; init; }
    [JsonPropertyName("browser_download_url")] public string BrowserDownloadUrl { get; init; }
}
