using KSPPluginFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframer
{
    public class NameRegisterWorker : MonoBehaviourExtended
    {
        string URL = GameframerService.HOSTNAME;
        string username;

        public static NameRegisterWorker CreateComponent(GameObject where, string username)
        {
            NameRegisterWorker w = where.AddComponent<NameRegisterWorker>();
            w.username = username;
            return w;
        }

        override internal void Start()
        {
            var form = new WWWForm();
            form.AddField("username", username);
            LogFormatted_DebugOnly("Name Register start");
            URL += "/users/" + username;
            var www = new WWW(URL, form.data, KSPUtils.GetNameAuthHeaders());
            LogFormatted_DebugOnly("Name Register starting ({0})", URL);
            StartCoroutine(WaitForRequest(www));
        }

        IEnumerator<object> WaitForRequest(WWW www)
        {
            yield return www;

            // check for errors
            if (www.error == null)
            {
                WelcomeUI ui = FindObjectOfType<WelcomeUI>();

                //LogFormatted_DebugOnly("WWW Ok!: {0} ", www.text);
                OldSimpleJSON.OldJSONNode n = OldSimpleJSON.OldJSONArray.Parse(www.text);

                LogFormatted_DebugOnly("Register results: {0}", n.ToString());
                if (n != null)
                {
                    //KARSettings settings = new KARSettings(KARSettings.LOCATION);
                    //settings.Load();
                    //settings.username = n["username"];
                    //settings.apiKey = n["key"];
                    //settings.Save();
                    SettingsManager.Instance.Reload();
                    SettingsManager.Instance.settings.username = n["username"];
                    SettingsManager.Instance.settings.apiKey = n["key"];
                    SettingsManager.Instance.Save();
                    LogFormatted_DebugOnly("Registered name successfully. [{0}] [{1}]", n["name"], n["key"]);
                    ui.RegisteredOkay();
                }
                else
                {
                    LogFormatted("Error trying to register name. {0}", www.text);
                }
            }
            else
            {
                LogFormatted_DebugOnly("WWW Error: {0}\n{1}", www.error, www.url);
            }

            Destroy(this);
        }
    }
}
