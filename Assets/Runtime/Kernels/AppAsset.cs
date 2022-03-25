using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class AssetPath
    {
        public static string LocalBundleURI(string package)
        {
            string path = AssetPath.LocalBundlePath(package);
            path = XCSharp.FileURI(path);
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = path.Replace("file://", "file:///");
            }
            return path;
        }
        public static string LocalBundlePath(string package)
        {
            return string.Format("{0}/{1}.bundle", AssetPath.LocalBundleHome, package);
        }
        public static string LocalBundleHome
        {
            get { return Application.persistentDataPath + "/Bundles"; }
        }

        public static string StreamBundleURI(string package)
        {
            string path = AssetPath.StreamBundlePath(package);
            if (Application.platform != RuntimePlatform.Android)
            {
                path = XCSharp.FileURI(path);
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    path = path.Replace("file://", "file:///");
                }
            }
            else
            {
                path = XCSharp.JarFileURI(path);
            }
            return path;
        }
        public static string StreamBundlePath(string package)
        {
            return string.Format("{0}/{1}.bundle", AssetPath.StreamBundleHome, package);
        }
        public static string StreamBundleHome
        {
            get { return string.Format("{0}/AssetBundle/{1}", Application.streamingAssetsPath, XUtility.PlatformString); }
        }
    }

    public class BundleInfo
    {
        public int remoteVersion = 0;
        public int localVersion = 0;
    }

    public class DownloadTask
    {
        public string url { get; private set; }
        public WWW www { get; private set; }
        public bool done { get; private set; }

        public DownloadTask(string url)
        {
            this.url = url;
            this.www = null;
            this.done = false;
        }

        public WWW Download()
        {
            this.done = false;
            this.www = new WWW(this.url);
            return this.www;
        }

        public void Done()
        {
            this.www = null;
            this.done = true;
        }
    }

    public class BundleLoading
    {
        public bool done { get; private set; }
        public string error { get; private set; }
        public AssetBundle bundle { get; private set; }

        public bool Load(string bundleName)
        {
            this.Begin();

            string bundlePath = AssetPath.LocalBundlePath(bundleName);

            byte[] data = XFile.ReadBytesFile(bundlePath);
            Autarky.Signature sig_v3;
            if (Autarky.Load(ref data, out sig_v3))
            {
                try
                {
                    this.bundle = AssetBundle.LoadFromMemory(data);
                    this.done = true;
                    return true;
                }
                catch (System.Exception e)
                {
                    this.error = e.Message;
                    this.done = true;
                    return false;
                }
            }
            else
            {
                this.error = Autarky.error;
                this.done = true;
                return false;
            }
        }

        public IEnumerator LoadAsync(string bundleName)
        {
            this.Begin();

            string bundlePath = AssetPath.LocalBundlePath(bundleName);
            string bundleURI = AssetPath.LocalBundleURI(bundleName);
            if (!System.IO.File.Exists(bundlePath))
            {
                bundleURI = AssetPath.StreamBundleURI(bundleName);
            }

            WWW www = new WWW(bundleURI);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                this.error = www.error;
                this.done = true;
                yield break;
            }

            byte[] data = www.bytes;
            Autarky.Signature sig;
            if (Autarky.Load(ref data, out sig))
            {
                if (sig.enableEncrypt)
                {
                    var req = AssetBundle.LoadFromMemoryAsync(data);
                    yield return req;

                    this.bundle = req.assetBundle;
                    this.done = true;
                }
                else
                {
                    this.bundle = www.assetBundle;
                    this.done = true;
                    yield break;
                }
            }
            else
            {
                this.error = Autarky.error;
                this.done = true;
                yield break;
            }
        }

        private void Begin()
        {
            this.done = false;
            this.error = "";
            this.bundle = null;
        }
    }

    public class BundleDownloader
    {
        private string mAssetHostURL = "";
        private Dictionary<string, BundleInfo> mAssetManifest = new Dictionary<string, BundleInfo>();
        private List<string> mDownloadNames = new List<string>();
        private Dictionary<string, DownloadTask> mDownloadList = new Dictionary<string, DownloadTask>();

        #region Properties
        public float progress
        {
            get
            {
                if (this.mDownloadList.Count == 0)
                    return 1.0f;

                float all = 0;
                float sum = 0.0f;
                foreach (var download in this.mDownloadList.Values)
                {
                    all += 1.0f;
                    sum += download.done ? 1.0f : (download.www != null ? download.www.progress : 0.0f);
                }
                return (int)((sum / all) * 100) / 100.0f;
            }
        } 
        #endregion

        #region Public Methods
        public bool CheckVersion(string hostUrl, string manifest)
        {
            Debug.Log("Asset Manifest : " + manifest);

            this.mAssetHostURL = hostUrl;
            if (!this.ParseManifest(manifest))
            {
                Debug.LogError("Invalid AssetManifest syntax.");
                return false;
            }

            return this.CheckDownloadList(this.mDownloadNames);
        }

        public void UpdateVersion()
        {
            this.DownloadAll(this.mDownloadNames);
        }

        public void DeleteLocalBundles()
        {
            string home = AssetPath.LocalBundleHome;
            if (System.IO.Directory.Exists(home))
            {
                System.IO.Directory.Delete(home, true);
            }
        }
        #endregion

        #region Private Methods
        private bool ParseManifest(string manifest)
        {
            this.mAssetManifest.Clear();
            string[] segments = manifest.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string seg in segments)
            {
                try
                {
                    string[] fragments = seg.Split(new string[] { "=" }, System.StringSplitOptions.RemoveEmptyEntries);

                    string name = fragments[0];
                    int version = int.Parse(fragments[1]);

                    BundleInfo info = new BundleInfo();
                    info.remoteVersion = version;
                    this.mAssetManifest[name] = info;
                }
                catch
                {
                    Debug.LogError("The asset-manifest is invalid at ��" + seg);
                    return false;
                }
            }
            return true;
        }

        private bool CheckDownloadList(List<string> downloadNames)
        {
            downloadNames.Clear();
            foreach (var node in this.mAssetManifest)
            {
                string bundleName = node.Key;
                BundleInfo bundleInfo = node.Value;
                if (this.NeedDownloadBundle(bundleName, bundleInfo))
                {
                    downloadNames.Add(bundleName);
                    Debug.LogWarningFormat(
                        "Local bundle [{0}] is outdated. localVersion={1}, remoteVersion={2}",
                        bundleName, bundleInfo.localVersion, bundleInfo.remoteVersion);
                }
            }
            return downloadNames.Count > 0;
        }

        private void DownloadAll(List<string> bundleNames)
        {
            if (string.IsNullOrEmpty(this.mAssetHostURL) || bundleNames.Count == 0)
                return;
            App.instance.StartCoroutine(this.DownloadAllAsync(this.mAssetHostURL, bundleNames));
        }

        private IEnumerator DownloadAllAsync(string assetHostURL, List<string> bundleNames)
        {
            foreach (var bundleName in bundleNames)
            {
                string url = System.IO.Path.Combine(assetHostURL, bundleName + ".bundle");
                DownloadTask download = new DownloadTask(url);
                this.mDownloadList[bundleName] = download;
            }
            foreach (var bundleName in bundleNames)
            {
                DownloadTask download = null;
                this.mDownloadList.TryGetValue(bundleName, out download);
                if (download == null)
                    continue;
                yield return download.Download();

                int count = 0;
                while (count <= 3 && !string.IsNullOrEmpty(download.www.error))
                {
                    count++;
                    yield return download.Download();
                }

                if (!string.IsNullOrEmpty(download.www.error))
                {
                    Debug.LogError(download.www.error + "\n" + download.url);
                    yield break;
                }

                byte[] data = download.www.bytes;
                Autarky.Signature sig_v3;
                if (Autarky.DecodeSignature(data, out sig_v3))
                {
                    BundleInfo info = null;
                    if (this.mAssetManifest.TryGetValue(bundleName, out info))
                    {
                        info.localVersion = sig_v3.version;
                    }
                    string localBundlePath = AssetPath.LocalBundlePath(bundleName);
                    if (System.IO.File.Exists(localBundlePath))
                    {
                        System.IO.File.Delete(localBundlePath);
                    }
                    XFile.WriteBytesFile(localBundlePath, data, 0, data.Length);
                    download.Done();
                    System.GC.Collect();
                }
                else
                {
                    Debug.LogErrorFormat(
                        "Bundle download successfully but file format is invalid. \n url : {0} \n bundleName : {1} \n error : {2}",
                        assetHostURL, bundleName, Autarky.error);
                }
            }
        }

        private bool NeedDownloadBundle(string bundleName, BundleInfo bundleInfo)
        {
            if (bundleInfo.localVersion != 0 && bundleInfo.localVersion >= bundleInfo.remoteVersion)
                return false;

            if (this.GetLocalBundleInfo(bundleName, ref bundleInfo))
            {
                if (bundleInfo.localVersion >= bundleInfo.remoteVersion)
                    return false;
            }

            return true;
        }

        private bool GetLocalBundleInfo(string bundleName, ref BundleInfo info)
        {
            string bundlePath = AssetPath.LocalBundlePath(bundleName);

            Autarky.Signature sig_v3;
            if (Autarky.DecodeSignature(bundlePath, out sig_v3))
            {
                info.localVersion = sig_v3.version;
                return true;
            }

            return false;
        }

        private bool GetBundleInfo(byte[] data, ref BundleInfo info)
        {
            Autarky.Signature sig_v3;
            if (Autarky.DecodeSignature(data, out sig_v3))
            {
                info.localVersion = sig_v3.version;
                return true;
            }
            return false;
        } 
        #endregion
    }

    public class BundleLoader
    {
        private Dictionary<string, BundleLoading> mBundleAsyncLoadings = new Dictionary<string, BundleLoading>();

        public BundleLoading NewBundleAsyncLoading(string bundleName)
        {
            var loading = new BundleLoading();
            this.mBundleAsyncLoadings.Add(bundleName, loading);
            return loading;
        }

        public BundleLoading GetBundleAsyncLoading(string bundleName)
        {
            BundleLoading loading = null;
            this.mBundleAsyncLoadings.TryGetValue(bundleName, out loading);
            return loading;
        }

        public void RemoveBundleAsyncLoading(string bundleName)
        {
            this.mBundleAsyncLoadings.Remove(bundleName);
        }
    }

    public class BundleCache
    {
        private Dictionary<string, AssetBundle> mBundleCache = new Dictionary<string, AssetBundle>();
        private List<AssetBundle> mFreqBundleList = new List<AssetBundle>();

        public void Put(string bundleName, AssetBundle assetBundle)
        {
            if (assetBundle == null)
                return;

            AssetBundle bundle = null;
            this.mBundleCache.TryGetValue(bundleName, out bundle);
            if (bundle != null)
            {
                bundle.Unload(false);
            }

            this.mBundleCache[bundleName] = assetBundle;
            this.mFreqBundleList.Add(bundle);
        }

        public AssetBundle Get(string bundleName)
        {
            AssetBundle bundle = null;
            this.mBundleCache.TryGetValue(bundleName, out bundle);
            if (bundle == null)
                return null;
            this.mFreqBundleList.Add(bundle);
            return bundle;
        }

        public void UnloadUnused(int cacheSize)
        {
            if (this.mFreqBundleList.Count <= cacheSize)
                return;

            while (this.mFreqBundleList.Count > cacheSize)
                this.mFreqBundleList.RemoveAt(0);

            Dictionary<string, AssetBundle> deletions = new Dictionary<string, AssetBundle>();
            foreach (var node in this.mBundleCache)
            {
                if (this.mFreqBundleList.Contains(node.Value))
                    continue;
                deletions.Add(node.Key, node.Value);
            }
            foreach (var node in deletions)
            {
                string bundleName = node.Key;
                AssetBundle bundle = node.Value;
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
                this.mBundleCache.Remove(bundleName);
            }
            deletions.Clear();
        }
    }

    public class AssetCache
    {
        private Dictionary<string, Dictionary<string, Object>> mAssetCache = new Dictionary<string, Dictionary<string, Object>>();
        private List<Object> mFreqAssetList = new List<Object>();

        public T Get<T>(string bundleName, string location) where T : Object
        {
            Dictionary<string, Object> category = null;
            if (!this.mAssetCache.TryGetValue(bundleName, out category))
                return null;

            Object asset = null;
            category.TryGetValue(location, out asset);
            if (asset == null)
                return null;

            this.mFreqAssetList.Add(asset);
            return asset as T;
        }

        public void Put(string bundleName, string location, Object asset)
        {
            Dictionary<string, Object> category = null;
            if (!this.mAssetCache.TryGetValue(bundleName, out category))
            {
                category = new Dictionary<string, Object>();
                this.mAssetCache.Add(bundleName, category);
            }

            if (!category.ContainsKey(location))
            {
                category.Add(location, asset);
                this.mFreqAssetList.Add(asset);
            }
        }

        public void UnloadUnused(int cacheSize = 0)
        {
            if (this.mFreqAssetList.Count <= cacheSize)
                return;

            while (this.mFreqAssetList.Count > cacheSize)
                this.mFreqAssetList.RemoveAt(0);

            Dictionary<string, Object> deletions = new Dictionary<string, Object>();
            foreach (var categoryNode in this.mAssetCache)
            {
                deletions.Clear();
                var category = categoryNode.Value;
                foreach (var assetNode in category)
                {
                    string location = assetNode.Key;
                    Object asset = assetNode.Value;
                    if (this.mFreqAssetList.Contains(asset))
                        continue;
                    deletions.Add(location, asset);
                }
                foreach (var deleteNode in deletions)
                {
                    category.Remove(deleteNode.Key);
                }
                deletions.Clear();
            }

            Resources.UnloadUnusedAssets();
        }
    }

    public class AssetWaitingQueue
    {
        private Dictionary<string, List<System.Delegate>> mWaitings = new Dictionary<string, List<System.Delegate>>();

        public int waitingCount
        {
            get
            {
                int result = 0;
                foreach (var waiting in this.mWaitings.Values)
                {
                    if (waiting == null)
                        continue;
                    result += waiting.Count;
                }
                return result;
            }
        }

        public bool Wait<T>(string bundleName, string location, System.Action<T> handler) where T : Object
        {
            List<System.Delegate> loadings = null;
            this.mWaitings.TryGetValue(location, out loadings);
            if (loadings == null)
            {
                loadings = new List<System.Delegate>();
                this.mWaitings.Add(location, loadings);
            }
            loadings.Add(handler);
            return loadings.Count > 1;
        }

        public void Notify<T>(string location, T asset) where T : Object
        {
            List<System.Delegate> loadings = null;
            this.mWaitings.TryGetValue(location, out loadings);
            if (loadings == null || loadings.Count == 0)
                return;
            foreach (var l in loadings)
            {
                if (l != null)
                {
                    l.DynamicInvoke(asset);
                }
            }
            loadings.Clear();
        }
    }

    public class GameObjectPool
    {
        private Dictionary<string, Dictionary<string, LinkedList<GameObject>>> mPool = new Dictionary<string, Dictionary<string, LinkedList<GameObject>>>();
        private int mCountPerGameObject = 3;

        public GameObject Pop(string bundleName, string prefabName)
        {
            Dictionary<string, LinkedList<GameObject>> category = null;
            if (!this.mPool.TryGetValue(bundleName, out category))
                return null;
            LinkedList<GameObject> slot = null;
            if (!category.TryGetValue(prefabName, out slot))
                return null;
            if (slot == null || slot.Count == 0)
                return null;
            GameObject go = slot.First.Value;
            slot.RemoveFirst();
            return go;
        }

        public void Push(string bundleName, GameObject go)
        {
            Dictionary<string, LinkedList<GameObject>> category = null;
            if (!this.mPool.TryGetValue(bundleName, out category))
            {
                category = new Dictionary<string, LinkedList<GameObject>>();
                this.mPool.Add(bundleName, category);
            }

            string prefabName = XUtility.CutCloneMarkForName(go.name);
            LinkedList<GameObject> slot = null;
            if (!category.TryGetValue(prefabName, out slot))
            {
                slot = new LinkedList<GameObject>();
                category.Add(prefabName, slot);
            }

            if (slot.Count < this.mCountPerGameObject)
            {
                slot.AddLast(go);
            }
            else
            {
                Object.Destroy(go);
            }
        }

        public void Clear()
        {
            foreach (var category in this.mPool.Values)
            {
                foreach (var slot in category.Values)
                {
                    foreach (var go in slot)
                    {
                        Object.Destroy(go);
                    }
                }
            }
            this.mPool.Clear();
        }
    }

    public class AssetManager
    {
        private BundleDownloader mBundleDownloader = new BundleDownloader();

        private BundleCache mBundleCache = new BundleCache();
        private BundleLoader mBundleLoader = new BundleLoader();

        private AssetCache mAssetCache = new AssetCache();
        private AssetWaitingQueue mAssetWaitings = new AssetWaitingQueue();

        private GameObjectPool mGameObjectPool = new GameObjectPool();

        private int mAssetCacheSize = 100;
        private int mBundleCatheSize = 2;

        private bool mIsLoadingScene = false;
        private string mLoadingSceneName = "";

        #region Properties
        public int bundleCacheSize
        {
            get { return this.mBundleCatheSize; }
            set { this.mBundleCatheSize = value; }
        }
        public int assetCacheSize
        {
            get { return this.mAssetCacheSize; }
            set { this.mAssetCacheSize = value; }
        }
        public bool isLoading
        {
            get
            {
                return
                    this.mIsLoadingScene ||
                    this.mAssetWaitings.waitingCount > 0;
            }
        }
        public string loadedSceneName
        {
            get { return this.mLoadingSceneName; }
        }
        #endregion

        #region Life Circle
        public bool Init()
        {
            return true;
        }

        public void Quit()
        {
            this.UnloadUnusedAssets();
        }

        public void Tick(float elapse)
        { }
        #endregion

        #region Public Methods
        public GameObject AcquireGameObject(string bundleName, string prefabName)
        {
            GameObject go = mGameObjectPool.Pop(bundleName, prefabName);
            if (go != null)
            {
                go.SetActive(true);
                return go;
            }

            GameObject prefab = this.LoadPrefab(bundleName, prefabName);
            if (prefab == null)
                return null;

            go = Object.Instantiate(prefab) as GameObject;
            return go;
        }

        public void AcquireGameObjectAsync(string bundleName, string prefabName, System.Action<GameObject> handler)
        {
            GameObject go = mGameObjectPool.Pop(bundleName, prefabName);
            if (go != null)
            {
                go.SetActive(true);
                XCSharp.InvokeAction(handler, go);
                return;
            }

            this.LoadPrefabAsync(bundleName, prefabName, prefab =>
            {
                if (prefab == null)
                {
                    XCSharp.InvokeAction(handler, null);
                    return;
                }
                GameObject result = Object.Instantiate(prefab) as GameObject;
                XCSharp.InvokeAction(handler, result);
            });
        }

        public void ReleaseGameObject(string bundleName, GameObject instance)
        {
            if (string.IsNullOrEmpty(bundleName) || instance == null)
                return;
            instance.transform.parent = null;
            instance.SetActive(false);
            mGameObjectPool.Push(bundleName, instance);
        }

        public GameObject LoadPrefab(string bundleName, string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;
            return this.LoadAsset<GameObject>(bundleName, "Prefabs/" + prefabName);
        }

        public void LoadPrefabAsync(string bundleName, string prefabName, System.Action<GameObject> handler)
        {
            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(prefabName))
            {
                XCSharp.InvokeAction(handler, null);
                return;
            }
            this.LoadAssetAsync<GameObject>(bundleName, "Prefabs/" + prefabName, handler);
        }

        public void LoadAssetsToCacheAsync(Dictionary<string, List<string>> assets)
        {
            foreach (var categoryNode in assets)
            {
                var bundleName = categoryNode.Key;
                var locationList = categoryNode.Value;
                this.LoadAssetsToCacheAsync(bundleName, locationList);
            }
        }

        public void LoadAssetsToCacheAsync(string bundleName, List<string> locationList)
        {
            foreach (var location in locationList)
            {
                if (string.IsNullOrEmpty(location))
                    continue;
                this.LoadAssetAsync<Object>(bundleName, location, null);
            }
        }

        public Object LoadAsset(string path)
        {
            return this.LoadAsset<Object>(path);
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            int index = path.IndexOf('/');
            if (index <= 0 || index >= path.Length - 1)
                return null;
            string bundleName = path.Substring(0, index);
            string location = path.Substring(index + 1);
            return this.LoadAsset<T>(bundleName, location);
        }

        public T LoadAsset<T>(string bundleName, string location) where T : Object
        {
            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(location))
            {
                Debug.LogError("LoadAsset: bundle name or location is null or empty. package=" + bundleName + " location=" + location);
                return null;
            }

            T asset = mAssetCache.Get<T>(bundleName, location);
            if (asset != null)
                return asset;

            string assetPath = bundleName + "/" + location;
            asset = Resources.Load<T>(assetPath);
            if (asset != null)
            {
                mAssetCache.Put(bundleName, location, asset);
                return asset;
            }

            AssetBundle bundle = this.LoadBundle(bundleName);
            if (bundle != null)
            {
                string assetName = System.IO.Path.GetFileName(location);
                asset = bundle.LoadAsset<T>(assetName);
            }
            if (asset != null)
            {
                mAssetCache.Put(bundleName, location, asset);
                return asset;
            }

            Debug.LogError("LoadAsset: failed. bundle=" + bundleName + " location=" + location);
            return null;
        }

        public void LoadAssetAsync<T>(string path, System.Action<T> handler) where T : Object
        {
            int index = path.IndexOf('/');
            if (index <= 0 || index >= path.Length - 1)
            {
                XCSharp.InvokeAction(handler, null);
                return;
            }

            string bundleName = path.Substring(0, index);
            string location = path.Substring(index + 1);
            this.LoadAssetAsync<T>(bundleName, location, handler);
        }

        public void LoadAssetAsync<T>(string bundleName, string location, System.Action<T> handler) where T : Object
        {
            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(location))
            {
                Debug.LogError("LoadAssetAsync: bundle name or location is null or empty. bundle=" + bundleName + "\n location=" + location);
                XCSharp.InvokeAction(handler, null);
                return;
            }

            App.instance.StartCoroutine(this.CreateAssetAsync<T>(bundleName, location, handler));
        }

        public void LoadSceneAsync(string sceneName, System.Action<string> handler)
        {
            App.instance.StartCoroutine(this.CreateSceneAsync(sceneName, handler));
        }

        public AssetBundle LoadBundle(string bundleName)
        {
            AssetBundle bundle = mBundleCache.Get(bundleName);
            if (bundle != null)
                return bundle;

            BundleLoading loading = new BundleLoading();
            loading.Load(bundleName);
            bundle = loading.bundle;

            mBundleCache.Put(bundleName, bundle);

            return bundle;
        }

        public void ClearUnusedGameObjects()
        {
            mGameObjectPool.Clear();
        }

        public void UnloadUnusedBundlesWithCache()
        {
            this.UnloadUnusedBundles(this.mBundleCatheSize);
        }

        public void UnloadUnusedAssetsWithCache()
        {
            this.UnloadUnusedAssets(this.mAssetCacheSize);
        }

        public void UnloadUnusedBundles(int cacheSize = 0)
        {
            mBundleCache.UnloadUnused(cacheSize);
        }

        public void UnloadUnusedAssets(int cacheSize = 0)
        {
            mAssetCache.UnloadUnused(cacheSize);
        }
        #endregion

        #region Private Methods
        private IEnumerator CreateAssetAsync<T>(string bundleName, string location, System.Action<T> handler) where T : Object
        {
            // get from cache
            {
                T asset = mAssetCache.Get<T>(bundleName, location);
                if (asset != null)
                {
                    XCSharp.InvokeAction(handler, asset);
                    yield break;
                }
            }

            // async begins
            if (mAssetWaitings.Wait(bundleName, location, handler))
                yield break;

            string assetPath = bundleName + "/" + location;
            var req = Resources.LoadAsync<T>(assetPath);
            yield return req;
            if (req.asset != null)
            {
                mAssetCache.Put(bundleName, location, req.asset);
                mAssetWaitings.Notify(location, req.asset as T);
                yield break;
            }

            AssetBundle bundle = mBundleCache.Get(bundleName);
            if (bundle == null)
            {
                BundleLoading loading = mBundleLoader.GetBundleAsyncLoading(bundleName);
                if (loading != null)
                {
                    while (!loading.done)
                        yield return null;
                    bundle = loading.bundle;
                }
                else
                {
                    loading = mBundleLoader.NewBundleAsyncLoading(bundleName);
                    yield return App.instance.StartCoroutine(loading.LoadAsync(bundleName));
                    if (!string.IsNullOrEmpty(loading.error))
                    {
                        mAssetWaitings.Notify(location, null as T);
                        yield break;
                    }
                    bundle = loading.bundle;
                    if (bundle != null)
                    {
                        mBundleCache.Put(bundleName, bundle);
                    }
                }
                mBundleLoader.RemoveBundleAsyncLoading(bundleName);
            }
            if (bundle == null)
            {
                mAssetWaitings.Notify(location, null as T);
                yield break;
            }

            {
                string assetName = System.IO.Path.GetFileName(location);
                var request = bundle.LoadAssetAsync<T>(assetName);
                yield return request;

                // check if the bundle is destroyed by extern code.
                if (bundle == null)
                {
                    mAssetWaitings.Notify(location, null as T);
                    yield break;
                }

                // get asset now.
                if (request.asset != null)
                {
                    mAssetCache.Put(bundleName, location, request.asset);
                }
                mAssetWaitings.Notify(location, request.asset as T);
            }
        }

        private IEnumerator CreateSceneAsync(string sceneName, System.Action<string> handler = null)
        {
            this.mIsLoadingScene = true;
            this.mLoadingSceneName = sceneName;

            string bundleName = this.GetBundleNameFromLevelName(sceneName);
            if (string.IsNullOrEmpty(bundleName))
            {
                var oper = SceneManager.LoadSceneAsync(sceneName);
                yield return oper;
            }
            else
            {
                AssetBundle bundle = mBundleCache.Get(bundleName);
                if (bundle == null)
                {
                    BundleLoading loading = new BundleLoading();
                    yield return App.instance.StartCoroutine(loading.LoadAsync(bundleName));
                    if (!string.IsNullOrEmpty(loading.error))
                    {
                        Debug.LogError(loading.error);
                        this.mIsLoadingScene = false;
                        XCSharp.InvokeAction(handler, "");                        
                        yield break;
                    }

                    bundle = loading.bundle;
                }
                if (bundle == null)
                {
                    this.mIsLoadingScene = false;
                    XCSharp.InvokeAction(handler, "");
                    yield break;
                }

                var oper = SceneManager.LoadSceneAsync(sceneName);
                yield return oper;                
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
            }

            this.mIsLoadingScene = false;
            XCSharp.InvokeAction(handler, sceneName);
        }

        #region Others
        private string GetBundleNameFromLevelName(string levelName)
        {
            string[] segments = levelName.Split(new string[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return "";
            return segments[0];
        }
        #endregion
        #endregion
    }
}
