using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
	public static float Get2DPerlin(float pX, float pY, float pOffset, float pScale)
	{
		return Mathf.PerlinNoise(
			(pX + 0.1f) / Settings.Get.ChunkWidth * pScale + pOffset, 
			(pY + 0.1f) / Settings.Get.ChunkWidth * pScale + pOffset);
	}
}
