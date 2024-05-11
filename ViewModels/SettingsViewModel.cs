using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.Views;
using FlowLauncherCommunity.Plugin.Bitwarden.Windows;

namespace FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

public partial class SettingsViewModel : INotifyPropertyChanged {
    private readonly Settings _settings;
    private readonly BitwardenCli _cli;

    public SettingsViewModel(BitwardenCli cli, Settings settings) {
        _cli = cli;
        _settings = settings;
    }

    public string ExePath => _settings.ExePath;

    [RelayCommand]
    private void ChangeExecutablePath() {
        var dialogResult = new DownloadBitwardenCliWindow(_cli, _settings).ShowDialog();
        if (dialogResult is true)
            OnPropertyChanged(nameof(ExePath));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
