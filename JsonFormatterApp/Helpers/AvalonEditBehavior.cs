using System;
using System.Windows;
using System.Windows.Data;
using ICSharpCode.AvalonEdit;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Helpers
{
    /// <summary>
    /// Attached behavior to enable binding on AvalonEdit TextEditor.Text property
    /// </summary>
    public static class AvalonEditBehavior
    {
        private static readonly DependencyProperty IsHandlerAttachedProperty =
            DependencyProperty.RegisterAttached(
                "IsHandlerAttached",
                typeof(bool),
                typeof(AvalonEditBehavior),
                new PropertyMetadata(false)
            );

        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableTextChanged)
            );

        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }

        private static bool GetIsHandlerAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHandlerAttachedProperty);
        }

        private static void SetIsHandlerAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHandlerAttachedProperty, value);
        }

        private static void OnBindableTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            var newText = e.NewValue as string ?? string.Empty;

            // Only update if text is different to avoid infinite loops
            if (editor.Text != newText)
            {
                var caretOffset = editor.CaretOffset;
                editor.Text = newText;

                // Restore caret position if possible
                if (caretOffset <= editor.Text.Length)
                {
                    editor.CaretOffset = caretOffset;
                }
            }

            // Attach event handler only once per editor instance
            if (!GetIsHandlerAttached(editor))
            {
                SetIsHandlerAttached(editor, true);

                editor.TextChanged += (sender, args) =>
                {
                    if (sender is TextEditor textEditor)
                    {
                        // Direct approach: Get the TabItem from DataContext and update JsonText directly
                        // This is more reliable than trying to work through the binding system
                        if (textEditor.DataContext is TabItem tabItem)
                        {
                            // Only update if the value is actually different to avoid infinite loops
                            if (tabItem.JsonText != textEditor.Text)
                            {
                                tabItem.JsonText = textEditor.Text;
                            }
                        }
                        else
                        {
                            // Fallback: Try using the binding expression
                            var bindingExpression = BindingOperations.GetBindingExpression(textEditor, BindableTextProperty);
                            if (bindingExpression != null)
                            {
                                textEditor.SetCurrentValue(BindableTextProperty, textEditor.Text);
                                bindingExpression.UpdateSource();
                            }
                        }
                    }
                };
            }
        }
    }
}
