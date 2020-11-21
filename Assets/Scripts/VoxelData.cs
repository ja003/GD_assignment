﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
	public static readonly int ChunkWidth = 16;
	public static readonly int ChunkHeight = 128;
	public static readonly int WorldSizeInChunks = 100;

	public static int WorldSizeInVoxels
	{
		get { return WorldSizeInChunks * ChunkWidth; }
	}

	public static readonly int ViewDistanceInChunks = 1;


	//public static readonly int TextureAtlasSizeInBlocks = 2; / try
	public static readonly int TextureAtlasSizeInBlocks = 16; //texturePack
	public static float NormalizedBlockTextureSize
	{
		get { return 1f / TextureAtlasSizeInBlocks; }
	}

	public static readonly Vector3[] voxelVerts = new Vector3[8]
	{
		new Vector3(0,0,0),
		new Vector3(1,0,0),
		new Vector3(1,1,0),
		new Vector3(0,1,0),
		new Vector3(0,0,1),
		new Vector3(1,0,1),
		new Vector3(1,1,1),
		new Vector3(0,1,1)
	};

	public static readonly Vector3[] faceChecks = new Vector3[6]
	{
		new Vector3(0,0,-1),
		new Vector3(0,0,1),
		new Vector3(0,1,0),
		new Vector3(0,-1,0),
		new Vector3(-1,0,0),
		new Vector3(1,0,0)
	};


	public static readonly int[,] voxelTris = new int[6, 4]
	{
		//bakck, front, top, bottom, left, right

		 {0, 3, 1, 2}, // Back Face
         {5, 6, 4, 7}, // Front Face
         {3, 7, 2, 6}, // Top Face
         {1, 5, 0, 4}, // Bottom Face
         {4, 7, 0, 3}, // Left Face
         {1, 2, 5, 6}  // Right Face
	};

	public static readonly Vector2[] voxelUvs = new Vector2[4]
	{
		new Vector2(0,0),
		new Vector2(0,1),
		new Vector2(1,0),
		new Vector2(1,1)
	};

}
