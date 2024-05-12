using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Records;
using FlowLauncherCommunity.Plugin.Bitwarden.Views;
using FlowLauncherCommunity.Plugin.Bitwarden.Windows;

namespace FlowLauncherCommunity.Plugin.Bitwarden;

public class Main : IAsyncPlugin, ISettingProvider, IContextMenu {
    private const string IcoPath = "icon.png";
    private PluginInitContext _context = null!;
    private Settings _settings = null!;
    private BitwardenCli _cli = null!;

    private Result _resultNoCli = null!;
    private Result _resultAuthenticate = null!;
    private Result _resultUnlockVault = null!;
    private Result _resultSyncVault = null!;
    private Result _resultLockVault = null!;
    private Result _resultLogOut = null!;


    private Result _resultExceptionInvalidEmail = null!;
    private Result _resultExceptionInvalidMasterPassword = null!;
    private Result _resultExceptionVaultIsLocked = null!;
    private Result _resultExceptionNotALogin = null!;
    private Result _resultExceptionNoUsername = null!;
    private Result _resultExceptionNoPassword = null!;
    private Result _resultExceptionNoTotp = null!;
    private Result _resultExceptionNotLoggedIn = null!;

    private readonly StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

    public async Task InitAsync(PluginInitContext context) {
        _context = context;
        _settings = _context.API.LoadSettingJsonStorage<Settings>();
        _cli = new BitwardenCli { ExePath = _settings.ExePath };
        InitializeActionResults();
        await _cli.VerifyExePath();
    }

    private void IncludeResultIfItMatches(List<Result> list, Result result, string search) {
        if (string.IsNullOrEmpty(search) ||
            result.Title.Contains(search, _stringComparison) ||
            result.SubTitle.Contains(search, _stringComparison)
           ) {
            list.Add(result);
        }
    }

    private void IncludeBitwardenItemIfItMatches(List<Result> list, BitwardenItemWithLogin item, string search) {
        if (!string.IsNullOrEmpty(search) &&
            !item.Login.Username.Contains(search, _stringComparison) &&
            !item.Name.Contains(search, _stringComparison)) return;

        var favorite = item.Favorite ? "⭐" : string.Empty;
        var icon = IcoPath;
        if (item.Login.Uris.FirstOrDefault() is { } uriObj) {
            var uri = new Uri(uriObj.Uri);
            icon = $"https://icons.bitwarden.net/{uri.Host}/icon.png";
        }
        list.Add(new Result {
            Title = favorite + item.Name,
            SubTitle = item.Login.Username,
            IcoPath = icon,
            ContextData = item.Id,
            Action = _ => {
                return true;
            },
        });
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token) {
        if (!_cli.HasBeenVerified) {
            return new List<Result> { _resultNoCli };
        }

        // await Task.Delay(500, token);
        var status = await _cli.GetStatus();

        try {
            var list = new List<Result>();
            switch (status) {
                case EBitwardenStatus.Unauthenticated:
                    list.Add(_resultAuthenticate);
                    break;
                case EBitwardenStatus.Locked:
                    IncludeResultIfItMatches(list, _resultUnlockVault, query.Search);
                    IncludeResultIfItMatches(list, _resultSyncVault, query.Search);
                    IncludeResultIfItMatches(list, _resultLogOut, query.Search);
                    break;
                case EBitwardenStatus.Unlocked:
                    IncludeResultIfItMatches(list, _resultLockVault, query.Search);
                    IncludeResultIfItMatches(list, _resultSyncVault, query.Search);
                    IncludeResultIfItMatches(list, _resultLogOut, query.Search);
                    var items = (await _cli.ListItems())
                        .OrderBy(v => v.Favorite)
                        .ThenBy(v => v.Name);
                    foreach (var item in items) {
                        IncludeBitwardenItemIfItMatches(list, item, query.Search);
                    }
                    break;
            }
            return list;
        } catch (InvalidEmailException) {
            return new List<Result> { _resultExceptionInvalidEmail };
        } catch (InvalidMasterPasswordException) {
            return new List<Result> { _resultExceptionInvalidMasterPassword };
        } catch (VaultIsLockedException) {
            return new List<Result> { _resultExceptionVaultIsLocked };
        } catch (NotALoginException) {
            return new List<Result> { _resultExceptionNotALogin };
        } catch (NoUsernameException) {
            return new List<Result> { _resultExceptionNoUsername };
        } catch (NoPasswordException) {
            return new List<Result> { _resultExceptionNoPassword };
        } catch (NoTotpException) {
            return new List<Result> { _resultExceptionNoTotp };
        } catch (NotLoggedInException) {
            return new List<Result> { _resultExceptionNotLoggedIn };
        } catch (BitwardenException e) {
            return new List<Result> {
                new Result {
                    Title = "An error occurred",
                    SubTitle = e.Message,
                    IcoPath = IcoPath,
                },
            };
        }
    }

    private void InitializeActionResults() {
        _resultNoCli = new Result {
            Title = "Bitwarden CLI not found",
            SubTitle = "Select this result to open the download window.",
            IcoPath = IcoPath,
            Action = _ => {
                new DownloadBitwardenCliWindow(_cli, _settings).ShowDialog();
                return true;
            },
        };
        _resultAuthenticate = new Result {
            Title = "Not authenticated",
            SubTitle = "Select this result to authenticate.",
            IcoPath = IcoPath,
            Action = _ => {
                new LogInWindow(_cli).ShowDialog();
                return true;
            },
        };
        _resultUnlockVault = new Result {
            Title = "Unlock vault",
            IcoPath = IcoPath,
            Action = _ => {
                new LogInWindow(_cli, true).ShowDialog();
                return true;
            },
        };
        _resultSyncVault = new Result {
            Title = "Sync vault",
            IcoPath = IcoPath,
            AsyncAction = async _ => {
                await _cli.Sync();
                return true;
            },
        };
        _resultLockVault = new Result {
            Title = "Lock vault",
            IcoPath = IcoPath,
            AsyncAction = async _ => {
                await _cli.Lock();
                return true;
            },
        };
        _resultLogOut = new Result {
            Title = "Log out",
            IcoPath = IcoPath,
            AsyncAction = async _ => {
                await _cli.LogOut();
                return true;
            },
        };
        _resultExceptionInvalidEmail = new Result {
            Title = "Invalid email",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionInvalidMasterPassword = new Result {
            Title = "Invalid master password",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionVaultIsLocked = new Result {
            Title = "Vault is locked",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionNotALogin = new Result {
            Title = "Not a login",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionNoUsername = new Result {
            Title = "No username",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionNoPassword = new Result {
            Title = "No password",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionNoTotp = new Result {
            Title = "No TOTP",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
        _resultExceptionNotLoggedIn = new Result {
            Title = "Not logged in",
            IcoPath = IcoPath,
            Action = _ => {
                return true;
            },
        };
    }

    public Control CreateSettingPanel() {
        return new SettingsControl(_cli, _settings);
    }

    public List<Result> LoadContextMenus(Result selectedResult) {
        if (selectedResult.ContextData is not Guid id) return new List<Result>();
        return new List<Result> {
            new Result {
                Title = $"{selectedResult.Title} — {selectedResult.SubTitle}",
                SubTitle = "Copy username",
                IcoPath = selectedResult.IcoPath,
                AsyncAction = async _ => {
                    var username = await _cli.GetUsername(id);
                    _context.API.CopyToClipboard(username);
                    return true;
                },
            },
            new Result {
                Title = $"{selectedResult.Title} — {selectedResult.SubTitle}",
                SubTitle = "Copy password",
                IcoPath = selectedResult.IcoPath,
                AsyncAction = async _ => {
                    var password = await _cli.GetPassword(id);
                    _context.API.CopyToClipboard(password);
                    return true;
                },
            },
        };
    }
}
