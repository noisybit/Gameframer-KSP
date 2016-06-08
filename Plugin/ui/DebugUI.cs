using System.Collections.Generic;
using KSPPluginFramework;
using UnityEngine;

namespace Gameframer
{
    //[KSPAddonFixed(KSPAddon.Startup.EditorAny, false, typeof(EditorDebugUI))]
    [WindowInitials(Caption = "¯\\_(ツ)_/¯", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class DebugUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 500;
        private static int OPEN_HEIGHT = 400;

        List<string> hostOptions = new List<string>() {
	        "Prod", "Test", "Dev"
	    };
        internal Vector2 scrollPosition = Vector2.zero;
        private EditorController editorController;

        private bool closeWindow = false;

        internal override void Awake()
        {
            editorController = FindObjectOfType<EditorController>();

            WindowRect = new Rect(SettingsManager.Instance.settings.editorX + 400, SettingsManager.Instance.settings.editorY, MAIN_WIDTH, OPEN_HEIGHT);
            Visible = true;
        }

        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }

        internal override void OnDestroy()
        {
        }

        void ToggleVisibility()
        {
            Visible = !Visible;
        }

        internal override void Update()
        {
        }

        internal override void OnGUIEvery()
        {
            if (closeWindow)
            {
                Destroy(this);
            }
        }
        private void CloseWindow()
        {
            LogFormatted("CloseWindow");
            this.Visible = false;
            Destroy(this);
        }

        private void DoGetVessels()
        {
            editorController.TryGetVessels();
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            GUILayout.BeginVertical();
            {
                GUILayout.Space(16);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(16);
                    GUILayout.TextField(SettingsManager.Instance.settings.username + ":" + SettingsManager.Instance.settings.apiKey, "SubduedText", GUILayout.ExpandWidth(true));
                    if (HighLogic.LoadedSceneIsEditor)
                    {
                        GUI.enabled = false;
                    }
                        if (GUILayout.Button("GET VESSELS"))
                        {
                            DoGetVessels();
                        }

                        GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
                //if(GameframerService.HOSTNAME != GameframerService.PROD)
                {
                    int selIndex = GUILayout.SelectionGrid(GameframerService.GetHostname(), hostOptions.ToArray(), 3);
                    if (GUI.changed)
                    {
                        GameframerService.SetHostname(selIndex);
                    }
                }

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.TextArea(GFLogger.Instance.ERROR_TEXT, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
            }
            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("CLOSE"))
            {
                CloseWindow();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
