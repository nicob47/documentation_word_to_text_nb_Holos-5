using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.Avalonia.Views.ResultViews;
using Prism.Events;

namespace H.Avalonia.Events
{
    /// <summary>
    /// Used to notify when a chapter is selected in the basic chapter selection menu in <see cref="ResultsSidebarView"/>, passing the chapter's unique identifier (string) as the event payload to <see cref="ResultsSummaryView"/>.
    /// </summary>
    public class BasicChapterSelectedEvent : PubSubEvent<string>
    {
    }
}
