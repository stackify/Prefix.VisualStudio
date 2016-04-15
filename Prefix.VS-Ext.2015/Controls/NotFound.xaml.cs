using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.ServiceProcess;

namespace Prefix.VSExt2015.Controls
{
    /// <summary>
    /// Interaction logic for NotFound.xaml
    /// </summary>
    public partial class NotFound : INotifyPropertyChanged
    {
        public NotFound()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            PrefixNotFound = string.IsNullOrWhiteSpace(Helpers.GetPrefixVersion()) ? Visibility.Visible :Visibility.Hidden;
            OnPropertyChanged(nameof(PrefixNotFound));

            if (PrefixNotFound != Visibility.Visible)
            {
                var sc = new ServiceController("StackifyPrefix");
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        PrefixNotRunning = Visibility.Hidden;
                        OnPropertyChanged(nameof(PrefixNotRunning));
                        break;
                    default:
                        PrefixNotRunning = Visibility.Visible;
                        OnPropertyChanged(nameof(PrefixNotRunning));
                        break;
                }
            }
            else
            {
                PrefixNotRunning = Visibility.Hidden;
                OnPropertyChanged(nameof(PrefixNotRunning));
            }

            base.OnRender(drawingContext);
        }
        
        public Visibility PrefixNotRunning { get; set; }
        public Visibility PrefixNotFound { get; set; }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
