using UnityEngine;

public class StickyObject : MonoBehaviour
{
    private bool hasStuck = false;
    
    void OnCollisionEnter(Collision collision)
    {
        // Don't stick to self or if already stuck
        if (hasStuck) return;
        if (collision.gameObject == gameObject) return;
        
        // Only stick to other objects with Rigidbody (buildable objects)
        Rigidbody otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (otherRigidbody == null) return;
        
        // Create the sticky connection
        StickToObject(collision.gameObject);
    }
    
    void StickToObject(GameObject target)
    {
        // Add a fixed joint to connect objects
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = target.GetComponent<Rigidbody>();
        
        // Stop any movement for clean sticking
        Rigidbody myRigidbody = GetComponent<Rigidbody>();
        if (myRigidbody != null)
        {
            myRigidbody.velocity = Vector3.zero;
            myRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Visual feedback - turn green when stuck
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green;
        }
        
        // Mark as stuck and remove sticky component
        hasStuck = true;
        
        Debug.Log($"{gameObject.name} stuck to {target.name}!");
        
        // Remove sticky component after use (one-time stick)
        Destroy(this);
    }
}