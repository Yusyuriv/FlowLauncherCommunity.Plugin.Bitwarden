namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

public class VaultIsLockedException : BitwardenException {
    public VaultIsLockedException() : base("Vault is locked.") { }
}
