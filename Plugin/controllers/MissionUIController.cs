using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections.Generic;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MissionUIController : MonoBehaviourExtended
    {
        private MissionUI gui;
        private OmniController omniController;
        private VesselListener vesselListener;
        private RecoveryListener recoveryListener;

        public OldJSONNode activeMission = OldJSONNode.Parse("{}");
        public List<OldJSONNode> eventList = new List<OldJSONNode>();

        public string missionName = "";
        public string missionDetails = "";
        public string missionPurpose = "";
        public OmniController.UserCaptureState userCapture;
        public bool paused = false;

        internal override void Start()
        {
            gui = FindObjectOfType<MissionUI>();
            if (gui != null) 
            {
                GFLogger.Instance.AddDebugLog("Registered GUI");
                gui.guiState = MissionUI.GUIScreen.Zero_Lookup;
            }

            omniController = FindObjectOfType<OmniController>();
            if (omniController != null)
            {
                GFLogger.Instance.AddDebugLog("Registered OmniController");
                omniController.StateChanged += missionController_StateChanged;
                omniController.EventFired += missionController_EventFired;
                omniController.DataUpdated += omniController_DataUpdated;
            }

            vesselListener = FindObjectOfType<VesselListener>();
            if (vesselListener != null)
            {
                GFLogger.Instance.AddDebugLog("Registered VesselListener");
                vesselListener.EventFired += missionController_EventFired;
                vesselListener.DataUpdated += omniController_DataUpdated;
            }

            recoveryListener = FindObjectOfType<RecoveryListener>();
        }

        public string GetMissionID()
        {
            return activeMission["_id"];
        }

        public void RenameMission(string name, string description)
        {
            omniController.RenameMission(name, description);
        }
        public string GetMissionName(bool truncate = false)
        {
            if (activeMission == null || activeMission["name"] == null)
            {
                return "Unknown Mission";
            }

            string missionName = activeMission["name"];
            if (truncate) {
                var maxLen = 10;
                missionName = missionName.Substring(0, Math.Min(maxLen, missionName.Length)) + (missionName.Length >= maxLen ? "..." : ""); 
            }

            return missionName;
        }
        public string GetMissionDescription()
        {
            if (activeMission == null || activeMission["description"] == null)
            {
                return "";
            }

            return activeMission["description"];
        }
        public void DeleteEvent(int index)
        {
            var missionToDeleteID = activeMission["_id"];
            var eventToDeleteID = eventList[index]["eid"];

            omniController.DeleteEvent(missionToDeleteID, eventToDeleteID);
        }

        public void DeleteMission(FinishedDelegate onDone, FinishedDelegate onFail)
        {
            omniController.DeleteMission(onDone, onFail);
        }
        public void TogglePauseRecording()
        {
            paused = !paused;
            omniController.TogglePauseRecording();
            vesselListener.TogglePauseRecording();
            //recoveryListener.TogglePauseRecording();
        }
        public void DoUserUpload(string eventName)
        {
            omniController.DoUserUpload();
        }
        public void DoUserCapture(string eventName, int videoType)
        {
            if (videoType == VideoOptions.STILL)
                ScreenMessages.PostScreenMessage("Image captured");
            
            omniController.DoUserCapture("User Event", videoType);
        }
        public void DoStopUserCapture()
        {
            omniController.DoStopUserCapture();
        }
        public void CancelUserUpload()
        {
            omniController.CancelUserUpload();
        }
        public int GetUserRecordingSpeed()
        {
            return omniController.GetUserRecordingSpeed();
        }
        public TimeSpan GetUserRecordingTime()
        {
            return omniController.GetUserRecordingTime();
        }
        public string GetUserRecordingTimeString()
        {
            TimeSpan t= omniController.GetUserRecordingTime();
            return String.Format("{0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);
        }
        public void StartRecording()
        {
            gui.guiState = MissionUI.GUIScreen.Two_Initializing;

            var nameTemp = SettingsManager.Instance.settings.username.ToCharArray()[SettingsManager.Instance.settings.username.IndexOf("-") + 1] + "";
            missionName = SettingsManager.Instance.settings.username.ToCharArray()[0] + nameTemp + "-" + HighLogic.fetch.currentGame.launchID.ToString();

            omniController.StartRecording(missionName.ToUpper(), missionDetails, missionPurpose);
        }

        internal override void LateUpdate()
        {
            userCapture = omniController.userCapture;
        }

        private void omniController_DataUpdated(OldJSONNode result)
        {
            //LogFormatted("omniController_DataUpdated (MissionUI)");//, result.ToString());
            GFLogger.Instance.AddDebugLog(result.ToString());
            if (result["data"] != null)
            {
                GFLogger.Instance.AddDebugLog("Picked out data");
                activeMission = result["data"];
            }
            else
            {
                activeMission = result;
            }

            if (SettingsManager.Instance.settings.offlineMode)
            {
                KSP.IO.File.WriteAllText<MissionCreationWorker>(activeMission.ToString(), activeMission["_id"] + ".json");
            }


            var eventArray = activeMission["events"].AsArray;
            GFLogger.Instance.AddDebugLog("\tEvent count = {0}", eventArray.Count);
            eventList.RemoveRange(0, eventList.Count);
            for (int i = eventArray.Count - 1; i >= 0; i--)
            {
                eventList.Add(eventArray[i]);
            }
        }

        void missionController_EventFired(EventModel evt)
        {
            //            gui.events.Add(evt);
            gui.statusMessage = evt.eventDescription;
            //ScreenMessages.PostScreenMessage(evt.eventName, 5, ScreenMessageStyle.UPPER_CENTER);

            if (evt.eventDescription == "Vessel Destroyed")
            {
                gui.vesselDestroyed = true;
            }
        }

        void missionController_StateChanged(OmniController.OmniState oldState, OmniController.OmniState newState)
        {
            GFLogger.Instance.AddDebugLog("missionController_StateChanged({0}, {1})", oldState, newState);
            //ScreenMessages.PostScreenMessage(String.Format("Gameframer: {0} -> {1}", oldState.ToString(), newState.ToString()), 5, ScreenMessageStyle.UPPER_CENTER);
            gui.vesselDestroyed = false;
            switch (newState)
            {
                case OmniController.OmniState.Initializing:
                    gui.statusMessage = "One momement please. Initializing...";
                    gui.guiState = MissionUI.GUIScreen.Zero_Lookup;
                    break;
                case OmniController.OmniState.UnknownMission:
                case OmniController.OmniState.NotRecording:
                    gui.statusMessage = "";
                    gui.guiState = MissionUI.GUIScreen.One_StartPrompt;
                    break;
                case OmniController.OmniState.Error:
                    gui.statusMessage = "Server error, please try again.";
                    gui.guiState = MissionUI.GUIScreen.ServerError;
                    break;
                case OmniController.OmniState.Recording:
                    gui.statusMessage = "Recording started";
                    gui.guiState = MissionUI.GUIScreen.Three_Active;
                    break;
                default: break;
            }
        }

        private void RecordingStarted(IAsyncResult result) {
        }
    }
}
