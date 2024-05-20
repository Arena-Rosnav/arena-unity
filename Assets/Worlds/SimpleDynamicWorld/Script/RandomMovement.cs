using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust the speed of movement
    public string targetTag = "Wall"; // Specify the tag to check for collisions

    private Rigidbody[] rbArray;
    private Vector3[] randomDirections;
    private bool[] isMovingArray;

    void Start()
    {
        rbArray = GetComponentsInChildren<Rigidbody>();
        randomDirections = new Vector3[rbArray.Length];
        isMovingArray = new bool[rbArray.Length];

        // Invoke the method to start movement for each Rigidbody after a delay
        for (int i = 0; i < rbArray.Length; i++)
        {
            InvokeRepeating("ChangeDirection", 0f, 2f); // Change direction every 2 seconds (adjust as needed)
        }
    }

    void Update()
    {
        for (int i = 0; i < rbArray.Length; i++)
        {
            if (isMovingArray[i])
            {
                // Move the object in the random direction using Rigidbody velocity
                rbArray[i].velocity = randomDirections[i] * moveSpeed;
            }
        }
    }

    void ChangeDirection()
    {
        for (int i = 0; i < rbArray.Length; i++)
        {
            // Generate a new random direction for each Rigidbody
            randomDirections[i] = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

            // Set each Rigidbody to start moving
            isMovingArray[i] = true;

            // Invoke the StopMovement method for each Rigidbody after a random time
            Invoke("StopMovement", Random.Range(0.5f, 1.5f)); // Object stops moving randomly between 0.5 to 1.5 seconds
        }
    }

    void StopMovement()
    {
        for (int i = 0; i < rbArray.Length; i++)
        {
            // Stop the movement by setting velocity to zero for each Rigidbody
            rbArray[i].velocity = Vector3.zero;
            isMovingArray[i] = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if collided with a GameObject having the specified tag
        if (collision.gameObject.CompareTag(targetTag))
        {
            // If collision occurs with the specified tag, change direction for each Rigidbody
            ChangeDirection();
        }
    }
}