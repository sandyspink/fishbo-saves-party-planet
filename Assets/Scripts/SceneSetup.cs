using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [Header("Scene Colors")]
    public Color groundColor = new Color(0.4f, 0.4f, 0.4f); // Dark grey
    public Color objectColor = new Color(0.7f, 0.7f, 0.7f); // Light grey
    
    void Start()
    {
        ColorizeScene();
    }
    
    void ColorizeScene()
    {
        // Find and color the ground (plane)
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in planes)
        {
            if (obj.GetComponent<MeshFilter>() && obj.GetComponent<MeshFilter>().mesh.name.Contains("Plane"))
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = groundColor;
                }
            }
        }
        
        // Find and color all cubes/objects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<MeshFilter>() && obj.GetComponent<Rigidbody>())
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null && rend.material.color == Color.white)
                {
                    rend.material.color = objectColor;
                }
            }
        }
    }
}