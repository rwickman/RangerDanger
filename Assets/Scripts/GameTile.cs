using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour {
	public enum BurnType {NonFlammable = 0, Slow, Normal, Fast}; //0, 0.5 modifier, 1 modifier, 2 modifier respectively

	public BurnType type;

	public float spreadBurnTimeMin = 0.1f;
	public float spreadBurnTimeMax = 0.2f;
	public float groundBurnDelayMin = 0.1f;
	public float groundBurnDelayMax = 0.3f;

	public Sprite onFireTree;
	public Sprite onFireBush;

	public GridPoint closestGridPoint;

	private Dictionary<string, GameTile> neighbors;

	private bool isOnFire;
	private float nextBurnTime = 0f;
	private bool spreadFireRunning = false;
	// Use this for initialization
	void Start () {
		//print(SetOnFire ());
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (isOnFire && neighbors.Count > 0 && !spreadFireRunning) {
			//StartCoroutine("SpreadFire");
			StartCoroutine("SpreadFire");
			/*
			if (transform.Find ("Tree(Clone)") || transform.Find ("Bush(Clone)")) {
				nextBurnTime = Time.time + spreadBurnTime;
			} else {
				nextBurnTime = Time.time + spreadBurnTime + groundBurnDelay;
			}*/

		}
	}
		
	public bool SetOnFire(){
		if (type != BurnType.NonFlammable && !isOnFire) {
			isOnFire = true;
			gameObject.transform.GetChild (0).GetComponent<SpriteRenderer>().color = Color.red;
			Transform tempTree = transform.Find ("Tree(Clone)");
			Transform tempBush = transform.Find ("Bush Container");
			if (tempTree) {
				tempTree.GetComponent<SpriteRenderer> ().sprite = onFireTree;
			}
			if (tempBush) {
				Transform tempBushClone= tempBush.Find ("Bush(Clone)");
				if (tempBushClone) {
					tempBushClone.GetComponent<SpriteRenderer> ().sprite = onFireBush;
				}

			}

		}
		return isOnFire;
	}

	public bool GetIsOnFire(){ return isOnFire; }

	public void SetNeighbors(Dictionary<string, GameTile> n){
		neighbors = n;
	}
	public void removeTree(){
		Transform tree = transform.Find("Tree(Clone)");
		if (tree) {
			Destroy (tree.gameObject);
		}
	}

	public Dictionary<string, GameTile> GetNeighbors() { return neighbors; }

	IEnumerator SpreadFire(){
		spreadFireRunning = true;
		bool foundTreeOrBush = false;
		//Loop through all the neighbors and add its key to the ArrayList equivalent to its type
		ArrayList  nList = new ArrayList ();
		foreach (KeyValuePair<string, GameTile> gt in neighbors) {
			//If has Tree or Bush, Burn this tile first
			if (gt.Value.transform.Find ("Tree(Clone)")) { //|| gt.Value.transform.Find ("Bush(Clone)")
				yield return new WaitForSeconds (Random.Range(spreadBurnTimeMin, spreadBurnTimeMax));
				gt.Value.SetOnFire ();
				neighbors.Remove (gt.Key);
				foundTreeOrBush = true;
				break;
			}
			for (int i = 0; i < (int)gt.Value.type; i++) {
				nList.Add (gt.Key);
			}
		}
		if (!foundTreeOrBush) {
			yield return new WaitForSeconds(Random.Range(spreadBurnTimeMin, spreadBurnTimeMax) + Random.Range(groundBurnDelayMin, groundBurnDelayMax));
			int ranNum = Random.Range(0, nList.Count);
			string choice = (string)nList [ranNum];
			neighbors [choice].SetOnFire ();
			neighbors.Remove (choice);
		}
		spreadFireRunning = false;
	}
}
