using MissionObfuscator.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace MissionObfuscator.Views {
    public sealed partial class WelcomePage : Page, INotifyPropertyChanged {
        public List<StorageFile> allFiles = new List<StorageFile>();

        public WelcomePage() {
            InitializeComponent();
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

        private async void AddDiagButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Button buttonStart = (Button)sender;
            buttonStart.IsEnabled = false;

            if (RemoveCommentsPage.Current == null || !RemoveCommentsPage.Current.hasRan) {
                PrintText("You must run the remove comments function first.");
                return;
            }

            ShellPage.Current.navEnabled(false, typeof(WelcomePage));
            await Task.Run(() => addDiagInformation());
        }

        private async void addDiagInformation() {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListMainPage.Items.Clear();
            });

            List<StorageFile> allFiles = RemoveCommentsPage.Current.allFiles;

            allFiles.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));
            PrintText(allFiles.Count.ToString() + " Total files capable of some obfuscation.");

            foreach (StorageFile file in allFiles) {
                if (file.Path.Contains("life_server") && file.Name.EndsWith(".sqf") && file.Name.StartsWith("fn_") && !file.Name.Contains("fn_govMaint.sqf") && !file.Name.Contains("fn_asyncQuery.sqf") && !file.Name.Contains("fn_queryConfig.sqf") && !file.Name.Contains("fn_bool.sqf") && !file.Name.Contains("fn_numberSafe.sqf")) {

                    var inputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    var streamReader = new StreamReader(inputStream.AsStreamForRead());

                    string fileContents = streamReader.ReadToEnd();
                    inputStream.Dispose();

                    /*

                    	/////////
		private _myScriptName = 'noScriptName';
		if(!(isNil '_fnc_scriptName')) then{
			_myScriptName = [_fnc_scriptName] param[0,'',['']];
		};
		private _myThisScript = 'noScriptHandle';
		if(canSuspend) then {
			private _myThisScript = [str(_thisScript)] param[0,'',['']];
		};

		private _myScriptNameParent = [_fnc_scriptNameParent] param[0,'',['']];

		if(!(isNil 'life_monitorFunctions') && !(_myScriptName isEqualTo '') && {!(_myScriptName in life_monitorFunctions)}) exitWith {
			life_monitorFunctions pushBack _myScriptName;
			private _scriptMonitorIdentifier = format['%1_%2_%3', _myScriptName, diag_frameNo, random(100000)];
			[_this, (!((_myThisScript find _myScriptName) isEqualTo -1)), _myScriptName, _myScriptNameParent, _scriptMonitorIdentifier, diag_tickTime] call life_fnc_monitorScriptHandle;
		};
	////////

                    */

                    fileContents = Regex.Replace(fileContents, @"^\A(.*)$", ("		private _myScriptName = 'noScriptName';		if(!(isNil '_fnc_scriptName')) then{			_myScriptName = [_fnc_scriptName] param[0,'',['']];		};		private _myThisScript = 'noScriptHandle';		if(canSuspend) then {			private _myThisScript = [str(_thisScript)] param[0,'',['']];		};		private _myScriptNameParent = [_fnc_scriptNameParent] param[0,'',['']];		if(!(isNil 'life_monitorFunctions') && !(_myScriptName isEqualTo '') && {!(_myScriptName in life_monitorFunctions)}) exitWith {			life_monitorFunctions pushBack _myScriptName;			private _scriptMonitorIdentifier = format['%1_%2_%3', _myScriptName, diag_frameNo, random(100000)];			[_this, (!((_myThisScript find _myScriptName) isEqualTo -1)), _myScriptName, _myScriptNameParent, _scriptMonitorIdentifier, diag_tickTime] call life_fnc_monitorScriptHandle;		};" + Environment.NewLine + "$1"), RegexOptions.Multiline);
                    //fileContents = Regex.Replace(fileContents, "`", "");

                    await FileIO.WriteTextAsync(file, fileContents);
                    PrintText("Debug logging added to " + file.Name);
                }
            }

            PrintText("Complete - Debugging information added to all capable server files.");
            await Task.Delay(2000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                AddDiagButton.IsEnabled = true;
                ShellPage.Current.navEnabled(true, typeof(WelcomePage));
            });
        }

        private async void PrintText(string text) {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                ListMainPage.Items.Insert(0, text);
            });
        }
    }
}
