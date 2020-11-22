using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public static class SaveSystem
{
	private static string worldDataPath;

	public static void SaveWorld(WorldData pWorld)
	{
		if(!Directory.Exists(worldDataPath))
			Directory.CreateDirectory(worldDataPath);

		Debug.Log("Saving " + pWorld.worldName + " to " + worldDataPath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(worldDataPath + "world.world", FileMode.Create);
		formatter.Serialize(stream, pWorld);
		stream.Close();

		//copy path to clipboard
		GUIUtility.systemCopyBuffer = worldDataPath;

		Thread thread = new Thread(() => SaveChunks(pWorld));
		thread.Start();
	}

	public static string DebugText;

	public static void SaveChunks(WorldData pWorld)
	{
		//only save modified chunks
		List<ChunkData> chunks = new List<ChunkData>(pWorld.modifiedChunks);
		pWorld.modifiedChunks.Clear();

		int count = 0;
		foreach(ChunkData chunk in chunks)
		{
			SaveChunk(chunk);
			count++;
			DebugText = $"{count}/{chunks.Count} chunks saved";
			Debug.Log(DebugText);
		}

		DebugText = count + " chunks saved. \n";
		DebugText += "Path copied to clipboard";
		Debug.Log(DebugText);
	}

	public static WorldData LoadWorld(string pWorldName, int seed = 0)
	{
		worldDataPath = Application.persistentDataPath + "/saves/" + pWorldName + "/";

		WorldData world;
		if(File.Exists(worldDataPath + "world.world"))
		{
			Debug.Log("Loading " + pWorldName + " from " + worldDataPath);
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(worldDataPath + "world.world", FileMode.Open);
			world = formatter.Deserialize(stream) as WorldData;
			stream.Close();
			return new WorldData(world);
		}

		Debug.Log("World " + pWorldName + " not found. Creating new.");

		world = new WorldData(pWorldName, seed);
		SaveWorld(world);
		return world;

	}

	public static void SaveChunk(ChunkData pChunk)
	{

		string chunkName = pChunk.coord.x + "-" + pChunk.coord.z;

		string saveChunkPath = worldDataPath + "chunks/";

		if(!Directory.Exists(saveChunkPath))
			Directory.CreateDirectory(saveChunkPath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(saveChunkPath + chunkName + ".chunk", FileMode.Create);
		//Debug.Log("Saving chunk " + chunkName);

		formatter.Serialize(stream, pChunk);
		stream.Close();

	}

	public static ChunkData LoadChunk(Vector3Int pCoord)
	{
		string chunkName = pCoord.x + "-" + pCoord.z;

		string loadPath = worldDataPath + "chunks/" + chunkName + ".chunk";

		if(File.Exists(loadPath))
		{
			Debug.Log("Loading chunk " + chunkName);

			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadPath, FileMode.Open);

			ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
			stream.Close();

			return chunkData;

		}

		return null;
	}
}
