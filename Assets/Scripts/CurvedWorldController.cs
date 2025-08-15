using UnityEngine;

public class CurvedWorldController : MonoBehaviour
{
    [Header("Curvature Settings")]
    [Range(0f, 0.1f)]
    public float curvatureX = 0.01f;
    [Range(0f, 0.1f)]
    public float curvatureY = 0.01f;
    
    private Material curveMaterial;
    private Shader curveShader;
    
    void Start()
    {
        // Find the curve shader
        curveShader = Shader.Find("Custom/WorldCurvature");
        if (curveShader == null)
        {
            Debug.LogError("WorldCurvature shader not found!");
            return;
        }
        
        // Create material with the shader
        curveMaterial = new Material(curveShader);
        curveMaterial.SetFloat("_CurvatureX", curvatureX);
        curveMaterial.SetFloat("_CurvatureY", curvatureY);
        
        ApplyCurvatureToAllObjects();
    }
    
    void ApplyCurvatureToAllObjects()
    {
        // Apply to all renderers in the scene
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer rend in allRenderers)
        {
            // Skip UI and particle systems
            if (rend.gameObject.layer == LayerMask.NameToLayer("UI")) continue;
            if (rend.GetComponent<ParticleSystem>() != null) continue;
            
            // Skip the ray gun beam
            if (rend.GetComponent<LineRenderer>() != null) continue;
            
            // Apply curved material
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Preserve the color but use curve shader
                Color originalColor = mats[i].color;
                mats[i] = new Material(curveMaterial);
                mats[i].color = originalColor;
            }
            rend.materials = mats;
        }
    }
    
    void Update()
    {
        // Allow runtime adjustment
        if (curveMaterial != null)
        {
            curveMaterial.SetFloat("_CurvatureX", curvatureX);
            curveMaterial.SetFloat("_CurvatureY", curvatureY);
            
            // Update all materials
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();
            foreach (Renderer rend in allRenderers)
            {
                if (rend.GetComponent<LineRenderer>() != null) continue;
                
                foreach (Material mat in rend.materials)
                {
                    if (mat.shader == curveShader)
                    {
                        mat.SetFloat("_CurvatureX", curvatureX);
                        mat.SetFloat("_CurvatureY", curvatureY);
                    }
                }
            }
        }
    }
}