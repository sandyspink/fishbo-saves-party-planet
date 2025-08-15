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
    public Transform raygunTip; // Drag the cube here or it will auto-find it
    public Color grabBeamColor = new Color(0, 1, 1, 0.5f);
    public Color idleBeamColor = new Color(0, 0.5f, 1, 0.3f);
    
    [Header("UI")]
    public bool showCrosshair = true;
    public Color crosshairColor = Color.white;
    
    [Header("Object Manipulation")]
    public LayerMask grabbableLayer = -1;
    
    private Camera playerCamera;
    private GameObject heldObject;
    private Rigidbody heldRigidbody;
    private float originalDrag;
    private float originalAngularDrag;
    private bool isRotating = false;
    private bool showBeam = false;
    
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Auto-find raygun tip if not assigned
        if (raygunTip == null)
        {
            // Look for a child object first (the cube raygun)
            if (transform.childCount > 0)
            {
                raygunTip = transform.GetChild(0);
            }
            else
            {
                // Fall back to camera position
                raygunTip = transform;
            }
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
        // Update beam visibility based on whether we're holding something
        showBeam = (heldObject != null);
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
        Vector3 rayStart = raygunTip != null ? raygunTip.position : playerCamera.transform.position;
        Vector3 rayDirection = raygunTip != null ? raygunTip.forward : playerCamera.transform.forward;
        Ray ray = new Ray(rayStart, rayDirection);
        
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
        
        // Only show beam when actively holding an object
        beamLine.enabled = showBeam;
        
        if (showBeam)
        {
            Vector3 startPos = raygunTip != null ? raygunTip.position : playerCamera.transform.position;
            Vector3 rayDirection = raygunTip != null ? raygunTip.forward : playerCamera.transform.forward;
            Vector3 endPos = startPos + rayDirection * holdDistance;
            
            beamLine.SetPosition(0, startPos);
            beamLine.SetPosition(1, endPos);
            
            // Use grab color when beam is active
            beamLine.startColor = grabBeamColor;
            beamLine.endColor = grabBeamColor;
            
            // Add subtle animation
            float pulse = Mathf.Sin(Time.time * 3) * 0.02f + 0.05f;
            beamLine.startWidth = pulse;
            beamLine.endWidth = pulse * 0.4f;
        }
    }
    
    void OnGUI()
    {
        if (showCrosshair && !showBeam)
        {
            DrawCrosshair();
        }
    }
    
    void DrawCrosshair()
    {
        float crosshairSize = 20f;
        float crosshairThickness = 2f;
        
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        // Set crosshair color
        GUI.color = crosshairColor;
        
        // Draw horizontal line
        GUI.DrawTexture(new Rect(center.x - crosshairSize / 2f, center.y - crosshairThickness / 2f, 
                                crosshairSize, crosshairThickness), Texture2D.whiteTexture);
        
        // Draw vertical line
        GUI.DrawTexture(new Rect(center.x - crosshairThickness / 2f, center.y - crosshairSize / 2f, 
                                crosshairThickness, crosshairSize), Texture2D.whiteTexture);
        
        // Reset GUI color
        GUI.color = Color.white;
    }
}