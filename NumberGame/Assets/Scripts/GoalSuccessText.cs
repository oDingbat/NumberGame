using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalSuccessText : MonoBehaviour {

	public TextMesh textMesh;

	float lifespan = 0.5f;
	float elapsedTime = 0;
	float riseSpeed = 2f;
	float sizeSpeed = 0.02f;
	Color colorInitial;
	Color colorDesired;

	private void Start () {
		colorInitial = textMesh.color;
		colorDesired = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0);
	}

	private void Update () {
		transform.position += new Vector3(0, Time.deltaTime * riseSpeed, 0);
		transform.localScale += Vector3.one * sizeSpeed * Time.deltaTime;
		textMesh.color = Color.Lerp(colorInitial, colorDesired, elapsedTime / lifespan);

		elapsedTime += Time.deltaTime;

		if (elapsedTime >= lifespan) {
			Destroy(gameObject);
		}
	}

	public void SetText (string newText) {
		textMesh.text = newText;
	}

}
