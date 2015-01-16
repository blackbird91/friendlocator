using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.UI.Input;

namespace TelerikFriendLocator
{
    public partial class ToggleSwitchControl : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked",
                typeof(bool),
                typeof(ToggleSwitchControl),
                new PropertyMetadata(false, OnIsCheckedChanged));
        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Unchecked;
        public ToggleSwitchControl()
        {
            InitializeComponent();
        }
        public bool IsChecked
        {
            set { SetValue(IsCheckedProperty, value); }
            get { return (bool)GetValue(IsCheckedProperty); }
        }
        static void OnIsCheckedChanged(DependencyObject obj,
                                       DependencyPropertyChangedEventArgs args)
        {
            (obj as ToggleSwitchControl).OnIsCheckedChanged(args);
        }
        void OnIsCheckedChanged(DependencyPropertyChangedEventArgs args)
        {
            fillRectangle.Visibility = IsChecked ? Visibility.Visible :
                                                   Visibility.Collapsed;
            slideBorder.HorizontalAlignment = IsChecked ? HorizontalAlignment.Right :
                                                          HorizontalAlignment.Left;
            if (IsChecked && Checked != null)
                Checked(this, new RoutedEventArgs());
            if (!IsChecked && Unchecked != null)
                Unchecked(this, new RoutedEventArgs());
        }

        protected override void OnManipulationCompleted(System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Point pt = e.ManipulationOrigin;
            if (pt.X > 0 && pt.X < this.ActualWidth &&
            pt.Y > 0 && pt.Y < this.ActualHeight)
                IsChecked ^= true;
            e.Handled = true;
            base.OnManipulationCompleted(e);
        }

        protected override void OnManipulationStarted(System.Windows.Input.ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            base.OnManipulationStarted(e);
        }

    }
}
