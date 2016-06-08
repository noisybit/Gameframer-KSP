using KerbalEngineer;
using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    class KSPUtils : MonoBehaviourExtended
    {
        #region JSON stuff
        public static OldJSONNode GetStatusPatch(ProtoVessel pv)
        {
            OldJSONNode patch = OldJSONNode.Parse("{}");
            OldJSONNode sitPatch = OldJSONNode.Parse("{}");
            sitPatch["missionTimeInDays"].AsDouble = GameTimeInDays(pv.missionTime);// ve.missionTime;
            sitPatch["missionTime"].AsDouble = pv.missionTime;// ve.missionTime;
            sitPatch["situation"] = pv.situation.ToString();
            sitPatch["body"] = PSystemManager.Instance.localBodies[pv.orbitSnapShot.ReferenceBodyIndex].bodyName;
            sitPatch["vesselName"] = pv.vesselName;
            ApplyFlightLoggerStats(sitPatch);
            patch["stats"] = sitPatch;

            //LogFormatted("Status patch generated: {0}", patch.ToString());

            return patch;
        }

        public static void ApplyFlightLoggerStats(OldJSONNode n)
        {
            if (!HighLogic.LoadedSceneIsFlight)
            { return; }

            string missionStats = FlightLogger.getMissionStats();
            //LogFormatted("FlightLoggerStats == {0}", missionStats);
            if (missionStats == null || missionStats.Length == 0)
            {
                LogFormatted("No missionStats");
                return;
            }

            try
            {
                string[] events = missionStats.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < events.Length; i += 2)
                {
                    var name = events[i].Trim().Replace(":", "");
                    var value = events[i + 1].Trim().Replace(".", "");
                    //LogFormatted("{0}:{1}", name, value);
                    if (name == "Total Mission Time")
                    {
                        n["totalMissionTime"] = value;
                    }
                    if (name == "Highest Altitude Achieved" && FlightGlobals.currentMainBody == CelestialBodies.GetBodyInfo("Kerbin").CelestialBody)
                    {
                        n["highestAltitude"] = (value.Replace("m", "").Replace(",", ""));
                    }
                    if (name == "Highest Speed Achieved")
                    {
                        n["highestSpeed"] = (value.Replace("m/s", "").Replace(",", ""));
                    }
                    if (name == "Highest Speed Over Land")
                    {
                        n["highestLandSpeed"] = (value.Replace("m/s", "").Replace(",", ""));
                    }
                    if (name == "Ground Distance Covered")
                    {
                        n["groundDistanceTraveled"] = (value.Replace("m", "").Replace(",", ""));
                    }
                    if (name == "Total Distance Traveled")
                    {
                        n["totalDistanceTraveled"] = (value.Replace("m", "").Replace(",", ""));
                    }
                    if (name == "Most Gee Force Endured")
                    {
                        n["maxGees"].AsFloat = float.Parse(value.Replace("G", "").Replace(",", ""));
                    }
                }
            }
            catch (Exception e)
            {
                LogFormatted("Exception parsing stats: {0}", e.Message);
            }
        }

        #endregion

        #region WWW authentication helpers
        public static string GetAuthHeader()
        {
            KARSettings settings = new KARSettings(KARSettings.LOCATION);
            settings.Load();
            string _authString = string.Format("{0}:{1}", settings.username, settings.apiKey);
            string _authHeader = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_authString));

            return _authHeader;
        }

        public static Dictionary<string, string> GetAuthHeaderDictionary()
        {
            var headers = new Dictionary<string, string>();
            headers["Authorization"] = GetAuthHeader();
            return headers;
        }

        public static string GetNameAuthHeaderString()
        {
            string _auth = string.Format("{0}:{1}", "UserAdmin", Credentials.API_KEY);
            string _authString = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_auth));
            return _authString;
        }

        public static Dictionary<string, string> GetNameAuthHeaders()
        {
            var headers = new Dictionary<string, string>();
            string _auth = string.Format("{0}:{1}", "UserAdmin", Credentials.API_KEY);
            headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_auth));
            return headers;
        }
        #endregion

        #region Part Utils
        public static int GetPartsHashcode(List<Part> parts)
        {
            Part[] p2 = parts.ToArray<Part>();
            Array.Sort(p2, delegate(Part x, Part y)
            {
                return x.partInfo.name.CompareTo(y.partInfo.name);
            });
            //StringBuilder b = new StringBuilder();
            String s = "";
            foreach (Part p in p2)
            {
                //LogFormatted("HASH {0}", p.partInfo.name);
                //b.Append(p.partInfo.name);
                s += p.partInfo.name;
            }

            //LogFormatted("HASH1 {0}", s);
            int HashCode = s.GetHashCode();
            //Console.WriteLine("HASH 0x{0:X8}\t\"{1}\"", HashCode, s);
            //LogFormatted("HASH2 {0}", HashCode);
            return HashCode;
        }
        public static bool IsScience(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if ((m is ModuleScienceContainer) || (m is ModuleScienceExperiment) || (m is ModuleScienceLab))
                {
                    return true;
                }
            }
            return false;
        }
        /*public static bool IsSepratron(Part p)
        {
            return IsSRB(p) && p.ActivatesEvenIfDisconnected;// && IsDecoupledInStage(p, p.inverseStage);
        }*/
        public static bool IsSolarPanel(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleDeployableSolarPanel)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsStrut(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m.moduleName.Contains("ModuleStrut"))
                {
                    return true;
                }

            }
            return false;
        }
        public static bool IsSRB(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleEngines)
                {
                    return (m as ModuleEngines).throttleLocked;
                }

                if (m is ModuleEnginesFX)
                {
                    return (m as ModuleEnginesFX).throttleLocked;
                }
            }
            return false;
        }
        public static bool IsEngine(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if ((m is ModuleEngines) || (m is ModuleEnginesFX) || (m is MultiModeEngine))
                {
                    //LogFormatted("isEngine test: {0} ({1})", (m as ModuleEngines).GetEngineType(), (m as ModuleEngines).GetInfo());
                    return !IsSRB(p);
                }
            }
            return false;
        }
        public static bool IsCommunications(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleDataTransmitter)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsParachute(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleParachute)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsCommandModule(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleCommand)
                {
                    return true;
                }
            }
            return false;
        }
        public static float GetPartMass(Part p)
        {
            return p.mass + p.GetResourceMass();
        }
        #endregion

        #region Misc, non-KSP
        public static string ToMixedCaseString(string s)
        {
            s.Replace("_", " ");

            if (s.Length < 2)
            {
                return s;
            }

            return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
        }
        public static long Now()
        {
            return DateTime.Now.ToFileTimeUtc();
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion
         
        #region Misc, KSP related
        public static bool VesselOK(Vessel check)
        {
            return (check != null &&
                check.isActiveVessel &&
                check.rootPart != null &&
                !check.isEVA &&
                check.isCommandable);
        }

        public static bool IsPermadeathEnabled()
        {
            if (HighLogic.CurrentGame == null) return false;
            return !HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn;
        }
        public static bool IsKerbinTimeEnabled()
        {
            return GameSettings.KERBIN_TIME;
        }
        public static double GameTimeInDays(double time)
        {
            if (IsKerbinTimeEnabled())
            {
                return time / 6 / 60 / 60;
            }
            else
            {
                return time / 24 / 60 / 60;
            }
        }
        public static CelestialBody GetCelestialBody(String name)
        {
            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                if (body.GetName().Equals(name)) return body;
            }
            return null;
        }
        public static CelestialBody GetSun()
        {
            return GetCelestialBody("Sun");
        }
        public static bool IsSun(CelestialBody body)
        {
            return body.RevealType().Equals("Sun");
        }
        public static double GetDistanceToSun(Vessel vessel)
        {
            if (vessel == null) return 0.0;
            CelestialBody sun = GetSun();
            if (sun == null) return 0.0;
            Vector3d posVessel = vessel.GetWorldPos3D();
            Vector3d posSun = sun.GetWorldSurfacePosition(0.0, 0.0, 0.0);
            return Vector3d.Distance(posVessel, posSun);
        }
        public static string GetBiomeName(Vessel v)
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return "";
            }
            if (FlightGlobals.currentMainBody.BiomeMap != null)
            {
                return FlightGlobals.currentMainBody.BiomeMap.GetAtt(v.latitude * Mathf.Deg2Rad, v.longitude * Mathf.Deg2Rad).name;
            }

            return "";
        }
        public static void AdjustAmbientLight(float minAmbient)
        {
            Color ambient = RenderSettings.ambientLight;
            float minValue = 1.0f;
            minValue = (ambient.r < minAmbient && ambient.r < minValue) ? ambient.r : minValue;
            minValue = (ambient.g < minAmbient && ambient.g < minValue) ? ambient.g : minValue;
            minValue = (ambient.b < minAmbient && ambient.b < minValue) ? ambient.b : minValue;
            if (minValue < 1.0f)
            {
                minValue = minAmbient - minValue;
                ambient.r = ambient.r + minValue;
                ambient.g = ambient.g + minValue;
                ambient.b = ambient.b + minValue;
                RenderSettings.ambientLight = ambient;
            }
        }
        public static string GetEventReportString(EventReport r)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendLine("\n---- EVENT REPORT ----");
                sb.AppendLine(String.Format("msg: {0}", r.msg));
                sb.AppendLine(String.Format("other: {0}", r.other));
                sb.AppendLine(String.Format("sender: {0}", r.sender));
                sb.AppendLine(String.Format("stage: {0}", r.stage));
                sb.AppendLine(String.Format("origin: {0}", r.origin.partName));
                if (r.origin != null)
                {
                    if (r.origin.vessel != null && r.origin.vessel.protoVessel != null)
                    {
                        sb.AppendLine(String.Format("vessel: {0}", r.origin.vessel.vesselName));
                        //sb.AppendLine(String.Format("mission id: {0}", r.origin.vessel.orbit.altitude));
                    }
                }
                sb.AppendLine("----------------------");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("EXCEPTION in " + System.Reflection.MethodBase.GetCurrentMethod().Name + "\n" + e.StackTrace);
            }

            return sb.ToString();
        }
        private void ListAllMissions()
        {
            LogFormatted("--LISTING ALL MISSIONS--");
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                var missionID = pv.protoPartSnapshots[pv.rootIndex].missionID;
                var flightID = pv.protoPartSnapshots[pv.rootIndex].flightID;

                LogFormatted("{2}-{3}\t{0} ({1})", pv.vesselName, pv.situation, missionID, flightID);
            }
            LogFormatted("--DONE LISTING ALL MISSIONS--");
        }
        /// <summary>
        /// Generates a thumbnail exactly like the one KSP generates automatically.
        /// Behaves exactly like ShipConstruction.CaptureThumbnail() but allows customizing the resolution.
        /// </summary>
        public static void CaptureThumbnail(ShipConstruct ship, int resolution, string saveFolder, string craftName)
        {
            if (ship.shipFacility != EditorFacility.VAB)
            {
                CraftThumbnail.TakeSnaphot(ship, resolution, saveFolder, craftName, 35, 135, 35, 135, 0.9f);
            }
            else
            {
                CraftThumbnail.TakeSnaphot(ship, resolution, saveFolder, craftName, 45, 45, 45, 45, 0.9f);
            }
        }
        /// <summary>
        /// Builds the path to the auto-generated thumbnail for the given ship.
        /// </summary>
        public static string GetCraftThumbnailPath(ShipConstruct ship)
        {
            return string.Format("thumbs/{0}_{1}_{2}.png", HighLogic.SaveFolder, ShipConstruction.GetShipsSubfolderFor(ship.shipFacility), ship.shipName);
        }
        #endregion
    }  
}