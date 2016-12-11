using Rocket.API;
using System.Xml.Serialization;

namespace Arechi.GroupLimiter
{
    public class Config : IRocketPluginConfiguration
    {
        public int GroupLimit, KickTimer;
        public string Color;

        [XmlArrayItem("Group")]
        [XmlArray(ElementName = "Whitelist")]
        public Whitelist[] Whitelist;

        public void LoadDefaults()
        {
            Color = "Green";
            GroupLimit = 5;
            KickTimer = 10;

            Whitelist = new Whitelist[]{
                new Whitelist("RocketMod", "103582791439889796")
            };
        }
    }

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