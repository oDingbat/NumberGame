using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour {

	public Tile[,] tiles;

	[Space(10)][Header("References")]
	public CameraController cameraController;
	public AudioManager audioManager;

	[Space(10)][Header("Tile Crunch")]
	public Transform tileCrunch;
	TextMesh[] tileCrunchNumbers;
	TextMesh[] tileCrunchOperators;

	[Space(10)][Header("Prefabs")]
	public GameObject prefab_Tile;
	public GameObject prefab_floatyText;
	public GameObject prefab_FloatyTextFailure;
	public GameObject prefab_goalSuccessText;
	public GameObject prefab_tileCrunchNumber;
	public GameObject prefab_tileCrunchOperator;

	[Space(10)][Header("Game Variables")]
	public int goal = 0;
	public int level = 1;
	public int lives = 3;
	public Gamemode[] gamemodes;
	public Gamemode gamemodeCurrent;
	public float timerCurrent = 16;
	float timerMax = 16;

	[Space(10)][Header("UI")]
	public TextMesh text_GoalTop;
	public TextMesh text_GoalBottom;
	public TextMesh text_MathTop;
	public TextMesh text_MathBottom;
	public TextMesh text_LevelTop;
	public TextMesh text_LevelBottom;

	[Space(10)][Header("Timer Pieces")]
	public SpriteRenderer bar_Background;
	Vector2 screenSize = new Vector2(1080, 1920) * (1f / 256f);
	Coroutine timerTickCoroutine;

	[Space(10)][Header("Variables")]
	public bool isLocked = false;
	public bool isStarted = false;      // Is the game started?
	public bool isGameOver = false;

	[Space(10)][Header("Audio")]
	public AudioClip clip_SolutionSuccess;
	public AudioClip clip_SolutionFail;
	public AudioClip clip_TileSelectedAddition;
	public AudioClip clip_TileSelectedSubtraction;
	public AudioClip clip_TileSelectedMultiplication;
	public AudioClip clip_TileSelectedDivision;
	public AudioClip clip_TimerTick;
	public AudioClip clip_TimerAlarm;

	// Constants
	Vector2 tileValueRange = new Vector2(1, 12);
	int boardSize = 4;
	int pixelsPerUnit = 256;
	Vector2 boardOffset;			// The negative vector2 origin for where tile pieces are created when starting at (0, 0)
	Vector2 tilePixelSpacing = new Vector2(228, 233);
	Vector2 tileSpacing;            // The spacing in 2D space between each tile

	// Variables
	int tilesLastConsumed;
	bool isDoingMath = false;
	float tileCrunchElapsedTime = 1;
	int tileCrunchCount = 0;

	private void InitializeBoard () {
		// Calculate Values
		tileSpacing = new Vector2(tilePixelSpacing.x / pixelsPerUnit, tilePixelSpacing.y / pixelsPerUnit);
		boardOffset = -((boardSize - 1f) / 2f) * tileSpacing;

		// Initialize tiles 2D Array
		tiles = new Tile[boardSize, boardSize];
	}
	
	private void InitializeTileCrunch () {
		// Initializes the tileCrunch gameObject initializing it's number and operator array
		tileCrunch.transform.localPosition = Vector3.zero;

		int boardTileCount = boardSize * boardSize;
		// Initialize tileCrunch arrays
		tileCrunchNumbers = new TextMesh[boardTileCount];
		tileCrunchOperators = new TextMesh[boardTileCount - 1];

		// Create tileCrunch pieces
		for (int i = 0; i < boardTileCount; i++) {
			tileCrunchNumbers[i] = Instantiate(prefab_tileCrunchNumber, Vector3.zero, Quaternion.identity, tileCrunch).GetComponent<TextMesh>();
			tileCrunchNumbers[i].transform.name = "Number (" + i + ")";
			if (i < boardTileCount - 1) {
				tileCrunchOperators[i] = Instantiate(prefab_tileCrunchOperator, Vector3.zero, Quaternion.identity, tileCrunch).GetComponent<TextMesh>();
				tileCrunchOperators[i].transform.name = "Operator (" + i + ")";
			}
		}
	}

	private void GetNewGoal () {
		// Find availableTiles
		List<Tile> tilesAvailable = new List<Tile>();
		for (int x = 0; x < boardSize; x++) {
			for (int y = 0; y < boardSize; y++) {
				if (tiles[x, y] != null && tiles[x, y].tileConnections.Count > 0) {
					tilesAvailable.Add(tiles[x, y]);
				}
			}
		}

		// Create tile path
		int desiredPathLength = (int)Mathf.Max(Mathf.Round(gamemodeCurrent.levelAttributesCurrent.comboLength + Random.Range(-gamemodeCurrent.levelAttributesCurrent.comboLength / 2f, gamemodeCurrent.levelAttributesCurrent.comboLength / 2f)), 2);
		List<Tile> pathTiles = new List<Tile>();
		pathTiles.Add(tilesAvailable[Random.Range(0, tilesAvailable.Count)]);
		tilesAvailable.Remove(pathTiles[0]);
		desiredPathLength--;
		while (desiredPathLength > 0 && pathTiles[pathTiles.Count - 1].tileConnections.Where(c => tilesAvailable.Contains(c.tile) == true).Count() > 0) {
			List<TileConnection> availableConnections = pathTiles[pathTiles.Count - 1].tileConnections.Where(tc => tilesAvailable.Contains(tc.tile) == true).ToList();
			TileConnection randomConnection = availableConnections[Random.Range(0, availableConnections.Count)];
			tilesAvailable.Remove(randomConnection.tile);
			pathTiles.Add(randomConnection.tile);
			desiredPathLength--;
		}
		
		goal = ComputeTilePath(pathTiles);

		// Set UI
		text_GoalTop.text = goal.ToString();
		text_GoalBottom.text = goal.ToString();

		// Modify Bar

		timerTickCoroutine = StartCoroutine(TimerTickCoroutine());
	}

	private void Update() {
		if (isStarted == true) {
			UpdateMath();
			UpdateTimerBar();
			UpdateTileCrunch();
		}
	}

	private void UpdateMath() {
		text_MathTop.transform.localPosition = Vector3.Lerp(text_MathTop.transform.localPosition, (isDoingMath ? new Vector3(0, 0, -1) : new Vector3(0, -0.1f, -1)), 10 * Time.deltaTime);
		Color lerpedColor = text_MathTop.color;
		lerpedColor.a = Mathf.Clamp01(lerpedColor.a + (isDoingMath ? 10 : -10) * Time.deltaTime);

		text_MathTop.color = lerpedColor;
		text_MathBottom.color = new Color(text_MathBottom.color.r, text_MathBottom.color.g, text_MathBottom.color.b, text_MathTop.color.a);
	}

	private void UpdateTileCrunch() {
		float sineValue = Mathf.Sin(Mathf.Clamp01(tileCrunchElapsedTime / 0.75f) * Mathf.PI * 0.5f);

		// Position
		tileCrunch.transform.position = Vector3.Lerp(Vector3.zero, Vector3.up * 0.5f, sineValue);
		tileCrunch.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.25f, sineValue);

		// Colors
		for (int i = 0; i < tileCrunchCount; i++) {
			tileCrunchNumbers[i].color = Color.Lerp(new Color(tileCrunchNumbers[i].color.r, tileCrunchNumbers[i].color.g, tileCrunchNumbers[i].color.b, 1), new Color(tileCrunchNumbers[i].color.r, tileCrunchNumbers[i].color.g, tileCrunchNumbers[i].color.b, 0), sineValue);
			if (i < tileCrunchCount - 1) {
				tileCrunchOperators[i].color = Color.Lerp(new Color(tileCrunchOperators[i].color.r, tileCrunchOperators[i].color.g, tileCrunchOperators[i].color.b, 1), new Color(tileCrunchOperators[i].color.r, tileCrunchOperators[i].color.g, tileCrunchOperators[i].color.b, 0), sineValue);
			}
		}

		tileCrunchElapsedTime = Mathf.Clamp(tileCrunchElapsedTime + Time.deltaTime, 0, 1);
	}

	private void UpdateTimerBar () {
		float timerPercentage;

		if (isLocked == false) {
			timerCurrent = Mathf.Clamp(timerCurrent - Time.deltaTime, 0, timerMax);

			timerPercentage = timerCurrent / timerMax;

			bar_Background.transform.position = Vector2.Lerp(Vector2.zero, new Vector2(0, -screenSize.y / 2), timerPercentage) + new Vector2(0, 1.5f);
			bar_Background.size = Vector3.Lerp(screenSize, new Vector3(screenSize.x, 0), timerPercentage);
		} else {
			timerPercentage = timerCurrent / timerMax;

			bar_Background.transform.position = Vector3.Lerp(bar_Background.transform.position, Vector2.Lerp(Vector2.zero, new Vector2(0, -screenSize.y / 2), timerPercentage) + new Vector2(0, 1.5f), 5 * Time.deltaTime);
			bar_Background.size = Vector3.Lerp(bar_Background.size, Vector3.Lerp(screenSize, new Vector3(screenSize.x, 0), timerPercentage), 5 * Time.deltaTime);
		}
		
		if (timerCurrent == 0) {
			// Lose
		}
	}

	private int ComputeTilePath (List<Tile> tilePath) {
		int result = tilePath[0].value;

		// Compute
		List<int> operatorsUsed = new List<int>();
		for (int i = 0; i < tilePath.Count - 1; i++) {
			if (tilePath[i].operationType == Tile.OperationType.Addition || operatorsUsed.Contains(i)) {        // Is the previous tile addition? Then use the next tiles operation ALWAYS
				operatorsUsed.Add(i + 1);
				switch (tilePath[i + 1].operationType) {
					case Tile.OperationType.Addition:
						result += tilePath[i + 1].value;
						break;
					case Tile.OperationType.Subtraction:
						result -= tilePath[i + 1].value;
						break;
					case Tile.OperationType.Multiplication:
						result *= tilePath[i + 1].value;
						break;
					case Tile.OperationType.Division:
						result /= tilePath[i + 1].value;
						break;
				}
			} else {
				operatorsUsed.Add(i);
				switch (tilePath[i].operationType) {
					case Tile.OperationType.Subtraction:
						result -= tilePath[i + 1].value;
						break;
					case Tile.OperationType.Multiplication:
						result *= tilePath[i + 1].value;
						break;
					case Tile.OperationType.Division:
						result /= tilePath[i + 1].value;
						break;
				}
			}
		}

		return result;
	}
	
	public bool AttemptSolution (List<Tile> tilePath) {
		bool solutionSuccessful = false;

		int value = ComputeTilePath(tilePath);

		if (value == goal) {
			solutionSuccessful = true;
			audioManager.PlayClip(clip_SolutionSuccess, 1f, 1f);
			StartCoroutine(SolutionSuccessful(value, tilePath));
		} else {
			audioManager.PlayClip(clip_SolutionFail);
			CreateFailureText(tilePath[tilePath.Count - 1], value);
			LoseLife();
		}

		return solutionSuccessful;
	}

	private IEnumerator SolutionSuccessful (int value, List<Tile> tilePath) {
		level++;
		text_LevelTop.text = level.ToString();
		text_LevelBottom.text = level.ToString();
		gamemodeCurrent.SetLevel(level);

		// End Timer Tick Coroutine
		timerCurrent = timerMax;
		if (timerTickCoroutine != null) {
			StopCoroutine(timerTickCoroutine);
		}

		// Lock board
		isLocked = true;
		text_GoalTop.gameObject.SetActive(false);
		text_GoalBottom.gameObject.SetActive(false);

		// Add Screenshake
		cameraController.AddScreenshake(tilePath[tilePath.Count - 2].tileConnections.Single(tc => tc.tile == tilePath[tilePath.Count - 1]).direction * 10, GetTilePathSpin(tilePath));
		DestroyTiles(tilePath);

		// Spawn goalSuccessText
		GoalSuccessText goalSuccessText = Instantiate(prefab_goalSuccessText, text_GoalTop.transform.position, Quaternion.identity).GetComponent<GoalSuccessText>();
		goalSuccessText.SetText(text_GoalTop.text);

		// Create TileCrunch
		CreateTileCrunch(tilePath, ShowMath(tilePath));

		yield return new WaitForSeconds(0.5f);

		yield return StartCoroutine(SpawnNewTiles(tilePath.Count, false));

		// Unlock board
		isLocked = false;
		text_GoalTop.gameObject.SetActive(true);
		text_GoalBottom.gameObject.SetActive(true);
		GetNewGoal();
	}

	private float GetTilePathSpin(List<Tile> tilePath) {
		float spin = 0;

		Vector2 currentDirection;
		Vector2 nextDirection;
		for (int i = 0; i < tilePath.Count - 2; i++) {
			currentDirection = tilePath[i + 1].arrayPosition - tilePath[i].arrayPosition;
			nextDirection = tilePath[i + 2].arrayPosition - tilePath[i + 1].arrayPosition;

			float spinValue = Quaternion.FromToRotation(currentDirection, nextDirection).eulerAngles.z;
			spinValue = (spinValue == 0 ? 0 : (spinValue == 90 ? -1 : 1));
			
			spin += spinValue;
		}
		
		return spin;
	}

	public string ShowMath (List<Tile> tilePath) {
		string mathString = "";

		string operators = "";

		if (tilePath.Count > 0) {
			isDoingMath = true;

			mathString += tilePath[0].value.ToString() + " ";

			List<int> operatorsUsed = new List<int>();

			for (int i = 0; i < tilePath.Count - 1; i++) {
				if (tilePath[i].operationType == Tile.OperationType.Addition || operatorsUsed.Contains(i)) {
					operatorsUsed.Add(i + 1);
					switch (tilePath[i + 1].operationType) {
						case Tile.OperationType.Addition:
							mathString += "+ ";
							operators += "+";
							break;
						case Tile.OperationType.Subtraction:
							mathString += "- ";
							operators += "-";
							break;
						case Tile.OperationType.Multiplication:
							mathString += "× ";
							operators += "×";
							break;
						case Tile.OperationType.Division:
							mathString += "÷ ";
							operators += "÷";
							break;
					}
				} else {
					operatorsUsed.Add(i);
					switch (tilePath[i].operationType) {
						case Tile.OperationType.Subtraction:
							mathString += "- ";
							operators += "-";
							break;
						case Tile.OperationType.Multiplication:
							mathString += "× ";
							operators += "×";
							break;
						case Tile.OperationType.Division:
							mathString += "÷ ";
							operators += "÷";
							break;
					}
				}
				mathString += tilePath[i + 1].value.ToString() + " ";
			}
			mathString.Trim(' ');

			if (mathString.Length > 28) {
				mathString = mathString.Substring(0, 28) + "...";
			}

			text_MathTop.text = mathString;
			text_MathBottom.text = mathString;
		} else {
			isDoingMath = false;
		}
		
		return operators;
	}

	private void DestroyTiles (List<Tile> tilesToDestroy) {
		tilesLastConsumed = tilesToDestroy.Count;

		// Destroy the tiles
		for (int x = 0; x < tiles.GetLength(0); x++) {
			for (int y = 0; y < tiles.GetLength(1); y++) {
				if (tilesToDestroy.Contains(tiles[x, y])) {
					DestroyTile(x, y);
				}
			}
		}
	}

	private void DestroyTile (int x, int y) {
		for (int i = 0; i < tiles[x, y].tileConnections.Count; i++) {
			tiles[x, y].tileConnections[i].tile.tileConnections.Remove(tiles[x, y].tileConnections[i].tile.tileConnections.Single(tc => tc.tile == tiles[x, y]));
		}

		Destroy(tiles[x, y].gameObject);
		tiles[x, y] = null;
	}

	private void CreateTileCrunch (List<Tile> tilePath, string operators) {
		// Reset tileCrunch transform
		tileCrunch.transform.localPosition = Vector3.zero;
		tileCrunch.transform.localScale = Vector3.one;
		tileCrunchCount = tilePath.Count;

		// Set tileCrunch numbers & operators
		for (int i = 0; i < tilePath.Count; i++) {
			// Numbers
			tileCrunchNumbers[i].text = tilePath[i].value.ToString();
			tileCrunchNumbers[i].transform.position = tilePath[i].transform.position;

			// Operators
			if (i < tilePath.Count - 1) {
				tileCrunchOperators[i].text = operators[i].ToString();
				tileCrunchOperators[i].transform.position = (tilePath[i].transform.position + tilePath[i + 1].transform.position) / 2f;
			}
		} 

		// Clear numbers & operators outside of tilePath count
		for (int i = tilePath.Count; i < boardSize * boardSize; i++) {
			tileCrunchNumbers[i].text = "";
			tileCrunchOperators[i - 1].text = "";
		}

		tileCrunchElapsedTime = 0;
	}

	private void CreateFailureText (Tile tileFinal, int value) {
		FloatyText newFloatyText = Instantiate(prefab_FloatyTextFailure, tileFinal.transform.position, Quaternion.identity).GetComponent<FloatyText>();
		newFloatyText.SetText(value.ToString(), BetterColor.pinkLight);
	}

	private IEnumerator SpawnNewTiles (int minTiles, bool firstSpawn = false) {
		int tileCountCurrent = 0;
		for (int a = 0; a < tiles.GetLength(0); a++) {
			for (int b = 0; b < tiles.GetLength(1); b++) {
				if (tiles[a, b] != null) {
					tileCountCurrent++;
				}
			}
		}

		int desiredTiles = 0;
		if (firstSpawn == false) {
			desiredTiles = (int)Mathf.Max((gamemodeCurrent.levelAttributesCurrent.tileCountDesired - (int)Mathf.Round(tileCountCurrent)) + Random.Range(-gamemodeCurrent.levelAttributesCurrent.tileSpawnCountDeviation / 2f, gamemodeCurrent.levelAttributesCurrent.tileSpawnCountDeviation / 2f), 2);
		} else {
			desiredTiles = gamemodeCurrent.tileCountInitial;
		}

		// Spawn tiles
		if (desiredTiles > 1) {
			for (int i = 0; i < desiredTiles; i++) {

				// Find availablePositions
				List<Vector2> availablePositionsNeighbor = new List<Vector2>();
				List<Vector2> availablePositionsRandom = new List<Vector2>();
				for (int x = 0; x < tiles.GetLength(0); x++) {
					for (int y = 0; y < tiles.GetLength(1); y++) {
						if (tiles[x, y] == null) {
							availablePositionsRandom.Add(new Vector2(x, y));
							if ((x > 0 && tiles[x - 1, y] != null) || (y > 0 && tiles[x, y - 1] != null) || (x < boardSize - 1 && tiles[x + 1, y] != null) || (y < boardSize - 1 && tiles[x, y + 1] != null)) {
								availablePositionsNeighbor.Add(new Vector2(x, y));
							}
						}
					}
				}

				// Spawn Tile
				if (availablePositionsNeighbor.Count + availablePositionsRandom.Count > 0) {
					Vector2 randomPosition = Vector2.zero;

					// Get position for new tile
					if ((i < 4 || i % 2 == 1) && availablePositionsNeighbor.Count > 0) {
						randomPosition = availablePositionsNeighbor[Random.Range(0, availablePositionsNeighbor.Count)];
					} else {
						randomPosition = availablePositionsRandom[Random.Range(0, availablePositionsRandom.Count)];
					}

					// Spawn the tile
					SpawnTile(randomPosition);

					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}

	private void SpawnTile (Vector2 pos) {
		// Create Tile prefab
		Tile newTile = Instantiate(prefab_Tile, boardOffset + new Vector2(pos.x * tileSpacing.x, pos.y * tileSpacing.y), Quaternion.identity).GetComponent<Tile>();
		tiles[(int)pos.x, (int)pos.y] = newTile;

		// Get random tile type
		float randomTileType = Random.Range(0.0f, gamemodeCurrent.levelAttributesCurrent.tileWeight_Addition + gamemodeCurrent.levelAttributesCurrent.tileWeight_Subtraction + gamemodeCurrent.levelAttributesCurrent.tileWeight_Multiplication + gamemodeCurrent.levelAttributesCurrent.tileWeight_Division);

		Tile.OperationType newOperationType = Tile.OperationType.Addition;

		float weightCurrent = gamemodeCurrent.levelAttributesCurrent.tileWeight_Addition;
		if (randomTileType > weightCurrent) {
			weightCurrent += gamemodeCurrent.levelAttributesCurrent.tileWeight_Subtraction;
			if (randomTileType > weightCurrent) {
				weightCurrent += gamemodeCurrent.levelAttributesCurrent.tileWeight_Multiplication;
				if (randomTileType > weightCurrent) {
					newOperationType = Tile.OperationType.Division;
				} else {
					newOperationType = Tile.OperationType.Multiplication;
				}
			} else {
				newOperationType = Tile.OperationType.Subtraction;
			}
		} else {
			newOperationType = Tile.OperationType.Addition;
		}

		// Set Tile values
		int tileValue = (int)Mathf.Round(Random.Range(1f, gamemodeCurrent.levelAttributesCurrent.tileValueCap));
		newTile.SetTile(tileValue, newOperationType);
		newTile.arrayPosition = new Vector2((int)pos.x, (int)pos.y);

		// Play Audio
		AudioClip spawnClip = null;
		switch (newTile.operationType) {
			case (Tile.OperationType.Addition):
				spawnClip = clip_TileSelectedAddition;
				break;
			case (Tile.OperationType.Subtraction):
				spawnClip = clip_TileSelectedSubtraction;
				break;
			case (Tile.OperationType.Multiplication):
				spawnClip = clip_TileSelectedMultiplication;
				break;
			case (Tile.OperationType.Division):
				spawnClip = clip_TileSelectedDivision;
				break;
		}
		audioManager.PlayClip(spawnClip, 0.75f, 1.5f);

		CreateTileConnections((int)pos.x, (int)pos.y);
	}

	private void CreateTileConnections (int x, int y) {
		Tile tileCurrent = tiles[x, y];
		// Left connection
		if (x > 0 && tiles[x - 1, y] != null) {
			tileCurrent.tileConnections.Add(new TileConnection(tiles[x - 1, y], new Vector2(-1, 0)));
			tiles[x - 1, y].tileConnections.Add(new TileConnection(tiles[x, y], new Vector2(1, 0)));
		}

		// Right connection
		if (x != boardSize - 1 && tiles[x + 1, y] != null) {
			tileCurrent.tileConnections.Add(new TileConnection(tiles[x + 1, y], new Vector2(1, 0)));
			tiles[x + 1, y].tileConnections.Add(new TileConnection(tiles[x, y], new Vector2(-1, 0)));
		}

		// Down connection
		if (y > 0 && tiles[x, y - 1] != null) {
			tileCurrent.tileConnections.Add(new TileConnection(tiles[x, y - 1], new Vector2(0, -1)));
			tiles[x, y - 1].tileConnections.Add(new TileConnection(tiles[x, y], new Vector2(0, 1)));
		}

		// Up connection
		if (y != boardSize - 1 && tiles[x, y + 1] != null) {
			tileCurrent.tileConnections.Add(new TileConnection(tiles[x, y + 1], new Vector2(0, 1)));
			tiles[x, y + 1].tileConnections.Add(new TileConnection(tiles[x, y], new Vector2(0, -1)));
		}

		
	}

	public IEnumerator StartGame (int gamemodeIndex) {
		gamemodeCurrent = gamemodes[gamemodeIndex];
		level = 1;
		gamemodeCurrent.SetLevel(level);

		InitializeBoard();

		yield return StartCoroutine(SpawnNewTiles(gamemodeCurrent.tileCountInitial, true));

		InitializeTileCrunch();
		GetNewGoal();

		isStarted = true;
	}

	private IEnumerator TimerTickCoroutine () {

		float timeDelay = 2f;
		int iterations = 6;
		int tickCount = (int)(8f / timeDelay);

		float pitch = 0.75f;

		for (int i = 0; i < iterations; i++) {
			for (int r = 0; r < tickCount; r++) {
				yield return new WaitForSeconds(timeDelay);
				//audioManager.PlayClip(clip_TimerTick, 1f, Mathf.Lerp(1.5f, 1f, timerCurrent / timerMax));
				audioManager.PlayClip(clip_TimerTick, 1f, pitch);
				pitch += 0.0375f;
			}

			if (i == (iterations - 1)) {
				for (int r = 0; r < tickCount - 1; r++) {
					yield return new WaitForSeconds(timeDelay);
					//audioManager.PlayClip(clip_TimerTick, 1f, Mathf.Lerp(1.5f, 1f, timerCurrent / timerMax));
					audioManager.PlayClip(clip_TimerTick, 1f, pitch);
					pitch += 0.0375f;
				}
			}

			timeDelay *= 0.5f;
		}
	}

	public void LoseLife () {
		// Subtract one life, cause GameOver if lives = 0

		// Subtract 1 life
		lives = (int)Mathf.Clamp(lives - 1, 0, Mathf.Infinity);

		// If we have zero lives, GameOver
		if (lives == 0) {
			GameOver();
		}
	}

	public void GameOver () {
		if (isGameOver == false) {
			isGameOver = true;
		}
	}

}
