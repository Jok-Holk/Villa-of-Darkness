using UnityEngine;

[System.Serializable]
public struct SanityLevelSettings
{
    public string levelName;

    [Header("Sanity Range")]
    public float sanityMin;   // nấc này bắt đầu từ %
    public float sanityMax;   // nấc này kết thúc ở %

    [Header("Drain")]
    public float drainRate;   // tiêu hao sanity/giây ở nấc này

    [Header("Shake")]
    public float traumaTarget;
    public float shakeIntensityX;
    public float shakeIntensityY;

    [Header("Post-Process")]
    public float grain;
    public float chromatic;
    public float saturation;
    public float exposure;
    public float vignette;
    public float lensDistort;
}

[CreateAssetMenu(fileName = "SanityData", menuName = "ScriptableObjects/SanityData")]
public class SanityData : ScriptableObject
{
    [Header("Nấc sanity — sắp xếp từ cao xuống thấp")]
    public SanityLevelSettings[] levels;

    [Header("Sanity Recovery")]
    public float recoveryRate;  // hồi sanity/giây khi đứng trong vùng sáng
}