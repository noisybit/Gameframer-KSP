using KSPPluginFramework;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    [WindowInitials(Caption = "Gameframer", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class EditorUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 230;
        private static int MAIN_HEIGHT = 255;

        private bool _addedLauncherButton = false;
        private ApplicationLauncherButton launcherButton;

        private bool needsUpgrade = false;
        private bool isEditorLocked = false;

        internal Vector2 scrollPosition = Vector2.zero;
        private EditorController ec;

        private Texture2D grayTexture;
        private GUIStyle splitter;

        #region overrides
        internal override void Awake()
        {
            InputLockManager.RemoveControlLock("GF_LOCK_" + this.WindowID.ToString());
            needsUpgrade = !VersionChecker.Instance.IsVersionOkay();

            DoInit();

            grayTexture = new Texture2D(1, 1);
            grayTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
            grayTexture.Apply();

            splitter = new GUIStyle();
            splitter.normal.background = grayTexture;
            splitter.stretchWidth = true;
            splitter.fixedHeight = 1;
            splitter.margin = new RectOffset(0, 0, 1, 1);

        }
        internal override void OnDestroy()
        {
            SettingsManager.Instance.settings.Save();
            RemoveButton();
            EditorLogic.fetch.Unlock("GF_LOCK_" + this.WindowID.ToString());
            isEditorLocked = false;
        }
        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }
        internal override void OnGUIEvery()
        {
            if (needsUpgrade) return;

            if (WindowRect.x != SettingsManager.Instance.settings.editorX ||
                WindowRect.y != SettingsManager.Instance.settings.editorY)
            {
                SettingsManager.Instance.settings.editorX = WindowRect.x;
                SettingsManager.Instance.settings.editorY = WindowRect.y;
            }
        }
        internal override void DrawWindow(int id)
        {
            CheckEditorLock();

            if (!this.Visible)
                return;

            if (GUI.Button(new Rect(WindowRect.width - 25, 7, 19, 19), "X"))
            {
                ToggleVisible();
            }

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Space(8);
            if (needsUpgrade)
            {
                CommonUI.DrawUpdateGUI(this);
            }
            else if (ec.errorState)
            {
                DrawUnhealthyServerGUI();
            }
            else
            {
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                var welcomeStr = "Hello <b>" + SettingsManager.Instance.settings.username + "</b>.";
                if (ec.vessels.Count == 0)
                {
                    welcomeStr += "You haven't uploaded any vessels yet. :(";
                }
                else
                {
                    welcomeStr += "You have uploaded <b>" + ec.vessels.Count + "</b> vessel" + ((ec.vessels.Count == 1) ? "." : "s.");
                }
                GUILayout.Label(welcomeStr, "NewBigText", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (ec.isUploading || ec.isBusy)
                {
                    GUI.enabled = false;
                }
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(MAIN_WIDTH), GUILayout.Height(125));

                if (ec.vessels.Count > 0)
                {
                    for (int i = 0; i < ec.vessels.Count; i++)
                    {
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("x", "SmallXButton"))
                        {
                            ec.DeleteVessel(ec.vessels[i]["_id"]);
                            GUILayout.EndHorizontal(); // end the block before breaking out 
                            break;
                        }

                        GUILayout.Label((string)ec.vessels[i]["name"], "ListText", GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("No vessels.", "BillboardContent", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                GUI.enabled = true;
                GUILayout.EndScrollView();
                GUILayout.Space(4);
                if (GFLogger.Instance.STATUS == 0)
                {
                    GUILayout.Label(GFLogger.Instance.STATUS_TEXT, "SubduedText", GUILayout.ExpandWidth(true));
                }
                else
                {
                    GUILayout.Label(GFLogger.Instance.STATUS_TEXT, "SubduedText", GUILayout.ExpandWidth(true));
                }
                GUILayout.Space(4);
                if (EditorLogic.fetch.ship.parts.Count() == 0 || ec.isBusy)
                {
                    GUI.enabled = false;
                } 
                else
                {
                    GUI.enabled = true;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Upload", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                {
                    ec.DoCaptureAndUpload();
                }
                /*if (GUILayout.Button("Add Screenshot", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                {
                    ec.AddScreenshot();
                }*/
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                if (ec.isUploading || ec.isBusy) { GUI.enabled = false; }
                SettingsManager.Instance.settings.editorAutoSave = GUILayout.Toggle(SettingsManager.Instance.settings.editorAutoSave , new GUIContent("Always upload when saved.", "Every time you save a vessel in the editor, it will be saved/updated on gameframer.com."), "ToggleStyle");
                GUI.enabled = true;
                GUILayout.Space(4);
                //GUILayout.Label("", splitter);
                GUILayout.Space(4);
                CommonUI.DrawNavButtons();
            }
            
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                LogFormatted("GUI changed, saving settings");
                SettingsManager.Instance.Save();
            }
        }
        #endregion

        #region Logic

        private void ToggleVisible()
        {
            EditorLogic.fetch.Unlock("GF_LOCK_" + this.WindowID.ToString());
            isEditorLocked = false;
            this.Visible = !this.Visible;
            SettingsManager.Instance.settings.editorVisible = this.Visible;
            SettingsManager.Instance.Save();
        }
        private void DrawUnhealthyServerGUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.Label("Sorry, I'm having trouble communicating with the Gameframer servers.\n\nPlease try again later or email support@gameframer.com.", "ErrorText", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label(ec.healthMessage, "ErrorText", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retry"))
            {
                ec.TryGetVessels();
            }
            if (GUILayout.Button(GameDatabase.Instance.GetTexture("Gameframer/Textures/bug_ffffff_17", false), GUILayout.Width(30), GUILayout.Height(30)))
            {
                GUIManager.Instance.ToggleDebugWindow();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }
        public void DoInit()
        {
            if (SettingsManager.Instance.settings.username == null || SettingsManager.Instance.settings.username.Length == 0)
            {
                return;
            }

            WindowRect = new Rect(SettingsManager.Instance.settings.editorX, SettingsManager.Instance.settings.editorY, MAIN_WIDTH, MAIN_HEIGHT);
            Visible = SettingsManager.Instance.settings.editorVisible;

            ec = FindObjectOfType<EditorController>();

            AddButton();
        }
        /* This was shamelessly ripped from Engineer by CYBUTEK
         * https://github.com/CYBUTEK/Engineer/blob/master/Engineer/BuildEngineer.cs */
        private void CheckEditorLock()
        {
            Vector2 mousePos = Input.mousePosition;
            Rect tempRect = WindowRect;
            mousePos.y = Screen.height - mousePos.y;
            if (tempRect.Contains(mousePos) && !isEditorLocked)
            {
                EditorLogic.fetch.Lock(true, true, true, "GF_LOCK_" + this.WindowID.ToString());
                isEditorLocked = true;
            }
            else if (!tempRect.Contains(mousePos) && isEditorLocked)
            {
                EditorLogic.fetch.Unlock("GF_LOCK_" + this.WindowID.ToString());
                isEditorLocked = false;
            }
        }
        #endregion

        #region Application Launcher
        private void AddButton()
        {
            //if (ApplicationLauncher.Ready && !_addedLauncherButton)
            if(!_addedLauncherButton)
            {
                launcherButton = ApplicationLauncher.Instance.AddModApplication(ToggleVisible, ToggleVisible,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                    (Texture)GameDatabase.Instance.GetTexture("Gameframer/Textures/gf_logo", false));
                _addedLauncherButton = true;
            }
        }
        private void RemoveButton()
        {
            ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
        }
        #endregion

    }
}
