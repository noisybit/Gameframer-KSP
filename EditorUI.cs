using KSPPluginFramework;
using System.Linq;
using UnityEngine;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    [WindowInitials(Caption = "Gameframer", Visible = true, DragEnabled = true, ClampToScreen = true)]
    public class EditorUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 275;
        private static int CLOSED_HEIGHT = 150;
        private static int OPEN_HEIGHT = 300;

        private static int HEADER_HEIGHT = 100;
        private static int FOOTER_HEIGHT = 50;

        private Rect windowSize;
        private EditorController ec = null;
        private bool showVessels = true;
        private bool hidden = false;
        private bool doOverwrite = true;
        Vector2 scrollPosition = Vector2.zero;

        private GUIStyle smallXStyle;
        private GUIStyle errorStyle;
        private GUIStyle listLabelStyle;
        private GUIStyle footerLabelStyle;
        private GUIStyle smallTextStyle;
        private GUIStyle headerLabelStyle;
        private GUIStyle bigLabelStyle;
        private GUIStyle bigGreenLabelStyle;

        internal bool oldShowVessels = true;
        internal bool refreshVesselsRequested = false;
        internal int saveSetting = 0;
        internal bool initOkay = false;
        public bool needsUpgrade = false;

        private ApplicationLauncherButton GUIToggleButton;
        private KARSettings settings = new KARSettings("KARSettings.cfg");
        private IGameframerService serviceInteface;

        internal override void Awake()
        {
            InputLockManager.RemoveControlLock("GF_LOCK_" + this.WindowID.ToString());
            serviceInteface = new GameframerService();

            LogFormatted("Checking version");
            needsUpgrade = !serviceInteface.CheckVersion();
            LogFormatted("NeedsUpdate = {0}", needsUpgrade);
            //Visible = false;            
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            settings.Load();
            doInit();
        }

        public void doInit()
        {
            settings.Load();

            if (settings.username == null || settings.username.Length == 0)
            {
                return;
            }

            if (settings.editorX <= 0 || settings.editorY <= 0 || settings.editorX >= Screen.width || settings.editorY >= Screen.height)
            {
                settings.editorX = 265;
                settings.editorY = 175;
                settings.Save();
            }

            windowSize = new Rect(settings.editorX, settings.editorY, MAIN_WIDTH, settings.editorOpened ? OPEN_HEIGHT : CLOSED_HEIGHT);
            WindowRect = windowSize;

            ec = FindObjectOfType<EditorController>();
            Visible = true;
            hidden = false;
            initOkay = true;
        }


        /* This was shamelessly ripped from Engineer by CYBUTEK
         * https://github.com/CYBUTEK/Engineer/blob/master/Engineer/BuildEngineer.cs */
        private bool isEditorLocked = false;
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

        void DummyVoid() { }

        void OnToggleOn()
        {
            hidden = true;
            Visible = true;
            //Settings.fetch.GUIEnabled = true;
        }

        void OnToggleOff()
        {
            hidden = false;
            Visible = false;
            //Settings.fetch.GUIEnabled = false;
        }

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready)
            {
                GUIToggleButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToggleOn,
                    OnToggleOff,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                    (Texture)GameDatabase.Instance.GetTexture("Gameframer/Textures/gf_logo", false));
            }
        }

        private void SetWindowSize(int width, int height)
        {
            windowSize = new Rect(WindowRect.x, WindowRect.y, width, height);
            WindowRect = windowSize;
        }

        internal override void OnDestroy()
        {
            EditorLogic.fetch.Unlock("GF_LOCK_" + this.WindowID.ToString());
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (GUIToggleButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(GUIToggleButton);
        }

        internal override void Update()
        {
            if (!Visible && !hidden && settings.username != null && settings.username.Length > 0)
            {
                //                doInit();
            }

            //toggle whether its visible or not
            if (Input.GetKeyDown(KeyCode.F10))
            {
                hidden = !hidden;
                Visible = !Visible;
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ec.DoCaptureAndUpload(doOverwrite);
            }
        }

        internal override void OnGUIEvery()
        {
            if (!initOkay) return;
            if (!needsUpgrade) return;

            if (EditorLogic.fetch.saveBtn.controlState == UIButton.CONTROL_STATE.ACTIVE && saveSetting == 1)
            {
                ec.DoCaptureAndUpload();
                refreshVesselsRequested = true;
            }

            if (WindowRect.x != ec.settings.editorX ||
               WindowRect.y != ec.settings.editorY ||
                showVessels != ec.settings.editorOpened)
            {
                ec.settings.editorX = WindowRect.x;
                ec.settings.editorY = WindowRect.y;
                ec.settings.editorOpened = showVessels;
                ec.settings.Save();
            }

            ec.saveSetting = this.saveSetting;

            if (refreshVesselsRequested)
            {
                ec.RefreshVessels();
                refreshVesselsRequested = false;
            }
        }

        internal Texture2D whiteTexture;
        internal Texture2D clearTexture;
        internal Texture2D activeTexture;
        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);

            whiteTexture = new Texture2D(MAIN_WIDTH, 1);
            for (int i = 0; i < MAIN_WIDTH; i++)
            {
                whiteTexture.SetPixel(i, 0, Color.white);
            }
            whiteTexture.Apply();

            clearTexture = new Texture2D(1, 1);
            Color clearBlack = new Color(0f, 0f, 0f, 0.0f);
            clearTexture.SetPixel(0, 0, clearBlack);
            clearTexture.Apply();
            activeTexture = new Texture2D(1, 1);
            Color activeBlue = new Color(45f, 45f, 45f, 0.5f);
            activeTexture.SetPixel(0, 0, activeBlue);
            activeTexture.Apply();

            smallXStyle = new GUIStyle(GUI.skin.button);
            smallXStyle.normal.textColor = Color.red;
            smallXStyle.hover.textColor = Color.red;
            smallXStyle.fixedWidth = smallXStyle.fixedHeight = 24;
            smallXStyle.normal.background = smallXStyle.onNormal.background = clearTexture;
            smallXStyle.padding = new RectOffset(0, 0, 0, 0);

            listLabelStyle = new GUIStyle(GUI.skin.label);
            listLabelStyle.normal.textColor = listLabelStyle.onNormal.textColor = Color.white;
            listLabelStyle.fontSize = 14;
            listLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            errorStyle = new GUIStyle(GUI.skin.label);
            errorStyle.normal.textColor = errorStyle.onNormal.textColor = Color.white;
            errorStyle.fontSize = 16;
            errorStyle.padding = new RectOffset(20, 20, 20, 20);

            smallTextStyle = new GUIStyle(GUI.skin.label);
            smallTextStyle.normal.textColor = smallTextStyle.onNormal.textColor = Color.white;
            smallTextStyle.fontSize = 14;
            smallTextStyle.padding = new RectOffset(0, 0, 4, 4);

            footerLabelStyle = new GUIStyle(GUI.skin.label);
            footerLabelStyle.normal.textColor = footerLabelStyle.onNormal.textColor = Color.gray;
            footerLabelStyle.hover.textColor = footerLabelStyle.onHover.textColor = Color.gray;
            footerLabelStyle.fontSize = 12;
            footerLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            headerLabelStyle = new GUIStyle(GUI.skin.label);
            headerLabelStyle.normal.textColor = headerLabelStyle.onNormal.textColor = Color.yellow;
            headerLabelStyle.fontSize = 16;
            headerLabelStyle.padding = new RectOffset(0, 0, 0, 0);

            bigLabelStyle = new GUIStyle(GUI.skin.label);
            bigLabelStyle.normal.textColor = bigLabelStyle.active.textColor = Color.white;
            bigLabelStyle.fontSize = 18;
            bigLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            bigGreenLabelStyle = new GUIStyle(GUI.skin.label);
            bigGreenLabelStyle.normal.textColor = bigGreenLabelStyle.active.textColor = Color.green;
            bigGreenLabelStyle.fontSize = 18;
            bigGreenLabelStyle.padding = new RectOffset(0, 0, 4, 4);

        }

        internal override void DrawWindow(int id)
        {
            if (needsUpgrade)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(20);
                GUILayout.Label("Sorry about this, but you need to download a new version of the plugin to continue using Gameframer.", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Dismiss"))
                {
                    this.Visible = false;
                    Destroy(this);
                }
                if (GUILayout.Button("Open browser"))
                {
                    Application.OpenURL("http://www.gameframer.com/#/ksp");
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.EndVertical();
                return;
            }

            if (this.Visible && initOkay)
            {
                CheckEditorLock();

                /***** GUI START ****/
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.Space(10);
                if (GUILayout.Button("Hello " + ec.settings.username + "!", headerLabelStyle, GUILayout.ExpandWidth(true)))
                {
                    Application.OpenURL("http://www.gameframer.com/#/ksp/u/" + ec.settings.username);
                }


                if (!ec.healthCheck)
                {
                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(OPEN_HEIGHT - HEADER_HEIGHT - FOOTER_HEIGHT));
                    GUILayout.Label("Sorry, I'm having trouble communicating with the Gameframer servers.\n\nPlease try again later or email support@gameframer.com.", errorStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label(ec.vessels.Count() + " vessel" + (ec.vessels.Count<JsonObject>() != 1 ? "s" : "") + " at gameframer.com", smallTextStyle, GUILayout.ExpandWidth(true));
                    /*if (GUILayout.Button((showVessels ? "Hide" : "Show"), showHideStyle, GUILayout.Width(BUTTON_WIDTH_X)))
                    {
                        showVessels = !showVessels;
                    }*/
                    GUILayout.EndHorizontal();

                    if (showVessels)
                    {
                        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(OPEN_HEIGHT - HEADER_HEIGHT - FOOTER_HEIGHT));
                        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(MAIN_WIDTH - 20), GUILayout.ExpandHeight(true));
                        for (int i = 0; i < ec.vessels.Count<JsonObject>(); i++)
                        {
                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("x", smallXStyle))
                            {
                                serviceInteface.DeleteVessel(ec.settings.username, (string)ec.vessels.ElementAt<JsonObject>(i)["shipname"]);
                                ec.vessels = ec.GetVessels();
                                refreshVesselsRequested = true;
                                GUILayout.EndHorizontal(); // end the block before breaking out 
                                break;
                            }

                            GUILayout.Label((string)ec.vessels.ElementAt<JsonObject>(i)["name"], listLabelStyle, GUILayout.ExpandWidth(true));
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                        //                    GUILayout.Space(10);    // padding-bottom for vessel list
                    }
                    else
                    {
                        //                    GUILayout.Space(10);    // padding-bottom for non-visible vessel list
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Upload Current Vessel", GUILayout.ExpandHeight(true)))
                    {
                        if (saveSetting == 0 && EditorLogic.fetch.ship.parts.Count() > 0)
                        {
                            ec.DoCaptureAndUpload(doOverwrite);
                            refreshVesselsRequested = true;
                        }
                        else
                        {
                            LogFormatted("No parts detected ({0}) or wrong saveSetting ({1}).", EditorLogic.fetch.ship.parts.Count(), saveSetting);
                        }
                        refreshVesselsRequested = true;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                /*                GUILayout.Label("Upload on", selectStyle);
                                saveSetting = GUILayout.SelectionGrid(saveSetting, new[] { "Demand", "Vessel Save"}, 2);*/
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("F10 to show/hide window", footerLabelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("GF Alpha 1", footerLabelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    Application.OpenURL("http://www.gameframer.com/");
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
    }
}
