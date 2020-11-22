using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public Vector3Int coord;

	GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();

	//public byte[,,] voxelMap = new byte[Settings.Get.ChunkWidth, Settings.Get.ChunkHeight, Settings.Get.ChunkWidth];

	//public VoxelState[,,] voxelMap => chunkData.map;

	ChunkData chunkData;
	//World world;

	bool isActive;
	public bool isVoxelMapPopulated;

	public Vector3 position => chunkObject.transform.position;

	public Chunk(Vector3Int pCoord, bool generateOnLoad)
	{
		coord = pCoord;
		IsActive = true;

		if(generateOnLoad)
			Init();
	}

	private void Init()
	{
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		meshRenderer.material = World.Instance.biome.chunkMaterial;
		chunkObject.transform.SetParent(World.Instance.transform);
		chunkObject.transform.position = new Vector3(coord.x * Settings.Get.ChunkWidth, 0, coord.z * Settings.Get.ChunkWidth);
		chunkObject.name = $"Chunk_{coord.x},{coord.z}";

		//chunkData = World.Instance.worldData.RequestChunk(new Vector3Int((int)position.x, 0, (int)position.z), true);
		chunkData = World.Instance.worldData.RequestChunk(coord, true);

		UpdateChunk();
	}

	public void UpdateChunk()
	{
		ClearMeshData();

		for(int y = 0; y < Settings.Get.ChunkHeight; y++)
		{
			for(int x = 0; x < Settings.Get.ChunkWidth; x++)
			{
				for(int z = 0; z < Settings.Get.ChunkWidth; z++)
				{
					if(World.Instance.BlockManager.IsSolid(chunkData.map[x, y, z].id))
						UpdateMeshData(new Vector3(x, y, z));
				}
			}
		}

		CreateMesh();
	}

	void ClearMeshData()
	{
		vertexIndex = 0;
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
	}

	public bool IsActive
	{
		get { return isActive; }
		set
		{
			isActive = value;
			chunkObject?.SetActive(value);
			//Debug.Log($"SetActive {coord} = {value}");
		}
	}

	public bool IsVoxelInChunk(Vector3 pos)
	{
		return IsVoxelInChunk(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
	}

	public bool IsVoxelInChunk(int x, int y, int z)
	{
		if(x < 0 || x > Settings.Get.ChunkWidth - 1)
			return false;
		if(y < 0 || y > Settings.Get.ChunkHeight - 1)
			return false;
		if(z < 0 || z > Settings.Get.ChunkWidth - 1)
			return false;
		return true;
	}

	public void TryDestroyVoxel(Vector3 pPos, float pMouseHoldTime)
	{
		int xCheck = Mathf.FloorToInt(pPos.x);
		int yCheck = Mathf.FloorToInt(pPos.y);
		int zCheck = Mathf.FloorToInt(pPos.z);


		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		EBlockId id = chunkData.map[xCheck, yCheck, zCheck].id;
		float duration = World.Instance.BlockManager.GetDuration(id);
		string durationText = id == EBlockId.Bedrock ? "undestructable" : duration.ToString("0.0");
		string debugText = $"Try destroy: {pMouseHoldTime:0.0}/{durationText}";
		World.Instance.DebugScreen.DestroyText = debugText;
		Debug.Log(debugText);
		if(duration < pMouseHoldTime)
			EditVoxel(pPos, EBlockId.None);
	}

	public void EditVoxel(Vector3 pos, EBlockId newID)
	{
		int xCheck = Mathf.FloorToInt(pos.x);
		int yCheck = Mathf.FloorToInt(pos.y);
		int zCheck = Mathf.FloorToInt(pos.z);


		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		chunkData.map[xCheck, yCheck, zCheck].id = newID;

		World.Instance.worldData.AddToModifiedChunkList(chunkData);

		UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
		UpdateChunk();
	}

	void UpdateSurroundingVoxels(int x, int y, int z)
	{
		Vector3 thisVoxel = new Vector3(x, y, z);

		for(int p = 0; p < 6; p++)
		{
			Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];
			if(!IsVoxelInChunk(currentVoxel))
			{
				//World.Instance.ChunksController.AddChunkToUpdate(currentVoxel + position);
				//World.Instance.ChunksController.chunksToUpdate.Insert(0, World.Instance.ChunksController.GetChunk(currentVoxel + position));
				World.Instance.ChunksController.GetChunk(thisVoxel + position).UpdateChunk();
			}
		}
	}

	public EBlockId GetVoxelFromGlobalVec3(Vector3 pos)
	{
		int xCheck = Mathf.FloorToInt(pos.x);
		int yCheck = Mathf.FloorToInt(pos.y);
		int zCheck = Mathf.FloorToInt(pos.z);

		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		return chunkData.map[xCheck, yCheck, zCheck].id;
	}

	bool CheckVoxel(Vector3 pos)
	{
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);

		if(!IsVoxelInChunk(x, y, z))
			return World.Instance.CheckForVoxel(pos + position);

		return World.Instance.BlockManager.IsSolid(chunkData.map[x, y, z].id);
	}

	//void PopulateVoxelMap()
	//{
	//	for(int y = 0; y < Settings.Get.ChunkHeight; y++)
	//	{
	//		for(int x = 0; x < Settings.Get.ChunkWidth; x++)
	//		{
	//			for(int z = 0; z < Settings.Get.ChunkWidth; z++)
	//			{
	//				voxelMap[x, y, z] = World.Instance.GetVoxel(new Vector3(x, y, z) + position);
	//			}
	//		}
	//	}
	//	isVoxelMapPopulated = true;
	//}

	private void CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	private void UpdateMeshData(Vector3 pos)
	{
		for(int p = 0; p < 6; p++)
		{
			if(!CheckVoxel(pos + VoxelData.faceChecks[p]))
			{
				EBlockId blockID = chunkData.map[(int)pos.x, (int)pos.y, (int)pos.z].id;

				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

				AddTexture(World.Instance.BlockManager.GetTexture(blockID, p));

				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 3);
				vertexIndex += 4;
			}
		}
	}

	void AddTexture(int textureID)
	{
		float y = textureID / Settings.Get.TextureAtlasSizeInBlocks;
		float x = textureID - (y * Settings.Get.TextureAtlasSizeInBlocks);

		x *= Settings.Get.NormalizedBlockTextureSize;
		y *= Settings.Get.NormalizedBlockTextureSize;

		y = 1 - y - Settings.Get.NormalizedBlockTextureSize;

		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + Settings.Get.NormalizedBlockTextureSize));
		uvs.Add(new Vector2(x + Settings.Get.NormalizedBlockTextureSize, y));
		uvs.Add(new Vector2(x + Settings.Get.NormalizedBlockTextureSize, y + Settings.Get.NormalizedBlockTextureSize));
	}
}

[Serializable]
public class VoxelState
{
	public EBlockId id;

	public VoxelState()
	{
		id = 0;
	}

	public VoxelState(EBlockId _id)
	{
		id = _id;
	}
}