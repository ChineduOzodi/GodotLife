using Godot;
using Life.Scripts.Classes;
using Life.Scripts.Classes.Tiles;
using System;
using System.Drawing;

public partial class WorldMap : TileMap
{
	
	private World world;

	public override void _Ready()
	{
		world = GetParent<World>();
	}

	public void GenerateMap()
	{
		var land_cells = new Godot.Collections.Array<Vector2I>();
		var water_cells = new Godot.Collections.Array<Vector2I>();
		var snow_cells = new Godot.Collections.Array<Vector2I>();

		for (int x = 0; x < world.Width; x++)
		{
			for (int y = 0; y < world.Height; y++)
			{
				FloorTile floorTile = world.floorTiles[x][y];
				GroundTile groundTile = world.groundTiles[x][y];

				Vector2I coord = new(x, y);

                if (groundTile.HasObject())
                {
                    this.SetCell(1, coord, groundTile.Obj.tileSetSourceId, groundTile.Obj.atlasCoords);
                }

                if (floorTile.hasRiver || floorTile.hasWaterBasin)
				{
					water_cells.Add(coord);
					continue;
				}

				switch (floorTile.biome)
				{
					case FloorTileType.land:
						land_cells.Add(coord);
						break;
					case FloorTileType.Water:
						water_cells.Add(coord);
						break;
					case FloorTileType.snow:
						snow_cells.Add(coord);
						break;
					case FloorTileType.SeaIce:
						water_cells.Add(coord);
						break;
					case FloorTileType.Glacier:
						snow_cells.Add(coord);
						break;
					default:
						throw new Exception($"Unrecognized biome type: {floorTile.biome}");
				}
			}
		}

		this.SetCellsTerrainConnect(0, land_cells, 0, 0, false);
		this.SetCellsTerrainConnect(0, water_cells, 0, 1, false);
		this.SetCellsTerrainConnect(0, snow_cells, 0, 2, false);
	}
}
