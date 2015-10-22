using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unibill;
using Uniject;
using ICSharpCode.SharpZipLib.Zip;

namespace Unibill.Impl
{
    public class DownloadManager {
        private IReceiptStore receiptStore;
        private IUtil util;
        private IStorage storage;
        private IURLFetcher fetcher;
        private ILogger logger;
        private volatile string persistentDataPath;

        /// <summary>
        /// This is a list of file bundle identifiers from unibiller.com.
        /// </summary>
        private List<string> scheduledDownloads = new List<string>();
        private List<string> workQueue = new List<string>();
        private int bufferSize = DEFAULT_BUFFER_SIZE;
        private const string DOWNLOAD_TOKEN_URL = "http://cdn.unibiller.com/download_token";
        private const string SCHEDULED_DOWNLOADS_KEY = "com.outlinegames.unibill.scheduled_downloads";
        private const int DEFAULT_BUFFER_SIZE = 2000000;
        private BillingPlatform platform;
        private string appSecret;

        public event Action<PurchasableItem, DirectoryInfo> onDownloadCompletedEvent;
        public event Action<PurchasableItem, string> onDownloadFailedEvent;
        public event Action<PurchasableItem, int> onDownloadProgressedEvent;

        public DownloadManager(IReceiptStore receiptStore, IUtil util, IStorage storage, IURLFetcher fetcher, ILogger logger, BillingPlatform platform, string appSecret) {
            this.receiptStore = receiptStore;
            this.util = util;
            this.storage = storage;
            this.fetcher = fetcher;
            this.logger = logger;
            this.platform = platform;
            this.appSecret = appSecret;
            this.scheduledDownloads = deserialiseDownloads ();
            this.workQueue = new List<string> (scheduledDownloads);
            this.persistentDataPath = util.persistentDataPath;
        }

        /// <summary>
        /// Sets the size of the download buffer.
        /// Only for testing.
        /// </summary>
        public void setBufferSize(int size) {
            this.bufferSize = size;
        }

        public void downloadContentFor(PurchasableItem item) {
            if (!item.hasDownloadableContent) {
                if (null != onDownloadFailedEvent) {
                    onDownloadFailedEvent (item, "The item has no downloadable content");
                }
                return;
            }

            if (isDownloaded (item.downloadableContentId)) {
                onDownloadCompletedEvent (item, new DirectoryInfo (getContentPath (item.downloadableContentId)));
                return;
            }

            if (!receiptStore.hasItemReceiptForFilebundle (item.downloadableContentId)) {
                if (null != onDownloadFailedEvent) {
                    onDownloadFailedEvent (item, "The item is not owned");
                }
                return;
            }

            if (!scheduledDownloads.Contains (item.downloadableContentId)) {
                scheduledDownloads.Add (item.downloadableContentId);
                workQueue.Add (item.downloadableContentId);
                serialiseDownloads ();
            }
        }

        /// <summary>
        /// Work on any scheduled downloads.
        /// Used for unit testing.
        /// </summary>
        public IEnumerator checkDownloads() {
            for (int t = 0; t < scheduledDownloads.Count; t++) {
                var scheduledDownload = scheduledDownloads [t];
                yield return util.InitiateCoroutine (download (scheduledDownload.ToString()));
            }
        }

        private UnityEngine.WaitForFixedUpdate waiter;

        /// <summary>
        /// Continuously monitor our scheduled downloads.
        /// </summary>
        public IEnumerator monitorDownloads() {
            waiter = new UnityEngine.WaitForFixedUpdate ();
            while (true) {
                while (workQueue.Count > 0) {
                    // Removal from the work queue is the responsibility of download function.
                    yield return util.InitiateCoroutine (download (workQueue[0]));
                }
                yield return waiter;
            }
        }

        public int getQueueSize() {
            return workQueue.Count;
        }

        private List<string> deserialiseDownloads() {
            var strings = storage.GetString (SCHEDULED_DOWNLOADS_KEY, "[]").arrayListFromJson ();
            var result = new List<string> ();
            if (null != strings) {
                foreach (var s in strings) {
                    result.Add (s.ToString ());
                }
            }
            return result;
        }

        private void serialiseDownloads() {
            List<object> toSerialise = new List<object> ();
            foreach (var s in scheduledDownloads) {
                toSerialise.Add (s);
            }
            storage.SetString (SCHEDULED_DOWNLOADS_KEY, MiniJSON.jsonEncode (toSerialise));
        }
            
        private IEnumerator download(string bundleId) {
            // Do we have a partial download?
            Directory.CreateDirectory(getDataPath(bundleId));
            if (!File.Exists (getZipPath (bundleId))) {
                logger.Log (bundleId);
                string downloadToken = "";
                // Get a download token.
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string> ();
                    try {
                        parameters.Add ("receipt", receiptStore.getItemReceiptForFilebundle (bundleId));
                    } catch (ArgumentException) {
                        onDownloadFailedPermanently (bundleId, string.Format("Bundle {0} no longer defined in inventory!", bundleId));
                        yield break;
                    }

                    parameters.Add ("bundleId", bundleId);
                    parameters.Add ("platform", platform.ToString ());
                    parameters.Add ("appSecret", appSecret);

                    yield return fetcher.doPost (DOWNLOAD_TOKEN_URL, parameters);
                    var response = fetcher.getResponse ();
                    if (!string.IsNullOrEmpty (response.error)) {
                        logger.Log ("Error downloading content: {0}. Unibill will retry later.", response.error);
                        yield return getRandomSleep ();
                        yield break;
                    }

                    var downloadTokenHash = (Dictionary<string, object>)MiniJSON.jsonDecode (response.contentString);
                    if (null == downloadTokenHash) {
                        logger.Log ("Error fetching download token. Unibill will retry later.");
                        yield return getRandomSleep ();
                        yield break;
                    }

                    bool success = bool.Parse (downloadTokenHash ["success"].ToString ());
                    if (!success) {
                        logger.LogError ("Error downloading bundle {0}. Download abandoned.", bundleId);
                        var errorString = "";
                        if (downloadTokenHash.ContainsKey ("error")) {
                            errorString = downloadTokenHash ["error"].ToString ();
                            logger.LogError (errorString);
                        }
                        onDownloadFailedPermanently (bundleId, errorString);
                        yield break;
                    }

                    downloadToken = downloadTokenHash ["url"].ToString ();
                }
                // Figure out the content length.
                // We can't do a HEAD because of Unity's wonderful
                // WWW class, so we do a 2 byte range GET and look at the headers.
                Dictionary<string, string> headers = new Dictionary<string, string> ();
                // These headers are required since on iOS
                // Unity wrongly adds if-modified headers that cause google to (rightly)
                // return content not modified.
                headers ["If-Modified-Since"] = "Tue, 1 Jan 1980 00:00:00 GMT";
                headers ["If-None-Match"] = "notanetag";
                long contentLength;
                {
                    var version = getVersionToDownload (bundleId);
                    if (version != "*") {
                        downloadToken += "&generation=" + version;
                    }

                    headers ["Range"] = "bytes=0-1";
                    yield return fetcher.doGet (downloadToken + (version != "*" ? "&generation=" + version : ""), headers);
                    IHTTPRequest response = fetcher.getResponse ();

                    if (version == "*") {
                        if (!response.responseHeaders.ContainsKey ("X-GOOG-GENERATION")) {
                            if (isContentNotFound(response)) {
                                string error = string.Format("404 - Downloadable Content missing for bundle {0}!", bundleId);
                                logger.LogError (error);
                                onDownloadFailedPermanently(bundleId, error);
                                yield break;
                            } else {
                                logger.LogError ("Malformed server response. Missing generation");
                                logger.LogError (response.error);
                                yield return getRandomSleep ();
                                yield break;
                            }
                        }

                        if (!string.IsNullOrEmpty (response.error)) {
                            logger.LogError ("Error downloading content. Will retry");
                            yield return getRandomSleep ();
                            yield break;
                        }

                        version = response.responseHeaders ["X-GOOG-GENERATION"].ToString ();
                        saveVersion (bundleId, version);
                        downloadToken += "&generation=" + version;
                    }

                    if (!response.responseHeaders.ContainsKey ("CONTENT-RANGE")) {
                        logger.LogError ("Malformed server response. Missing content-range");
                        yield return getRandomSleep ();
                        yield break;
                    }

                    string contentRange = response.responseHeaders ["CONTENT-RANGE"].ToString ();
                    contentLength = long.Parse (contentRange.Split (new char[] { '/' }, 2) [1]);
                }

                //// Fetch the content.
                {
                    PurchasableItem downloadingItem = receiptStore.getItemFromFileBundle (bundleId);
                    using (FileStream f = openDownload (bundleId)) {
                        long rangeStart = f.Length;
                        if (rangeStart > 0) {
                            f.Seek (0, SeekOrigin.End);
                        }
                        long rangeEnd = Math.Min (rangeStart + bufferSize, contentLength);

                        int lastProgress = -1;
                        while (rangeStart < rangeEnd) {
                            string header = string.Format ("bytes={0}-{1}", rangeStart, rangeEnd);
                            headers ["Range"] = header;
                            yield return fetcher.doGet (downloadToken, headers);

                            var response = fetcher.getResponse ();
                            if (!string.IsNullOrEmpty (response.error)) {
                                logger.LogError ("Error downloading content. Will retry.");
                                logger.LogError (response.error);
                                yield return getRandomSleep ();
                                yield break;
                            }

                            int progress = (int)(((float)rangeEnd / (float)contentLength) * 100.0f);
                            progress = Math.Min (99, progress);
                            if (null != onDownloadProgressedEvent && lastProgress != progress) {
                                onDownloadProgressedEvent (downloadingItem, progress);
                                lastProgress = progress;
                            }
                            try {
                                f.Write (response.bytes, 0, response.bytes.Length);
                                f.Flush ();
                            } catch (IOException i) {
                                onDownloadFailedPermanently (bundleId, i.Message);
                                yield break;
                            }
                            rangeStart = rangeEnd + 1;
                            rangeEnd = rangeStart + bufferSize;
                            rangeEnd = Math.Min (rangeEnd, contentLength);
                        }
                    }
                }
                File.Move(getPartialPath(bundleId), getZipPath(bundleId));
                File.Delete (getVersionPath (bundleId));
            }

            // Make sure this download isn't reattempted during unpacking.
            workQueue.Remove (bundleId);
            util.RunOnThreadPool(() => {
                Unpack(bundleId);
            });
        }

        /// <summary>
        /// Determine if a request failed with a 404.
        /// Not trivial given the lack of access to response codes.
        /// </summary>
        private bool isContentNotFound(IHTTPRequest request) {
            foreach (var z in request.responseHeaders) {
                if (z.Value.ToUpper().Contains ("404 NOT FOUND")) {
                    return true;
                }
            }
            return request.error.Contains ("404");
        }

        /// <summary>
        /// Integrity check and extract the zip file.
        /// </summary>
        private void Unpack(string bundleId) {
            try {
                string zipPath = getZipPath(bundleId);
                if (!File.Exists(zipPath)) {
                    logger.LogError("No download found: " + zipPath);
                    util.RunOnMainThread (() => onUnpackFailedRecoverable (bundleId));
                    return;
                }

                logger.Log("Verifying download...");
                if (!verifyDownload(zipPath)) {
                    logger.LogError("Download failed integrity check. Deleting...");
                    Directory.Delete(getDataPath(bundleId), true);
                    util.RunOnMainThread (() => onUnpackFailedRecoverable (bundleId));
                    return;
                }
                logger.Log("Download verified.");
                logger.Log("Unpacking");
                // Unpack the zip into a temporary folder.
                DeleteIfExists (getUnpackPath (bundleId));

                Directory.CreateDirectory(getUnpackPath(bundleId));
                using(var s = new BufferedStream(new FileStream(getZipPath(bundleId), FileMode.Open))) {
                    ZipUtils.decompress(s, getUnpackPath(bundleId));
                }

                logger.Log("Unpack complete");

                // Delete any existing content folder.
                DeleteIfExists (getContentPath (bundleId));

                // Rename our unpack folder to content, which should be atomic.
                Directory.Move (getUnpackPath (bundleId), getContentPath (bundleId));

                // Clean up.
                File.Delete(getZipPath(bundleId));

                util.RunOnMainThread (() => FinishDownload (bundleId));
            } catch (IOException i) {
                // This will typically indicate a disk full condition,
                // though it could occur if storage is removed.
                logger.LogError (i.Message);
                onDownloadFailedPermanently(bundleId, i.Message);
            } catch (Exception e) {
                logger.LogError (e.Message);
                logger.LogError (e.StackTrace);
                onDownloadFailedPermanently(bundleId, e.Message);
            }
        }

        private void DeleteIfExists(string folder) {
            if (Directory.Exists (folder)) {
                Directory.Delete (folder, true);
            }
        }

        private void onUnpackFailedRecoverable(string bundleId) {
            // Schedule for redownload.
            Enqueue (bundleId);
        }

        private void Enqueue(string bundleId) {
            if (!workQueue.Contains (bundleId)) {
                workQueue.Add (bundleId);
            }
        }

        private void onDownloadFailedPermanently(string bundleId, string error) {
            util.RunOnMainThread (() => {
                removeDownloadFromQueues(bundleId);
                if (null != onDownloadFailedEvent) {
                    try {
                        onDownloadFailedEvent (receiptStore.getItemFromFileBundle (bundleId), error);
                    } catch (ArgumentException) {
                        onDownloadFailedEvent(null, error);
                    }
                }
            });
        }

        private void removeDownloadFromQueues(string bundleId) {
            workQueue.Remove (bundleId);
            scheduledDownloads.Remove(bundleId);
            serialiseDownloads ();
        }

        private void FinishDownload(string bundleId) {
            scheduledDownloads.Remove (bundleId);
            serialiseDownloads ();
            if (null != onDownloadCompletedEvent) {
                onDownloadCompletedEvent (receiptStore.getItemFromFileBundle (bundleId), new DirectoryInfo(getContentPath(bundleId)));
            }
        }

        private bool verifyDownload(string filepath) {
            try {
                using (ZipFile z = new ZipFile(filepath)) {
                    return z.TestArchive(true);
                }
            }
            catch (Exception) {
                return false;
            }
        }

        private byte[] decodeBase64String(string s) {
            return Convert.FromBase64String(s);
        }

        private FileStream openDownload(string bundleId) {
            return new FileStream(getPartialPath(bundleId), FileMode.OpenOrCreate);
        }

        public string getContentPath(string bundleId) {
            return Path.Combine(getDataPath(bundleId), "content");
        }

        private string getUnpackPath(string bundleId) {
            return Path.Combine (getDataPath (bundleId), "unpack");
        }

        private string getZipPath(string bundleId) {
            return Path.Combine(getDataPath(bundleId), "download.zip");
        }

        private string getPartialPath(string bundleId) {
            return Path.Combine(getDataPath(bundleId), "download.partial");
        }

        private void saveVersion(string bundleId, string version) {
            File.WriteAllText(getVersionPath(bundleId), version);
        }

        /// <summary>
        /// Get the version number of any in progress download
        /// by examining the data directory.
        /// If not version is found, return a wildcard.
        /// </summary>
        private string getVersionToDownload(string bundleId) {
            string versionPath = getVersionPath(bundleId);
            if (File.Exists(versionPath)) {
                string contents = File.ReadAllText(versionPath);
                long l;
                if (long.TryParse(contents, out l)) {
                    return contents;
                }
            }

            // Latest version will do.
            return "*";
        }

        private string getVersionPath(string bundleId) {
            return Path.Combine(getDataPath(bundleId), "download.version");
        }

        private string getRootContentPath() {
            return Path.Combine (persistentDataPath, "unibill-content");
        }

        public string getDataPath(string bundleId) {
            return Path.Combine (getRootContentPath(), bundleId);
        }

        public bool isDownloaded(string bundleId) {
            return Directory.Exists (getContentPath (bundleId));
        }

        public void deleteContent(string bundleId) {
            Directory.Delete (getDataPath (bundleId), true);
        }

        private Random rand = new Random();
        private object getRandomSleep() {
            int delay = 30 + rand.Next (30);
            logger.Log ("Backing off for {0} seconds", delay);
            return util.getWaitForSeconds(delay);
        }
    }
}
