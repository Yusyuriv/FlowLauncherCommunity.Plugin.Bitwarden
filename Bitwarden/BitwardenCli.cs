using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;
using OtpNet;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;

public class BitwardenCli {
    private const string CommandLock = "lock";
    private const string CommandUnlock = "unlock";
    private const string CommandLogIn = "login";
    private const string CommandLogOut = "logout";
    private const string CommandSync = "sync";
    private static readonly string[] CommandGetUsername = { "get", "username" };
    private static readonly string[] CommandGetPassword = { "get", "password" };
    private static readonly string[] CommandGetItem = { "get", "item" };
    private static readonly string[] CommandListItems = { "list", "items" };
    private const string CommandListItemsArgSearch = "--search";
    private const string CommandStatus = "status";

    private const string CommandArgNoInteraction = "--nointeraction";
    private const string CommandArgRaw = "--raw";
    private const string CommandArgHelp = "--help";
    private static readonly string[] CommandArgMethodTotp = { "--method", "0", "--code" };

    private const string ErrorVaultIsLocked = "Vault is locked.";
    private const string ErrorInvalidMasterPassword = "Invalid master password.";
    private const string ErrorInvalidEmail = "Email address is invalid.";
    private const string ErrorNotALogin = "Not a login.";
    private const string ErrorNoUsername = "No username available for this login.";
    private const string ErrorNoPassword = "No password available for this login.";
    private const string ErrorNoTotp = "No TOTP available for this login.";
    private const string ErrorYouAreNotLoggedIn = "You are not logged in.";

    private static readonly string[] VerificationStrings = {
        "Commands:",
        "Examples:",
        "bw login",
        "bw lock",
        "bw list",
        "Bitwarden",
    };

    public bool HasBeenVerified { get; private set; } = false;

    private string _exePath = string.Empty;

    public string ExePath {
        get => _exePath;
        set {
            _exePath = value;
            HasBeenVerified = false;
        }
    }

    private string? _session = null;

    public async Task VerifyExePath() {
        if (string.IsNullOrWhiteSpace(ExePath)) {
            HasBeenVerified = false;
            return;
        }

        try {
            var response = await CreateAndRunProcess(CommandArgHelp);
            HasBeenVerified = VerificationStrings.All(v => response.Contains(v));
        } catch {
            HasBeenVerified = false;
        }
    }

    public async Task Lock() {
        await CreateAndRunProcess(CommandLock);
        _session = null;
    }

    public async Task Unlock(string password) {
        _session = await CreateAndRunProcess(CommandUnlock, password);
    }

    public async Task LogIn(string email, string password, string totp) {
        var totpArgs = new List<string> {
            CommandLogIn,
            email,
            password,
        };
        if (!string.IsNullOrEmpty(totp)) {
            totpArgs.AddRange(CommandArgMethodTotp);
            totpArgs.Add(totp);
        }
        _session = await CreateAndRunProcess(totpArgs.ToArray());
    }

    public async Task LogOut() {
        await CreateAndRunProcess(CommandLogOut);
        _session = null;
    }

    public async Task Sync() {
        await CreateAndRunProcess(CommandSync);
    }

    public async Task<BitwardenItem> GetItem(Guid id) {
        var json = await CreateAndRunProcess(CommandGetItem, id.ToString());
        var item = BitwardenItem.Parse(json);
        if (item is null) {
            throw new Exception("Failed to parse Bitwarden item.");
        }
        return item;
    }

    public async Task<string> GetUsername(Guid id) {
        return await CreateAndRunProcess(CommandGetUsername, id.ToString());
    }

    public async Task<string> GetPassword(Guid id) {
        return await CreateAndRunProcess(CommandGetPassword, id.ToString());
    }

    public async Task<string> GetTotp(Guid id) {
        var item = await CreateAndRunProcess(CommandGetItem, id.ToString());
        var totpSecret = BitwardenItem.Parse(item).Login?.Totp;
        if (string.IsNullOrWhiteSpace(totpSecret)) return "";
        var bytes = Base32Encoding.ToBytes(totpSecret);
        var totp = new Totp(bytes);
        return totp.ComputeTotp();
    }

    public async Task<BitwardenItemWithLogin[]> ListItems(string search = "") {
        var args = string.IsNullOrWhiteSpace(search) switch {
            true => CommandListItems,
            false => CommandListItems.Concat(new []{ CommandListItemsArgSearch, search}).ToArray(),
        };
        var json = await CreateAndRunProcess(args);
        var items = BitwardenItem.ParseArray(json);
        return items
            .Where(v => v.Login is not null)
            .Select(
                v => new BitwardenItemWithLogin(
                    v.Id,
                    v.Name,
                    v.Favorite,
                    v.Login!
                )
            )
            .ToArray();
    }

    public async Task<EBitwardenStatus> GetStatus() {
        var json = await CreateAndRunProcess(CommandStatus);
        return BitwardenStatus.Parse(json).Status;
    }

    private ProcessStartInfo CreateProcess(IEnumerable<string> args) {
        var startInfo = new ProcessStartInfo {
            FileName = ExePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in args) {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.ArgumentList.Add(CommandArgNoInteraction);
        startInfo.ArgumentList.Add(CommandArgRaw);
        if (!string.IsNullOrEmpty(_session)) {
            startInfo.EnvironmentVariables["BW_SESSION"] = _session;
        }

        return startInfo;
    }

    private Task<string> CreateAndRunProcess(string[] command, params string[] args) {
        return CreateAndRunProcess(command.Concat(args).ToArray());
    }

    private async Task<string> CreateAndRunProcess(params string[] args) {
        var startInfo = CreateProcess(args);
        var process = Process.Start(startInfo);
        var task = process?.WaitForExitAsync();
        if (task is not null) await task;
        if (process is null) {
            throw new NotImplementedException();
        }

        if (process.ExitCode is 0) return await process.StandardOutput.ReadToEndAsync();

        var message = await process.StandardError.ReadToEndAsync();
        if (message.Contains(ErrorVaultIsLocked)) throw new VaultIsLockedException();
        if (message.Contains(ErrorInvalidMasterPassword)) throw new InvalidMasterPasswordException();
        if (message.Contains(ErrorInvalidEmail)) throw new InvalidEmailException();
        if (message.Contains(ErrorNotALogin)) throw new NotALoginException();
        if (message.Contains(ErrorNoUsername)) throw new NoUsernameException();
        if (message.Contains(ErrorNoPassword)) throw new NoPasswordException();
        if (message.Contains(ErrorNoTotp)) throw new NoTotpException();
        if (message.Contains(ErrorYouAreNotLoggedIn)) throw new NotLoggedInException();

        throw new BitwardenException(message);
    }
}
