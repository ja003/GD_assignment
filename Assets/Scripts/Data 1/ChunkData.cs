using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
	public Vector3Int position;

	public ChunkData(Vector3Int pos)
	{
		position = pos;
	}

	public ChunkData(int x, int z) //x,z
	{
		position = new Vector3Int(x, 0, z);
	}

	[HideInInspector]
	public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

	public void Populate()
	{
		for(int y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for(int x = 0; x < VoxelData.ChunkWidth; x++)
			{
				for(int z = 0; z < VoxelData.ChunkWidth; z++)
				{
					map[x, y, z] = new VoxelState(World.Instance.GetVoxel(
						new Vector3(x + position.x, y, z + position.y)));
				}
			}
		}
		World.Instance.worldData.AddToModifiedChunkList(this);
	}
}
