using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Permissions;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

using Logger = Rocket.Core.Logging.Logger;

namespace Arechi.GroupLimiter
{
    public class GroupLimiter : RocketPlugin<Config>
    {
        public static GroupLimiter Instance;
        public Dictionary<CSteamID, Group> group = new Dictionary<CSteamID, Group>();
        public bool Log;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"bypassed","{0} bypassed the group limit."},
                    {"whitelisted","{0} bypassed the group limit with a whitelisted group."},
                    {"rejected", "{0} was rejected join access for exceeding the group limit."},
                    {"update", "Group: {0} Online Members: {1}."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            Log = Instance.Configuration.Instance.ExtraLogging;

            UnturnedPermissions.OnJoinRequested += OnJoinRequested;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;

            Logger.Log("GroupLimiter has been loaded!");
            Logger.Log("Current Group Limit: " + Instance.Configuration.Instance.GroupLimit);
        }

        protected override void Unload()
        {
            UnturnedPermissions.OnJoinRequested -= OnJoinRequested;
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;

            Logger.Log("GroupLimiter has been unloaded!");
        }

        private void OnJoinRequested(CSteamID id, ref ESteamRejection? rejection)
        {
            UnturnedPlayer Player = UnturnedPlayer.FromCSteamID(id);
            if (Player.SteamGroupID == null || Player.SteamGroupID == CSteamID.Nil) return;
            if (group.ContainsKey(Player.SteamGroupID))
            {
                Group Group = group[Player.SteamGroupID];
                if (Group.Amount >= Instance.Configuration.Instance.GroupLimit)
                {
                    if (Player.HasPermission("group.bypass"))
                    {
                        if (Log) Logger.Log(Instance.Translate("bypassed", Player.CharacterName));
                        return;
                    }

                    for (int i = 0; i < Configuration.Instance.Whitelist.Length; i++)
                    {
                        if (Configuration.Instance.Whitelist[i].SteamID == Player.SteamGroupID.ToString())
                        {
                            if (Log) Logger.Log(Instance.Translate("whitelisted", Player.CharacterName));
                            return;
                        }
                    }

                    rejection = ESteamRejection.SERVER_FULL;
                    if (Log) Logger.Log(Instance.Translate("rejected", Player.CharacterName));
                    return;
                }
            }
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (player.SteamGroupID == null || player.SteamGroupID == CSteamID.Nil) return;
            if (!group.ContainsKey(player.SteamGroupID))
                group.Add(player.SteamGroupID, new Group() { /*PlayerID = player.CSteamID,*/ Amount = 0 });

            group[player.SteamGroupID].Amount++;
            if (Log) Logger.Log(Instance.Translate("update", player.SteamGroupID, group[player.SteamGroupID].Amount));
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (player.SteamGroupID == null || player.SteamGroupID == CSteamID.Nil) return;
            if (group.ContainsKey(player.SteamGroupID))
            {
                group[player.SteamGroupID].Amount--;

                if (group[player.SteamGroupID].Amount == 0)
                    group.Remove(player.SteamGroupID);
            }
        }
    }
}
