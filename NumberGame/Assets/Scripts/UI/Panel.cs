using System.Collections;
using UnityEngine;

public class Panel : MonoBehaviour {

	public UIObject[] uiObjects;

	public bool isActive;

	private void Start () {
		if (isActive == true) {
			ActivatePanel();
		} else {
			DeactivatePanelInstantly();
		}
	}

	public void ActivatePanel () {
		// First, Hide all of the uiObjects
		foreach (UIObject uiObjectCurrent in uiObjects) {
			uiObjectCurrent.Hide();
		}

		StartCoroutine(SpawnButtons());
	}

	public void DeactivatePanelInstantly () {
		foreach (UIObject uiObjectCurrent in uiObjects) {
			uiObjectCurrent.Hide();
		}
	}

	private IEnumerator SpawnButtons () {
		// Spawn each uiObject this panel has, one by one
		foreach (UIObject uiObjectCurrent in uiObjects) {
			yield return new WaitForSeconds(0.1f);
			uiObjectCurrent.Spawn();
		}
	}

	public IEnumerator DeactivatePanel () {
		yield return new WaitForSeconds(0.25f);

		// Despawn each uiObject this panel has, one by one
		foreach (UIObject uiObjectCurrent in uiObjects) {
			if (uiObjectCurrent is UIButton == false || (uiObjectCurrent is UIButton && (uiObjectCurrent as UIButton).isClickable == true)) {
				uiObjectCurrent.Despawn();
				yield return new WaitForSeconds(0.1f);				
			}
		}
	}



}
