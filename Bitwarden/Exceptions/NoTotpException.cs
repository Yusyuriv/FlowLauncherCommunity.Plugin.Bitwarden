namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class NoTotpException : BitwardenException {
    public NoTotpException() : base("No TOTP found for this login.") { }
}
