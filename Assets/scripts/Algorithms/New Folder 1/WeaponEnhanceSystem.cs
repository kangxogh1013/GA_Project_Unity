using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEnhancementResult
{
    public int RequiredExp { get; set; }
    public int TotalCost { get; set; }
    public int ObtainedExp { get; set; }
    public Dictionary<WeaponMaterial, int> MaterialUsage { get; set; } = new();

    public int ExcessExp => ObtainedExp - RequiredExp;

    public override string ToString()
    {
        string output = $"필요 경험치: {RequiredExp}\n획득 경험치: {ObtainedExp}\n";
        output += $"소비 골드: {TotalCost}\n초과 경험치: {ExcessExp}\n\n강화 재료:\n";

        foreach (var material in MaterialUsage)
        {
            output += $"{material.Key.materialName}: {material.Value}개 (경험치: {material.Value * material.Key.experience}, 비용: {material.Value * material.Key.cost})\n";
        }

        return output;
    }
}

public class WeaponEnhanceCalculator
{
    /// <summary>
    /// 완전 탐색: 모든 조합을 시도해서 최적의 조합 찾기
    /// </summary>
    public static WeaponEnhancementResult BruteForceSearch(List<WeaponMaterial> materials, int requiredExp)
    {
        WeaponEnhancementResult optimal = null;
        int optimalScore = int.MaxValue;

        var maxQuantities = materials.Select(m => requiredExp / m.experience + 1).ToList();

        void SearchCombinations(List<int> currentCombination, int materialIndex)
        {
            if (materialIndex == materials.Count)
            {
                int totalExp = currentCombination.Sum((qty, idx) => qty * materials[idx].experience);
                if (totalExp < requiredExp) return;

                int totalCost = currentCombination.Sum((qty, idx) => qty * materials[idx].cost);
                int score = totalExp - requiredExp + totalCost;

                if (score < optimalScore)
                {
                    optimalScore = score;
                    optimal = new WeaponEnhancementResult
                    {
                        RequiredExp = requiredExp,
                        TotalCost = totalCost,
                        ObtainedExp = totalExp,
                        MaterialUsage = currentCombination
                            .Select((qty, idx) => new { material = materials[idx], qty })
                            .Where(x => x.qty > 0)
                            .ToDictionary(x => x.material, x => x.qty)
                    };
                }
                return;
            }

            for (int quantity = 0; quantity <= maxQuantities[materialIndex]; quantity++)
            {
                currentCombination.Add(quantity);
                SearchCombinations(currentCombination, materialIndex + 1);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }

        SearchCombinations(new(), 0);
        return optimal ?? new WeaponEnhancementResult { RequiredExp = requiredExp };
    }

    /// <summary>
    /// 경험치 낭비 최소: 작은 단위 재료들을 활용해 낭비 최소화
    /// </summary>
    public static WeaponEnhancementResult MinimizeExcessExp(List<WeaponMaterial> materials, int requiredExp)
    {
        var sorted = materials.OrderBy(m => m.experience).ToList();
        var smallest = sorted[0];
        var second = sorted[1];

        int remaining = requiredExp;
        int smallestCount = 0;
        int secondCount = 0;

        while (remaining > 0)
        {
            if (remaining >= second.experience)
            {
                secondCount++;
                remaining -= second.experience;
            }
            else
            {
                smallestCount++;
                remaining -= smallest.experience;
            }
        }

        var result = new WeaponEnhancementResult
        {
            RequiredExp = requiredExp,
            TotalCost = smallestCount * smallest.cost + secondCount * second.cost,
            ObtainedExp = smallestCount * smallest.experience + secondCount * second.experience,
            MaterialUsage = new()
        };

        if (smallestCount > 0) result.MaterialUsage[smallest] = smallestCount;
        if (secondCount > 0) result.MaterialUsage[second] = secondCount;

        return result;
    }

    /// <summary>
    /// 골드 효율 최대: 경험치/비용 비율이 가장 좋은 재료 우선 구매
    /// </summary>
    public static WeaponEnhancementResult MaximizeGoldEfficiency(List<WeaponMaterial> materials, int requiredExp)
    {
        var sorted = materials.OrderByDescending(m => m.EfficiencyRatio).ToList();
        return GreedyPurchase(sorted, materials[0], requiredExp);
    }

    /// <summary>
    /// 경험치 우선: 경험치가 큰 재료부터 구매
    /// </summary>
    public static WeaponEnhancementResult PrioritizeHighExp(List<WeaponMaterial> materials, int requiredExp)
    {
        var sorted = materials.OrderByDescending(m => m.experience).ToList();
        return GreedyPurchase(sorted, materials[0], requiredExp);
    }

    private static WeaponEnhancementResult GreedyPurchase(List<WeaponMaterial> sortedMaterials,
                                                          WeaponMaterial fillMaterial,
                                                          int requiredExp)
    {
        var usage = new Dictionary<WeaponMaterial, int>();
        int totalExp = 0;
        int totalCost = 0;
        int needed = requiredExp;

        foreach (var material in sortedMaterials)
        {
            int quantity = needed / material.experience;
            if (quantity > 0)
            {
                usage[material] = quantity;
                totalExp += quantity * material.experience;
                totalCost += quantity * material.cost;
                needed -= quantity * material.experience;
            }
        }

        // 남은 경험치를 가장 작은 단위로 채우기
        while (needed > 0)
        {
            if (!usage.ContainsKey(fillMaterial))
                usage[fillMaterial] = 0;

            usage[fillMaterial]++;
            totalExp += fillMaterial.experience;
            totalCost += fillMaterial.cost;
            needed -= fillMaterial.experience;
        }

        return new WeaponEnhancementResult
        {
            RequiredExp = requiredExp,
            TotalCost = totalCost,
            ObtainedExp = totalExp,
            MaterialUsage = usage
        };
    }
}
