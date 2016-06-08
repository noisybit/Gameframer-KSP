using System;
using KSPPluginFramework;
using UnityEngine;

namespace Gameframer
{
    //[KSPAddonFixed(KSPAddon.Startup.EditorAny, false, typeof(EditorDebugUI))]
    [WindowInitials(Caption = "Gameframer Settings", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class SettingsUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 300;
        private static int OPEN_HEIGHT = 255;

        private bool closeWindow = false;

        private bool busyLinking = false;

        internal override void Awake()
        {
            WindowRect = new Rect(300, 100, MAIN_WIDTH, OPEN_HEIGHT);
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

        internal override void OnGUIEvery()
        {
            if (closeWindow)
            {
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            this.Visible = false;

            Destroy(this);
        }

        private void ApplyChanges()
        {
            CloseWindow();
        }

        private void DoLink()
        {
            var URL = GameframerService.HOSTNAME + "/token";
            WWWClient client = new WWWClient(this, URL);
            busyLinking = true;
            client.AddHeader("Authorization", KSPUtils.GetAuthHeader());
            client.AddHeader("X-HTTP-Method-Override", "GET");
            client.OnDone = (WWW www) =>
            {
                // TODO check http code
                busyLinking = false;
                SettingsManager.Instance.settings.token = www.text;
                SettingsManager.Instance.Save();
                Application.OpenURL(new Uri(GameframerService.GetWebBase(false) + "link?token=" + www.text).ToString());
            };

            client.OnFail = (WWW www) =>
            {
                busyLinking = false;
                ScreenMessages.PostScreenMessage("Error linking account", 3.0f, ScreenMessageStyle.UPPER_LEFT);
                GFLogger.Instance.AddError("{0}.{1}: WWW_ERROR: {2}", this.GetType().Name, "client.OnDone", www.error);
            };

            client.Request();
        }

        private void DrawFooterButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(75)))
            {
                CloseWindow();
            }
            if (GUILayout.Button("Ok", GUILayout.Width(75)))
            {
                ApplyChanges();
            }
            GUILayout.EndHorizontal();
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            if (GUI.Button(new Rect(WindowRect.width - 24, 5, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                CloseWindow();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginVertical("NormalBox");
            GUILayout.Space(8);
            GUILayout.Label("Vessels", "HeaderStyle");
            SettingsManager.Instance.settings.editorAutoSave = GUILayout.Toggle(SettingsManager.Instance.settings.editorAutoSave, "Automatically upload when saved", "ToggleStyle");
            GUILayout.Space(20);
            GUILayout.Label("Missions", "HeaderStyle");
            SettingsManager.Instance.settings.boostAmbientLight = GUILayout.Toggle(SettingsManager.Instance.settings.boostAmbientLight, "Boost ambient light when recording", "ToggleStyle");
            /*if (GUILayout.Button(new GUIContent("Automatic recording details")))
            {
                GUIManager.Instance.ToggleSettingsDetailsWindow();
            }*/
            GUILayout.Space(20);
            GUILayout.Label("Other", "HeaderStyle");
            SettingsManager.Instance.settings.offlineMode = GUILayout.Toggle(SettingsManager.Instance.settings.offlineMode, "Debug (local data dump)", "ToggleStyle");

            GUILayout.Space(20);
            GUILayout.Label("Web Links", "HeaderStyle");           
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Link account", "Link this username with your gameframer.com login.")))
            {
                DoLink();
            }
            if (GUILayout.Button(new GUIContent("Forum thread", "Discuss the mod and Gameframer"), GUILayout.ExpandWidth(true)))
            { 
                Application.OpenURL(GameframerService.GetForumURL());
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(VersionChecker.VERSION + "-beta", "Subdued2Text");
            GUILayout.Space(8);
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                LogFormatted("GUI changed, saving settings");
                SettingsManager.Instance.Save();
            }
        }
    }
}
