using System.Collections.Generic;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static EventDetails GetEventDetails(string eventCode)
        {
            var eventDetailsDictionary = new Dictionary<string, EventDetails>
            {
                { "airQuality", new EventDetails("Air Quality", "Severe or Extreme", "Observed") },
                { "civilEmerg", new EventDetails("Civil Emergency", "Severe or Extreme", "Observed") },
                { "terrorism", new EventDetails("Terrorism", "Severe or Extreme", "Observed") },
                { "animalDang", new EventDetails("Dangerous Animal", "Severe or Extreme", "Observed") },
                { "wildFire", new EventDetails("Wildfire", "Severe or Extreme", "Likely or Observed") },
                { "industryFire", new EventDetails("Industrial Fire", "Severe or Extreme", "Observed") },
                { "urbanFire", new EventDetails("Urban Fire", "Severe or Extreme", "Observed") },
                { "forestFire", new EventDetails("Forest Fire", "Severe or Extreme", "Likely or Observed") },
                { "stormSurge", new EventDetails("Storm Surge", "Severe or Extreme", "Likely or Observed") },
                { "flashFlood", new EventDetails("Flash Flood", "Severe or Extreme", "Likely or Observed") },
                { "damOverflow", new EventDetails("Dam Overflow", "Severe or Extreme", "Likely or Observed") },
                { "earthquake", new EventDetails("Earthquake", "Severe or Extreme", "Likely or Observed") },
                { "magnetStorm", new EventDetails("Magnetic Storm", "Severe or Extreme", "Likely or Observed") },
                { "landslide", new EventDetails("Landslide", "Severe or Extreme", "Likely or Observed") },
                { "meteor", new EventDetails("Meteor", "Severe or Extreme", "Likely or Observed") },
                { "tsunami", new EventDetails("Tsunami", "Severe or Extreme", "Likely or Observed") },
                { "lahar", new EventDetails("Lahar", "Severe or Extreme", "Likely or Observed") },
                { "pyroclasticS", new EventDetails("Pyroclastic Surge", "Severe or Extreme", "Likely or Observed") },
                { "pyroclasticF", new EventDetails("Pyroclastic Flow", "Severe or Extreme", "Likely or Observed") },
                { "volcanicAsh", new EventDetails("Volcanic Ash", "Severe or Extreme", "Likely or Observed") },
                { "chemical", new EventDetails("Chemical", "Severe or Extreme", "Observed") },
                { "biological", new EventDetails("Biological", "Severe or Extreme", "Observed") },
                { "radiological", new EventDetails("Radiological", "Severe or Extreme", "Observed") },
                { "explosives", new EventDetails("Explosives", "Severe or Extreme", "Likely or Observed") },
                { "fallObject", new EventDetails("Falling Object", "Severe or Extreme", "Observed") },
                { "drinkingWate", new EventDetails("Drinking Water", "Severe or Extreme", "Observed") },
                { "amber", new EventDetails("Amber Alert", "Severe or Extreme", "Observed") },
                { "hurricane", new EventDetails("Hurricane", "Severe or Extreme", "Observed") },
                { "thunderstorm", new EventDetails("Thunderstorm", "Severe or Extreme", "Observed") },
                { "tornado", new EventDetails("Tornado", "Severe or Extreme", "Likely or Observed") },
                { "testMessage", new EventDetails("Test Message", "Minor", "Observed") },
                { "911Service", new EventDetails("911 Service", "Severe or Extreme", "Observed") }
            };

            if (eventDetailsDictionary.TryGetValue(eventCode, out EventDetails eventDetails))
            {
                return eventDetails;
            }
            else
            {
                return new EventDetails(eventCode, "Unknown Severity", "Unknown Certainty");
            }
        }

        public class EventDetails
        {
            public string FriendlyName { get; }
            public string Severity { get; }
            public string Certainty { get; }

            public EventDetails(string friendlyName, string severity, string certainty)
            {
                FriendlyName = friendlyName;
                Severity = severity;
                Certainty = certainty;
            }
        }
    }
}
