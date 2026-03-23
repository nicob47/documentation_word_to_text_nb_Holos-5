using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Core.Services
{
    /// <summary>
    /// Represents the UI state for the results sidebar that needs to be preserved across ViewModel disposal cycles.
    /// </summary>
    public class ResultsSidebarUIState : UIStateBase
    {
        /// <summary>
        /// The ListBoxItem selected in the results menu (if any)
        /// </summary>
        public ListBoxItem? SelectedItem { get; set; }

        /// <summary>
        /// When this state was last accessed (for cleanup purposes)
        /// </summary>
        public DateTime LastAccessed { get; set; }
    }
}
