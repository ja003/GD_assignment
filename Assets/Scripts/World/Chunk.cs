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
    List<Vector3> normals = new List<Vector3>();
	ChunkData chunkData;

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
		meshRenderer.receiveShadows = false;

		meshRenderer.material = World.Instance.TerrainAttributes.chunkMaterial;
		chunkObject.transform.SetParent(World.Instance.transform);
		chunkObject.transform.position = new Vector3(coord.x * Settings.Get.ChunkWidth, 0, coord.z * Settings.Get.ChunkWidth);
		chunkObject.name = $"Chunk_{coord.x},{coord.z}";

		chunkData = World.Instance.WorldData.RequestChunk(coord);

		UpdateChunk();
	}

	public void UpdateChunk()
	{
		//Debug.Log($"Update chunk {coord}");

		ClearMeshData();

		for(int y = 0; y < Settings.Get.ChunkHeight; y++)
		{
			for(int x = 0; x < Settings.Get.ChunkWidth; x++)
			{
				for(int z = 0; z < Settings.Get.ChunkWidth; z++)
				{
					if(World.Instance.BlockManager.IsSolid(chunkData.map[x, y, z]))
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
		normals.Clear();
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

	public bool IsVoxelInChunk(Vector3 pPos)
	{
		return IsVoxelInChunk(Mathf.FloorToInt(pPos.x), Mathf.FloorToInt(pPos.y), Mathf.FloorToInt(pPos.z));
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

		EBlockId id = chunkData.map[xCheck, yCheck, zCheck];
		float duration = World.Instance.BlockManager.GetDuration(id);
		string durationText = id == EBlockId.Bedrock ? "undestructable" : duration.ToString("0.0");
		string debugText = $"Try destroy: {pMouseHoldTime:0.0}/{durationText}";
		World.Instance.DebugScreen.DestroyText = debugText;
		Debug.Log(debugText);
		if(duration < pMouseHoldTime)
			EditVoxel(pPos, EBlockId.None);
	}

	public void EditVoxel(Vector3 pPos, EBlockId pNewID)
	{
		int xCheck = Mathf.FloorToInt(pPos.x);
		int yCheck = Mathf.FloorToInt(pPos.y);
		int zCheck = Mathf.FloorToInt(pPos.z);


		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		chunkData.map[xCheck, yCheck, zCheck] = pNewID;

		World.Instance.WorldData.AddToModifiedChunkList(chunkData);

		UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
		UpdateChunk();
	}

	void UpdateSurroundingVoxels(int x, int y, int z)
	{
		Vector3 thisVoxel = new Vector3(x, y, z);

		for(int p = 0; p < 6; p++)
		{
			Vector3 currentVoxel = thisVoxel + VoxelData.FaceChecks[p];
			if(!IsVoxelInChunk(currentVoxel))
			{
				World.Instance.ChunksController.GetChunk(currentVoxel + position).UpdateChunk();
			}
		}
	}

	public EBlockId GetVoxelFromGlobalVec3(Vector3 pPos)
	{
		int xCheck = Mathf.FloorToInt(pPos.x);
		int yCheck = Mathf.FloorToInt(pPos.y);
		int zCheck = Mathf.FloorToInt(pPos.z);

		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		return chunkData.map[xCheck, yCheck, zCheck];
	}

	bool IsVoxelSolid(Vector3 pPos)
	{
		int x = Mathf.FloorToInt(pPos.x);
		int y = Mathf.FloorToInt(pPos.y);
		int z = Mathf.FloorToInt(pPos.z);

		if(!IsVoxelInChunk(x, y, z))
			return World.Instance.VoxelController.IsVoxelSolid(pPos + position);

		return World.Instance.BlockManager.IsSolid(chunkData.map[x, y, z]);
	}

	private void CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.normals = normals.ToArray();
		meshFilter.mesh = mesh;
	}

	private void UpdateMeshData(Vector3 pPos)
	{
		for(int p = 0; p < 6; p++)
		{
			if(!IsVoxelSolid(pPos + VoxelData.FaceChecks[p]))
			{
				EBlockId blockID = chunkData.map[(int)pPos.x, (int)pPos.y, (int)pPos.z];

				vertices.Add(pPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 0]]);
				vertices.Add(pPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 1]]);
				vertices.Add(pPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 2]]);
				vertices.Add(pPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 3]]);

				for(int i = 0; i < 4; i++)
					normals.Add(VoxelData.FaceChecks[p]);

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

	void AddTexture(int pTextureID)
	{
		float y = pTextureID / Settings.Get.TextureAtlasSizeInBlocks;
		float x = pTextureID - (y * Settings.Get.TextureAtlasSizeInBlocks);

		x *= Settings.Get.NormalizedBlockTextureSize;
		y *= Settings.Get.NormalizedBlockTextureSize;

		y = 1 - y - Settings.Get.NormalizedBlockTextureSize;

		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + Settings.Get.NormalizedBlockTextureSize));
		uvs.Add(new Vector2(x + Settings.Get.NormalizedBlockTextureSize, y));
		uvs.Add(new Vector2(x + Settings.Get.NormalizedBlockTextureSize, y + Settings.Get.NormalizedBlockTextureSize));
	}
}