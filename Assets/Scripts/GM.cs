using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GM : MonoBehaviour {
	public EnvironmentManager environment;
	public PlayerMovement movement;
	public PlayerSound sound;
	public GameObject player;

	public GameObject levelPanel;
	public GameObject winPanel;
	public Text winPointText;

	public int maxLevels = 3;

	public float levelTransitionDelay = 2f;
	private float initPlayerMovement = 0f;


	public void UpdateLevel(){
		StartCoroutine ("updateLevel");
	}
	private IEnumerator updateLevel(){
		Debug.Log ("Updating Level");
		++environment.level;
		if (environment.level > maxLevels) {
			sound.playWinSound ();
			winPointText.text = "You Scored: " + player.GetComponent<CoinCollect> ().points;
			player.GetComponent<CoinCollect> ().pointsText.enabled = false;
			winPanel.SetActive(true);
		} else{
			levelPanel.GetComponentInChildren<Text> ().text = "Level: " + environment.level;
			levelPanel.SetActive (true);
			environment.GenerateEnvironment();
			yield return new WaitForSeconds (levelTransitionDelay);
			levelPanel.SetActive (false);
			movement.enabled = true;
			if (initPlayerMovement == 0f) {
				initPlayerMovement = movement.speed;
			}
			movement.speed = initPlayerMovement + environment.level;
		}

	}

	public void StartGame(){
		SceneManager.LoadScene ("InTheWoods");
		StartCoroutine ("startGame");
	}

	IEnumerator startGame(){
		levelPanel.GetComponentInChildren<Text> ().text = "Level: " + environment.level;
		levelPanel.SetActive (true);
		yield return new WaitForSeconds (levelTransitionDelay);
		levelPanel.SetActive (false);
	}

	public void Reset(){
		print ("Resetting game");
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	public void ExitToMainMenu(){
		SceneManager.LoadScene ("StartMenu");
	}
	public void ExitGame(){
		print ("Quitting game");
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
		Application.Quit ();
	}
}
