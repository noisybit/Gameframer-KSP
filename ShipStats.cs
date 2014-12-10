using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameframer
{
    public class KAMRShip
    {
        //        public string _id { get; set; }
        public string name { get; set; }
        public string KSP_VERSION { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string craft { get; set; }
        public string shipname { get; set; }
        public string username { get; set; }
        public DateTime createdOn { get; set; }
        public string building { get; set; }
        public StageStats stats { get; set; }
        //public StageStats[] stages { get; set; }
        public GFPartInfo[] allParts { get; set; }

        public KAMRShip(string username, string name, string description, string imageFilename, string craftFilename, DateTime createdOn, List<Part> parts)
        {
            this.username = username;
            this.name = this.shipname = name;
            this.description = description;
            this.image = imageFilename;
            this.craft = craftFilename;
            this.createdOn = createdOn;
            this.building = HighLogic.LoadedScene == GameScenes.SPH ? "SPH" : "VAB";
            List<GFPartInfo> _allParts = new List<GFPartInfo>();
            parts.ForEach(part => _allParts.Add(new GFPartInfo(part)));
            int stageCount = parts.Max(part => part.inverseStage);
            this.allParts = _allParts.ToArray();
            this.stats = new StageStats(parts);
            /*stages = new StageStats[stageCount];
            for (int i = 0; i < stageCount; i++)
            {
                Debug.Log(String.Format("Building part list for stage {0}", i));
                var stageParts = parts.Where(part => (part.inverseStage == i));
                Debug.Log(String.Format("\tFound {0} parts", stageParts.Count()));
                stages[i] = new StageStats(stageParts.ToList<Part>(), i);
                Debug.Log(String.Format("\tparts = {0}", SimpleJson.SerializeObject(stages[i])));
                Debug.Log(String.Format("Building part list for stage {0}", i));
            }*/
        }
    }

    public class GFPartInfo
    {
        public int stage { get; set; }
        public string author { get; set; }
        public PartCategories category { get; set; }
        public float cost { get; set; }
        public string description { get; set; }
        public string manufacturer { get; set; }
        public string partPath { get; set; }
        public string title { get; set; }
        public string TechRequired { get; set; }
        public string typeDescription { get; set; }

        public GFPartInfo(Part p)
        {
            this.stage = p.inverseStage;
            this.author = p.partInfo.author;
            this.category = p.partInfo.category;
            this.cost = p.partInfo.cost;
            this.description = p.partInfo.description;
            this.manufacturer = p.partInfo.manufacturer;
            this.partPath = p.partInfo.partPath;
            this.title = p.partInfo.title;
            this.TechRequired = p.partInfo.TechRequired;
            this.typeDescription = p.partInfo.typeDescription;
        }
    }

    public class StageStats
    {
        public float mass { get; set; }
        public float cost { get; set; }
        public float deltaV { get; set; }
        public float deltaTime { get; set; }
        public int stageNum { get; set; }
        public int partCount { get; set; }
        public int strutCount { get; set; }
        public int crewCapacity { get; set; }
        public int srbCount { get; set; }
        public int engineCount { get; set; }
        public int scienceCount { get; set; }

        public StageStats()
        {

        }
        public StageStats(List<Part> parts, int stageNum = -1)
        {
            this.stageNum = stageNum;
            this.mass = parts.Sum(part => GetPartMass(part));
            this.cost = parts.Sum(part => part.partInfo.cost);
            this.partCount = parts.Count();
            this.strutCount = parts.Count(part => part is StrutConnector);
            this.crewCapacity = parts.Sum(part => part.CrewCapacity);
            this.srbCount = parts.Sum(part => IsSRB(part) ? 1 : 0);
            this.engineCount = parts.Sum(part => IsEngine(part) ? 1 : 0);
            IEnumerable<Part> scienceParts = parts.Where(p => (IsScience(p) && !IsCommandModule(p)));
            this.scienceCount = scienceParts.Count();
        }

        internal bool IsScience(Part p)
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
        internal bool IsCommandModule(Part p)
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
        internal float GetPartMass(Part p)
        {
            return p.mass + p.GetResourceMass();
        }
        internal bool IsSRB(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m is ModuleEngines)
                {
                    //UnityEngine.Debug.Log("KAR, IsSRB found engine...");
                    ModuleEngines engine = m as ModuleEngines;

                    //UnityEngine.Debug.Log("Propellant: " + engine.propellants.ToString());
                    //UnityEngine.Debug.Log("Can you shut it down? " + (engine.allowShutdown ? "yes" : "no"));
                    //UnityEngine.Debug.Log("Can you change its throttle? " + (engine.throttleLocked ? "yes" : "no"));

                    if (!engine.allowShutdown)// && engine.throttleLocked)
                        return true;
                }
            }

            return false;
        }

        internal bool IsEngine(Part p)
        {
            foreach (PartModule m in p.Modules)
            {
                if ((m is ModuleEngines) || (m is ModuleEnginesFX))
                {
                    //UnityEngine.Debug.Log("KAR, Engine: " + m.ToString());
                    return true;
                }
            }

            return false;
        }
    }
}
