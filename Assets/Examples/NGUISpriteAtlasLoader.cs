using UnityEngine;
using System.Collections;
using Bedivere.AssetBundles;
using UniRx;
using System.IO;

public class NGUISpriteAtlasLoader : MonoBehaviour 
{
    public UISprite spritePrefab;
    public UIGrid parent;

    const string CLOUD_DOWNLOAD_URL = "https://s3-ap-southeast-1.amazonaws.com/assets.touchten.com/picture-pop";
    public AssetBundleManager bundleManager { get { return AssetBundleManager.Instance; }}

    public void GetAssets(string bundleName, string atlasName, IProgress<float> progressNotifier)
    {
        string downloadURL = Path.Combine(Utility.STREAMING_ASSETS_PATH, "Android");
        downloadURL = System.Uri.EscapeUriString(downloadURL);
        if (!downloadURL.Contains("://"))
            downloadURL = "file://" + downloadURL;
        
        Debug.LogFormat("GetAssets {0} from {1}", bundleName, downloadURL);

        var query = 
            from manifest in bundleManager.LoadAssetBundleManifestStream(downloadURL)
            from bundle in bundleManager.LoadAssetBundleStream(downloadURL, bundleName, progressNotifier)
            select bundle;

        query.Subscribe(
            res => 
            {
                UIAtlas atlas = res.LoadAsset<GameObject>(atlasName).GetComponent<UIAtlas>();

                BetterList<string> sprites = atlas.GetListOfSprites();
                for (int i = 0; i < sprites.size; i++)
                {
                    UISprite spr = GameObject.Instantiate<UISprite>(spritePrefab);
                    spr.transform.SetParent(parent.transform);
                    spr.transform.localScale = Vector3.one;
                    spr.transform.localPosition = Vector3.zero;

                    spr.atlas = atlas;
                    spr.spriteName = sprites[i];
                }
                    
                parent.Reposition();
            }
        );
    }

    public void OnGetAssetsClicked()
    {
        var progressNotifier = new ScheduledNotifier<float>();
        GetAssets("asset2", "Atlas2", progressNotifier);
        progressNotifier.Subscribe(x => Debug.Log(x));
    }

    public void OnClearCacheClicked()
    {
        Debug.LogFormat("[AssetBundle]Clear Cache: {0}", Caching.CleanCache());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnGetAssetsClicked();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            OnClearCacheClicked();
        }
    }
}
