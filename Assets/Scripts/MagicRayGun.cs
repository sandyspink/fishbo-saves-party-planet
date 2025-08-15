using UnityEngine;

public class MagicRayGun : MonoBehaviour
{
    [Header("Raygun Settings")]
    public float grabRange = 10f;
    public float holdDistance = 3f;
    public float throwForce = 10f;
    public float rotationSpeed = 100f;
    public float scrollSpeed = 0.5f;
    
    [Header("Visual Effects")]
    public LineRenderer beamLine;
    public Color grabBeamColor = new Color(0, 1, 1, 0.5f);
    public Color idleBeamColor = new Color(0, 0.5f, 1, 0.3f);
    
    [Header("Object Manipulation")]
    public LayerMask grabbableLayer = -1;
    
    private Camera playerCamera;
    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    private float originalDrag;
    private float originalAngularDrag;
    private bool isRotating = false;
    
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Create beam line renderer if not assigned
        if (beamLine == null)
        {
            GameObject beamObject = new GameObject("RayGun Beam");
            beamObject.transform.parent = transform;
            beamLine = beamObject.AddComponent<LineRenderer>();
            beamLine.startWidth = 0.05f;
            beamLine.endWidth = 0.02f;
            beamLine.material = new Material(Shader.Find("Sprites/Default"));
            beamLine.startColor = idleBeamColor;
            beamLine.endColor = idleBeamColor;
        }
    }
    
    void Update()
    {
        UpdateBeamVisual();
        
        // Left click to grab/release
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }
        
        // Right click to throw
        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            ThrowObject();
        }
        
        // Hold R to rotate
        isRotating = Input.GetKey(KeyCode.R);
        
        // E to make held object sticky
        if (Input.GetKeyDown(KeyCode.E) && heldObject != null)
        {
            ToggleStickyMode();
        }
        
        // Scroll to adjust distance
        if (heldObject != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                holdDistance = Mathf.Clamp(holdDistance + scroll * scrollSpeed * 10, 1f, grabRange);
            }
        }
        
        // Update held object position
        if (heldObject != null)
        {
            UpdateHeldObject();
        }
    }
    
    void TryGrabObject()
    {
        RaycastHit hit;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out hit, grabRange, grabbableLayer))
        {
            if (hit.collider.CompareTag("Grabbable") || hit.collider.GetComponent<Rigidbody>() != null)
            {
                heldObject = hit.collider.gameObject;
                heldRigidbody = heldObject.GetComponent<Rigidbody>();
                
                if (heldRigidbody == null)
                {
                    heldRigidbody = heldObject.AddComponent<Rigidbody>();
                }
                
                // Store original values
                originalDrag = heldRigidbody.drag;
                originalAngularDrag = heldRigidbody.angularDrag;
                
                // Make object easier to move
                heldRigidbody.drag = 10;
                heldRigidbody.angularDrag = 10;
                heldRigidbody.useGravity = false;
                
                // Set hold distance to current distance
                holdDistance = Mathf.Min(hit.distance, grabRange * 0.7f);
                
                Debug.Log($"Grabbed {heldObject.name}");
            }
        }
    }
    
    void ReleaseObject()
    {
        if (heldRigidbody != null)
        {
            // Restore original physics values
            heldRigidbody.drag = originalDrag;
            heldRigidbody.angularDrag = originalAngularDrag;
            heldRigidbody.useGravity = true;
            
            // Stop any remaining velocity for clean placement
            heldRigidbody.velocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
        }
        
        Debug.Log($"Released {heldObject.name}");
        heldObject = null;
        heldRigidbody = null;
    }
    
    void ThrowObject()
    {
        if (heldRigidbody != null)
        {
            // Restore physics
            heldRigidbody.drag = originalDrag;
            heldRigidbody.angularDrag = originalAngularDrag;
            heldRigidbody.useGravity = true;
            
            // Apply throw force
            heldRigidbody.velocity = playerCamera.transform.forward * throwForce;
            
            Debug.Log($"Threw {heldObject.name}");
        }
        
        heldObject = null;
        heldRigidbody = null;
    }
    
    void UpdateHeldObject()
    {
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
        
        if (isRotating)
        {
            // Rotate object with mouse while holding R
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            
            heldObject.transform.Rotate(playerCamera.transform.up, rotX, Space.World);
            heldObject.transform.Rotate(playerCamera.transform.right, -rotY, Space.World);
        }
        
        // Smoothly move object to target position
        if (heldRigidbody != null)
        {
            Vector3 force = (targetPosition - heldObject.transform.position) * 50f;
            heldRigidbody.velocity = force;
        }
    }
    
    void ToggleStickyMode()
    {
        if (heldObject == null) return;
        
        // Check if object already has sticky component
        StickyObject sticky = heldObject.GetComponent<StickyObject>();
        if (sticky == null)
        {
            // Add sticky component
            sticky = heldObject.AddComponent<StickyObject>();
            
            // Visual feedback - make it glow orange when sticky
            Renderer rend = heldObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.yellow;
            }
            
            Debug.Log($"Made {heldObject.name} sticky!");
        }
        else
        {
            // Remove sticky mode
            Destroy(sticky);
            
            // Reset color
            Renderer rend = heldObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.white;
            }
            
            Debug.Log($"Removed sticky from {heldObject.name}");
        }
    }
    
    void UpdateBeamVisual()
    {
        if (beamLine == null) return;
        
        Vector3 startPos = playerCamera.transform.position;
        Vector3 endPos = startPos + playerCamera.transform.forward * (heldObject != null ? holdDistance : grabRange * 0.3f);
        
        beamLine.SetPosition(0, startPos);
        beamLine.SetPosition(1, endPos);
        
        // Change color when holding object
        Color beamColor = heldObject != null ? grabBeamColor : idleBeamColor;
        beamLine.startColor = beamColor;
        beamLine.endColor = beamColor;
        
        // Add subtle animation
        float pulse = Mathf.Sin(Time.time * 3) * 0.02f + 0.05f;
        beamLine.startWidth = pulse;
        beamLine.endWidth = pulse * 0.4f;
    }
}