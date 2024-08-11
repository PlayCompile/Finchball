using UnityEngine;

public class scoreSquare : MonoBehaviour
{
    public int scoreValue;
    public GameManager manager;
    public Collider detected;

    private void OnTriggerEnter(Collider other)
    {
        detected = other;
        if (other.name == "playerCenter")
        {
            // If there's no lastSquare or the current square has a higher score, assign it
            if (manager.lastSquare == null || scoreValue > manager.lastSquare.scoreValue)
            {
                manager.lastSquare = this;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        detected = other;

        if (other.name == "playerCenter")
        {
            // Continuously check if this square should take priority
            if (manager.lastSquare == null || scoreValue > manager.lastSquare.scoreValue)
            {
                manager.lastSquare = this;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        detected = null;
        if (other.name == "playerCenter" && manager.lastSquare == this)
        {
            manager.lastSquare = null;
        }
    }
}