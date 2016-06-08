using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class GFWorker : MonoBehaviourExtended
    {
        protected string URL = GameframerService.HOSTNAME;
        protected string method;
        protected List<PostData> data;
        protected string responseLocation;
        private DateTime requestStartTime;
        private DateTime requestEndTime;
        public FinishedDelegate OnDone
        {
            set { successCallback = value; }
        }
        public FinishedDelegate OnFail
        {
            set { failureCallback = value; }
        }
        protected FinishedDelegate successCallback;
        protected FinishedDelegate failureCallback;

        internal override void Start()
        {
            DoRequest();
        }
        internal override void OnDestroy()
        {
            if (data != null)
            {
                data.Clear();
                data = null;
            }
            method = null;
            URL = null;
            successCallback = null;
            failureCallback = null;
        }

        protected void initialize(string urlAction, List<PostData> data, string method)
        {
            this.URL += urlAction;
            this.method = method;
            this.data = data;
        }
        protected void DoRequest()
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}: {2} {3}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, method, URL));

            WWWClient client = new WWWClient(this, URL);
            client.AddHeader("Authorization", KSPUtils.GetAuthHeader());
            client.AddHeader("X-HTTP-Method-Override", this.method);
            int dataTally = 0;
            if (data != null)
            {
                foreach (var entry in data)
                {
                    if (entry.GetType() == typeof(MultiPostData))
                    {
                        var entry2 = (MultiPostData)entry;
                        client.AddBinaryData(entry2.key, entry2.data, entry2.filename, entry2.mimeType);
                        dataTally += entry2.data.Length;
                    }
                    else
                    {
                        client.SetBody(entry.data, entry.mimeType);
                    }
                }

                GFLogger.Instance.AddDebugLog("Added data to request: {0:000}kb", (dataTally / 1000));
            }
            else
            {
                client.SetBody(Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}"), PostData.JSON);
            }

            client.OnDone = (WWW www) =>
            {
                requestEndTime = DateTime.Now;
                OldJSONNode n = null;
                try
                {
                    n = OldJSONNode.Parse(www.text);
                    www.responseHeaders.TryGetValue("location", out responseLocation);
                    //GFLogger.Instance.AddDebugLog("GFWorker response location: {0}", responseLocation);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.Log("Exception parsing json response. " + e.ToString());
                    //                    GFLogger.Instance.AddError("ERROR PARSING: [" + n.ToString() + "]");
                }
                if (n != null && n["error"].AsInt == 1)
                {
                    GFLogger.Instance.AddError("{0}.{1}: ERROR: {2:0}ms", this.GetType().Name, "client.OnDone", requestEndTime.Subtract(requestStartTime).Duration().TotalMilliseconds);
                    if (failureCallback != null)
                    {
                        failureCallback(n);
                    }
                }
                else
                {
                    if (n == null)
                    {
                        if (failureCallback != null)
                        {
                            failureCallback(null);
                        }
                    }
                    else
                    {
                        GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}: OK: {2:0}ms", this.GetType().Name, "client.OnDone", requestEndTime.Subtract(requestStartTime).Duration().TotalMilliseconds));

                        if (successCallback != null)
                        {
                            successCallback(n);
                        }
                    }
                }

                Destroy(this);
            };

            client.OnFail = (WWW www) =>
            {
                requestEndTime = DateTime.Now;
                // unauthorized
                if (www.error.ToString().ToUpper().IndexOf("UNAUTHORIZED") > -1 || www.error.ToString().IndexOf("401") > -1)
                {
                    GFLogger.Instance.AddError("{0}.{1}: UNAUTHORIZED: {2:0}", this.GetType().Name, "client.OnDone", requestEndTime.Subtract(requestStartTime).Duration().TotalMilliseconds);
                    failureCallback(OldJSONNode.Parse("{ body: \"" + www.text + "\", message: \"Unauthorized\", error:1}"));
                }
                else
                {
                    GFLogger.Instance.AddError("{0}.{1}: WWW_ERROR: {2}", this.GetType().Name, "client.OnDone", www.error);
                    failureCallback(OldJSONNode.Parse("{ body: \"" + www.text + "\",  message: \"" + www.error + "\"}"));
                }
                Destroy(this);
            };

            client.OnDisposed = () =>
            {
                requestEndTime = DateTime.Now;
                GFLogger.Instance.AddError("{0}.{1}: WWW_TIMEOUT: {2:0}", this.GetType().Name, "client.OnDone", requestEndTime.Subtract(requestStartTime).Duration().TotalMilliseconds);
                failureCallback(OldJSONNode.Parse("{ message: \"WWW_TIMEOUT\'}"));
                Destroy(this);
            };

            requestStartTime = DateTime.Now;
            client.Request();
        }

        /* Automatically wraps the passed JSON in a List<PostData> and uses main CreateWorker call */
        public static GFWorker CreateWorker(GameObject where, string method, string urlAction, OldJSONNode n)
        {
            List<PostData> data = new List<PostData>();
            data.Add(new PostData(PostData.JSON, Encoding.UTF8.GetBytes(n.ToString())));

            return CreateWorker(where, urlAction, data, method);
        }        
        public static GFWorker CreateWorker(GameObject where, string urlAction, List<PostData> data, string method)
        {
            GFWorker newWorker = where.AddComponent<GFWorker>();
            newWorker.initialize(urlAction, data, method);

            return newWorker;
        }
    }
}
