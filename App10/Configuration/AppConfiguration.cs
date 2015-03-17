using App10.Common;
using Windows.Storage;

namespace App10.Configuration
{
    /// <summary>
    /// Handles all the configuration stuff.
    /// </summary>
    internal class AppConfiguration : BindableBase
    {
        #region Private Const Data Member

        /// <summary>
        /// Keeps the name of roaming settings key for tab animation.
        /// </summary>
        private const string AnimateTabsKey = "AnimateTab";

        /// <summary>
        /// Keeps the name of roaming settings key for tab selection via right tap.
        /// </summary>
        private const string SelectTabWithRightTapKey = "SelectTabWithRightTap";

        #endregion Private Const Data Member


        #region Private Data Member

        /// <summary>
        /// The flag that indicates whether new tabs should be animated in the App Bar (<c>true</c>) or not.
        /// </summary>
        private bool m_AnimateNewTabs;

        /// <summary>
        /// The flag that indicates whether navigation tabs are selected via right tap (<c>true</c>) or not.
        /// </summary>
        private bool m_SelectTabWithRightTap;

        #endregion Private Data Member


        #region Private Static Data Member

        /// <summary>
        /// Keeps the one and only Instance of this class.
        /// </summary>
        private static AppConfiguration s_Instance = new AppConfiguration();

        #endregion Private Static Data Member


        #region Public Properties

        /// <summary>
        /// Gets / sets the flag that indicates whether new tabs should be animated in the App Bar (<c>true</c>) or not.
        /// </summary>
        public bool AnimateNewTabs
        {
            get { return (m_AnimateNewTabs); }
            set { SetProperty(ref m_AnimateNewTabs, value); }
        }

        /// <summary>
        /// Gets / sets the flag that indicates whether navigation tabs are selected via right tap (<c>true</c>) or not.
        /// </summary>
        public bool SelectTabWithRightTap
        {
            get { return (m_SelectTabWithRightTap); }
            set { SetProperty(ref m_SelectTabWithRightTap, value); }
        }

        #endregion Public Properties


        #region Public Static Properties

        /// <summary>
        /// Provides access to the app's configuration.
        /// </summary>
        public static AppConfiguration Current { get { return (s_Instance); } }

        #endregion Public Static Properties


        #region Private Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AppConfiguration"/>.
        /// </summary>
        private AppConfiguration()
        {
            // Set default value.
            AnimateNewTabs = true;

            // Read it from the roaming settings
            ApplicationDataContainer roamingSettings = GetRoamingSettings();

            // Get the config value
            AnimateNewTabs = (bool)roamingSettings.Values[AppConfiguration.AnimateTabsKey];

            SelectTabWithRightTap = (bool)roamingSettings.Values[AppConfiguration.SelectTabWithRightTapKey];
        }

        #endregion Private Constructors


        #region Private Methods

        /// <summary>
        /// Returns the roaming settings with all keys needed for this app.
        /// </summary>
        /// <returns>Roaming Settings.</returns>
        private ApplicationDataContainer GetRoamingSettings()
        {
            // Get the roaming settings
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

            // If the key does not exits, create it with default value.
            if (!roamingSettings.Values.ContainsKey(AppConfiguration.AnimateTabsKey))
            {
                roamingSettings.Values.Add(AppConfiguration.AnimateTabsKey, true);
            }

            // If the key does not exits, create it with default value.
            if (!roamingSettings.Values.ContainsKey(AppConfiguration.SelectTabWithRightTapKey))
            {
                roamingSettings.Values.Add(AppConfiguration.SelectTabWithRightTapKey, false);
            }

            // Return the settings
            return (roamingSettings);
        }

        #endregion Private Methods


        #region Public Methods

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        public void Save()
        {
            // Get the roaming settings
            ApplicationDataContainer roamingSettings = GetRoamingSettings();

            // Set the values in RoamingSettings; that's it. Properties are set via DataBinding
            roamingSettings.Values[AppConfiguration.AnimateTabsKey] = AnimateNewTabs;

            roamingSettings.Values[AppConfiguration.SelectTabWithRightTapKey] = SelectTabWithRightTap;
        }

        #endregion Public Methods
    }
}
