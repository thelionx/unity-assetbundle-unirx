using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Bedivere.AssetBundles
{
    public class AssetBundlesMenuItems
    {
        const string kSimulationMode = "Assets/Asset Bundles - UniRx/Toggle Simulation Mode";

        [MenuItem(kSimulationMode)]
        public static void ToggleSimulationMode()
        {
            AssetBundlePreferences.SimulateAssetBundleInEditor = !AssetBundlePreferences.SimulateAssetBundleInEditor;
        }

        [MenuItem(kSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(kSimulationMode, AssetBundlePreferences.SimulateAssetBundleInEditor);
            return true;
        }

        [MenuItem("Assets/Asset Bundles - UniRx/Build AssetBundles")]
        static public void BuildAssetBundles()
        {
            BuildScript.BuildAssetBundles();
        }
    }
}
