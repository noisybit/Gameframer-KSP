using KSPPluginFramework;
using OldSimpleJSON;
using System;
using UnityEngine;

namespace Gameframer
{
    [WindowInitials(Caption = "Advanced Settings", Visible = true, DragEnabled = true, ClampToScreen = true, TooltipsEnabled = true)]
    public class SettingsRecordDetailUI : MonoBehaviourWindow
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

        private void CloseWindow()
        {
            SettingsManager.Instance.Save();
            GUIManager.Instance.ToggleSettingsDetailsWindow();
        }

        private void DrawFooterButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Done", GUILayout.Width(75)))
            {
                CloseWindow();
            }
            GUILayout.EndHorizontal();
        }

        private string videoKey = "Y";
        private string imageKey = "T";
        private bool videoKeyEnabled = true;
        private bool imageKeyEnabled = true;

        internal override void DrawWindow(int id)
        {
            if (GUI.Button(new Rect(WindowRect.width - 24, 5, 20, 20), new GUIContent(GameDatabase.Instance.GetTexture("Gameframer/Textures/times_ffffff_17", false))))
            {
                CloseWindow();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginVertical("NormalBox");
            GUILayout.Space(10);

            /**** This programming is bad and you should feel bad ****/
            GUILayout.Label("Automatic Event Capture", "HeaderStyle");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            SettingsManager.Instance.settings.onLaunch = GUILayout.Toggle(SettingsManager.Instance.settings.onLaunch, new GUIContent("Launch"), "ToggleStyle");
            SettingsManager.Instance.settings.onStage = GUILayout.Toggle(SettingsManager.Instance.settings.onStage, new GUIContent("Staging"), "ToggleStyle");
            SettingsManager.Instance.settings.onSubOrbit = GUILayout.Toggle(SettingsManager.Instance.settings.onSubOrbit, new GUIContent("Sub-orbit"), "ToggleStyle");
            SettingsManager.Instance.settings.onOrbit = GUILayout.Toggle(SettingsManager.Instance.settings.onOrbit, new GUIContent("Orbit"), "ToggleStyle");
            SettingsManager.Instance.settings.onLanding = GUILayout.Toggle(SettingsManager.Instance.settings.onLanding, new GUIContent("Landing"), "ToggleStyle");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            SettingsManager.Instance.settings.onEVA = GUILayout.Toggle(SettingsManager.Instance.settings.onEVA, new GUIContent("EVA"), "ToggleStyle");
            SettingsManager.Instance.settings.onDocking = GUILayout.Toggle(SettingsManager.Instance.settings.onDocking, new GUIContent("Docking"), "ToggleStyle");
            SettingsManager.Instance.settings.onVesselDestroyed = GUILayout.Toggle(SettingsManager.Instance.settings.onVesselDestroyed, new GUIContent("Vessel Destroyed"), "ToggleStyle");
            SettingsManager.Instance.settings.onVesselRecovered = GUILayout.Toggle(SettingsManager.Instance.settings.onVesselRecovered, new GUIContent("Vessel Recovered"), "ToggleStyle");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            DrawFooterButtons();
            GUILayout.EndVertical();
        }
    }
}
