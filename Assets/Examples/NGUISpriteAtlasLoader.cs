using UnityEngine;
using System.Collections;
using Bedivere.AssetBundles;
using UniRx;

public class NGUISpriteAtlasLoader : MonoBehaviour 
{
    public UISprite spritePrefab;
    public UIGrid parent;

    #region constants
    const string CLOUD_DOWNLOAD_URL = "https://s3-ap-southeast-1.amazonaws.com/assets.touchten.com/picture-pop";
    #endregion
    public AssetBundleManager bundleManager { get { return AssetBundleManager.Instance; }}

    void GetAssets(string bundleName, string atlasName)
    {
        string downloadURL = CLOUD_DOWNLOAD_URL;

        var query = 
            from manifest in bundleManager.LoadAssetBundleManifestStream(downloadURL)
            from bundle in bundleManager.LoadAssetBundleStream(downloadURL, bundleName)
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetAssets("asset2", "Atlas2");
        }
    }
}
