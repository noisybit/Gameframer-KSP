using KronalUtils;
using KSPPluginFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class FileWrapper
    {
        public string url, filename;
        public byte[] data;
    }


    [KSPAddonFixed(KSPAddon.Startup.EditorAny, false, typeof(EditorController))]
    public class EditorController : MonoBehaviourExtended
    {
        private KRSVesselShot vesselShot;
        private Rect orthoViewRect;
        public int saveSetting = 0; // 0 = on save, 1 = on launch
        public KARSettings settings = new KARSettings("KARSettings.cfg");
        public uint shipCount = 0;
        public IEnumerable<JsonObject> vessels = new List<JsonObject>();
        public string lastShipNameUploaded;
        public string lastShipID;
        public bool healthCheck = false;

        private IGameframerService serviceInteface;

        #region OVERRIDES

        internal override void Start()
        {
            LogFormatted(System.Reflection.MethodBase.GetCurrentMethod().Name);
            serviceInteface = new GameframerService();
            vesselShot = new KRSVesselShot();
            
            settings.Load();
            vessels = GetVessels();
        }

        internal IEnumerable<JsonObject> GetVessels()
        {
            Stopwatch watch;
            watch = Stopwatch.StartNew();
            LogFormatted("Getting vessels...");
            IEnumerable<JsonObject> _vessels = new List<JsonObject>();

            try
            {
                _vessels = serviceInteface.GetVessels(settings.username);
            }
            catch (Exception e)
            {
                LogFormatted("Exception getting vessels {0}", e.Message);
            }

            healthCheck = true;
            watch.Stop();
            LogFormatted("GetVessels took {0}ms", watch.ElapsedMilliseconds);

            return _vessels;
        }

        internal override void Awake()
        {
        }

        internal override void OnDestroy()
        {
        }

        internal override void Update()
        {
            if ((this.orthoViewRect.width * this.orthoViewRect.height) > 1f)
            {
                this.vesselShot.Update(false, (int)this.orthoViewRect.width * 2, (int)this.orthoViewRect.height * 2);
            }
        }

        #endregion

        public void RefreshVessels()
        {
            vessels = GetVessels();
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

        public void DoCaptureAndUpload(bool overwrite = true)
        {
            string shipName = RemoveSpecialCharacters(EditorLogic.fetch.shipNameField.Text);
            string shipDescription = EditorLogic.fetch.shipDescriptionField.Text;
            List<Part> parts = EditorLogic.fetch.ship.parts;
            string shipID;
            string imageName;
            string craftName;

            if (shipName != lastShipNameUploaded)
            {
                shipID = System.Guid.NewGuid().ToString();
            }
            else
            {
                shipID = lastShipID;
            }

            imageName = shipID + ".jpg";
            craftName = shipID + ".craft";

            Stopwatch watch;
            watch = Stopwatch.StartNew();
            serviceInteface.SaveCraft(settings.username, craftName);
            watch.Stop();
            LogFormatted("DoUploadCraft took {0}ms", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            KAMRShip shipWrapper = new KAMRShip(settings.username, shipName, shipDescription, imageName, craftName, DateTime.Now, parts);
            serviceInteface.SaveJson(settings.username, shipWrapper);
            watch.Stop();
            LogFormatted("DoUploadJson took {0}ms", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            TakeAndSaveScreenshot(imageName);
            watch.Stop();
            LogFormatted("TakeAndSaveScreenshot took {0}ms", watch.ElapsedMilliseconds);

            if (shipName != lastShipNameUploaded)
            {
                lastShipNameUploaded = shipName;
                lastShipID = shipID;
            }

            vessels = GetVessels();
        }

        private void TakeAndSaveScreenshot(string filename)
        {
            var watch = Stopwatch.StartNew();
            this.vesselShot.Update();
            RenderTexture t = this.vesselShot.GetTexture();
            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = t;
            Texture2D screenShot = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            watch.Stop();
            LogFormatted("\tScreenshot took {0}ms", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            JPGEncoder encoder = new JPGEncoder(screenShot.GetPixels(), screenShot.width, screenShot.height, 85);
            encoder.doEncoding();
            watch.Stop();
            LogFormatted("\tJPG Encoding took {0}ms", watch.ElapsedMilliseconds);

            RenderTexture.active = oldActive;
            t = null;

            serviceInteface.SaveScreenshot(settings.username, filename, encoder.GetBytes());
        }
    }
}
