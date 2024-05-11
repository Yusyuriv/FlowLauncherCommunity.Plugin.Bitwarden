namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class NotALoginException : BitwardenException {
    public NotALoginException() : base("Not a login.") { }
}
