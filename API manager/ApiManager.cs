//this is converted from Vektors "downloadmanager" code email me here for inquiries: masonmesser23@gmail.com


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
}
