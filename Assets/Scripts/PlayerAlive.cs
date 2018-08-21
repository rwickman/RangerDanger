using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAlive : MonoBehaviour {

	public bool isAlive = true;
	public GameObject deadPanel;
	public Sprite deadSprite;

	private PlayerMovement movement;
	private PlayerSound sound;
	private SpriteRenderer rend;
	// Use this for initialization
	void Start () {
		movement = GetComponent<PlayerMovement> ();
		sound = GetComponent<PlayerSound> ();
		rend = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Kill(){
		isAlive = false;
		movement.rb.velocity = Vector2.zero;
		movement.enabled = false;
		rend.sprite = deadSprite;
		sound.playDeathSound ();
		deadPanel.SetActive (true);
	}
}
