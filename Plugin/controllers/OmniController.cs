using KSPPluginFramework;
using OldSimpleJSON;
using System;
using UnityEngine;

namespace Gameframer
{
    public delegate void EventDelegate(EventModel evt);
    public delegate void FinishedDelegate(OldJSONNode result);
    public delegate void StateChangedDelegate(OmniController.OmniState oldState, OmniController.OmniState newState);

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class OmniController : MonoBehaviourExtended
    {
        public event EventDelegate EventFired;
        public event FinishedDelegate DataUpdated;
        public event StateChangedDelegate StateChanged;
        private OldJSONNode _serverV;
        public OldJSONNode serverV
        {
            get { return _serverV; }
            private set
            {
                if (_serverV != value)
                {
                    _serverV = value;
                }

                if (DataUpdated != null)
                {
                    DataUpdated(serverV);
                }
            }
        }

        public UserCaptureState userCapture { get; private set; }
        public TimelapseEventWorker timelapseWorker { get; private set; }

        private Vessel currentVessel;
        private VesselWatcher vesselWatcher;
        private Vessel.Situations previousSituation = Vessel.Situations.PRELAUNCH;

        private double lastMissionTime = 0;
        private int _userEvents = 0; // used to provide event name if none provided
        private bool listenersSetup = false;

        private OmniState _state;
        public OmniState state
        {
            get { return _state; }
            private set
            {
                GFLogger.Instance.AddDebugLog("OmniController state changed: {0} -> {1}", _state, value);

                bool didStateChange = false;
                if (_state != value)
                {
                    _state = value;
                    didStateChange = true;
                }

                if (didStateChange || state == OmniState.Error)
                {
                    if (StateChanged != null)
                        StateChanged(state, value);
                }
            }
        }

        #region MonoBehavior Stuff
        internal override void Start()
        {
            state = OmniState.Initializing;
            userCapture = UserCaptureState.Idle;
        }
        internal override void OnDestroy()
        {
            MarksAndTimers.ClearAll();
            RemoveEventListeners();
        }
        internal override void LateUpdate()
        {
            if (state == OmniState.Paused)
                return;

            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (TimeWarp.fetch.current_rate_index > 1)
            {
                //GFLogger.Instance.AddDebugLog("OmniController.LateUpdate skipped, timerate = {0}", TimeWarp.fetch.current_rate_index);
                return;
            }

            try
            {
                CheckVessel();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Gameframer: EXCEPTION in " + System.Reflection.MethodBase.GetCurrentMethod().Name + ".CheckVessel()\n" + e.StackTrace);
            }

            if (FlightGlobals.ActiveVessel == null)
                return;

            if (FlightGlobals.ActiveVessel.isEVA)
                return;

            if (currentVessel == null)
                return;

            // a "sub-mission"
            if (currentVessel.rootPart.missionID == FlightGlobals.ActiveVessel.rootPart.missionID &&
                currentVessel.rootPart.flightID != FlightGlobals.ActiveVessel.rootPart.flightID)
                return;

            if (currentVessel != null && state == OmniState.Recording)
            {
                if (currentVessel.situation != Vessel.Situations.PRELAUNCH)
                {
                    // rate limited inside FlightRecorder
                    vesselWatcher.DoTelemetryUpdate();

                    if ((currentVessel.missionTime - lastMissionTime) > 30)
                    {
                        GFLogger.Instance.AddDebugLog("Status update: {0}, {1}", currentVessel.situation, currentVessel.missionTime);
                        GFLogger.Instance.AddDebugLog("MARKS: {0}", MarksAndTimers.DebugString());
                        lastMissionTime = FlightLogger.met;// currentVessel.missionTime;
                        vesselWatcher.PatchMission(gameObject, KSPUtils.GetStatusPatch(currentVessel.protoVessel));
                        vesselWatcher.SendTelemetry(gameObject);
                    }
                }


                switch (currentVessel.situation)
                {
                    case Vessel.Situations.PRELAUNCH:
                        {
                            break;
                        }
                    case Vessel.Situations.SUB_ORBITAL:
                    case Vessel.Situations.FLYING:
                        {
                            var timerName = "launched" + currentVessel.mainBody.bodyName;
                            var antiTimerName = "landed" + currentVessel.mainBody.bodyName;
                            var timerLimit = 750;

                            if (currentVessel.situation == Vessel.Situations.SUB_ORBITAL && currentVessel.mainBody.bodyName == "Kerbin")
                                break;
                            if (MarksAndTimers.CheckMark(timerName))
                                break;
                            //GFLogger.Instance.AddDebugLog("ascent altitudes: {0:0.0}, {1:0.0}", currentVessel.altitude, KMAUtils.findAltitude(currentVessel.transform));
                            if (currentVessel.altitude < 100)
                            {
                                //GFLogger.Instance.AddDebugLog("too low to mark launch");
                                break;
                            }

                            if (MarksAndTimers.IsRunning(timerName))
                            {
                                GFLogger.Instance.AddDebugLog("Checking {0} ({1} > {2})", timerName, MarksAndTimers.CheckTimer(timerName), timerLimit);
                                if (MarksAndTimers.CheckTimer(timerName) > timerLimit)
                                {
                                    GFLogger.Instance.AddDebugLog("MARKED: '{0}', CLEARED: '{1}'", timerName, antiTimerName);
                                    MarksAndTimers.DoMark(timerName);
                                    MarksAndTimers.ClearMark(antiTimerName);

                                    FireEvent("onLaunch", "Launched from " + currentVessel.mainBody.bodyName, currentVessel);
                                    //FireEvent("onLaunch", "Launched from " + FlightGlobals.ActiveVessel.mainBody.bodyName, FlightGlobals.ActiveVessel);
                                }
                            }
                            else
                            {
                                //MarksAndTimers.StartTimer(timerName);
                            }
                            break;
                        }
                    case Vessel.Situations.ORBITING:
                        {
                            break;
                        }
                    case Vessel.Situations.ESCAPING:
                        {
                            break;
                        }
                    case Vessel.Situations.LANDED:
                    case Vessel.Situations.SPLASHED:
                        {
                            var timerName = "landed" + currentVessel.mainBody.bodyName;
                            var antiTimerName = "launched" + currentVessel.mainBody.bodyName;
                            var timerLimit = 750;

                            if (MarksAndTimers.CheckMark(timerName))
                                break;
                            if (currentVessel.isEVA) // || !currentVessel.IsRecoverable)
                                break;
                            if (previousSituation == Vessel.Situations.PRELAUNCH)
                                break;

                            if (MarksAndTimers.IsRunning(timerName))
                            {
                                var VesselAboveTerrain = FlightGlobals.ActiveVessel.altitude - Math.Max(FlightGlobals.ActiveVessel.pqsAltitude, 0);
                                GFLogger.Instance.AddDebugLog("landing altitudes: {0:0.0}, {1:0.0}, {2:0.0}", VesselAboveTerrain, currentVessel.terrainAltitude, currentVessel.altitude);
                                if (VesselAboveTerrain > 50)
                                {
                                    GFLogger.Instance.AddDebugLog("too high to mark landing");
                                    break;
                                }

                                GFLogger.Instance.AddDebugLog("landing timer is running");
                                if (MarksAndTimers.CheckTimer(timerName) > timerLimit)
                                {
                                    GFLogger.Instance.AddDebugLog("landing timer limit surpassed");
                                    MarksAndTimers.DoMark(timerName);
                                    MarksAndTimers.ClearMark(antiTimerName);
                                    FireEvent("onLand", "Landed on " + currentVessel.mainBody.bodyName, currentVessel);
                                }
                            }
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }

                previousSituation = currentVessel.situation;
            }
        }
        #endregion

        #region Public Methods

        public void TogglePauseRecording()
        {
            if (state == OmniController.OmniState.Paused)
            {
                ScreenMessages.PostScreenMessage("Gameframer unpaused");
                SetupEventListeners();
                state = OmniController.OmniState.Recording;
            }
            else if (state == OmniController.OmniState.Recording)
            {
                ScreenMessages.PostScreenMessage("Gameframer paused");
                RemoveEventListeners();
                state = OmniController.OmniState.Paused;
            }
        }
        public void DeleteEvent(string missionToDeleteID, string eventToDeleteID)
        {
            GFLogger.Instance.AddDebugLog("Doing event delete for : " + missionToDeleteID);
            var w = GFWorker.CreateWorker(this.gameObject, "/missions/" + missionToDeleteID + "/events/" + eventToDeleteID, null, "DELETE");
            w.OnDone = (OldJSONNode n) =>
            {
                serverV = n["data"];
                GFLogger.Instance.AddDebugLog("Delete OK:" + n.ToString());
            };

            w.OnFail = (OldJSONNode n) =>
            {
                GFLogger.Instance.AddDebugLog("Delete FAILED:" + n.ToString());
            };
        }
        public void DeleteMission(FinishedDelegate onDone, FinishedDelegate onFail)
        {
            state = OmniState.UnknownMission;

            var w = GFWorker.CreateWorker(gameObject, "DELETE", "/missions/" + serverV["_id"], OldJSONNode.Parse("{}"));
            w.OnDone = (OldJSONNode n) =>
            {
                state = OmniState.UnknownMission;
                if (onDone != null)
                {
                    onDone(n);
                }
            };
            w.OnFail = (OldJSONNode n) =>
            {
                state = OmniState.Error;
                if (onFail != null)
                {
                    onFail(n);
                }
            };
        }
        public int GetUserRecordingSpeed()
        {
            return timelapseWorker.GetSpeed();
        }
        public TimeSpan GetUserRecordingTime()
        {
            return timelapseWorker.GetRecordingTimeElapsed();
        }
        public void RenameMission(string newName, string description)
        {
            OldJSONNode namePatch = OldJSONNode.Parse("{}");
            namePatch["name"] = newName;
            if (description != null && description.Length > 0)
            {
                namePatch["description"] = description;
            }
            vesselWatcher.PatchMission(this.gameObject, namePatch);
        }
        public void DoStopUserCapture()
        {
            timelapseWorker.StopTimelapse();
        }
        public void CancelUserUpload()
        {
            userCapture = UserCaptureState.Idle;
            GFLogger.Instance.AddDebugLog("Cancel User upload");
            try
            {
                GFLogger.Instance.AddDebugLog("Doing event delete for : " + timelapseWorker.fullMissionID + ", " + timelapseWorker.eventID);
                DeleteEvent(timelapseWorker.fullMissionID, timelapseWorker.eventID);
            }
            catch (Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception deleting user timelapse: {0}", e.Message);
            }

            Destroy(timelapseWorker);
        }
        public void DoUserUpload()
        {
            userCapture = UserCaptureState.Uploading;
            timelapseWorker.DoUpload();
        }
        public void DoUserCapture(string eventNotes, int videoType)
        {
            var pv = FlightGlobals.ActiveVessel.protoVessel;
            var missionID = FlightGlobals.ActiveVessel.isEVA ? vesselWatcher.MissionID() : FlightGlobals.ActiveVessel.rootPart.missionID;
            var flightID = FlightGlobals.ActiveVessel.isEVA ? vesselWatcher.FlightID() : FlightGlobals.ActiveVessel.rootPart.flightID;

            if (videoType == VideoOptions.STILL)
            {
                MissionEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                               GFDataUtils.GetEvent("onUserStill", eventNotes, FlightGlobals.ActiveVessel, true), VideoOptions.STILL, OnDone, OnFail);
            }
            else if (videoType == VideoOptions.TIMELAPSE)
            {
                if (userCapture != UserCaptureState.Capturing)
                {

                    timelapseWorker = TimelapseEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                        GFDataUtils.GetEvent("onUserEvent", eventNotes.Length > 0 ? eventNotes : "User event " + _userEvents++,
                        FlightGlobals.ActiveVessel, true), OnUserDone, OnUserFail, OnUserStopped);
                    userCapture = UserCaptureState.Capturing;
                }
            }
        }
        public void StartRecording(string missionName, string missionDetails = null, string missionPurpose = null)
        {
            SetupEventListeners();
            CheckVessel(true);
            if (state == OmniState.NotRecording || state == OmniState.UnknownMission || state == OmniState.Error)
            {
                GFLogger.Instance.AddDebugLog("Starting recording: {0} {1} {2}", missionName, missionDetails, missionPurpose);
                GFLogger.Instance.AddDebugLog("V = {0}", (currentVessel != null));

                MissionCreationWorker.
                    CreateComponent(gameObject, missionName, missionDetails, missionPurpose, currentVessel, MissionPostDone, MissionPostFail);
            }
        }
        #endregion


        private void FireEvent(string name, string description, Vessel v)
        {
            if (v == null || v.protoVessel == null)
                return;

            if (EventFired != null)
            {
                EventFired(new EventModel(name, description, v));
            }

            var pv = v.protoVessel;
            var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
            var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

            MissionEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                GFDataUtils.GetEvent(name, description, v), VideoOptions.VIDEO, OnDone, OnFail);
        }


        #region Vessel Watcher callbacks
        private void PatchDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));

            serverV = n["data"];
            state = OmniState.Recording;
        }
        private void PatchFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));

            state = OmniState.Error;
        }
        private void MissionPostDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            GFLogger.Instance.AddDebugLog("MissionPostDone: \n" + n.ToString() + "\n");
            if (n["code"] == "1")
            {
                state = OmniState.Error;
            }
            else
            {
                state = OmniState.Recording;
                ScreenMessages.PostScreenMessage("Gameframer: Mission recording started", 2, ScreenMessageStyle.UPPER_LEFT);
                serverV = n["data"];
                vesselWatcher.SetServerResponse(serverV);
            }
        }
        private void MissionPostFail(OldJSONNode n)
        {
            UnityEngine.Debug.Log("MissionPostFail: " + n.ToString());
            ScreenMessages.PostScreenMessage("Gameframer: Error recording mission", 3, ScreenMessageStyle.UPPER_LEFT);
            state = OmniState.Error;
        }
        private void OnUserDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog("OnUserDone");
            serverV = n;
            userCapture = UserCaptureState.Idle;
        }

        private void OnUserStopped(OldJSONNode n)
        {
            userCapture = UserCaptureState.Prompting;
        }
        private void OnUserFail(OldJSONNode n)
        {
            //state = OmniState.Error;
            ScreenMessages.PostScreenMessage("Error capturing event.", 1, ScreenMessageStyle.UPPER_RIGHT);
            userCapture = UserCaptureState.Idle;
            GFLogger.Instance.AddError("User timelapse error: ", n.ToString());
        }
        private void OnDone(OldJSONNode n)
        {
            state = OmniState.Recording;
            if (DataUpdated != null)
            {
                //LogFormatted("OnDone done calling DataUpdated");
                DataUpdated(n);
            }
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            GFLogger.Instance.AddDebugLog(n.ToString());
        }
        private void OnFail(OldJSONNode n)
        {
            state = OmniState.Error;
            GFLogger.Instance.AddError(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }
        private void VesselChangeInitDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog("Gameframer: Known mission, resumed recording.", 2, ScreenMessageStyle.UPPER_LEFT);
            serverV = n;
            SetupEventListeners();
            state = OmniState.Recording;
        }
        private void VesselChangeInitFail(OldJSONNode n)
        {
            // not a real failure. API returns 404 because the mission is not found
            if (n != null && ((String)n["message"]).IndexOf("404") > -1)
            {
                serverV = OldJSONNode.Parse("{}");
                state = OmniState.UnknownMission;
                GFLogger.Instance.AddDebugLog("Unknown mission.");
            }
            // "real" error
            else
            {
                state = OmniState.Error;
                GFLogger.Instance.AddError("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region Events Need Vessel Watcher
        private void onCrewKilled(EventReport r)
        {
            try
            {
                GFLogger.Instance.AddDebugLog(System.Reflection.MethodBase.GetCurrentMethod().Name);
                GFLogger.Instance.AddDebugLog("Crew Killed: {0}, {1}", r.sender, r.eventType);
                GFLogger.Instance.AddDebugLog("Crew Killed: ActiveVessel: {0}", FlightGlobals.ActiveVessel == null ? "null" : FlightGlobals.ActiveVessel.vesselName);
                //GFLogger.Instance.AddDebugLog("Event report: {0}", KSPUtils.GetEventReportString(r));
                // r.sender == kerbal's name who was killed
                // r.eventType == CREW_KILLED
                // r.origin seems to always be null
                FireEvent("onCrewKilled", r.sender + " killed", FlightGlobals.ActiveVessel);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("EXCEPTION in " + System.Reflection.MethodBase.GetCurrentMethod().Name + "\n" + e.StackTrace);
            }
        }
        #endregion

        #region Event Listeners
        private void SetupEventListeners()
        {
            if (listenersSetup)
            {
                GFLogger.Instance.AddDebugLog("Listeners already setup, bailing");
                return;
            }
            GameEvents.onLaunch.Add(this.onLaunch);
            GameEvents.onCrewKilled.Add(this.onCrewKilled);
            // ship redocks ?
            GameEvents.onSameVesselDock.Add(this.onSameVesselDock);
            // ship splits via docking ?
            GameEvents.onSameVesselUndock.Add(this.onSameVesselUndock);
            //GameEvents.onPartAttach.Add(this.onPartAttach);
            GameEvents.onPartCouple.Add(this.onPartCouple);
            GameEvents.onGameSceneLoadRequested.Add(this.onGameSceneLoadRequested);
            GameEvents.onVesselSituationChange.Add(this.onVesselSituationChange);

            GFLogger.Instance.AddDebugLog("Event listeners ADDED in {0}", this.gameObject.GetInstanceID());
            listenersSetup = true;
        }
        private void RemoveEventListeners()
        {
            GameEvents.onLaunch.Remove(this.onLaunch);
            GameEvents.onCrewKilled.Remove(this.onCrewKilled);
            // ship redocks ?
            GameEvents.onSameVesselDock.Remove(this.onSameVesselDock);
            // ship splits via docking ?
            GameEvents.onSameVesselUndock.Remove(this.onSameVesselUndock);
            //GameEvents.onPartAttach.Remove(this.onPartAttach);
            GameEvents.onPartCouple.Remove(this.onPartCouple);
            GameEvents.onGameSceneLoadRequested.Remove(this.onGameSceneLoadRequested);
            GameEvents.onVesselSituationChange.Remove(this.onVesselSituationChange);

            listenersSetup = false;
            GFLogger.Instance.AddDebugLog("Event listeners REMOVED in {0}", this.gameObject.GetInstanceID());
        }
        private void onSameVesselDock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action)
        {
            Part from = action.from.part;
            ProtoVessel fromPV = from.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot fromRoot = fromPV.protoPartSnapshots[fromPV.rootIndex];
            Part to = action.to.part;
            ProtoVessel toPV = to.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot toRoot = toPV.protoPartSnapshots[toPV.rootIndex];
            GFLogger.Instance.AddDebugLog("Couple1: {0} -> {1}", fromPV.vesselName, toPV.vesselName);
            GFLogger.Instance.AddDebugLog("Couple2: {0} -> {1}", fromRoot.missionID, toRoot.missionID);
            GFLogger.Instance.AddDebugLog("Couple3: {0} -> {1}", fromRoot.flightID, toRoot.flightID);

            if (from == null || from.vessel == null || from.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("from vessel " + from.vessel);
            if (to == null || to.vessel == null || to.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("to vessel " + to.vessel);
            Vessel activeVessel = action.from.vessel.isActiveVessel ? action.from.vessel : action.to.vessel;
            ProtoPartSnapshot activeRoot = action.from.vessel.isActiveVessel ? fromRoot : toRoot;
            if (activeVessel != null && activeVessel.isActiveVessel && activeRoot != null)
            {
                string s = String.Format("!!!{0} docked with {1}!!!",
                                                   action.from.vessel.vesselName,
                                                   action.to.vessel.vesselName);
                string antis = String.Format("!!!{0} undocked with {1}!!!",
                                   action.from.vessel.vesselName,
                                   action.to.vessel.vesselName);

                if (MarksAndTimers.CheckMark(s))
                {
                    GFLogger.Instance.AddDebugLog("Skipping existing recorded docking");
                    return;
                }
                GFLogger.Instance.AddDebugLog("MARKED: '{0}', CLEARED: '{1}'", s, antis);
                MarksAndTimers.DoMark(s);
                MarksAndTimers.ClearMark(antis);
                MissionEventWorker.CreateComponent(this.gameObject,
                   activeRoot.missionID, activeRoot.flightID,
                    GetEvent("onDock", s), VideoOptions.VIDEO, OnDone, OnFail);
            }
        }
        private void onSameVesselUndock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action)
        {
            Part from = action.from.part;
            ProtoVessel fromPV = from.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot fromRoot = fromPV.protoPartSnapshots[fromPV.rootIndex];
            Part to = action.to.part;
            ProtoVessel toPV = to.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot toRoot = toPV.protoPartSnapshots[toPV.rootIndex];
            GFLogger.Instance.AddDebugLog("Couple1: {0} -> {1}", fromPV.vesselName, toPV.vesselName);
            GFLogger.Instance.AddDebugLog("Couple2: {0} -> {1}", fromRoot.missionID, toRoot.missionID);
            GFLogger.Instance.AddDebugLog("Couple3: {0} -> {1}", fromRoot.flightID, toRoot.flightID);

            if (from == null || from.vessel == null || from.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("from vessel " + from.vessel);
            if (to == null || to.vessel == null || to.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("to vessel " + to.vessel);
            Vessel activeVessel = action.from.vessel.isActiveVessel ? action.from.vessel : action.to.vessel;
            ProtoPartSnapshot activeRoot = action.from.vessel.isActiveVessel ? fromRoot : toRoot;
            if (activeVessel != null && activeVessel.isActiveVessel && activeRoot != null)
            {
                string antis = String.Format("!!!{0} docked with {1}!!!",
                                                   action.from.vessel.vesselName,
                                                   action.to.vessel.vesselName);
                string s = String.Format("!!!{0} undocked with {1}!!!",
                                                   action.from.vessel.vesselName,
                                                   action.to.vessel.vesselName);
                if (MarksAndTimers.CheckMark(s))
                {
                    GFLogger.Instance.AddDebugLog("Skipping existing recorded undocking");
                    return;
                }
                GFLogger.Instance.AddDebugLog("MARKED: '{0}', CLEARED: '{1}'", s, antis);
                MarksAndTimers.ClearMark(antis);
                MarksAndTimers.DoMark(s);
                MissionEventWorker.CreateComponent(this.gameObject,
                   activeRoot.missionID, activeRoot.flightID,
                    GetEvent("onUndock", s), VideoOptions.VIDEO, OnDone, OnFail);
            }
        }
        private void onPartCouple(GameEvents.FromToAction<Part, Part> action)
        {
            Part from = action.from;
            ProtoVessel fromPV = from.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot fromRoot = fromPV.protoPartSnapshots[fromPV.rootIndex];
            Part to = action.to;
            ProtoVessel toPV = to.protoPartSnapshot.pVesselRef;
            ProtoPartSnapshot toRoot = toPV.protoPartSnapshots[toPV.rootIndex];
            GFLogger.Instance.AddDebugLog("Couple1: {0} -> {1}", fromPV.vesselName, toPV.vesselName);
            GFLogger.Instance.AddDebugLog("Couple2: {0} -> {1}", fromRoot.missionID, toRoot.missionID);
            GFLogger.Instance.AddDebugLog("Couple3: {0} -> {1}", fromRoot.flightID, toRoot.flightID);

            if (from == null || from.vessel == null || from.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("from vessel " + from.vessel);
            if (to == null || to.vessel == null || to.vessel.isEVA)
                return;
            GFLogger.Instance.AddDebugLog("to vessel " + to.vessel);
            Vessel activeVessel = action.from.vessel.isActiveVessel ? action.from.vessel : action.to.vessel;
            ProtoPartSnapshot activeRoot = action.from.vessel.isActiveVessel ? fromRoot : toRoot;
            if (activeVessel != null && activeVessel.isActiveVessel && activeRoot != null)
            {
                string s = String.Format("{0} docked with {1}",
                                                   action.from.vessel.vesselName,
                                                   action.to.vessel.vesselName);
                if (MarksAndTimers.CheckMark(s))
                {
                    GFLogger.Instance.AddDebugLog("Skipping existing recorded docking");
                    return;
                }
                GFLogger.Instance.AddDebugLog("MARKED: '{0}'", s);
                MarksAndTimers.DoMark(s);
                MissionEventWorker.CreateComponent(this.gameObject,
                   activeRoot.missionID, activeRoot.flightID,
                    GetEvent("onDock", s), VideoOptions.VIDEO, OnDone, OnFail);
            }

            GFLogger.Instance.AddDebugLog("Docking done");
        }
        private void onGameSceneLoadRequested(GameScenes scene)
        {
            if (scene == GameScenes.FLIGHT)
            {
            }
            else
            {
                currentVessel = null;
                if (vesselWatcher != null)
                {
                    Destroy(vesselWatcher);
                }
            }
        }

        private void onReachSpace(Vessel v)
        {
            ProtoPartSnapshot root = v.protoVessel.protoPartSnapshots[v.protoVessel.rootIndex];
            MissionEventWorker.CreateComponent(this.gameObject, root.missionID, root.flightID,
                GetEvent("onReachSpace", "Reached space"), VideoOptions.VIDEO, OnDone, OnFail);
        }

        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            try
            {
                if (a.host == currentVessel)
                {
                    GFLogger.Instance.AddDebugLog("Situation Change: {0}, {1}", a.from, a.to);
                    if ((a.from == Vessel.Situations.SUB_ORBITAL && a.to == Vessel.Situations.ORBITING) ||
                        (a.from == Vessel.Situations.ESCAPING && a.to == Vessel.Situations.ORBITING))
                    {

                        ProtoPartSnapshot root = a.host.protoVessel.protoPartSnapshots[a.host.protoVessel.rootIndex];
                        MissionEventWorker.CreateComponent(this.gameObject, root.missionID, root.flightID,
                            GFDataUtils.GetEvent("onOrbit", "Achieved orbit around " + a.host.mainBody.bodyName, a.host),
                            VideoOptions.VIDEO, OnDone, OnFail);
                        if (EventFired != null)
                        {
                            EventFired(new EventModel("onOrbit", "Achieved orbit around " + a.host.mainBody.bodyName, a.host));
                        }
                    }
                    else if (a.from == Vessel.Situations.FLYING && a.to == Vessel.Situations.SUB_ORBITAL ||
                      a.from == Vessel.Situations.ESCAPING && a.to == Vessel.Situations.SUB_ORBITAL)
                    {
                        /*MissionEventWorker.CreateComponent(this.gameObject,
vesselWatcher.GetEventURL(), GetEvent("onSubOrbit", "Sub-orbit around " + a.host.mainBody.bodyName), VideoOptions.VIDEO, OnDone, OnFail);
                        if (EventFired != null)
                        {
                            EventFired(new EventModel("onSubOrbit", "Sub-orbit around " + a.host.mainBody.bodyName, a.host));
                        }*/
                    }
                    else if (a.from == Vessel.Situations.ORBITING && a.to == Vessel.Situations.SUB_ORBITAL)
                    {
                        var pv = FlightGlobals.ActiveVessel.protoVessel;
                        var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
                        var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

                        MissionEventWorker.CreateComponent(this.gameObject,
                        missionID, flightID,//vesselWatcher.GetEventURL(), 
                        GFDataUtils.GetEvent("onLostOrbit", "Lost orbit around " + a.host.mainBody.bodyName, a.host), VideoOptions.VIDEO, OnDone, OnFail);
                        if (EventFired != null)
                        {
                            EventFired(new EventModel("onLostOrbit", "Lost orbit around " + a.host.mainBody.bodyName, a.host));
                        }
                    }
                    else if ((a.from == Vessel.Situations.ORBITING || a.from == Vessel.Situations.SUB_ORBITAL) && a.to == Vessel.Situations.ESCAPING)
                    {

                        /*                        MissionEventWorker.CreateComponent(this.gameObject,
                        vesselWatcher.GetEventURL(), GFDataUtils.GetEvent("onEscape", "Escape trajectory from " + a.host.mainBody.bodyName, a.host), VideoOptions.VIDEO, OnDone, OnFail);
                                                if (EventFired != null)
                                                {
                                                    EventFired(new EventModel("onEscape", "Escape trajectory from " + a.host.mainBody.bodyName, a.host));
                                                }*/
                        //                        GFLogger.Instance.AddDebugLog("On ESCAPE!!!");
                        //GFLogger.Instance.AddDebugLog("ALTITUDE STATS = {0} | {1}, {2}, {3}", v.mainBody.maxAtmosphereAltitude, v.altitude, v.pqsAltitude, v.terrainAltitude);
                    }
                    else if (a.from == Vessel.Situations.LANDED && (a.to == Vessel.Situations.FLYING || a.to == Vessel.Situations.SUB_ORBITAL))
                    {
                        GFLogger.Instance.AddDebugLog("Started timing ascent from {0}", currentVessel.mainBody.bodyName);
                        //ScreenMessages.PostScreenMessage("LAUNCHED FROM " + currentVessel.mainBody.bodyName, 10, ScreenMessageStyle.UPPER_CENTER);
                        MarksAndTimers.StartTimer("launched" + currentVessel.mainBody.bodyName);
                    }
                }
                else
                {
                    //GFLogger.Instance.AddDebugLog("Situation change for other vessel");
                }
            }
            catch (Exception e)
            {
                GFLogger.Instance.AddDebugLog("Situation Change Exception {0}\n{1}", e.Message, e.StackTrace.ToString());
            }
        }
        private void onLaunch(EventReport r)
        {
            // r.eventType == CRASH
            // r.sender == part name that crashed
            // r.other == where it crashed (i.e. terrain)

            GFLogger.Instance.AddDebugLog("onLaunch {0}, {1}, {2}, {3}", r.eventType, r.sender, r.other, r.param);
            GFLogger.Instance.AddDebugLog("Started onLaunch timer inside onLaunch");
            MarksAndTimers.StartTimer("launched" + currentVessel.mainBody.bodyName);
            /*ScreenMessages.PostScreenMessage("onLaunch!");

            if (EventFired != null)
            {
                EventFired(new EventModel("onLaunch!", "Launched!", currentVessel));
            }

            var pv = FlightGlobals.ActiveVessel.protoVessel;
            var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
            var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

            MissionEventWorker.CreateComponent(currentVessel.gameObject, missionID, flightID,
                                               GFDataUtils.GetEvent("onLaunch!", "Launched!", currentVessel), VideoOptions.VIDEO, OnDone, OnFail);*/

        }


        #endregion

        #region Helpers
        private bool CheckVessel(bool force = false)
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return false;
            }

            if (KSPUtils.VesselOK(FlightGlobals.ActiveVessel))
            {
                if (force || currentVessel == null || currentVessel.id != FlightGlobals.ActiveVessel.id)
                {
                    if (force || currentVessel == null || currentVessel.rootPart.missionID != FlightGlobals.ActiveVessel.rootPart.missionID)
                    {
                        lastMissionTime = 0;
                        currentVessel = FlightGlobals.ActiveVessel;

                        //state = OmniState.NotRecording;

                        GFLogger.Instance.AddDebugLog("Vessel switch to {0}/{1}/{2}", currentVessel.rootPart.missionID, currentVessel.rootPart.flightID, currentVessel.rootPart.initialVesselName);
                        MarksAndTimers.ClearAll();
                        if (currentVessel.situation != Vessel.Situations.PRELAUNCH)
                        {
                            MarksAndTimers.DoMark("launched" + currentVessel.mainBody.bodyName);
                        }

                        vesselWatcher = VesselWatcher.CreateComponent(gameObject, currentVessel, VesselChangeInitDone, VesselChangeInitFail, PatchDone, PatchFail);
                    }
                    else
                    {
                        //GFLogger.Instance.AddDebugLog("Skipping switching vessels because {0} == {1}", currentVessel.rootPart.missionID, FlightGlobals.ActiveVessel.rootPart.missionID);
                    }
                }
            }

            return (currentVessel == null);
        }
        private OldJSONNode GetEvent(string eventName, string description)
        {
            return GFDataUtils.GetEvent(eventName, description, currentVessel);
        }
        #endregion

        #region enums
        public enum OmniState
        {
            Initializing,
            UnknownMission,
            NotRecording,
            Recording,
            Paused,
            Error
        }
        ;

        public enum UserCaptureState
        {
            Idle,
            Capturing,
            Prompting,
            Uploading
        };
        #endregion
    }
}

