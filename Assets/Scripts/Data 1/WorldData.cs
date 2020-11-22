using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WorldData
{
	public string worldName = "Prototype";
	public int seed;

	[NonSerialized]
	public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();


	[NonSerialized]
	public List<ChunkData> modifiedChunks = new List<ChunkData>();

	public WorldData(WorldData worldData)
	{
		worldName = worldData.worldName;
		seed = worldData.seed;
	}

	public WorldData(string name, int seed)
	{
		worldName = name;
		this.seed = seed;
	}

	public ChunkData RequestChunk(Vector3Int coord, bool create)
	{
		ChunkData c;

		if(chunks.ContainsKey(coord))
			c = chunks[coord];
		else if(!create)
			c = null;
		else
		{
			LoadChunk(coord);
			c = chunks[coord];
		}
		return c;

	}

	internal void LoadChunk(Vector3Int coord)
	{
		if(chunks.ContainsKey(coord))
			return;

		//from file
		ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
		if(chunk != null)
		{
			chunks.Add(coord, chunk);
			return;
		}

		chunks.Add(coord, new ChunkData(coord));
		chunks[coord].Populate();
	}

	public void AddToModifiedChunkList(ChunkData chunk)
	{
		// Only add to list if ChunkData is not already in the list.
		if(!modifiedChunks.Contains(chunk))
			modifiedChunks.Add(chunk);

	}

	bool IsVoxelInWorld(Vector3 pos)
	{
		return pos.x >= 0 && pos.x < Settings.Get.WorldSizeInVoxels &&
			pos.y >= 0 && pos.y < Settings.Get.ChunkHeight &&
			pos.z >= 0 && pos.z < Settings.Get.WorldSizeInVoxels;
	}

	public void SetVoxel(Vector3 pos, EBlockId value)
	{
		if(!IsVoxelInWorld(pos))
			return;

		Vector3Int coord = GetChunkCoord(pos);

		ChunkData chunk = RequestChunk(coord, true);

		Vector3Int voxel = new Vector3Int((int)(pos.x - coord.x), (int)(pos.y), (int)(pos.z - coord.z));

		chunk.map[voxel.x, voxel.y, voxel.z].id = value;
		AddToModifiedChunkList(chunk);
	}

	public VoxelState GetVoxel(Vector3 pos)
	{
		if(!IsVoxelInWorld(pos))
			return null;

		Vector3Int coord = GetChunkCoord(pos);

		ChunkData chunk = RequestChunk(coord, true);

		if(chunk == null)
			return null;

		// Then create a Vector3Int with the position of our voxel *within* the chunk.
		Vector3Int voxel = new Vector3Int((int)(pos.x - coord.x), (int)pos.y, (int)(pos.z - coord.z));

		// Then set the voxel in our chunk.
		return chunk.map[voxel.x, voxel.y, voxel.z];

	}

	private Vector3Int GetChunkCoord(Vector3 pos)
	{
		// Find out the ChunkCoord value of our voxel's chunk.
		int x = Mathf.FloorToInt(pos.x / Settings.Get.ChunkWidth);
		int z = Mathf.FloorToInt(pos.z / Settings.Get.ChunkWidth);

		// Then reverse that to get the position of the chunk.
		x *= Settings.Get.ChunkWidth;
		z *= Settings.Get.ChunkWidth;

		return new Vector3Int(x, 0, z);
	}
}
