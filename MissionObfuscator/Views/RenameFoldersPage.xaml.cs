using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace MissionObfuscator.Views {
    public sealed partial class RenameFoldersPage : Page, INotifyPropertyChanged {
        public static RenameFoldersPage Current;

        public RenameFoldersPage() {
            InitializeComponent();
            Current = this;
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

        private async void RenameFoldersButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Button buttonStart = (Button)sender;

            if (RemoveCommentsPage.Current == null || !RemoveCommentsPage.Current.hasRan) {
                PrintText("Feature not implemented.");
                return;
            }

            buttonStart.IsEnabled = false;

            //ShellPage.Current.navEnabled(false, typeof(RenameFoldersPage));
            await Task.Run(() => renameAllFolders());
        }

        private async void renameAllFolders() {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRenameFolders.Items.Clear();
            });

            PrintText("Feature not implemented.");
        }

        private async void PrintText(string text) {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRenameFolders.Items.Insert(0, text);
            });
        }
    }
}
