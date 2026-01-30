using UnityEngine;

public class MaskProperties : MonoBehaviour
{
    [Header("Mask Properties")]
    public MaskColor maskColor = MaskColor.White;
    public int crackAmounts;
    public EmotionType emotion;

    public enum MaskColor
    {
        White,
        Black,
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }

    public enum EmotionType
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Fear,
        Surprise
    }
}
