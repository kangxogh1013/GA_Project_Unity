using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponEnhanceUI : MonoBehaviour
{
    [SerializeField] private List<WeaponMaterial> enhancementMaterials = new();
    [SerializeField] private int currentWeaponLevel = 1;

    [SerializeField] private Text weaponLevelDisplay;
    [SerializeField] private Text calculationResultDisplay;

    [SerializeField] private Button bruteForceButton;
    [SerializeField] private Button minimizeWasteButton;
    [SerializeField] private Button maximizeEfficiencyButton;
    [SerializeField] private Button prioritizeExpButton;
    [SerializeField] private Button enhanceButton;

    private WeaponEnhancementResult currentResult;

    private int RequiredExperienceForNextLevel
        => 8 * (currentWeaponLevel + 1) * (currentWeaponLevel + 1);

    private void OnEnable()
    {
        bruteForceButton.onClick.AddListener(ExecuteBruteForceCalculation);
        minimizeWasteButton.onClick.AddListener(ExecuteMinimizeWaste);
        maximizeEfficiencyButton.onClick.AddListener(ExecuteMaxEfficiency);
        prioritizeExpButton.onClick.AddListener(ExecutePrioritizeExp);
        enhanceButton.onClick.AddListener(ExecuteWeaponEnhance);
    }

    private void OnDisable()
    {
        bruteForceButton.onClick.RemoveListener(ExecuteBruteForceCalculation);
        minimizeWasteButton.onClick.RemoveListener(ExecuteMinimizeWaste);
        maximizeEfficiencyButton.onClick.RemoveListener(ExecuteMaxEfficiency);
        prioritizeExpButton.onClick.RemoveListener(ExecutePrioritizeExp);
        enhanceButton.onClick.RemoveListener(ExecuteWeaponEnhance);
    }

    private void Start()
    {
        UpdateLevelDisplay();
    }

    private void DisplayResult(WeaponEnhancementResult result)
    {
        currentResult = result;
        if (calculationResultDisplay != null)
            calculationResultDisplay.text = result.ToString();

        Debug.Log($"[무기 강화 계산]\n{result}");
    }

    private void ExecuteBruteForceCalculation()
    {
        Debug.Log("[완전 탐색 시작]");
        var result = WeaponEnhanceCalculator.BruteForceSearch(
            enhancementMaterials,
            RequiredExperienceForNextLevel
        );
        DisplayResult(result);
    }

    private void ExecuteMinimizeWaste()
    {
        Debug.Log("[경험치 낭비 최소화]");
        var result = WeaponEnhanceCalculator.MinimizeExcessExp(
            enhancementMaterials,
            RequiredExperienceForNextLevel
        );
        DisplayResult(result);
    }

    private void ExecuteMaxEfficiency()
    {
        Debug.Log("[골드 효율 최대화]");
        var result = WeaponEnhanceCalculator.MaximizeGoldEfficiency(
            enhancementMaterials,
            RequiredExperienceForNextLevel
        );
        DisplayResult(result);
    }

    private void ExecutePrioritizeExp()
    {
        Debug.Log("[높은 경험치 우선]");
        var result = WeaponEnhanceCalculator.PrioritizeHighExp(
            enhancementMaterials,
            RequiredExperienceForNextLevel
        );
        DisplayResult(result);
    }

    private void ExecuteWeaponEnhance()
    {
        if (currentResult != null && currentResult.ObtainedExp >= RequiredExperienceForNextLevel)
        {
            currentWeaponLevel++;
            UpdateLevelDisplay();
            calculationResultDisplay.text = "";
            currentResult = null;
            Debug.Log($"[강화 성공] 현재 레벨: {currentWeaponLevel}");
        }
        else
        {
            Debug.LogWarning("[강화 실패] 경험치가 부족합니다!");
        }
    }

    private void UpdateLevelDisplay()
    {
        if (weaponLevelDisplay != null)
            weaponLevelDisplay.text = $"무기 레벨: +{currentWeaponLevel}";
    }
}
