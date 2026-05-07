using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int copper = 100;
    public int iron = 50;
    public int titanium = 20;
    public int diamond = 0;
    public GameObject floatingTextPrefab;
    void Start()
    {
        Instance = this;
    }

    public void AddResourcesWithFloatingText(int copperAmount, int ironAmount, int titaniumAmount, int diamondAmount, Vector3 position)
    {
        AddResources(copperAmount, ironAmount, titaniumAmount, diamondAmount);
        
        GameObject floatingTextObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        TextMeshPro textMesh = floatingTextObj.GetComponent<TextMeshPro>();
        textMesh.text = "";
        
        if(copperAmount > 0) textMesh.text += $"+{copperAmount} Copper\n";
        if(ironAmount > 0) textMesh.text += $"+{ironAmount} Iron\n";
        if(titaniumAmount > 0) textMesh.text += $"+{titaniumAmount} Titanium\n";
        if(diamondAmount > 0) textMesh.text += $"+{diamondAmount} Diamond\n";
    }
    
    public void AddResources(int copperAmount, int ironAmount, int titaniumAmount, int diamondAmount)
    {
        copper += copperAmount;
        iron += ironAmount;
        titanium += titaniumAmount;
        diamond += diamondAmount;
    }
    
    public bool CanUseResources(int copperAmount, int ironAmount, int titaniumAmount, int diamondAmount)
    {
        return copper >= copperAmount && iron >= ironAmount && titanium >= titaniumAmount && diamond >= diamondAmount;
    }
    
    public bool UseResources(int copperAmount, int ironAmount, int titaniumAmount, int diamondAmount)
    {
        if (CanUseResources(copperAmount, ironAmount, titaniumAmount, diamondAmount))
        {
            copper -= copperAmount;
            iron -= ironAmount;
            titanium -= titaniumAmount;
            diamond -= diamondAmount;
            return true;
        }
        return false;
    }
}