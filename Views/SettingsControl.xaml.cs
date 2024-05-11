using System.Windows.Controls;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

namespace FlowLauncherCommunity.Plugin.Bitwarden.Views;

public partial class SettingsControl : UserControl {
    public SettingsControl(BitwardenCli cli, Settings settings) {
        InitializeComponent();
        DataContext = new SettingsViewModel(cli, settings);
    }
}
