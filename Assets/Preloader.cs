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

	private AssetBundle lastAssetBundle = null;
	IEnumerator LoadAssetBundle(string bundleName, string variant, string bundleGroup)
	{
		lastAssetBundle = null;

		if (variant != null)
		{
			bundleName = bundleName + "." +  activeVariant;
		}

		var path = Application.streamingAssetsPath;
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
	// Use this for initialization
	IEnumerator BeginExample ()
	{
		yield return LoadAssetBundle ("myassets", activeVariant, "variants");
		yield return lastAssetBundle.LoadAllAssetsAsync ();
		yield return LoadAssetBundle ("scene-bundle", null, null);
		yield return lastAssetBundle.LoadAllAssetsAsync ();
		yield return SceneManager.LoadSceneAsync ("resolutions", LoadSceneMode.Additive);
	}

	// Initialize the downloading url and AssetBundleManifest object.
	protected IEnumerator Initialize()
	{
		// Don't destroy this gameObject as we depend on it to run the loading script.
		DontDestroyOnLoad(gameObject);

		// With this code, when in-editor or using a development builds: Always use the AssetBundle Server
		// (This is very dependent on the production workflow of the project. 
		// 	Another approach would be to make this configurable in the standalone player.)
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
		//AssetBundleManager.SetDevelopmentAssetBundleServer ();
		Debug.Log("dataPath: " + Application.dataPath + " streamingPath: " + Application.streamingAssetsPath);
		AssetBundleManager.SetSourceAssetBundleDirectory(Utility.GetPlatformName() + "/");
		#else
		// Use the following code if AssetBundles are embedded in the project for example via StreamingAssets folder etc:
		AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
		// Or customize the URL based on your deployment or configuration
		//AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
		#endif

		// Initialize AssetBundleManifest which loads the AssetBundleManifest object.
		var request = AssetBundleManager.Initialize();

		if (request != null)
			yield return StartCoroutine(request);
	}
		
}