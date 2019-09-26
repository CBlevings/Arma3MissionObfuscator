using MissionObfuscator.Helpers;
using MissionObfuscator.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace MissionObfuscator.Views {
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings-codebehind.md
    // TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged {
        private string _ClientFolderSelected;

        public string ClientFolderSelected {
            get { return _ClientFolderSelected; }
            set { Set(ref _ClientFolderSelected, (string)value); }
        }

        private string _ServerFolderSelected;

        public string ServerFolderSelected {
            get { return _ServerFolderSelected; }
            set { Set(ref _ServerFolderSelected, (string)value); }
        }

        private string _ClientFolderSelectedFA;

        public string ClientFolderSelectedFA {
            get { return _ClientFolderSelectedFA; }
            set { Set(ref _ClientFolderSelectedFA, (string)value); }
        }

        private string _ServerFolderSelectedFA;

        public string ServerFolderSelectedFA {
            get { return _ServerFolderSelectedFA; }
            set { Set(ref _ServerFolderSelectedFA, (string)value); }
        }


        private string _OutputFolderSelected;

        public string OutputFolderSelected {
            get { return _OutputFolderSelected; }
            set { Set(ref _OutputFolderSelected, (string)value); }
        }

        private string _OutputFolderSelectedFA;

        public string OutputFolderSelectedFA {
            get { return _OutputFolderSelectedFA; }
            set { Set(ref _OutputFolderSelectedFA, (string)value); }
        }


        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme {
            get => _elementTheme;

            set => Set(ref _elementTheme, value);
        }

        private string _versionDescription;

        public string VersionDescription {
            get => _versionDescription;

            set => Set(ref _versionDescription, value);
        }

        public SettingsPage() {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            await InitializeAsync();
            ClientFolderSelected = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ClientFolderSelected));
            ServerFolderSelected = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ServerFolderSelected));
            ClientFolderSelectedFA = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ClientFolderSelectedFA));//File access tokens
            ServerFolderSelectedFA = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(ServerFolderSelectedFA));
            OutputFolderSelected = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(OutputFolderSelected));
            OutputFolderSelectedFA = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(OutputFolderSelectedFA));//File access tokens
        }

        private async void ClientFolderTextBox_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null) {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(folder);

                textBox.Text = folder.Path;
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(ClientFolderSelected), folder.Path);
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(ClientFolderSelectedFA), faToken);
            }

            if (StorageApplicationPermissions.FutureAccessList.CheckAccess(folder)) {
                Debug.WriteLine("Access to client granted");
            } else {
                Debug.WriteLine("Access to client is denied.");
            }
        }

        private async void ServerFolderTextBox_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null) {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(folder);

                textBox.Text = folder.Path;
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(ServerFolderSelected), folder.Path);
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(ServerFolderSelectedFA), faToken);
            }

            if (StorageApplicationPermissions.FutureAccessList.CheckAccess(folder)) {
                Debug.WriteLine("Access to server folder granted");
            } else {
                Debug.WriteLine("Access to server folder is denied.");
            }
        }

        private async void OutputFolderSelected_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null) {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(folder);

                textBox.Text = folder.Path;
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(OutputFolderSelected), folder.Path);
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(OutputFolderSelectedFA), faToken);
            }

            if (StorageApplicationPermissions.FutureAccessList.CheckAccess(folder)) {
                Debug.WriteLine("Access to server folder granted");
            } else {
                Debug.WriteLine("Access to server folder is denied.");
            }
        }

        private async Task InitializeAsync() {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription() {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e) {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null) {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null) {
            if (Equals(storage, value)) {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
