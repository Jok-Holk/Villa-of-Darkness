using UnityEngine;

public class DebugSanity : MonoBehaviour
{
    private SanitySystem _sanity;
    void Start() => _sanity = FindObjectOfType<SanitySystem>();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) _sanity.DecreaseSanity(0.35f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) _sanity.DecreaseSanity(0.35f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) _sanity.DecreaseSanity(0.25f);
    }
}