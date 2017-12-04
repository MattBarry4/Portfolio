using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Game Manager Class
/// Coded by Matthew Barry
/// Handles keeping track of the objects within the scene
/// Gets closest enemy for player class
/// Handles GameOver and RestartGame functions
/// </summary>
public class GameManager : MonoBehaviour {

    #region Variables
    // public object variables
    public GameObject player; // spawned object of the player
    public GameObject playerPrefab; // prefab for the player gameobject
    public GameObject spawnpoint; // player respawn point
    public GameObject soundManager; // gameobject for the sound manager
    public Button deathMainMenu; // button for the main menu
    public Text scoreText; // score text
    public AudioClip mainTheme; // audio for our main menu theme
    // public numeric variables
    public float score; // score the player has achieved 
    public float restartTimer; // how long until we load the game back up
    // public boolean variables
    public bool DESKTOPGAME = false; // if game is being played on desktop
    public bool MOBILEGAME = false; // if game is being played on a mobile device
    // public list variables
    public List<GameObject> enemies = new List<GameObject>(); // all enemies in the game
    // private numberic variables
    private float sinceLastFrame = 0; // keeps track of how much time has passed since last frame
    // private boolean variables
    private bool gameOver = false; // if game has ended
    // private component variables
    private Player playerScript; // player script
    private EnemySpawner enemySpawner; // enemy spawner
    private BeatMapper beatMapper; // beat mapper component
    private AudioSource audioSource; // audio source component
    #endregion

    /// <summary>
    /// Sets initial variables and components
    /// </summary>
    void Start ()
    {
        // set beat mapper component
        beatMapper = soundManager.GetComponent<BeatMapper>();
        // keeping track of all objects in the game
        enemySpawner = GetComponent<EnemySpawner>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<Player>();
        // set audio source component
        audioSource = soundManager.GetComponent<AudioSource>();
        audioSource.clip = mainTheme;
        audioSource.Play();
        // detect what platform we are playing on
        #if UNITY_STANDALONE
            DESKTOPGAME = true;
        #endif
        #if UNITY_IOS
            MOBILEGAME = true;
        #endif
        #if UNITY_ANDROID
            MOBILEGAME = true;
        #endif
    }

    /// <summary>
    /// Checks to see if game has ended or not
    /// </summary>
    void Update ()
    {
        if (gameOver == false)
        {
            // call methods
            // get the closest enemy from our list and give it to the player
            GetClosestEnemy();
        }
        // game is over, work on restarting it for player
        else
        {
            // while timer is still below our restart time limit
            if (sinceLastFrame <= restartTimer)
            {
                // adds time since last frame to the float
                sinceLastFrame += Time.deltaTime;
                if(sinceLastFrame >= restartTimer)
                {
                    // timer has been reached, restart the game by resetting values
                    Debug.Log(sinceLastFrame);
                    GameObject player = Instantiate(playerPrefab, spawnpoint.transform.position, spawnpoint.transform.rotation);
                    player.GetComponent<Player>().enabled = true;
                    score = 0;
                    sinceLastFrame = 0;
                    gameOver = false;
                    enemySpawner.enabled = true;
                    enemySpawner.gameOver = false;
                    mainTheme = soundManager.GetComponent<AudioSource>().clip;
                    this.Start();
                    enemySpawner.Start();
                    PlayGame();
                }
            }
        }
	}

    /// <summary>
    /// Gets the closest enemy to the player
    /// </summary>
    public void GetClosestEnemy()
    {
        if (enemies.Count > 0)
        {
            float closestDistance = 999; // temporary storage of the closest distance found so far in the list
            GameObject closestEnemy = this.gameObject; // temporary storage of the closest enemy in the game

            // go through every enemy and find closest one to the player
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

                    if (distance < closestDistance)
                    {
                        // reset distance and enemy since this object is closer
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
            // go tell the player which enemy is the closest
            playerScript.closestEnemy = closestEnemy;
        }
    }

    /// <summary>
    /// Method to activate certain characteristics of the enviromnent
    /// </summary>
    public void EnvironmentEffect(int comboCount)
    {
        // Loop through and turn off all the lights in the scene
        GameObject[] lights = GameObject.FindGameObjectsWithTag("TreeLight");
        // for the number of combos we have hit in a row
        for (int i = 0; i < comboCount; i++)
        {
            int randomLightnum = Random.Range(0, comboCount);
            lights[randomLightnum].GetComponent<LightFlicker>().flash = true;
            lights[randomLightnum].GetComponent<LightFlicker>().myLight.enabled = true;
            int randomColorNum = Random.Range(0, lights[randomLightnum].GetComponent<LightFlicker>().colors.Count);
            lights[randomLightnum].GetComponent<LightFlicker>().myLight.color = lights[randomLightnum].GetComponent<LightFlicker>().colors[randomColorNum];
        }
    }

    /// <summary>
    /// Called when enemy has reached player to end game
    /// </summary>
    public void GameOver()
    {
        enemySpawner.gameOver = true;
        gameOver = true;
        playerScript.Die();
        RestartGame();
    }

    /// <summary>
    /// Restarts game
    /// </summary>
    /// <returns></returns>
    public void RestartGame()
    {
        // restart the game
        // first make sure there are enemies in our list
        if (enemies.Count > 0)
        {
            // make a temporary list of enemies to destroy to keep Unity happy
            List<GameObject> tempList = new List<GameObject>();
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    // add to temporary list
                    tempList.Add(enemy);
                }
            }
            foreach (GameObject enemy in tempList)
            {
                enemy.GetComponent<Enemy>().gameOver = true;
                enemy.GetComponent<Enemy>().Die();
            }
        }
    }

    /// <summary>
    /// Stops the audio source
    /// </summary>
    public void PlayGame()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Starts the audio source
    /// </summary>
    public void PlayAudio()
    {
        audioSource.Play();
    }

    /// <summary>
    /// Handles lights turning off
    /// </summary>
    public void TurnTheLightsOff()
    {
        // Loop through and turn off all the lights in the scene
        GameObject[] lights = GameObject.FindGameObjectsWithTag("TreeLight");
        foreach (GameObject go in lights)
        {
            go.GetComponent<Light>().enabled = false;
        }
    }
}
