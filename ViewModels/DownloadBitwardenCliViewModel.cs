using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowLauncherCommunity.Plugin.Bitwarden.Bitwarden;
using Microsoft.Win32;

namespace FlowLauncherCommunity.Plugin.Bitwarden.ViewModels;

public partial class DownloadBitwardenCliViewModel : ObservableObject
{
    private const string DownloadUrl = "https://vault.bitwarden.com/download/?app=cli&platform=windows";
    private const string DownloadExeName = "bw.exe";

    private readonly Settings _settings;
    private readonly BitwardenCli _cli;
    private readonly Action _close;

    [ObservableProperty]
    private string _exePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonText))]
    private bool _isInteractive = true;
    public string DownloadButtonText => IsInteractive ? "Download automatically" : "Cancel download";

    [ObservableProperty]
    private Visibility _progressBarVisibility = Visibility.Hidden;

    [ObservableProperty]
    private int _progressBarValue;

    [ObservableProperty]
    private bool _progressBarIsIndeterminate = true;

    [ObservableProperty]
    private bool _isDownloadButtonInteractive = true;

    private CancellationTokenSource _cancellationTokenSource = new();

    public DownloadBitwardenCliViewModel(BitwardenCli cli, Settings settings, Action close) {
        _cli = cli;
        _settings = settings;
        _close = close;
        _exePath = settings.ExePath;
    }

    [RelayCommand]
    private void Cancel() {
        _close();
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Download() {
        if (!IsInteractive) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            IsInteractive = true;
            return;
        }
        var cancellationToken = _cancellationTokenSource.Token;
        IsInteractive = false;
        ProgressBarVisibility = Visibility.Visible;
        ProgressBarValue = 0;
        ProgressBarIsIndeterminate = true;
        try {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(DownloadUrl, cancellationToken: cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken);

            var totalBytes = response.Content.Headers.ContentLength ?? 0L;
            if (totalBytes == 0L) return;

            var tempArchivePath = Path.GetTempFileName();
            await using var fileStream = File.Create(tempArchivePath);

            var buffer = new byte[8192];
            var totalRead = 0L;

            ProgressBarIsIndeterminate = false;
            while (true) {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken: cancellationToken);
                if (bytesRead == 0) break;

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken: cancellationToken);

                totalRead += bytesRead;
                await Application.Current.Dispatcher.InvokeAsync(
                    () => ProgressBarValue = (int)(totalRead * 100 / totalBytes),
                    DispatcherPriority.Background,
                    cancellationToken: cancellationToken
                );
            }

            ProgressBarIsIndeterminate = true;

            fileStream.Close();
            cancellationToken.ThrowIfCancellationRequested();
            using var archive = ZipFile.OpenRead(tempArchivePath);
            if (archive.Entries is not [{ Name: "bw.exe" }]) return;
            var tempExePath = Path.GetTempFileName();
            cancellationToken.ThrowIfCancellationRequested();
            archive.Entries[0].ExtractToFile(tempExePath, true);
            cancellationToken.ThrowIfCancellationRequested();
            var pluginLocation = Path.GetDirectoryName(typeof(DownloadBitwardenCliViewModel).Assembly.Location);
            if (pluginLocation is null) return;
            var exePath = Path.Combine(pluginLocation, DownloadExeName);
            await fileStream.DisposeAsync();
            archive.Dispose();
            File.Delete(tempArchivePath);
            File.Move(tempExePath, exePath, true);
            cancellationToken.ThrowIfCancellationRequested();
            ExePath = exePath;
            _settings.ExePath = exePath;
            _cli.ExePath = exePath;
            await _cli.VerifyExePath();
            IsInteractive = true;
            if (_cli.HasBeenVerified) _close();
        } catch (TaskCanceledException) {
            // ignored
        } finally {
            ProgressBarValue = 0;
            ProgressBarVisibility = Visibility.Hidden;
            IsInteractive = true;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    [RelayCommand]
    private void Browse() {
        var dialog = new OpenFileDialog();
        if (dialog.ShowDialog() is true) {
            ExePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task Save() {
        IsInteractive = false;
        IsDownloadButtonInteractive = false;
        var oldExePath = _cli.ExePath;
        _cli.ExePath = ExePath;
        try {
            await _cli.VerifyExePath();
        } catch {
            // ignore
        }

        IsInteractive = true;
        IsDownloadButtonInteractive = true;
        if (_cli.HasBeenVerified) {
            _settings.ExePath = ExePath;
            _close();
        } else {
            _cli.ExePath = oldExePath;
            MessageBox.Show("Invalid path. Please try again.");
        }
    }
}
