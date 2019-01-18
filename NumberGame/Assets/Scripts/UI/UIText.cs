using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIText : UIObject {

	[Space(10)][Header("References")]
	public AudioManager audioManager;
	public GameObject textContainer;
	public GameObject textTop;

	[Space(10)][Header("Sounds")]
	public AudioClip clip_Sound;

	// Hidden Variables
	float heightPercentage;
	public bool isDespawning = false;

	// Constants
	Vector3 textOffset = new Vector3(0, 3.75f, -0.5f);

	private void Update () {
		// Lerp press percentage
		heightPercentage = Mathf.Lerp(heightPercentage, (isDespawning == true ? 1 : 0), 20f * Time.deltaTime);

		// ButtonTop movement
		textTop.transform.localPosition = Vector3.Lerp(textOffset, new Vector3(0, 0, -0.5f), heightPercentage);

		if (isDespawning == true && textContainer.activeSelf == true) {
			if (heightPercentage > 0.9f) {
				Hide();
			}
		}
	}

	public override void Hide() {
		heightPercentage = 1;
		isDespawning = true;

		// Hide this button
		textContainer.SetActive(false);
	}
	
	public override void Spawn() {
		heightPercentage = 1;
		isDespawning = false;

		// Show this textObject
		textContainer.SetActive(true);

		// ButtonTop movement
		textTop.transform.localPosition = new Vector3(0, 0, -0.5f);

		audioManager.PlayClip(clip_Sound, 0.75f, 1.5f);
	}

	public override void Despawn() {
		isDespawning = true;
		audioManager.PlayClip(clip_Sound, 0.75f, 1.5f);
	}
}
