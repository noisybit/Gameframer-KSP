namespace Gameframer
{
    public class GUIManager : Singleton<GUIManager>
    {
        private VesselsListUI vesselsWindow;
        private SettingsRecordDetailUI settingsDetailWindow;
        private MissionsListUI missionsWindow;
        private DebugUI debugWindow;
        private SettingsUI settingsWindow;

        protected GUIManager()
        {
        }
        public void CloseVesselsWindow()
        {
            if (vesselsWindow != null)
            {
                vesselsWindow.Visible = false;
                Destroy(vesselsWindow);
            }
        }
        public void CloseMissionsWindow()
        {
            if (missionsWindow != null)
            {
                missionsWindow.Visible = false;
                Destroy(missionsWindow);
            }
        }
        public void ToggleSettingsDetailsWindow()
        {
            if (settingsDetailWindow == null || !settingsDetailWindow.Visible)
            {
                settingsDetailWindow = gameObject.AddComponent<SettingsRecordDetailUI>();
                settingsDetailWindow.Visible = true;
                settingsDetailWindow.DragEnabled = true;
                settingsDetailWindow.ClampToScreen = true;
            }
            else
            {
                settingsDetailWindow.Visible = false;
                Destroy(settingsDetailWindow);
            }
        }
        public void ToggleVesselsWindow()
        {
            if (vesselsWindow == null || !vesselsWindow.Visible)
            {
                vesselsWindow = gameObject.AddComponent<VesselsListUI>();
                vesselsWindow.Visible = true;
                vesselsWindow.DragEnabled = true;
                vesselsWindow.ClampToScreen = true;
            }
            else
            {
                vesselsWindow.Visible = false;
                Destroy(vesselsWindow);
            }
        }
        public void ToggleDebugWindow()
        {
            if (debugWindow == null || !debugWindow.Visible)
            {
                debugWindow = gameObject.AddComponent<DebugUI>();
                debugWindow.Visible = true;
                debugWindow.DragEnabled = true;
                debugWindow.ClampToScreen = true;
            }
            else
            {
                debugWindow.Visible = false;
                Destroy(debugWindow);
            }
        }
        public void ToggleSettingsWindow()
        {
            if (settingsWindow == null || !settingsWindow.Visible)
            {
                settingsWindow = gameObject.AddComponent<SettingsUI>();
                settingsWindow.Visible = true;
                settingsWindow.DragEnabled = true;
                settingsWindow.ClampToScreen = true;
            }
            else
            {
                settingsWindow.Visible = false;
                Destroy(settingsWindow);
            }
        }
        public void ToggleMissionsWindow()
        {
            if (missionsWindow == null || !missionsWindow.Visible)
            {
                missionsWindow = gameObject.AddComponent<MissionsListUI>();
                missionsWindow.Visible = true;
            }
            else
            {
                missionsWindow.Visible = false;
                Destroy(missionsWindow);
            }
        }

    }
}

