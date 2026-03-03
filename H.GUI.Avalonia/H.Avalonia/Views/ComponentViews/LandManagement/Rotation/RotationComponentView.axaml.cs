using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation;
using System;

namespace H.Avalonia.Views.ComponentViews.LandManagement.Rotation;

public partial class RotationComponentView : UserControl
{
    #region Fields

    private DispatcherTimer? _scrollTimer;
    private double _scrollStartOffset;
    private double _scrollTargetOffset;
    private DateTime _scrollStartTime;
    private const int ScrollDurationMs = 600; // Gentle 600ms scroll duration

    #endregion

    #region Properties

    /// <summary>
    /// Avalonia styled property for controlling the visibility of advanced configuration options.
    /// This property is registered with Avalonia's property system to enable data binding
    /// and is bound to the AdvancedOptionsToggleView control in the XAML.
    /// </summary>
    public static readonly StyledProperty<bool> ShowAdvancedOptionsProperty =
        AvaloniaProperty.Register<RotationComponentView, bool>(nameof(ShowAdvancedOptions), defaultValue: false);

    #endregion

    #region Constructors

    // ==================================================================================
    // INITIALIZATION
    // ==================================================================================
    // Sets up the view, initializes components, and subscribes to necessary events.
    // ==================================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="RotationComponentView"/> class.
    /// Sets up event handlers for DataContext changes to enable automatic scrolling
    /// when users select crops for editing.
    /// </summary>
    public RotationComponentView()
    {
        InitializeComponent();
        
        // Set default value for design time - makes it easier to see all options in the designer
        if (Design.IsDesignMode)
        {
            ShowAdvancedOptions = true;
        }

        // Subscribe to DataContext changes to handle ViewModel lifecycle events
        // This allows us to properly attach/detach event handlers when the ViewModel changes
        this.DataContextChanged += OnDataContextChanged;
    }

    #endregion

    #region Propertie

    /// <summary>
    /// Gets or sets whether advanced options should be displayed in the rotation configuration UI.
    /// This controls the visibility of additional settings that are useful for power users
    /// but may overwhelm new users.
    /// </summary>
    public bool ShowAdvancedOptions
    {
        get => GetValue(ShowAdvancedOptionsProperty);
        set => SetValue(ShowAdvancedOptionsProperty, value);
    }

    #endregion

    #region Private Methods

    // ==================================================================================
    // EVENT HANDLERS - VIEWMODEL LIFECYCLE
    // ==================================================================================
    // Manages the subscription to ViewModel property changes to enable automatic
    // scrolling behavior when users interact with the crop rotation grid.
    // ==================================================================================

    /// <summary>
    /// Handles changes to the DataContext (ViewModel) of this view.
    /// Unsubscribes from the old ViewModel's PropertyChanged event and subscribes to the new one.
    /// This ensures we don't have memory leaks and that we always respond to the active ViewModel.
    /// </summary>
    /// <param name="sender">The source of the event (this view).</param>
    /// <param name="e">Event arguments.</param>
    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        // Unsubscribe from old ViewModel if it exists
        // This prevents memory leaks and ensures we don't respond to changes from inactive ViewModels
        if (sender is RotationComponentView view && view.DataContext is RotationComponentViewModel oldViewModel)
        {
            oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // Subscribe to new ViewModel's property changes
        // This allows us to respond to changes in the ViewModel's state, particularly crop selection
        if (this.DataContext is RotationComponentViewModel newViewModel)
        {
            newViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    // ==================================================================================
    // EVENT HANDLERS - AUTO-SCROLL BEHAVIOR
    // ==================================================================================
    // Implements smart scrolling that automatically brings Step 4 (crop editing section)
    // into view when a user clicks on a crop in the preview grid (Step 3).
    // ==================================================================================

    /// <summary>
    /// Handles property changes in the ViewModel to trigger automatic scrolling behavior.
    /// When a user clicks on a crop cell in the preview grid (Step 3), this method
    /// automatically scrolls the view to show the crop editing section (Step 4).
    /// The scrolling only occurs when explicitly triggered by grid cell selection,
    /// not when the selection changes programmatically.
    /// </summary>
    /// <param name="sender">The ViewModel that raised the PropertyChanged event.</param>
    /// <param name="e">Event arguments containing the name of the changed property.</param>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Check if the SelectedCropDto property changed - this indicates a crop was selected for editing
        if (e.PropertyName == nameof(RotationComponentViewModel.SelectedCropDto))
        {
            var viewModel = sender as RotationComponentViewModel;
            
            // Only scroll if:
            // 1. A crop is actually selected (not null)
            // 2. The selection came from a grid cell click (ShouldTriggerAutoScroll flag is true)
            // This prevents unwanted scrolling when crops are selected programmatically
            if (viewModel?.SelectedCropDto != null && viewModel.ShouldTriggerAutoScroll)
            {
                // Capture to local variable for use in closure
                var vm = viewModel;

                // Use Dispatcher to ensure the UI layout has been updated before scrolling
                // This prevents scrolling to the wrong position due to pending layout changes
                Dispatcher.UIThread.Post(() =>
                {
                    ScrollToBottomSmooth();

                    // Reset the auto-scroll flag to prevent scrolling on subsequent programmatic changes
                    vm.ShouldTriggerAutoScroll = false;
                }, DispatcherPriority.Background);
            }
        }
    }

    // ==================================================================================
    // SCROLL MANAGEMENT
    // ==================================================================================
    // Helper methods for controlling the scroll position of the main content area.
    // ==================================================================================

    /// <summary>
    /// Scrolls the main content area to the bottom, bringing Step 4 (crop editing section) into view.
    /// This provides a better user experience by automatically showing the editing controls
    /// when a user selects a crop from the preview grid without requiring manual scrolling.
    /// </summary>
    private void ScrollToBottomSmooth()
    {
        // Find the ScrollViewer control by name from the XAML
        var scrollViewer = this.FindControl<ScrollViewer>("MainScrollViewer");
        if (scrollViewer == null)
        {
            return;
        }

        // Get the current and target scroll positions
        _scrollStartOffset = scrollViewer.Offset.Y;
        _scrollTargetOffset = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

        // If already at bottom, no need to scroll
        if (Math.Abs(_scrollStartOffset - _scrollTargetOffset) < 1.0)
        {
            return;
        }

        // Initialize scroll animation
        _scrollStartTime = DateTime.Now;

        // Stop any existing scroll animation
        _scrollTimer?.Stop();

        // Create and start a timer for smooth scrolling
        _scrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };

        _scrollTimer.Tick += (s, e) =>
        {
            var elapsed = (DateTime.Now - _scrollStartTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / ScrollDurationMs, 1.0);

            // Apply easing function (cubic ease in-out for smooth acceleration/deceleration)
            var easedProgress = EaseInOutCubic(progress);

            // Calculate current offset
            var currentOffset = _scrollStartOffset + (_scrollTargetOffset - _scrollStartOffset) * easedProgress;

            // Apply the scroll offset
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, currentOffset);

            // Stop when animation is complete
            if (progress >= 1.0)
            {
                _scrollTimer.Stop();
            }
        };

        _scrollTimer.Start();
    }

    /// <summary>
    /// Cubic easing in-out function for smooth animation
    /// Provides gentle acceleration at start and deceleration at end
    /// </summary>
    /// <param name="t">Progress from 0 to 1</param>
    /// <returns>Eased value from 0 to 1</returns>
    private double EaseInOutCubic(double t)
    {
        return t < 0.5
            ? 4 * t * t * t
            : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }

    #endregion
}