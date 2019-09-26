using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace MissionObfuscator.Views {
    public sealed partial class RenameFilesPage : Page, INotifyPropertyChanged {
        public static RenameFilesPage Current;
        public static Random random = new Random();

        public RenameFilesPage() {
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


        private async void RenameFilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Button buttonStart = (Button)sender;

            if (RemoveCommentsPage.Current == null || !RemoveCommentsPage.Current.hasRan) {
                PrintText("You must run the remove comments function first.");
                return;
            }

            buttonStart.IsEnabled = false;

            ShellPage.Current.navEnabled(false, typeof(RenameFilesPage));
            await Task.Run(() => renameAllFiles());
        }

        private async void renameAllFiles() {
            //Array of all valid SQF files.
            //Array with the original file names, no extension no fn_
            //Array with the random name

            //Do foreach to add original file name to array
            //And to add corresponding random new name
            //Then rename file.

            //Then call a seperate function, or within the same function a renameAllVariables function
            //This one takes the array with all original names and 2nd array with random names
            //Loops through all files including non.sqf ones like description.ext
            //It does a foreach loop within the loop that loops through all valid files.
            //inside the inner foreach loop it does a foreach on each original file name, and replaces what it can with the new random name.

            //Basically what I have now but it works better for whatever reason.

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRenameFiles.Items.Clear();
            });

            List<StorageFile> allFiles = RemoveCommentsPage.Current.allFiles;

            List<string> duplicateFiles = new List<string>();
            List<string> oldFileNames = new List<string>();
            List<string> randomFileNames = new List<string>();
            List<StorageFile> workableFiles = new List<StorageFile>();
            string allNamesAsStrings = "";//We store all names as strings together so we can see if a certain file is part of another functions name.

            PrintText("Generating names and checking for files with same name.");
            foreach (StorageFile fileRename in allFiles) {//We populate the random strings and associate them with their new file.
                string randomFileName = "";
                string oldFileName = "";

                if (fileRename.Name.EndsWith(".sqf") && fileRename.Name.StartsWith("fn_")) {
                    do {
                        randomFileName = RandomString(14);
                    } while (randomFileNames.Contains(randomFileName));
                    oldFileName = fileRename.Name.Substring(3, (fileRename.Name.Length - 7)).ToUpper();

                    if (oldFileNames.Contains(oldFileName)) {//We are going to ignore files that match the name of other files since replacing referenes is troublesome for those.
                        duplicateFiles.Add(oldFileName);
                    } else {
                        if (allNamesAsStrings.Contains(oldFileName)) {
                            duplicateFiles.Add(oldFileName);
                        } else {
                            randomFileNames.Add(randomFileName);
                            oldFileNames.Add(oldFileName);
                            workableFiles.Add(fileRename);
                        }
                    }
                    allNamesAsStrings += (" " + oldFileName + " ");
                }
            }

            PrintText("Beginning rename of all files.");
            for (int i = 0; i < (workableFiles.Count - 1); i++) {
                StorageFile fileToRename = workableFiles[i];

                if (!duplicateFiles.Contains(oldFileNames[i])) {
                    await fileToRename.RenameAsync("fn_" + randomFileNames[i] + ".sqf");//rename the file, the reference StorageFile stays updated.
                    PrintText("fn_" + oldFileNames[i] + ".sqf" + " file renamed to  " + "fn_" + randomFileNames[i] + ".sqf");
                } else {
                    PrintText("Multiple files with the name fn_" + oldFileNames[i] + ".sqf exist and as such do not support obfuscation. Please rename them.");
                }
            }

            //Add section to rename certain files specified in the settings page that are not fn_.sqf files.
            //These files get renamed above and added to the proper arrays. Regex will need to be adjusted for these files.

            PrintText("Beginning rename of all file references in all files.");
            int totalReferencesReplaces = 0;
            for (int i = 0; i < (allFiles.Count - 1); i++) {
                StorageFile file = allFiles[i];

                var inputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                var streamReader = new StreamReader(inputStream.AsStreamForRead());
                string fileContents = streamReader.ReadToEnd();
                inputStream.Dispose();

                //This loop allows all variables in every file to be renamed with the appropriate updated values.
                int replaceCount = 0;
                for (int varLoop = 0; varLoop < (oldFileNames.Count - 1); varLoop++) {
                    string newVarName = randomFileNames[varLoop];
                    string oldVarName = oldFileNames[varLoop];

                    if (!duplicateFiles.Contains(oldVarName)) {//prevents renaming of variables from duplicate files
                        fileContents = Regex.Replace(fileContents, ("_fnc_" + oldVarName), (match) => { replaceCount++; return ("_fnc_" + newVarName); }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        fileContents = Regex.Replace(fileContents, ("fn_" + oldVarName + ".sqf"), (match) => { replaceCount++; return ("fn_" + newVarName + ".sqf"); }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        fileContents = Regex.Replace(fileContents, ("class " + oldVarName + @"([^\S\n]*{|[^\S\n]*;)"), (match) => { replaceCount++; if (match.ToString().EndsWith(";")) { return ("class " + newVarName + ";"); } else { return ("class " + newVarName + "{"); }; }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    }
                }
                await FileIO.WriteTextAsync(file, fileContents);
                PrintText("All " + replaceCount + " file call references renamed in " + file.Name);
                totalReferencesReplaces += replaceCount;
            }

            PrintText("Complete - " + oldFileNames.Count + " files and their " + totalReferencesReplaces + " references renamed. ");
            await Task.Delay(1000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                RenameFilesButton.IsEnabled = true;
                ShellPage.Current.navEnabled(true, typeof(RenameFilesPage));
            });
        }

        public static string RandomString(int length) {
            const string chars = "0O";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async void PrintText(string text) {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListRenameFiles.Items.Insert(0, text);
            });
        }
    }
}
