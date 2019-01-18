using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(BoxCollider2D))]
public class UIButton : UIObject {

	[Space(10)][Header("Settings")]
	public UnityEvent pressEvent;
	public Vector2 buttonOffset = new Vector2(0, 0.0625f);

	[Space(10)][Header("References")]
	public AudioManager audioManager;
	public Panel parentPanel;
	public BoxCollider2D boxCollider;
	public GameObject buttonContainer;
	public SpriteRenderer sprite_ButtonTop;
	public SpriteRenderer sprite_ButtonBottom;
	public TextMesh text_ButtonText;

	[Space(10)][Header("Prefabs")]
	public GameObject prefab_DissolveText;
	public GameObject prefab_DissolveArrow;

	[Space(10)][Header("Sounds")]
	public AudioClip clip_Pressed;
	public AudioClip clip_Activated;

	[Space(10)][Header("Variables")]
	public bool isPressed = false;
	public bool isClickable = true;

	// Hidden Variables
	float pressPercentage;

	private void Start () {
		// Get References
		parentPanel = transform.parent.GetComponent<Panel>();

		// Get BoxCollider2D
		if (boxCollider == null) {
			boxCollider = GetComponent<BoxCollider2D>();
		}

		// Initialize button
		sprite_ButtonBottom.transform.localPosition = -buttonOffset;
		sprite_ButtonTop.transform.localPosition = buttonOffset;

		// Sizes
		sprite_ButtonTop.size = boxCollider.size;
		sprite_ButtonBottom.size = boxCollider.size;

		// Resize Collider
		boxCollider.size = boxCollider.size + new Vector2(0, buttonOffset.y * 2);
	}

	private void Update() {
		// Lerp press percentage
		pressPercentage = Mathf.Lerp(pressPercentage, (isPressed == true || isClickable == false ? 1 : 0), 20f * Time.deltaTime);

		// ButtonTop movement
		sprite_ButtonTop.transform.localPosition = Vector3.Lerp(buttonOffset, -buttonOffset, pressPercentage);

		if (isClickable == false && buttonContainer.activeSelf == true) {
			if (pressPercentage > 0.9f) {
				Hide();
			}
		}
	}

	#region Button Click Functionality
	public void Press () {
		if (isClickable == true) {
			isPressed = true;
			audioManager.PlayClip(clip_Pressed, 1f, 1f);
		}
	}
	public void Release (bool activate = false) {
		if (isClickable == true) {
			isPressed = false;
			
			if (activate == true) {
				audioManager.PlayClip(clip_Pressed, 0.75f, 1.5f);
				audioManager.PlayClip(clip_Activated, 0.9f, 1f);
				Activate();
			} else {
				audioManager.PlayClip(clip_Pressed, 0.9f, 1.5f);
			}
		}
	}
	public void Activate () {
		pressEvent.Invoke();
		Dissolve();
	}
	#endregion

	#region Dissolve & Appear Functionality
	public void Dissolve () {
		// Intantiate DissolveText
		if (text_ButtonText != null) {
			DissolveText newDissolveText = Instantiate(prefab_DissolveText, text_ButtonText.transform.position, Quaternion.identity).GetComponent<DissolveText>();
			newDissolveText.SetText(text_ButtonText.text, BetterColor.white);
		} else {
			DissolveArrow newDissolveArrow = Instantiate(prefab_DissolveArrow, sprite_ButtonTop.transform.position, Quaternion.identity).GetComponent<DissolveArrow>();
		}

		Hide();
	}
	public override void Hide () {
		pressPercentage = 1;
		boxCollider.enabled = false;

		// Make this button None-Clickable
		isClickable = false;

		// Hide this button
		buttonContainer.SetActive(false);
	}
	public override void Spawn () {
		// Play Audio
		audioManager.PlayClip(clip_Pressed, 0.75f, 1.5f);
		boxCollider.enabled = true;

		// Show this button
		buttonContainer.SetActive(true);

		// Enable clicking
		isClickable = true;

		// Set ButtonTop position
		sprite_ButtonTop.transform.localPosition = -buttonOffset;
		pressPercentage = 1;
	}
	public override void Despawn () {
		// Make this button None-Clickable
		audioManager.PlayClip(clip_Pressed, 0.75f, 1.5f);
		isClickable = false;
	}
	#endregion
}
