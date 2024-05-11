using System;
using FlowLauncherCommunity.Plugin.Bitwarden.Records;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;

public record BitwardenStatus(
    string? ServerUrl,
    DateTime? LastSync,
    string? UserEmail,
    Guid? UserId,
    EBitwardenStatus Status
) : ParseableRecord<BitwardenStatus>;
