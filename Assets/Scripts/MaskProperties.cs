using UnityEngine;

public class MaskProperties : MonoBehaviour
{
    [Header("Mask Properties")]
    public Color maskColor = Color.white;
    public bool isCracked;
    public EmotionType emotion;

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
