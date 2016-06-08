using KSPPluginFramework;
using OldSimpleJSON;
using System;
using UnityEngine;

namespace Gameframer
{
    [WindowInitials(Caption = "Gameframer Missions", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class MissionsListUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH2 = 600;
        private static int OPEN_HEIGHT2 = 550;
        public enum EditState { VIEWING, EDITING };
        private EditState editState = EditState.VIEWING;
        private EditState newEditState = EditState.VIEWING;
        private bool requestGetMissions = false;
        private bool editEvents = false;
        private bool busy = false;

        internal Vector2 scrollPosition2 = Vector2.zero;
        internal Vector2 scrollPosition3 = Vector2.zero;
        internal Vector2 scrollPosition4 = Vector2.zero;
        private bool closeWindow2 = false;
        protected string missionsText;

        protected OldJSONNode missions;
        protected OldJSONNode selectedMission;
        protected string lastSelectedMissionID;
        protected string newSelectedMissionID;
        private string missionToDeleteID;
        private string eventToDeleteID;
        private int selectedMissionIndex = 0;

        bool doSave = false;
        OldJSONNode patch;
        string patchName = "";
        string patchDescription = "";

        internal override void Awake()
        {
            requestGetMissions = true;
            WindowRect = new Rect(400, 250, MAIN_WIDTH2, OPEN_HEIGHT2);
            Visible = true;
            patch = OldJSONNode.Parse("{}");
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

        private void OnPatchDone(OldJSONNode n)
        {
            editState = EditState.VIEWING;
            newSelectedMissionID = null;
            selectedMission = n["data"];
            requestGetMissions = true;
        }
        private void OnPatchFail(OldJSONNode n)
        {
            editState = EditState.EDITING;
        }

        private int FindMissionIndex(string idToFind)
        {
            int index = -1;
            for (int i = 0; i < missions.AsArray.Count; i++)
            {
                if (idToFind.Equals(missions[i]["_id"]))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private bool HasUserEvents(OldJSONNode n)
        {
            foreach (OldJSONNode e in n["events"].AsArray)
            {
                if (e["userEvent"].AsBool)
                {
                    return true;
                }
            }
            return false;
        }

        internal override void LateUpdate()
        {
            if (closeWindow2)
            {
                Destroy(this);
            }

            if (newSelectedMissionID != null && !newSelectedMissionID.Equals(lastSelectedMissionID))
            {
                // swap selections
                selectedMissionIndex = FindMissionIndex(newSelectedMissionID);
                selectedMission = missions[selectedMissionIndex];
                lastSelectedMissionID = newSelectedMissionID;

                if (selectedMissionIndex == -1)
                {
                    LogFormatted("Couldn't find mission {0}", lastSelectedMissionID);
                    selectedMission = null;
                    newEditState = editState = EditState.VIEWING;
                    patchDescription = "";
                    patchName = "";
                }
                else
                {
                    newEditState = editState = EditState.VIEWING;
                    patchDescription = selectedMission["description"];
                    if (patchDescription == null)
                        patchDescription = "";
                    patchName = selectedMission["name"];
                }

            }

            if (newEditState != editState)
            {
                // transition to editing
                if (newEditState == EditState.EDITING)
                {
                    doSave = false;
                    patchName = selectedMission["name"];
                    patchDescription = selectedMission["description"];
                }
                else
                // transition back to viewing
                {
                    if (doSave)
                    {
                        patch = OldJSONNode.Parse("{}");
                        patch["name"] = patchName;
                        //patch["description"] = patchDescription;
                        //LogFormatted("Saving: {0}", patch.ToString());
                        MissionPatchWorker.CreateComponent(this.gameObject, "/missions/" + selectedMission["_id"], patch, OnPatchDone, OnPatchFail);
                    }

                    patch = OldJSONNode.Parse("{}");
                }

                editState = newEditState;
            }

            if (missionToDeleteID != null)
            {
                LogFormatted("Doing mission delete for : " + missionToDeleteID);
                var w = GFWorker.CreateWorker(this.gameObject, "/missions/" + missionToDeleteID, null, "DELETE");
                busy = true;
                w.OnDone = (OldJSONNode n) =>
                {
                    busy = false;
                    selectedMission = null;
                    requestGetMissions = true;
                };

                w.OnFail = (OldJSONNode n) =>
                {
                    busy = false;
                    missions = null;
                    selectedMission = null;
                    requestGetMissions = true;
                };
                missionToDeleteID = null;
            }
            else if (requestGetMissions)
            {
                requestGetMissions = false;
                var w = GFWorker.CreateWorker(this.gameObject, "/missions?username=" + SettingsManager.Instance.settings.username + "&sort=-updatedAt&web=1", null, "GET");
                busy = true;
                w.OnDone = (OldJSONNode n) =>
                {
                    busy = false;
                    missions = OldJSONNode.Parse("[]");
                    foreach (OldJSONNode n2 in n.AsArray)
                    {
                        if (n2["_id"] != null && ((String)n2["_id"]).Length > 0)
                        {
                            missions.AsArray.Add(n2);
                            //GFLogger.Instance.AddDebugLog((i++) + " : " + n2.ToString());
                        }
                    }
                    selectedMission = null;
                };

                // Handle error
                w.OnFail = (OldJSONNode n) =>
                {
                    busy = false;
                    missions = null;
                    selectedMission = null;
                };
            }

            if (eventToDeleteID != null)
            {
                LogFormatted("Doing event delete for : " + eventToDeleteID);
                var url = String.Format("/missions/{0}/events/{1}", selectedMission["_id"], eventToDeleteID);
                LogFormatted("URL: {0}", url);
                var w = GFWorker.CreateWorker(this.gameObject, "/missions/" + selectedMission["_id"] + "/events/" + eventToDeleteID, null, "DELETE");
                busy = true;
                w.OnDone = (OldJSONNode n) =>
                {
                    busy = false;
                    LogFormatted("Refreshing events for mission");
                    var w2 = GFWorker.CreateWorker(this.gameObject, "/missions/" + selectedMission["_id"] + "/events", null, "GET");
                    w2.OnDone = (OldJSONNode n2) =>
                        {
                            selectedMission["events"] = n2["data"];
                        };
                };

                w.OnFail = (OldJSONNode n) =>
                {
                    busy = false;
                    LogFormatted("Delete event failed!");
                    LogFormatted(n.ToString());
                    //events = null;
                    //selectedevent = null;
                    requestGetMissions = true;
                };
                eventToDeleteID = null;
            }
        }

        private void CloseWindow()
        {
            LogFormatted("CloseWindow");
            this.Visible = false;
            Destroy(this);
        }

        private void DrawEventsList(OldJSONNode n)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Events", "FormHeader");
            GUILayout.FlexibleSpace();
/*            if (!editEvents)
            {
                //if (hasUserEvents)
                {
                    if (GUILayout.Button("Edit"))
                    {
                        editEvents = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Done"))
                {
                    editEvents = false;
                    doSaveEvents = true;
                }
            }*/
            GUILayout.EndHorizontal();
            if (n["events"].Count > 0)
            {
                scrollPosition3 = GUILayout.BeginScrollView(scrollPosition3, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.Space(8);
                //for (int i = n["events"].Count - 1; i >= 0; i--)
                for (int i = 0; i < n["events"].AsArray.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    //if (editEvents/* && n["events"][i]["userEvent"].AsBool*/)
                    {
                        if (GUILayout.Button(new GUIContent("x", "Delete event"), "SmallXButton"))
                        {
                            eventToDeleteID = n["events"][i]["eid"];
                            LogFormatted("eventToDeleteID: " + eventToDeleteID);
                        }
                    }
                    GUILayout.Label(i + ": ", "ListText", GUILayout.Width(20));
                    if (editEvents && n["events"][i]["userEvent"].AsBool)
                    {
                        n["events"][i]["description"] = GUILayout.TextField(n["events"][i]["description"], "WrappedTextField", GUILayout.ExpandWidth(true), GUILayout.Height(75));
                    }
                    else
                    {
                        GUILayout.Label(n["events"][i]["description"], "ListText", GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(8);
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No events recorded.");
            }
        }

        private void DrawMissionGUI(OldJSONNode n)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            if (editState == EditState.EDITING)
                GUI.enabled = true;
            else
                GUI.enabled = false;
            GUILayout.Label("Name", "FormHeader", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (patchName != null)
                patchName = GUILayout.TextField(patchName, GUILayout.ExpandWidth(true));
            else
                GUILayout.Label("no name");
            GUI.enabled = true;

            GUILayout.Space(8);
            if (editState == EditState.EDITING)
                GUI.enabled = true;
            else
                GUI.enabled = false;
            GUILayout.Label("Description", "FormHeader");
            if (patchDescription != null)
                patchDescription = GUILayout.TextField(patchDescription, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            else
                GUILayout.Label("No description");

            GUI.enabled = true;

            GUILayout.Space(8);

            DrawEventsList(n);

            GUILayout.EndVertical();
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    GUILayout.FlexibleSpace();
                    if (GUI.Button(new Rect(WindowRect.width - 24, 4, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
                    {
                        CloseWindow();
                    }
                }
                GUILayout.EndHorizontal();

                if (missions == null || missions.AsArray.Count == 0)
                {
                    if (busy)
                    {
                        GUILayout.Label("Getting mission list from Gameframer.com.", "BillboardContent", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                    else
                    {
                        GUILayout.Label("No missions. Upload them as you are flying.", "BillboardContent", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                }
                else
                {
                    if (busy) GUI.enabled = false;
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    {
                        GUILayout.BeginVertical(GUILayout.Width(200));
                        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        {
                            //selectedMissionIndex = GUILayout.SelectionGrid(selectedMissionIndex, missionNames.ToArray(), 1);
                            if (missions != null && missions.Count > 0)
                            {
                                GUILayout.Space(8);
                                //for (int i = missions.Count - 1; i > 0; i--)
                                for (int i = 0; i < missions.AsArray.Count; i++)
                                {
                                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    if (selectedMission == missions[i])
                                    {
                                        GUI.enabled = false;
                                    }
                                    //if (GUILayout.Button(" " + i, GUILayout.Width(40)))
                                    if (GUILayout.Button(" ", GUILayout.Width(40)))
                                    {
                                        newSelectedMissionID = missions[i]["_id"];
                                        LogFormatted("Set new selected mission {0}", newSelectedMissionID);
                                    }

                                    GUI.enabled = true;
                                    if (busy) GUI.enabled = false;
                                    GUILayout.Label(missions[i]["name"], "ListText", GUILayout.ExpandWidth(true));
                                    GUILayout.Label(missions[i], "ListText", GUILayout.ExpandWidth(true));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.Space(8);
                            }
                        }
                        GUILayout.EndScrollView();
                        if (GUILayout.Button(new GUIContent("Mission Gallery", "Open your mission gallery in an external browser.")))
                        {
                            Application.OpenURL(GameframerService.GetWebBase() + "ksp/missions?username=" + SettingsManager.Instance.settings.username);
                        }
                        GUILayout.EndVertical();

                        scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        {
                            GUILayout.Space(8);
                            if (selectedMission != null)
                            {
                                GUILayout.Space(8);
                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                GUILayout.Label("Mission Details", "FormHeader", GUILayout.ExpandWidth(true));
                                GUILayout.FlexibleSpace();
                                if (editState == EditState.VIEWING)
                                {
                                    if (GUILayout.Button("Edit"))
                                    {
                                        newEditState = EditState.EDITING;
                                    }
                                }
                                else if (editState == EditState.EDITING)
                                {
                                    if (GUILayout.Button("Cancel"))
                                    {
                                        doSave = false;
                                        newEditState = EditState.VIEWING;
                                    }
                                    if (GUILayout.Button("Save"))
                                    {
                                        doSave = true;
                                        newEditState = EditState.VIEWING;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.Space(8);
                                DrawMissionGUI(selectedMission);
                                GUILayout.FlexibleSpace();
                                GUILayout.Space(8);
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(8);
                                if (GUILayout.Button(new GUIContent("Delete mission", "DELETE from Gameframer.com. This CANNOT be undone."), "RedButtonStyle", GUILayout.Width(125), GUILayout.Height(35)))
                                {
                                    missionToDeleteID = selectedMission["_id"];
                                    LogFormatted("missionToDelete: " + missionToDeleteID);
                                }
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(new GUIContent("Gameframer",
                                    GameDatabase.Instance.GetTexture("Gameframer/Textures/external-link_ffffff_17", false),
                                    "View mission on gameframer.com (opens in a browser)"), GUILayout.Width(125), GUILayout.Height(35)))
                                {
                                    Application.OpenURL(GameframerService.GetWebBase() + "ksp/missions/" + selectedMission["_id"]);
                                }
                                GUILayout.Space(8);
                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(8);
                        GUILayout.EndHorizontal();
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
            }
            GUILayout.EndVertical();
        }
    }
}
