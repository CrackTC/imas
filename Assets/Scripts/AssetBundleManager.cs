using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

static class AssetBundleManager
{
    private static readonly Dictionary<string, AssetBundle> LoadedAssetBundles = new();

    private static IEnumerator Load(string path, Action<AssetBundle> cont)
    {
#if UNITY_EDITOR
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        var request = AssetBundle.LoadFromFileAsync(fullPath);
        yield return request;
        cont(request.assetBundle);
#else
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(fullPath);
        yield return request.SendWebRequest();
        cont(UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request));
#endif
    }

    public static IEnumerator LoadAssetBundle(string path, Action<AssetBundle> cont)
    {
        if (LoadedAssetBundles.TryGetValue(path, out var ab))
        {
            while (ab == null)
            {
                yield return null;
                ab = LoadedAssetBundles[path];
            }
            cont(ab);
            yield break;
        }

        LoadedAssetBundles[path] = null;
        yield return Load(
            path,
            ab =>
            {
                LoadedAssetBundles[path] = ab;
                cont(ab);
            }
        );
    }
}
