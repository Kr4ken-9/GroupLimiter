using Rocket.API;
using System.Xml.Serialization;

namespace Arechi.GroupLimiter
{
    public class Config : IRocketPluginConfiguration
    {
        public int GroupLimit;
        public string Color;
        public bool ExtraLogging;

        [XmlArrayItem("Group")]
        [XmlArray(ElementName = "Whitelist")]
        public Whitelist[] Whitelist;

        public void LoadDefaults()
        {
            Color = "Green";
            GroupLimit = 5;
            ExtraLogging = true;

            Whitelist = new Whitelist[]{
                new Whitelist("RocketMod", "103582791439889796")
            };
        }
    }
}