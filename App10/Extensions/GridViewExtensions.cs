using Windows.UI.Xaml.Controls;

namespace App10.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="GridView"/>.
    /// </summary>
    internal static class GridViewExtensions
    {
        /// <summary>
        /// Checks if at least one item of the GridView is selected.
        /// </summary>
        /// <param name="instance"><see cref="GridView"/> to work with.</param>
        /// <returns><c>true</c> if at least one item is selected, otherwise <c>false</c>.</returns>
        public static bool IsItemSelected
          (
          this GridView instance
          )
        {
            // Are items selected? Also set if only one item is selected.
            if (instance.SelectedItems != null
              && instance.SelectedItems.Count > 0)
            {
                return (true);
            }

            // Nothing is selected.
            return (false);
        }
    }
}
