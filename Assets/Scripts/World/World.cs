using System;
using UnityEngine;

public class World : MonoBehaviour
{
	public static World Instance { get; private set; }

	[SerializeField] public Player Player;

	[SerializeField] public BlockManager BlockManager;
	[SerializeField] public TerrainAttributes TerrainAttributes;
	[SerializeField] public Settings settings;
	public ChunksController ChunksController;
	public VoxelController VoxelController;

	[NonSerialized] public Vector3Int PlayerCoord;
	Vector3Int playerLastCoord;

	[SerializeField] public DebugScreen DebugScreen;

	[NonSerialized] public WorldData WorldData;

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
		VoxelController = new VoxelController();

		WorldData = SaveSystem.LoadWorld("test_world");

		Vector3 spawnPosition = new Vector3(
			Settings.Get.WorldCentre,
			//Settings.Get.ChunkHeight / 2 - 50,
			TerrainAttributes.solidGroundHeight + 10,
			Settings.Get.WorldCentre);

		Player.transform.position = spawnPosition;
		playerLastCoord = CoordConvertor.GetChunkCoord(Player.transform.position);

		LoadWorld();

		GenerateWorld();
	}

	void Update()
	{
		PlayerCoord = CoordConvertor.GetChunkCoord(Player.transform.position);

		if(!PlayerCoord.Equals(playerLastCoord))
		{
			playerLastCoord = PlayerCoord;
			ChunksController.CheckViewDistance(PlayerCoord);
		}

		ChunksController.CreateNextChunk();

		if(Input.GetKeyDown(KeyCode.F3))
			DebugScreen.gameObject.SetActive(!DebugScreen.gameObject.activeSelf);

		if(Input.GetKeyDown(KeyCode.F1))
			SaveSystem.SaveWorld(WorldData);
	}

	void LoadWorld()
	{
		int mid = Settings.Get.WorldSizeInChunks / 2;
		for(int x = mid - Settings.Get.LoadDistance; x < mid + Settings.Get.LoadDistance; x++)
		{
			for(int z = mid - Settings.Get.LoadDistance; z < mid + Settings.Get.LoadDistance; z++)
			{
				WorldData.LoadChunk(new Vector3Int(x, 0, z));
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
			}
		}
	}
}