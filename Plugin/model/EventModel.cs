using OldSimpleJSON;

namespace Gameframer
{
    public class EventModel
    {
        public string eventName;
        public string eventDescription;

        public bool isUserEvent;

        public double startTime;
        public double endTime;
        public double universalTime;
        public double endUniversalTime;

        public string situation;

        public string bodyName;
        public double latitude;
        public double longitude;
        public double apoapsis;
        public double periapsis;
        public double orbitalPeriod;

        public EventModel(string eventName, string eventDescription,
            Vessel vessel, bool isUserEvent = false)
        {
            this.eventName = eventName;
            this.eventDescription = eventDescription;
            this.isUserEvent = isUserEvent;
            this.startTime = vessel.missionTime;
            this.endTime = -1;
            this.situation = vessel.situation.ToString();
            this.universalTime = Planetarium.GetUniversalTime();
            this.endUniversalTime = -1;
         
            this.bodyName = vessel.mainBody.bodyName;
        }

        public OldJSONNode ToJSON()
        {
            OldJSONNode eventNode = OldJSONNode.Parse("{}");
            eventNode["name"] = eventName;
            eventNode["description"] = eventDescription;
            eventNode["situation"] = situation;

            eventNode["missionTime"].AsDouble = startTime;
            eventNode["endMissionTime"].AsDouble = endTime;

            eventNode["missionTimeInDays"].AsDouble = KSPUtils.GameTimeInDays(startTime);
            eventNode["endMissionTimeInDays"].AsDouble = KSPUtils.GameTimeInDays(endTime);
            eventNode["universalTime"].AsDouble = universalTime;
            eventNode["endUniversalTime"].AsDouble = endUniversalTime;

            eventNode["body"] = bodyName;
            eventNode["latitude"].AsDouble = latitude;
            eventNode["longitude"].AsDouble = longitude;
            eventNode["apoapsis"].AsDouble = apoapsis;
            eventNode["periapsis"].AsDouble = periapsis;
            eventNode["period"].AsDouble = orbitalPeriod;
            
            eventNode["userEvent"].AsBool = isUserEvent;
            return eventNode;
        }
    }
}
