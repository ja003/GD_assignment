using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
	[SerializeField]
	private List<BlockType> _blockTypes;
	private Dictionary<EBlockId, BlockType> blockTypes = new Dictionary<EBlockId, BlockType>();

	private void Awake()
	{
		foreach(var bt in _blockTypes)
		{
			blockTypes.Add(bt.Id, bt);
		}
	}

	internal bool IsSolid(EBlockId pId)
	{
		return pId != EBlockId.None;
	}

	internal int GetTexture(EBlockId pId, int pFace)
	{
		return blockTypes[pId].GetTexture(pFace);
	}

	internal float GetDuration(EBlockId pId)
	{
		switch(pId)
		{
			case EBlockId.Yellow: return 0.2f;
			case EBlockId.Blue: return 0.5f;
			case EBlockId.Pink: return 1f;
			case EBlockId.Orange: return 2f;

			case EBlockId.Bedrock: return int.MaxValue;

		}
		return 0;
	}
}
