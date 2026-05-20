using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Drill : MonoBehaviour
{
    private float timer = 0f;

    public int copperAmount;
    public int ironAmount;
    public int titaniumAmount;
    public int diamondAmount;

    public void ConfigureFromOreTag(string oreTag)
    {
        switch (oreTag)
        {
            case "CopperOre":
                copperAmount = 10;
                break;
            case "IronOre":
                ironAmount = 10;
                break;
            case "TitaniumOre":
                titaniumAmount = 10;
                break;
            default:
                Debug.LogWarning($"Drill.ConfigureFromOreTag: unsupported ore tag '{oreTag}'. Drill will not mine.");
                break;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 10f)
        {
            timer = 0f;
            MineResource();
        }
    }

    private void MineResource()
    {
        if (ResourceManager.Instance == null)
            return;

        ResourceManager.Instance.AddResourcesWithFloatingText(
            copperAmount,
            ironAmount,
            titaniumAmount,
            diamondAmount,
            transform.position + Vector3.up * 4
        );
    }
}