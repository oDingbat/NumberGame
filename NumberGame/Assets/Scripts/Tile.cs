using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	[Space(10)][Header("Settings")]
	public int value;						// The number value this tile has
	public OperationType operationType;		// The operationType for this tile
	public enum OperationType { Addition, Subtraction, Multiplication, Division }
	public List<TileConnection> tileConnections = new List<TileConnection>();

	[Space(10)][Header("References")]
	public TextMesh textTop;
	public TextMesh textBottom;
	public TextMesh text_operatorTop;
	public TextMesh text_operatorBottom;
	public SpriteRenderer spriteTop;
	public SpriteRenderer spriteBottom;
	public SpriteRenderer bridgeTop;
	public SpriteRenderer bridgeBottom;
	public SpriteRenderer bridgeShadow;
	public SpriteRenderer mergeCornerTop;
	public SpriteRenderer mergeCornerBottom;
	public SpriteRenderer mergeCornerShadow;

	[Space(10)][Header("Variables")]
	public bool isSelected;
	public Vector2 arrayPosition;
	public Tile mergeTileNext = null;
	public Tile mergeTilePrevious = null;
	public Tile mergeTileNextOld = null;

	// Constants
	float bridgeMergeSpeed = 7.5f;

	// Hidden Varibles
	float mergePercentage = 0;
	Vector2 mergeTileLastPos;
	bool operatorUsed = false;
	bool useNextTileOperator = false;
	Color colorTop = BetterColor.white;
	Color colorBottom = BetterColor.grayLight;
	Color colorOperation = BetterColor.blackDark;
	Color colorTileTop = BetterColor.white;
	Color colorTileBottom = BetterColor.grayLight;
	public string mergeCorner;
	Vector2 cornerOffsetA;
	Vector2 cornerOffsetB;
	Vector2 cornerOffsetC;
	Vector2 cornerOffsetD;

	public void SetTile (int newValue, OperationType newOperationType) {
		// Set value
		value = newValue;
		textTop.text = value.ToString();
		textBottom.text = value.ToString();

		// Set operationType
		operationType = newOperationType;

		// Set Colors
		switch (operationType) {
			case (OperationType.Addition):
				spriteTop.color = BetterColor.white;
				spriteBottom.color = BetterColor.grayLight;
				textBottom.color = BetterColor.grayLight;
				break;
			case (OperationType.Subtraction):
				spriteTop.color = BetterColor.pinkLight;
				spriteBottom.color = BetterColor.pinkDark;
				textBottom.color = BetterColor.pinkDark;
				break;
			case (OperationType.Multiplication):
				spriteTop.color = BetterColor.goldLight;
				spriteBottom.color = BetterColor.goldDark;
				textBottom.color = BetterColor.goldDark;
				break;
			case (OperationType.Division):
				spriteTop.color = BetterColor.purpleLight;
				spriteBottom.color = BetterColor.purpleDark;
				textBottom.color = BetterColor.purpleDark;
				break;
		}

		// Store colors for later
		colorTop = textTop.color;
		colorBottom = textBottom.color;
		colorTileTop = spriteTop.color;
		colorTileBottom = spriteBottom.color;

	}

	private void Update () {
		UpdateMovement();
		UpdateBridge();
	}

	private void UpdateMovement () {
		spriteTop.transform.localPosition = Vector3.Lerp(spriteTop.transform.localPosition, (isSelected ? new Vector3(0, 0.06f, 0) : new Vector3(0, 0.03f, 0)), 15 * Time.deltaTime);

		if (isSelected == true && mergeTilePrevious != null) {
			spriteTop.color = Color.Lerp(colorTileTop, BetterColor.white, mergeTilePrevious.mergePercentage);
			spriteBottom.color = Color.Lerp(colorTileBottom, BetterColor.grayLight, mergeTilePrevious.mergePercentage);
		} else {
			spriteTop.color = Color.Lerp(spriteTop.color, colorTileTop, 25 * Time.deltaTime);
			spriteBottom.color = Color.Lerp(spriteBottom.color, colorTileBottom, 25 * Time.deltaTime);
		}
	}

	public void SetNextMergeTile (Tile newNextMergeTile, string corner = "Null") {
		if (mergeCorner == "Null" && newNextMergeTile != mergeTileNextOld) {
			mergePercentage = 0;
		}

		mergeTileNext = newNextMergeTile;
		mergeCorner = corner;
		
		switch (mergeCorner) {
			case "Top Left":
				mergeCornerTop.transform.localEulerAngles = new Vector3(0, 0, 270);
				break;
			case "Top Right":
				mergeCornerTop.transform.localEulerAngles = new Vector3(0, 0, 180);
				break;
			case "Bottom Left":
				mergeCornerTop.transform.localEulerAngles = new Vector3(0, 0, 0);
				break;
			case "Bottom Right":
				mergeCornerTop.transform.localEulerAngles = new Vector3(0, 0, 90);
				break;
		}

		mergeCornerBottom.transform.localEulerAngles = mergeCornerTop.transform.localEulerAngles;
		mergeCornerShadow.transform.localEulerAngles = mergeCornerTop.transform.localEulerAngles;

		if (corner != "Null" && corner != "Release") {
			Vector2 previousDirection = Vector3.Normalize(transform.position - mergeTilePrevious.transform.position);
			Vector2 nextDirection = Vector3.Normalize(mergeTileNext.transform.position - transform.position);

			mergeCornerTop.gameObject.SetActive(true);
			mergeCornerBottom.gameObject.SetActive(true);
			mergeCornerShadow.gameObject.SetActive(true);

			cornerOffsetA = (Vector2)(spriteTop.transform.position - spriteTop.transform.localPosition) + (-previousDirection * 0.40625f);
			cornerOffsetB = (Vector2)(spriteTop.transform.position - spriteTop.transform.localPosition) + (-previousDirection * 0.40625f) + nextDirection * (0.40625f);
			cornerOffsetC = (Vector2)(spriteTop.transform.position - spriteTop.transform.localPosition) + (-previousDirection * 0.4375f);
			cornerOffsetD = (Vector2)(spriteTop.transform.position - spriteTop.transform.localPosition) + (-previousDirection * 0.4375f) + nextDirection * (0.4375f);
		}
		
		if (mergeTileNext != null) {
			mergeTileNextOld = mergeTileNext;
		}
	}

	public void SetPreviousMergeTile(Tile newPreviousMergeTile) {
		mergeTilePrevious = newPreviousMergeTile;
	}

	public void SetMergeOperation (string mergeOperator) {
		text_operatorTop.text = mergeOperator;
		text_operatorBottom.text = mergeOperator;

		switch (mergeOperator) {
			case ("+"):
				colorOperation = BetterColor.blackDark;
				break;
			case ("-"):
				colorOperation = BetterColor.pinkLight;
				break;
			case ("×"):
				colorOperation = BetterColor.goldLight;
				break;
			case ("÷"):
				colorOperation = BetterColor.purpleLight;
				break;
		}
	}

	private void UpdateBridge () {
		if (mergeTileNext != null) {
			mergePercentage = Mathf.Clamp01(mergePercentage + Time.deltaTime * 5f);
			mergeTileLastPos = mergeTileNext.transform.position;

			// Toggle bridge visibility
			bridgeTop.enabled = true;
			bridgeBottom.enabled = true;
			bridgeShadow.enabled = true;
		} else {
			mergePercentage = Mathf.Clamp01(mergePercentage - Time.deltaTime * 5f);

			if (Vector2.Distance(bridgeTop.size, new Vector2(0.75f, 0.75f)) < 0.01f) {
				// Toggle bridge visibility
				bridgeTop.enabled = false;
				bridgeBottom.enabled = false;
				bridgeShadow.enabled = false;

				bridgeTop.size = Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
				bridgeBottom.size = Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
				bridgeShadow.size = Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
				bridgeTop.transform.position = Vector3.Lerp(new Vector3(spriteTop.transform.position.x, spriteTop.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteTop.transform.localPosition.y, 0), mergePercentage);
				bridgeBottom.transform.position = Vector3.Lerp(new Vector3(spriteBottom.transform.position.x, spriteBottom.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteBottom.transform.localPosition.y, 0), mergePercentage);
				bridgeShadow.transform.position = Vector3.Lerp(new Vector3(spriteBottom.transform.position.x, spriteBottom.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteBottom.transform.localPosition.y, 0), mergePercentage);
			}
		}
		
		bridgeTop.color = Color.Lerp((mergeTilePrevious != null ? BetterColor.white : spriteTop.color), BetterColor.white, mergePercentage);
		bridgeBottom.color = Color.Lerp((mergeTilePrevious != null ? BetterColor.grayLight : spriteBottom.color), BetterColor.grayLight, mergePercentage);

		if (mergeTilePrevious != null) {
			textTop.color = Color.Lerp(colorTop, mergeTilePrevious.colorOperation, mergeTilePrevious.mergePercentage);
			textBottom.color = Color.Lerp(colorBottom, BetterColor.grayLight, mergeTilePrevious.mergePercentage);
		} else {
			textTop.color = Color.Lerp(colorTop, BetterColor.blackDark, mergePercentage);
			textBottom.color = Color.Lerp(colorBottom, BetterColor.grayLight, mergePercentage);
		}

		// Bridge merge animation
		bridgeTop.size = new Vector2(0.75f, 0.75f) + Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
		bridgeBottom.size = new Vector2(0.75f, 0.75f) + Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
		bridgeShadow.size = new Vector2(0.8125f, 0.81f) + Vector2.Lerp(Vector2.zero, new Vector2(Mathf.Abs(transform.position.x - mergeTileLastPos.x), Mathf.Abs(transform.position.y - mergeTileLastPos.y)), mergePercentage);
		bridgeTop.transform.position = Vector3.Lerp(new Vector3(spriteTop.transform.position.x, spriteTop.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteTop.transform.localPosition.y, 0), mergePercentage);
		bridgeBottom.transform.position = Vector3.Lerp(new Vector3(spriteBottom.transform.position.x, spriteBottom.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteBottom.transform.localPosition.y, 0), mergePercentage);
		bridgeShadow.transform.position = Vector3.Lerp(new Vector3(spriteBottom.transform.position.x, spriteBottom.transform.position.y, 0), new Vector3((transform.position.x + mergeTileLastPos.x) / 2f, (transform.position.y + mergeTileLastPos.y) / 2f + spriteBottom.transform.localPosition.y, 0), mergePercentage);

		// Operator text animation
		text_operatorTop.transform.position = Vector3.Lerp(transform.position + new Vector3(0, spriteTop.transform.localPosition.y, 0), (transform.position + (Vector3)mergeTileLastPos) / 2f + new Vector3(0, spriteTop.transform.localPosition.y, 0), mergePercentage);
		text_operatorTop.color = Color.Lerp(new Color(colorOperation.r, colorOperation.g, colorOperation.b, 0), colorOperation, mergePercentage);
		text_operatorBottom.color = Color.Lerp(new Color(colorOperation.r, colorOperation.g, colorOperation.b, 0), BetterColor.grayLight, mergePercentage);

		// Merge Corner animation
		float mergePercentageIncreased = Mathf.Clamp01(mergePercentage + 0.875f);
		if (mergeTilePrevious != null && mergeCorner != "Release") {
			mergeCornerTop.transform.position = spriteTop.transform.localPosition + Vector3.Lerp(cornerOffsetA, cornerOffsetB, mergePercentageIncreased);
			mergeCornerBottom.transform.position = spriteBottom.transform.localPosition + Vector3.Lerp(cornerOffsetA, cornerOffsetB, mergePercentageIncreased);
			mergeCornerShadow.transform.position = spriteBottom.transform.localPosition + Vector3.Lerp(cornerOffsetC, cornerOffsetD, mergePercentageIncreased);
			mergeCornerTop.color = Color.Lerp(BetterColor.white, BetterColor.white, mergePercentageIncreased);
			mergeCornerBottom.color = Color.Lerp(colorBottom, BetterColor.grayLight, mergePercentageIncreased);
		} else {
			mergeCornerTop.transform.position = spriteTop.transform.position;
			mergeCornerBottom.transform.position = spriteBottom.transform.position;
			mergeCornerTop.color = spriteTop.color;
			mergeCornerBottom.color = colorBottom;

			mergeCornerTop.gameObject.SetActive(false);
			mergeCornerBottom.gameObject.SetActive(false);
			mergeCornerShadow.gameObject.SetActive(false);
		}
	}

}

[System.Serializable]
public class TileConnection {
	public Tile tile;
	public Vector2 direction;

	public TileConnection (Tile _tile, Vector2 _direction) {
		tile = _tile;
		direction = _direction;
	}
}
