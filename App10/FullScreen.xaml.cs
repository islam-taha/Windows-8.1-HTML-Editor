﻿using App10.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ActiproSoftware.Text.Lexing;
using ActiproSoftware.Text.Languages.Xml;
using ActiproSoftware.Text.Languages.Xml.Implementation;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Searching;
using ActiproSoftware.UI.Xaml.Controls.SyntaxEditor;
using ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.EditActions;
using ActiproSoftware.Compatibility;
using ActiproSoftware.UI.Xaml.Extensions;
using ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.Highlighting.Implementation;
using System.Text;
using Windows.UI.ApplicationSettings;
using System.Xml.Linq;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.UI.Text;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Interop;
using ActiproSoftware.Text;
using App10.SettingsCharms;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace App10
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class FullScreen : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public string _n { get; set; }
        string fileToken = string.Empty;
        private bool hasPendingParseData;
        private ISearchResultSet lastResultSet;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
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
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public FullScreen()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            searchView.SyntaxEditor = pg;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null) pg.Text = (String)e.Parameter;
            this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void GoTo()
        {
            // Validate
            int lineIndex;
            if ((!int.TryParse(lineNumberTextBox.Text, out lineIndex)) || (lineIndex < 1) || (lineIndex > pg.ActiveView.CurrentSnapshot.Lines.Count))
            {
                MessageDialog md = new MessageDialog("Please enter a valid line number");
                await md.ShowAsync();
            }

            try
            {
                // Set caret position (make zero-based)
                pg.Caret.Position = new TextPosition(lineIndex - 1, 0);
                pg.ActiveView.Scroller.ScrollLineToVisibleMiddle();
                // Focus the editor
                pg.Focus(FocusState.Programmatic);
            }
            catch (Exception)
            {

            }
        }
        private void OnGoToLineButtonClick(object sender, RoutedEventArgs e)
        {
            GoTo();
        }
        private void BoldClick(object sender, RoutedEventArgs e)
        {
            pg.FontStyle = Windows.UI.Text.FontStyle.Normal;
        }

        private void OnErrorListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            IParseError error = listBox.SelectedItem as IParseError;
            if (error != null)
            {
                pg.ActiveView.Selection.StartPosition = error.PositionRange.StartPosition;
                pg.Focus(FocusState.Programmatic);
            }
        }
        private void OnFindResultsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Quit if there is not result set stored yet
            if (lastResultSet == null)
                return;

            ListBox control = sender as ListBox;
            if (control == null)
                return;

            // Account for first line in results displaying search info
            int resultIndex = control.SelectedIndex - 1;

            if ((resultIndex >= 0) && (resultIndex < lastResultSet.Results.Count))
            {
                // A valid result was clicked
                ISearchResult result = lastResultSet.Results[resultIndex];
                if (result.ReplaceSnapshotRange.IsDeleted)
                {
                    // Find result
                    pg.ActiveView.Selection.SelectRange(result.FindSnapshotRange.TranslateTo(pg.ActiveView.CurrentSnapshot, TextRangeTrackingModes.Default).TextRange);
                }
                else
                {
                    // Replace result
                    pg.ActiveView.Selection.SelectRange(result.ReplaceSnapshotRange.TranslateTo(pg.ActiveView.CurrentSnapshot, TextRangeTrackingModes.Default).TextRange);
                }

                // Focus the editor
                pg.Focus(FocusState.Programmatic);
            }
        }
        private void OnEditorViewSearch(object sender, EditorViewSearchEventArgs e)
        {
            this.UpdateResults(e.ResultSet);
        }

        private void ItalicClick(object sender, RoutedEventArgs e)
        {
            pg.FontStyle = FontStyle.Italic;
        }

        private async void save()
        {
            try
            {
                if (String.IsNullOrEmpty(fileToken))
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    savePicker.FileTypeChoices.Add("HTML", new List<string>() { ".html" });
                    savePicker.SuggestedFileName = "New File";
                    StorageFile file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                        fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        await FileIO.WriteTextAsync(file, pg.Text);
                        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    }
                }
                else
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(fileToken);
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    await FileIO.WriteTextAsync(file, pg.Text);
                    // Let Windows know that we're finished changing the file so the server app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    switch (status)
                    {
                        case FileUpdateStatus.Complete:
                            MessageDialog md = new MessageDialog("Saved Successfully");
                            await md.ShowAsync();
                            break;
                        case FileUpdateStatus.CompleteAndRenamed:
                            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(fileToken, file);
                            break;
                        default:
                            MessageDialog s = new MessageDialog("Saving failed");
                            await s.ShowAsync(); break;
                    }

                }
            }
            catch (Exception) { }
        }
        private async void open()
        {
            try
            {
                FileSavePicker save = new FileSavePicker();
                if (pg.Text != null && !String.IsNullOrEmpty(fileToken))
                {
                    MessageDialog md = new MessageDialog("Won't you save your work");
                    await md.ShowAsync();
                }
                //define the file open Picker to pick a file
                FileOpenPicker openPicker = new FileOpenPicker();
                //filtering the files appears in the picker
                openPicker.FileTypeFilter.Add(".html");
                //picking single file from the file picker
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                    fileToken = (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file));
                if (file != null && file.FileType.Equals(".html"))
                {
                    string text = await FileIO.ReadTextAsync(file);
                    pg.Text = text;
                }

                else if (file != null && file.FileType.Equals(".html"))
                {
                    string text = await FileIO.ReadTextAsync(file);
                    pg.Text = text;
                }

                else if (file != null)
                {
                    pg.Text = await FileIO.ReadTextAsync(file);
                }
            }
            catch (Exception) { }
        }
        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            save();
        }
        private void OnSyntaxEditorDocumentParseDataChanged(object sender, EventArgs e)
        {
            hasPendingParseData = true;
        }
        private void OnSyntaxEditorUserInterfaceUpdate(object sender, object e)
        {
            // If there is a pending parse data change...
            if (hasPendingParseData)
            {
                // Clear flag
                hasPendingParseData = false;

                XmlParseData parseData = pg.Document.ParseData as XmlParseData;

                if (parseData != null)
                {
                    // Output errors
                    errorListBox.ItemsSource = parseData.Errors;

                    // Show well-formed state
                    messagePanel.Content = String.Format("Well-formed: {0}", parseData.IsWellFormed ? "Yes" : "No");
                }
                else
                {
                    // Clear UI
                    errorListBox.ItemsSource = null;
                    messagePanel.Content = null;
                }

                // Update the error count
            }
        }
        private void OnSyntaxEditorViewSelectionChanged(object sender, EditorViewSelectionEventArgs e)
        {
            // Quit if this event is not for the active view
            if (!e.View.IsActive)
                return;

            // Update line, col, and character display
            linePanel.Text = String.Format("Ln {0}", e.CaretPosition.DisplayLine);
            columnPanel.Text = String.Format("Col {0}", e.CaretDisplayCharacterColumn);
            characterPanel.Text = String.Format("Ch {0}", e.CaretPosition.DisplayCharacter);
        }
        private void UpdateResults(ISearchResultSet resultSet)
        {
            // Show the results

            StringBuilder text = new StringBuilder();

            // Line 1
            switch (resultSet.OperationType)
            {
                case SearchOperationType.FindAll:
                    text.Append("Find all ");
                    break;
                case SearchOperationType.FindNext:
                    text.Append("Find next ");
                    break;
                case SearchOperationType.ReplaceAll:
                    text.Append("Replace all ");
                    break;
                case SearchOperationType.ReplaceNext:
                    text.Append("Replace next ");
                    break;
            }
            text.Append("\"");
            text.Append(resultSet.Options.FindText.Split(new char[] { '\r', '\n' })[0]);
            switch (resultSet.OperationType)
            {
                case SearchOperationType.ReplaceAll:
                case SearchOperationType.ReplaceNext:
                    text.Append(", \"");
                    text.Append(resultSet.Options.ReplaceText.Split(new char[] { '\r', '\n' })[0]);
                    break;
            }
            text.Append("\"");
            if (resultSet.Wrapped)
                text.Append(" (wrapped)");

            findResultsListBox.Items.Clear();
            findResultsListBox.Items.Add(text.ToString());

            // Results lines
            foreach (ISearchResult result in resultSet.Results)
            {
                if (result.ReplaceSnapshotRange.IsDeleted)
                {
                    // Find result
                    findResultsListBox.Items.Add(String.Format("({0},{1}): {2}", result.FindSnapshotRange.StartPosition.Line + 1,
                    result.FindSnapshotRange.StartPosition.Character + 1, result.FindSnapshotRange.StartLine.Text));
                }
                else
                {
                    // Replace result
                    findResultsListBox.Items.Add(String.Format("({0},{1}): {2}", result.ReplaceSnapshotRange.StartPosition.Line + 1,
                        result.ReplaceSnapshotRange.StartPosition.Character + 1, result.ReplaceSnapshotRange.StartLine.Text));
                }
            }

            // Save the result set
            lastResultSet = resultSet;
        }
        // Open file button click 
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            open();
        }

        private async void OnNewButtonClick(object sender, RoutedEventArgs e)
        {
            //messgae dialog to remeber the user to save his work
            MessageDialog md = new MessageDialog("Won't you save your work!");

            md.Commands.Add(new UICommand("OK Wait :)"));
            md.Commands.Add(new UICommand("No Thanks :)", (command) =>
            {
                pg.Document.SetText(null);
                fileToken = string.Empty;
            }));
            await md.ShowAsync();
        }

        private async void brsee()
        {
            // create a folder in picture library
            await KnownFolders.PicturesLibrary.CreateFolderAsync("HTML-LAB", CreationCollisionOption.OpenIfExists);
            //assign the folder to an object 
            StorageFolder folder = await KnownFolders.PicturesLibrary.GetFolderAsync("HTML-LAB");
            // create a file to run in the browser
            StorageFile f = await folder.CreateFileAsync("swapFile.html", CreationCollisionOption.ReplaceExisting);
            //copy the text to the created file
            await FileIO.WriteTextAsync(f, pg.Text);
            await Windows.System.Launcher.LaunchFileAsync(f);
        }
        //run your code in a larger screen of the runtime browser 
        private void brse_Click(object sender, RoutedEventArgs e)
        {
            brsee();
        }

        private void lineNUmberButton_Checked(object sender, RoutedEventArgs e)
        {
            pg.IsLineNumberMarginVisible = true;
        }

        private void toggle_uncheck(object sender, RoutedEventArgs e)
        {
            pg.IsLineNumberMarginVisible = false;
        }

        private bool isCtrlKeyPressed;
        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = false;
        }
        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = true;
            else if (isCtrlKeyPressed)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.O: open(); isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.F:
                        pg.ActiveView.ExecuteEditAction(new FormatDocumentAction());
                        pg.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                        isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.I: pg.FontStyle = FontStyle.Italic; isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.B: brsee(); isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.N: pg.FontStyle = Windows.UI.Text.FontStyle.Normal; isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.E: isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.F1: pg.IsLineNumberMarginVisible = true; lineNumberButton.IsChecked = true; isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.F2: pg.IsLineNumberMarginVisible = false; lineNumberButton.IsChecked = false; isCtrlKeyPressed = false; break;
                    case Windows.System.VirtualKey.K: this.Frame.Navigate(typeof(ContentPage), pg.Text); isCtrlKeyPressed = false; break;
                }
                isCtrlKeyPressed = false;
            }
        }

        private void OnBackToWindowsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ContentPage), pg.Text);
        }

        private void OnFormatButton_Click(object sender, RoutedEventArgs e)
        {
            pg.ActiveView.ExecuteEditAction(new FormatDocumentAction());
            pg.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
    }
}