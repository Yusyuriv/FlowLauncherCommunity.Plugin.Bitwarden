using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden.Exceptions;

namespace FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

public partial class LogInViewModel : INotifyPropertyChanged {
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _totp = string.Empty;
    private readonly BitwardenCli _cli;
    private readonly Action _afterLogin;

    public bool UnlockOnly { get; }
    public string ButtonText => UnlockOnly ? "Unlock" : "Log in";

    public LogInViewModel(BitwardenCli cli, Action afterLogin, bool unlockOnly = false) {
        _cli = cli;
        _afterLogin = afterLogin;
        UnlockOnly = unlockOnly;
    }

    public string Username {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Password {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string Totp {
        get => _totp;
        set => SetField(ref _totp, value);
    }

    [RelayCommand]
    private async Task LogInOrUnlock() {
        try {
            if (UnlockOnly) {
                await _cli.Unlock(_password);
            } else {
                await _cli.LogIn(_username, _password, _totp);
            }

            _afterLogin();
        } catch (BitwardenException e) {
            MessageBox.Show(e.Message);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}
