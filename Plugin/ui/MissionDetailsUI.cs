using KSPPluginFramework;
using OldSimpleJSON;
using System;
using UnityEngine;

namespace Gameframer
{
    [WindowInitials(Caption = "Mission Details", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class MissionDetailsUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH = 400;
        private static int OPEN_HEIGHT = 250;

        internal Vector2 scrollPosition = Vector2.zero;
        private Rect windowSize;
        string missionName = "";
        string missionDescription = "";
        bool dirty = false;
        private MissionUIController uiController;

        internal override void Awake()
        {
            uiController = FindObjectOfType<MissionUIController>();
            windowSize = new Rect(200, 350, MAIN_WIDTH, OPEN_HEIGHT);
            WindowRect = windowSize;
            Visible = true;

            missionName = uiController.GetMissionName();
            missionDescription = uiController.GetMissionDescription();
        }

        internal override void OnGUIOnceOnly()
        {
            SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
        }

        private void SetWindowSize(int width, int height)
        {
            windowSize = new Rect(WindowRect.x, WindowRect.y, width, height);
            WindowRect = windowSize;
        }

        internal override void OnDestroy()
        {
        }

        internal override void OnGUIEvery()
        {
        }

        private void DrawEventsList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(125));
            if (uiController.eventList.Count > 0)
            {
                for (int i = 0; i < uiController.eventList.Count; i++)
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("x", "SmallXButton"))
                    {
                        uiController.DeleteEvent(i);
                        GUILayout.EndHorizontal(); // end the block before breaking out 
                        break;
                    }

                    var metLabel = String.Format("{0:0}", uiController.eventList[i]["missionTime"].AsDouble);
                    GUILayout.Label(metLabel, "ListMETText", GUILayout.Width(60));
                    GUILayout.Label((string)uiController.eventList[i]["description"], "ListText", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Space(12);
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUILayout.Label("No events recorded.", "SubduedText");
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void CloseWindow()
        {
            this.Visible = false;
            Destroy(this);
        }

        private void SaveChanges()
        {
            uiController.RenameMission(missionName, missionDescription);
        }

        private void DrawSettingsInputs()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Name", "WhiteRightLabelStyle", GUILayout.Width(75));
            missionName = GUILayout.TextField(missionName, 40);
            if (GUI.changed)
            {
                dirty = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Description", "WhiteRightLabelStyle", GUILayout.Width(75));
            missionDescription = GUILayout.TextArea(missionDescription, 500, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            if (GUI.changed)
            {
                dirty = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Events", "WhiteRightLabelStyle", GUILayout.Width(75));
            DrawEventsList();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        private void DrawSettingsButtons()
        {
            GUILayout.BeginHorizontal("NormalBox", GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("View mission page", "Open mission page in browser"), GUILayout.Width(125), GUILayout.Height(35)))
            {
                Application.OpenURL(GameframerService.GetWebBase() + "ksp/missions/" + uiController.activeMission["_id"]);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(uiController.paused ? "Resume recording" : "Pause recording", GUILayout.Width(125), GUILayout.Height(35)))
            {
                uiController.TogglePauseRecording();
            }
            GUILayout.EndHorizontal();
        }
        private void DeleteDone(OldJSONNode n)
        {
            ScreenMessages.PostScreenMessage(n["data"]["name"] + " has been deleted.", 5, ScreenMessageStyle.UPPER_RIGHT);
            GUI.enabled = true;
            this.Visible = false;
            Destroy(this);
        }
        private void DeleteFailed(OldJSONNode n)
        {
            ScreenMessages.PostScreenMessage("Error deleting mission.", 5, ScreenMessageStyle.UPPER_RIGHT);
            GUI.enabled = true;
        }
        private void DrawFooterButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("Delete mission", "DELETE from Gameframer.com. This CANNOT be undone."), "RedButtonStyle", GUILayout.Width(125)))
            {
                uiController.DeleteMission(DeleteDone, DeleteFailed);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(75)))
            {
                CloseWindow();
            }
            if (!dirty)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Save", GUILayout.Width(75)))
            {
                SaveChanges();
                CloseWindow();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        internal override void DrawWindow(int id)
        {
            if (GUI.Button(new Rect(WindowRect.width - 24, 5, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                CloseWindow();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginVertical("NormalBox");
            GUILayout.Space(10);
            DrawSettingsInputs();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            DrawSettingsButtons();
            GUILayout.Space(10);
            DrawFooterButtons();
            GUILayout.EndVertical();
        }
    }
}
