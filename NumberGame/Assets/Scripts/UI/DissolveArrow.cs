using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveArrow : MonoBehaviour {

	public float lifespan = 0.1f;
	public Vector2 rise = new Vector2(0, 0.65f);
	public SpriteRenderer arrow;

	Vector3 startingPos;
	float elapsedTime = 0;

	private void Start() {
		startingPos = transform.localPosition;
	}

	void Update() {
		elapsedTime += Time.deltaTime;
		float sineTime = Mathf.Sin(Mathf.Clamp01(elapsedTime / lifespan) * Mathf.PI * 0.5f);

		transform.localPosition = Vector3.Lerp(startingPos, startingPos + (Vector3)rise, sineTime);

		arrow.color = Color.Lerp(new Color(arrow.color.r, arrow.color.g, arrow.color.b, 1), new Color(arrow.color.r, arrow.color.g, arrow.color.b, 0), (elapsedTime / lifespan));

		if (arrow.color.a < 0.0125f) {
			Destroy(gameObject);
		}
	}
}
