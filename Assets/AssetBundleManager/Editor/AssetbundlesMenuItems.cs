using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AssetBundles
{
	public class AssetBundlesMenuItems
	{
		[MenuItem ("Assets/AssetBundles/Build AssetBundles")]
		static public void BuildAssetBundles ()
		{
			BuildScript.BuildAssetBundles();
		}
	}
}