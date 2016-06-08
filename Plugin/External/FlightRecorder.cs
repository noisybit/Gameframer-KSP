//Author  : Géraud Benazet
//License : CC BY SA 3.0 http://creativecommons.org/licenses/by-sa/3.0/deed.en_GB

 
using System;
using System.Collections.Generic;
using System.Text;

namespace Gameframer
{
    public class FlightRecorder
    {
        public static double DEFAULT_CHECK_INTERVAL = 3.0;
        public double CHECK_INTERVAL = DEFAULT_CHECK_INTERVAL;

        Vessel _vessel;
        String header;
        String filename;
        double entryTime;
        Dictionary<String, String> dataFields = new Dictionary<String, String>();
        public StringBuilder sb;


        public FlightRecorder(Vessel v) {
            vessel = v;
            Reset();
        }

        public Vessel vessel
        {
            get { return _vessel; }
            set
            {
                if (value == _vessel)
                {
                    return;
                }

                if (header == null)
                {
                    OnStart();
                }

                filename = value.id.ToString() + ".csv";

                if (!KSP.IO.File.Exists<FlightRecorder>(filename))
                {
                    Reset();
                }

                _vessel = value;
                //vesselState = new VesselState();
            }
        }
        //VesselState _vesselState;
        /*public VesselState vesselState
        {
            get { return _vesselState; }
            set
            {
                _vesselState = value;
                if (_vesselState != null)
                {
                    _vesselState.Update(vessel);
                }
            }
        }*/

        public void Reset()
        {
            sb = new StringBuilder(header + Environment.NewLine);
            //KSP.IO.File.WriteAllText<MissionController>(header + Environment.NewLine, filename, null);
        }

        public void OnStart()
        {
            if (!HighLogic.LoadedSceneIsEditor)
            {
                header = @"GeeForce";

                String[] config = KSP.IO.File.ReadAllLines<FlightRecorder>("FlightRecorder.cfg", null);
                String configline;
                String dataHeader;
                String dataMemberName;
                String dataModuleName;
                int indexofModule;
                int indexofMember;

                foreach (String line in config)
                {
                    if (line.IndexOf(@"//") == -1)
                        configline = line;
                    else
                        configline = line.Substring(0, line.IndexOf(@"//"));

                    // trim whitespaces, tabulations and parenthesis
                    configline = configline.Replace(" ", "").Replace("\u0009", "").Replace("(", "").Replace(")", "");

                    if (configline.IndexOf(@":") == -1) continue; // exit if incorrect line

                    try // Because users have imagination...
                    {
                        dataHeader = configline.Substring(0, configline.IndexOf(@":"));
                        dataMemberName = configline.Substring(configline.IndexOf(@":") + 1);

                        indexofModule = dataMemberName.IndexOf('<');
                        indexofMember = dataMemberName.IndexOf(">.");

                        if (indexofModule != -1 && indexofMember != -1)
                        {
                            dataModuleName = dataMemberName.Substring(indexofModule + 1, indexofMember - indexofModule - 1);
                            dataMemberName = dataMemberName.Substring(indexofMember + 2);
                        }
                        else
                        {
                            dataModuleName = "";
                        }

                        //print(dataHeader + " : <" + dataModuleName + ">" + dataMemberName);

                        header += "," + dataHeader;
                        dataFields.Add(dataMemberName, dataModuleName);

                    }
                    catch
                    {
                    }
                }
            }

            sb = new StringBuilder();
        }


        private Object getMember(Object o, String m)
        {
            // Is it a Member ?
            foreach (System.Reflection.FieldInfo i in o.GetType().GetFields())
            {
                if (i.Name == m)
                    return o.GetType().GetField(m).GetValue(o);
            }
            // Is it a Property ?
            foreach (System.Reflection.PropertyInfo i in o.GetType().GetProperties())
            {
                if (i.Name == m)
                    return o.GetType().GetProperty(m).GetValue(o, null);
            }
            // Is it a Method ?
            foreach (System.Reflection.MethodInfo i in o.GetType().GetMethods())
            {
                if (i.Name == m)
                    return o.GetType().GetMethod(m).Invoke(o, null);
            }
            return null;
        }
        private Object getValue(Object parent, String value)
        {
            Object o;
            String m;
            int period = value.IndexOf(@".");

            //Parsing value name
            if (period == -1)
                m = value;
            else
                m = value.Substring(0, period);

            //Serializing object
            o = getMember(parent, m);

            //print(@"Created " + o.GetType() + " " + o);


            //Recursing
            if (period == -1)
                return o;
            else
                return getValue(o, value.Substring(period + 1));

        }

        public void UpdateData()
        {
            //KSP.IO.File.AppendAllText<FlightRecorder>("OnUpdate()" + Environment.NewLine, filename, null);

            if (vessel == null)
                return;

            // throttle recording by CHECK_INTERVAL
            if ((vessel.missionTime > entryTime + CHECK_INTERVAL) && (vessel.situation != Vessel.Situations.PRELAUNCH))
            {
                //vesselState.Update(vessel);

                String data;

                data = FlightGlobals.getGeeForceAtPosition(vessel.transform.position).magnitude.ToString();
                sb.Append(data);

                foreach (KeyValuePair<String, String> dataField in dataFields)
                {

                    if (dataField.Value == "")
                    {
                        //print("single value");
                        data += "," + getValue(this, dataField.Key).ToString();
                        sb.Append("," + getValue(this, dataField.Key).ToString());
                    }
                    else
                    {
                        //print("sum");
                        float v = 0;

                        foreach (Part p in this.vessel.Parts)
                        {
                            foreach (PartModule e in p.Modules)
                            {
                                if (e.GetType().ToString() == dataField.Value)
                                    v += (float)getValue(e, dataField.Key);
                            }
                        }
                        data += "," + v.ToString();
                        sb.Append("," + v.ToString());
                    }
                }

                sb.Append(Environment.NewLine);
                entryTime = vessel.missionTime;
            }
        }
    }
}
