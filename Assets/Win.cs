using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour {
	
	public GM game;

	public string playerTag = "Player";

	void OnTriggerEnter2D(Collider2D col){
		if(col.gameObject.tag.Equals(playerTag)){
			PlayerMovement movement = col.gameObject.GetComponent<PlayerMovement> ();
			movement.rb.velocity = Vector2.zero;
			movement.enabled = false;
			game.UpdateLevel ();
		}
	}

}
