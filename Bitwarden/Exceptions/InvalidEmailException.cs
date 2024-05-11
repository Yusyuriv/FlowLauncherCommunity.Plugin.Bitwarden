namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class InvalidEmailException : BitwardenException {
    public InvalidEmailException() : base("Invalid email.") { }
}
