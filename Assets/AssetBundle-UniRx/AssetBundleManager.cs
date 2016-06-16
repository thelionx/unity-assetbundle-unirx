using UnityEngine;
using System.Collections;
using UniRx;
using System.IO;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bedivere.AssetBundles
{
    public class AssetBundleManager : MonoBehaviour 
    {
        public static AssetBundleManager Instance;

        private AssetBundleManifest manifest;
        private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

        void Awake()
        {
            Instance = this;
        }

        public IObservable<AssetBundleManifest> LoadAssetBundleManifestStream(string baseDownloadURL)
        {
            return Observable.Create<AssetBundleManifest> (
                stream =>
                {
                    if (manifest) {
                        stream.OnNext(manifest);
                        stream.OnCompleted();
                    }
                    else
                    {
                        string url = Path.Combine(baseDownloadURL, Utility.GetPlatformName());

                        var observable = ObservableWWW.GetWWW(url);
                        observable.Subscribe(
                            www => 
                            {
                                manifest = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                                stream.OnNext(manifest);
                                stream.OnCompleted();
                            },
                            error => stream.OnError(error)
                        );
                    }

                    return Disposable.Empty;
                }
            );
        }

        public IObservable<AssetBundle> LoadAssetBundleStream(string baseDownloadURL, string bundleName, IProgress<float> progressNotifier = null)
        {
            return Observable.Create<AssetBundle>(
                stream => 
                {
                    if (loadedBundles.ContainsKey(bundleName))
                    {
                        Debug.LogFormat("[AssetBundle]{0} is already loaded", bundleName);

                        AssetBundle bundle = loadedBundles[bundleName];
                        stream.OnNext(bundle);
                        stream.OnCompleted();
                    }
                    else
                    {
                        string url = Path.Combine(baseDownloadURL, bundleName);

                        Debug.LogFormat("[AssetBundle]Cached : {0} | {1}", Caching.IsVersionCached(url, manifest.GetAssetBundleHash(bundleName)), url); 

                        var observable = ObservableWWW.LoadFromCacheOrDownload(url, manifest.GetAssetBundleHash(bundleName), progressNotifier);
                        observable.Subscribe(
                            bundle =>
                            {
                                if (!loadedBundles.ContainsKey(bundleName))
                                    loadedBundles.Add(bundleName, bundle);
                                
                                stream.OnNext(bundle);
                                stream.OnCompleted();
                            },
                            error => stream.OnError(error)
                        ); 
                    }
                    return Disposable.Empty;
                }
            );
        }

        #if UNITY_EDITOR
        public static UnityEngine.Object EditorLoadAssetFromAssetBundle(string bundleName, string assetName, Type type)
        {
            string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
            return AssetDatabase.LoadAssetAtPath(paths[0], type);
        }
        #endif

        public void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            AssetBundle bundle = null;
            loadedBundles.TryGetValue(bundleName, out bundle);

            if (bundle != null)
            {
                Debug.LogFormat("[AssetBundle]Unloaded : {0}", bundleName);
                bundle.Unload(unloadAllLoadedObjects);
                loadedBundles.Remove(bundleName);
            }
        }

        public bool IsBundleCached(string baseDownloadURL, string assetBundleName)
        {
            return Caching.IsVersionCached(Path.Combine(baseDownloadURL, assetBundleName), manifest.GetAssetBundleHash(assetBundleName));
        }

        public void CleanCache()
        {
            Debug.LogFormat("[AssetBundle]Clean Cache : {0}", Caching.CleanCache());
        }

        void LoadAssetBundleFromFile(string bundleName)
        {
            string path = Path.Combine(Application.dataPath, Utility.AssetBundlesOutputPath.Replace("Assets/", ""));
            Debug.Log(Path.Combine(path, bundleName));
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(path, bundleName));

            UIAtlas atlas = bundle.LoadAsset<UIAtlas>("Atlas1");
            Debug.Log(atlas.texture);
        }
    }
}
