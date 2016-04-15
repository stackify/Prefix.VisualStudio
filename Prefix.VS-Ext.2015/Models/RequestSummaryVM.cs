using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FontAwesome.WPF;

namespace Prefix.VSExt2015.Models
{

    // ReSharper disable once InconsistentNaming
    public class RequestSummaryVM
    {
        private readonly RequestSummary _source;
        private readonly List<DependencyVM> _dependencies;
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        // ReSharper disable once ConvertToAutoProperty
        public RequestSummary Source => _source;

        public RequestSummaryVM(RequestSummary source)
        {
            _source = source;
            _dependencies = GetDependencies();
        }

        public string Started => $"{Source.Started.ToLocalTime():hh:mm:ss.fff}";
        public string Url => Source.RawUrl;
        public string ReportingUrl => $"{Source.HttpMethod} {Source.ReportingUrl}";
        public string AppName => Source.AppName;
        public string Took => $"{Source.TookMs}ms";
        public string SlowestComponent => string.IsNullOrWhiteSpace(Source.Slowest?.Name) ? "" : $"{Source.Slowest.ItemType}-{Source.Slowest.Name} ({Source.Slowest.TookMs}ms)";
        public string Link => $"{Helpers.GetPrefixEndpoint()}trace/{Source.ID}";
        public string HttpMethod => Source.HttpMethod;

        public DependencyVM Errors => new DependencyVM
        {
            Icon = FontAwesomeIcon.Bug,
            Text = $"{Source.Errors}  ",
            Link = "",
            Visibility = Source.Errors > 0 ? Visibility.Visible : Visibility.Collapsed,
            Tooltip = "Exceptions"
        };

        public DependencyVM StatusCode => new DependencyVM
        {
            Text = $"{Source.Status}",
            Visibility = Source.Status >= 300 || Source.Status < 200 ? Visibility.Visible : Visibility.Collapsed,
        };

        // ReSharper disable once ConvertToAutoProperty
        public List<DependencyVM> Dependencies => _dependencies;

        private List<DependencyVM> GetDependencies()
        {
            var dependencies = new List<DependencyVM>();

            if (Source.DBCalls > 0)
                dependencies.Add(new DependencyVM
                {
                    Icon = FontAwesomeIcon.Database,
                    Text = $"{Source.DBCalls}  ",
                    Link = "",
                    Tooltip = "Database Calls"
                });


            if (Source.CacheCalls > 0)
                dependencies.Add(new DependencyVM
                {
                    Icon = FontAwesomeIcon.Exchange,
                    Text = $"{Source.CacheCalls}  ",
                    Link = "",
                    Tooltip = "Cache Calls"
                });


            if (Source.WSCalls > 0)
                dependencies.Add(new DependencyVM
                {
                    Icon = FontAwesomeIcon.CloudDownload,
                    Text = $"{Source.WSCalls}  ",
                    Link = "",
                    Tooltip = "Web Service Calls"
                });

            return dependencies;
        }
    }

    // ReSharper disable once InconsistentNaming
    public class DependencyVM
    {
        public string Text { get; set; }
        public string Link { get; set; }
        public FontAwesomeIcon Icon { get; set; }
        public System.Windows.Visibility Visibility { get; set; } = System.Windows.Visibility.Hidden;
        public string Tooltip { get; set; }
    }

    public class RequestSummaries : ObservableConcurrentDictionary<string, RequestSummaryVM>
    {
        #region Constructors

        public RequestSummaries() { }

        public RequestSummaries(IDictionary<string, RequestSummaryVM> dictionary) : base(dictionary)
        {
        }

        public RequestSummaries(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public RequestSummaries(IDictionary<string, RequestSummaryVM> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        #endregion

        public override IEnumerator<KeyValuePair<string, RequestSummaryVM>> GetEnumerator()
        {
            return Dictionary.Values.OrderBy(i => i.Source.Started).Select(i => new KeyValuePair<string, RequestSummaryVM>(i.Source.ID, i)).GetEnumerator();
        }
    }
}
