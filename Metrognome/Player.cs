using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main Player Class
/// Coded by Matthew Barry
/// Controls player input, player attacking and animations
/// Player dying, and time management(delays) for all the above
/// </summary>
public class Player : MonoBehaviour
{
    #region Variables
    // public object variables
    public Transform swordTransfrom; // transform of the sword
    public GameObject hitCircle; // indicates the distance that the player can hit
    public GameObject closestEnemy; // closest enemy in the game to the player
    public GameObject playerDestruct; // prefab of the player destructable
    public GameObject swordDestruct; // prefab of the sword destructable
    public TrailRenderer swipeRend; // holds the trail renderer for the player
    // public numeric variables
    public int hitCount = 0; // amount of notes the player has hit in a row
    public int comboCount = 0; // the number of combinations the player has hit
    public float swordDelay; // how long after a miss until player can swing again
    public float comboTimer; // how long we give the player to be able to hit a combo after sword swing complete
    public float hitDistance; // float for how far away player can reach
    public float voxelLife = 3f; // life that the destructed bolts will last
    // public boolean variables
    public bool comboEnabled = false; // if the player is in the middle of a combo attack
    // private numeric variables
    private int animTracker = 0; // tracks the current attack animation
    private float sinceLastSwing = 0; // time passed since last swing time by player
    private float swingDelayTimer = 0; // timer to incriment once a player misses a attack
    private float sinceLastKeyPressed = 0; // time since one of the last two player keys were pressed
    // private boolean variables
    private bool swinging = false; // if the player is swinging his sword
    private bool missed = false; // keeps track if the player missed a target
    // private component variables
    private GameManager gm; // game manager
    private EnemySpawner es; // the EnemySpawner that keeps track of the current time
    private Animator anim; // holds the component animation for the player
    #endregion

    /// <summary>
    /// Sets initial variables and components
    /// </summary>
    void Start()
    {
        // set component to attributes script
        gm = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
        es = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnemySpawner>();
        anim = GetComponent<Animator>();
        // disable the swipe renderer and set animation speed
        swipeRend.enabled = false;
        anim.speed = 5;
    }

    /// <summary>
    /// Handles user input and functionality
    /// </summary>
    void Update()
    {
        // incriment seconds once every frame *** 1 second passed every 60 frames ***
        if(swinging)
        {
            sinceLastSwing += Time.deltaTime;
            // check to see if combo timer has run out
            if(sinceLastSwing >= comboTimer)
            {
                // reset our stats back to idle
                sinceLastSwing = 0;
                animTracker = 0;
                anim.SetInteger("attack", animTracker);
                swinging = false;
                comboEnabled = false;
                comboCount = 0;
            }
        }
        // if the player has missed, add to the delay
        if(missed)
        {
            swingDelayTimer += Time.deltaTime;
            // if the timer has reached the delay, reset the stats
            if(swingDelayTimer >= swordDelay)
            {
                missed = false;
                animTracker = 0;
                anim.SetInteger("attack", animTracker);
                swingDelayTimer = 0;
            }
        }
        // if the player is able to use input and not in the middle of recovering from a missed target
        else
        {
            // if the game is being played on a computer and not mobile
            if (gm.DESKTOPGAME == true)
            {
                // input for either of the player buttons to attack
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.M))
                {
                    // input to see if player hit both keys at the same time
                    if (Input.GetKeyDown(KeyCode.Z) && Input.GetKeyDown(KeyCode.M))
                    {
                        DoubleInput();
                    }
                    // only one key was pressed, continue on from here
                    else
                    {
                        SingleInput();
                    }
                }
            }
            // if the game is being played on a mobile device and not a computer
            else if (gm.MOBILEGAME == true)
            {
                bool leftSideTouched = false;
                bool rightSideTouched = false;
                // if there are any touches currently
                if (Input.touchCount > 0)
                {
                    // loop through all of the touches we have in the input manager right now
                    foreach (Touch touch in Input.touches)
                    {
                        // get the current touch we are checking
                        Touch currentTouch = touch;

                        // if it began this frame
                        if (currentTouch.phase == TouchPhase.Began)
                        {
                            // check to see if the left side of the screen has been touched
                            if (currentTouch.position.x < Screen.width / 2)
                            {
                                leftSideTouched = true;
                            }
                            // check to see if the right side of the screen has been touched
                            else if (currentTouch.position.x > Screen.width / 2)
                            {
                                rightSideTouched = true;
                            }
                        }
                    }
                    // loop ends and we see what our results are for either single touch or double touch
                    // if left side touched but right side was not touched
                    if(leftSideTouched == true && rightSideTouched == false)
                    {
                        SingleInput();
                    }
                    // if right side was touched but left side was not 
                    else if (leftSideTouched == false && rightSideTouched == true)
                    {
                        SingleInput();
                    }
                    // both left and right side of the screen were touched
                    else if (leftSideTouched == true && rightSideTouched == true)
                    {
                        DoubleInput();
                    }
                }
            }
        }
        // some animation is being played that isn't idle
        if(animTracker > 0)
        {
            swipeRend.enabled = true;
        }
        // player isn't swinging at all
        else
        {
            swipeRend.enabled = false;
        }
    }

    /// <summary>
    /// Called when player inputs one single key to attack
    /// </summary>
    public void SingleInput()
    {
        body.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        if (swinging == false)
        {
            swinging = true;
            animTracker = 1;
            Swing();
        }
        // player is swinging, let's check if there's a combo
        else
        {
            // if player swang in the combo timer zone, swing again but increase animation number and reset timer
            // to determine if a combo has been achieved
            if (sinceLastSwing <= comboTimer)
            {
                Swing();
                comboEnabled = true;
                animTracker += 1;
                // if animTracker is 5 or greater, we need to reset the the sequence to the beginning
                if (animTracker >= 5)
                {
                    animTracker = 1;
                }
            }
        }
    }

    /// <summary>
    /// Called when player inputs two keys at the same time to attack
    /// </summary>
    public void DoubleInput()
    {
        // for debugging purposes, turn the mesh blue
        body.GetComponent<SkinnedMeshRenderer>().material.color = Color.blue;
        // if not already swinging, start the first sequence of swings
        if (swinging == false)
        {
            swinging = true;
            animTracker = 1;
            Swing();
        }
        // player is swinging, let's check if there's a combo
        else
        {
            // if player swang in the combo timer zone, swing again but increase animation number and reset timer
            // to determine if a combo has been achieved
            if (sinceLastSwing <= comboTimer)
            {
                Swing();
                comboEnabled = true;
                animTracker += 1;
                // if animTracker is 5 or greater, we need to reset the the sequence to the beginning
                if (animTracker >= 5)
                {
                    animTracker = 1;
                }
            }
        }
    }

    /// <summary>
    /// Method to swing the sword
    /// </summary>
    public void Swing()
    {
        // reset the time since last swing
        sinceLastSwing = 0;
        // turn towards the enemy first *** will be the closest enemy ***
        if (closestEnemy != null && closestEnemy != gm.gameObject)
        {
            transform.LookAt(closestEnemy.transform);
        }
        // make sure there is a closest enemy
        if (closestEnemy != null)
        {
            // get distance to closest enemy
            float distance = Vector3.Distance(this.transform.position, closestEnemy.transform.position);
            // if the distance is instead the hit radius we have set for the player
            if (distance <= hitDistance && closestEnemy != gm.gameObject)
            {
                // enemy is in range, good job player, kill the enemy (or do damage at least)
                closestEnemy.GetComponent<Enemy>().health -= 1;
                //Debug.Log("Destroy: " + es.currentGameTime);
                anim.SetInteger("attack", animTracker);
                // increase hit count and combo count if combo is enabled
                hitCount++;
                if(comboEnabled)
                {
                    comboCount++;
                    gm.EnvironmentEffect(comboCount);
                }
            }
            // target out of player's range, call missed method
            else
            {
                Missed();
            }
        }
        // there are no enemies on the screen so miss anyway
        else
        {
            Missed();
        }
    }

    /// <summary>
    /// Method when player misses a swing
    /// </summary>
    public void Missed()
    {
        // we missed the player, set stats back to 0 and false
        anim.SetInteger("attack", animTracker);
        missed = true;
        swinging = false;
        sinceLastSwing = 0;
        hitCount = 0;
        comboCount = 0;
    }

    /// <summary>
    /// Method called when player dies
    /// </summary>
    public void Die()
    {
        // make player disinigrate into voxels
        GameObject sword = Instantiate(swordDestruct, swordTransfrom.position, swordTransfrom.rotation);
        GameObject bolt = Instantiate(playerDestruct, transform.position, transform.rotation);
        // destroy the real gameobjects so it's just the broken voxels
        Destroy(this.gameObject);
        Destroy(sword, voxelLife);
        Destroy(bolt, voxelLife);
    }
}
