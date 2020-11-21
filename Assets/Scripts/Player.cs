using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
	private const int PLAYER_HEIGHT = 1;
	public float sprintSpeed = 6f;
	public float walkSpeed = 3f;
	public float jumpForce = 4f;
	public float gravity = -9.8f;

	public float playerWidth = 0.15f;
	public float boundsTolerance = 0.1f;

	float horizontal;
	float vertical;
	float mouseHorizontal;
	float mouseVertical;
	[SerializeField] float mouseHorizontalSensitivity = 20;
	[SerializeField] float mouseVerticalSensitivity = 20;
	Vector3 velocity;
	float verticalMomentum;
	bool jumpRequest;

	List<EBlockId> inventory = new List<EBlockId>();
	[SerializeField] Text selectedBlockText;
	public int selectedBlockIndex = 0;

	private bool isSprinting;
	private bool isGrounded;

	[SerializeField] World world;
	[SerializeField] Transform cam;

	[SerializeField] Transform highlightBlock;
	[SerializeField] Transform PlaceBlock;



	public float checkIncrement = 0.1f;
	public float reach = 8f;

	private void Start()
	{
		UnityEngine.Cursor.lockState = CursorLockMode.Locked;
		selectedBlockText.text = "";

		inventory.Add(EBlockId.Yellow);
		inventory.Add(EBlockId.Orange);
		inventory.Add(EBlockId.Pink);
		inventory.Add(EBlockId.Blue);
	}

	void FixedUpdate()
	{
		CalculateVelocity();
		if(jumpRequest)
			Jump();

		transform.Rotate(Vector3.up * mouseHorizontal);
		cam.Rotate(Vector3.right * -mouseVertical);
		transform.Translate(velocity, Space.World);
	}

	void Update()
	{
		GetPlayerInputs();
		PlaceCursorBlock();
	}

	void Jump()
	{
		verticalMomentum = jumpForce;
		isGrounded = false;
		jumpRequest = false;
	}

	private void CalculateVelocity()
	{
		if(verticalMomentum > gravity)
			verticalMomentum += Time.fixedDeltaTime * gravity;

		if(isSprinting)
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
		else
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

		velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

		if((velocity.z > 0 && front) || (velocity.z < 0 && back))
			velocity.z = 0;
		if((velocity.x > 0 && right) || (velocity.x < 0 && left))
			velocity.x = 0;

		if(velocity.y < 0)
			velocity.y = CheckDownSpeed(velocity.y);
		else if(velocity.y > 0)
			velocity.y = CheckUpSpeed(velocity.y);
	}

	private float CheckDownSpeed(float downSpeed)
	{
		if(
			world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
			world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
			world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
			world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)
			)
		{
			isGrounded = true;
			return 0;
		}
		else
		{
			isGrounded = false;
			return downSpeed;
		}
	}

	private float CheckUpSpeed(float upSpeed)
	{
		if(
			world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2 + upSpeed, transform.position.z - playerWidth) ||
			world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2 + upSpeed, transform.position.z - playerWidth) ||
			world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2 + upSpeed, transform.position.z + playerWidth) ||
			world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2 + upSpeed, transform.position.z + playerWidth)
			)
		{
			return 0;
		}
		else
		{
			return upSpeed;
		}
	}

	bool disableInput;
	float mouseDownTime;

	private void GetPlayerInputs()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			disableInput = !disableInput;

		if(disableInput)
			return;

		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		mouseHorizontal = Input.GetAxis("Mouse X") * mouseHorizontalSensitivity;
		mouseVertical = Input.GetAxis("Mouse Y") * mouseVerticalSensitivity;

		if(Input.GetKeyDown(KeyCode.LeftShift))
			isSprinting = true;
		if(Input.GetKeyUp(KeyCode.LeftShift))
			isSprinting = false;

		if(isGrounded && Input.GetButtonDown("Jump"))
			jumpRequest = true;

		float scroll = Input.GetAxis("Mouse ScrollWheel");

		if(scroll != 0)
		{
			if(scroll > 0)
				selectedBlockIndex++;
			if(scroll < 0)
				selectedBlockIndex--;

			selectedBlockIndex = selectedBlockIndex % inventory.Count;
			selectedBlockText.text = inventory[selectedBlockIndex] + " block selected";
		}

		if(highlightBlock.gameObject.activeSelf)
		{
			if(Input.GetMouseButtonDown(0))
			{
				mouseDownTime = Time.time;
			}

			else if(Input.GetMouseButtonUp(0))
			{
				world.GetChunk(highlightBlock.position).TryDestroyVoxel(highlightBlock.position, Time.time - mouseDownTime);
			}
		}

		if(Input.GetMouseButtonDown(1))
		{
			world.GetChunk(PlaceBlock.position).EditVoxel(PlaceBlock.position, inventory[selectedBlockIndex]);
		}
	}

	void PlaceCursorBlock()
	{
		float step = checkIncrement;
		Vector3 lastPos = new Vector3();

		while(step < reach)
		{
			Vector3 pos = cam.position + (cam.forward * step);
			if(world.CheckForVoxel(pos))
			{
				highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
				PlaceBlock.position = lastPos;

				highlightBlock.gameObject.SetActive(true);
				PlaceBlock.gameObject.SetActive(true);

				return;
			}

			lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
			step += checkIncrement;
		}

		highlightBlock.gameObject.SetActive(false);
		PlaceBlock.gameObject.SetActive(false);
	}

	private bool CheckVoxel(float pXOfset, float pYOfset, float pZOfset)
	{
		return CheckVoxel(new Vector3(pXOfset, pYOfset, pZOfset));
	}
	private bool CheckVoxel(Vector3 pOffset)
	{
		return world.CheckForVoxel(transform.position + pOffset);
	}

	public bool front => CheckVoxel(0, 0, playerWidth) || CheckVoxel(0, PLAYER_HEIGHT, playerWidth);
	public bool back => CheckVoxel(0, 0, -playerWidth) || CheckVoxel(0, PLAYER_HEIGHT, -playerWidth);
	public bool left => CheckVoxel(-playerWidth, 0, 0) || CheckVoxel(-playerWidth, PLAYER_HEIGHT, 0);
	public bool right => CheckVoxel(playerWidth, 0, 0) || CheckVoxel(playerWidth, PLAYER_HEIGHT, 0);
}
