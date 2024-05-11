using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Records;

public abstract record ParseableRecord<T> {
    public static T Parse(string json) {
        var record = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        });
        if (record is null) {
            throw new Exception($"Failed to parse {typeof(T).Name}.");
        }
        return record;
    }

    public static T[] ParseArray(string json) {
        var records = JsonSerializer.Deserialize<T[]>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        });
        if (records is null) {
            throw new Exception($"Failed to parse {typeof(T).Name}.");
        }
        return records;
    }
}
