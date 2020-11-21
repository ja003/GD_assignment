using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome")]
public class BiomeAttrributes : ScriptableObject
{
	public string biomeName;
	public int solidGroundHeight;
	public int terrainHeight;
	public float terrainScale;
	public Lode[] lodes;
}

[Serializable]
public class Lode
{
	public string nodeName;
	public byte blockID;
	public int minHeight;
	public int maxHeight;
	public float scale;
	public float threshold;
	public float noiseOffset;
}