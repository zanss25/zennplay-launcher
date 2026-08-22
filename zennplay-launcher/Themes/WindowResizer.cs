using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace zennplay_launcher.Themes
{
    public static class WindowResizer
    {
        public static readonly AttachedProperty<WindowEdge?> EdgeProperty =
            AvaloniaProperty.RegisterAttached<Control, WindowEdge?>("Edge", typeof(WindowResizer));

        public static void SetEdge(Control element, WindowEdge? value) =>
            element.SetValue(EdgeProperty, value);

        public static WindowEdge? GetEdge(Control element) =>
            element.GetValue(EdgeProperty);

        static WindowResizer()
        {
            EdgeProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (e.NewValue is WindowEdge edge)
                {
                    control.PointerPressed += (sender, args) =>
                    {
                        if (args.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
                        {
                            // Mencari Window induk secara eksplisit
                            var window = control.FindAncestorOfType<Window>() ?? TopLevel.GetTopLevel(control) as Window;
                            window?.BeginResizeDrag(edge, args);
                        }
                    };
                }
            });
        }
        
    }
}