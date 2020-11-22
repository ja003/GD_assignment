using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public static class SaveSystem
{
	private static string worldDataPath;

	public static void SaveWorld(WorldData world)
	{
		if(!Directory.Exists(worldDataPath))
			Directory.CreateDirectory(worldDataPath);

		Debug.Log("Saving " + world.worldName + " to " + worldDataPath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(worldDataPath + "world.world", FileMode.Create);
		formatter.Serialize(stream, world);
		stream.Close();

		//copy path to clipboard
		GUIUtility.systemCopyBuffer = worldDataPath;

		Thread thread = new Thread(() => SaveChunks(world));
		thread.Start();
	}

	public static string debugText;

	public static void SaveChunks(WorldData world)
	{
		//only save modified chunks
		List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
		world.modifiedChunks.Clear();

		int count = 0;
		foreach(ChunkData chunk in chunks)
		{
			SaveChunk(chunk, world.worldName);
			count++;
			debugText = $"{count}/{chunks.Count} chunks saved";
			Debug.Log(debugText);
		}

		debugText = count + " chunks saved. \n";
		debugText += "Path copied to clipboard";
		Debug.Log(debugText);
	}

	public static WorldData LoadWorld(string worldName, int seed = 0)
	{
		worldDataPath = Application.persistentDataPath + "/saves/" + worldName + "/";

		WorldData world;
		if(File.Exists(worldDataPath + "world.world"))
		{
			Debug.Log("Loading " + worldName + " from " + worldDataPath);
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(worldDataPath + "world.world", FileMode.Open);
			world = formatter.Deserialize(stream) as WorldData;
			stream.Close();
			return new WorldData(world);
		}

		Debug.Log("World " + worldName + " not found. Creating new.");

		world = new WorldData(worldName, seed);
		SaveWorld(world);
		return world;

	}

	public static void SaveChunk(ChunkData chunk, string worldName)
	{

		string chunkName = chunk.coord.x + "-" + chunk.coord.z;

		// Set our save location and make sure we have a saves folder ready to go.
		string saveChunkPath = worldDataPath + "chunks/";

		// If not, create it.
		if(!Directory.Exists(saveChunkPath))
			Directory.CreateDirectory(saveChunkPath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(saveChunkPath + chunkName + ".chunk", FileMode.Create);
		//Debug.Log("Saving chunk " + chunkName);

		formatter.Serialize(stream, chunk);
		stream.Close();

	}

	public static ChunkData LoadChunk(string worldName, Vector3Int pCoord)
	{
		string chunkName = pCoord.x + "-" + pCoord.z;

		// Get the path to our world saves.
		string loadPath = worldDataPath + "chunks/" + chunkName + ".chunk";

		// Check if a save exists for the name we were passed.
		if(File.Exists(loadPath))
		{
			Debug.Log("Loading chunk " + chunkName);

			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadPath, FileMode.Open);

			ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
			stream.Close();

			return chunkData;

		}

		// If we didn't find the chunk in our folder, return null and our WorldData script
		// will make a new one.
		return null;

	}

}
