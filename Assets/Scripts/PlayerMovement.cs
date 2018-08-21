using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed =7f;

	public float smooth = 0.8f;

	public Rigidbody2D rb;

	public Sprite behindSprite;
	public Sprite forwardSprite;

	private SpriteRenderer rend;
	private bool isFlipped;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		rend = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		//if (h != 0 || v != 0)
		Move (h, v);
	}

	void Move(float x, float y){
		rb.velocity = Vector2.Lerp (Vector2.zero, new Vector2 (x, y) * speed, 0.9f);
		if (y > 0) {
			rend.sprite = behindSprite;
		} else if (y < 0) {
			rend.sprite = forwardSprite;
		}

		if (x > 0 && !isFlipped) {
			isFlipped = true;
			rend.flipX = true;
		} else if (x < 0 && isFlipped) {
			isFlipped = false;
			rend.flipX = false;
		}
	}
}
