namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class NotLoggedInException : BitwardenException {
    public NotLoggedInException(): base("You are not logged in.") { }
}
