using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunksController
{
	Chunk[,] chunks;
    List<Vector3Int> activeChunks = new List<Vector3Int>();

	List<Vector3Int> chunksToCreate = new List<Vector3Int>();

	public ChunksController()
	{
		chunks = new Chunk[Settings.Get.WorldSizeInChunks, Settings.Get.WorldSizeInChunks];
	}

	public Chunk GetChunk(Vector3 pPos)
	{
		Vector3Int coord = CoordConvertor.GetChunkCoord(pPos);
		return chunks[coord.x, coord.z];
	}

	public void CheckViewDistance(Vector3Int pPlayerCoord)
	{
		List<Vector3Int> previouslyActiveChunks = new List<Vector3Int>(activeChunks);

		for(int x = pPlayerCoord.x - Settings.Get.ViewDistanceInChunks; x <= pPlayerCoord.x + Settings.Get.ViewDistanceInChunks; x++)
		{
			for(int z = pPlayerCoord.z - Settings.Get.ViewDistanceInChunks; z <= pPlayerCoord.z + Settings.Get.ViewDistanceInChunks; z++)
			{
				Vector3Int c = new Vector3Int(x, 0, z);
				bool isInWorld = IsChunkInWorld(c);
				//Debug.Log($"{x},{z} isInWorld = {isInWorld}, player at {playerCoord}");
				if(isInWorld)
				{
					if(chunks[x, z] == null)
					{
						chunks[x, z] = new Chunk(c, false);
						chunksToCreate.Add(c);
					}
					else if(!chunks[x, z].IsActive)
					{
						chunks[x, z].IsActive = true;
						activeChunks.Add(c);
					}
				}

				for(int i = 0; i < previouslyActiveChunks.Count; i++)
				{
					Vector3Int prevC = previouslyActiveChunks[i];
					if(prevC.Equals(c))
						previouslyActiveChunks.RemoveAt(i);
				}
			}
		}

		foreach(var ch in previouslyActiveChunks)
		{
			chunks[ch.x, ch.z].IsActive = false;
			activeChunks.Remove(ch);
		}

	}

	public void CreateNextChunk()
	{
		if(chunksToCreate.Count == 0)
			return;

		Vector3Int c = chunksToCreate[0];
		chunksToCreate.RemoveAt(0);
		CreateChunkAt(c);
	}

	void CreateChunkAt(Vector3Int pCoord)
	{
		if(!IsChunkInWorld(pCoord))
			return;

		chunks[pCoord.x, pCoord.z] = new Chunk(pCoord, true);
		activeChunks.Add(pCoord);
	}

	bool IsChunkInWorld(Vector3Int pCoord)
	{
		return pCoord.x > 0 && pCoord.x < Settings.Get.WorldSizeInChunks &&
			pCoord.z > 0 && pCoord.z < Settings.Get.WorldSizeInChunks;
	}

	public void AddChunkToCreate(Vector3Int pPos)
	{
		chunksToCreate.Insert(0, pPos);
	}
}
