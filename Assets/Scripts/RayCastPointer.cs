using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class RayCastPointer : MonoBehaviour
{
    public float raycast_length;
    public float maxRayDistance; // Maximum length of the ray trace line

    private LineRenderer lineRenderer;
    public GameObject Hand;
    public GameObject HandPivot;
    public GameObject Character;
    public GameObject Dart;
    private GameObject newDart;
    private GameObject newEdible;
    private DartScoreSystem dartScoreSystem;
    public HealthManager healthManager; // Reference to the HealthManager script
    public HydrationManager hydrationManager; // Reference to the HealthManager script

    public Vector3 rayPivot = new Vector3(0f, 0f, 0f);
    private Color originalButtonColor; // Store the original color of the button

    private CharacterMovement charMovement;
    private GameObject lastHitObject; // Store the last object hit by the ray

    void Start()
    {
        // Ensure we have LineRenderer component attached
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Set up initial line renderer properties
        lineRenderer.positionCount = 2; // Two points for a straight line
        lineRenderer.startWidth = 0.07f; // Adjust as necessary
        lineRenderer.endWidth = 0.02f; // Adjust as necessary
        lineRenderer.material.color = Color.white; // Set the color to white

        // Remove the shadow from the line renderer material
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        charMovement = Character.GetComponent<CharacterMovement>();
        dartScoreSystem = GameObject.Find("DartScoreSystem").GetComponent<DartScoreSystem>();

        maxRayDistance = raycast_length;

    }

    void Update()
    {
        // Vector3 cameraPosition = Camera.main.transform.position;

        // Vector3 directionToCamera = cameraPosition - transform.position;

        // Make the hand face same direction as camera
        // Get the hand's position
        Vector3 handPosition = Hand.transform.position;

        // Get the hand's forward direction
        Vector3 handForward = Hand.transform.up;

        // Calculate the end position of the ray
        Vector3 rayEnd = handPosition + handForward * maxRayDistance;

        lineRenderer.SetPosition(0, handPosition);
        lineRenderer.SetPosition(1, rayEnd);
        ChangeAllButtonColors(Color.white);

        // Perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(handPosition, handForward, out hit, maxRayDistance))
        {
            rayPivot = rayEnd;

            // Debug.Log(rayPivot);
            // If the ray hits something, set the end position of the line to the point of intersection
            lineRenderer.SetPosition(0, handPosition);
            lineRenderer.SetPosition(1, hit.point);

            // Detect which object the end of the line touches
            GameObject hitObject = hit.collider.gameObject;
            // Debug.Log("Hit Object: " + hitObject.name);

            if (hitObject.GetComponent<Graphic>() != null)
            {
                // Debug.Log("UI Element Hit: " + hitObject.name);
                // You can perform UI-specific actions here
            }

            // Enable the Outline component if it exists
            Outline outline = hitObject.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }

            // Disable Outlines for the last hit object if it's different from the current hit object
            if(hitObject.tag != "Outline Objects")
            {
                DisableAllOutlines();
                
            }

            // Store the current hit object
            lastHitObject = hitObject;

            // Left Fridge door
            if (hitObject.name.Contains("Fridge.L") && Input.GetButtonDown("js2"))
            {
                LeftFridgeDoor leftFridgeDoor = hitObject.GetComponent<LeftFridgeDoor>();
                if(leftFridgeDoor.open)
                {
                    StartCoroutine(leftFridgeDoor.closing());
                    leftFridgeDoor.open = false;
                }
                else
                {
                    StartCoroutine(leftFridgeDoor.opening());
                    leftFridgeDoor.open = true;
                }
            }
            // Right Fridge door
            else if (hitObject.name.Contains("Fridge.R") && Input.GetButtonDown("js2"))
            {
                RightFridgeDoor rightFridgeDoor = hitObject.GetComponent<RightFridgeDoor>();
                if(rightFridgeDoor.open)
                {
                    StartCoroutine(rightFridgeDoor.closing());
                    rightFridgeDoor.open = false;
                }
                else
                {
                    StartCoroutine(rightFridgeDoor.opening());
                    rightFridgeDoor.open = true;
                }
            }

            // Stamina Regen
            if(hitObject.name.Contains("Cola"))
            {
                if(Input.GetButtonDown("js2"))
                {
                    newEdible = (GameObject)Instantiate(hitObject);
                    newEdible.transform.SetParent(HandPivot.transform);
                    
                    maxRayDistance = 0.5f;

                    newEdible.transform.position = handPosition + handForward * maxRayDistance;

                    if(hitObject.name.Contains("Cup"))
                    {
                        newEdible.transform.localScale = new Vector3(16f, 16f, 16f);
                    }
                }
                else if(Input.GetButtonDown("js5"))
                {
                    // Add audio cue;
                    Destroy(newEdible);
                    // healthManager.ResetHealth();
                    hydrationManager.ResetHealth();

                    maxRayDistance = raycast_length;
                }

            }

            // Stamina Regen
            if(hitObject.name.Contains("_fruit"))
            {
                if(Input.GetButtonDown("js2"))
                {
                    newEdible = (GameObject)Instantiate(hitObject);
                    newEdible.transform.SetParent(HandPivot.transform);
                    
                    maxRayDistance = 0.5f;

                    newEdible.transform.position = handPosition + handForward * maxRayDistance;
                }
                else if(Input.GetButtonDown("js5"))
                {
                    // Add audio cue;
                    Destroy(newEdible);
                    healthManager.ResetHealth();

                    maxRayDistance = raycast_length;
                }

            }

            // if (hitObject.name == "Coffee" && Input.GetButtonDown("js2"))
            // {
            //     // Reset the health to 100
            //     hydrationManager.ResetHealth();

            // }
            if (Input.GetButtonDown("js5"))
            {
                // Check if the hit object has a Button component
                Button button = hitObject.GetComponent<Button>();
                ColorBlock colors = button.colors;
                colors.normalColor = Color.red; // Change to whatever color you want
                button.colors = colors;                // If it does, invoke the onClick event
                if (button != null)
                {
                    button.onClick.Invoke();
                }
                else
                {
                    Debug.LogWarning("The hit object does not have a Button component attached.");
                }
            }

            if(hitObject.tag == "Outline Objects" && (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X)))
            {
                if(hitObject.GetComponent<PickedUp>() != null && !hitObject.name.Contains("dart"))
                {
                    if(!hitObject.GetComponent<PickedUp>().pickedUp)
                    {
                        hitObject.transform.SetParent(HandPivot.transform);
                        hitObject.GetComponent<Rigidbody>().useGravity = false;
                    }

                    maxRayDistance = 1f;

                    hitObject.GetComponent<PickedUp>().pickedUp = true;
                    hitObject.transform.position = handPosition + handForward * maxRayDistance;
                }
            }

            // Dart Spawn in Hand
            if(hitObject.name == "Dart_Table" && (Input.GetButtonDown("js2") || Input.GetKeyDown(KeyCode.X)))
            {
                if(dartScoreSystem.currentDarts > 0 && !DartInHand(HandPivot.transform))
                {
                    newDart = (GameObject)Instantiate(Dart);
                    
                    if(!newDart.GetComponent<PickedUp>().pickedUp)
                    {
                        newDart.transform.SetParent(HandPivot.transform);
                        newDart.GetComponent<Rigidbody>().useGravity = false;
                    }

                    maxRayDistance = 1f;

                    newDart.GetComponent<PickedUp>().pickedUp = true;
                    newDart.transform.position = handPosition + handForward * maxRayDistance;
                }
            }
            // drop the dart if you dont want to throw it
            if(DartInHand(HandPivot.transform) && Input.GetButtonDown("js10"))
            {
                Destroy(newDart);
            }

            // Reset Dart Game
            if(hitObject.name == "ResetGame" && Input.GetButtonDown("js5"))
            {
                hitObject.GetComponent<Button>().onClick.Invoke();
            }
            if (hitObject.GetComponent<Button>() != null)
            {
                // Store the original color if it's not stored yet
                if (originalButtonColor == Color.clear)
                {
                    originalButtonColor = hitObject.GetComponent<Button>().colors.normalColor;
                }

                // Change the color of the button when hovered
                ColorBlock colors = hitObject.GetComponent<Button>().colors;
                colors.normalColor = Color.red; // Change to whatever color you want
                hitObject.GetComponent<Button>().colors = colors;
            }
        }
        else
        {
            // If the ray doesn't hit anything, disable the outline for the last hit object
            DisableAllOutlines();
        }

    }
    void ChangeAllButtonColors(Color color)
    {
        // Find all buttons in the scene
        Button[] buttons = FindObjectsOfType<Button>();

        // Change the color of each button
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            button.colors = colors;
        }
    }
    // Method to disable outline for the last hit object
    public void DisableAllOutlines()
    {
        // Find all game objects with the tag "OutlineObjects"
        GameObject[] outlineObjects = GameObject.FindGameObjectsWithTag("Outline Objects");

        // Loop through each object found
        foreach (GameObject obj in outlineObjects)
        {
            // Check if the object has an "Outline" component
            Outline outlineComponent = obj.GetComponent<Outline>();
            if (outlineComponent != null)
            {
                // Disable the "Outline" component
                outlineComponent.enabled = false;
            }
        }

        maxRayDistance = 10f;
    }
    // Method to check if Dart is already in Hand
    public bool DartInHand(Transform parent)
    {
        // Iterate through each child transform
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            GameObject childObject = child.gameObject;

            if(childObject.name.Contains("dart") && childObject.name.Contains("(Clone)"))
            {
                return true;
            }
        }
        return false;
    }
}
