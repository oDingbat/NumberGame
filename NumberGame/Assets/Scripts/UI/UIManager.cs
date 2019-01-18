using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	[Space(10)][Header("References")]
	public Board board;
	public Panel panelCurrent;
	public Panel panelGame;

	public void ChangeActivePanel (Panel panelNew) {
		StartCoroutine(ChangeActivePanelCoroutine(panelNew));
	}
	
	public void QuitGame () {
		StartCoroutine(QuitGameCoroutine());
	}

	public void StartGamemode (int gamemodeIndex) {
		StartCoroutine(StartGamemodeCoroutine(gamemodeIndex));
	}

	private IEnumerator StartGamemodeCoroutine (int gamemodeIndex) {
		// Deactivate current panel
		yield return StartCoroutine(panelCurrent.DeactivatePanel());

		yield return new WaitForSeconds(0.25f);

		panelCurrent.gameObject.SetActive(false);

		// Activate game panel		
		panelCurrent = panelGame;
		panelCurrent.gameObject.SetActive(true);

		yield return new WaitForSeconds(0.0625f);

		panelCurrent.ActivatePanel();

		yield return new WaitForSeconds(1f);

		StartCoroutine(board.StartGame(gamemodeIndex));
	}

	private IEnumerator QuitGameCoroutine () {
		yield return StartCoroutine(panelCurrent.DeactivatePanel());

		yield return new WaitForSeconds(0.125f);

		Application.Quit();
	}

	private IEnumerator ChangeActivePanelCoroutine (Panel panelNew) {
		yield return new WaitForSeconds(0.0625f);

		// Deactivate current panel
		yield return StartCoroutine(panelCurrent.DeactivatePanel());
		

		yield return new WaitForSeconds(0.125f);

		panelCurrent.gameObject.SetActive(false);

		// Activate new panel		
		panelCurrent = panelNew;
		panelCurrent.gameObject.SetActive(true);
		panelCurrent.ActivatePanel();
	}

}
