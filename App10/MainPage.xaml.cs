using App10.Collections;
using App10.Configuration;
using App10.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using App10.SettingsCharms;
using System.Diagnostics;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
namespace App10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string _n { get; set; }

        #region Private Properties

        /// <summary>
        /// Gets / sets the list of the AppBar's navigation items.
        /// </summary>
        private ObservableCollection<ContentPage> NavigationItemList { get; set; }

        /// <summary>
        /// Gets / sets the <see cref="GestureRecognizer"/>.
        /// </summary>
        /// <remarks>
        /// It is needed to give optical feedback when user right-tapped.
        /// </remarks>
        private GestureRecognizer GestureRecognizer { get; set; }

        /// <summary>
        /// Gets / sets a flag indication whether the pointer capture previously held is lost.
        /// </summary>
        private bool PointerCaptureIsLost { get; set; }

        /// <summary>
        /// Gets / sets the <see cref="TransitionCollection"/> of the navigation items in the app bar.
        /// </summary>
        private TransitionCollection NavigationItemTransitionCollection { get; set; }

        #endregion Private Properties


        #region Protected Properties

        /// <summary>
        /// An implementation of <see cref="IObservableMap&lt;String, Object&gt;"/> designed to be
        /// used as a trivial view model.
        /// </summary>
        private IObservableMap<String, Object> DefaultViewModel
        {
            get
            {
                return this.GetValue(DefaultViewModelProperty) as IObservableMap<String, Object>;
            }

            set
            {
                this.SetValue(DefaultViewModelProperty, value);
            }
        }

        #endregion Protected Properties


        #region Public Static Properties

        /// <summary>
        /// Identifies the <see cref="DefaultViewModel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultViewModelProperty =
            DependencyProperty.Register("DefaultViewModel", typeof(IObservableMap<String, Object>), typeof(MainPage), null);

        #endregion Public Static Properties


        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="MainPage"/>.
        /// </summary>
        /// 

        public MainPage()
        {
            this.KeyDown += NoContentGrid_KeyDown;
            this.KeyUp += NoContentGrid_KeyUp;
            // Create an empty default view model
            DefaultViewModel = new ObservableDictionary<String, Object>();

            // Create navigation list
            NavigationItemList = new ObservableCollection<ContentPage>();
            //NavigationItemList = new List<ContentPage>(); // does not work (throws unhandled exception "Value does not fall within the expected range."

            // Initialize the component
            this.InitializeComponent();

            // Initialize the gesture recognition if Right Tap Selection is enabled.
            if (AppConfiguration.Current.SelectTabWithRightTap)
            {
                InitializeGestureRecognition();
            }

            // Setup the tapping event handler
            SetupTapEventHandler();

            // Set the items of the DefaultViewModel
            DefaultViewModel["NavigationItemList"] = NavigationItemList;

            // Register handler for configuration changes.
            AppConfiguration.Current.PropertyChanged += OnConfigurationChanged;
            CoreWindow.GetForCurrentThread().KeyDown += NoContentGrid_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += NoContentGrid_KeyUp;
            // Keep the TransitionCollection for animation when configured
            NavigationItemTransitionCollection = NavigationGridView.ItemContainerTransitions;

            // Remove it from the GridView
            NavigationGridView.ItemContainerTransitions = null;
        }

        private void NoContentGrid_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            Debug.WriteLine(args.VirtualKey.ToString());
        }

        private void NoContentGrid_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Debug.WriteLine(args.VirtualKey.ToString());
        }

        #endregion Public Constructors


        #region Private Methods

        /// <summary>
        /// Initializes the gesture recognition.
        /// </summary>
        private void InitializeGestureRecognition()
        {
            // Create an instance
            GestureRecognizer = new GestureRecognizer();

            // Set the gesture settings. It's Hold, not RightTap what we need.
            GestureRecognizer.GestureSettings |= GestureSettings.Hold;

            // Register the event handler.
            CoreWindow coreWindow = Window.Current.CoreWindow;
            coreWindow.PointerPressed += OnCoreWindowPointerPressed;
            coreWindow.PointerReleased += OnCoreWindowPointerReleased;
            coreWindow.PointerMoved += OnCoreWindowPointerMoved;
            coreWindow.PointerCaptureLost += OnCoreWindowPointerCaptureLost;
        }

        /// <summary>
        /// Uninitializes the gesture recognition.
        /// </summary>
        private void UnintializeGestureRecognition()
        {
            // Release the instance
            GestureRecognizer = null;

            // De-register the event handler.
            CoreWindow coreWindow = Window.Current.CoreWindow;
            coreWindow.PointerPressed -= OnCoreWindowPointerPressed;
            coreWindow.PointerReleased -= OnCoreWindowPointerReleased;
            coreWindow.PointerMoved -= OnCoreWindowPointerMoved;
            coreWindow.PointerCaptureLost += OnCoreWindowPointerCaptureLost;
        }

        /// <summary>
        /// Setup the Tap event handler of the AppBar Navigation GridView depending on the
        /// item selection mode (single / right tapping).
        /// </summary>
        private void SetupTapEventHandler()
        {
            // First, remove all handlers; makes it easy to make sure no handler will be added twice after config changes.
            NavigationGridView.Tapped -= OnNavigationItemSwitchPage;
            NavigationGridView.RightTapped -= OnNavigationItemRightTapped;
            NavigationGridView.DoubleTapped -= OnNavigationItemSwitchPage;

            // Only add the required handlers
            if (AppConfiguration.Current.SelectTabWithRightTap)
            {
                NavigationGridView.Tapped += OnNavigationItemSwitchPage;
                NavigationGridView.RightTapped += OnNavigationItemRightTapped;
            }
            else
            {
                NavigationGridView.DoubleTapped += OnNavigationItemSwitchPage;
            }
        }

        /// <summary>
        /// Invoked when one of the configuration properties was changed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event arguments.</param>
        private void OnConfigurationChanged
          (
          object sender,
          PropertyChangedEventArgs args
          )
        {
            // We only need to do something in case the tab selection mode has changed.
            if (!args.PropertyName.Equals("SelectTabWithRightTap"))
            {
                return;
            }

            SetupTapEventHandler();

            // If right tapping is enabled, but gesture recognition is not present, initializes it.
            if (AppConfiguration.Current.SelectTabWithRightTap
              && GestureRecognizer == null)
            {
                InitializeGestureRecognition();
                return;
            }

            // If right tappung is disabled, but gesture recognition is present, release it.
            if (!AppConfiguration.Current.SelectTabWithRightTap
              && GestureRecognizer != null)
            {
                UnintializeGestureRecognition();
                return;
            }
        }
        /// <summary>
        /// Invoked when an item of the navigation list is right tapped.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event-Arguments.</param>
        private void OnNavigationItemRightTapped
          (
          object sender,
          RightTappedRoutedEventArgs args
          )
        {
            // Get the tapped element.
            FrameworkElement element = args.OriginalSource as FrameworkElement;

            if (element == null)
            {
                return;
            }

            // Extract the data context
            ContentPage page = element.DataContext as ContentPage;

            if (page == null)
            {
                return;
            }

            if (NavigationGridView.SelectedItems.Contains(page))
            {
                NavigationGridView.SelectedItems.Remove(page);
            }
            else
            {
                NavigationGridView.SelectedItems.Add(page);
            }
        }

        /// <summary>
        /// Invoked when the app should switch to another page.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event-Arguments.</param>
        /// <remarks>
        /// Depending on the settings, this handler gets invoked when the item in the AppBar was tapped or double tapped.
        /// Refer to <see cref="SetupTapEventHandler"/> to see when which event will be handled.
        /// </remarks>
        private void OnNavigationItemSwitchPage
          (
          object sender,
          RoutedEventArgs args
          )
        {
            // Get the tapped element.
            FrameworkElement element = args.OriginalSource as FrameworkElement;

            if (element == null)
            {
                return;
            }

            // Extract the data context
            ContentPage page = element.DataContext as ContentPage;

            if (page == null)
            {
                return;
            }

            // Show the selected page.
            ContentFrame.Content = page;

            // Close AppBar
            AppBar.IsOpen = false;
        }

        /// <summary>
        /// Involked when the add button of the AppBar is clicked.
        /// </summary>
        /// <param name="sender">Event's sender.</param>
        /// <param name="args">Event arguments.</param>
        /// 
        private async void add()
        {
            // Add a new page
            // Hide the text block showing no content page is opened.
            NoContentGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // If animation is requested
            if (AppConfiguration.Current.AnimateNewTabs)
            {
                // Start the progess bar
                StartAppBarProgress();
                // Restore navigation item TransitionCollections
                NavigationGridView.ItemContainerTransitions = NavigationItemTransitionCollection;
            }

            // Create new page
            ContentPage page = new ContentPage();

            // Add to list
            NavigationItemList.Add(page);

            // Show
            ContentFrame.Content = page;

            // Changed for Windows 8.1: always have to update the layout of the app bar
            // Refresh the AppBar to reflect the new number of content pages; there will be no transition when it is opened again
            AppBar.UpdateLayout();

            // Do animation stuff if requested
            if (AppConfiguration.Current.AnimateNewTabs)
            {
                // Wait a while so we can see how the new item is added to the navigation AppBar
                await Task.Run(async () => { await Task.Delay(1000); });
            }

            // Always close the AppBar
            AppBar.IsOpen = false;

            // If animation is requested
            if (AppConfiguration.Current.AnimateNewTabs)
            {
                // Stop the AppBar's progress bar
                StopAppBarProgress();
                // Remove navigation item TransitionCollections
                NavigationGridView.ItemContainerTransitions = null;
            }
            f.Visibility = Visibility.Collapsed;
        }
        private void OnAddButtonClicked
          (
          object sender,
          RoutedEventArgs args
          )
        {
            add();
        }

        /// <summary>
        /// Starts the animation of the progress bar in the AppBar. 
        /// </summary>
        private void StartAppBarProgress()
        {
            // Start progress bar
            LoadPageProgressBar.IsIndeterminate = true;

            // Show the progress bar
            LoadPageProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        /// <summary>
        /// Stops the animation of the progress bar in the AppBar.
        /// </summary>
        private void StopAppBarProgress()
        {
            // Hide the progress bar
            LoadPageProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // Stop progress
            LoadPageProgressBar.IsIndeterminate = false;
        }

        /// <summary>
        /// Invoked when the remove button of the AppBar is clicked.
        /// </summary>
        /// <param name="sender">Event's sender.</param>
        /// <param name="args">Event arguments.</param>
        private async void remove()
        {
            // Keep the list of the items to be removed. Closing the AppBar clears this list in grid.
            List<object> selectedItems = NavigationGridView.SelectedItems.ToList();

            // Ask the user to continue
            if (await CancelRemovePages(selectedItems.Count))
            {
                return;
            }

            // Was the current page removed?
            bool isCurrentPageRemoved = false;


            // Remove all pages that are selected.
            foreach (object item in selectedItems)
            {
                ContentPage page = item as ContentPage;

                if (page == null)
                {
                    continue;
                }

                // Check if current page is to be removed.
                if (ContentFrame.Content == page)
                {
                    isCurrentPageRemoved = true;
                }

                // Remove it from the list
                NavigationItemList.Remove(page);
            }

            // If no content page exists any more, show a hint.
            if (NavigationItemList.Count < 1)
            {
                NoContentGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ContentFrame.Content = null;
                return;
            }

            // Set first page as current if current page was removed; there is at minimum one page left.
            if (isCurrentPageRemoved)
            {
                ContentFrame.Content = NavigationItemList[0];
            }
        }
        private void OnRemoveButtonClicked
          (
          object sender,
          RoutedEventArgs args
          )
        {
            remove();
        }

        /// <summary>
        /// Asks the user if removing the pages should be canceled.
        /// </summary>
        /// <param name="numberOfSelectedPages">Number of selected pages to be removed</param>
        /// <returns><c>true</c> if the users has canceled the operation, otherwise <c>false</c>.</returns>
        private async Task<bool> CancelRemovePages
          (
          int numberOfSelectedPages
          )
        {
            // Flag telling us the user canceled the operation.
            bool userCanceled = false;

            // Create the dialog
            MessageDialog dialog = new MessageDialog(String.Format("The selected {0} page(s) will be removed. Continue?", numberOfSelectedPages));

            // Add the commands
            dialog.Commands.Add(new UICommand("Yes"));

            dialog.Commands.Add(new UICommand("No", uiCommandInvokedHandler => { userCanceled = true; }));

            // No is default
            dialog.DefaultCommandIndex = 1;

            // Wait for user input
            await dialog.ShowAsync();

            // Return the User cancelation value
            return (userCanceled);
        }

        /// <summary>
        /// Invoked when the AppBar is unloaded.
        /// </summary>
        /// <param name="sender">Event's sender.</param>
        /// <param name="args">Event arguments.</param>
        private void OnAppBarUnloaded
          (
          object sender,
          RoutedEventArgs args
          )
        {
            // Clear navigation item selection.
            NavigationGridView.SelectedItems.Clear();
        }

        /// <summary>
        /// Invoked when the selection of the navigation item grid has changed.
        /// </summary>
        /// <param name="sender">Event's sender.</param>
        /// <param name="args">Event arguments.</param>
        private void OnNavigationGridSelectionChanged
          (
          object sender,
          SelectionChangedEventArgs args)
        {
            // Just enable / disable Remove button.
            RemoveButton.IsEnabled = NavigationGridView.IsItemSelected();
        }

        /// <summary>
        /// Invoked when the pointer device initiates a Press action.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event arguments.</param>
        /// <remarks>
        /// The handler is only registered when right tapping is set for item selection (<see cref="AppConfiguration.Current.SelectTabWithRightTap"/> is <c>true</c>).
        /// So we do not have to check the flag inside the method.
        /// </remarks>
        private void OnCoreWindowPointerPressed
          (
          object sender,
          PointerEventArgs args
          )
        {
            // In case we've lost the pointer capture, just reset the flag and do nothing.
            // Otherwise, an exception will be thrown that says "Failed to start tracking the pointer, because it is already being tracked".
            if (PointerCaptureIsLost)
            {
                PointerCaptureIsLost = false;
                return;
            }

            // We still have the capture, so handle the event.
            GestureRecognizer.ProcessDownEvent(args.CurrentPoint);
        }

        /// <summary>
        /// Invoked when the pointer device that previously initiated a Press action is released, while within the same element.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event arguments.</param>
        /// <remarks>
        /// The handler is only registered when right tapping is set for item selection (<see cref="AppConfiguration.Current.SelectTabWithRightTap"/> is <c>true</c>).
        /// So we do not have to check the flag inside the method.
        /// </remarks>
        private void OnCoreWindowPointerReleased
          (
          object sender,
          PointerEventArgs args
          )
        {
            GestureRecognizer.ProcessUpEvent(args.CurrentPoint);
        }

        /// <summary>
        /// Invoked when a pointer moves while the pointer remains within the hit test area of the element.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event arguments.</param>
        /// <remarks>
        /// The handler is only registered when right tapping is set for item selection (<see cref="AppConfiguration.Current.SelectTabWithRightTap"/> is <c>true</c>).
        /// So we do not have to check the flag inside the method.
        /// </remarks>
        private void OnCoreWindowPointerMoved
          (
          object sender,
          PointerEventArgs args
          )
        {
            GestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints());
        }

        /// <summary>
        /// Invoked when pointer capture previously held by an element moves to another element or elsewhere.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event arguments.</param>
        /// <remarks>
        /// The handler is only registered when right tapping is set for item selection (<see cref="AppConfiguration.Current.SelectTabWithRightTap"/> is <c>true</c>).
        /// So we do not have to check the flag inside the method.
        /// see http://social.msdn.microsoft.com/Forums/windowsapps/en-US/ceb4a3dc-05f4-4361-bbf7-039310a5d0d3/failed-to-start-tracking-the-pointer-because-it-is-already-being-tracked
        /// </remarks>
        private void OnCoreWindowPointerCaptureLost
          (
          object sender,
          PointerEventArgs args
          )
        {
            // Just set the flag that we've lost the capture.
            PointerCaptureIsLost = true;
        }

        #endregion Private Methods
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
            if (ContentPage.s_PageCounter > 0) ContentPage.s_PageCounter = 0;
        }

        private bool isctrl;
        private void NoContentGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) isctrl = false;
        }
        private void NoContentGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
            {
                isctrl = true;
            }
            else if (isctrl)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.F3: add(); isctrl = false; break;
                }
                isctrl = false;
            }

        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            SettingsCommand defaultsCommand = new SettingsCommand("About", "About",
                (handler) =>
                {
                    SettingsFlyout1 sf = new SettingsFlyout1();
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(defaultsCommand);


        }

    }
}