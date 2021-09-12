using System.Collections;
using UnityEngine;

namespace Dirt.GameEditor.Imgui
{
    public static partial class DirtGUI
    {
        public static int QuickVariable(int currentOption, IList choices, params GUILayoutOption[] options)
        {
            object choice = choices[currentOption];

            GUILayout.BeginHorizontal();
            bool decrease = GUILayout.Button("-", GUILayout.Width(20f));
            GUILayout.TextField(choice.ToString(), options);
            bool increase = GUILayout.Button("+", GUILayout.Width(20f));
            GUILayout.EndHorizontal();
            if (decrease)
                currentOption = Mathf.Max(0, currentOption - 1);
            if (increase)
                currentOption = Mathf.Min(choices.Count - 1, currentOption + 1);
            return currentOption;
        }
    }
}