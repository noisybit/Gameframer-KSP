using KSPPluginFramework;
using OldSimpleJSON;
using UnityEngine;

namespace Gameframer
{
    [WindowInitials(Caption = "Gameframer Vessels", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class VesselsListUI : MonoBehaviourWindow
    {
        private static int MAIN_WIDTH2 = 600;
        private static int OPEN_HEIGHT2 = 550;
        public enum EditState { VIEWING, EDITING };
        private EditState editState = EditState.VIEWING;
        private EditState newEditState = EditState.VIEWING;
        private bool requestGetVessels = false;
        private bool busy = false;

        internal Vector2 scrollPosition2 = Vector2.zero;
        internal Vector2 scrollPosition3 = Vector2.zero;
        internal Vector2 scrollPosition4 = Vector2.zero;
        private bool closeWindow2 = false;
        protected string vesselsText;

        protected OldJSONNode vessels;
        protected OldJSONNode selectedVessel;
        private string vesselToDeleteID;
        private int selectedVesselIndex = -1;
        private int oldSelectedVesselIndex = -2;

        bool doSave = false;
        OldJSONNode patch;
        string patchVesselName = "";
        string patchVesselDescription = "";

        internal override void Awake()
        {
            requestGetVessels = true;
            WindowRect = new Rect(350, 200, MAIN_WIDTH2, OPEN_HEIGHT2);
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

        private void OnPatchDone(OldJSONNode n)
        {
            editState = EditState.VIEWING;
            selectedVesselIndex = -1;
            selectedVessel = null;
            selectedVessel = n["data"];
            requestGetVessels = true;
        }
        private void OnPatchFail(OldJSONNode n)
        {
            selectedVesselIndex = -1;
            editState = EditState.EDITING;
        }

        internal override void LateUpdate()
        {
            if (closeWindow2)
            {
                Destroy(this);
            }

            if (newEditState != editState)
            {
                if (newEditState == EditState.EDITING)
                {
                    doSave = false;
                    patchVesselName = selectedVessel["name"];
                    patchVesselDescription = selectedVessel["description"];
                    if (patchVesselDescription == null)
                        patchVesselDescription = "";
                    LogFormatted("Editing: {0}", patchVesselName);
                }
                else
                {
                    if (doSave && (patchVesselName != selectedVessel["name"]))
                    {
                        patch = OldJSONNode.Parse("{}");
                        patch["name"] = patchVesselName;
                        patch["description"] = patchVesselDescription;
                        var w2 = GFWorker.CreateWorker(this.gameObject, "PATCH", "/vessels/" + selectedVessel["_id"], patch);
                        busy = true;
                        w2.OnDone = (OldJSONNode n) =>
                        {
                            busy = false;
                            GFLogger.Instance.AddDebugLog("PATCH VESSEL RESPONSE: " + n.ToString());
                        };
                        w2.OnFail = (OldJSONNode n) =>
                        {
                            busy = false;
                            GFLogger.Instance.AddDebugLog("PATCH VESSEL FAIL: " + n.ToString());
                        };
                    }
                    else
                    {
                        LogFormatted("NOT Saving");
                    }

                    patch = OldJSONNode.Parse("{}");
                }

                editState = newEditState;
            }

            if (vesselToDeleteID != null)
            {
                LogFormatted("Doing vessel delete for : " + vesselToDeleteID);
                var w = GFWorker.CreateWorker(this.gameObject, "/vessels/" + vesselToDeleteID, null, "DELETE");
                busy = true;
                w.OnDone = (OldJSONNode n) =>
                {
                    busy = false;
                    GFLogger.Instance.AddDebugLog("DELETE VESSEL RESPONSE: " + n.ToString());
                    selectedVessel = null;
                    selectedVesselIndex = -1;


                    requestGetVessels = true;
                };

                w.OnFail = (OldJSONNode n) =>
                {
                    busy = false;
                    LogFormatted("Error retrieving vessels");
                    GFLogger.Instance.AddDebugLog("DELETE VESSEL FAIL: " + n.ToString());
                    vessels = null;

                    selectedVessel = null;
                    requestGetVessels = true;
                };
                vesselToDeleteID = null;
            }
            else if (requestGetVessels)
            {
                requestGetVessels = false;
                LogFormatted("Getting vessels:");
                var w = GFWorker.CreateWorker(this.gameObject, "/vessels?username=" + SettingsManager.Instance.settings.username + "&sort=-updatedAt", null, "GET");
                busy = true;
                w.OnDone = (OldJSONNode n) =>
                {
                    busy = false;
                    if (n == null)
                    {
                        GFLogger.Instance.AddDebugLog("GET VESSELS RESPONSE: null");
                        vessels = null;
                    }
                    else
                    {
                        GFLogger.Instance.AddDebugLog("GET VESSELS RESPONSE: " + n.ToString());
                        vessels = n["data"];
                    }

                    selectedVessel = null;
                    selectedVesselIndex = -1;
                };

                // Handle error
                w.OnFail = (OldJSONNode n) =>
                {
                    busy = false;
                    if (n == null)
                        GFLogger.Instance.AddDebugLog("GET VESSELS RESPONSE: null");
                    else
                        GFLogger.Instance.AddDebugLog("GET VESSELS FAIL: " + n.ToString());

                    vessels = null;
                    selectedVessel = null;
                    selectedVesselIndex = -1;
                    LogFormatted("Error retrieving vessels");
                };
            }
            else if (selectedVesselIndex != oldSelectedVesselIndex)
            {
                if (selectedVesselIndex == -1)
                {
                    selectedVessel = null;
                    patchVesselName = "";
                    patchVesselDescription = "";
                }
                else
                {
                    newEditState = editState = EditState.VIEWING;
                    selectedVessel = vessels[selectedVesselIndex];
                    patchVesselName = selectedVessel["name"];
                    patchVesselDescription = selectedVessel["description"];
                    if (patchVesselDescription == null)
                        patchVesselDescription = "";
                }
                oldSelectedVesselIndex = selectedVesselIndex;
            }
        }

        private void CloseWindow()
        {
            this.Visible = false;
            Destroy(this);
        }

        private void DrawPartsList(OldJSONNode n)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Parts", "FormHeader");
            if (n["partsHisto"].Count > 0)
            {
                scrollPosition3 = GUILayout.BeginScrollView(scrollPosition3, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                for (int i = n["partsHisto"].Count - 1; i > 0; i--)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("x" + n["partsHisto"][i]["count"], "ListMETText", GUILayout.Width(50));
                    GUILayout.Label(n["partsHisto"][i]["name"], "ListText", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No parts recorded.");
            }
            GUILayout.EndVertical();
        }

        private void DrawVesselGUI(OldJSONNode n)
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
            GUILayout.Label(patchVesselName, "MissionContent");
            GUILayout.Space(8);
            GUILayout.Label("Description", "FormHeader");
            if (editState == EditState.EDITING)
                GUI.enabled = true;
            else
                GUI.enabled = false;
            patchVesselDescription = GUILayout.TextField(patchVesselDescription, "WrappedTextField");
            GUI.enabled = true;

            GUILayout.Space(8);
            DrawPartsList(n);

            GUILayout.EndVertical();
        }

        internal override void DrawWindow(int id)
        {
            if (!this.Visible)
                return;

            if (GUI.Button(new Rect(WindowRect.width - 24, 4, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                CloseWindow();
            }

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                if (vessels == null || vessels.Count == 0)
                {
                    if (busy)
                    {
                        GUILayout.Label("Getting vessel list from Gameframer.com.", "BillboardContent", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                    else
                    {
                        GUILayout.Label("No vessels. Upload them from the VAB or SPH.", "BillboardContent",
                            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    {
                        GUILayout.BeginVertical(GUILayout.Width(200));
                        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        {
                            //selectedMissionIndex = GUILayout.SelectionGrid(selectedMissionIndex, vesselNames.ToArray(), 1);
                            if (vessels != null && vessels.Count > 0)
                            {
                                GUILayout.Space(16);
                                for (int i = 0; i < vessels.Count; i++)
                                {
                                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    if (selectedVesselIndex == i)
                                    {
                                        GUI.enabled = false;
                                    }
                                    if (GUILayout.Button(" ", GUILayout.Width(40)))
                                    {
                                        selectedVesselIndex = i;
                                    }
                                    GUI.enabled = true;
                                    GUILayout.Label(vessels[i]["name"], "ListText", GUILayout.ExpandWidth(true));
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                        GUILayout.EndScrollView();
                        if (GUILayout.Button(new GUIContent("Vessel Gallery", "Open your vessel gallery in an external browser.")))
                        {
                            Application.OpenURL(GameframerService.GetWebBase() + "ksp/vessels?username=" + SettingsManager.Instance.settings.username);
                        }
                        GUILayout.EndVertical();
                        scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        {
                            GUILayout.Space(8);
                            if (selectedVessel != null)
                            {
                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                GUILayout.Label("Vessel Details", "FormHeader", GUILayout.ExpandWidth(true));
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
                                    //if (GUILayout.Button(new GUIContent(
                                    //GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ff0000_17", false), "Discard changes"), "SmallIconStyle"))
                                    if (GUILayout.Button("Cancel"))
                                    {
                                        doSave = false;
                                        newEditState = EditState.VIEWING;
                                    }
                                    if (GUILayout.Button("Save"))
                                    //if (GUILayout.Button(new GUIContent(
                                    //GameDatabase.Instance.GetTexture("Gameframer/Textures/check_0fe00f_17", false), "Save changes"), "SmallIconStyle"))
                                    {
                                        doSave = true;
                                        newEditState = EditState.VIEWING;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.Space(8);
                                DrawVesselGUI(selectedVessel);
                                GUILayout.Space(8);
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(8);
                                if (GUILayout.Button(new GUIContent("Delete vessel", "DELETE from Gameframer.com. This CANNOT be undone."), "RedButtonStyle", GUILayout.Width(125), GUILayout.Height(35)))
                                {
                                    vesselToDeleteID = selectedVessel["_id"];
                                    LogFormatted("vesselToDelete: " + vesselToDeleteID);
                                }
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(new GUIContent("Gameframer",
                                    GameDatabase.Instance.GetTexture("Gameframer/Textures/external-link_ffffff_17", false),
                                    "View <b>" + selectedVessel["name"] + "</b> on gameframer.com (opens in a browser)"), GUILayout.Width(125), GUILayout.Height(35)))
                                {
                                    Application.OpenURL(GameframerService.GetWebBase() + "ksp/vessels/" + selectedVessel["_id"]);
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
                }
            }
            GUILayout.EndVertical();
        }
    }
}
