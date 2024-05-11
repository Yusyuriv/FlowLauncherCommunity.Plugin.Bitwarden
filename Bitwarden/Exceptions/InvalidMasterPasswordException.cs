namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class InvalidMasterPasswordException : BitwardenException {
    public InvalidMasterPasswordException() : base("Invalid master password.") { }
}
