using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Minecraft/Settings")]
public class Settings : ScriptableObject
{
    public int LoadDistance = 16;

	public int ChunkWidth = 16;
	public int ChunkHeight = 128;
	public int WorldSizeInChunks = 100;

	public int WorldSizeInVoxels
	{
		get { return WorldSizeInChunks * ChunkWidth; }
	}

	public int WorldCentre
	{

		get { return (WorldSizeInChunks * ChunkWidth) / 2; }

	}

	public int ViewDistanceInChunks = 1;


	public int TextureAtlasSizeInBlocks = 16;
	public float NormalizedBlockTextureSize
	{
		get { return 1f / TextureAtlasSizeInBlocks; }
	}

	public static Settings Get => World.Instance.settings;

}
