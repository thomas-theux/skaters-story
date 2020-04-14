﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class SkateboardController : MonoBehaviour {

    public int playerID = 0;

    private Player player;
    private Rigidbody2D rb;

    public CharacterSheet CharacterSheetScript;

    // 0 = stop; 1 = roll; 2 = skate; 3 = air;
    public int SkateMode = 0;

    // Force variables for roll, skate, and ollie
    public float MaxRollForce;               // The max speed when just rolling (without X)
    public float MaxSkateForce;              // The max speed when holding the X button
    public float OllieForce = 300.0f;        // How high the skater can jump
    private float AppliedForce = 0.0f;       // Just an interims variable to calculate
    private float IncreaseForce = 0.1f;      // How fast the skateboard is accelerating until it reaches max speed

    private float ollieForceMin = 100.0f;
    private float ollieForceMax = 500.0f;

    // Variables for ground checking
    public bool isGrounded = false;
    public Transform GroundChecker;
    private float GroundDistance = 0.1f;
    [SerializeField] private LayerMask platformLayerMask;

    private bool startedDelay = false;
    private float enterDelay = 0.0f;
    private float enterIncrease = 0.04f;
    private float enterThreshold = 1.0f;

    public int savedSkateMode = 0;        // When the player ollies the current skate mode will be saved and will be applied again after landing

    // REWIRED
    private bool XButtonDown = false;
    private bool XButtonUp = false;
    private bool dPadLeft = false;
    private bool dPadRight = false;

    // DEV variables
    private bool DEVRESET = false;
    public float BoardSpeed = 0;


    private void Awake() {
        player = ReInput.players.GetPlayer(playerID);
        rb = GetComponent<Rigidbody2D>();
    }


    private void Start() {
        GetStats();
    }


    private void GetStats() {
        MaxRollForce = CharacterSheetScript.StatSpeed / 2;
        MaxSkateForce = CharacterSheetScript.StatSpeed;

        OllieForce = CharacterSheetScript.StatOllie;
    }


    private void Update() {
        GetInput();

        isGrounded = Physics2D.OverlapCircle(GroundChecker.position, GroundDistance, platformLayerMask);

        ///////////////////////////////////////////////////////////////////////////////////////

        // Reset player to the beginning of the level
        if (DEVRESET) DEVReset();
        BoardSpeed = rb.velocity.magnitude;

        ///////////////////////////////////////////////////////////////////////////////////////

        // 0 = Stop
        if (SkateMode == 0) {
            if (dPadRight) {
                // Start to ROLL
                SkateMode = 1;
                rb.velocity = transform.right * MaxRollForce;
            }

            if (XButtonUp) {
                // JUMP while skating
                if (isGrounded) {
                    ApplyOllieForce();
                }
            }

            if (XButtonDown) {
                // JUMP while standing still
                EnterSkateMode();
            }
        }

        // 1 = Roll
        if (SkateMode == 1) {
            ApplyPushForce(MaxRollForce);

            if (dPadLeft) {
                // Decelerating until standing still
                ResetValuesWhenStopping();
            }

            if (XButtonUp) {
                // JUMP while rolling
                if (isGrounded) {
                    ApplyOllieForce();
                }
            }

            if (XButtonDown) {
                // Go into SKATE mode
                EnterSkateMode();
            }
        }

        // 2 = Skate
        if (SkateMode == 2) {
            ApplyPushForce(MaxSkateForce);

            if (!XButtonDown) {
                if (dPadLeft) {
                    // Decelerating until standing still
                    ResetValuesWhenStopping();
                }
            }

            if (XButtonUp) {
                // JUMP while skating
                if (isGrounded) {
                    ApplyOllieForce();
                }
            }
        }

        // 3 = Air
        if (SkateMode == 3) {
            // Can do tricks
            if (isGrounded) {
                SkateMode = savedSkateMode;

                // Check for bails
            }
        }

        if (!isGrounded) {
            if (SkateMode != 3) {
                savedSkateMode = SkateMode;
                SkateMode = 3;
            }
        }
    }


    private void GetInput() {
        XButtonDown = player.GetButton("X");
        XButtonUp = player.GetButtonUp("X");

        dPadLeft = player.GetButton("DPad Left");
        dPadRight = player.GetButton("DPad Right");

        DEVRESET = player.GetButtonUp("Reset (DEV)");
    }


    private void EnterSkateMode() {
        if (!startedDelay) {
            enterDelay = 0.0f;
            startedDelay = true;
        }

        enterDelay += enterIncrease;

        if (enterDelay >= enterThreshold) {
            SkateMode = 2;
        }
    }


    private void ResetValuesWhenStopping() {
        SkateMode = 0;
        AppliedForce = 0;
        startedDelay = false;
    }


    private void ApplyPushForce(float maxForce) {
        if (AppliedForce < maxForce) {
            AppliedForce += IncreaseForce;
        } else {
            AppliedForce = maxForce;
        }

        // rb.velocity = transform.right * AppliedForce;
        // rb.AddForce(transform.right * maxForce, ForceMode2D.Force);

        rb.velocity = new Vector2(AppliedForce, rb.velocity.y);
    }


    private void ApplyOllieForce() {
        startedDelay = false;

        float ollieForceMultiplier = BoardSpeed / 10;
        float calculatedOllieForce = OllieForce * ollieForceMultiplier;

        if (calculatedOllieForce < ollieForceMin) {
            calculatedOllieForce = ollieForceMin;
        } else if (calculatedOllieForce > ollieForceMax) {
            calculatedOllieForce = ollieForceMax;
        }

        Vector2 newOllieForce = new Vector2(0f, calculatedOllieForce);
        rb.AddForce(newOllieForce);
    }


    private void DEVReset() {
        savedSkateMode = 0;
        SkateMode = 0;

        AppliedForce = 0;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        this.transform.position = new Vector3(0, 0.2f, 0);
        this.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

}
