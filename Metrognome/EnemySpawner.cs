using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

/*
 * EnemySpawner class
 * Coded by Matthew Barry and John Blau
 * Spawns enemies into the scene based on beats of music
 */

public class EnemySpawner : MonoBehaviour {

    #region Variables
    // public object variables
    public GameObject player;  // The player object in the scene
    public GameObject soundManager; // Get our sound manager game object, that controls audio in the scene
    // public numeric variables
    public int numEnemiesSpawned = 0; // The total number of enemies we have spawned this instance
    public float speed = 5f; // speed of the enemy spawned
    public float delayTimer; // delay timer is the amount of time the song should wait before playing
    public float currentGameTime = 0f; // The current game time- starts when the enemy spawner turns on and the first fixedUpdate is called
    // public boolean variables
    public bool gameOver = false; // controls if game is still being played or if it is over
    // public list variables
    public List<GameObject> enemy = new List<GameObject>(); // enemy gameobject prefab to use for this scene
    // private numeric variables
    private int positionInMap = -1; // Start at a position of -1 in our beatmap so when we increment we start off at index 0
    private float spawnOffset; // controls the offest of the spawn of the next enemy
    private float timeSinceLastSpawn; // The time since we last spawned in a bullet
    private float timeUntilNextBeat;    // The time until the next beat needs to spawn
    private float bps; // number of beats per second in a song read in using JSON
    private Vector3 center; // the center of the circle
    private Vector3 pos; // the position around the circle
    // private boolean variables
    private bool spawning = false; // true if we are spawning bullets, false if not
    private bool startedMusic; // have we started playing music yet
    // private list variables
    private List<float> beatMap; // get our beatmap, which is the player inputted beatMap
    // private component variables
    private GameManager gm; // the Game Manager script
    private BeatMapper beatMapper;  //our beatMapper script, which contains the player's beatMap
    #endregion

    /// <summary>
    /// Sets initial variables and components
    /// </summary>
    public void Start()
    {
        // current amount of time passed since the game started
        currentGameTime = 0f;
        startedMusic = false;
        // reset the map
        positionInMap = -1;
        
        // Get to our beatMap 
        soundManager = GameObject.FindWithTag("SoundManager");
        beatMapper = soundManager.GetComponent<BeatMapper>();
        beatMap = beatMapper.listOfBeats;

        // set GameManager script
        gm = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
        player = GameObject.FindWithTag("Player");

        // Calculate our beats per minute
        bps = this.gameObject.GetComponent<MusicManager>().rootObj.beatsPerMinute / 60f;     

        // calculate the distance between the player and where the enemies spawn
        CalculateDistance();

        // If our beatMap doesn't have anything contained in it, we want to spawn based on beats per minute
        if (beatMap.Count <= 0)
        {
            timeUntilNextBeat = (1f / bps);
            spawning = true;
        }
        // Otherwise, we do want to spawn based on our beatMap input
        else
        {
            positionInMap = 0;
            timeUntilNextBeat = beatMap[positionInMap] - beatMapper.timeBetweenStartOfSongAndFirstBeat;
            timeSinceLastSpawn = 0f;
            spawning = true;
        }
    }
	
	/// <summary>
    /// Incremenets our currentGameTime and starts music when needed based on enemy spawning functionality
    /// </summary>
	void Update ()
    {
        currentGameTime += Time.deltaTime;
        // starting music if not already started and delay timer has been reached
        if (!startedMusic && currentGameTime > delayTimer)
        {
            gm.PlayAudio();
            startedMusic = true;
        }
	}

    /// <summary>
    /// Spawns in the enemies at the right time
    /// </summary>
    private void FixedUpdate()
    {
        timeSinceLastSpawn += Time.deltaTime;

        // if our game hasn't ended yet and we've started spawning bullets
        if ((gameOver == false) && spawning)
        {         
            // if the time that has passed is greater than the time we have until the next beat spawns in 
            if ((timeSinceLastSpawn) >= timeUntilNextBeat)
            {
                // let's spawn in our bullet
                SpawnEnemy();
                // don't need to recalculate beats per second enemies
                if ((beatMap.Count != 0) && (positionInMap < beatMap.Count))
                {
                    positionInMap++;

                    timeSinceLastSpawn = (timeSinceLastSpawn - timeUntilNextBeat);
                    timeUntilNextBeat = beatMap[positionInMap] - beatMap[positionInMap - 1];
                }
                else
                {
                    timeSinceLastSpawn = timeSinceLastSpawn - timeUntilNextBeat;
                }
            }
        }
        // if gameover = true
        else if (gameOver)
        {
            HandleGameOver();
        }
    }

    /// <summary>
    /// Spawns an enemy at a random location around the player
    /// </summary>
    private void SpawnEnemy()
    {
            center = transform.position;
            Quaternion rot = Quaternion.identity;
            int randomNum = 0;

            // offset distance based on how long it has been since we should have spawned in the bullet
            float offsetDistance = speed * (timeSinceLastSpawn - timeUntilNextBeat);
            float maxCircleDistance = 25.0f;

            // calulations for the circle to spawn enemies
            pos = RandomCircle(center, maxCircleDistance - offsetDistance);

            // Get a random number to pick what color the enemy will be
            randomNum = Random.Range(0, enemy.Count);

            // spawn the enemy in the correct location
            GameObject newEnemy = Instantiate(enemy[randomNum], pos, rot);

            //newEnemy.GetComponent<Enemy>().speed = speed;
            // add them to the GM list
            gm.enemies.Add(newEnemy);

        numEnemiesSpawned++;
        //UnityEngine.Debug.Log("Spawned Enemy at: " + (currentGameTime - spawnOffset - (offsetDistance / speed)));
    }

    /// <summary>
    /// Handles the Game Over case for this EnemySpawner. Stops spawning enemies, and deactivates the class
    /// </summary>
    private void HandleGameOver()
    {
        spawning = false;
        // Don't forget to reset our position in the map
        positionInMap = 0;
        this.enabled = false;
    }

    /// <summary>
    /// Generate a circle around the player and pick a random location which an enemy will spawn from
    /// </summary>
    /// <param name="center">The player's position</param>
    /// <param name="radius">The radius of the circle we want to draw</param>
    /// <returns>A random position in a circle around the player</returns>
    Vector3 RandomCircle(Vector3 center, float radius)
    {
        float ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = 1;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }

    /// <summary>
    /// Calculates the distance from the player to the place where the enemies will be spawning
    /// </summary>
    public void CalculateDistance()
    {
        // calulations for the circle to spawn enemies
        center = transform.position;
        pos = RandomCircle(center, 25.0f);
        // get distance from player and radius
        float distance = Vector3.Distance(player.transform.position, pos) - player.GetComponent<Player>().hitDistance;

        // divide distance by speed
        // how long it takes the enemy to get to the player from spawnpoint
        spawnOffset = distance / speed;

        // negate value 
        spawnOffset = -spawnOffset;

        // Calculate our delay timer before the song should start
        delayTimer = Mathf.Abs((spawnOffset + 0.0001f) + beatMapper.timeBetweenStartOfSongAndFirstBeat);

        gm.TurnTheLightsOff();

       // gm.StartSongDelay();
    }
}
