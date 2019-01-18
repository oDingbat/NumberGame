using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	[Space(10)][Header("References")]
	public GameObject prefab_jukebox;
	public AudioSource[] jukeboxes;

	[Space(10)][Header("Settings")]
	public int jukeboxCount = 32;

	// Hidden Variables
	int jukeboxIncrement = 0;

	private void Start () {
		InitializeJukeboxes();
	}

	private void InitializeJukeboxes () {
		jukeboxes = new AudioSource[jukeboxCount];

		for (int i = 0; i < jukeboxCount; i++) {
			AudioSource newJukebox = Instantiate(prefab_jukebox, Vector3.zero, Quaternion.identity, transform).GetComponent<AudioSource>();
			jukeboxes[i] = newJukebox;

			newJukebox.name = "Jukebox (" + i + ")";
		}
	}

	public void PlayClip (AudioClip clip) {
		PlayClip(clip, 1f, 1f);
	}

	public void PlayClip (AudioClip clip, float volume) {
		PlayClip(clip, volume, 1f);
	}

	public void PlayClip (AudioClip clip, float volume, float pitch) {
		// Stop the jukebox (in case it's already playing something)
		jukeboxes[jukeboxIncrement].Stop();

		// Set jukebox variables
		jukeboxes[jukeboxIncrement].clip = clip;
		jukeboxes[jukeboxIncrement].volume = volume;
		jukeboxes[jukeboxIncrement].pitch = pitch;
		jukeboxes[jukeboxIncrement].spatialBlend = 0;			// Disable dimensional sound

		// Play jukebox
		jukeboxes[jukeboxIncrement].Play();

		// Increment jukeboxIncrement
		jukeboxIncrement = (jukeboxIncrement == jukeboxCount - 1 ? 0 : jukeboxIncrement + 1);
	}

}
