using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy Class
/// Coded by Matthew Barry
/// Used to control AI to make them move towards player
/// Detects if the enemy has reached the player
/// </summary>
public class Enemy : MonoBehaviour {

    #region Variables
    // public object variables
    public GameObject player; // gameobject for the player prefab
    public GameObject boltDestruct; // prefab of the dead bolt
    public Material deadMat; // temp material for testing
    // public numeric variables
    public float speed = 5f; // how fast the enemy moves
    public float health; // how many hits the enemy can take
    public float distance; // distance from this enemy to the player
    public float disappearTimer; // timer until enemy disappears
    public float voxelLife = 4f; // life that the destructed bolts will last
    // public boolean variables
    public bool gameOver; // check to see if game is over
    // private numberic variables
    private float sinceLastFrame = 0; // keeps track of how much time has passed since last frame
    // private boolean variables
    private bool dead; // check if enemy is dead or not
    // private component variables
    private GameManager gm; // game manager component
    private EnemySpawner es; // the enemy spawner, for keeping track of the current game time
    #endregion

    /// <summary>
    /// Sets initial variables and components
    /// </summary>
    void Start ()
    {
        // set player object, needs to have correct tag
        player = GameObject.FindWithTag("Player");
        // set component script
        gm = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
        es = GameObject.FindWithTag("Manager").GetComponent<EnemySpawner>();
        // have bolt face player
        transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position);
	}

    /// <summary>
    /// Keeps track of enemy health and functionality based on that value
    /// </summary>
    void Update ()
    {
        // if the enemy has some health left and player still exist
        if (health > 0 && player != null)
        {
            // move towards the player's position
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
            // check to see how far away player is from this enemy
            CheckDistance();
        }
        // player doesn't exist or no health remaining
        else
        {
            // if not dead already, become dead
            if (!dead)
            {
                // destruct the bolt
                Destruct();
                // die();
                dead = true;
            }
        }
    }

    /// <summary>
    /// Checks to see if enemy has reached player
    /// </summary>
    public void CheckDistance()
    {
        // get distance
        distance = Vector3.Distance(player.transform.position, this.transform.position);

        if(distance <= 0.5f)
        {
            // enemy has reached player...RIP
            Destroy(this.gameObject);
            gm.GameOver();
        }
    }

    /// <summary>
    /// Kill the enemy and start disappear timer
    /// </summary>
    public void Die()
    {
        if (gameOver == false)
        {
            gm.score += 1;
        }
        // remove from the list
        gm.enemies.Remove(this.gameObject);
        Debug.Log("Destroy: " + es.currentGameTime);
        // change mat and start timer to go away
        Disappear();
    }

    /// <summary>
    /// Destruct the bolt
    /// </summary>
    void Destruct()
    {
        GameObject bolt = Instantiate(boltDestruct, transform.position, transform.rotation);      
        Destroy(bolt, voxelLife);
        Destroy(gameObject);
    }

    /// <summary>
    /// Delay by disappearTimer and then destroy this gameobject
    /// </summary>
    public void Disappear()
    {
        // while timer is still below our restart time limit
        while (sinceLastFrame <= disappearTimer)
        {
            // adds time since last frame to the float
            sinceLastFrame += Time.deltaTime;
            if(sinceLastFrame >= disappearTimer)
            {
                // timer has been reached, destroy this gameobject
                Destroy(this.gameObject);
            }
        }
    }
}
