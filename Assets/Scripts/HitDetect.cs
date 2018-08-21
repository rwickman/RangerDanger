using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetect : MonoBehaviour {
	
	private string groundTag = "Ground";
	private string treeTag = "Tree";
	private PlayerAlive player;
	// Use this for initialization
	void Start () {
		
		player = GetComponentInParent<PlayerAlive> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerStay2D(Collider2D col){
		if ( player.isAlive && (col.gameObject.tag.Equals (groundTag) || col.gameObject.tag.Equals (treeTag) ) && col.transform.parent.gameObject.GetComponent<GameTile> ().GetIsOnFire ()) {
			print ("Player is dead");
			player.Kill();
		}
		//print ("Touching: " + col.gameObject.name);
	}
}
