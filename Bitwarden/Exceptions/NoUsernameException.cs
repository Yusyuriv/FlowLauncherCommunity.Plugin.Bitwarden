namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class NoUsernameException : BitwardenException {
    public NoUsernameException() : base("No username found for this login.") { }
}
