using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RuleResult { Allow, Deny }

[System.Serializable]
public struct EntryFacts
{
    public MaskProperties.MaskColor maskColor;
    public bool hasCracks;
    public MaskProperties.EmotionType emotion;

    public EntryFacts(MaskProperties props)
    {
        maskColor = props != null ? props.maskColor : MaskProperties.MaskColor.White;
        hasCracks = props != null && props.hasCracks;
        emotion = props != null ? props.emotion : MaskProperties.EmotionType.Neutral;
    }
}

[System.Serializable]
public class EntryRule
{
    public bool useMaskColor;
    public MaskProperties.MaskColor maskColor;

    public bool useHasCracks;
    public bool hasCracks;

    public bool useEmotion;
    public MaskProperties.EmotionType emotion;

    public RuleResult result;

    public int SpecificityScore()
    {
        int s = 0;
        if (useMaskColor) s++;
        if (useHasCracks) s++;
        if (useEmotion) s++;
        return s;
    }

    public bool Matches(in EntryFacts f)
    {
        if (useMaskColor && maskColor != f.maskColor) return false;
        if (useHasCracks && hasCracks != f.hasCracks) return false;
        if (useEmotion && emotion != f.emotion) return false;
        return true;
    }
}

public class Settings : MonoBehaviour
{
    [Header("Rules (top to bottom tie-breaker)")]
    public List<EntryRule> rules = new List<EntryRule>();

    [Header("Auto-Generate Rules")]
    [SerializeField] private bool autoGenerateRules = true;
    [SerializeField] private int startingDifficulty = 1;
    [SerializeField] private int maxRules = 8;
    [SerializeField] private int maxGenerateAttempts = 200;
    [SerializeField] private int masksPerNewRule = 3;
    [SerializeField] private int masksPerNewRuleStep = 1;
    [SerializeField] private TMP_Text ruleText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text masksRemainingText;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float ruleTimeSeconds = 180f;
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private GameObject gameOverPanel;

    private int currentDifficulty;
    private int masksProcessed;
    private int masksSinceLastRule;
    private int currentHealth;
    private int currentMasksPerNewRule;
    private float timeRemaining;
    private bool gameOver;

    private void Start()
    {
        if (autoGenerateRules)
        {
            currentDifficulty = Mathf.Max(1, startingDifficulty);
            GenerateInitialRule(currentDifficulty);
        }

        currentHealth = Mathf.Max(1, maxHealth);
        currentMasksPerNewRule = Mathf.Max(1, masksPerNewRule);
        timeRemaining = Mathf.Max(1f, ruleTimeSeconds);
        UpdateTimerText();
        UpdateMasksRemainingText();
        UpdateHealthUI();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public bool TryGetBestMatch(in EntryFacts f, out EntryRule bestRule)
    {
        bestRule = null;
        int bestScore = -1;

        for (int i = 0; i < rules.Count; i++)
        {
            EntryRule rule = rules[i];
            if (rule == null || !rule.Matches(f))
            {
                continue;
            }

            int score = rule.SpecificityScore();
            if (score > bestScore)
            {
                bestScore = score;
                bestRule = rule;
            }
        }

        return bestRule != null;
    }

    private void Update()
    {
        if (gameOver)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            TriggerGameOver("Time ran out.");
            return;
        }

        UpdateTimerText();
    }

    public void RegisterDecision(bool correct)
    {
        if (!correct)
        {
            currentHealth = Mathf.Max(0, currentHealth - 1);
            if (currentHealth == 0)
            {
                TriggerGameOver("Health depleted.");
            }
            UpdateHealthUI();
        }

        if (gameOver)
        {
            return;
        }

        if (!autoGenerateRules)
        {
            return;
        }

        masksProcessed++;
        masksSinceLastRule++;
        if (currentMasksPerNewRule <= 0)
        {
            return;
        }

        if (masksSinceLastRule >= currentMasksPerNewRule)
        {
            currentDifficulty = Mathf.Max(1, currentDifficulty + 1);
            AddRule(currentDifficulty);
            currentMasksPerNewRule = Mathf.Max(1, currentMasksPerNewRule + masksPerNewRuleStep);
            masksSinceLastRule = 0;
        }
        UpdateMasksRemainingText();
    }

    private void GenerateInitialRule(int difficulty)
    {
        rules.Clear();
        AddRule(difficulty);
    }

    private void AddRule(int difficulty)
    {
        if (rules.Count >= maxRules)
        {
            return;
        }

        int attempts = 0;
        while (attempts < maxGenerateAttempts)
        {
            attempts++;
            EntryRule rule = CreateRuleWithConstraints(difficulty);
            if (IsConflictingRule(rule))
            {
                continue;
            }
            rules.Add(rule);
            UpdateRuleText(rule);
            ResetTimer();
            UpdateMasksRemainingText();
            return;
        }
    }

    private EntryRule CreateRuleWithConstraints(int difficulty)
    {
        EntryRule denyRule;
        if (TryPickDenyRule(out denyRule) && Random.value < 0.5f)
        {
            EntryRule allowFromDeny = CreateAllowRuleFrom(denyRule, difficulty);
            if (allowFromDeny != null)
            {
                return allowFromDeny;
            }
        }

        return CreateRandomDenyRule(difficulty);
    }

    private EntryRule CreateRandomRule(int difficulty)
    {
        EntryRule rule = new EntryRule();

        float fieldChance = Mathf.Clamp01(0.25f + (difficulty * 0.1f));

        rule.useMaskColor = Random.value < fieldChance;
        rule.useHasCracks = Random.value < fieldChance;
        rule.useEmotion = Random.value < fieldChance;

        if (!rule.useMaskColor && !rule.useHasCracks && !rule.useEmotion)
        {
            rule.useMaskColor = true;
        }

        rule.maskColor = (MaskProperties.MaskColor)Random.Range(0, System.Enum.GetValues(typeof(MaskProperties.MaskColor)).Length);
        rule.hasCracks = Random.value < 0.5f;
        rule.emotion = (MaskProperties.EmotionType)Random.Range(0, System.Enum.GetValues(typeof(MaskProperties.EmotionType)).Length);

        rule.result = Random.value < 0.5f ? RuleResult.Allow : RuleResult.Deny;

        return rule;
    }

    private EntryRule CreateRandomDenyRule(int difficulty)
    {
        EntryRule rule = CreateRandomRule(difficulty);
        rule.result = RuleResult.Deny;
        return rule;
    }

    private bool TryPickDenyRule(out EntryRule denyRule)
    {
        denyRule = null;
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i] != null && rules[i].result == RuleResult.Deny)
            {
                denyRule = rules[i];
                return true;
            }
        }
        return false;
    }

    private EntryRule CreateAllowRuleFrom(EntryRule denyRule, int difficulty)
    {
        if (denyRule == null || denyRule.result != RuleResult.Deny)
        {
            return null;
        }

        EntryRule rule = new EntryRule();

        rule.useMaskColor = denyRule.useMaskColor;
        rule.maskColor = denyRule.maskColor;

        rule.useHasCracks = denyRule.useHasCracks;
        rule.hasCracks = denyRule.hasCracks;

        rule.useEmotion = denyRule.useEmotion;
        rule.emotion = denyRule.emotion;

        int attempts = 0;
        while (rule.SpecificityScore() <= denyRule.SpecificityScore() && attempts < maxGenerateAttempts)
        {
            attempts++;

            if (!rule.useMaskColor && Random.value < 0.5f)
            {
                rule.useMaskColor = true;
                rule.maskColor = (MaskProperties.MaskColor)Random.Range(0, System.Enum.GetValues(typeof(MaskProperties.MaskColor)).Length);
                continue;
            }

            if (!rule.useHasCracks && Random.value < 0.5f)
            {
                rule.useHasCracks = true;
                rule.hasCracks = Random.value < 0.5f;
                continue;
            }

            if (!rule.useEmotion)
            {
                rule.useEmotion = true;
                rule.emotion = (MaskProperties.EmotionType)Random.Range(0, System.Enum.GetValues(typeof(MaskProperties.EmotionType)).Length);
                continue;
            }
        }

        if (rule.SpecificityScore() <= denyRule.SpecificityScore())
        {
            return null;
        }

        rule.result = RuleResult.Allow;
        return rule;
    }

    private bool IsConflictingRule(EntryRule candidate)
    {
        for (int i = 0; i < rules.Count; i++)
        {
            EntryRule existing = rules[i];
            if (existing == null)
            {
                continue;
            }

            if (RulesAreIdentical(existing, candidate) && existing.result != candidate.result)
            {
                return true;
            }

            if (existing.SpecificityScore() == candidate.SpecificityScore() &&
                RulesCanOverlap(existing, candidate) &&
                existing.result != candidate.result)
            {
                return true;
            }
        }

        return false;
    }

    private static bool RulesAreIdentical(EntryRule a, EntryRule b)
    {
        return a.useMaskColor == b.useMaskColor &&
               a.useHasCracks == b.useHasCracks &&
               a.useEmotion == b.useEmotion &&
               (!a.useMaskColor || a.maskColor == b.maskColor) &&
               (!a.useHasCracks || a.hasCracks == b.hasCracks) &&
               (!a.useEmotion || a.emotion == b.emotion);
    }

    private static bool RulesCanOverlap(EntryRule a, EntryRule b)
    {
        if (a.useMaskColor && b.useMaskColor && a.maskColor != b.maskColor) return false;
        if (a.useHasCracks && b.useHasCracks && a.hasCracks != b.hasCracks) return false;
        if (a.useEmotion && b.useEmotion && a.emotion != b.emotion) return false;
        return true;
    }

    private void UpdateRuleText(EntryRule rule)
    {
        if (ruleText == null || rule == null)
        {
            return;
        }

        ruleText.text = DescribeRule(rule);
    }

    private static string DescribeRule(EntryRule rule)
    {
        string action = rule.result == RuleResult.Allow ? "Let in" : "Turn away";

        int parts = rule.SpecificityScore();
        if (parts <= 0)
        {
            return $"Newest Rule: {action} everyone";
        }

        if (rule.useMaskColor && parts == 1)
        {
            return $"Newest Rule: {action} all {rule.maskColor} masks";
        }

        string text = $"Newest Rule: {action} masks with ";
        bool first = true;

        if (rule.useMaskColor)
        {
            text += $"{rule.maskColor} color";
            first = false;
        }

        if (rule.useHasCracks)
        {
            text += first ? "" : ", ";
            text += rule.hasCracks ? "cracked" : "not cracked";
            first = false;
        }

        if (rule.useEmotion)
        {
            text += first ? "" : ", ";
            text += $"{rule.emotion} emotion";
        }

        return text;
    }

    private void UpdateHealthUI()
    {
        if (heartIcons == null || heartIcons.Length == 0)
        {
            return;
        }

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null)
            {
                continue;
            }
            heartIcons[i].enabled = i < currentHealth;
        }
    }

    private void UpdateMasksRemainingText()
    {
        if (masksRemainingText == null)
        {
            return;
        }

        int remaining = Mathf.Max(0, currentMasksPerNewRule - masksSinceLastRule);
        masksRemainingText.text = $"Masks to next rule: {remaining}";
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        int remainder = seconds % 60;
        timerText.text = $"{minutes:00}:{remainder:00}";
    }

    private void ResetTimer()
    {
        timeRemaining = Mathf.Max(1f, ruleTimeSeconds);
        UpdateTimerText();
    }

    private void TriggerGameOver(string reason)
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        Debug.LogWarning(reason, this);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
