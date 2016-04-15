using System.Windows;

namespace Prefix.VSExt2015.Models
{
    public class TraceBrowserModel
    {
        public RequestSummaries Summaries { get;  } = new RequestSummaries();
        public string Version { get; set; }
        public bool PrefixRunning { get; set; } = false;
    }
}
