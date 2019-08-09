using Godot;
using System;

public class Map : Node2D
{
    private World world;

    private int width;
    private int height;
    private OpenSimplexNoise noise;
    private float waterLevel;
    private int tileSize;
    private float noiseScale;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        world = GetParent<World>();
    }

    public override void _Draw()
    {
        base._Draw();

        int xOffset = -width / 2;
        int yOffset = -height / 2;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float elev = (noise.GetNoise2d((x + xOffset) * noiseScale, (y + yOffset) * noiseScale) + 1) * .5f;
                if (elev > waterLevel)
                {
                    Color color = new Color(0.2f, 0.5f, 0.2f, 1).LinearInterpolate(new Color(0, 0.1f, 0), (elev - waterLevel) / (1 - waterLevel));
                    DrawRect(new Rect2((x + xOffset) * tileSize - tileSize / 2, (y + yOffset) * tileSize - tileSize / 2, tileSize, tileSize),
                        color);
                }
                else
                {
                    Color color;
                    float deepestWaterElev = 0.3f;
                    Color deepestWaterColor = new Color(0.1f, 0.1f, 0.4f); ;
                    if (elev < deepestWaterElev)
                    {
                        color = deepestWaterColor;
                    } else
                    {
                        color = new Color(0.2f, 0.2f, .6f).LinearInterpolate(deepestWaterColor, (waterLevel - elev) / (waterLevel - deepestWaterElev));
                    }
                    DrawRect(new Rect2((x + xOffset) * tileSize - tileSize / 2, (y + yOffset) * tileSize - tileSize / 2, tileSize, tileSize),
                        color);
                }

            }
        }

    }

    public void GenerateMap(int width, int height, OpenSimplexNoise noise, float waterLevel, int tileSize, float noiseScale)
    {
        this.width = width;
        this.height = height;
        this.noise = noise;
        this.waterLevel = waterLevel;
        this.tileSize = tileSize;
        this.noiseScale = noiseScale;
        Update();
    }
}
