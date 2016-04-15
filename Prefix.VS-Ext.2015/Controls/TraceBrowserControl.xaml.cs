using System.Windows.Controls;
using Prefix.VSExt2015.Models;

namespace Prefix.VSExt2015.Controls
{
    public partial class TraceBrowserControl : UserControl
    {
        public TraceBrowserControl()
        {
            InitializeComponent();
        }

        public TraceBrowserModel Model { get; private set; } = new TraceBrowserModel();
    }
}