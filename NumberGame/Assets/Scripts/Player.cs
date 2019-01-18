using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	[Space(10)][Header("Settings")]
	public LayerMask tileMask;
	public LayerMask buttonMask;

	[Space(10)][Header("References")]
	public AudioManager audioManager;
	public GameObject cursor;
	public Camera camera;
	public Board board;

	[Space(10)][Header("Prefabs")]
	public GameObject prefab_selectionIndicator;

	[Space(10)][Header("Audio")]
	public AudioClip clip_TileSelectedAddition;
	public AudioClip clip_TileSelectedSubtraction;
	public AudioClip clip_TileSelectedMultiplication;
	public AudioClip clip_TileSelectedDivision;
	public AudioClip clip_TileDeselected;

	public List<Tile> tilesSelected = new List<Tile>();
	public UIButton buttonPressed = null;

	// Variables
	Vector2 mousePosDesired;
	string operators = "";

	private void Start () {
		Cursor.visible = false;
	}

	private void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Break();
		}

		UpdateCursor();
		UpdateSelectingAndDeselecting();
	}
	
	private void UpdateCursor () {
		mousePosDesired = camera.ScreenToWorldPoint(Input.mousePosition);
		cursor.transform.position = Vector3.Lerp(cursor.transform.position, mousePosDesired, 50 * Time.deltaTime);

		if (Input.GetMouseButtonDown(0)) {
			Cursor.visible = false;
		}
	}

	private void UpdateSelectingAndDeselecting () {

		// Selecting
		if (Input.GetMouseButton(0)) {

			// Selecting Tiles
			if (board.isLocked == false && board.isGameOver == false) {
				RaycastHit2D hitT = Physics2D.Raycast(mousePosDesired, Vector2.up, 0f, tileMask);
				if (hitT.transform != null) {
					Tile hitTile = hitT.transform.GetComponent<Tile>();
					if (tilesSelected.Contains(hitTile) == false) {
						// Selecting
						if (tilesSelected.Count == 0) {     // Is this the first tile we have selected?
							SelectTile(hitTile);
						} else if (tilesSelected[tilesSelected.Count - 1].tileConnections.Exists(tc => tc.tile == hitTile) == true) {       // Is the hitTile connected to a tile we already have selected
							SelectTile(hitTile);
						}
					} else {
						// Deselecting
						if (tilesSelected.Count > 1 && hitTile != tilesSelected[tilesSelected.Count - 1]) {
							int tileIndex = tilesSelected.IndexOf(hitTile);
							// Loop through each tile AFTER tile at 'tileIndex' and deselect them
							while (tilesSelected.Count > tileIndex + 1) {
								DeselectTile(tileIndex);
							}
						}
					}
					operators = board.ShowMath(tilesSelected);
				}
			}

			// Selecting Buttons
			RaycastHit2D hitB = Physics2D.Raycast(mousePosDesired, Vector2.up, 0f, buttonMask);
			if (hitB.transform != null) {
				UIButton hitButton = hitB.transform.GetComponent<UIButton>();
				if (hitButton != buttonPressed) {
					if (buttonPressed != null) {
						buttonPressed.Release(false);
					}

					if (hitButton.isClickable == true) {
						buttonPressed = hitButton;
						buttonPressed.Press();
						SpawnSelectionIndicator(hitButton);
					}
				}
			} else if (buttonPressed != null) {    // Deselect button if we're already pressing one
				buttonPressed.Release(false);
				buttonPressed = null;
			}
		}

		// Clear tiles
		if (Input.GetMouseButtonUp(0)) {

			bool solutionSuccessful = false;
			if (tilesSelected.Count > 1) {      // TODO: Broaden this?
				solutionSuccessful = board.AttemptSolution(tilesSelected);
			}

			if (solutionSuccessful == false) {
				if (tilesSelected.Count > 0) {
					if (tilesSelected.Count == 1) {
						PlayTileClip(tilesSelected[0], false);
					} else {
						audioManager.PlayClip(clip_TileDeselected);
					}
				}
				foreach (Tile tile in tilesSelected) {
					tile.isSelected = false;
					tile.SetNextMergeTile(null, "Null");
					tile.SetPreviousMergeTile(null);
				}
			}

			tilesSelected.Clear();
			operators = board.ShowMath(tilesSelected);

			// Deselect Button
			if (buttonPressed != null) {
				buttonPressed.Release(true);
				buttonPressed = null;
			}
		}

		// Relay operatorString to tiles
		for (int i = 0; i < tilesSelected.Count - 1; i++) {
			Tile t = tilesSelected[i];
			t.SetMergeOperation(operators[i].ToString());
		}

	}

	public void SelectTile (Tile tile) {
		// Selects the specified tile

		tilesSelected.Add(tile);			// Add tile to tilesSelected list
		tile.isSelected = true;				// Set tile.isSelected to true
		SpawnSelectionIndicator(tile);		// Spawn a selectionIndicator
		PlayTileClip(tile, true);           // Play the selection audio

		// If tilesSelected.Count > 1, Set NextMergeTile & PreviousMergeTile
		if (tilesSelected.Count > 1) {
			tilesSelected[tilesSelected.Count - 2].SetNextMergeTile(tile, FetchCorner());
			tile.SetPreviousMergeTile(tilesSelected[tilesSelected.Count - 2]);   // Set merge tile for new tile
		}
	}

	public void DeselectTile (int tileIndex) {
		// Deselects the specified tile

		tilesSelected[tileIndex + 1].isSelected = false;
		tilesSelected[tileIndex + 1].SetPreviousMergeTile(null);
		tilesSelected[tileIndex + 1].SetNextMergeTile(null);
		PlayTileClip(tilesSelected[tileIndex + 1], false);
		tilesSelected.RemoveAt(tileIndex + 1);
		tilesSelected[tilesSelected.Count - 1].SetNextMergeTile(null);
	}

	private void PlayTileClip (Tile tile, bool isSelecting) {
		AudioClip desiredClip = null;
		switch (tile.operationType) {
			case (Tile.OperationType.Addition):
				desiredClip = clip_TileSelectedAddition;
				break;
			case (Tile.OperationType.Subtraction):
				desiredClip = clip_TileSelectedSubtraction;
				break;
			case (Tile.OperationType.Multiplication):
				desiredClip = clip_TileSelectedMultiplication;
				break;
			case (Tile.OperationType.Division):
				desiredClip = clip_TileSelectedDivision;
				break;
		}

		if (isSelecting == true) {
			audioManager.PlayClip(desiredClip, 1f, 1f);
		} else {
			audioManager.PlayClip(desiredClip, 0.9f, 1.5f);
		}
	}

	Vector2 dirUp = new Vector2(0, 1);
	Vector2 dirDown = new Vector2(0, -1);
	Vector2 dirLeft = new Vector2(-1, 0);
	Vector2 dirRight = new Vector2(1, 0);

	private string FetchCorner () {
		string corner = "Release";
		if (tilesSelected.Count > 2) {
			
			Tile tileA = tilesSelected[tilesSelected.Count - 3];
			Tile tileB = tilesSelected[tilesSelected.Count - 2];
			Tile tileC = tilesSelected[tilesSelected.Count - 1];

			Vector2 directionAB = Vector3.Normalize(tileB.transform.position - tileA.transform.position);
			Vector2 directionBC = Vector3.Normalize(tileC.transform.position - tileB.transform.position);

			if (directionAB == dirRight) {
				if (directionBC == dirUp) {
					corner = "Top Left";
				} else if (directionBC == dirDown) {
					corner = "Bottom Left";
				}
			} else if (directionAB == dirLeft) {
				if (directionBC == dirUp) {
					corner = "Top Right";
				} else if (directionBC == dirDown) {
					corner = "Bottom Right";
				}
			} else if (directionAB == dirUp) {
				if (directionBC == dirLeft) {
					corner = "Bottom Left";
				} else if (directionBC == dirRight) {
					corner = "Bottom Right";
				}
			} else if (directionAB == dirDown) {
				if (directionBC == dirLeft) {
					corner = "Top Left";
				} else if (directionBC == dirRight) {
					corner = "Top Right";
				}
			}
		}

		return corner;
	}

	private void SpawnSelectionIndicator (Tile tile) {
		SelectionIndicator newSelectionIndicator = Instantiate(prefab_selectionIndicator, tile.spriteTop.transform.position, Quaternion.identity, tile.spriteTop.transform).GetComponent<SelectionIndicator>();
		newSelectionIndicator.InitializeIndicator(tile.spriteTop.color);
	}

	private void SpawnSelectionIndicator(UIButton button) {
		SelectionIndicator newSelectionIndicator = Instantiate(prefab_selectionIndicator, button.sprite_ButtonTop.transform.position, Quaternion.identity).GetComponent<SelectionIndicator>();
		newSelectionIndicator.InitializeIndicator(button.sprite_ButtonTop.color, button.sprite_ButtonTop.size);
	}

}
