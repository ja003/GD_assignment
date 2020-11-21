using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class World : MonoBehaviour
{
	public int seed;

	public Transform Player;
	public Vector3 spawnPosition;

	public Material material;

	[SerializeField]
	public BlockManager BlockManager;


	public BiomeAttrributes biome;
	[SerializeField] public Settings settings;

	Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

	List<Vector3Int> activeChunks = new List<Vector3Int>();

	public Vector3Int playerCoord;
	Vector3Int playerLastCoord;

	List<Vector3Int> chunksToCreate = new List<Vector3Int>();
	public List<Chunk> chunksToUpdate = new List<Chunk>();

	public GameObject debugScreen;

	bool isCreatingChunks;

	Thread ChunkUpdateThread;
	public object ChunkUpdateThreadLock = new object();


	private static World _instance;
	public static World Instance { get { return _instance; } }

	public WorldData worldData;

	public string appPath;

	private void Awake()
	{
		if(_instance != null && _instance != this)
			Destroy(gameObject);
		else
			_instance = this;
	}

	private void Start()
	{
		appPath = Application.persistentDataPath;

		worldData = SaveSystem.LoadWorld("XX");

		Random.InitState(seed);
		LoadWorld();

		GenerateWorld();
		spawnPosition = new Vector3(
			(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f,
			VoxelData.ChunkHeight / 2 - 50,
			(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);

		Player.position = spawnPosition;
		playerLastCoord = CoordConvertor.FloorToInt(Player.position);

		if(settings.enableThreading)
		{
			ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
			ChunkUpdateThread.Start();
		}

	}

	void Update()
	{
		playerCoord = CoordConvertor.FloorToInt(Player.position);

		if(!playerCoord.Equals(playerLastCoord))
		{
			playerLastCoord = playerCoord;
			CheckViewDistance();
		}

		if(chunksToCreate.Count > 0)
			CreateChunk();

		//if(chunksToDraw.Count > 0)
		//	chunksToDraw.Dequeue().CreateMesh();

		if(!settings.enableThreading)
		{
			//if(!applyingModifications)
			//	ApplyModifications();

			if(chunksToUpdate.Count > 0)
				UpdateChunks();

		}

		if(chunksToCreate.Count > 0 && !isCreatingChunks)
			StartCoroutine(CreateChunks());

		if(Input.GetKeyDown(KeyCode.F3))
			debugScreen.SetActive(!debugScreen.activeSelf);

		if(Input.GetKeyDown(KeyCode.F1))
			SaveSystem.SaveWorld(worldData);

	}

	//Vector3Int GetChunkCoord(Vector3 pos)
	//{
	//	int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
	//	int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
	//	return new ChunkCoord(x, z);
	//}

	public Chunk GetChunk(Vector3 pos)
	{
		int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
		int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
		return chunks[x, z];
	}

	void CheckViewDistance()
	{
		//ChunkCoord coord = GetChunkCoord(Player.position);
		playerLastCoord = playerCoord;

		List<Vector3Int> previouslyActiveChunks = new List<Vector3Int>(activeChunks);

		for(int x = playerCoord.x - VoxelData.ViewDistanceInChunks; x <= playerCoord.x + VoxelData.ViewDistanceInChunks; x++)
		{
			for(int z = playerCoord.z - VoxelData.ViewDistanceInChunks; z <= playerCoord.z + VoxelData.ViewDistanceInChunks; z++)
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

	void LoadWorld()
	{
		int mid = VoxelData.WorldSizeInChunks / 2;
		for(int x = mid - settings.LoadDistance; x < mid + settings.LoadDistance; x++)
		{
			for(int z = mid - settings.LoadDistance; z < mid + settings.LoadDistance; z++)
			{
				worldData.LoadChunk(new Vector3Int(x, 0, z));
			}
		}

	}

	void GenerateWorld()
	{
		int mid = VoxelData.WorldSizeInChunks / 2;
		for(int x = mid - VoxelData.ViewDistanceInChunks; x < mid + VoxelData.ViewDistanceInChunks; x++)
		{
			for(int z = mid - VoxelData.ViewDistanceInChunks; z < mid + VoxelData.ViewDistanceInChunks; z++)
			{
				Vector3Int coord = new Vector3Int(x, 0, z);
				chunks[x, z] = new Chunk(coord, true);
				activeChunks.Add(coord);
			}
		}

	}

	void CreateChunk()
	{
		Vector3Int c = chunksToCreate[0];
		chunksToCreate.RemoveAt(0);
		chunks[c.x, c.z].Init();

	}

	void UpdateChunks()
	{

		lock(ChunkUpdateThreadLock)
		{

			chunksToUpdate[0].UpdateChunk();
			if(!activeChunks.Contains(chunksToUpdate[0].coord))
				activeChunks.Add(chunksToUpdate[0].coord);
			chunksToUpdate.RemoveAt(0);

		}
	}

	void ThreadedUpdate()
	{

		while(true)
		{
			//if(!applyingModifications)
			//	ApplyModifications();

			if(chunksToUpdate.Count > 0)
				UpdateChunks();

		}

	}

	private void OnDisable()
	{

		if(settings.enableThreading)
		{
			ChunkUpdateThread.Abort();
		}

	}

	IEnumerator CreateChunks()
	{
		isCreatingChunks = true;

		while(chunksToCreate.Count > 0)
		{
			chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
			activeChunks.Add(chunksToCreate[0]);
			chunksToCreate.RemoveAt(0);
			yield return null;
		}

		isCreatingChunks = false;
	}

	public bool CheckForVoxel(float x, float y, float z)
	{
		return CheckForVoxel(new Vector3(x, y, z));
	}
	public bool CheckForVoxel(Vector3 pos)
	{
		VoxelState voxel = worldData.GetVoxel(pos);
		return voxel != null && BlockManager.IsSolid(voxel.id);
	}

	public EBlockId GetVoxel(Vector3 pos)
	{
		int yPos = Mathf.FloorToInt(pos.y);

		//immutable

		if(!IsVoxelInWorld(pos))
			return 0;

		// bottom block -> bedrock
		if(yPos == 0)
			return EBlockId.Bedrock; //todo: special

		//basic terrain

		int terrainHeight = Mathf.FloorToInt(
			biome.terrainHeight *
			Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) +
			biome.solidGroundHeight;

		EBlockId voxelValue;

		if(yPos == terrainHeight)
			voxelValue = EBlockId.Grass; //grass
		else if(yPos < terrainHeight && yPos > terrainHeight - 2)
			voxelValue = EBlockId.Dirt; //dirt
		else if(yPos > terrainHeight)
			return EBlockId.None; //air
		else
			voxelValue = EBlockId.Stone; //stone

		return voxelValue;
	}

	public VoxelState GetVoxelState(Vector3 pos)
	{
		return worldData.GetVoxel(pos);
	}


	bool IsChunkInWorld(Vector3Int pCoord)
	{
		return pCoord.x > 0 && pCoord.x < VoxelData.WorldSizeInChunks - 1 &&
			pCoord.z > 0 && pCoord.z < VoxelData.WorldSizeInChunks - 1;
	}

	bool IsVoxelInWorld(Vector3 pos)
	{
		return pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
			pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
			pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels;
	}
}

[System.Serializable]
public class BlockType
{
	public EBlockId Id;
	//public bool isSolid;

	[Header("Texture values")]
	public int backFaceTexture;
	public int frontFaceTexture;
	public int topFaceTexture;
	public int botFaceTexture;
	public int leftFaceTexture;
	public int rightFaceTexture;

	public int GetTexture(int faceIndex)
	{
		switch(faceIndex)
		{
			case 0:
				return backFaceTexture;
			case 1:
				return frontFaceTexture;
			case 2:
				return topFaceTexture;
			case 3:
				return botFaceTexture;
			case 4:
				return leftFaceTexture;
			case 5:
				return rightFaceTexture;
		}
		Debug.LogError("Cnat find texture");
		return 0;
	}


}

public enum EBlockId
{
	None,
	Grass,
	Dirt,
	Stone,
	Bedrock,

	Yellow,
	Blue,
	Pink,
	Orange,
}