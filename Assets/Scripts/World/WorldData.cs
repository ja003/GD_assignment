using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WorldData
{
	public string worldName;
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

	public ChunkData RequestChunk(Vector3Int pCoord)
	{
		ChunkData c;

		if(chunks.ContainsKey(pCoord))
			c = chunks[pCoord];
		else
		{
			LoadChunk(pCoord);
			c = chunks[pCoord];
		}
		return c;

	}

	internal void LoadChunk(Vector3Int coord)
	{
		if(chunks.ContainsKey(coord))
			return;

		//from file
		ChunkData chunk = SaveSystem.LoadChunk(coord);
		//Debug.Log($"Load chunk at {coord} = {chunk}");
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

	//public void SetVoxel(Vector3 pos, EBlockId value)
	//{
	//	if(!IsVoxelInWorld(pos))
	//		return;

	//	Vector3Int coord = GetChunkCoord(pos);

	//	ChunkData chunk = RequestChunk(coord, true);

	//	Vector3Int voxel = new Vector3Int((int)(pos.x - coord.x), (int)(pos.y), (int)(pos.z - coord.z));

	//	chunk.map[voxel.x, voxel.y, voxel.z].id = value;
	//	AddToModifiedChunkList(chunk);
	//}

	public EBlockId GetVoxel(Vector3 pos)
	{
		if(!IsVoxelInWorld(pos))
			return EBlockId.None;

		Vector3Int coord = GetChunkCoord(pos);

		ChunkData chunk = RequestChunk(coord);

		if(chunk == null)
			return EBlockId.None;

		// Then create a Vector3Int with the position of our voxel *within* the chunk.
		//Vector3Int voxel = new Vector3Int((int)(pos.x - coord.x), (int)pos.y, (int)(pos.z - coord.z));
		Vector3Int voxel = new Vector3Int((int)(pos.x - coord.x * Settings.Get.ChunkWidth), (int)pos.y, (int)(pos.z - coord.z * Settings.Get.ChunkWidth));
		//Vector3Int voxel = new Vector3Int((int)(pos.x), (int)pos.y, (int)(pos.z));

		// Then set the voxel in our chunk.
		return chunk.map[voxel.x, voxel.y, voxel.z];

	}

	private Vector3Int GetChunkCoord(Vector3 pos)
	{
		// Find out the ChunkCoord value of our voxel's chunk.
		int x = Mathf.FloorToInt(pos.x / Settings.Get.ChunkWidth);
		int z = Mathf.FloorToInt(pos.z / Settings.Get.ChunkWidth);

		// Then reverse that to get the position of the chunk.
		//x *= Settings.Get.ChunkWidth;
		//z *= Settings.Get.ChunkWidth;

		return new Vector3Int(x, 0, z);
	}
}
