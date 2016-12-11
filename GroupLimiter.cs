using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Arechi.GroupLimiter
{
    public class GroupLimiter : RocketPlugin<Config>
    {
        public static GroupLimiter Instance;
        public Dictionary<CSteamID,  Group> group = new Dictionary<CSteamID, Group>();

        public class Group
        {
            //public CSteamID PlayerID { get; set; }
            public int Amount { get; set; }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"current_amount", "Current group members: {0}/{1}"},
                    {"bypass", "You exceed the group limit, {0}, but we'll overlook it ;)"},
                    {"group_whitelisted", "The group {0} is whitelisted, you may pass."},
                    {"kick_message_player", "Sorry, {0}, you will be kicked in {1} seconds for exceeding group limit."},
                    {"kick_message_server","{0} had to be kicked because he was one member too much for his group"},
                };
            }
        }

        protected override void Load()
        {
            Instance = this;

            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;

            Rocket.Core.Logging.Logger.Log("GroupLimiter has been loaded!");
            Rocket.Core.Logging.Logger.LogWarning("Current Group Limit: " + Instance.Configuration.Instance.GroupLimit);
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;

            Rocket.Core.Logging.Logger.Log("GroupLimiter has been unloaded!");
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (group.ContainsKey(player.SteamGroupID))
            {
                Group Group = group[player.SteamGroupID];
                //Group.PlayerID = player.CSteamID;
                Group.Amount += 1;

                UnturnedChat.Say(player, Instance.Translate("current_amount", Group.Amount, Instance.Configuration.Instance.GroupLimit), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));

                if (Group.Amount > Instance.Configuration.Instance.GroupLimit)
                {
                    if (player.HasPermission("group.bypass"))
                    {
                        UnturnedChat.Say(player, Instance.Translate("bypass", player.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));
                        Rocket.Core.Logging.Logger.LogWarning(player.DisplayName + "exceeded limit but he bypassed it.");
                        return;
                    }

                    for (int i = 0; i < Configuration.Instance.Whitelist.Length; i++)
                    {
                        if (Configuration.Instance.Whitelist[i].SteamID == player.SteamGroupID.ToString())
                        {
                            UnturnedChat.Say(player, Instance.Translate("group_whitelisted", Configuration.Instance.Whitelist[i].Name, Instance.Configuration.Instance.GroupLimit), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));
                            Rocket.Core.Logging.Logger.Log(player.DisplayName + "exceeded limit but his group is whitelisted.");
                            return;
                        }
                    }

                    UnturnedChat.Say(player, Instance.Translate("kick_message_player", player.DisplayName, Instance.Configuration.Instance.KickTimer), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        Thread.Sleep(Instance.Configuration.Instance.KickTimer * 1000);

                        Provider.kick(player.CSteamID, "Exceeding group limit");
                        UnturnedChat.Say(Instance.Translate("kick_message_server", player.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));
                        Rocket.Core.Logging.Logger.LogWarning(player.DisplayName + " got kicked for exceeding group limit");

                    }).Start();
                }
            }
            else
            {
                group[player.SteamGroupID] = new Group() { /*PlayerID = player.CSteamID,*/ Amount = 1 };
                Group Group = group[player.SteamGroupID];
                UnturnedChat.Say(player, Instance.Translate("current_amount", Group.Amount, Instance.Configuration.Instance.GroupLimit), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.green));
            }   
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (group.ContainsKey(player.SteamGroupID))
            {
                Group Group = group[player.SteamGroupID];
                Group.Amount -= 1;

                if (Group.Amount == 0)
                {
                    group.Remove(player.SteamGroupID);
                }
            }
        }
    }
}
