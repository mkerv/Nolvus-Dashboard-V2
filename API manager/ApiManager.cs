//this is converted from Vektors "downloadmanager" code email me here for inquiries: masonmesser23@gmail.com
//I know this is a lot of copy/paste but I'm adding a speed limit function!
//the speed limit function will be configurable when you start the installer.
//I'm also going to make the installer be able to function offline
//IDK why Vektor uses the Nolvus login, or why you even have to create an acct. Will investigate...


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.Net
using Vcc.Nolvus.

namespace DownloadManager
{
    //setup of download handler
    public IDownloadHandler DownloadHandler {get; set;}
    
    //IDK
    public class FileDownloadRequestEvent : EventArgs
    {
        private string _DownloadUrl;

        public string DownloadUrl
        {
            get
            {
                return _DownloadUrl;
            }
        }

        public FileDownloadRequestEvent(string Url)
        {
            _DownloadUrl = Url;
        }
    }
    public delegate void OnFileDownloadRequestedHandler(object sender, FileDownloadRequestEvent EventArgs);

    //Download Handler
    public class DownloaderHandler : IDownloadHandler
    {
        private bool _IsDownloadComplete = false;
        private bool LinkOnly;
        public bool  IsDownloadComplete
        {
            get
            {
                return _IsDownloadComplete;
            }
        }
    }

    //timer for downloads?
    private Stopwatch SW = new Stopwatch();

    event OnFileDownloadRequestedHandler OnFileDownloadRequestEvent;
    public event OnFileDownloadRequestedHandler OnFileDownloadRequestEvent
    {
        add
        {
            if (OnFileDownloadRequestEvent !=null)
            {
                lock (OnFileDownloadRequestEvent)
                {
                    OnFileDownloadRequestEvent += value;
                }
            }
            else
            {
                OnFileDownloadRequestEvent = value;
            }
            remove
            {
                if (OnFileDownloadRequestEvent != null)
                {
                    lock (OnFileDownloadRequestEvent)
                    {
                        OnFileDownloadRequestEvent -= value;
                    }
                }
            }
        }
    }

    public event EventHandler<DownloadItem> OnBeforeDownloadFired;
    public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

    private readonly DownloadProgress DownloadProgress;
    public event DownloadProgressChangedHandler DownloadProgressChanged;

    public ChromeDownloadHandler(bool DownloadLinkOnly, DownloadProgressChangedHandler OnProgress = null)
    {
        if (OnProgress != null) DownloadProgressChanged += OnProgress;
        LinkOnly = DownloadLinkOnly;
        DownloadProgress = new DownloadProgress();
    }

    public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
    {
        OnBeforeDownload?.Invoke(this, downloadItem);

        if (!callback.IsDisposed)
        {
            using (callback)
            {
                string DownloadsDirectoryPath = ServiceSingleton.Folder.DownloadDirectory;

                OnFileDownloadRequestEvent(this, new FileDownloadRequestEvent(downloadItem.Url))

                SW.Start();

                Callback.continue(
                    Path.Combine(
                        DownloadsDirectoryPath,
                        downloadItem.SuggestedFileName
                    ),
                    showDialog: true
                );
            }
        }
    }

    public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
    {
        //using the downloadItem function to download the file, while checking if the Download has started/has ended
        On DownloadUpdatedFired?.Involke(this, downloadItem)

        //checking file integrity
        if (downloadItem.IsValid)
        {
            DownloadProgress.BytesRecieve = downloadItem.RecievedBytes;

            //checking download progress
            if (downloadItem.TotalBytes > 0L)
            {
                DownloadProgress.TotalBytesToReceive = downloadItem.TotalBytes;
            }

            //outputing progress/formating for progress
            DownloadProgress.ProgressPercentage = downloadItem.PercentComplete;
            
            //Getting the network speed of the user (not stored) and then getting how long the download will take to finish
            DownloadPRogress.Speed = downloadItem.RecievedBytes / 1024d / 1024d / SW.Elapsed.TotalSeconds;

            //?? IDK
            DownloadProgress.BytesRecievedAsString= (downloadItem.RecievedBytes / 1024d / 1024d).ToString("0.00");
            DownloadProgress.TotalBytesToReceiveAsString = (downloadItem.TotalBytes / 1024d / 1024d).ToString("0.00");

            //Using the name from the file's host, and not setting a custom name (this then helps with installing mods into MO2)
            DownloadProgress.FileName = downloadItem.SuggestedFileName;

            if (downloadItem.IsInProgress && (downloadItem.PercentComplete !=0))
            {
                if (DownloadProgressChanged != null)
                {
                    DownloadProgressChanged(this, DownloadProgress);
                }
            }

            //telling program what file to download next (I think, that's what this looks like)
            if (downloadItem.Iscomplete)
            {
                SW.Stop();
                _IsDownloadComplete = true;
                OnFileDownloadCompletedEvent(this, new FIleDownloadRequestEvent(downloadItem.Url));
            }
        }
    }

    //Checking if the server is recieving requests
    public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
    {
        if (!Linkonly)
        {
            return true;
        }

        //failure protocall
        else
        {
            OnFileDownloadRequestEvent(this, new FileDownloadRequestEvent(url))
            return false;
        }
    }

    //setting up the browser
    public class ChromeDownloader : chromiumWebBrowser
    {
        private BrowserWindow Browser;
        private WebSite WebSite;
        private string File;
        private string ModId;
        private TaskCompletionSource<object> TaskCompletionDownload = new TaskCompletionSource<object>();
        private TaskCompletionSource<string> TaskCompletionDownloadLink = new TaskCompletionSource<string>();
        event OnFileDownloadRequestedHandler OnFileDownloadRequestEvent;
        public event OnFileDownloadRequestedHandler OnFileDownloadRequestedHandler
        {
            //I feel like this is repeated somewhere else...
            add
            {
                if (OnFileDownloadRequestEvent != null)
                {
                    lock (OnFileDownloadCompletedEvent)
                    {
                        OnFileDownloadRequestEvent += value;
                    }
                }
                else
                {
                    OnFileDownloadRequestEvent = value;
                }
            }
            remove
            {
                if (OnFileDownloadRequestEvent != null)
                {
                    lock (OnFileDownloadRequestEvent)
                    {
                        OnFileDownloadRequestEvent -= value;
                    }
                }
            }

            private string _Urla;
            public string Url {get { return _Url;}}

            //setting up link types (enb, Nexus, and other)
            public ChromiumDownloader(BrowserWindow Window, string address, bool LinkOnly, DownloadProgressChangedHAndler OnProgress)
                :base(address)
            {
                _Url = address;
                Browser = Window;

                Dock = DockStyle.Fill;

                if (_Url.Contains("nexusmods.com"))
                {
                    WebSite = WebSite.Nexus;
                }
                else if (Url.Contains("enbdev.com"))
                {
                    Website = Website.EnbDev;
                }
                else
                {
                    WebSite = Website.Other
                }

                DownloadHandler = new ChromeDownloadHandler(LinkOnly, OnProgress);
                (DownloadHandler as ChromeDownloadHandler).OnFileDownloadRequest += DownloadRequested;
                (DownloadHandler as ChromeDownloadHandler).OnFileDOwnloadCompleted += DownloadCompleted;

                this.LoadingStateChanged += Browser_LoadingStateChanged;
                this.FrameLoadEnd += Browser_FramLoadEnd;

                TaskCompletionDownload = new TaskCompletionSource<obkect>();
                TaskCompletionDownloadLink = new TaskCompletionSource<string>();
            }
            
            //what to do with nexus links
            private void Browser_FramLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
            {
                if (e.Frame.IsMain)
                {
                    switch (Website)
                    {
                        case WebSite.Nexus:
                            HAndleNexusLoadEnd(e.Url);
                            break;
                    }
                }
            }

            private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
            {
                if (!e.IsLoading)
                {
                    UnRegisterLoadingStateEvent();

                    switch (website)
                    {
                        case WebSite.Nexus:
                            HandleNexusLoadState();
                            break;
                        case WebSite.EnbDev:
                            HandleEnbDev();
                            break;
                        case Website.Other:
                            HandleOthers();
                            break;
                    }
                }
            }

            private void RegisterFrameLoadEnd()
            {   
                this.FrameLoadEnd += Browser_FrameLoadEnd;
            }

            private void UnRegisterFrameLoadEnd()
            {
                this.FrameLoadEnd -= Browser_FrameLoadEnd;
            }

            private void RegisterLoadingStateEvent()
            {
                this.LoadingStateChanged += Browser_LoadingStateChanged;
            }
            private void UnRegisterLoadingStateEvent()
            {
                this.LoadingStateChanged -= Browser_LoadingStateChanged;
            }

            private void DownloadRequested(object sender, FileDownloadRequestEvent EventArgs)
            {
                TaskCompletionDownloadLink.SetResult(EventArgs.DownloadUrl);
                OnFileDownloadRequestEvent(this, EventArgs);
            }

            private void DownloadCompleted(object sender, FileDownloadRequestEvent EventArgs)
            {
                TaskCompletionDownload.SetResult(null);
            }

            public bool IsDownloadComplete
            {
                get{
                    return (DownloadHandler as ChromeDownloadHandler).IsDownloadComplete;
                }
            }

            public Task AwaitDownload(string FileName)
            {
                File = FileName;
                return TaskCompletionDownload.Task;
            }

            public Task<string> AwaitDownloadLink(string NexusModId)
            {
                ModId = NexusModId;
                return TaskCompletionDownloadLink.Task;
            }

            #region Scripts

            private async Task<bool> EvaluateScriptWithResponse(string Script)
            {
                var Script Execution = await this.GetMainFrame().EvaluateeScriptAsync(Script);
// praying to fucking god this works...
                return ScriptExecution.Success && (int)ScriptExecution.Result == 1;
            }

            private async Task EvaluateScript(string Script)
            {
                await this.GetMainFrame.EvaluateScriptAsync(Script);
            }

            private voide ExecutrScript(string Script)
            {
                this.GetMainFrame().ExecuteJavaScriptAsync(Script);
            }

            #endregion
            #region NExus

            private async Task<bool> IsLoginNeeded()
            {
                return await EvaluateScriptWithResponse(ScriptManager.GetIsLoginNeeded());
            }

            private void RedirectToLogin()
            {
                ExecuteScript(ScriptManager.GetRedirectToLogin());
            }

            private async Task<bool> IsModNotFound()
            {
                return await EvaluateScriptWithResponse(ScriptManager.GetIsModNotFound());
            }

            private async Task<bool> IsDownloadAvailable()
            {
                return await EvaluateScriptWithResponse(ScriptManager.GetIsDownloadAvailable());
            }

            private async void InitializeNexusManualDownload()
            {
                await EvaluateScript(ScriptManager.GetNexusManualDownloadInit());

                await Task.Delay(100).ContinueWith(T =>
                {
                    ExecuteScript(ScriptManager.getNexusManualDownload());
                });
            }
        }
    }
}
