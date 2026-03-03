using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;

namespace H.Avalonia.Behaviors;

/// <summary>
/// Behavior that selects all text in a NumericUpDown control when it receives focus.
/// This makes it easier for users to quickly change numeric values by typing a new value.
/// </summary>
public class SelectAllOnFocusBehavior : Behavior<NumericUpDown>
{
    private bool _isAttached;

    /// <summary>
    /// Called when the behavior is attached to the control.
    /// Subscribes to the GotFocus event.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null && !_isAttached)
        {
            AssociatedObject.GotFocus += OnGotFocus;
            _isAttached = true;
        }
    }

    /// <summary>
    /// Called when the behavior is detached from the control.
    /// Unsubscribes from the GotFocus event.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null && _isAttached)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            _isAttached = false;
        }
    }

    /// <summary>
    /// Handles the GotFocus event by selecting all text in the NumericUpDown's inner TextBox.
    /// Only fires when focus lands inside this specific control, not from a bubbled event.
    /// </summary>
    private void OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not NumericUpDown numericUpDown)
            return;

        // Find the inner TextBox for this specific NumericUpDown
        var textBox = FindTextBoxInTemplate(numericUpDown);
        if (textBox == null)
            return;

        // Ignore bubbled focus events — only act when focus landed on our own inner TextBox
        if (e.Source is not TextBox sourceTb || sourceTb != textBox)
            return;

        // Use a dispatcher to ensure the selection happens after the control is fully focused
        Dispatcher.UIThread.Post(() =>
        {
            textBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    /// <summary>
    /// Recursively searches for a TextBox control within the visual tree.
    /// </summary>
    private TextBox? FindTextBoxInTemplate(Control control)
    {
        if (control is TextBox textBox)
        {
            return textBox;
        }

        foreach (var child in control.GetVisualChildren())
        {
            if (child is Control childControl)
            {
                var result = FindTextBoxInTemplate(childControl);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }
}
