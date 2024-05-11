using System.Text.Json.Serialization;
using FlowLauncherCommunity.Plugin.Bitwarden.Records;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Github.Records;

public record GithubRelease : ParseableRecord<GithubRelease> {
    [JsonPropertyName("tag_name")] public string TagName { get; init; }
    public string Name { get; init; }
    public bool Draft { get; init; }
    public bool Prerelease { get; init; }
    [JsonPropertyName("created_at")] public string CreatedAt { get; init; }
    [JsonPropertyName("published_at")] public string PublishedAt { get; init; }
    public GithubReleaseAsset[] Assets { get; init; }
}
