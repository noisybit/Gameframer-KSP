using KSPPluginFramework;
using OldSimpleJSON;
using System;
using KSP.UI.Screens;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class RecoveryListener : MonoBehaviourExtended
    {
        internal override void Start()
        {
            GFLogger.Instance.AddDebugLog("RecoveryListener created");
            GameEvents.onVesselRecoveryProcessing.Add(this.onVesselRecoveryProcessing);
            GameEvents.onVesselRecovered.Add(this.onVesselRecovered);
            GameEvents.onVesselTerminated.Add(this.onVesselTerminated);
        }

        internal override void OnDestroy()
        {
            GFLogger.Instance.AddDebugLog("RecoveryListener destroyed");
            GameEvents.onVesselRecoveryProcessing.Remove(this.onVesselRecoveryProcessing);
            GameEvents.onVesselRecovered.Remove(this.onVesselRecovered);
            GameEvents.onVesselTerminated.Remove(this.onVesselTerminated);
        }

        public void onVesselRecoveryProcessing(ProtoVessel pv, MissionRecoveryDialog d, float f)
        {
            if (pv.vesselType == VesselType.Debris || pv.vesselType == VesselType.Flag || pv.vesselType == VesselType.SpaceObject || pv.vesselType == VesselType.Unknown)
            {
                //GFLogger.Instance.AddDebugLog("onVesselRecovered BAILING, VESSEL WAS NOT CONTROLLABLE");
                GFLogger.Instance.AddDebugLog("onVesselRecoveryProcessing BAILING, bad vessel type {0}", pv.vesselType);
                return;
            }
            /*if (!pv.wasControllable)
            {
                GFLogger.Instance.AddDebugLog("onVesselRecoveryProcessing BAILING, VESSEL WAS NOT CONTROLLABLE");
                return;
            }*/

            GFLogger.Instance.AddDebugLog(System.Reflection.MethodBase.GetCurrentMethod().Name + ", name = " + pv.vesselName + ", met = " + pv.missionTime);
            GFLogger.Instance.AddDebugLog("RECOVERY: {0}, {1}, {2}", pv.vesselName, pv.wasControllable, pv.vesselType);
            GFLogger.Instance.AddDebugLog("{0}/{1} recoveryProcessing", pv.protoPartSnapshots[pv.rootIndex].missionID, pv.protoPartSnapshots[pv.rootIndex].flightID);

            var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
            var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;
            GFLogger.Instance.AddDebugLog("Recovery {0}/{1}", missionID, flightID);            
            OldJSONNode recoveryPatch = OldJSONNode.Parse("{}");
            recoveryPatch["recoveryFactor"] = d.recoveryFactor;
            recoveryPatch["recoveryLocation"] = d.recoveryLocation;
            recoveryPatch["fundsEarned"].AsDouble = d.fundsEarned;
            recoveryPatch["reputationEarned"].AsDouble = d.reputationEarned;
            recoveryPatch["scienceEarned"].AsDouble = d.scienceEarned;
            OldJSONNode patch = OldJSONNode.Parse("{}");
            patch["recovery"] = recoveryPatch;

            MissionPatchWorker.CreateComponent(this.gameObject, "/missions/" + missionID + "/" + flightID, patch, OnDone, OnFail);
        }

        private void onVesselRecovered(ProtoVessel pv, bool b)
        {
            GFLogger.Instance.AddDebugLog(System.Reflection.MethodBase.GetCurrentMethod().Name + ", name = " + pv.vesselName + ", met = " + pv.missionTime);
            GFLogger.Instance.AddDebugLog("RECOVERED: {0}, {1}, {2}", pv.vesselName, pv.wasControllable, pv.vesselType);

            if (pv == null)
                return;

            if (pv.vesselType == VesselType.Debris || pv.vesselType == VesselType.Flag || pv.vesselType == VesselType.SpaceObject || pv.vesselType == VesselType.Unknown)
            {
                //GFLogger.Instance.AddDebugLog("onVesselRecovered BAILING, VESSEL WAS NOT CONTROLLABLE");
                GFLogger.Instance.AddDebugLog("onVesselRecovered BAILING, bad vessel type {0}", pv.vesselType);
                return;
            }
            /*if(pv.vesselType == VesselType.EVA)
            {
                GFLogger.Instance.AddDebugLog("onVesselRecovered BAILING, EVA");
                return;
            } */
            var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
            var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;
            
            OldJSONNode patch = OldJSONNode.Parse("{}");
            patch["finalized"].AsBool = true;
            OldJSONNode sitPatch = OldJSONNode.Parse("{}");
            sitPatch["missionTimeInDays"].AsDouble = KSPUtils.GameTimeInDays(pv.missionTime);
            sitPatch["situation"] = "Recovered";
            sitPatch["vesselName"] = pv.vesselName;
            patch["stats"] = sitPatch;

            patch = KSPUtils.GetStatusPatch(pv);            
            MissionPatchWorker.CreateComponent(this.gameObject, "/missions/" + missionID + "/" + flightID, patch, OnDone, OnFail);

            var theEvent = GFDataUtils.GetEvent("onRecovered", "Recovered", pv.vesselRef);
            GFLogger.Instance.AddDebugLog("Recovered event: {0}", theEvent.ToString());
            MissionEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                theEvent, VideoOptions.NONE, OnDone, OnFail);
        }

        // deleted from the tracking station
        private void onVesselTerminated(ProtoVessel pv)
        {
            GFLogger.Instance.AddDebugLog(System.Reflection.MethodBase.GetCurrentMethod().Name + ", name = " + pv.vesselName + ", met = " + pv.missionTime);
            GFLogger.Instance.AddDebugLog("{0}/{1} terminated", pv.protoPartSnapshots[pv.rootIndex].missionID, pv.protoPartSnapshots[pv.rootIndex].flightID);
            var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
            var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

            OldJSONNode patch = OldJSONNode.Parse("{}");
            patch["finalized"].AsBool = true;
            OldJSONNode sitPatch = KSPUtils.GetStatusPatch(pv);
            sitPatch["missionTimeInDays"].AsDouble = KSPUtils.GameTimeInDays(pv.missionTime);
            sitPatch["situation"] = "Terminated";
            sitPatch["vesselName"] = pv.vesselName;
            patch["stats"] = sitPatch;

            MissionPatchWorker.CreateComponent(this.gameObject, "/missions/" + missionID + "/" + flightID, patch, OnDone, OnFail);

            MissionEventWorker.CreateComponent(this.gameObject, missionID, flightID,
                GFDataUtils.GetEvent("onTerminated", "Terminated", pv.vesselRef), VideoOptions.NONE, OnDone, OnFail);
        }

        private void OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }
        private void OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddError(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }
    }
}
