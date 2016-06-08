using KSPPluginFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameframer
{
    /* Welcome screen to choose a username. */
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    [WindowInitials(Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class WelcomeUI : MonoBehaviourWindow
    {
        private KARSettings settings = new KARSettings(KARSettings.LOCATION);
        public static int MAIN_WIDTH = 500;
        public static int MAIN_HEIGHT = 400;

        public List<string> names = new List<string>();
        public GUISkin _mySkin = null;
        public int selGridInt = 0;
        public int screenNumber = 0;
        public bool serverError = false;
        public bool needsUpgrade = false;
        private string selectedName;
        public bool registeredOkay = false;

        internal override void Awake()
        {
            LogFormatted("Checking version");
            needsUpgrade = !VersionChecker.Instance.IsVersionOkay();

            WindowRect = new Rect(20, Screen.height - MAIN_HEIGHT - 90, MAIN_WIDTH, MAIN_HEIGHT);
            settings.Load();

            if (settings.username.Length > 0)
            {
                LogFormatted("Already have a username: {0}", settings.username);
                this.Visible = false;
                Destroy(this);
            }
            else
            {
                LogFormatted("No username found! [{0}]", settings.username);
            }
        }

        internal override void OnDestroy()
        {
        }

        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
            GUI.skin = HighLogic.Skin;

            if (needsUpgrade)
            {
                CommonUI.DrawUpdateGUI(this);
            }
            else
            {
                PotentialNameWorker.CreateComponent(gameObject);
            }
        }

        private void GetNewNames()
        {
                GFLogger.Instance.AddDebugLog("Getting new names");
                PotentialNameWorker.CreateComponent(gameObject);
                UnityEngine.Debug.Log("??????????????????????????????");
                selGridInt = 0;
        }

        private void RegisterName()
        {
            screenNumber++;
            registeredOkay = false;
            selectedName = names[selGridInt];
            NameRegisterWorker.CreateComponent(gameObject, selectedName);
        }

        public void RegisteredOkay()
        {
                if (gameObject.GetComponent<SpaceCenterUI>() == null)
                {
                    SettingsManager.Instance.Reload();
                    LogFormatted("Creating SpaceCenterUI");
                    var ssUI = gameObject.AddComponent<SpaceCenterUI>();
                    ssUI.Visible = true;
                    ssUI.DragEnabled = true;
                    ssUI.TooltipsEnabled = true;
                    ssUI.ClampToScreen = true;
                }
                Visible = false;
                Destroy(this);
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
            {
                return;
            }

            if (needsUpgrade)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(75);
                GUILayout.BeginHorizontal();
                GUILayout.Space(75);
                GUILayout.BeginVertical();
                GUILayout.Label("Gameframer", "BigGreenLabel", GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                GUILayout.Label("Sorry about this, but you need to download a new version of the plugin to continue using Gameframer.", "WelcomeText", GUILayout.ExpandWidth(true));
                GUILayout.Space(50);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Dismiss"))
                {
                    this.Visible = false;
                    Destroy(this);
                }
                if (GUILayout.Button("Open browser"))
                {
                    Application.OpenURL(GameframerService.GetWebBase());
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(75);
                GUILayout.EndHorizontal();
                GUILayout.Space(75);
                GUILayout.EndVertical();
                return;
            }

            if (screenNumber == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(6);
                GUILayout.Label("Welcome to Gameframer for KSP. Thanks for participating in the public beta.", "WelcomeText", GUILayout.ExpandWidth(true));
                GUILayout.Space(16);
                GUILayout.Label("This is a one-time setup screen. You won't see it again unless you reinstall KSP or delete this mod.", "WelcomeText", GUILayout.ExpandWidth(true));
                GUILayout.Space(16);
                GUILayout.Label("This mod and the Gameframer service are early beta products and do not reflect their final quality.", "DisclaimerStyle", GUILayout.ExpandWidth(true));
                GUILayout.Label("If you choose to use this mod, some of your gameplay data will be uploaded to gameframer.com. No personally identifiable information is associated with the data.", "DisclaimerStyle", GUILayout.ExpandWidth(true));
                GUILayout.Space(16);
                GUILayout.Label("Sound good?", "WelcomeText", GUILayout.ExpandWidth(true));
                GUILayout.Space(16);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Nah, not feeling it", "ButtonStyle", GUILayout.ExpandWidth(false)))
                {
                    Visible = false;
                    Destroy(this);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Let's do this", "ButtonStyle", GUILayout.ExpandWidth(false)))
                {
                    GetNewNames();
                    screenNumber++;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.EndVertical();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            else if (screenNumber == 1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(6);
                GUILayout.Label("Great! Just one more thing to do...", "WelcomeText", GUILayout.ExpandWidth(true));
                GUILayout.Space(16);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Choose a username", "WelcomeText", GUILayout.ExpandWidth(true));
                if (GUILayout.Button(new GUIContent((Texture)GameDatabase.Instance.GetTexture("Gameframer/Textures/refresh_ffffff_32", false), "More random names"), GUILayout.Width(32), GUILayout.Height(32)))
                {
                    GetNewNames();
                }
                GUILayout.EndHorizontal();
                if (names.Count() > 0)
                {
                    GUILayout.Space(6);
                    selGridInt = GUILayout.SelectionGrid(selGridInt, names.ToArray<string>(), 2, "ButtonStyle");
                    GUILayout.Space(16);
                    GUILayout.Space(32);

                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ok, looks good", "ButtonStyle"))
                    {
                        RegisterName();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Sorry, I'm having trouble getting names from the Gameframer servers.", "ErrorText", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Try Later", "ButtonStyle"))
                    {
                        this.Visible = false;
                        Destroy(this);
                    }
                    if (GUILayout.Button("Retry Now", "ButtonStyle"))
                    {
                        GetNewNames();
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(20);
                GUILayout.EndVertical();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            else if (screenNumber == 2)
            {
                if (!registeredOkay)
                {
                    GUILayout.Label("Talking to the Gameframer servers... One moment please.");
                    GUI.enabled = false;
                }
                else
                {
                    // left margin
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                    GUILayout.Label(GameDatabase.Instance.GetTexture("Gameframer/Textures/fa-thumbs-up_34_0_ffff00_none", false),
                        GUILayout.Width(34), GUILayout.Height(34));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("That's it. We're ready to go. The plugin will appear in the VAB/SPH and while flying missions. Now go build some rockets and explore!", "WelcomeText", GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20);
                    //GUILayout.FlexibleSpace();  
                    GUILayout.BeginHorizontal("NormalBox");
                    string userPageURL = GameframerService.GetWebBase(false) + "/profile/" + selectedName;
                    GUILayout.Label("Once you upload some stuff you'll find it here.", "WelcomeText");
                    if (GUILayout.Button(new GUIContent("User Page", "Will open in an external web browser"), GUILayout.Width(100)))
                    {
                        Application.OpenURL(userPageURL);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal("NormalBox");
                    GUILayout.Label("If you have any problems, questions, or suggestions.", "WelcomeText");
                    if (GUILayout.Button(new GUIContent("Help", "Will open in an external web browser"), GUILayout.Width(100)))
                    {
                        Application.OpenURL(GameframerService.GetWebBase() + "/help");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    // footer
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(
                            GameDatabase.Instance.GetTexture("Gameframer/Textures/check_0fe00f_17", false), GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            screenNumber++;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(20);
                    GUILayout.EndVertical();
                    // right margin
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
                GUI.enabled = true;
            }
            else if (screenNumber == 3)
            {
                Visible = false;
                Destroy(this);
            }
        }
    }
}