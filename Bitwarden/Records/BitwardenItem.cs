using System;
using FlowLauncherCommunity.Plugin.Bitwarden.Records;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;

public record BitwardenItem(
    DateTime RevisionDate,
    DateTime CreationDate,
    DateTime? DeletedDate,
    string Object,
    Guid Id,
    Guid? OrganizationId,
    Guid? FolderId,
    string Name,
    string? Notes,
    bool Favorite,
    BitwardenLogin Login,
    Guid[] CollectionIds
) : ParseableRecord<BitwardenItem>;
