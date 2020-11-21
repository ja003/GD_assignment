using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
	[SerializeField] World world;
	[SerializeField] Text text;

	float frameRate;
	float timer;

	private void Update()
	{
		string debugText = "DEBUG\n";


		if(timer > 1)
		{
			frameRate = (int)(1f / Time.unscaledDeltaTime);
			timer = 0;
		}
		else
			timer += Time.deltaTime;

		debugText += "framerate = " + frameRate + "\n";
		Vector3 playerPos = world.Player.position;
		debugText += $"XYZ = [{(int)playerPos.x},{(int)playerPos.y},{(int)playerPos.z}]\n";
		debugText += $"chunk = {world.playerCoord}\n";

		text.text = debugText;

	}
}
