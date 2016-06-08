using KSPPluginFramework;
using UnityEngine;

namespace Gameframer
{
    public class CommonUI : MonoBehaviourExtended
    {
        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }

        public static void DrawUpdateGUI(MonoBehaviourWindow w)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUILayout.Label("Sorry about this, but you need to download a new version of the plugin to continue using Gameframer.", "UpgradeText", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open browser"))
            {
                Application.OpenURL(GameframerService.GetWebBase());
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndVertical();
        }


        public static void DrawNavButtons(bool includeSettings = true)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            if (GUILayout.Button(
                new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/rocket_ffffff_17", false), "Vessels"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                GUIManager.Instance.ToggleVesselsWindow();
            }
            if (GUILayout.Button(
                new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/flag_ffffff_17", false), "Missions"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                GUIManager.Instance.ToggleMissionsWindow();
            }
            GUILayout.FlexibleSpace();
            if (includeSettings)
            {
                if (GUILayout.Button(
                    new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/gear_ffffff_17", false), "Help & Settings"), GUILayout.Width(30), GUILayout.Height(30)))
                {
                    GUIManager.Instance.ToggleSettingsWindow();
                }
            }
            if (GFLogger.PRINT_DEBUG_INFO)
            {
                if (GUILayout.Button("D", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    GUIManager.Instance.ToggleDebugWindow();
                }
            }
//            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}