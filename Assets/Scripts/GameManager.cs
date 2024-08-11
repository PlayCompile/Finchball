using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public InputActionAsset inputAss;
    public GameObject playerPrefab;
    public Transform startPosition;
    public GameObject objPlayer;
    public float moveSpeed = 5f; // Speed at which the player moves
    public Text txtPosition;
    public Text txtPuckSpeed;
    private InputAction inputMove;
    private InputAction inputLaunch;
    private float leftLimitPos = 0.55f;
    private float rightLimitPos = -0.55f;
    private Animator animator;
    private GameObject objArrow;
    private GameObject objGauge;
    private int playerState = 0; //[0]free move,[1]thrustGauge,[2]launch,[3]inMotion,[4]retired
    private setGauge SetGauge;
    private Rigidbody playerRigidbody;
    public float angleAdjustment = 270f;
    private bool puckHasStartedMoving = false;
    public float launchForce = 10f; // Force applied during launch
    public scoreSquare lastSquare;
    public int pucks = 3;
    public int currentScore;
    public int potentialScore;
    public int playerScore;
    public Text txtScore;
    public Text txtPucks;
    public GameObject UIendGame;
    public int gameLevel = 1;
    public Transform levelsRoot;
    private List<GameObject> gameLevels = new List<GameObject>();
    public Text txtLevel;
    public int numOfLevels = 0;
    public bool nextLevel = false;
    private List<GameObject> playerPucks = new List<GameObject>();

    void Start()
    {
        inputMove = inputAss.FindAction("Move");
        inputLaunch = inputAss.FindAction("Launch");
        setupPlayer();
        Time.timeScale = 0.4f;
        foreach (Transform levelObjects in levelsRoot)
        {
            gameLevels.Add(levelObjects.gameObject);
        }
        numOfLevels = gameLevels.Count;
        playerPucks.Add(objPlayer);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    void setupPlayer()
    {
        objArrow = objPlayer.transform.Find("pivotArrow/Arrow").gameObject;
        objGauge = objPlayer.transform.Find("pivotArrow/gauge").gameObject;
        animator = objArrow.transform.parent.GetComponent<Animator>();
        SetGauge = objGauge.GetComponent<setGauge>();
        playerRigidbody = objPlayer.GetComponent<Rigidbody>();
        inputMove.Enable();
        inputLaunch.Enable();
        playerState = 0;
    }

    IEnumerator autoGauge()
    {
        float currentVal = 0f;
        while (playerState == 1)
        {
            // Charging the gauge
            while (currentVal < 1f)
            {
                currentVal += 0.01f;
                SetGauge.setValue = currentVal;
                yield return new WaitForEndOfFrame();
            }

            // Decharging the gauge
            while (currentVal > 0f)
            {
                currentVal -= 0.01f;
                SetGauge.setValue = currentVal;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void Update()
    {
        Vector2 movementInput = inputMove.ReadValue<Vector2>(); // Read the input value

        // Always move the player based on input
        MovePlayer(movementInput);

        // Clamp the player's position within the allowed limits
        Vector3 currentPos = objPlayer.transform.position;
        float clampedZ = Mathf.Clamp(currentPos.z, rightLimitPos, leftLimitPos); // rightLimitPos should be less than leftLimitPos
        objPlayer.transform.position = new Vector3(currentPos.x, currentPos.y, clampedZ);

        // Free move > Gauge charge
        if (playerState == 0)
        {
            if (inputLaunch.WasPressedThisFrame())
            {
                playerState = 1;
                StartCoroutine(autoGauge());
            }
        }
        
        if (playerState == 1)
        { // Gauge charge > Thrust
            if (inputLaunch.WasReleasedThisFrame())
            {
                playerState = 2;
                LaunchPlayer();
            }
        }

        if (playerState == 3) // Puck in motion
        {          
            // Check if the puck has started moving
            if (!puckHasStartedMoving && playerRigidbody.velocity.magnitude > 0)
            {
                puckHasStartedMoving = true;
            }

            // If the puck has started moving, check if it has stopped
            if (puckHasStartedMoving && playerRigidbody.velocity.magnitude == 0)
            {
     //           playerState = 4; // Puck may retire and live on a farm!
                puckHasStartedMoving = false; // Reset the flag for the next puck
                spawnNewPuck();
            }
        }

        txtPuckSpeed.text = playerRigidbody.velocity.magnitude.ToString();

        // Set state appearance
        if (playerState == 0)
        {
            objArrow.SetActive(true);
            objGauge.SetActive(false);
            inputMove.Enable();
            animator.speed = 1f;
        }

        if (playerState == 1)
        {
            objArrow.SetActive(false);
            objGauge.SetActive(true);
            inputMove.Disable();
            animator.speed = 0f;
        }

        if (playerState == 2 || playerState == 3)
        {
            objArrow.SetActive(false);
            objGauge.SetActive(false);
            inputMove.Disable();
        }

        txtPosition.text = objPlayer.transform.position.z.ToString();

        // Realtime scoring
        if (lastSquare != null) 
        { 
            currentScore = lastSquare.scoreValue;
            potentialScore = playerScore + currentScore;
        }
        else { potentialScore = playerScore; currentScore = 0; }
        txtScore.text = potentialScore.ToString("D2");
        if (pucks < 0) { pucks = 0; }
        txtPucks.text = pucks.ToString();
        txtLevel.text = "Level: " + gameLevel.ToString() + "/" + numOfLevels;
        if (nextLevel) { loadNextLevel(); }
    }

    void loadNextLevel()
    {
        nextLevel = false;
        gameLevels[gameLevel-1].SetActive(false);
        gameLevel++;
        gameLevels[gameLevel-1].SetActive(true);
        pucks = 4; // Spawn will reduce this by 1
        foreach (GameObject puckObject in playerPucks)
        {
            Destroy(puckObject);
        }
        playerPucks.Clear();
        UIendGame.SetActive(false);
        spawnNewPuck();
    }

    void spawnNewPuck()
    {
        if (pucks > 0)
        {
            objPlayer.transform.Find("puckBase/playerCenter").gameObject.SetActive(false);
            objArrow.SetActive(false);
            objGauge.SetActive(false);
            lastSquare = null;
            playerScore = potentialScore;
            pucks--;
            if (pucks == 0) { endGame(); }
            else
            {
                GameObject newPuck = Instantiate(playerPrefab);
                newPuck.transform.position = startPosition.transform.position;
                newPuck.transform.localPosition = startPosition.transform.localPosition;
                newPuck.transform.localScale = startPosition.transform.localScale;
                newPuck.transform.rotation = startPosition.transform.rotation;
                newPuck.transform.localRotation = startPosition.transform.localRotation;
                objPlayer = newPuck;
                playerPucks.Add(objPlayer);
                setupPlayer();
            }
        }
    }

    void endGame()
    {
        UIendGame.transform.Find("txtTotalScore").GetComponent<Text>().text = playerScore.ToString("D2");
        UIendGame.SetActive(true);
        inputLaunch.Disable();
        inputMove.Disable();
    }

    void MovePlayer(Vector2 movementInput)
    {
        // Invert the x-axis input to correct the movement direction
        Vector3 movement = new Vector3(0f, 0f, -movementInput.x) * moveSpeed * Time.deltaTime;

        // Update the player position
        objPlayer.transform.Translate(movement, Space.World);
    }

    void LaunchPlayer()
    {
        float applyForce = SetGauge.setValue * launchForce;
        // Get the direction the arrow is pointing
        Vector3 launchDirection = objArrow.transform.parent.transform.forward;

        // Apply the angle adjustment (rotate around the Y-axis)
        launchDirection = Quaternion.Euler(0, angleAdjustment, 0) * launchDirection;

        // Apply force to the player object in the direction of the adjusted arrow
        playerRigidbody.AddForce(launchDirection * applyForce, ForceMode.Impulse);
        playerState = 3;
    }
}