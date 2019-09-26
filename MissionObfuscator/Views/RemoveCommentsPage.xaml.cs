using MissionObfuscator.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MissionObfuscator.Views {
    public sealed partial class RemoveCommentsPage : Page, INotifyPropertyChanged {
        public static RemoveCommentsPage Current;
        public bool hasRan = false;
        public List<StorageFile> allFiles = new List<StorageFile>();

        public RemoveCommentsPage() {
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

        /*
         *
         *
         *
         *  Search ^\A(.*)$
            Replace with Word $1

            Select the last line of the text-file ^(.*)$\z

            insert word at the end of file
            Search ^(.*)$\z
            Replace with $1 Word
         *
         *
         *
         *	Obfuscation issue where space got remove here: _newStringArrayset//this will be fixed by ignoring that file.

	        Add substr.sqf to the ignore list

	        Other issue with random \ in a file. (	File: fn_sellHouse.sqf) no idea why thats there

	        Change file renaming back to regualr A-Z
	
	        ignore all sqf files in C:\Users\Poseidon\Desktop\AltisLifeDev\AsylumClient\Altis_life.Altis\core\chatEvents
	        create a custom setting for storing these as an array.
         *
         *
         */

        private async void RemoveCommentsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Button buttonStart = (Button)sender;
            buttonStart.IsEnabled = false;

            StorageFolder clientFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(await ApplicationData.Current.LocalSettings.ReadAsync<string>("ClientFolderSelectedFA"));
            StorageFolder serverFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(await ApplicationData.Current.LocalSettings.ReadAsync<string>("ServerFolderSelectedFA"));
            StorageFolder outputFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(await ApplicationData.Current.LocalSettings.ReadAsync<string>("OutputFolderSelectedFA"));

            PrintText("Preparing obfuscator by moving files and populating file lists.");
            PrintText("--Copying client folder to output directory.");
            await CopyFolderAsync(clientFolder, outputFolder);
            PrintText("--Copying server folder to output directory.");
            await CopyFolderAsync(serverFolder, outputFolder);
            PrintText("--Move complete.");

            ShellPage.Current.navEnabled(false, typeof(RemoveCommentsPage));
            await Task.Run(() => removeAllComments(outputFolder));
        }

        public static async Task CopyFolderAsync(StorageFolder source, StorageFolder destinationContainer, string desiredName = null) {
            StorageFolder destinationFolder = null;
            destinationFolder = await destinationContainer.CreateFolderAsync(
                desiredName ?? source.Name, CreationCollisionOption.ReplaceExisting);

            foreach (var file in await source.GetFilesAsync()) {
                await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting);
            }
            foreach (var folder in await source.GetFoldersAsync()) {
                await CopyFolderAsync(folder, destinationFolder);
            }
        }

        private async void removeAllComments(StorageFolder outputStorage) {
            if (!RemoveCommentsPage.Current.hasRan) {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    List1.Items.Clear();
                });

                List<StorageFolder> foldersRemaining = new List<StorageFolder>();
                foldersRemaining.Add(outputStorage);

                PrintText("Populating file lists....");
                while (foldersRemaining.Count > 0) {
                    IReadOnlyList<IStorageItem> currentItems = await foldersRemaining[0].GetItemsAsync();
                    foldersRemaining.RemoveAt(0);

                    foreach (IStorageItem file in currentItems) {
                        if (file.IsOfType(StorageItemTypes.Folder)) {
                            foldersRemaining.Add((StorageFolder)file);
                        } else {
                            if (file.Name.EndsWith(".sqf") || file.Name.EndsWith(".txt") || file.Name.EndsWith(".ext") || file.Name.EndsWith(".hpp") || file.Name.EndsWith(".h") || file.Name.EndsWith(".sqm") || file.Name.EndsWith(".fsm") || file.Name.EndsWith(".cpp")) {
                                allFiles.Add((StorageFile)file);
                            }
                        }
                    }
                }
            }

            allFiles.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));
            PrintText(allFiles.Count.ToString() + " Total files capable of some obfuscation.");

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            foreach (StorageFile file in allFiles) {
                if (!file.Name.EndsWith(".sqm") && !file.Name.EndsWith(".fsm") && !file.Name.EndsWith(".ext")) {//dont remove comments or slashes from sqm/fsm
                    var inputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    var streamReader = new StreamReader(inputStream.AsStreamForRead());

                    string fileContents = streamReader.ReadToEnd();
                    inputStream.Dispose();

                    fileContents = Regex.Replace(fileContents, @"://", "IOQWY-WEBSITEHTTPFIXTHINGY-ITGY");//quick fix for regex thinking http:// are comments.
                    fileContents = Regex.Replace(fileContents, blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, me => { if (me.Value.StartsWith("/*") || me.Value.StartsWith("//")) return me.Value.StartsWith("//") ? Environment.NewLine : ""; return me.Value; }, RegexOptions.Singleline);
                    fileContents = Regex.Replace(fileContents, @"IOQWY-WEBSITEHTTPFIXTHINGY-ITGY", "://");

                    await FileIO.WriteTextAsync(file, fileContents);
                    PrintText("Comments removed from " + file.Name);
                    if (file.Equals(allFiles[(allFiles.Count - 1)])) {
                        PrintText("Complete - All comments removed from files.");
                        await Task.Delay(1000);
                        RemoveCommentsPage.Current.hasRan = true;

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                            RemoveCommentsButton.IsEnabled = true;
                            ShellPage.Current.navEnabled(true, typeof(RemoveCommentsPage));
                        });
                    }
                }
            }
        }

        private async void PrintText(string text) {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                List1.Items.Insert(0, text);
            });
        }



    }
}
