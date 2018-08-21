using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GridPoint  {
	public Vector2 point;
	public Color gridColor;
	public ArrayList cells; // Arraylist of GameObject of the GroundContainers
	public ArrayList neighbors; // Will be an arraylist of GridPoint
	public float cost;

	public  GridPoint (){
		cells = new ArrayList ();
		neighbors = new ArrayList ();
	}

	public GridPoint getLeastCostNeighbor(){
		try{
			float min = Mathf.Infinity;
			GridPoint least = null;
			foreach (GridPoint n in neighbors) {
				if (n.cost < min) {
					least = n;
					min = n.cost;
				}
			}
			return least;
		} catch(Exception e){
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}
		return null;
	}

	public GridPoint getLeastCostNeighbor(Vector2 to){
		try{
			float min = Mathf.Infinity;
			GridPoint least = null;
			foreach (GridPoint n in neighbors) {
				float dist = Vector2.Distance(n.point, to);
				if (dist < min) {
					least = n;
					min = dist;
				}
			}
			return least;
		} catch(Exception e){
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}
		return null;
	}
	public void changeAllColors(Color c, bool visualize){
		if (visualize) {
			gridColor = c;

			foreach (GameObject cell in cells) {
				cell.transform.Find ("GroundSprite(Clone)").gameObject.GetComponent<SpriteRenderer> ().color = gridColor;
			}
		}
	}

	public void removeAllTrees(){
		foreach (GameObject cell in cells) {
			cell.GetComponent<GameTile> ().removeTree ();
		}
	}
}
	

public class VoronoiDiagram {

	private int numCells;
	private ArrayList colors;
	private GameObject[,] tiles; // This contains the actually gameobject tiles
	private ArrayList gridVectorList;  // This is a temp data structure to be used to store info about grid pointvalues
	private GridPoint[] grid;

	private Vector2 start, end;
	private Vector2[] miscPaths, fireStart;
	private bool visualize;
	private int level;

	private EnvironmentManager ev;



	public VoronoiDiagram(int numCells, GameObject[,] tilesObj, Vector2 start, Dictionary<string, Vector2[]> pathEPs, bool visualize, int level, EnvironmentManager evi){
		this.numCells = numCells;
		tiles = tilesObj;
		grid = new GridPoint[numCells + level*2 + 1]; // numCells + 1 will be the player pos and numCells + 2 will be the endPoint, numCells + 3 will be fireStart
		colors = new ArrayList ();
		gridVectorList = new ArrayList ();
		this.start = start;
		this.end = pathEPs["exit"][0];
		this.visualize = visualize;
		this.fireStart = pathEPs["fire"];
		this.miscPaths = pathEPs["misc"];
		this.level = level;
		ev = evi;
	}
		
	/// <summary>
	/// Creates the Voronoi diagram.
	/// </summary>
	public void createDiagram(){
		genGridPoints ();
		for (int i = 0; i < tiles.GetLength (0); i++) {
			for (int j = 0; j < tiles.GetLength (1); j++) {
				GridPoint closest = getClosestPoint (new Vector2 (i, j));
				/*if (closest.gridColor.Equals(grid[numCells + 1].gridColor)) {
					Debug.Log ("Closest to the end");
				}*/
				if (visualize) {
					tiles [i, j].transform.Find ("GroundSprite(Clone)").gameObject.GetComponent<SpriteRenderer> ().color = closest.gridColor;
				}
				tiles [i, j].GetComponent<GameTile>().closestGridPoint = closest;
				closest.cells.Add (tiles [i, j]);
			}
		}
		establishEdges ();
		createPath ();
	}

	private void establishEdges(){
		// Loop through all of the GridPoints
		for (int i = 0; i < grid.GetLength (0); i++) {
			// Loop through all cells belonging to the GridPoint
			foreach(GameObject cell in grid[i].cells){
				//This will loop through all of the cells neighbors and check if the neighbor is within the same Island
				//i.e., they share the same closestPoint
				GameTile cellGT = cell.GetComponent<GameTile>();
				foreach (KeyValuePair<string, GameTile> gt in cellGT.GetNeighbors()) {
					//If they are not in the same island, then it must be a neighbor
					if (!gt.Value.closestGridPoint.Equals (cellGT.closestGridPoint) && !grid[i].neighbors.Contains(gt.Value.closestGridPoint)) {
						grid [i].neighbors.Add (gt.Value.closestGridPoint);
						//Debug.Log ("Not within the same island");
					}
				}
			}
		}
	}

	private GridPoint getClosestPoint(Vector2 p){
		GridPoint closest = grid[0];
		float closestDist = Vector2.Distance (p, closest.point);
		for (int i = 1; i < grid.Length; i++) {
			float curr = Vector2.Distance (grid [i].point, p);
			if (curr < closestDist) {
				closestDist = curr;
				closest = grid [i];
			}
		}
		return closest;
	}

	private void genGridPoints() {
		for (int i = 0; i < numCells; i++) {
			GridPoint gp = new GridPoint();
			gp.point = randomPoint ();
			gp.gridColor = genColor ();
			gp.cost = Vector2.Distance (gp.point, end);
			grid[i] = gp;
		}
		// Handle the player and end points
		// They won't need a random point
		GridPoint playerPoint = new GridPoint();
		GridPoint endPoint = new GridPoint();


		playerPoint.point = start;
		playerPoint.gridColor = genColor ();
		playerPoint.cost = Vector2.Distance (start, end);
		grid [numCells] = playerPoint;

		endPoint.point = end;
		endPoint.gridColor = genColor ();
		endPoint.cost = 0f;
		grid [numCells + 1] = endPoint;

		for (int i = 0; i < fireStart.Length; i++) {
			GridPoint firePoint= new GridPoint ();
			firePoint.point = fireStart[i];
			firePoint.gridColor = genColor ();
			firePoint.cost = Vector2.Distance (start, end);
			grid [numCells + 2 + i] = firePoint;
		}

		for (int i = 0; i < miscPaths.Length; i++) {
			GridPoint misc= new GridPoint ();
			misc.point = miscPaths[i];
			misc.gridColor = genColor ();
			misc.cost = Vector2.Distance (start, end);
			grid [numCells + 2 + i + fireStart.Length] = misc;
		}
	}

	private Color genColor(){
		Color col;
		do {
			col = new Color (UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
		} while(colors.Contains (colors));
		colors.Add (col);
		return col;
	}

	private Vector2 randomPoint(){
		Vector2 gridPos;
		do {
			gridPos = new Vector2 (UnityEngine.Random.Range (0, tiles.GetLength (0)), UnityEngine.Random.Range (0, tiles.GetLength (1)));
		} while(gridVectorList.Contains (gridPos));
		gridVectorList.Add (gridPos);
		return gridPos;
	}

	/// <summary>
	/// Creates the least cost path from player to end, with the key numbered [0,n].
	/// </summary>
	/// <returns>The least cost path.</returns>
	private Dictionary<int, GridPoint> createLeastCostPath(){
		// This method may never converge to the least cost path
		Dictionary<int, GridPoint> path = new Dictionary<int, GridPoint> ();
		int count = 0;
		GridPoint curr = grid [numCells]; //The start grid point of the player
		path.Add(count, curr);
		count++;
		//This sometimes throws an error NullReferenceException: Object reference not set to an instance of an object
		while (!curr.Equals(grid[numCells +1])){
			curr = curr.getLeastCostNeighbor ();
			if (path.ContainsValue (curr)) {
				Debug.Log ("Wasn't able to converge!");
				break;
			}
			path.Add (count, curr);
			count++;
		}
		return path;
	}
	/// <summary>
	/// Creates the least cost path towards the fire
	/// </summary>
	/// <returns>The least cost path.</returns>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	private Dictionary<int, GridPoint> createLeastCostPath(Vector2 toP, GridPoint toPoint){
		// This method may never converge to the least cost path
		Dictionary<int, GridPoint> path = new Dictionary<int, GridPoint> ();
		int count = 0;
		GridPoint curr = grid [numCells]; //The start grid point of the player
		path.Add(count, curr);
		count++;
		while (!curr.Equals(toPoint)){
			curr = curr.getLeastCostNeighbor (toP);
			if (path.ContainsValue (curr)) {
				Debug.Log ("Wasn't able to converge!");
				break;
			}
			path.Add (count, curr);
			count++;
		}
		return path;
	}

	/// <summary>
	/// Creates the path. Utilizes createLeastCostPath to find path from start to finish. Also will remove trees along path.
	/// </summary>
	private void createPath(){
		Dictionary<int, GridPoint> leastCostPath = createLeastCostPath ();
		Dictionary<int, GridPoint>[] trickPaths = new Dictionary<int, GridPoint>[miscPaths.Length + fireStart.Length]; // The +1 is for the firePath

		for(int i = 0; i < fireStart.Length + miscPaths.Length; i++){
			Dictionary<int, GridPoint> tempPath;
			if (i < fireStart.Length) {
				tempPath = createLeastCostPath (fireStart [i], grid [numCells + 2 + i]);
			} else {
				tempPath = createLeastCostPath (miscPaths [i - fireStart.Length], grid[numCells + 2 + i]);
			}
			trickPaths [i] = tempPath;
		}

		Color playerColor = leastCostPath [0].gridColor;
		foreach (KeyValuePair<int, GridPoint> gp in leastCostPath) {
			//Dont need to change the color of the player since its already its own color
			if (gp.Key != 0) {
				gp.Value.changeAllColors (playerColor, visualize);
			}
			gp.Value.removeAllTrees ();
			if (!gp.Value.point.Equals (start) && !gp.Value.point.Equals (end)) {
				ev.createCoin (gp.Value.point);
			}
		}
		//This is for all the paths that are not to the actaul goal
		Debug.Log("Num trickPaths: " + trickPaths.Length);
		for (int i = 0; i < trickPaths.Length; i++) {
			Dictionary<int, GridPoint> currPath = trickPaths [i];
			int count = 0;
			foreach (KeyValuePair<int, GridPoint> gp in currPath) {
				if (gp.Key != 0) {
					gp.Value.changeAllColors (playerColor, visualize);
				}
				if (count < currPath.Count - 1) {
					gp.Value.removeAllTrees ();
					Debug.Log ("Create coin Called");
					if (!gp.Value.point.Equals (start) && !gp.Value.point.Equals (end) ) {
						ev.createCoin (gp.Value.point);
					}
					count++;
				}
			}
		}
	}
		
}
