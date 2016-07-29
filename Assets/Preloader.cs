using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using AssetBundles;

public class Preloader : MonoBehaviour {

	const string sceneAssetBundle = "scene-bundle";
	const string variantSceneName = "resolutions";
	private string activeVariant;
	private bool bundlesLoaded;				// used to remove the loading buttons

	void Awake ()
	{
		bundlesLoaded = false;
	}

	void OnGUI ()
	{
		//sample logic to determine neede asset bundle variant
		//use device capabilities instead
		if (!bundlesLoaded)
		{
			GUILayout.Space (20);
			GUILayout.BeginHorizontal ();
			GUILayout.Space (20);
			GUILayout.BeginVertical ();
			if (GUILayout.Button ("Load SD"))
			{
				activeVariant = "sd";
				bundlesLoaded = true;
				StartCoroutine (BeginExample ());
				BeginExample ();
			}
			GUILayout.Space (5);
			if (GUILayout.Button ("Load HD"))
			{
				activeVariant = "hd";
				bundlesLoaded = true;
				StartCoroutine (BeginExample ());
				Debug.Log ("Loading HD");
			}
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}
	}


	//https://docs.unity3d.com/ScriptReference/AssetBundle.html
	private AssetBundle lastAssetBundle = null;

	IEnumerator LoadStreamingAssetBundle(string bundleName, string variant, string bundleGroup)
	{
		lastAssetBundle = null;

		if (variant != null)
		{
			bundleName = bundleName + "." +  activeVariant;
		}

		var path = Application.streamingAssetsPath;
		//https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
		if (path.Contains("://"))
		{
			path += "/" + Utility.GetPlatformName () + "/";
			if (bundleGroup != null)
			{
				path +=  bundleGroup + "/";
			}
			path += bundleName;

			Debug.Log ("Loading asset bundle from URL: " + path);
			WWW www = WWW.LoadFromCacheOrDownload (path, 1);
			yield return www;
			if (www.error != null)
			{
				Debug.LogError(www.error);
			}

			lastAssetBundle = www.assetBundle;
		}
		else
		{
			path = Path.Combine(path, Utility.GetPlatformName ());
			if (bundleGroup != null)
			{
				path = Path.Combine (path, Path.Combine (bundleGroup, bundleName));
			}
			else
			{
				path = Path.Combine (path, bundleName);
			}

			var request = AssetBundle.LoadFromFileAsync (path);
			yield return request;
			lastAssetBundle = request.assetBundle;
		}
			
	}

	IEnumerator BeginExample ()
	{
		//first load selected bundle variant
		yield return LoadStreamingAssetBundle ("myassets", activeVariant, "variants");
		yield return lastAssetBundle.LoadAllAssetsAsync ();

		//load content, dependent on variable assest, works only if dependent content loaded is loaded dynamically from another bundle
		//content can be scene or prefab
		yield return LoadStreamingAssetBundle ("scene-bundle", null, null);
		yield return lastAssetBundle.LoadAllAssetsAsync ();

		yield return SceneManager.LoadSceneAsync ("resolutions", LoadSceneMode.Additive);
	}		
}