using KerbalEngineer;
using KerbalEngineer.VesselSimulator;
using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gameframer
{
    public class GFDataUtils
    {
        private static Dictionary<string, string> partModMap = new Dictionary<string, string>();
        private static bool isModMapGenerated = false;
        private static Dictionary<string, string> GetModDictionary()
        {
            //foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies) {
            //LogFormatted ("Assembly: {0}", a.name);
            //}

            if (isModMapGenerated)
            {
                return partModMap;
            }

            try
            {
                GFLogger.Instance.AddDebugLog("Generating mod map");
                var path = new KARSettings(KARSettings.LOCATION).FilePath.Split(System.IO.Path.AltDirectorySeparatorChar);
                var fullPath = "";
                for (int i = 0; i < path.Length; i++)
                {
                    GFLogger.Instance.AddDebugLog("{0}: {1}", i, path[i]);
                    fullPath += path[i] + System.IO.Path.AltDirectorySeparatorChar;
                    GFLogger.Instance.AddDebugLog(i + ": " + fullPath);
                    if (path[i].ToUpper() == "GAMEDATA")
                        break;
                }
                GFLogger.Instance.AddDebugLog("Correct path = " + fullPath);

                var fileList = new System.IO.DirectoryInfo(fullPath).GetFiles("*.cfg", System.IO.SearchOption.AllDirectories);
                GFLogger.Instance.AddDebugLog("Found " + fileList.Length + " potential parts");
                string sdebug = "PART, MOD\n";
                foreach (System.IO.FileInfo fi in fileList)
                {
                    var cn = ConfigNode.Load(fi.FullName);
                    var partNodes = cn.GetNodes("PART");
                    if (partNodes != null && partNodes.Length > 0)
                    {
                        int gdIndex = fi.DirectoryName.IndexOf("GameData");
                        string s = fi.DirectoryName.Substring(gdIndex + ("GameData" + System.IO.Path.DirectorySeparatorChar).Length);
                        var modName = s.Substring(0, s.IndexOf(System.IO.Path.DirectorySeparatorChar));
                        var n1 = partNodes[0].GetValues()[0];
                        sdebug += n1 + ", " + modName + "\n";
                        partModMap.Add(n1, modName);
                    }
                }

                isModMapGenerated = true;
                //if (GFLogger.PRINT_DEBUG_INFO)
                {
                    KSP.IO.File.WriteAllText<EditorController>(sdebug, "modmap.csv");
                }
            }
            catch (Exception e)
            {
                isModMapGenerated = false;
                GFLogger.Instance.AddDebugLog(String.Format("ModMap failed: {0}", e.ToString()));
            }
            return partModMap;
        }
        public static OldJSONNode FindEvent(OldJSONNode eventToFind, OldJSONNode events)
        {
            OldJSONNode result = OldJSONNode.Parse("{}");
            //Debug.Log("FindEvent: eventToFind" + eventToFind.ToString() + "\nevents:" + events.ToString());
            for (int i = 0; i < events.AsArray.Count; i++)
            {
                var temp = events[i];
                //Debug.Log("Checking " + i + ": " + temp["missionTime"]);
                if (temp["missionTime"].AsDouble == eventToFind["missionTime"].AsDouble &&
                    temp["name"].ToString().CompareTo(eventToFind["name"].ToString()) == 0 &&
                    temp["description"].ToString().CompareTo(eventToFind["description"].ToString()) == 0)
                {
                    GFLogger.Instance.AddDebugLog("Found event! {0}", temp.ToString());
                    result = temp;
                }
            }

            return result;
        }

        public static OldJSONNode CreateJsonForVessel(ShipConstruct v)
        {
            OldJSONNode vesselJSON = OldJSONNode.Parse("{}");
            vesselJSON["username"] = SettingsManager.Instance.settings.username;
            vesselJSON["gfID"] = KSPUtils.GetPartsHashcode(v.parts).ToString();
            vesselJSON["mode"] = KSPUtils.ToMixedCaseString(HighLogic.fetch.currentGame.Mode.ToString());
            vesselJSON["file_version_major"].AsInt = HighLogic.fetch.currentGame.file_version_major;
            vesselJSON["file_version_minor"].AsInt = HighLogic.fetch.currentGame.file_version_minor;
            vesselJSON["file_version_revision"].AsInt = HighLogic.fetch.currentGame.file_version_revision;
            vesselJSON["flagURL"] = HighLogic.fetch.currentGame.flagURL.ToString();
            vesselJSON["gameTitle"] = HighLogic.fetch.currentGame.Title;
            KSPDateTime dt = new KSPDateTime(Planetarium.GetUniversalTime());
            vesselJSON["creationDate"].AsDouble = dt.UT;
            vesselJSON["creationDateStandard"] = dt.ToStringStandard(DateStringFormatsEnum.DateTimeFormat);

            // basic indic
            vesselJSON["name"] = KSPUtils.RemoveSpecialCharacters(v.shipName);
            vesselJSON["description"] = v.shipDescription;
            vesselJSON["building"] = v.shipFacility == EditorFacility.SPH ? "SPH" : "VAB";
            // sizes
            var size = ShipConstruction.CalculateCraftSize(v);
            vesselJSON["width"].AsFloat = size.x;
            vesselJSON["height"].AsFloat = size.y;
            vesselJSON["length"].AsFloat = size.z;

            // masses
            float dry, fuel;
            v.GetShipMass(out dry, out fuel);
            vesselJSON["dryMass"].AsFloat = dry;
            vesselJSON["fuelMass"].AsFloat = fuel;
            vesselJSON["mass"].AsFloat = v.GetTotalMass();

            // costs
            v.GetShipCosts(out dry, out fuel);
            vesselJSON["dryCost"].AsFloat = dry;
            vesselJSON["fuelCost"].AsFloat = fuel;
            vesselJSON["cost"].AsFloat = dry + fuel;

            // part stats
            vesselJSON["partCount"].AsInt = v.parts.Count;
            vesselJSON["strutCount"].AsInt = v.Sum(part => KSPUtils.IsStrut(part) ? 1 : 0);
            vesselJSON["crewCapacity"].AsInt = v.Sum(part => part.CrewCapacity);
            vesselJSON["srbCount"].AsInt = v.Sum(part => KSPUtils.IsSRB(part) ? 1 : 0);
            vesselJSON["solarPanelCount"].AsInt = v.Sum(part => KSPUtils.IsSolarPanel(part) ? 1 : 0);
            vesselJSON["engineCount"].AsInt = v.Sum(part => KSPUtils.IsEngine(part) ? 1 : 0);
            IEnumerable<Part> scienceParts = v.Where(p => (KSPUtils.IsScience(p) && !KSPUtils.IsCommandModule(p)));
            vesselJSON["scienceCount"].AsInt = scienceParts.Count();
            IEnumerable<Part> commsParts = v.Where(p => (KSPUtils.IsCommunications(p)));
            vesselJSON["commsCount"].AsInt = commsParts.Count();
            IEnumerable<Part> parachuteParts = v.Where(p => (KSPUtils.IsParachute(p)));
            vesselJSON["parachuteCount"].AsInt = parachuteParts.Count();

            PopulateVesselJsonWithSimulation(vesselJSON);
            PopulateVesselJsonWithMods(vesselJSON);

            //GFLogger.Instance.AddDebugLog("FINAL JSON:" + vesselJSON.ToString());

            return vesselJSON;
        }
        public static OldJSONNode CreateJsonForMission(string missionName, string missionDetails, string missionPurpose, Vessel v)
        {
            Part p = v.rootPart;

            OldJSONNode mission = OldJSONNode.Parse("{}");
            mission["name"] = missionName;
            mission["description"] = missionDetails;
            mission["purpose"] = missionPurpose;
            mission["gfID"] = KSPUtils.GetPartsHashcode(v.parts).ToString();
            mission["flightID"] = p.flightID.ToString();
            mission["missionID"] = p.missionID.ToString();
            mission["launchID"] = HighLogic.fetch.currentGame.launchID.ToString();
            mission["vesselID"] = v.id.ToString();

            mission["mode"] = KSPUtils.ToMixedCaseString(HighLogic.fetch.currentGame.Mode.ToString());
            mission["file_version_major"].AsInt = HighLogic.fetch.currentGame.file_version_major;
            mission["file_version_minor"].AsInt = HighLogic.fetch.currentGame.file_version_minor;
            mission["file_version_revision"].AsInt = HighLogic.fetch.currentGame.file_version_revision;
            mission["flagURL"] = HighLogic.fetch.currentGame.flagURL.ToString();
            mission["gameTitle"] = HighLogic.fetch.currentGame.Title;
            mission["launchCrew"] = GetCrewData(p.vessel);
            KSPDateTime dt = new KSPDateTime(Planetarium.GetUniversalTime());
            mission["launchDateTime"].AsDouble = dt.UT;
            mission["launchDateTimeStandard"] = dt.ToStringStandard(DateStringFormatsEnum.DateTimeFormat);

            mission["startTime"] = GetTimeNode(v);
            OldJSONNode situation = OldJSONNode.Parse("{}");
            situation["situation"] = Enum.GetName(typeof(Vessel.Situations), v.situation);
            situation["body"] = v.mainBody.bodyName;

            return mission;
        }

        private static OldJSONNode
            GetEvent(string eventName, string eventDescription, ProtoVessel pv, bool userEvent = false)
        {
            OldJSONNode eventNode = OldJSONNode.Parse("{}");
            eventNode["name"] = eventName;
            eventNode["description"] = eventDescription;
            eventNode["situation"] = pv.situation.ToString();
            GFLogger.Instance.AddDebugLog("EVENT: {0}, pv.met = {1}, pv.launch = {2}, duration = {3}", eventName, pv.missionTime, pv.launchTime, (Planetarium.fetch.time - pv.launchTime));
            eventNode["missionTime"].AsDouble = pv.missionTime;
            eventNode["universalTime"].AsDouble = Planetarium.GetUniversalTime();
            eventNode["missionTimeInDays"].AsDouble = KSPUtils.GameTimeInDays(pv.missionTime);
            eventNode["body"] = PSystemManager.Instance.localBodies[pv.orbitSnapShot.ReferenceBodyIndex].bodyName;
            eventNode["endMissionTime"].AsDouble = -1;
            eventNode["endUniversalTime"].AsDouble = -1;
            eventNode["endMissionTimeInDays"].AsDouble = -1;
            eventNode["userEvent"].AsBool = userEvent;
            return eventNode;
        }
        public static OldJSONNode
            GetEvent(string eventName, string eventDescription, Vessel vessel, bool userEvent = false)
        {
            OldJSONNode eventNode = GetEvent(eventName, eventDescription, vessel.protoVessel, userEvent);
            eventNode["startTime"] = GetTimeNode(vessel);
            eventNode["missionTime"].AsDouble = vessel.missionTime;
            eventNode["situation"] = vessel.situation.ToString();
            eventNode["body"] = vessel.mainBody.bodyName;
            eventNode["biome"] = KSPUtils.GetBiomeName(vessel);

            OldJSONNode stats = OldJSONNode.Parse("{}");
            KSPUtils.ApplyFlightLoggerStats(stats);
            GFLogger.Instance.AddDebugLog("Flight stats");
            GFLogger.Instance.AddDebugLog(stats.ToString());
            eventNode["stats"] = stats;
            eventNode["stats"]["throttle"].AsDouble = vessel.ctrlState.mainThrottle;
            eventNode["stats"]["period"].AsDouble = vessel.orbit.period;
            eventNode["stats"]["x"].AsDouble = vessel.transform.position.x;
            eventNode["stats"]["y"].AsDouble = vessel.transform.position.y;
            eventNode["stats"]["z"].AsDouble = vessel.transform.position.z;
            eventNode["stats"]["latitude"].AsDouble = vessel.latitude;
            eventNode["stats"]["longitude"].AsDouble = vessel.longitude;
            eventNode["stats"]["stage"].AsDouble = vessel.currentStage;
            eventNode["stats"]["acceleration"].AsDouble = vessel.acceleration.magnitude;
            eventNode["stats"]["orbit"]["epoch"].AsDouble = vessel.orbit.epoch;
            eventNode["stats"]["orbit"]["meanAnomalyAtEpoch"].AsDouble = vessel.orbit.meanAnomalyAtEpoch;
            eventNode["stats"]["orbit"]["apoapsis"].AsDouble = vessel.orbit.ApA;
            eventNode["stats"]["orbit"]["periapsis"].AsDouble = vessel.orbit.PeA;
            eventNode["stats"]["orbit"]["period"].AsDouble = vessel.orbit.period;
            eventNode["stats"]["orbit"]["x"].AsDouble = vessel.orbit.pos.x;
            eventNode["stats"]["orbit"]["y"].AsDouble = vessel.orbit.pos.y;
            eventNode["stats"]["orbit"]["z"].AsDouble = vessel.orbit.pos.z;

            GFLogger.Instance.AddDebugLog(eventNode.ToString());
            return eventNode;
        }

        private static void PopulateVesselJsonWithMods(OldJSONNode vesselJSON)
        {
            try
            {
                var modmap = GetModDictionary();
                GFLogger.Instance.AddDebugLog(modmap.Count + " modmap length");
                vesselJSON["mods"] = OldJSONArray.Parse("[]");
                List<string> tempList = new List<string>();
                foreach (Part p in EditorLogic.fetch.ship.parts)
                {
                    string modName = "";
                    modmap.TryGetValue(p.partInfo.name.Replace(".", "_"), out modName);
                    if (modName.Equals("Squad") || modName.Equals("NASAmission"))
                    {
                        continue;
                    }
                    if (!tempList.Contains(modName))
                    {
                        tempList.Add(modName);
                        vesselJSON["mods"].AsArray.Add(modName);
                    }
                }
                GFLogger.Instance.AddDebugLog("MODS: " + vesselJSON["mods"].AsArray.ToString());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Exception: " + e.Message);
                UnityEngine.Debug.Log("StackTrace: " + e.StackTrace);
            }

        }
        private static void PopulateVesselJsonWithSimulation(OldJSONNode vesselJSON)
        {

            // Delta-V calculations via Kerbal Engineer's simulator
            SimManager.Gravity = CelestialBodies.GetBodyInfo("Kerbin").Gravity;
            SimManager.Atmosphere = 1.0f;
            SimManager.RequestSimulation();
            SimManager.TryStartSimulation();
            while (!SimManager.ResultsReady())
            {
                GFLogger.Instance.AddDebugLog("Sim running");
            }
            GFLogger.Instance.AddDebugLog("Sim done");
            GFLogger.Instance.AddDebugLog(String.Format("Auto total  : {0:0.0}s, {1:0.0}, {2:0.0}", SimManager.LastStage.totalTime, SimManager.LastStage.inverseTotalDeltaV, SimManager.LastStage.totalDeltaV));

            vesselJSON["stages"] = OldJSONArray.Parse("[]");
            foreach (Stage s in SimManager.Stages)
            {
                vesselJSON["stages"].AsArray.Add(s.number.ToString(), GetVesselStageJson(s));
            }
            foreach (OldJSONNode stageJson in vesselJSON["stages"].AsArray)
            {
                stageJson["engineCount"].AsInt =
                    EditorLogic.fetch.ship.Sum(
                    part => part.inverseStage == stageJson["stageNum"].AsInt
                        && KSPUtils.IsEngine(part) ? 1 : 0);
            }
            vesselJSON["deltav"].AsDouble = SimManager.LastStage.totalDeltaV;
            vesselJSON["deltaTime"].AsDouble = SimManager.LastStage.totalTime;
            vesselJSON["twr"].AsDouble = SimManager.LastStage.actualThrustToWeight;

            SimManager.Atmosphere = 0.0f;
            SimManager.RequestSimulation();
            SimManager.TryStartSimulation(true);

            while (!SimManager.ResultsReady())
            {
                //GFLogger.Instance.AddDebugLog("Sim running2");
            }
            //GFLogger.Instance.AddDebugLog("Sim done2");

            vesselJSON["stagesVac"] = OldJSONArray.Parse("[]");
            foreach (Stage s in SimManager.Stages)
            {
                vesselJSON["stagesVac"].AsArray.Add(s.number.ToString(), GetVesselStageJson(s));
            }
            vesselJSON["deltavVac"].AsDouble = SimManager.LastStage.totalDeltaV;
            vesselJSON["timeVac"].AsDouble = SimManager.LastStage.totalTime;
            vesselJSON["twrVac"].AsDouble = SimManager.LastStage.actualThrustToWeight;
        }

        private static OldJSONNode GetTimeNode(Vessel vessel)
        {
            KSPDateTime dt = new KSPDateTime(Planetarium.GetUniversalTime());
            OldJSONNode timeNode = OldJSONNode.Parse("{}");
            timeNode["year"].AsInt = dt.Year;
            timeNode["month"].AsInt = dt.Month;
            timeNode["day"].AsInt = dt.DayOfYear;
            timeNode["hour"].AsInt = dt.Hour;
            timeNode["min"].AsInt = dt.Minute;
            timeNode["sec"].AsInt = dt.Second;
            timeNode["ms"].AsInt = dt.Millisecond;
            return timeNode;
        }
        private static OldJSONNode GetCrewData(Vessel v)
        {
            OldJSONArray crewJson = OldJSONNode.Parse("[]").AsArray;
            List<ProtoCrewMember> crew = v.GetVesselCrew();

            foreach (ProtoCrewMember item in crew)
            {
                crewJson.Add(item.name);
            }
            return crewJson;
        }

        private static OldJSONNode GetVesselStageJson(Stage stage)
        {
            OldJSONNode n = OldJSONNode.Parse(SimpleJson.SerializeObject(stage));

            n["stageNum"].AsInt = n["number"].AsInt;
            n["engineCount"].AsInt = EditorLogic.fetch.ship.Sum(
                part => part.inverseStage == n["stageNum"].AsInt
                && KSPUtils.IsEngine(part) ? 1 : 0);
            
            //UnityEngine.Debug.Log(n.ToString());
            return n;
        }
    }
}
