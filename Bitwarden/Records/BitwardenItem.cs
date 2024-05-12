using System;
using FlowLauncherCommunity.Plugin.Bitwarden.Records;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;

public record BitwardenItem(
    Guid Id,
    string Name,
    bool Favorite,
    BitwardenLogin? Login
) : ParseableRecord<BitwardenItem>;

public record BitwardenItemWithLogin(
    Guid Id,
    string Name,
    bool Favorite,
    BitwardenLogin Login
);
