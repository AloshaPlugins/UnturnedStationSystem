using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using SDG.Framework.Debug;

namespace StationSystem
{
    public class Config : IRocketPluginConfiguration
    {
        public string Name;
        public ushort GasEffect, MenuEffect;
        public uint GasPrice;
        public double GasMultiplier = 0.1;
        public float StationDistance, GasDistance;
        public List<ushort> Stations = new List<ushort>();
        public void LoadDefaults()
        {
            Name = "Alosha Plugins";
            GasEffect = 50800;
            MenuEffect = 50801;
            GasPrice = 5;
            GasMultiplier = 0.1;
            StationDistance = 5f;
            GasDistance = 15f;
            Stations = new List<ushort>()
            {
                369
            };
        }
    }
}
