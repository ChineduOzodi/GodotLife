using Godot;
using System;

namespace Life
{
    [GlobalClass]
    public partial class TreeData : Resource
    {

        public int FruitCount { get; set; } = 0;

        [Export]
        public AtlasTexture NormalTexture { get; set; }

        [Export]
        public AtlasTexture FruitTexture { get; set; }

        [Export(PropertyHint.Range, "0,1,")]
        public float SpawnWithFruitChance { get; set; } = 0.1f;

        [Export]
        public int FruitMax { get; set; } = 15;

        /// <summary>
        /// Each time tree grows more fruit, what is the minimum fruit it can add.
        /// </summary>
        [Export]
        public int FruitGrowthMin { get; set; } = 1;

        /// <summary>
        /// Each time tree grows more fruit, what is the maximum fruit it can add.
        /// </summary>
        [Export]
        public int FruitGrowthMax { get; set; } = 5;

        /// <summary>
        /// Seconds before tree grows fruit.
        /// </summary>
        [Export]
        public float FruitGrowthSeconds { get; set; } = 120f;

        [Export]
        public float FruitGrowthVarianceSeconds { get; set; } = 5f;
    }
}

