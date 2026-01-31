using UnityEngine;

public class MaskProperties : MonoBehaviour
{
    [Header("Mask Properties")]
    public MaskColor maskColor;
    public MaskType maskType;

    public enum MaskColor
    {
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }

    public enum MaskType
    {
        Happy,
        Angry,
        Winking,
        CryLaugh,
        Hat,
        Crying,
        Blushing,
        Scared,
        Evil,
        Broken,
        XD,
        Karjala
    }
}
