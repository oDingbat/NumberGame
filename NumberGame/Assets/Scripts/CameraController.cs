using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	[Space(10)][Header("References")]
	public Transform rig;
	
	Vector3 cameraOffset = new Vector3(0, 1.5f, 0);
	Vector2 velocity;
	float angularVelocity;
	Vector2 positionDesired;

	private void Start () {
		positionDesired = rig.transform.position;
	}

	public void AddScreenshake (Vector2 screenshakeVelocity, float screenshakeAngularVelocity) {
		velocity += screenshakeVelocity * 2.5f;
		angularVelocity += screenshakeAngularVelocity * 15f;
	}

	public void Update () {
		UpdateCameraMovement();
		UpdateRigMovement();
	}

	private void UpdateCameraMovement() {
		transform.position += (Vector3)velocity * Time.deltaTime;
		transform.rotation *= Quaternion.Euler(0, 0, angularVelocity * Time.deltaTime * 25);

		velocity = Vector2.Lerp(velocity, (cameraOffset - transform.localPosition) * 15f, 9f * Time.deltaTime);
		angularVelocity = Mathf.Lerp(angularVelocity, -transform.rotation.z * 100, 9f * Time.deltaTime);
	}

	private void UpdateRigMovement () {
		rig.transform.position = Vector3.Lerp(rig.transform.position, positionDesired, 12.5f * Time.deltaTime);
	}

}
