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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace App10
{
    public sealed partial class SettingsFlyout1 : SettingsFlyout
    {
        public SettingsFlyout1()
        {
            this.InitializeComponent();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=islamtaha@outlook.com&subject=The subject of an email&body= ");
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }

        private async void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            var ur = new Uri("http://www.actiprosoftware.com");
            await Windows.System.Launcher.LaunchUriAsync(ur);
        }
    }
}
