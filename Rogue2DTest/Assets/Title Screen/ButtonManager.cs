using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonManager : MonoBehaviour {

	public void NewGame(string choice){
		PlayerPrefs.SetString ("hero", choice);
		SceneManager.LoadScene ("_Complete-Game");
}
}
