using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Prefix.VSExt2015.Models;

namespace Prefix.VSExt2015
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("348b53d2-29d8-475b-81b6-e5ba674e458b")]
    public sealed class TraceBrowserToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceBrowserToolWindow"/> class.
        /// </summary>
        public TraceBrowserToolWindow() : base(null)
        {
            Caption = $"Prefix v{Helpers.GetPrefixVersion()}";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Control = new Controls.TraceBrowserControl();
            Content = Control;

            MyDispatcher = Dispatcher.CurrentDispatcher;

            Control.Template = (ControlTemplate)Control.Resources["StartingUp"];
            CreateHubspotConnection(Helpers.GetPrefixEndpoint());
        }

        internal void CreateHubspotConnection(string prefixEndpoint)
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
                
                var hubConnection = new HubConnection(prefixEndpoint);
                var signalr = hubConnection.CreateHubProxy("signalhub");
                signalr.On("requestCompleted", (List<RequestSummary> a) =>
                {
                    MyDispatcher.Invoke(delegate
                    {
                        foreach (var summary in a)
                        {
                            // prevent duplicate keys
                            if (Control.Model.Summaries.ContainsKey(summary.ID) == false)
                            {
                                Control.Model.Summaries.Add(new KeyValuePair<string, RequestSummaryVM>(summary.ID,
                                    new RequestSummaryVM(summary)));
                            }
                        }
                        
                        var list = Control.FindName("TraceList") as ListView;
                        if (list == null) return;
                        list.ScrollIntoView(list.Items.GetItemAt(list.Items.Count - 1));
                    });
                });

                signalr.On("requestRemoved", (List<string> a) =>
                {
                    MyDispatcher.Invoke(delegate
                    {
                        foreach (var id in a)
                        {
                            Control.Model.Summaries.Remove(id);
                        }
                    });
                });

                signalr.On("version", (Models.PrefixVersion version) =>
                {
                    MyDispatcher.Invoke(delegate
                    {
                        if (!string.IsNullOrWhiteSpace(version?.version))
                        {
                            Control.Model.Version = $"v{version.version}";
                        }
                    });
                });

                _hub = signalr;
                _connectionChecker = new ConnectionChecker(MyDispatcher, hubConnection, this);
                _timer = new Timer(_connectionChecker.CheckStatus, null, 0, 1500);
            }
            catch (Exception e)
            {
                Debug.Write(e);
            }
        }

        private Timer _timer;
        private Dispatcher MyDispatcher { get; set; }
        // ReSharper disable once NotAccessedField.Local
        private IHubProxy _hub;
        internal readonly Controls.TraceBrowserControl Control;
        private ConnectionChecker _connectionChecker;
    }

    internal class ConnectionChecker
    {
        private readonly Dispatcher _dispatcher;
        private readonly HubConnection _connection;
        private readonly TraceBrowserToolWindow _browser;

        public ConnectionChecker(Dispatcher dispatcher, HubConnection connection, TraceBrowserToolWindow browser)
        {
            _dispatcher = dispatcher;
            _connection = connection;
            _browser = browser;
        }

        public void CheckStatus(object stateInfo)
        {
            try
            {
                switch (_connection.State)
                {
                    case ConnectionState.Connecting:
                        _dispatcher.Invoke(delegate
                        {
                            _browser.Control.Model.PrefixRunning = false;
                            _browser.Control.Template =
                                (ControlTemplate)_browser.Control.Resources["Connecting"];

                        });
                        break;
                    case ConnectionState.Connected:
                        _dispatcher.Invoke(delegate
                        {
                            if (_browser.Control.Model.PrefixRunning) return;
                            if (_browser.Control.Model.Summaries.Count > 0)
                            {
                                _browser.Control.Model.PrefixRunning = true;
                                _browser.Control.Template =
                                    (ControlTemplate) _browser.Control.Resources["LitTemplate"];
                                return;
                            }
                            _browser.Control.Template =
                            (ControlTemplate)_browser.Control.Resources["Waiting"];

                        });
                        break;
                    case ConnectionState.Reconnecting:
                        _dispatcher.Invoke(delegate
                        {
                            _browser.Control.Template =
                                (ControlTemplate)_browser.Control.Resources["Connecting"];
                            _browser.Control.Model.PrefixRunning = false;
                        });
                        break;
                    case ConnectionState.Disconnected:
                        _dispatcher.Invoke(delegate
                        {
                            var prefixEndpoint = Helpers.GetPrefixEndpoint();
                            if (_connection.Url.StartsWith(prefixEndpoint, StringComparison.OrdinalIgnoreCase) == false)
                            {
                                // endpoint has changed
                                _browser.CreateHubspotConnection(prefixEndpoint);
                                _browser.Control.Model.PrefixRunning = false;
                                _browser.Control.Template =
                                    (ControlTemplate)_browser.Control.Resources["Connecting"];
                                return;
                            }
                            _browser.Control.Template =
                                (ControlTemplate)_browser.Control.Resources["NotFoundTemplate"];
                            _browser.Control.Model.PrefixRunning = false;
                            _connection.Start();
                        });
                        break;
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
