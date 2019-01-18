using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Gamemode {

	[Space(10)]
	public string name = "new gamemode";

	[Space(10)][Header("Level Modifiers")]
	public List<LevelAttributes> levelAttributes;
	public LevelAttributes levelAttributesCurrent;

	[Space(10)][Header("Tile Count Settings")]
	public int tileCountInitial;                    // The starting number of tiles
	
	public void SetLevel (int level) {
		if (levelAttributes.Count >= 2) {
			LevelAttributes levelAttributesA = null;
			LevelAttributes levelAttributesB = null;

			for (int i = 1; i < levelAttributes.Count; i++) {
				if (level <= levelAttributes[i].level) {
					levelAttributesA = levelAttributes[i - 1];
					levelAttributesB = levelAttributes[i];
				}
			}

			if (levelAttributesA == null && levelAttributesB == null) {
				levelAttributesCurrent = new LevelAttributes(levelAttributes[levelAttributes.Count - 1]);
			} else {
				levelAttributesCurrent.Interpolate(levelAttributesA, levelAttributesB, level);
			}
		} else {
			levelAttributesCurrent = new LevelAttributes(levelAttributes[0]);
		}
	}

}

[System.Serializable]
public class LevelAttributes {

	public string name;
	public int level;

	[Space(10)][Header("Tile Values")]
	public float tileValueCap;					// The maximum value any spawned tile can have

	[Space(10)][Header("Tile Spawning")]
	public float tileCountDesired;				// The number of tiles that will spawn per level
	public float tileSpawnCountDeviation;		// The deviation between tileSpawnCount and the true number of tiles spawned each level

	[Space(10)][Header("Tile Weights")]
	public float tileWeight_Addition;
	public float tileWeight_Subtraction;
	public float tileWeight_Multiplication;
	public float tileWeight_Division;

	[Space(10)][Header("Tile Weights")]
	public float comboLength;
	public float comboLengthDeviation;

	public LevelAttributes (LevelAttributes copiedLevelAttributes) {
		name = copiedLevelAttributes.name;

		level = copiedLevelAttributes.level;

		tileValueCap = copiedLevelAttributes.tileValueCap;

		tileCountDesired = copiedLevelAttributes.tileCountDesired;
		tileSpawnCountDeviation = copiedLevelAttributes.tileSpawnCountDeviation;

		tileWeight_Addition = copiedLevelAttributes.tileWeight_Addition;
		tileWeight_Subtraction = copiedLevelAttributes.tileWeight_Subtraction;
		tileWeight_Multiplication = copiedLevelAttributes.tileWeight_Multiplication;
		tileWeight_Division = copiedLevelAttributes.tileWeight_Division;

		comboLength = copiedLevelAttributes.comboLength;
		comboLengthDeviation = copiedLevelAttributes.comboLengthDeviation;
}

	public void Interpolate (LevelAttributes levelAttributesA, LevelAttributes levelAttributesB, int currentLevel) {
		level = currentLevel;

		float valueBetweenLevels = (float)currentLevel / ((float)levelAttributesB.level - (float)levelAttributesA.level);

		tileValueCap = Mathf.Lerp(levelAttributesA.tileValueCap, levelAttributesB.tileValueCap, valueBetweenLevels);

		tileCountDesired = Mathf.Lerp(levelAttributesA.tileCountDesired, levelAttributesB.tileCountDesired, valueBetweenLevels);
		tileSpawnCountDeviation = Mathf.Lerp(levelAttributesA.tileSpawnCountDeviation, levelAttributesB.tileSpawnCountDeviation, valueBetweenLevels);

		tileWeight_Addition = Mathf.Lerp(levelAttributesA.tileWeight_Addition, levelAttributesB.tileWeight_Addition, valueBetweenLevels);
		tileWeight_Subtraction = Mathf.Lerp(levelAttributesA.tileWeight_Subtraction, levelAttributesB.tileWeight_Subtraction, valueBetweenLevels);
		tileWeight_Multiplication = Mathf.Lerp(levelAttributesA.tileWeight_Multiplication, levelAttributesB.tileWeight_Multiplication, valueBetweenLevels);
		tileWeight_Division = Mathf.Lerp(levelAttributesA.tileWeight_Division, levelAttributesB.tileWeight_Division, valueBetweenLevels);

		comboLength = Mathf.Lerp(levelAttributesA.comboLength, levelAttributesB.comboLength, valueBetweenLevels);
		comboLengthDeviation = Mathf.Lerp(levelAttributesA.comboLengthDeviation, levelAttributesB.comboLengthDeviation, valueBetweenLevels);
	}

}
