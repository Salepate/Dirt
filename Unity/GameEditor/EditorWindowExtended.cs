//-----------------------------------------------------------------------------
//- EditorWindowExtended Class
//-
//- Improved editor window class
//-		Folder system
//-		Color presets
//-----------------------------------------------------------------------------
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class EditorWindowExtended : EditorWindow
{
	// ctor
	public EditorWindowExtended()
	{
		m_Folders = new Dictionary<int, bool>();
        m_Tabs = new Dictionary<GUIContent[], int>();
	}

	//---------------------------------------------------------------------
	//- Unity Flow
	//---------------------------------------------------------------------
	protected virtual void OnEnable()
	{
		EditorSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/UI/EditorSkin.guiskin");
	}

    //---------------------------------------------------------------------
    //- Color & layouts/skin
    //---------------------------------------------------------------------
	public GUISkin EditorSkin;
	public static readonly Color ColorDefault = new Color(1f, 1f, 1f);
    public static readonly Color ColorSave = new Color(0.2f, 0.9f, 0.2f);
    public static readonly Color ColorNew = new Color(0.2f, 0.9f, 0.2f);
    public static readonly Color ColorCurrent = new Color(0.4f, 0.4f, 1.0f);
    public static readonly Color ColorAlert = new Color(0.8f, 0.2f, 0.2f);

	public static readonly GUILayoutOption MediumField = GUILayout.Width(200f);

    //---------------------------------------------------------------------
    //- Tab System
    //---------------------------------------------------------------------
    private Dictionary<GUIContent[], int> m_Tabs;

    protected delegate void TabDelegate();

    /// <summary>
    /// Create Tabs
    /// </summary>
    /// <param name="tabTitles">tab names</param>
    /// <param name="tabCallbacks">tab gui functions</param>
    /// <returns></returns>
    protected GUIContent[] MakeTabs(string[] tabTitles, out TabDelegate[] tabCallbacks, params TabDelegate[] cbs)
    {
        Debug.Assert(cbs.Length == tabTitles.Length, string.Format("Mismatch Titles {0}/ Delegates {1}", tabTitles.Length, cbs.Length));
        GUIContent[] copy = new GUIContent[tabTitles.Length];
        tabCallbacks = new TabDelegate[tabTitles.Length];
        for (int i = 0; i < copy.Length; ++i)
        {
            copy[i] = new GUIContent(tabTitles[i]);
            tabCallbacks[i] = cbs[i];
        }

        return copy;
    }

    protected TabDelegate ShowTabs(GUIContent[] content, TabDelegate[] tabCallback, out int indexChanged)
    {
        if ( !m_Tabs.ContainsKey(content))
        {
            m_Tabs.Add(content, 0);
        }
        int current = m_Tabs[content];

        int selected = GUILayout.Toolbar(current, content);
        indexChanged = -1;
        if ( selected != current )
        {
            m_Tabs[content] = selected;
            indexChanged = selected;
        }

        return tabCallback[selected];
    }

    protected TabDelegate ShowTabs(GUIContent[] content, TabDelegate[] tabCallback)
    {
        return ShowTabs(content, tabCallback, out _);
    }

    //---------------------------------------------------------------------
    //- Folder System
    //---------------------------------------------------------------------
    private Dictionary<int, bool> m_Folders;

	protected bool Folder(string label, string strHash, bool defaultUnfold = true)
	{
		int hash = strHash.GetHashCode();
		if (!m_Folders.ContainsKey(hash))
		{
			m_Folders[hash] = defaultUnfold;
		}

		EditorGUILayout.BeginVertical("box");
		m_Folders[hash] = EditorGUILayout.Foldout(m_Folders[hash], label);

		return m_Folders[hash];
	}

	protected bool Folder(string foldName, bool defaultUnfold = true)
	{
		return Folder(foldName, foldName, defaultUnfold);
	}

	protected void EndFolder()
	{
		EditorGUILayout.EndVertical();
	}

	protected void ResetFolders()
	{
		m_Folders.Clear();
	}

    protected void ForceFolderUnfold(string strHash, bool unfold)
    {
        int hash = strHash.GetHashCode();
        m_Folders[hash] = unfold;
    }

	protected bool IsFolded(string folderName)
	{
		bool res = false;
		m_Folders.TryGetValue(folderName.GetHashCode(), out res);
		return res;
	}
}
