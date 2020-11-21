using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordConvertor 
{
	public static Vector3Int FloorToInt(Vector3 pPos)
	{
		int x = Mathf.FloorToInt(pPos.x / VoxelData.ChunkWidth);
		int y = Mathf.FloorToInt(pPos.y / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt(pPos.z / VoxelData.ChunkWidth);
		return new Vector3Int(x, y, z);
	}
}
