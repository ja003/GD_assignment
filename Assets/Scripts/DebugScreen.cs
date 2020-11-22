using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
	[SerializeField] World world;
	[SerializeField] Text controls;
	[SerializeField] Text info;

	float frameRate;
	float timer;

	public string DestroyText;

	private void Start()
	{
		string controlsText = "CONTROLS\n";
		controlsText += $"Press F1 to save\n";
		controlsText += $"Press F3 to hide UI\n";
		controlsText += $"Scroll to change item\n";
		controls.text = controlsText;
	}

	private void Update()
	{
		string infoText = "INFO\n";

		if(timer > 1)
		{
			frameRate = (int)(1f / Time.unscaledDeltaTime);
			timer = 0;
		}
		else
			timer += Time.deltaTime;

		infoText += "framerate = " + frameRate + "\n";
		infoText += $"chunk = [{world.PlayerCoord.x},{world.PlayerCoord.z}]\n";
		infoText += $"{SaveSystem.DebugText}\n";
		infoText += $"{DestroyText}\n";

		info.text = infoText;
	}
}
