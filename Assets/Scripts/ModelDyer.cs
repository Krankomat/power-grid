using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
 * Colors all children of a GameObject named "Model" to the desired color. 
 * Use this, if some state changes happen - like a preview when placing down a building. 
 */

public class ModelDyer : MonoBehaviour
{

    public Material hoverMaterial;

    [HideInInspector] public bool isDyed = false; 

    private const string ModelName = "Model";
    private List<Renderer> materialRenderers; 
    private Material[] initialMaterials;
    private Renderer materialRenderer;


    void Start()
    {
        Transform modelTransform = GetModelTransformIfIsChild();
        materialRenderers = GetRenderersOfChildrenFromTransform(modelTransform);
        initialMaterials = GetListOfMaterialsFromRenderers(materialRenderers).ToArray(); 
    }


    void Update()
    {
        if (GameManager.Instance.isDebugging)
            ChangeMaterialsOnButtonPress(); 
    }


    private Transform GetModelTransformIfIsChild()
    {
        foreach (Transform modelTransform in transform)
            if (string.Equals(modelTransform.gameObject.name, ModelName))
                return modelTransform;

        Debug.LogError("Model Dyer Error: There was no \"" + ModelName + "\" GameObject found in " + gameObject.name + ". ");
        return null;
    } 


    private List<Renderer> GetRenderersOfChildrenFromTransform(Transform transform)
    {
        List<Renderer> renderers = new List<Renderer>(); 

        foreach (Transform materialTransform in transform.GetComponentsInChildren<Transform>())
        {
            materialRenderer = materialTransform.gameObject.GetComponent<Renderer>();

            // Probably an empty GameObject used for grouping 
            if (materialRenderer == null)
                continue;

            renderers.Add(materialRenderer);
        }

        return renderers; 
    }


    private List<Material> GetListOfMaterialsFromRenderers(List<Renderer> renderers)
    {
        List<Material> materials = new List<Material>();

        foreach (Renderer renderer in renderers)
            materials.Add(renderer.material); 

        return materials; 
    }


    private void ChangeMaterialsBackToInitial()
    {
        for (int i = 0; i < initialMaterials.Length; i++)
            materialRenderers[i].material = initialMaterials[i];

        isDyed = false; 
    }


    private void ChangeMaterialsTo(Material material)
    {
        foreach (Renderer materialRenderer in materialRenderers)
            materialRenderer.material = hoverMaterial;

        isDyed = true; 
    }


    private void ChangeMaterialsOnButtonPress()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            if (isDyed)
                ChangeMaterialsBackToInitial();
            else
                ChangeMaterialsTo(hoverMaterial);
        }
    }

}
