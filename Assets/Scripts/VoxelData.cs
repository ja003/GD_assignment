using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
	public static readonly Vector3[] VoxelVerts = new Vector3[8]
	{
		new Vector3(0.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 1.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 1.0f),
	};

	public static readonly Vector3Int[] FaceChecks = new Vector3Int[6]
	{
		new Vector3Int(0, 0, -1),
		new Vector3Int(0, 0, 1),
		new Vector3Int(0, 1, 0),
		new Vector3Int(0, -1, 0),
		new Vector3Int(-1, 0, 0),
		new Vector3Int(1, 0, 0),
	};

	public static readonly int[,] VoxelTris = new int[6, 4]
	{
		{0, 3, 1, 2}, //back 
		{5, 6, 4, 7}, //front 
		{3, 7, 2, 6}, //top 
		{1, 5, 0, 4}, //bottom 
		{4, 7, 0, 3}, //left 
		{1, 2, 5, 6}  //right 
	};
}
