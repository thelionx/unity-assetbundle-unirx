using UnityEngine;
using System.Collections;
using UniRx;
using System.IO;

namespace Bedivere.AssetBundle
{
    public class AssetBundleManager : MonoBehaviour 
    {
        const string BASE_DOWNLOAD_URL = "https://s3-ap-southeast-1.amazonaws.com/assets.touchten.com/target+acquired/Android/";

        private AssetBundleManifest manifest;
        void LoadAssetBundleManifest()
        {
            string url = Path.Combine(BASE_DOWNLOAD_URL, Utility.GetPlatformName());

            Debug.Log("Load Asset Bundle Manifest : " + url);
            var observable = ObservableWWW.GetWWW(url);
            observable.Subscribe(
                www => 
                {
                    manifest = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    string[] bundles = manifest.GetAllAssetBundles();
                    for (int i = 0; i < bundles.Length; i++)
                    {
                        Debug.Log(manifest.GetAssetBundleHash(bundles[i]));
                    }
                },
                error =>
                {
                    Debug.Log(error.Message);
                }
            );

        }

        void LoadAssetBundle(string bundleName)
        {
            string url = Path.Combine(BASE_DOWNLOAD_URL, bundleName);
            if (Caching.IsVersionCached(url, manifest.GetAssetBundleHash(bundleName)))
                Debug.LogFormat("{0} is alredy cached", bundleName);
            else
                Debug.LogFormat("{0} is not cached", bundleName);

            var observable = ObservableWWW.LoadFromCacheOrDownload(url, manifest.GetAssetBundleHash(bundleName));
            observable.Subscribe(bundle =>
                {
                    string[] names = bundle.GetAllAssetNames();
                    for (int i = 0; i < names.Length; i++)
                    {
                        Debug.Log(names[i]);
                    }
                }
            );
        }

        void IsCached()
        {
            Caching.IsVersionCached("ASD", 0);
        }

        void ClearCache()
        {
            Debug.LogFormat("Clean Cache : {0}", Caching.CleanCache());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
                LoadAssetBundleManifest();

            if (Input.GetKeyDown(KeyCode.S))
                LoadAssetBundle(manifest.GetAllAssetBundles()[0]);

            if (Input.GetKeyDown(KeyCode.D))
                ClearCache();
        }
    }
}
