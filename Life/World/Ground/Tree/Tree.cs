using GameGlobal;
using Goap;
using Godot;
using Life.GOAP.Actions;
using System;
using System.Collections.Generic;
using WorldGen;

namespace Life
{
    public partial class Tree : GroundObject, IGoapActionHolder
    {
        [Export]
        public TreeData TreeData { get; set; }

        [Export]
        Sprite2D Sprite { get; set; }

        [Export]
        Timer FruitTimer { get; set; }

        public override void _Ready()
        {
            Sprite ??= GetNode<Sprite2D>("Sprite2D");
            FruitTimer ??= GetNode<Timer>("FruitTimer");

            StartFruitTimer();
            FruitTimer.Timeout += OnFruitTimerTimeout;
        }

        public override void OnWorldSpawn(WorldGen.World world)
        {
            if (Global.RNG.Randf() <= TreeData.SpawnWithFruitChance)
            {
                SetFruitCount(Global.RNG.RandiRange(1, TreeData.FruitMax));
            }
        }

        public void SetFruitCount(int FruitCount)
        {
            if (FruitCount > 0 && TreeData.FruitCount == 0)
            {
                Sprite.Texture = TreeData.FruitTexture;
            }

            if (FruitCount == 0 && TreeData.FruitCount > 0)
            {
                Sprite.Texture = TreeData.NormalTexture;
            }

            TreeData.FruitCount = FruitCount;
        }

        public void StartFruitTimer()
        {
            FruitTimer.WaitTime = TreeData.FruitGrowthSeconds + Global.RNG.RandfRange(-TreeData.FruitGrowthVarianceSeconds,TreeData.FruitGrowthVarianceSeconds);
            FruitTimer.Start();
        }

        public void OnFruitTimerTimeout()
        {
            SetFruitCount(TreeData.FruitCount + Global.RNG.RandiRange(TreeData.FruitGrowthMin, TreeData.FruitGrowthMax));
            StartFruitTimer();
        }

        public List<GoapAction> GetActions(Node2D agent)
        {
            return [new PickTreeAction(this, agent.GlobalPosition.DistanceSquaredTo(GlobalPosition))];
        }
    }
}

