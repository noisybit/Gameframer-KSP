using KSPPluginFramework;
using UnityEngine;

namespace Gameframer
{
    //[KSPAddonFixed(KSPAddon.Startup.EditorAny, false, typeof(EditorIntroUI))]
    [WindowInitials(Caption = "Gameframer Introduction", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class IntroUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 300;
        private static int OPEN_HEIGHT = 400;
        private string INTRO_TEXT = "As you conduct your mission various events like stage activation or achieving orbit will trigger recording of short video clips.";
        private string INTRO_TEXT2 = "If you would like to record your own events, use the record button. Press it again to stop recording. This will create a timelapse video that will be compressed to around seven seconds long.";
        private string INTRO_TEXT3 = "Every mission is given a name automatically. If you would like to rename it, add a description, or delete events click the edit button.";
        private string INTRO_TEXT4 = "Head over to gameframer.com to view your mission page and video clips. That's it, have fun!";
        private bool closeWindow = false;

        internal override void Awake()
        {
            WindowRect = new Rect(SettingsManager.Instance.settings.missionX + 250, SettingsManager.Instance.settings.missionY, MAIN_WIDTH, OPEN_HEIGHT);
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
            SettingsManager.Instance.settings.seenMissionIntroHelp = true;
            SettingsManager.Instance.settings.Save();
            this.Visible = false;
            Destroy(this);
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            GUILayout.BeginVertical();
            GUILayout.Space(14);
            GUILayout.Label("Mission Recording", "HeaderStyle");
            GUILayout.Space(7);
            GUILayout.Label(INTRO_TEXT, "ContentStyle");
            GUILayout.Space(14);
            GUILayout.Label(INTRO_TEXT2, "ContentStyle");
            GUILayout.Space(14);
            GUILayout.Label(INTRO_TEXT3, "ContentStyle");
            GUILayout.Space(14);
            GUILayout.Label(INTRO_TEXT4, "ContentStyle");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Got it!"))
            {
                CloseWindow();
            }
            GUILayout.EndVertical();
        }
    }
}
