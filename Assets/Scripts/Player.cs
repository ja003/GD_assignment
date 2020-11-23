using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	private const int PLAYER_HEIGHT = 2;
	[SerializeField] float sprintSpeed = 6f;
	[SerializeField] float walkSpeed = 3f;
	[SerializeField] float jumpForce = 4f;
	[SerializeField] float gravity = -9.8f;

	[SerializeField] float playerWidth = 0.15f;
	[SerializeField] float boundsTolerance = 0.1f;

	float horizontal;
	float vertical;
	float mouseHorizontal;
	float mouseVertical;
	[SerializeField] public float mouseSensitivity = 20;
	Vector3 velocity;
	float verticalMomentum;
	bool jumpRequest;

	List<EBlockId> inventory = new List<EBlockId>();
	[SerializeField] Text selectedBlockText;
	int selectedBlockIndex = 0;

	[SerializeField] Image hitStrengthBar;

	private bool isSprinting;
	private bool isGrounded;

	[SerializeField] World world;
	[SerializeField] Transform cam;

	[SerializeField] Transform highlightDestroy;
	[SerializeField] Transform highlightPlace;

	[SerializeField] float checkIncrement = 0.1f;
	[SerializeField] float reach = 8f;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		selectedBlockText.text = "";

		inventory.Add(EBlockId.Yellow);
		inventory.Add(EBlockId.Blue);
		inventory.Add(EBlockId.Pink);
		inventory.Add(EBlockId.Orange);

		UpdateSelectedBlock();
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
		UpdateHitStrengthBar();
	}

	private void UpdateHitStrengthBar()
	{
		bool active = highlightDestroy.gameObject.activeSelf && Input.GetMouseButton(0);
		hitStrengthBar.gameObject.SetActive(active);
		if(!active)
			return;

		float mouseHoldTime = Time.time - mouseDownStart;
		const int max_durability = 2;
		const int max_bar_height = 400;
		float percentage = mouseHoldTime / max_durability;
		hitStrengthBar.rectTransform.sizeDelta = new Vector2(40, percentage * max_bar_height);
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

	private float CheckDownSpeed(float pDownSpeed)
	{
		if(
			IsVoxelSolid(-playerWidth, pDownSpeed, playerWidth) ||
			IsVoxelSolid(playerWidth, pDownSpeed, -playerWidth) ||
			IsVoxelSolid(playerWidth, pDownSpeed, playerWidth) ||
			IsVoxelSolid(-playerWidth, pDownSpeed, playerWidth)
			)
		{
			isGrounded = true;
			return 0;
		}
		else
		{
			isGrounded = false;
			return pDownSpeed;
		}
	}

	private float CheckUpSpeed(float pUpSpeed)
	{
		if(
			IsVoxelSolid(-playerWidth, PLAYER_HEIGHT + pUpSpeed, playerWidth) ||
			IsVoxelSolid(playerWidth, PLAYER_HEIGHT + pUpSpeed, -playerWidth) ||
			IsVoxelSolid(playerWidth, PLAYER_HEIGHT + pUpSpeed, playerWidth) ||
			IsVoxelSolid(-playerWidth, PLAYER_HEIGHT + pUpSpeed, playerWidth)
			)
		{
			return 0;
		}
		else
		{
			return pUpSpeed;
		}
	}

	bool disableInput;
	float mouseDownStart;

	private void GetPlayerInputs()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			disableInput = !disableInput;

		if(disableInput)
			return;

		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		mouseHorizontal = Input.GetAxis("Mouse X") * mouseSensitivity;
		mouseVertical = Input.GetAxis("Mouse Y") * mouseSensitivity;

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
			UpdateSelectedBlock();

			if(Input.GetKey(KeyCode.LeftControl))
				mouseSensitivity += Mathf.Sign(scroll);
		}

		

		if(highlightDestroy.gameObject.activeSelf)
		{
			if(Input.GetMouseButtonDown(0))
			{
				mouseDownStart = Time.time;
			}

			else if(Input.GetMouseButtonUp(0))
			{
				world.ChunksController.GetChunk(highlightDestroy.position).TryDestroyVoxel(highlightDestroy.position, Time.time - mouseDownStart);
			}

			if(Input.GetMouseButtonDown(1))
			{
				world.ChunksController.GetChunk(highlightPlace.position).EditVoxel(highlightPlace.position, inventory[selectedBlockIndex]);
			}
		}
	}

	private void UpdateSelectedBlock()
	{
		selectedBlockIndex = (inventory.Count + selectedBlockIndex) % inventory.Count;
		selectedBlockText.text = inventory[selectedBlockIndex] + " block selected";
	}

	void PlaceCursorBlock()
	{
		float step = checkIncrement;
		Vector3 lastPos = new Vector3();

		while(step < reach)
		{
			Vector3 pos = cam.position + (cam.forward * step);
			if(world.VoxelController.IsVoxelSolid(pos))
			{
				highlightDestroy.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
				highlightPlace.position = lastPos;

				highlightDestroy.gameObject.SetActive(true);
				highlightPlace.gameObject.SetActive(true);

				return;
			}

			lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
			step += checkIncrement;
		}

		highlightDestroy.gameObject.SetActive(false);
		highlightPlace.gameObject.SetActive(false);
	}

	private bool IsVoxelSolid(float pXOfset, float pYOfset, float pZOfset)
	{
		return IsVoxelSolid(new Vector3(pXOfset, pYOfset, pZOfset));
	}
	private bool IsVoxelSolid(Vector3 pOffset)
	{
		return world.VoxelController.IsVoxelSolid(transform.position + pOffset);
	}

	// COLLISION CHECKS	
	bool front => IsVoxelSolid(0, 0, playerWidth) || IsVoxelSolid(0, PLAYER_HEIGHT, playerWidth);
	bool back => IsVoxelSolid(0, 0, -playerWidth) || IsVoxelSolid(0, PLAYER_HEIGHT, -playerWidth);
	bool left => IsVoxelSolid(-playerWidth, 0, 0) || IsVoxelSolid(-playerWidth, PLAYER_HEIGHT, 0);
	bool right => IsVoxelSolid(playerWidth, 0, 0) || IsVoxelSolid(playerWidth, PLAYER_HEIGHT, 0);
}
