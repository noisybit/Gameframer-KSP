using KSPPluginFramework;
using OldSimpleJSON;
using System;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorController : MonoBehaviourExtended
    {
        public OldJSONArray vessels = OldJSONArray.Parse("[]").AsArray;

        public bool errorState = false;
        public string healthMessage = "";
        public bool isUploading = false;
        public bool isBusy = false;

        #region OVERRIDES

        internal override void Start()
        {
            TryGetVessels();
            //GameEvents.onEditorLoad.Add(this.onEditorLoad);
            EditorLogic.fetch.saveBtn.onClick.AddListener(SaveButtonClicked);
            GFLogger.Instance.SetUserStatusMessage("Ready");
        }

        internal override void OnDestroy()
        {
            SettingsManager.Instance.settings.Save();
        }

        internal override void Update()
        {
        }

        #endregion



        private void onEditorLoad(ShipConstruct s, CraftBrowser.LoadType data)
        {
            GFLogger.Instance.ClearUserStatusMessage();
        }

        public void SaveButtonClicked()
        {
            if (SettingsManager.Instance.settings.editorAutoSave)
            {
                DoCaptureAndUpload();
            }
        }

        private String FindVesselID(String vesselName)
        {
            for(int i=0; i<vessels.Count;i++)
            {
                OldJSONNode n = vessels[i];
                if (vesselName.CompareTo(n["name"]) == 0)
                {
                    return n["_id"];
                }
            }

            return null;
        }


        #region Public methods
        public void DeleteVessel(string vesselToDeleteID)
        {
            GFLogger.Instance.AddDebugLog("Doing vessel delete for : " + vesselToDeleteID);
            var w = GFWorker.CreateWorker(this.gameObject, "/vessels/" + vesselToDeleteID, null, "DELETE");
            isBusy = true;
            w.OnDone = (OldJSONNode n) =>
            {
                isBusy = false;
                GFLogger.Instance.SetUserStatusMessage(String.Format("Deleted {0}", n["data"]["name"]));
                TryGetVessels();
            };

            w.OnFail = (OldJSONNode n) =>
            {
                isBusy = false;
                GFLogger.Instance.SetUserStatusMessage("Failed deleting vessel", 1);
                TryGetVessels();
            };
        }

        public void TryGetVessels()
        {
            isBusy = true;
            VesselListLookup.CreateComponent(this.gameObject, SettingsManager.Instance.settings.username, GetVesselsCallback, GetVesselsFailed);
        }

        public void DoCaptureAndUpload()
        {
            isBusy = true;
            VesselCreator.CreateWorker(gameObject, UploadDone, UploadFailed);
        }

        public void AddScreenshot()
        {
            isBusy = true;

            String vesselID = FindVesselID(EditorLogic.fetch.ship.shipName);
            if (vesselID != null)
            {
                VesselSnapshot.CreateWorker(vesselID, gameObject, UploadDone, UploadFailed);
            }
            else
            {
                isBusy = false;
            }
        }
        #endregion


        private void GetVesselsFailed(OldJSONNode data)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            isBusy = false;
            errorState = true;
            GFLogger.Instance.AddDebugLog(data.ToString());
            GFLogger.Instance.SetUserStatusMessage("Failed retrieving vessels", 1);
        }
        private void GetVesselsCallback(OldJSONNode data)
        {
            OldJSONNode vesselsData = (data == null || data["data"] == null) ? OldJSONArray.Parse("[]") : data["data"];

            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}: vessels.length = {2}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, vesselsData.AsArray.Count));
            vessels = vesselsData.AsArray;
            isBusy = false;
            errorState = false;
        }

        private void UploadDone(OldJSONNode n)
        {
            isBusy = false;
            GFLogger.Instance.SetUserStatusMessage(String.Format("{0} uploaded.", n["data"]["name"]));
            ScreenMessages.PostScreenMessage(String.Format("Gameframer: {0} saved", n["data"]["name"]), 2, ScreenMessageStyle.UPPER_LEFT);
            TryGetVessels();
        }
        private void UploadFailed(OldJSONNode n)
        {
            isBusy = false;
            GFLogger.Instance.SetUserStatusMessage("Vessel upload failed", 1);
            ScreenMessages.PostScreenMessage("Vessel upload failed.", 3, ScreenMessageStyle.UPPER_RIGHT);
            TryGetVessels();
        }

    }
}
