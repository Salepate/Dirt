using UnityEngine;

namespace Dirt.GameEditor
{
    public static class DirtGUI
    {
        public static readonly Color ColorDefault = new Color(1f, 1f, 1f);
        public static readonly Color ColorSave = new Color(0.5f, 1f, 0.62f);
        public static readonly Color ColorNew = new Color(0.7f, 0.7f, 1f);
        public static readonly Color ColorCurrent = new Color(0.4f, 0.4f, 1.0f);
        public static readonly Color ColorAlert = new Color(0.8f, 0.2f, 0.2f);
        public static readonly GUILayoutOption MediumField = GUILayout.Width(200f);
    }
}