using System.Windows;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Windows;

public partial class LogInWindow : Window {
    private readonly BitwardenCli _cli;
    private readonly LogInViewModel _viewModel;

    public LogInWindow(BitwardenCli cli, bool unlockOnly = false) {
        _cli = cli;
        InitializeComponent();
        _viewModel = new LogInViewModel(_cli, Close, unlockOnly);
        DataContext = _viewModel;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) {
        _viewModel.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
    }
}
