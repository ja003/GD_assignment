using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockType
{
	public EBlockId Id;

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