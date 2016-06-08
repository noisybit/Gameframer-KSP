using KSPPluginFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframer
{
    public class PotentialNameWorker : MonoBehaviourExtended
    {
        string URL = GameframerService.HOSTNAME;
        public static PotentialNameWorker CreateComponent(GameObject where)
        {
            GFLogger.Instance.AddDebugLog("PotentialNameGetter creation");
            PotentialNameWorker w = where.AddComponent<PotentialNameWorker>();
            return w;
        }

        override internal void Start()
        {
            GFLogger.Instance.AddDebugLog("PotentialNameGetter start");
            URL += "/potential-names/8";
            var www = new WWW(URL, null, KSPUtils.GetNameAuthHeaders());
            GFLogger.Instance.AddDebugLog("Uploader starting ({0})", URL);
            StartCoroutine(WaitForRequest(www));
        }

        IEnumerator<object> WaitForRequest(WWW www)
        {
            yield return www;

            // check for errors
            if (www.error == null)
            {
                WelcomeUI ui = FindObjectOfType<WelcomeUI>();
                ui.names = new List<string>();

                LogFormatted_DebugOnly("WWW Ok!: {0} ", www.text);
                OldSimpleJSON.OldJSONNode n = OldSimpleJSON.OldJSONArray.Parse(www.text);
                OldSimpleJSON.OldJSONArray a = n.AsArray;
                for (int i = 0; i < a.Count; i++)
                {
                    ui.names.Add(a[i]);
                }

            }
            else
            {
                LogFormatted("WWW Error: {0}\n{1}", www.error, www.url);
            }

            Destroy(this);
        }
    }
}
