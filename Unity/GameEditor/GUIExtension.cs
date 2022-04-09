//-----------------------------------------------------------------------------
//- GUIExtension static Class
//-
//- Extend GUI functionalities
//-		IM Colored Button
//-		Sprite display
//-----------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class GUIExtension
{
	[System.Flags]
	public enum ListViewFlags
	{
		None		= 0,
		Manageable	= 1,
		Confirm		= 2
	}

	internal class ListViewData
	{
		public Vector2 scrollView;
	}

	

	public static bool Button(GUIContent content, Color c, params GUILayoutOption[] options)
	{
		Color old = GUI.backgroundColor;
		GUI.backgroundColor = c;
		bool pressed = GUILayout.Button(content, options);
		GUI.backgroundColor = old;
		return pressed;
	}

	public static bool Button(string content, Color c, params GUILayoutOption[] options)
	{
		return Button(new GUIContent(content), c, options);
	}

    public static void DrawSprite(Sprite sprite)
    {
        float w = sprite.textureRect.width;
        float h = sprite.textureRect.height;
        Rect r = GUILayoutUtility.GetRect(w, h);
        DrawSprite(r, sprite, new Vector2(w, h));
    }

    public static void DrawSprite(Sprite sprite, Vector2 size)
    {
        Rect r = GUILayoutUtility.GetRect(size.x, size.y);
        DrawSprite(r, sprite, size);
    }

    public static void DrawSprite(Rect position, Sprite sprite, Vector2 size)
	{
		Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
								   sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
		Vector2 actualSize = size;
		// actualSize.y *= (sprite.rect.height / sprite.rect.width);
		GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
	}

	public delegate void ViewDelegate<T>(int index, T e);

	private static void DefaultDeleteDelegate<T>(int indx, T e) { }

	public static int DrawView<T>(List<T> list, ViewDelegate<T> del, int current = -1, ListViewFlags flags = ListViewFlags.None)
	{
		return DrawView<T>(list, del, DefaultDeleteDelegate, current, flags);			
	}

	public static int DrawView<T>(List<T> list, ViewDelegate<T> del, ViewDelegate<T> removeDelegate, int current = -1, ListViewFlags flags = ListViewFlags.None)
	{
		bool managed = (flags & ListViewFlags.Manageable) != ListViewFlags.None ;

		int listCount = list.Count;
		int guiControl = GUIUtility.GetControlID(FocusType.Passive);
		ListViewData data = (ListViewData)  GUIUtility.GetStateObject(typeof(ListViewData), guiControl);
		int lastClicked = current;

		// GUILayout.BeginVertical();
		if ( managed )
		{
			//if ( GUIExtension.Button("New", EditorWindowExtended.ColorNew, GUILayout.Width(60f)) )
			//{
			//	list.Add(new T());
			//}
		}

		data.scrollView = EditorGUILayout.BeginScrollView(data.scrollView, GUILayout.ExpandHeight(false));
		for(int i = 0; i < listCount; ++i)
		{
			bool markForDelete = false;
			Color old = GUI.backgroundColor;
			if ( i == lastClicked )
			{
				GUI.backgroundColor = new Color(0.35f, 0.35f, 1f, 1f);
			}
			GUILayout.BeginHorizontal();
			GUI.backgroundColor = old;
			del(i, list[i]);

			if ( managed )
			{
				markForDelete = GUIExtension.Button("x", EditorWindowExtended.ColorAlert, GUILayout.Width(20f));
			}

			GUILayout.EndHorizontal();

			if (markForDelete)
			{
				bool confirmDelete = (flags & ListViewFlags.Confirm) != ListViewFlags.Confirm || EditorUtility.DisplayDialog("Warning", "Delete Item?", "Yes", "No");

				if ( confirmDelete )
				{
					if (lastClicked == i)
					{
						lastClicked = -1;
					}

					removeDelegate(i, list[i]);
					list.RemoveAt(i);
					break;
				}
			}

			Rect itemRect = GUILayoutUtility.GetLastRect();

			if ( Event.current.type == EventType.MouseDown )
			{
				if ( itemRect.Contains(Event.current.mousePosition) )
				{
					if ( lastClicked != i )
					{
						lastClicked = i;

						GUI.FocusControl("");

						Event.current.Use();
					}
				}
			}
		}
		EditorGUILayout.EndScrollView();
	//	GUILayout.EndVertical();
		return lastClicked;
	}
}