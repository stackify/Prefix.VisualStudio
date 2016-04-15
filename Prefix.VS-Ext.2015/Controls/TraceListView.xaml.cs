using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prefix.VSExt2015.Models;

namespace Prefix.VSExt2015.Controls
{
    /// <summary>
    /// Interaction logic for TraceListView.xaml
    /// </summary>
    public partial class TraceListView
    {
        public TraceListView()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)TraceList.Items).CollectionChanged += ListView_CollectionChanged;
        }

        private volatile bool _isUserScroll = true;
        public bool IsAutoScrollEnabled { get; set; } = true;

        private void TraceButton_Click(object sender, RoutedEventArgs e)
        {
            var element = e.Source as Button;
            if (!(element?.DataContext is KeyValuePair<string, RequestSummaryVM>)) return;
            var context = (KeyValuePair<string, RequestSummaryVM>)element.DataContext;
            try
            {
                System.Diagnostics.Process.Start(context.Value.Link);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, "Error opening browser");
            }
            e.Handled = true;
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (!IsAutoScrollEnabled)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    TraceList.ScrollIntoView(e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TraceList.ScrollIntoView(TraceList.Items.GetItemAt(TraceList.Items.Count - 1));
                    break;
            }
        }

        private void TraceList_Loaded(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(TraceList) != 0)
            {
                Decorator border = VisualTreeHelper.GetChild(TraceList, 0) as Decorator;
                ScrollViewer sv = border?.Child as ScrollViewer;
                if (sv != null)
                {
                    sv.ScrollChanged += ScrollViewer_ScrollChanged;
                }
            }
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Math.Abs(e.VerticalChange) < 1.0)
                return;

            if (_isUserScroll)
            {
                if (e.VerticalChange > 0.0)
                {
                    double scrollerOffset = e.VerticalOffset + e.ViewportHeight;
                    if (Math.Abs(scrollerOffset - e.ExtentHeight) < 5.0)
                    {
                        // The user has tried to move the scroll to the bottom, activate autoscroll.
                        IsAutoScrollEnabled = true;
                    }
                }
                else
                {
                    // The user has moved the scroll up, deactivate autoscroll.
                    IsAutoScrollEnabled = false;
                }
            }
            _isUserScroll = true;
        }
    }
}
