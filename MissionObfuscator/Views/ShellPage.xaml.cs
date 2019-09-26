using MissionObfuscator.Helpers;
using MissionObfuscator.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;

namespace MissionObfuscator.Views {
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page, INotifyPropertyChanged {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);
        public static ShellPage Current;

        private bool _isBackEnabled;
        private WinUI.NavigationViewItem _selected;

        public bool IsBackEnabled {
            get => _isBackEnabled;
            set => Set(ref _isBackEnabled, value);
        }

        public WinUI.NavigationViewItem Selected {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ShellPage() {
            InitializeComponent();
            DataContext = this;
            Current = this;
            HideNavViewBackButton();
            Initialize();
        }

        public void navEnabled(bool enabled, Type activePage) {
            WinUI.NavigationViewItem selectedItem = (WinUI.NavigationViewItem)navigationView.SettingsItem;
            selectedItem.IsEnabled = enabled;

            if (!enabled)
                HideNavViewBackButton();

            foreach (object item in navigationView.MenuItems) {
                selectedItem = (WinUI.NavigationViewItem)item;
                if (!IsMenuItemForPageType(selectedItem, activePage))
                    selectedItem.IsEnabled = enabled;
            }
        }

        private void HideNavViewBackButton() {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6)) {
                navigationView.IsBackButtonVisible = WinUI.NavigationViewBackButtonVisible.Collapsed;
            }
        }

        private void Initialize() {
            NavigationService.Frame = shellFrame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            navigationView.BackRequested += OnBackRequested;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            KeyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            KeyboardAccelerators.Add(_backKeyboardAccelerator);
            await Task.CompletedTask;
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e) {
            IsBackEnabled = false; //NavigationService.CanGoBack; -- Disable nav back button
            if (e.SourcePageType == typeof(SettingsPage)) {
                Selected = navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            Selected = navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType) {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        private void OnItemInvoked(WinUI.NavigationView sender, WinUI.NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                NavigationService.Navigate(typeof(SettingsPage));
                return;
            }

            var item = navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
            NavigationService.Navigate(pageType);
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args) {
            NavigationService.GoBack();
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null) {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue) {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
            var result = NavigationService.GoBack();
            args.Handled = result;
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
