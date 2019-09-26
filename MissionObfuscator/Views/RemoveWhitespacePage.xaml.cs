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
    public sealed partial class RemoveWhitespacePage : Page, INotifyPropertyChanged {
        public static RemoveWhitespacePage Current;

        public RemoveWhitespacePage() {
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

        private async void RemoveWhitespaceButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Button buttonStart = (Button)sender;

            if (RemoveCommentsPage.Current == null || !RemoveCommentsPage.Current.hasRan) {
                PrintText("You must run the remove comments function first.");
                return;
            }

            buttonStart.IsEnabled = false;

            ShellPage.Current.navEnabled(false, typeof(RemoveWhitespacePage));
            await Task.Run(() => removeAllWhitespace());
        }

        private async void removeAllWhitespace() {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRemoveWhitespace.Items.Clear();
            });



            List<StorageFile> allFiles = RemoveCommentsPage.Current.allFiles;

            foreach (StorageFile file in allFiles) {
                if (!file.Path.Contains("chatEvents") && !file.Name.EndsWith(".sqm") && !file.Name.EndsWith(".fsm") && !file.Name.EndsWith(".ext") && !file.Name.EndsWith(".hpp") && !file.Name.EndsWith("macro.h")) {
                    var inputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    var streamReader = new StreamReader(inputStream.AsStreamForRead());

                    string fileContents = streamReader.ReadToEnd();
                    inputStream.Dispose();

                    //fileContents = Regex.Replace(fileContents, ("_fnc_" + oldVarName), , RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    fileContents = Regex.Replace(
                        fileContents,
                        @"(^.*(#define|#include)(?-s).*$)",
                        (match) => {
                            PrintText("match = " + match.ToString().Replace(Environment.NewLine, ""));
                            if (match.Success) {
                                return (Regex.Replace(match.ToString(), @"^((?-s).*)", "$1ALKSJDHYAJS-MAKENEWLINE-----", RegexOptions.Multiline | RegexOptions.IgnoreCase));
                            } else {
                                return "";
                            }
                        },
                        RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase
                    );

                    fileContents = Regex.Replace(fileContents, @" ", "IOQWY-EUKHJLASDNM-ASDKASJHKU-ITGY");
                    fileContents = Regex.Replace(fileContents, @"\s+", "");
                    //fileContents = Regex.Replace(fileContents, @".sqf", ".sqf ");
                    fileContents = Regex.Replace(fileContents, @"IOQWY-EUKHJLASDNM-ASDKASJHKU-ITGY", " ");
                    fileContents = Regex.Replace(fileContents, @"ALKSJDHYAJS-MAKENEWLINE-----", Environment.NewLine);
                    //#include

                    await FileIO.WriteTextAsync(file, fileContents);
                    PrintText("Whitespace removed from " + file.Name);
                }
            }

            PrintText("Complete - All Whitespace removed from files.");
            await Task.Delay(2000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                RemoveWhitespaceButton.IsEnabled = true;
                ShellPage.Current.navEnabled(true, typeof(RemoveWhitespacePage));
            });
        }

        private async void PrintText(string text) {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRemoveWhitespace.Items.Insert(0, text);
            });
        }
    }
}
