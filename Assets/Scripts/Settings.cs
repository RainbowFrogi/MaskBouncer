using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum RuleResult { Allow, Deny }

[System.Serializable]
public struct EntryFacts
{
    public MaskProperties.MaskColor maskColor;
    public int crackAmounts;
    public MaskProperties.EmotionType emotion;

    public EntryFacts(MaskProperties props)
    {
        maskColor = props != null ? props.maskColor : MaskProperties.MaskColor.White;
        crackAmounts = props != null ? props.crackAmounts : 0;
        emotion = props != null ? props.emotion : MaskProperties.EmotionType.Neutral;
    }
}

[System.Serializable]
public class EntryRule
{
    public bool useMaskColor;
    public MaskProperties.MaskColor maskColor;

    public bool useCrackAmounts;
    public int crackAmounts;

    public bool useEmotion;
    public MaskProperties.EmotionType emotion;

    public RuleResult result;

    public int SpecificityScore()
    {
        int s = 0;
        if (useMaskColor) s++;
        if (useCrackAmounts) s++;
        if (useEmotion) s++;
        return s;
    }

    public bool Matches(in EntryFacts f)
    {
        if (useMaskColor && maskColor != f.maskColor) return false;
        if (useCrackAmounts && crackAmounts != f.crackAmounts) return false;
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
    [SerializeField] private TMP_Text ruleText;

    private int currentDifficulty;
    private int masksProcessed;

    private void Start()
    {
        if (autoGenerateRules)
        {
            currentDifficulty = Mathf.Max(1, startingDifficulty);
            GenerateInitialRule(currentDifficulty);
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

    public void RegisterDecision()
    {
        if (!autoGenerateRules)
        {
            return;
        }

        masksProcessed++;
        if (masksPerNewRule <= 0)
        {
            return;
        }

        if (masksProcessed % masksPerNewRule == 0)
        {
            currentDifficulty = Mathf.Max(1, currentDifficulty + 1);
            AddRule(currentDifficulty);
        }
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
        rule.useCrackAmounts = Random.value < fieldChance;
        rule.useEmotion = Random.value < fieldChance;

        if (!rule.useMaskColor && !rule.useCrackAmounts && !rule.useEmotion)
        {
            rule.useMaskColor = true;
        }

        rule.maskColor = (MaskProperties.MaskColor)Random.Range(0, System.Enum.GetValues(typeof(MaskProperties.MaskColor)).Length);
        rule.crackAmounts = Random.Range(0, 4 + Mathf.Min(difficulty, 6));
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

        rule.useCrackAmounts = denyRule.useCrackAmounts;
        rule.crackAmounts = denyRule.crackAmounts;

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

            if (!rule.useCrackAmounts && Random.value < 0.5f)
            {
                rule.useCrackAmounts = true;
                rule.crackAmounts = Random.Range(0, 4 + Mathf.Min(difficulty, 6));
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
               a.useCrackAmounts == b.useCrackAmounts &&
               a.useEmotion == b.useEmotion &&
               (!a.useMaskColor || a.maskColor == b.maskColor) &&
               (!a.useCrackAmounts || a.crackAmounts == b.crackAmounts) &&
               (!a.useEmotion || a.emotion == b.emotion);
    }

    private static bool RulesCanOverlap(EntryRule a, EntryRule b)
    {
        if (a.useMaskColor && b.useMaskColor && a.maskColor != b.maskColor) return false;
        if (a.useCrackAmounts && b.useCrackAmounts && a.crackAmounts != b.crackAmounts) return false;
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

        if (rule.useCrackAmounts)
        {
            text += first ? "" : ", ";
            text += $"{rule.crackAmounts} cracks";
            first = false;
        }

        if (rule.useEmotion)
        {
            text += first ? "" : ", ";
            text += $"{rule.emotion} emotion";
        }

        return text;
    }
}
