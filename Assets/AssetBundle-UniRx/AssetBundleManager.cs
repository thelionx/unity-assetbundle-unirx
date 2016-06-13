using UnityEngine;
using System.Collections;
using UniRx;
using System.IO;
using System.Collections.Generic;
using System;

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

        public IObservable<AssetBundle> LoadAssetBundleStream(string baseDownloadURL, string bundleName)
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

                        var observable = ObservableWWW.LoadFromCacheOrDownload(url, manifest.GetAssetBundleHash(bundleName));
                        observable.Subscribe(
                            bundle =>
                            {
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

        void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            AssetBundle bundle = null;
            loadedBundles.TryGetValue(bundleName, out bundle);

            if (bundle != null)
            {
                bundle.Unload(unloadAllLoadedObjects);
                loadedBundles.Remove(bundleName);
            }
        }

        void IsCached()
        {
            Caching.IsVersionCached("ASD", 0);
        }

        void ClearCache()
        {
            Debug.LogFormat("Clean Cache : {0}", Caching.CleanCache());
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
