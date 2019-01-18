using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyText : MonoBehaviour {

	public float lifespan = 0.1f;
	public Vector2 rise = new Vector2(0, 0.65f);

	public TextMesh textA;
	public TextMesh textB;

	Vector3 startingPos;

	float elapsedTime = 0;
	bool isDead = false;
	float flashDelay = 0.1f;

	private void Start() {
		startingPos = transform.localPosition;
	}
	
	void Update () {
		elapsedTime += Time.deltaTime;
		float sineTime = Mathf.Sin(Mathf.Clamp01(elapsedTime / lifespan) * Mathf.PI * 0.5f);

		transform.localPosition = Vector3.Lerp(startingPos, startingPos + (Vector3)rise, sineTime);

		textA.color = Color.Lerp(new Color(textA.color.r, textA.color.g, textA.color.b, 1), new Color(textA.color.r, textA.color.g, textA.color.b, 0), (elapsedTime / lifespan));
		if (textB != null) {
			textB.color = Color.Lerp(new Color(textB.color.r, textB.color.g, textB.color.b, 1), new Color(textB.color.r, textB.color.g, textB.color.b, 0), (elapsedTime / lifespan));
		}

		if (textA.color.a < 0.05f) {
			Destroy(gameObject);
		}
	}

	public void SetText (string text, Color color) {
		textA.text = text;
		textA.color = color;
		if (textB != null) {
			textB.text = text;
		}
	}

	

	

}
