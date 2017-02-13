using System.Xml.Serialization;

namespace Arechi.GroupLimiter
{
    public sealed class Whitelist
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("SteamID")]
        public string SteamID;

        public Whitelist(string name, string steamid)
        {
            Name = name;
            SteamID = steamid;
        }
        public Whitelist()
        {
            Name = "";
            SteamID = "";
        }
    }
}
