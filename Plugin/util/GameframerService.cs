using System;

namespace Gameframer
{
    public class GameframerService
    {
        public static string PROD = "https://api.gameframer.com/api/v2";
        public static string TEST = "https://staging.gameframer.com/api/v2";
        public static string DEV = "http://nibbler.local:30000/api/v2";
        public static string HOSTNAME = PROD;

        public static void SetHostname(int hostOption)
        {
            switch (hostOption)
            {
                case 0: HOSTNAME = PROD; break;
                case 1: HOSTNAME = TEST; break;
                case 2: HOSTNAME = DEV; break;
            };
        }
        public static int GetHostname()
        {
            if (HOSTNAME == PROD) return 0;
            if (HOSTNAME == TEST) return 1;

            return 2;
        }
        public static string GetForumURL()
        {
            return "http://forum.kerbalspaceprogram.com/index.php?/topic/127491-105-gameframer-ksp-automatically-document-and-show-off-your-stuff-v061-beta/";
        }
        public static string GetWebBase(bool includeGames = true)
        {
            if (HOSTNAME == PROD) return new Uri("http://gameframer.com/#/" + (includeGames ? "games/" : "")).ToString();
            if (HOSTNAME == TEST) return new Uri("http://testweb.gameframer.com/#/" + (includeGames ? "games/" : "")).ToString();

            return new Uri("http://nibbler.local:9000/#/" + (includeGames ? "games/" : "")).ToString();
        }
    }
}
