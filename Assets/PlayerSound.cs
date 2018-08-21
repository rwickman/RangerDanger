using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour {

	public AudioClip[] randomSounds;
	public AudioClip deathSound;
	public AudioClip winSound;
	public float soundDelayMin;
	public float soundDelayMax;

	private float nextSound = 5f;
	private AudioSource audS;
	private PlayerAlive alive;
	// Use this for initialization
	void Start () {
		nextSound = Time.time + 5f;
		audS = GetComponent<AudioSource> ();
		alive = GetComponent<PlayerAlive> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextSound && randomSounds.Length > 0 && alive.isAlive) {
			audS.Stop ();
			audS.PlayOneShot (randomSounds[Random.Range (0, randomSounds.Length)]);
			nextSound += Random.Range (soundDelayMin, soundDelayMax);
		}
	}

	public void playDeathSound(){
		audS.Stop ();
		audS.PlayOneShot (deathSound);
	}

	public void playWinSound(){
		audS.Stop ();
		audS.PlayOneShot (winSound);
	}

}
