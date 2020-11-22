using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainAttributes", menuName = "Minecraft/Terrain")]
public class TerrainAttributes : ScriptableObject
{
	public Material chunkMaterial;
	public int solidGroundHeight;
	public int terrainHeight;
	public float terrainScale;
	public int dirtThickness;
}

