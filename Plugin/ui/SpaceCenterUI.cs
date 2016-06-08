using KSPPluginFramework;
using UnityEngine;
using KSP.UI.Screens;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    [WindowInitials(Caption = "Gameframer", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class SpaceCenterUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 240;
        private static int MAIN_HEIGHT = 110;

        private bool _addedLauncherButton = false;
        private ApplicationLauncherButton launcherButton;

        internal override void Awake()
        {
            LogFormatted("SpaceCenterUI: Awake()");
            SettingsManager.Instance.Reload();
            LogFormatted("SpaceCenterUI: 2");
            if (SettingsManager.Instance.settings.username.Length == 0)
            {
                LogFormatted("No username, destroyed SpaceCenterUI");
                Visible = false;
                Destroy(this);
                return;
            }

            LogFormatted("SpaceCenterUI: 3");
            DoInit();
            LogFormatted("SpaceCenterUI: 4");
        }

        private void AddButton()
        {
            //if (ApplicationLauncher.Ready && _addedLauncherButton)
            if (!_addedLauncherButton)
            {
                launcherButton = ApplicationLauncher.Instance.AddModApplication(ToggleVisible, ToggleVisible,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture)GameDatabase.Instance.GetTexture("Gameframer/Textures/gf_logo", false));
                _addedLauncherButton = true;
            }
        }

        private void ToggleVisible()
        {
            this.Visible = !this.Visible;

            if (!this.Visible)
            {
                GUIManager.Instance.CloseMissionsWindow();
                GUIManager.Instance.CloseVesselsWindow();
            }

            SettingsManager.Instance.settings.spaceCenterVisible = this.Visible;
            SettingsManager.Instance.Save();
        }

        private void RemoveButton()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
            }
        }

        public void DoInit()
        {
            LogFormatted("SpaceCenterUI: DoInit1");
            if (SettingsManager.Instance.settings.username == null ||
                SettingsManager.Instance.settings.username.Length == 0)
            {
                return;
            }

            LogFormatted("SpaceCenterUI: DoInit2; {0}, {1}, {2}", SettingsManager.Instance.settings.spaceCenterX, SettingsManager.Instance.settings.spaceCenterY, SettingsManager.Instance.settings.spaceCenterVisible);
            WindowRect = new Rect(SettingsManager.Instance.settings.spaceCenterX, SettingsManager.Instance.settings.spaceCenterY, MAIN_WIDTH, MAIN_HEIGHT);
            LogFormatted("SpaceCenterUI: DoInit3");
            Visible = SettingsManager.Instance.settings.spaceCenterVisible;
            LogFormatted("SpaceCenterUI: DoInit4");
            AddButton();
            LogFormatted("SpaceCenterUI: DoInit5");
        }

        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }

        internal override void OnDestroy()
        {
            SettingsManager.Instance.settings.Save();
            RemoveButton();
        }

        void ToggleVisibility()
        {
            Visible = !Visible;
        }

        internal override void OnGUIEvery()
        {
            if (!VersionChecker.Instance.IsVersionOkay()) return;

            if (WindowRect.x != SettingsManager.Instance.settings.spaceCenterX ||
                WindowRect.y != SettingsManager.Instance.settings.spaceCenterY)
            {
                SettingsManager.Instance.settings.spaceCenterX = WindowRect.x;
                SettingsManager.Instance.settings.spaceCenterY = WindowRect.y;
            }
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            if (GUI.Button(new Rect(WindowRect.width - 24, 4, 21, 21), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                ToggleVisible();
            }

            /***** GUI START ****/
            if (!VersionChecker.Instance.IsVersionOkay())
            {
                CommonUI.DrawUpdateGUI(this);
            }
            else
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUILayout.BeginHorizontal(GUILayout.Height(32), GUILayout.ExpandWidth(true));
                GUILayout.Label(GameDatabase.Instance.GetTexture("Gameframer/Textures/star_ffff00_32", false),
                    GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Label("Hi <b>" + SettingsManager.Instance.settings.username + "</b>!", "MissionContent", GUILayout.Height(32));
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                CommonUI.DrawNavButtons();
                GUILayout.EndVertical();
            }
        }
    }
}
