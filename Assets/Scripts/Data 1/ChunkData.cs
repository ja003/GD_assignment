using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
	static Settings settings => World.Instance.settings;

	public Vector3Int position;

	public ChunkData(Vector3Int pos)
	{
		position = pos;
		Init();
	}

	public ChunkData(int x, int z) //x,z
	{
		position = new Vector3Int(x, 0, z);
		Init();
	}

	private void Init()
	{
		map = new VoxelState[Settings.Get.ChunkWidth, Settings.Get.ChunkHeight, Settings.Get.ChunkWidth];
	}

	[HideInInspector]
	public VoxelState[,,] map;

	public void Populate()
	{
		for(int y = 0; y < Settings.Get.ChunkHeight; y++)
		{
			for(int x = 0; x < Settings.Get.ChunkWidth; x++)
			{
				for(int z = 0; z < Settings.Get.ChunkWidth; z++)
				{
					map[x, y, z] = new VoxelState(World.Instance.GetVoxel(
						new Vector3(x + position.x, y, z + position.y)));
				}
			}
		}
		World.Instance.worldData.AddToModifiedChunkList(this);
	}
}
