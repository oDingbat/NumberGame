using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveText : MonoBehaviour {

	public float lifespan = 0.1f;
	public Vector2 rise = new Vector2(0, 0.65f);
	public TextMesh textA;

	Vector3 startingPos;
	float elapsedTime = 0;

	private void Start() {
		startingPos = transform.localPosition;
	}

	void Update() {
		elapsedTime += Time.deltaTime;
		float sineTime = Mathf.Sin(Mathf.Clamp01(elapsedTime / lifespan) * Mathf.PI * 0.5f);

		transform.localPosition = Vector3.Lerp(startingPos, startingPos + (Vector3)rise, sineTime);

		textA.color = Color.Lerp(new Color(textA.color.r, textA.color.g, textA.color.b, 1), new Color(textA.color.r, textA.color.g, textA.color.b, 0), (elapsedTime / lifespan));

		if (textA.color.a < 0.0125f) {
			Destroy(gameObject);
		}
	}

	public void SetText(string text, Color color) {
		textA.text = text;
		textA.color = color;
	}
}
