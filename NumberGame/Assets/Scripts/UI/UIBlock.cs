using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlock : UIObject {
	
	[Space(10)][Header("References")]
	public AudioManager audioManager;
	public GameObject blockContainer;
	public GameObject blockTop;

	[Space(10)][Header("Sounds")]
	public AudioClip clip_Sound;

	// Hidden Variables
	float heightPercentage;
	public bool isDespawning = false;

	// Constants
	Vector2 blockOffset = new Vector2(0, 0.1f);

	private void Update () {
		// Lerp press percentage
		heightPercentage = Mathf.Lerp(heightPercentage, (isDespawning == true ? 1 : 0), 20f * Time.deltaTime);

		// ButtonTop movement
		blockTop.transform.localPosition = Vector3.Lerp(blockOffset, Vector3.zero, heightPercentage);

		if (isDespawning == true && blockContainer.activeSelf == true) {
			if (heightPercentage > 0.9f) {
				Hide();
			}
		}
	}

	public override void Hide() {
		heightPercentage = 1;
		isDespawning = true;

		// Hide this block
		blockContainer.SetActive(false);
	}
	
	public override void Spawn() {
		heightPercentage = 1;
		isDespawning = false;

		// Show this block
		blockContainer.SetActive(true);

		// BlockTop movement
		blockTop.transform.localPosition = new Vector3(0, 0, -0.5f);

		audioManager.PlayClip(clip_Sound, 0.75f, 1.5f);
	}

	public override void Despawn() {
		isDespawning = true;
		audioManager.PlayClip(clip_Sound, 0.75f, 1.5f);
	}
}
