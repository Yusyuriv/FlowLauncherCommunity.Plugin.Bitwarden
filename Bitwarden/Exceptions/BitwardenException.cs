using System;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class BitwardenException : Exception {
    public BitwardenException(string message) : base(message) { }
}
