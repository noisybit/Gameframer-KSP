using KSPPluginFramework;
using UnityEngine;
using KSP.UI.Screens;

namespace Gameframer
{
    /* KAMR : Kerbal Automated Mission Recorder */
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    [WindowInitials(Caption = "Gameframer", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class MissionUI : MonoBehaviourWindow
    {
        public enum GUIScreen { Zero_Lookup, One_StartPrompt, Two_Initializing, Three_Active, ServerError };

        public static int MAIN_WIDTH = 230;
        public static int MAIN_HEIGHT = 100;

        public bool vesselDestroyed = false;
        private bool _addedLauncherButton = false;
        private ApplicationLauncherButton launcherButton;
        private Texture2D grayTexture;
        private GUIStyle splitter;
        internal Vector2 scrollPosition = Vector2.zero;

        private MissionUIController uiController;
        private MissionDetailsUI detailsWindow;

        /* updated by MissionUIController */
        public GUIScreen guiState = GUIScreen.Zero_Lookup;
        public string statusMessage = "Gameframer preparing to record...";

        #region Overrides
        internal override void Awake()
        {
            WindowRect = new Rect(SettingsManager.Instance.settings.missionX, SettingsManager.Instance.settings.missionY, MAIN_WIDTH, MAIN_HEIGHT);
            Visible = SettingsManager.Instance.settings.missionVisible;

            AddButton();

            uiController = FindObjectOfType<MissionUIController>();

            grayTexture = new Texture2D(1, 1);
            grayTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
            grayTexture.Apply();

            splitter = new GUIStyle();
            splitter.normal.background = grayTexture;
            splitter.stretchWidth = true;
            splitter.fixedHeight = 1;
            splitter.margin = new RectOffset(0, 0, 1, 1);

            if (!SettingsManager.Instance.settings.seenMissionIntroHelp)
            {
                gameObject.AddComponent<IntroUI>();
            }
        }
        internal override void OnDestroy()
        {
            SettingsManager.Instance.settings.Save();

            RemoveButton();
            StopRepeatingWorker();
        }
        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }
        internal override void OnGUIEvery()
        {
            if (WindowRect.x != SettingsManager.Instance.settings.missionX ||
                WindowRect.y != SettingsManager.Instance.settings.missionY)
            {
                SettingsManager.Instance.settings.missionX = WindowRect.x;
                SettingsManager.Instance.settings.missionY = WindowRect.y;
            }
        }
        internal override void DrawWindow(int id)
        {
            if (GUI.Button(new Rect(WindowRect.width - 25, 7, 19, 19), "X"))
            {
                ToggleVisible();
            }

            // needs upgrade
            if (!VersionChecker.Instance.IsVersionOkay())
            {
                CommonUI.DrawUpdateGUI(this);
            }
            else
            // normal
            {
                GUILayout.BeginVertical();

                switch (guiState)
                {
                    case GUIScreen.Zero_Lookup: DrawInitializingGUI(); break;
                    case GUIScreen.One_StartPrompt: DrawFirstPromptGUI(); break;
                    case GUIScreen.Two_Initializing: DrawInitializingGUI(); break;
                    case GUIScreen.Three_Active: DrawRecordGUI(); break;
                    case GUIScreen.ServerError: DrawErrorGUI(); break;
                    default: DrawInitializingGUI();  break;
                }

                if (guiState == GUIScreen.Three_Active)
                {
                    CommonUI.DrawNavButtons(true);
                }
                GUILayout.EndVertical();
            }
        }
        #endregion


        #region Util
        private void AddButton()
        {
            //if (ApplicationLauncher.Ready && !_addedLauncherButton)
            if(!_addedLauncherButton)
            {
                launcherButton = ApplicationLauncher.Instance.AddModApplication(ToggleVisible, ToggleVisible,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("Gameframer/Textures/gf_logo", false));
                _addedLauncherButton = true;
            }
        }
        private void RemoveButton()
        {
            ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
        }
        private void ToggleEditMissionWindow()
        {
            if (detailsWindow == null || !detailsWindow.Visible)
            {
                detailsWindow = gameObject.AddComponent<MissionDetailsUI>();
            }
            else
            {
                detailsWindow.Visible = false;
                Destroy(detailsWindow);
            }
        }
        private void ToggleVisible()
        {
            this.Visible = !this.Visible;

            SettingsManager.Instance.settings.missionVisible = this.Visible;
            SettingsManager.Instance.Save();
        }
        #endregion

        #region Draw GUI Logic
        private void DrawInitializingGUI()
        {
            GUILayout.BeginVertical("NormalBox", GUILayout.ExpandWidth(true));
            GUILayout.Space(40);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(40);
            GUILayout.Label("Initializing...", "InitQuestion", GUILayout.ExpandWidth(true));
            GUILayout.Space(40);
            GUILayout.EndHorizontal();
            GUILayout.Space(40);
            GUILayout.EndVertical();
        }
        private void DrawErrorGUI()
        {
            GUILayout.Label("Error communicating with server.", "BillboardContent");
            if (GUILayout.Button("Try again"))
            {
                uiController.StartRecording();
            }
        }
        private void DrawFirstPromptGUI()
        {
            if (vesselDestroyed)
            {
                GUILayout.Label("Vessel destroyed");
            }
            else
            {
                GUILayout.BeginVertical("NormalBox", GUILayout.ExpandWidth(true));
                GUILayout.Space(20);
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label(GameDatabase.Instance.GetTexture("Gameframer/Textures/question_ffff00_32", false), "InitIconStyle");
                GUILayout.Label("Record this mission?", "InitQuestion", GUILayout.Height(38));
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ff0000_17", false), "Maybe next time."), GUILayout.Width(30), GUILayout.Height(30)))
                {
                    ToggleVisible();
                }
                if (GUILayout.Button(GameDatabase.Instance.GetTexture("Gameframer/Textures/check_0fe00f_17", false), GUILayout.Width(30), GUILayout.Height(30)))
                {
                    guiState = GUIScreen.Two_Initializing;
                    uiController.StartRecording();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        private void DrawUserRecordGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            if (uiController.userCapture == OmniController.UserCaptureState.Idle)
            {
                if (MapView.MapIsEnabled)
                {
                    GUILayout.Label("Can't record video while in map view.", "SubduedContentStyle", GUILayout.Height(30), GUILayout.ExpandWidth(true));
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.Height(30));
                    GUI.enabled = !vesselDestroyed;

                    if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/image", false), "Capture still image without GUI"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        uiController.DoUserCapture("User Still", VideoOptions.STILL);
                    }

                    if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/video", false), "Record timelapse"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        uiController.DoUserCapture("User Timelapse", VideoOptions.TIMELAPSE);
                    }
                    GUILayout.Space(5);
                    GUILayout.Label("00:00:000", "SubduedContentStyle", GUILayout.Height(30));
                    GUILayout.EndHorizontal();
                }
            }
            else if (uiController.userCapture == OmniController.UserCaptureState.Capturing)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(30));
                if (GUILayout.Button(
                    new GUIContent(
                        GameDatabase.Instance.GetTexture("Gameframer/Textures/stop", false),
                        "Stop recording"), GUILayout.Width(30), GUILayout.Height(30)))
                {
                    uiController.DoStopUserCapture();
                }
                GUILayout.Space(5);
                GUILayout.Label(uiController.GetUserRecordingTimeString(), "ContentStyle", GUILayout.Height(30));
                /*GUILayout.FlexibleSpace();
                if (uiController.GetUserRecordingSpeed() > 1)
                {
                    GUILayout.Label("Timelapse " + uiController.GetUserRecordingSpeed() + "x", "ButtonBottomLabel");
                }*/

                GUILayout.EndHorizontal();
            }
            else if (uiController.userCapture == OmniController.UserCaptureState.Prompting || uiController.userCapture == OmniController.UserCaptureState.Uploading)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (uiController.userCapture == OmniController.UserCaptureState.Uploading)
                {
                    GUI.enabled = false;
                    GUILayout.Label(GameDatabase.Instance.GetTexture("Gameframer/Textures/arrow-up_ffffff_17", false), GUILayout.Width(30), GUILayout.Height(30));
                    GUILayout.Label("Uploading...", "ContentStyle", GUILayout.Height(30));
                }
                else
                {
                    GUI.enabled = true;
                    GUILayout.Label("Save?", "ContentStyle", GUILayout.Height(30));
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ff0000_17", false), "Discard recording"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        uiController.CancelUserUpload();
                    }
                    if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/check_0fe00f_17", false), "Save and upload"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        uiController.DoUserUpload("User event");
                    }
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }
        private void DrawRecordGUI()
        {
            if (uiController == null)
                return;

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("Mission Name", "SubduedContentStyle", GUILayout.Height(15), GUILayout.ExpandWidth(true));
            GUILayout.Label(uiController.GetMissionName(), "ContentStyle", GUILayout.Height(15), GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/pencil", false), "Edit mission details"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                ToggleEditMissionWindow();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            //GUILayout.Label("", splitter);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("Last Event", "SubduedContentStyle", GUILayout.Height(15), GUILayout.ExpandWidth(true));
            GUILayout.Label(statusMessage, "ContentStyle", GUILayout.Height(15), GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/web", false), "Open mission page in web browser"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                Application.OpenURL(GameframerService.GetWebBase() + "ksp/missions/" + uiController.activeMission["_id"]);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("", splitter);
            GUILayout.Space(4);
            if (uiController.paused)
            {
                if (GUILayout.Button("Resume Recording"))
                {
                    uiController.TogglePauseRecording();
                }
            }
            else
            {
                DrawUserRecordGUI();
            }
            GUILayout.Space(4);
            GUILayout.Label("", splitter);
            GUILayout.Space(4);
            GUILayout.EndVertical();
            GUI.enabled = true;
        }
        #endregion
    }
}
