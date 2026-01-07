using Godot;
using System;

namespace WorldGen
{
    [GlobalClass]
    public partial class GroundObjectSettings : Resource
    {
        [Export]
        public GroundGen Generator { get; set; }

        [Export]
        public PackedScene PackedScene {  get; set; }

        [Export]
        public int PatternIndex {  get; set; }

        public GroundObjectSettings() {}

        public GroundObjectSettings(GroundGen generator, int patternIndex) {
            Generator = generator;
            PatternIndex = patternIndex;
        }

        public GroundObjectSettings(GroundGen generator, PackedScene packedScene)
        {
            Generator = generator;
            PackedScene = packedScene;
        }
    }
}

