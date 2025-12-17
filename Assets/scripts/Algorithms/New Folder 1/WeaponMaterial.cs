using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Material", menuName = "Weapon/Enhancement Material")]
public class WeaponMaterial : ScriptableObject
{
    public string materialName;
    public int experience;
    public int cost;

    public float EfficiencyRatio
    {
        get
        {
            return cost == 0 ? 0 : (float)experience / (float)cost;
        }
    }
}
