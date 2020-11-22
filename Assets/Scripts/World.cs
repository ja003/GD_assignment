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

	[SerializeField]
	public BlockManager BlockManager;


	[SerializeField] public BiomeAttrributes biome;
	[SerializeField] public Settings settings;

	public ChunksController ChunksController;



	public Vector3Int playerCoord;
	Vector3Int playerLastCoord;


	public DebugScreen DebugScreen;

	public static World Instance { get; private set; }

	public WorldData worldData;


	private void Awake()
	{
		if(Instance != null && Instance != this)
			Destroy(gameObject);
		else
			Instance = this;

	}

	private void Start()
	{

		ChunksController = new ChunksController();
		worldData = SaveSystem.LoadWorld("XX");


		spawnPosition = new Vector3(
			Settings.Get.WorldCentre,
			Settings.Get.ChunkHeight / 2 - 50,
			Settings.Get.WorldCentre);

		Player.position = spawnPosition;
		playerLastCoord = CoordConvertor.GetChunkCoord(Player.position);

		Random.InitState(seed);
		LoadWorld();

		GenerateWorld();

	}

	void Update()
	{
		playerCoord = CoordConvertor.GetChunkCoord(Player.position);

		if(!playerCoord.Equals(playerLastCoord))
		{
			playerLastCoord = playerCoord;
			ChunksController.CheckViewDistance(playerCoord);
		}

		ChunksController.CreateNextChunk();

		if(Input.GetKeyDown(KeyCode.F3))
			DebugScreen.gameObject.SetActive(!DebugScreen.gameObject.activeSelf);

		if(Input.GetKeyDown(KeyCode.F1))
			SaveSystem.SaveWorld(worldData);

	}

	

	

	void LoadWorld()
	{
		int mid = Settings.Get.WorldSizeInChunks / 2;
		for(int x = mid - Settings.Get.LoadDistance; x < mid + Settings.Get.LoadDistance; x++)
		{
			for(int z = mid - Settings.Get.LoadDistance; z < mid + Settings.Get.LoadDistance; z++)
			{
				worldData.LoadChunk(new Vector3Int(x, 0, z));
			}
		}

	}

	void GenerateWorld()
	{
		int mid = Settings.Get.WorldSizeInChunks / 2;
		int viewDist = Settings.Get.ViewDistanceInChunks;
		for(int x = mid - viewDist; x < mid + viewDist; x++)
		{
			for(int z = mid - viewDist; z < mid + viewDist; z++)
			{
				Vector3Int coord = new Vector3Int(x, 0, z);
				ChunksController.AddChunkToCreate(coord);				
				//ChunksController.CreateChunkAt(coord);				
			}
		}

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

		if(!IsVoxelInWorld(pos))
			return 0;


		//undestructable block at the bottom
		if(yPos == 0)
			return EBlockId.Bedrock; 

		int terrainHeight = Mathf.FloorToInt(
			biome.terrainHeight *
			Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) +
			biome.solidGroundHeight;


		if(yPos < terrainHeight + 10 && IsVoxelAtWorldBorder(pos))
			return EBlockId.Bedrock;

		EBlockId voxelValue;

		if(yPos == terrainHeight)
			voxelValue = EBlockId.Grass;
		else if(yPos < terrainHeight && yPos > terrainHeight - biome.dirtThickness)
			voxelValue = EBlockId.Dirt; 
		else if(yPos > terrainHeight)
			return EBlockId.None; 
		else
			voxelValue = EBlockId.Stone; 

		return voxelValue;
	}

	public VoxelState GetVoxelState(Vector3 pos)
	{
		return worldData.GetVoxel(pos);
	}

	bool IsVoxelInWorld(Vector3 pos)
	{
		return pos.x >= 0 && pos.x < Settings.Get.WorldSizeInVoxels &&
			pos.y >= 0 && pos.y < Settings.Get.ChunkHeight &&
			pos.z >= 0 && pos.z < Settings.Get.WorldSizeInVoxels;
	}

	bool IsVoxelAtWorldBorder(Vector3 pos)
	{
		int borderSize = settings.ChunkWidth + 1;
		return pos.x < borderSize || pos.x > Settings.Get.WorldSizeInVoxels - borderSize ||
			pos.z < borderSize || pos.z > Settings.Get.WorldSizeInVoxels - borderSize;
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