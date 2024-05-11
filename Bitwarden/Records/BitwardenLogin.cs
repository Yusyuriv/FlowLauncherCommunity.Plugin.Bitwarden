using System;
using FlowLauncherCommunity.Plugin.Bitwarden.Records;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;

public record BitwardenLogin(
    BitwardenUri[] Uris,
    string Username,
    string Password,
    string? Totp,
    DateTime? PasswordRevisionDate
) : ParseableRecord<BitwardenLogin>;
