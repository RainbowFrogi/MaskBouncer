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
