using KSPPluginFramework;
using OldSimpleJSON;
using System;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class VesselListener : MonoBehaviourExtended
    {
        public event EventDelegate EventFired;
        public event FinishedDelegate DataUpdated;
        private bool paused = false;

        internal override void Start()
        {
            GFLogger.Instance.AddDebugLog("VesselListener created: " + this.gameObject.GetInstanceID());
            SetupEventListeners();
        }

        internal override void OnDestroy()
        {
            GFLogger.Instance.AddDebugLog("VesselListener destroyed: " + this.gameObject.GetInstanceID());
            RemoveEventListeners();
        }

        public void TogglePauseRecording()
        {
            if (paused)
            {
                ScreenMessages.PostScreenMessage("Gameframer unpaused");
                SetupEventListeners();
                paused = false;
            }
            else
            {
                ScreenMessages.PostScreenMessage("Gameframer paused");
                RemoveEventListeners();
                paused = true;
            }
        }

        internal void SetupEventListeners()
        {
            GameEvents.onVesselWillDestroy.Add(this.onVesselWillDestroy);
            GameEvents.VesselSituation.onLand.Add(this.onLand);
            GameEvents.onCrewOnEva.Add(this.onCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(this.onCrewBoardVessel);
            GameEvents.onVesselSOIChanged.Add(this.onVesselSOIChanged);
            GameEvents.onStageSeparation.Add(this.onStageSeparation);
            GameEvents.onVesselOrbitClosed.Add(this.onVesselOrbitClosed);
            GameEvents.onVesselOrbitClosed.Add(this.onVesselOrbitEscaped);
            GameEvents.onCrash.Add(this.onCrash);
            GameEvents.onStageActivate.Add(this.onStageActivate);
            GameEvents.OnExperimentDeployed.Add(this.OnExperimentDeployed);
            //GameEvents.OnScienceRecieved.Add(this.OnScienceReceived);
            //GameEvents.OnScienceChanged.Add(this.OnScienceChanged);
        }

        internal void RemoveEventListeners()
        {
            GameEvents.onVesselWillDestroy.Remove(this.onVesselWillDestroy);
            GameEvents.VesselSituation.onLand.Remove(this.onLand);
            GameEvents.onCrash.Remove(this.onCrash);
            GameEvents.onCrewOnEva.Remove(this.onCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(this.onCrewBoardVessel);
            GameEvents.onVesselSOIChanged.Remove(this.onVesselSOIChanged);
            GameEvents.onStageSeparation.Remove(this.onStageSeparation);
            GameEvents.onVesselOrbitClosed.Remove(this.onVesselOrbitClosed);
            GameEvents.onVesselOrbitClosed.Remove(this.onVesselOrbitEscaped);
            GameEvents.onCrash.Remove(this.onCrash);
            GameEvents.onStageActivate.Remove(this.onStageActivate);
            GameEvents.OnExperimentDeployed.Remove(this.OnExperimentDeployed);
            //GameEvents.OnScienceRecieved.Remove(this.OnScienceReceived);
            //GameEvents.OnScienceChanged.Remove(this.OnScienceChanged);
        }

        #region Event Listeners
        private void onVesselWillDestroy(Vessel v)
        {
            if (v == null || v.isEVA || !v.isActiveVessel)
                return;

            GFLogger.Instance.AddDebugLog("VesselListener.onVesselWillDestroy");
            GFLogger.Instance.AddDebugLog("VesselListener.onVesselWillDestroy {0}, commandable={1}, vesselType={2}, isActive={3}", v.vesselName, v.isCommandable, v.vesselType, v.isActiveVessel);

            if (v.vesselType == VesselType.Debris || v.vesselType == VesselType.Flag || v.vesselType == VesselType.SpaceObject || v.vesselType == VesselType.Unknown)
            {
                //GFLogger.Instance.AddDebugLog("onVesselRecovered BAILING, VESSEL WAS NOT CONTROLLABLE");
                GFLogger.Instance.AddDebugLog("onVesselWillDestroy BAILING, bad vessel type {0}", v.vesselType);
                return;
            }

            /*if (!v.IsControllable)
            {
                GFLogger.Instance.AddDebugLog("onVesselWillDestroy BAILING, VESSEL WAS NOT CONTROLLABLE");
                return;
            }*/

            if (v.isCommandable && v.IsControllable)
            {
                ProtoPartSnapshot root = v.protoVessel.protoPartSnapshots[v.protoVessel.rootIndex];

                OldJSONNode patch = OldJSONNode.Parse("{}");
                patch["finalized"].AsBool = true;
                OldJSONNode sitPatch = KSPUtils.GetStatusPatch(v.protoVessel);
                sitPatch["situation"] = "Destroyed";
                patch["stats"] = sitPatch;

                if (EventFired != null)
                {
                    EventFired(new EventModel("onVesselWillDestroy", "Vessel Destroyed", v));
                }
                MissionEventWorker.CreateComponent(this.gameObject, root.missionID, root.flightID,
                                                    GFDataUtils.GetEvent("onVesselWillDestroy", "Vessel Destroyed", v),
                                                    VideoOptions.VIDEO, OnDone, OnFail);

                MissionPatchWorker.CreateComponent(this.gameObject, "/missions/" + root.missionID + "/" + root.flightID, patch, OnDone, OnFail);
            }
            /*else
            {
                ProtoPartSnapshot root = v.protoVessel.protoPartSnapshots[v.protoVessel.rootIndex];
                GFLogger.Instance.AddDebugLog("onVesselWillDestroy NOT: {0}, {1}", root.missionID, root.flightID);
            }*/
        }


        private void onLand(Vessel v, CelestialBody b)
        {
            GFLogger.Instance.AddDebugLog("VesselListener.onLand {0}, {1}", v.vesselName, b.bodyName);

            if (!v.isCommandable || !v.isActiveVessel)
                return;

            if (MarksAndTimers.CheckMark("landed" + b.bodyName))
                return;

            /** Starts a timer for landed so we don't record multiple, repeated "hop" landings.
             * Timer is checked in OmniController. **/
            MarksAndTimers.StartTimer("landed" + b.bodyName);
        }


        public void onCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                GFLogger.Instance.AddDebugLog("Crew EVA: {0}, {1}|{2} -> {3}, {4}|{5} ", data.from.partInfo.title, data.from.protoPartSnapshot.flightID,
                data.from.protoPartSnapshot.missionID, data.to.partInfo.title,
                data.to.protoPartSnapshot.flightID,
                 data.to.protoPartSnapshot.missionID);

                if (!MarksAndTimers.CheckMark(data.to.vessel.vesselName))
                {
                    MarksAndTimers.DoMark(data.to.vessel.vesselName);
                    FireEvent("onCrewEVA", data.to.vessel.vesselName + " EVA start", data.from.vessel);
                }
            }
            catch (System.Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception onCrewOnEva {0}", e.Message);
            }
        }


        public void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                GFLogger.Instance.AddDebugLog("Crew BOARD: {0}, {1}|{2} -> {3}, {4}|{5} ", data.from.partInfo.title, data.from.protoPartSnapshot.flightID,
                data.from.protoPartSnapshot.missionID, data.to.partInfo.title,
                data.to.protoPartSnapshot.flightID,
                 data.to.protoPartSnapshot.missionID);

                if (MarksAndTimers.CheckMark(data.from.vessel.vesselName))
                {
                    MarksAndTimers.ClearMark(data.from.vessel.vesselName);

                    FireEvent("onCrewEVAEnd", data.from.vessel.vesselName + " boarded " + data.to.vessel.vesselName, data.to.vessel);
                }
            }
            catch (System.Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception onCrewBoardVessel {0}", e.Message);
            }
        }

        private void onVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
        {
            try
            {
                if (e.host != null && e.host.isActiveVessel)
                {
                    
                    FireEvent("onVesselSOIChanged", "SOI changed to " + e.to.bodyName, e.host);
                }
            }
            catch (System.Exception ex)
            {
                GFLogger.Instance.AddDebugLog("Exception in onVesselSOIChanged: " + ex.Message);
            }
        }

        private void onStageActivate(int i)
        {
            try
            {
                if (!MarksAndTimers.CheckMark(String.Format("Stage{0}", i)))
                {
                    MarksAndTimers.DoMark(String.Format("Stage{0}", i));
                    FireEvent("onStageActivate", "Stage " + i + " activated", FlightGlobals.ActiveVessel);
                }
            }
            catch (System.Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception in onStageActivate: " + e.Message);
            }
        }
        private void OnScienceChanged(float f, TransactionReasons d)
        {
            UnityEngine.Debug.Log("Science Changed: " + f + ", " + d.ToString());
            if (FlightGlobals.ActiveVessel == null)
                return;
             
            FireEvent("onScienceChanged", d.ToString() + " (" + f + ")", FlightGlobals.ActiveVessel);
        }
        private void OnScienceReceived(float f, ScienceSubject s, ProtoVessel pv, bool b)
        {
            UnityEngine.Debug.Log("Science Received: " + f + ", " + s.title);
            if (FlightGlobals.ActiveVessel == null)
                return;

            FireEvent("onScienceReceived", s.title + " (" + f + ")", FlightGlobals.ActiveVessel);

        }
        private void OnExperimentDeployed(ScienceData s)
        {
            if (FlightGlobals.ActiveVessel == null)
                return;

            FireEvent("onExperimentDeployed", s.title, FlightGlobals.ActiveVessel);
        }

        private void onCrash(EventReport r)
        {
            GFLogger.Instance.AddDebugLog("onCrash: {0}", KSPUtils.GetEventReportString(r));
        }

        // Fired for debris that is separator from the main ship
        private void onStageSeparation(EventReport r)
        {
            GFLogger.Instance.AddDebugLog("onStageSeparation: {0}", KSPUtils.GetEventReportString(r));
        }

        public void onVesselOrbitClosed(Vessel v)
        {
            GFLogger.Instance.AddDebugLog("Vessel closed orbit around {0}", v.mainBody.bodyName);
        }

        public void onVesselOrbitEscaped(Vessel v)
        {
            GFLogger.Instance.AddDebugLog("Vessel on escape trajectory from: {0}", v.mainBody.bodyName);
        }
        #endregion

        #region Callbacks
        private void OnDone(OldJSONNode n)
        {
            try
            {
                GFLogger.Instance.AddDebugLog("VesselListener.OnDone\n{0}", n.ToString());
                if (DataUpdated != null)
                {
                    DataUpdated(n);
                }
            }
            catch (Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception in VesselListener.OnDone: {0}", e.Message);
            }
        }
        private void OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog("{0}.{1} ERROR:\n{2}", this._ClassName, System.Reflection.MethodBase.GetCurrentMethod().Name, n.ToString());
        }

        private void FireEvent(string name, string description, Vessel v)
        {
            if (v == null || v.protoVessel == null)
                return;

            if (EventFired != null)
            {
                EventFired(new EventModel(name, description, v));
            }

            try
            {
                var pv = v.protoVessel;
                var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
                var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

                MissionEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                    GFDataUtils.GetEvent(name, description, v), VideoOptions.VIDEO, OnDone, OnFail);
            }
            catch (System.Exception e)
            {
                GFLogger.Instance.AddDebugLog("Exception FireEvent: {0}", e.Message);
            }
        }
        #endregion
    }
}
