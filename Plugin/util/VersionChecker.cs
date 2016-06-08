using OldSimpleJSON;
using System;
using System.IO;
using System.Net;

/** BB: Move logic to server **/

namespace Gameframer
{
    public class VersionChecker : Singleton<VersionChecker>
    {
        public static string VERSION = "0.6.2";

        public bool versionChecked { get; private set; }
        public bool versionOk { get; private set; }

        public static float GetMajorMinorVersion()
        {
            float majorMinor = 0.0f;

            string[] versions = VERSION.Split('.');
            if (versions.Length > 1)
            {
                majorMinor = float.Parse(versions[0] + "." + versions[1]);
            }
            else
            {
                majorMinor = float.Parse(VERSION);
            }

            return majorMinor;
        }

        protected VersionChecker()
        {
            versionChecked = false;
            versionOk = false;
        }
        public void Save()
        {
        }
        void Awake()
        {
            IsVersionOkay();
        }

        public bool IsVersionOkay()
        {
            int tryCount = 0;
            int maxTries = 10;
            bool isOkay = false;
            string all = "";

            if (versionChecked)
            {
                return versionOk;
            }

            if (GFLogger.PRINT_DEBUG_INFO)
            {
                GFLogger.Instance.AddDebugLog("IsVersionOkay?");
            } 
            while (tryCount++ < maxTries && !isOkay)
            {
                try
                {
                    string url = "http://download.gameframer.com/whitelist.json";
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                    wr.Method = "GET";
                    Stream objStream = wr.GetResponse().GetResponseStream();
                    StreamReader objReader = new StreamReader(objStream);

                    string sLine = "";
                    int i = 0;
                    while (sLine != null)
                    {
                        i++;
                        sLine = objReader.ReadLine();
                        if (sLine != null)
                        {
                            all += sLine;
                        }
                    }
                    OldJSONNode n = JSONData.Parse(all);
                    if (GFLogger.PRINT_DEBUG_INFO)
                    {
                        GFLogger.Instance.AddDebugLog("WHITELIST: " + n.ToString());
                    }
                    OldJSONArray whiteList = n["white"].AsArray;
                    float minGreen = 0.0f;
                    float maxGreen = 0.0f;
                    for (int j = 0; j < whiteList.Count; j++)
                    {
                        if (GFLogger.PRINT_DEBUG_INFO)
                        {
                            GFLogger.Instance.AddDebugLog(VersionChecker.VERSION + " ?= " + whiteList[j]);
                        }
                        var temp = float.Parse(whiteList[j]);
                        maxGreen = Math.Max(maxGreen, temp);
                        minGreen = Math.Min(minGreen, temp);
                    }
                    if (VersionChecker.GetMajorMinorVersion() >= minGreen && VersionChecker.GetMajorMinorVersion() <= maxGreen)
                    {
                        isOkay = true;
                        versionOk = true;
                    } 
                    versionChecked = true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("Exception in CheckVersion() " + e.ToString());
                }
                finally
                {
                    UnityEngine.Debug.Log("CheckVersion try count " + tryCount + " of " + maxTries);
                }
            }

            GFLogger.Instance.AddDebugLog("GF Version: " + VersionChecker.VERSION + (isOkay ? " is good." : " needs an update."));

            return isOkay;
        }
    }
}

