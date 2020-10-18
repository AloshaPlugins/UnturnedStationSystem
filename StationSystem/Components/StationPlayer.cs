using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace StationSystem.Components
{
    public class StationPlayer : UnturnedPlayerComponent
    {
        public bool Activity = false, Dolduruyor = false, uiclosed = false;
        public StructureData Data = null;

        public void Connect(bool activity, StructureData data, bool doldurmak = false)
        {
            this.Activity = activity;
            this.Data = data;
            this.Dolduruyor = doldurmak;
        }

        public void Update()
        {
            if (!Activity) return;
            if (Data == null)
            {
                this.Activity = false;
                return;
            }

            
            var distance = Vector3.Distance(Data.point, Player.Position);
            if (distance > Main.instance.Configuration.Instance.GasDistance)
            {
                EffectManager.askEffectClearByID(Main.instance.Configuration.Instance.GasEffect, Player.CSteamID);
                this.Activity = false;
                this.Data = null;
                this.Dolduruyor = false;
                ChatManager.serverSendMessage($"<size=20><color=yellow>BENZINCI</color></size> <color=white> pompa bu kadar uzak bir mesafeye gidemez. </color>", Color.white, Player.SteamPlayer(), default, EChatMode.GLOBAL, default, true);
                return;
            }
        }
    }
}
