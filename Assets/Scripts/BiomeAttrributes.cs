using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome")]
public class BiomeAttrributes : ScriptableObject
{
	public Material chunkMaterial;
	public int solidGroundHeight;
	public int terrainHeight;
	public float terrainScale;
	public int dirtThickness;
}

