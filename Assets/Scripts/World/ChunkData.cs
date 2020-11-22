using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
	int x;
	int z;
	public Vector3Int coord
	{

		get { return new Vector3Int(x, 0, z); }
		set
		{
			x = value.x;
			z = value.z;
		}
	}

	public ChunkData(Vector3Int pPos)
	{
		coord = pPos;
		Init();
	}

	public ChunkData(int x, int z)
	{
		coord = new Vector3Int(x, 0, z);
		Init();
	}

	private void Init()
	{
		map = new EBlockId[Settings.Get.ChunkWidth, Settings.Get.ChunkHeight, Settings.Get.ChunkWidth];
	}

	[HideInInspector]
	public EBlockId[,,] map;

	public void Populate()
	{
		for(int y = 0; y < Settings.Get.ChunkHeight; y++)
		{
			for(int x = 0; x < Settings.Get.ChunkWidth; x++)
			{
				for(int z = 0; z < Settings.Get.ChunkWidth; z++)
				{
					map[x, y, z] = World.Instance.VoxelController.GetVoxel(
						new Vector3(
							x + coord.x * Settings.Get.ChunkWidth,
							y,
							z + coord.z * Settings.Get.ChunkWidth));
				}
			}
		}
	}

	public override string ToString()
	{
		return $"[{x},{z}]";
	}
}
