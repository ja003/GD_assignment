using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController
{
	TerrainAttributes terrainAttributes => World.Instance.TerrainAttributes;

	public bool IsVoxelSolid(float pX, float pY, float pZ)
	{
		return IsVoxelSolid(new Vector3(pX, pY, pZ));
	}
	public bool IsVoxelSolid(Vector3 pPos)
	{
		EBlockId voxel = World.Instance.WorldData.GetVoxel(pPos);
		return World.Instance.BlockManager.IsSolid(voxel);
	}

	public EBlockId GetVoxel(Vector3 pPos)
	{
		int yPos = Mathf.FloorToInt(pPos.y);

		if(!IsVoxelInWorld(pPos))
			return 0;


		//undestructable block at the bottom
		if(yPos == 0)
			return EBlockId.Bedrock;

		int terrainHeight = Mathf.FloorToInt(
			terrainAttributes.terrainHeight *
			Noise.Get2DPerlin(pPos.x, pPos.z, 0, terrainAttributes.terrainScale)) +
			terrainAttributes.solidGroundHeight;

		//enhance terrain height difference
		Vector2 p = new Vector2(pPos.x, pPos.z);
		const int circlesCenterStep = 50;
		const int circleScaleCoeff = 30;

		//get center of 'hill'
		float x = (int)(pPos.x / circlesCenterStep) * circlesCenterStep + circlesCenterStep / 2;
		float z = (int)(pPos.z / circlesCenterStep) * circlesCenterStep + circlesCenterStep / 2;
		Vector2 c = new Vector2(x, z);
		if(Vector2.Distance(p, c) < circlesCenterStep / 2 - 5)
		{
			//enhance height based on distance from hill center
			float addition = 1 + 2 * Vector2.Distance(p, c) / circleScaleCoeff;

			//hills at odd indexes are reverted
			int indexX = Mathf.FloorToInt(c.x / (circlesCenterStep * 2));
			int indexY = Mathf.FloorToInt(c.y / (circlesCenterStep * 2));
			if(indexX % 2 == 1)
				addition *= -1;
			if(indexY % 2 == 1)
				addition *= -1;

			terrainHeight = (int)(terrainHeight + addition);
		}


		if(yPos < terrainHeight + 10 && IsVoxelAtWorldBorder(pPos))
			return EBlockId.Bedrock;

		EBlockId voxelValue;

		if(yPos == terrainHeight)
			voxelValue = EBlockId.Grass;
		else if(yPos < terrainHeight && yPos > terrainHeight - terrainAttributes.dirtThickness)
			voxelValue = EBlockId.Dirt;
		else if(yPos > terrainHeight)
			return EBlockId.None;
		else
			voxelValue = EBlockId.Stone;

		return voxelValue;
	}

	bool IsVoxelInWorld(Vector3 pPos)
	{
		return pPos.x >= 0 && pPos.x < Settings.Get.WorldSizeInVoxels &&
			pPos.y >= 0 && pPos.y < Settings.Get.ChunkHeight &&
			pPos.z >= 0 && pPos.z < Settings.Get.WorldSizeInVoxels;
	}

	bool IsVoxelAtWorldBorder(Vector3 pPos)
	{
		int borderSize = Settings.Get.ChunkWidth + 1;
		return pPos.x < borderSize || pPos.x > Settings.Get.WorldSizeInVoxels - borderSize ||
			pPos.z < borderSize || pPos.z > Settings.Get.WorldSizeInVoxels - borderSize;
	}
}
