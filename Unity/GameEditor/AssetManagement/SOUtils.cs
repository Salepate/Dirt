using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Framework
{
	public static class SOUtils
	{
		public static DerivedSO To<DerivedSO>(ScriptableObject obj) where DerivedSO : ScriptableObject
		{
			Debug.Assert(obj != null, "No Scriptable Object provided");
			Debug.Assert(obj.GetType() == typeof(DerivedSO), 
				"Invalid Scriptable Object: " + obj.GetType().Name + " (" + typeof(DerivedSO).Name +" expected)");

			return (DerivedSO)obj;
		}

		public static void RemoveScriptableObjectFrom(Object obj, ScriptableObject so)
		{
			if (AssetDatabase.GetAssetPath(so) != "")
			{
				GameObject.DestroyImmediate(so, true);
				AssetDatabase.SaveAssets();
			}
			else
			{
				Debug.LogWarning("ScriptableObject is not in this object");
			}
		}

		public static void RenameScriptable(ScriptableObject so, string newName)
		{
			string path = AssetDatabase.GetAssetPath(so);
			AssetDatabase.ClearLabels(so);
			AssetDatabase.SetLabels(so, new string[] { newName });
			AssetDatabase.RenameAsset(path, newName);
			AssetDatabase.SaveAssets();
		}

		public static void DeleteScriptable(ScriptableObject so)
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(so));
			AssetDatabase.SaveAssets();
		}

		public static void AddScriptableObjectTo(Object obj, ScriptableObject so, string subAssetName)
		{
			if (AssetDatabase.GetAssetPath(so) == "")
			{
				AssetDatabase.AddObjectToAsset(so, obj);

				if ( subAssetName !=null )
				{
					AssetDatabase.ClearLabels(so);
					so.name = subAssetName;
					AssetDatabase.SetLabels(so, new string[] { subAssetName });
				}

				AssetDatabase.SaveAssets();
			}
			else
			{
				Debug.LogWarning("ScriptableObject is already attached");
			}
		}
		public static void AddScriptableObjectTo(Object obj, ScriptableObject so) { AddScriptableObjectTo(obj, so, null); }

		public static T CreateScriptableObject<T>(string path, System.Type subType) where T : ScriptableObject
		{
			T obj = null;


			if ( typeof(T).IsAssignableFrom(subType) )
			{
				obj = (T)ScriptableObject.CreateInstance(subType);


				if (path == null || path.Length < 1)
				{

					path = "Assets/ScriptableObjects/" + (typeof(T).ToString()) + ".asset";
				}

				path = AssetDatabase.GenerateUniqueAssetPath(path);
				SaveScriptableObject(obj, path);
			}
			else
			{
				Debug.LogError("Subtype " + subType.Name + " does not inherit from " + typeof(T).Name);
			}

			return obj;
		}

		public static T CreateScriptableObject<T>(string path) where T : ScriptableObject
		{
			T obj = ScriptableObject.CreateInstance<T>();

			if (path == null || path.Length < 1)
			{

				path = "Assets/ScriptableObjects/" + (typeof(T).ToString()) + ".asset";
			}

			path = AssetDatabase.GenerateUniqueAssetPath(path);
			SaveScriptableObject(obj, path);

			return obj;
		}

		public static void SaveScriptableObject(ScriptableObject scriptObj, string path)
		{
			AssetDatabase.CreateAsset(scriptObj, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		public static List< T > GetAllScriptables<T>(string path) where T : ScriptableObject
		{
			string[] assets = AssetDatabase.FindAssets("t:" + typeof(T).ToString(), new string[] { path });
			List<T> results = new List<T>();

			for(int i = 0; i < assets.Length; ++i)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
				T inst = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if ( inst != null)
				{
					results.Add(inst);
				}
			}

			return results;
		}
	}
}