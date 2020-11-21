using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public static class SaveSystem
{
	public static void SaveWorld(WorldData world)
	{
		string savePath = World.Instance.appPath + "/saves/" + world.worldName + "/";

		if(!Directory.Exists(savePath))
			Directory.CreateDirectory(savePath);

		Debug.Log("Saving " + world.worldName + " to " + savePath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);
		formatter.Serialize(stream, world);
		stream.Close();

		Thread thread = new Thread(() => SaveChunks(world));
		thread.Start();
	}

	public static void SaveChunks(WorldData world)
	{

		// Copy modified chunks into a new list and clear the old one to prevent
		// chunks being added to list while it is saving.
		List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
		world.modifiedChunks.Clear();

		// Loop through each chunk and save it.
		int count = 0;
		foreach(ChunkData chunk in chunks)
		{

			SaveSystem.SaveChunk(chunk, world.worldName);
			count++;

		}

		Debug.Log(count + " chunks saved.");

	}

	public static WorldData LoadWorld(string worldName, int seed = 0)
	{
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";
		WorldData world;
		if(File.Exists(loadPath + "world.world"))
		{
			Debug.Log("Loading " + worldName + " from " + loadPath);
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);
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

		string chunkName = chunk.position.x + "-" + chunk.position.y;

		// Set our save location and make sure we have a saves folder ready to go.
		string savePath = World.Instance.appPath + "/saves/" + worldName + "/chunks/";

		// If not, create it.
		if(!Directory.Exists(savePath))
			Directory.CreateDirectory(savePath);

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath + chunkName + ".chunk", FileMode.Create);
		Debug.Log("Saving chunk " + chunkName);

		formatter.Serialize(stream, chunk);
		stream.Close();

	}

	public static ChunkData LoadChunk(string worldName, Vector3Int position)
	{

		string chunkName = position.x + "-" + position.z;

		// Get the path to our world saves.
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName + ".chunk";

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
