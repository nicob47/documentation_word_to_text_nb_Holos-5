using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Core.Services
{
    public class ResultsSidebarUIState
    {
        /// <summary>
        /// The ListBoxItem selected in the results menu (if any)
        /// </summary>
        public ListBoxItem? SelectedItem { get; set; }

        /// <summary>
        /// When this state was last accessed (for cleanup purposes)
        /// </summary>
        public DateTime LastAccessed { get; set; }

        /// <summary>
        /// Additional UI state that can be preserved (scroll positions, expanded sections, etc.)
        /// </summary>
        public Dictionary<string, object> AdditionalState { get; set; } = new();

        /// <summary>
        /// Helper method to get typed additional state
        /// </summary>
        /// <typeparam name="T">The type to cast to</typeparam>
        /// <param name="key">The state key</param>
        /// <returns>The typed value, or default(T) if not found or wrong type</returns>
        public T? GetAdditionalState<T>(string key)
        {
            if (AdditionalState.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default(T);
        }

        /// <summary>
        /// Helper method to set additional state
        /// </summary>
        /// <param name="key">The state key</param>
        /// <param name="value">The value to store</param>
        public void SetAdditionalState(string key, object value)
        {
            AdditionalState[key] = value;
        }
    }
}
