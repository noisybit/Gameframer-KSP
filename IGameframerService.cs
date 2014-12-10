using System;
using System.Collections.Generic;
using System.Net;

namespace Gameframer
{
    interface IGameframerService
    {
        List<string> GetNames();
        HttpStatusCode RegisterName(String name);
        bool CheckVersion();
        bool SaveCraft(string username, String craftname);
        bool SaveJson(string username, KAMRShip ship);
        bool SaveScreenshot(string username, string filename, byte[] bytes);
        IEnumerable<JsonObject> GetVessels(string username);
        bool DeleteVessel(string username, string shipname);
    }
}
