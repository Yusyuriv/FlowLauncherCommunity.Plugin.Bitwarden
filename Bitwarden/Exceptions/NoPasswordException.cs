namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class NoPasswordException : BitwardenException {
    public NoPasswordException() : base("No password found for this login.") { }
}
