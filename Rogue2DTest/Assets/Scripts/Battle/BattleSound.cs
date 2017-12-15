using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSound : MonoBehaviour
{

	public AudioClip swordatk;
	public AudioClip knightHit;
	public AudioClip orcHurt;
	public AudioSource efxSource;                   //Drag a reference to the audio source which will play the sound effects.
	public AudioSource musicSource;                 //Drag a reference to the audio source which will play the music.
	public static BattleSound instance = null;
	
	void Awake ()
	{
		//Check if there is already an instance of SoundManager
		if (instance == null)
			//if not, set it to this.
			instance = this;
		//If instance already exists:
		else if (instance != this)
			//Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
			Destroy (gameObject);

		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad (gameObject);
	}

	public void PlaySwordAtk()
	{
	//Set the clip of our efxSource audio source to the clip passed in as a parameter.
	efxSource.clip = swordatk;

	//Play the clip.
	efxSource.Play ();
	}

	public void PlayKnightHurt()
	{
	//Set the clip of our efxSource audio source to the clip passed in as a parameter.
	efxSource.clip = knightHit;

	//Play the clip.
	efxSource.Play ();
	}

	public void PlayOrcHurt()
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		efxSource.clip = orcHurt;

		//Play the clip.
		efxSource.Play ();
	}

	public void PlaySingle(AudioClip clip)
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		efxSource.clip = clip;

		//Play the clip.
		efxSource.Play ();
	}
}
