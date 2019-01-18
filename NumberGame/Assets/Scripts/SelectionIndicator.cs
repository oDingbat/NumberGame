using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour {

	[Space(10)][Header("References")]
	public SpriteRenderer sprite;

	[Space(10)][Header("Settings")]
	public float lifespan = 0.5f;
	public float sizeIncrease = 0.75f;
	
	// Hidden Variables
	Vector2 sizeInitial;
	Vector2 sizeDelta;
	float elapsedTime = 0;

	private void Start () {
		sizeInitial = sprite.size;
		sizeDelta = sizeInitial + new Vector2(sizeIncrease, sizeIncrease);
	}
	
	void Update () {
		elapsedTime += Time.deltaTime;

		sprite.size = Vector2.Lerp(sizeInitial, sizeDelta, elapsedTime / lifespan);
		sprite.color = Color.Lerp(new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1), new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0), elapsedTime / lifespan);

		if (elapsedTime >= lifespan) {
			Destroy(gameObject);
		}
	}

	public void InitializeIndicator (Color newColor) {
		InitializeIndicator(newColor, new Vector2(0.75f, 0.75f));
	}

	internal void InitializeIndicator(Color newColor, Vector2 size) {
		sprite.color = newColor;
		sprite.size = new Vector2(size.x, size.y);
	}
}
