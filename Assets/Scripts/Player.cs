﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public bool isGrounded;
	public bool isSprinting;

	Transform cam;
	World world;

	public float walkSpeed = 3f;
	public float sprintSpeed = 6f;
	public float jumpForce = 5f;
	public float gravity = -9.8f;

	public float playerWidth = 0.15f;

	float horizontal;
	float vertical;
	float mouseHorizontal;
	float mouseVertical;
	Vector3 velocity;
	float verticalMomemtum = 0.0f;
	bool jumpRequest;

	public Transform highlightBlock;
	public Transform placeHighlightBlock;

	public float checkIncrement = 0.1f;
	public float reach = 8;

	// Start is called before the first frame update
	void Start () {
		cam = GameObject.Find ("Main Camera").transform;
		world = GameObject.Find ("World").GetComponent<World> ();
	}

	void FixedUpdate () {
		CalculateVelocity ();

		if (jumpRequest) {
			Jump ();
		}


		transform.Translate (velocity, Space.World);
	}

	void Update () {
		GetPlayerInputs ();
		transform.Rotate (Vector3.up * mouseHorizontal);
		cam.Rotate (Vector3.right * -mouseVertical);
	}

	void Jump () {
		verticalMomemtum = jumpForce;
		isGrounded = false;
		jumpRequest = false;
	}

	void CalculateVelocity () {
		// Affect vertical momentum with gravity
		if (verticalMomemtum > gravity) {
			verticalMomemtum += Time.fixedDeltaTime * gravity;
		}

		// if we are sprinting, use the psrint multiplier
		if (isSprinting) {
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
		}
		else {
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
		}

		// Apply vertical momentum
		velocity += Vector3.up * verticalMomemtum * Time.fixedDeltaTime;

		if ((velocity.z > 0 && front) || (velocity.z < 0 && back)) {
			velocity.z = 0;
		}
		if ((velocity.x > 0 && right) || (velocity.x < 0 && left)) {
			velocity.x = 0;
		}
		if (velocity.y < 0) {
			velocity.y = checkDownSpeed (velocity.y);
		}
		else if (velocity.y > 0) {
			velocity.y = checkUpSpeed (velocity.y);
		}
	}

	void GetPlayerInputs () {
		// TODO: Use unitys new input package
		horizontal = Input.GetAxis ("Horizontal");
		vertical = Input.GetAxis ("Vertical");
		mouseHorizontal = Input.GetAxis ("Mouse X");
		mouseVertical = Input.GetAxis ("Mouse Y");

		if (Input.GetButtonDown ("Sprint")) {
			isSprinting = true;
		}
		if (Input.GetButtonUp ("Sprint")) {
			isSprinting = false;
		}

		if (isGrounded && Input.GetButtonDown ("Jump")) {
			jumpRequest = true;
		}
	}

	void placeCursorBlock () {
		float step = checkIncrement;
		Vector3 lastPos = new Vector3();

		while(step < reach) {
			Vector3 pos = cam.position + (cam.forward)
		}
	}

	float checkDownSpeed (float downSpeed) {
		if (
			world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
		) {
			isGrounded = true;
			return 0;
		}
		else {
			isGrounded = false;
			return downSpeed;
		}
	}

	float checkUpSpeed (float upSpeed) {
		if (
			world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
			world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
		) {
			return 0;
		}
		else {
			return upSpeed;
		}
	}

	public bool front {
		get {
			if (
				world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
				world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
			) {
				return true;
			}
			else {
				return false;
			}
		}
	}

	public bool back {
		get {
			if (
				world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
				world.CheckForVoxel (new Vector3 (transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
			) {
				return true;
			}
			else {
				return false;
			}
		}
	}

	public bool left {
		get {
			if (
				world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
				world.CheckForVoxel (new Vector3 (transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
			) {
				return true;
			}
			else {
				return false;
			}
		}
	}

	public bool right {
		get {
			if (
				world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
				world.CheckForVoxel (new Vector3 (transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
			) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}
