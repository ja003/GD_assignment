using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordConvertor 
{
	public static Vector3Int GetChunkCoord(Vector3 pPos)
	{
		int x = Mathf.FloorToInt(pPos.x / Settings.Get.ChunkWidth);
		int y = 0;
		int z = Mathf.FloorToInt(pPos.z / Settings.Get.ChunkWidth);
		return new Vector3Int(x, y, z);
	}
}
