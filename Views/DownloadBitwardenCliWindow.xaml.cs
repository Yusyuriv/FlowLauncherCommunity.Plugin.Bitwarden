using System;
using System.ComponentModel;
using System.Windows;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Views;

public partial class DownloadBitwardenCliWindow : Window {
    private readonly DownloadBitwardenCliViewModel _viewModel;
    public DownloadBitwardenCliWindow(BitwardenCli cli, Settings settings) {
        InitializeComponent();
        _viewModel = new DownloadBitwardenCliViewModel(cli, settings, SaveDataAndClose);
        DataContext = _viewModel;
    }

    private void SaveDataAndClose() {
        DialogResult = true;
        Close();
    }

    private void DownloadBitwardenCliWindow_OnClosing(object? sender, CancelEventArgs e) {
        if (_viewModel.IsInteractive) return;
        e.Cancel = true;
    }
}
