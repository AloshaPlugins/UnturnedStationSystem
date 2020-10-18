using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using StationSystem.Components;
using Steamworks;
using UnityEngine;

namespace StationSystem
{
    public class Main : RocketPlugin<Config>
    {
        public static Main instance;
        protected override void Load()
        {
            instance = this;
            UnturnedPlayerEvents.OnPlayerUpdateGesture += UnturnedPlayerEventsOnOnPlayerUpdateGesture;
            UnturnedPlayerEvents.OnPlayerUpdateGesture += araçİşlemi;
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
        }

        private void OnEffectButtonClicked(Player player, string buttonname)
        {
            if (buttonname == "StationSystem_Close")
            {
                var comp = player.GetComponent<StationPlayer>();
                comp.uiclosed = true;
                EffectManager.askEffectClearByID(Configuration.Instance.MenuEffect, player.channel.owner.playerID.steamID);
                player.serversideSetPluginModal(false);
                return;
            }
        }

        public void ShowUI(UnturnedPlayer player, InteractableVehicle vehicle, uint totalCount = 0, int time = 0)
        {
            EffectManager.sendUIEffect(Configuration.Instance.MenuEffect, 761, player.CSteamID, true, Configuration.Instance.Name, vehicle.asset.vehicleName, $"{time} Saniye", $"{totalCount}");
            vehicle.getDisplayFuel(out var cf, out var mf);

            var x = Convert.ToInt32(Math.Floor((((cf * 1.0d) / mf) * 100)));
            var sayı = EnYakınOnluğaYuvarla(x);
            EffectManager.sendUIEffectText(761, player.CSteamID, true, "Fuel_Text", $"%{x}");

            var dönecek = sayı / 10;
            for (int i = 0; i < dönecek; i++)
            {
                EffectManager.sendUIEffectVisibility(761, player.CSteamID, true, "Fuel_"+i , true);
            }
        }
        public int EnYakınOnluğaYuvarla(int sayı)
        {
            return (int)(Math.Ceiling(sayı / 10.0d) * 10);
        }

        private IEnumerator BeginFuel(InteractableVehicle vehicle, UnturnedPlayer player)
        {
            if (!vehicle.usesFuel) yield break;

            var maxfuel = vehicle.asset.fuel;
            var currentfuel = vehicle.fuel;
            if (maxfuel == currentfuel) yield break;

            var litreBaşına = ushort.Parse((maxfuel * Configuration.Instance.GasMultiplier).ToString());
            var zaman = (int)((maxfuel - currentfuel) / litreBaşına);
            uint totalCount = 0;
            var comp = player.GetComponent<StationPlayer>();
            if (!comp.uiclosed)
            {
                player.Player.serversideSetPluginModal(true);
                ShowUI(player, vehicle, totalCount, zaman);
            }

            while (true)
            {
                if(vehicle == null || player == null) yield break;
                if (vehicle.isDead || vehicle.isUnderwater || vehicle.isExploded)
                {
                    EffectManager.askEffectClearByID(Configuration.Instance.MenuEffect, player.CSteamID);
                    player.Player.serversideSetPluginModal(false);

                    yield break;
                }

                comp = player.GetComponent<StationPlayer>();

                if (comp.Activity == false || comp.Data == null || comp.Dolduruyor == false)
                {
                    uiSil(player, true);
                    yield break;
                }

                if (player.Experience - Configuration.Instance.GasPrice == uint.MaxValue)
                {
                    comp.Connect(false, null);
                    uiSil(player, false);
                    yield break;
                }
                if(!comp.uiclosed) ShowUI(player, vehicle, totalCount, zaman);
                if (vehicle.fuel >= maxfuel)
                {
                    comp.Connect(false, null);
                    uiSil(player);
                    yield break;
                }
                vehicle.askFillFuel(litreBaşına);
                player.Experience -= Configuration.Instance.GasPrice;
                totalCount += Configuration.Instance.GasPrice;
                zaman--;
                yield return new WaitForSeconds(1f);
            }
        }

        private void uiSil(UnturnedPlayer player, bool durum = false)
        {
            EffectManager.askEffectClearByID(Configuration.Instance.GasEffect, player.CSteamID);
            if (durum)
            {
                EffectManager.askEffectClearByID(Configuration.Instance.MenuEffect, player.CSteamID);
                player.Player.serversideSetPluginModal(false);
            }

        }

        private void araçİşlemi(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture != UnturnedPlayerEvents.PlayerGesture.PunchLeft) return;
            var comp = player.GetComponent<StationPlayer>();
            if (comp == null) return;

            if (comp.Data == null || comp.Activity == false) return;

            if (comp.Dolduruyor == true)
            {
                if (comp.uiclosed)
                {
                    comp.uiclosed = false;
                    player.Player.serversideSetPluginModal(true);
                }
                return;
            }

            var raycast = Physics.Raycast(new Ray(player.Player.look.aim.position, player.Player.look.aim.forward), out var info,
                Configuration.Instance.GasDistance, RayMasks.VEHICLE);
            if (!raycast) return;

            var hit = info;
            if (hit.transform == null) return;

            var vehicle = hit.transform.GetComponentInChildren<InteractableVehicle>();
            if (vehicle == null) return;
            comp.Dolduruyor = true;
            comp.uiclosed = false;
            
            StartCoroutine(BeginFuel(vehicle, player));
        }

        private void UnturnedPlayerEventsOnOnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture != UnturnedPlayerEvents.PlayerGesture.PunchRight) return;
            var raycast = Physics.Raycast(new Ray(player.Player.look.aim.position, player.Player.look.aim.forward), out var info,
                Configuration.Instance.StationDistance, RayMasks.STRUCTURE | RayMasks.STRUCTURE_INTERACT);
            if (!raycast) return;

            var hit = info;
            if (hit.transform == null) return;

            var flag = StructureManager.tryGetInfo(hit.transform, out var x, out var y, out var index, out var region);
            if (!flag) return;
            var structer = region.structures[index];
            if (!Configuration.Instance.Stations.Contains(structer.structure.id)) return;
            var comp = player.GetComponent<StationPlayer>();
            if (comp == null) return;
            if (comp.Activity == true && comp.Data == structer)
            {
                comp.Activity = false;
                comp.Dolduruyor = false;
                comp.Data = null;
                uiSil(player);
                return;
            }
            if (comp.Dolduruyor == true || comp.Activity == true) return;
            comp.Connect(true, structer);
            EffectManager.sendUIEffect(Configuration.Instance.GasEffect, 760, player.CSteamID, true);
        }
 
        protected override void Unload()
        {
            instance = null;
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= UnturnedPlayerEventsOnOnPlayerUpdateGesture;
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= araçİşlemi;
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;

        }
    }
}
