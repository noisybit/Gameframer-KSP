using System;
using KSPPluginFramework;
using UnityEngine;
using System.Collections.Generic;

namespace Gameframer
{
    [WindowInitials(Caption = "Gameframer Settings", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class ExternalEventsUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 300;
        private static int OPEN_HEIGHT = 255;

        private bool closeWindow = false;
        private GFPublicInterface inter;
        private Dictionary<string, bool> extEvents;
        private Dictionary<string, bool> temp = new Dictionary<string, bool>();
        private bool init = false;
        internal Vector2 scrollPosition = Vector2.zero;

        internal override void Awake()
        {
            WindowRect = new Rect(500, 410, MAIN_WIDTH, OPEN_HEIGHT);
            Visible = true;
            RefreshStuff();
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

        private void RefreshStuff(bool force = false)
        {
            if (!init || force)
            {
                inter = FindObjectOfType<GFPublicInterface>();
                extEvents = inter.GetRegisteredEvents();
                temp = new Dictionary<string, bool>(extEvents);
                if (extEvents != null)
                    init = true;
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

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            if (GUI.Button(new Rect(WindowRect.width - 24, 5, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                CloseWindow();
            }

            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUILayout.Label("External Events", "HeaderStyle");
            GUILayout.Label("Other mods can request event capture by Gameframer. You can enable or disable them here.", "SubduedText");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(MAIN_WIDTH), GUILayout.Height(125));
            if (extEvents.Count > 0) 
            {
                foreach (KeyValuePair<string, bool> extEvent in extEvents)
                {
                    temp[extEvent.Key] = GUILayout.Toggle(extEvent.Value, extEvent.Key, "ToggleStyle");
                }
            }
            else
            {
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.Label("No external events registered.", "SubduedContentStyle", GUILayout.Width(MAIN_WIDTH - 100));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal(); 
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(8);
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                foreach (KeyValuePair<string, bool> extEvent in temp)
                {
                    if (extEvents[extEvent.Key] != temp[extEvent.Key])
                    {
                        extEvents[extEvent.Key] = temp[extEvent.Key];
                    }
                }
            }
        }
    }
}
