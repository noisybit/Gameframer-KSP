using KSPPluginFramework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Gameframer
{
    /* Welcome screen to choose a username. */
    [KSPAddonFixed(KSPAddon.Startup.SpaceCentre, false, typeof(WelcomeUI))]
    [WindowInitials(Visible = true, DragEnabled = true, ClampToScreen = true)]
    public class WelcomeUI : MonoBehaviourWindow
    {
        private KARSettings settings = new KARSettings("KARSettings.cfg");
        private IGameframerService serviceInteface;
        private Rect windowSize;

        public static int MAIN_WIDTH = 500;
        public static int MAIN_HEIGHT = 400;

        List<string> names = new List<string>();
        public GUISkin _mySkin = null;
        public int selGridInt = 0;
        public int screenNumber = 0;
        public bool serverError = false;
        public bool needsUpgrade = false;

        GUIStyle buttonStyle;
        GUIStyle labelStyle;
        GUIStyle greenCenteredLabelStyle;
        GUIStyle disclaimerLabelStyle;
        GUIStyle bigLabelStyle;
        GUIStyle bigGreenLabelStyle;
        GUIStyle errorStyle;

        internal override void Awake()
        {
            InputLockManager.RemoveControlLock("GF_LOCK_" + this.WindowID.ToString());
            serviceInteface = new GameframerService();
            //windowSize = new Rect(Screen.width / 2 - MAIN_WIDTH / 2, Screen.height / 2 - MAIN_HEIGHT / 2 - 50, MAIN_WIDTH, MAIN_HEIGHT);
            windowSize = new Rect(20, Screen.height - MAIN_HEIGHT - 90, MAIN_WIDTH, MAIN_HEIGHT);
            WindowRect = windowSize;
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
                LogFormatted("Checking version");
                needsUpgrade = !serviceInteface.CheckVersion();
            }
        }

        internal override void OnDestroy()
        {
            EditorLogic.fetch.Unlock("GF_LOCK_" + this.WindowID.ToString());
        }

        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
            GUI.skin = HighLogic.Skin;

            if (!needsUpgrade)
            {
                names = serviceInteface.GetNames();
            }

            buttonStyle = new GUIStyle(GUI.skin.button);
            labelStyle = new GUIStyle(GUI.skin.label);
            greenCenteredLabelStyle = new GUIStyle(GUI.skin.label);
            disclaimerLabelStyle = new GUIStyle(GUI.skin.label);
            bigLabelStyle = new GUIStyle(GUI.skin.label);
            bigGreenLabelStyle = new GUIStyle(GUI.skin.label);
            errorStyle = new GUIStyle(GUI.skin.label);

            buttonStyle.fontSize = 18;
            buttonStyle.fontStyle = FontStyle.Normal;
            buttonStyle.normal.textColor = Color.white;
            //            buttonStyle.onActive.textColor = buttonStyle.onFocused.textColor = Color.yellow;
            buttonStyle.padding = new RectOffset(8, 8, 8, 8);

            labelStyle.fontSize = 18;
            labelStyle.onNormal.textColor = Color.yellow;
            labelStyle.padding = new RectOffset(0, 0, 4, 4);

            greenCenteredLabelStyle.fontSize = 20;
            greenCenteredLabelStyle.alignment = TextAnchor.MiddleCenter;
            greenCenteredLabelStyle.onNormal.textColor = Color.green;
            greenCenteredLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            disclaimerLabelStyle.fontSize = 18;
            disclaimerLabelStyle.alignment = TextAnchor.MiddleLeft;
            disclaimerLabelStyle.normal.textColor = Color.yellow;
            disclaimerLabelStyle.padding = new RectOffset(20, 20, 4, 4);

            bigLabelStyle.normal.textColor = bigLabelStyle.active.textColor = Color.white;
            bigLabelStyle.fontSize = 20;
            bigLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            bigGreenLabelStyle.normal.textColor = bigGreenLabelStyle.active.textColor = Color.green;
            bigGreenLabelStyle.fontSize = 20;
            bigGreenLabelStyle.padding = new RectOffset(0, 0, 4, 4);

            errorStyle.normal.textColor = errorStyle.focused.textColor = Color.red;
            errorStyle.hover.textColor = errorStyle.active.textColor = Color.red;
            errorStyle.fontSize = 18;
            errorStyle.onNormal.textColor = errorStyle.onFocused.textColor = errorStyle.onHover.textColor = errorStyle.onActive.textColor = Color.red;
            errorStyle.padding = new RectOffset(10, 10, 10, 10);
        }

        private void SetWindowSize(int width, int height)
        {
            windowSize = new Rect(windowSize.x, windowSize.y, MAIN_WIDTH, MAIN_HEIGHT);
            WindowRect = windowSize;
        }

        internal override void OnGUIEvery()
        {
            if (this.WindowRect.Contains(Input.mousePosition))
            {
                InputLockManager.GetControlLock("GF_LOCK_" + this.WindowID.ToString());
            }

            if (screenNumber == 2 && Visible == false)
            {
                if (settings.Save())
                {
                    LogFormatted("Saved: Path={0} Name={1} Exists={2}", settings.FilePath, settings.FileName, settings.FileExists);
                    Destroy(this);
                }
                else
                {
                    LogFormatted("Error saving: Path={0} Name={1} Exists={2}", settings.FilePath, settings.FileName, settings.FileExists);
                }
            }
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
                GUILayout.Label("Gameframer", bigGreenLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                GUILayout.Label("Sorry about this, but you need to download a new version of the plugin to continue using Gameframer.", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(50);
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
                GUILayout.Label("Welcome to Gameframer for KSP. Thanks for participating in the public alpha.", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("This is a one-time setup screen. You won't see it again unless you reinstall KSP or delete this mod.", bigLabelStyle, GUILayout.ExpandWidth(true));

                GUILayout.Label("This is an early alpha product. There will be bugs and stuff might be slow.", disclaimerLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("If you choose to use this mod, some of your gameplay data will be uploaded to gameframer.com. No personally identifiable information is associated with the data.", disclaimerLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("Sound good?", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Nah, not feeling it", buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    Visible = false;
                    Destroy(this);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Let's do this", buttonStyle, GUILayout.ExpandWidth(false)))
                {
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
                GUILayout.Label("Great! Just one more thing to do...", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("Choose a username", labelStyle, GUILayout.ExpandWidth(true));
                if (names.Count() > 0)
                {
                    GUILayout.Space(6);
                    selGridInt = GUILayout.SelectionGrid(selGridInt, names.ToArray<string>(), 2, buttonStyle);
                    GUILayout.Space(16);
                    GUILayout.Label("You will access all of your KSP data at", bigLabelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Label("gameframer.com/#/ksp/u/" + names[selGridInt], greenCenteredLabelStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(32);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Get more names", buttonStyle))
                    {
                        names = serviceInteface.GetNames();
                        selGridInt = 0;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ok, looks good", buttonStyle))
                    {
                        HttpStatusCode registerResult = serviceInteface.RegisterName(names[selGridInt]);
                        settings.username = names[selGridInt];
                        settings.Save();
                        if (registerResult == HttpStatusCode.OK)
                        {
                            screenNumber++;
                            LogFormatted("Registered name successfully. [{0}]", registerResult);
                        }
                        else
                        {
                            LogFormatted("Bad return code trying to register name. {0}", registerResult.ToString());
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Sorry, I'm having trouble getting names from the Gameframer servers.", errorStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Try Later", buttonStyle))
                    {
                        this.Visible = false;
                        Destroy(this);
                    }
                    if (GUILayout.Button("Retry Now", buttonStyle))
                    {
                        names = serviceInteface.GetNames();
                        selGridInt = 0;
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
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(6);

                GUILayout.Label("That's it. You'll find all your stuff at", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("<b>gameframer.com/#/ksp/u/" + settings.username + "</b>", greenCenteredLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("If you have any problems, questions, or suggestions head to <b>gameframer.com/#/ksp/help</b>", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Now go build some rockets!", bigLabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Choose a different username", buttonStyle))
                {
                    screenNumber--;
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Bye!", buttonStyle))
                {
                    screenNumber++;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                GUILayout.EndVertical();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            else if (screenNumber == 3)
            {
                Visible = false;
                Destroy(this);
            }
        }
    }
}