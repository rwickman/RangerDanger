using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour {

	public int xTiles;
	public int yTiles;
	public float treeMinDistance;
	public Transform player;
	public GameObject end;
	public GM game;
	public float minDistBetweenPaths;
	public GameObject coinObj;

	public int numVoronoiCells = 10;

	public bool lightFire = true, generateTrees = true, visualizeVoronoi;
	public GameObject groundTile, tree, bush, groundContainer;

	public int level = 1;

	private Vector2 fireStart;
	private GameObject[,] tiles;
	private Vector2[] miscPaths;
	private Vector2 exit;

	// Use this for initialization
	void Start () {
		if (xTiles <= 1) {
			xTiles = 2;
		}
		if (yTiles <=1){
			yTiles = 2;
		}
		//minDistBetweenPaths = Mathf.Clamp (minDistBetweenPaths, 1, Mathf.Min (xTiles, yTiles)); //Should probably still have a better check than this
		//exit = end.position;
		StartCoroutine("showFirstLevel");
		GenerateEnvironment ();
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void GenerateEnvironment(){
		if (level > 1) {
			foreach (Transform child in transform) {
				Destroy (child.gameObject);
			}
		}
		xTiles = 50 * level;
		yTiles = 50 * level;
		numVoronoiCells = 50 * level;
		minDistBetweenPaths = 20 * level;

		player.transform.position = new Vector2 (xTiles / 2, yTiles / 2);
		tiles = new GameObject[xTiles,yTiles];
		//Create exit
		Dictionary<string, Vector2[]> pathEPs =  GeneratePathEndPoints (level);

		exit = pathEPs ["exit"][0];

		GameObject exitObj = Instantiate (end, exit, Quaternion.identity, transform);
		exitObj.GetComponent<Win> ().game = this.game;

		//Instantiate GameObject for environment
		for (int i = 0; i < xTiles; i++) {
			for (int j = 0; j < yTiles; j++) {
				tiles [i,j] = Instantiate (groundContainer, new Vector2 (i, j), Quaternion.identity, transform);
				Instantiate (groundTile, new Vector2 (i, j), Quaternion.identity, tiles [i,j].transform);
				if ((i ==0 || j == 0 || i == xTiles -1 || j == yTiles - 1) && !(i == exit.x && j == exit.y)){
					GameObject bushCon = new GameObject("Bush Container");
					bushCon.transform.parent = tiles [i,j].transform;
					Instantiate (bush, new Vector2 (i, j), Quaternion.identity, bushCon.transform);
				}
			}
		}
		if (generateTrees) {
			GenerateTrees ();
		}


		//Set neighbors (Should optimize this later)
		for (int i = 0; i < xTiles; i++) {
			for (int j = 0; j < yTiles; j++) {
				GameTile gt= tiles [i, j].GetComponent<GameTile>();
				Dictionary<string, GameTile> neighbors = new Dictionary<string, GameTile>();
				if (i != 0) {
					neighbors.Add ("Top", tiles [i - 1, j].GetComponent<GameTile> ());
				}
				if (i != xTiles - 1) {
					neighbors.Add ("Bottom", tiles [i + 1, j].GetComponent<GameTile> ());
				}
				if (j != 0) {
					neighbors.Add ("Left", tiles [i , j -1].GetComponent<GameTile> ());
				}
				if (j != yTiles - 1) {
					neighbors.Add ("Right", tiles [i, j + 1].GetComponent<GameTile> ());
				}
				gt.SetNeighbors (neighbors);
			}
		}
		GeneratePaths (pathEPs);
		if (lightFire) {
			StartBurn (pathEPs["fire"]);
		}


	}
		

	void StartBurn(Vector2[] startPositions){
		foreach (Vector2 startPos in startPositions) {
			tiles [(int)startPos.x, (int)startPos.y].GetComponent<GameTile> ().SetOnFire ();
		}
	}

	/// Will use Poisson-Disc Sampling Algorithm to generate trees
	private void GenerateTrees(){
		PoissonDiscSampler sampler = new PoissonDiscSampler (xTiles - 3, yTiles -3 , treeMinDistance);
		foreach (Vector2 sample in sampler.Samples()) {
			int xPos = (int)sample.x;
			int yPos = (int)sample.y;
			Instantiate (tree, sample, Quaternion.identity, tiles[xPos,yPos].transform);
		}
	}

	private void GeneratePaths(Dictionary<string, Vector2[]> pathEPs){
		VoronoiDiagram vd = new VoronoiDiagram (numVoronoiCells, tiles, player.position, pathEPs, visualizeVoronoi, level, this);
		vd.createDiagram ();

	}

	/// <summary>
	/// Generates the exit and fire, index 0 and 1 will be exit and fire, respectively.
	/// </summary>
	/// <returns>The exit and fire.</returns>
	private Dictionary<string, Vector2[]> GeneratePathEndPoints(int level = 1){
		// Number of paths is equal to level * 2
		int direction = Random.Range (1, 5); //bot == 1, top == 2, left == 3, right == 4
		int x = 0, y = 0;

		Dictionary<string, Vector2[]> pathEPs = new Dictionary<string, Vector2[]> ();
		pathEPs.Add ("exit", new Vector2[1]);
		pathEPs.Add ("fire", new Vector2[level]);
		pathEPs.Add ("misc", new Vector2[level - 1]);


		Vector2[] pos = new Vector2[level * 2];	//Used to temporarily store all the positions for checking (wouldn't need this if optimized)
		Vector2 testPoint;

		//Need to handle special case for level = 1
		for (int i = 0; i < level * 2; i++) {
			//Until I prove this wont run forever (because of the min distance requirement) I need to check for tries
			int tries = 0;

			do {
				if (i > 1 || level > 1) {
					direction = Random.Range (1, 5);
				}
				switch (direction) {
				case 1:
					x = Random.Range (1, xTiles - 1);
					y = 0;
					if(level == 1)
						direction = 2;
					break;
				case 2:
					x = Random.Range (1, xTiles - 1);
					y = yTiles -1;
					if(level == 1)
						direction = 1;
					break;
				case 3:
					x = 0;
					y = Random.Range(1, yTiles -1);
					if(level == 1)
						direction = 4;
					break;
				case 4:
					x = xTiles - 1;
					y = Random.Range(1, yTiles -1);
					if(level ==1)
						direction = 3;
					break;
				}
				testPoint = new Vector2 (x, y);
				tries++;
				if(tries > 10){
					break;
				}
			} while(!isDistanceGreaterThanX(testPoint, pos, minDistBetweenPaths));
			//Check which element in the dict to add it to
			if (i == 0)
				pathEPs ["exit"] [0] = testPoint;
			else if (i <= level) 
				pathEPs ["fire"] [i - 1] = testPoint;
			else 
				pathEPs ["misc"] [i - level - 1] = testPoint;
			
			pos [i] = testPoint;
		}
		return pathEPs;
	}

	private bool isDistanceGreaterThanX(Vector2 toTest, Vector2[] endpoints, float x){
		foreach (Vector2 endPoint in endpoints) {
			if (Vector2.Distance (toTest, endPoint) < x) {
				return false;
			}
		}
		return true;
	}
	IEnumerator showFirstLevel(){
		game.levelPanel.SetActive (true);
		yield return new WaitForSeconds (2f);
		game.levelPanel.SetActive (false);
	}

	public void createCoin(Vector2 pos){
		Instantiate (coinObj, pos, Quaternion.identity, transform).SetActive(true);
	}
}
