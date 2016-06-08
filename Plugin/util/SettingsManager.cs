namespace Gameframer
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        public KARSettings settings { get; private set; }

        protected SettingsManager()
        {
        }
        public void Save()
        {
            settings.Save();
        }
        void Awake()
        {
            Reload();
        }
        public void Reload()
        {
            settings = new KARSettings(KARSettings.LOCATION);
            settings.Load();
        }
    }
}

