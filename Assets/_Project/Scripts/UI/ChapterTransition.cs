using UnityEngine;
using System.Collections;

public class ChapterTransition : MonoBehaviour
{
    [SerializeField] private bool _isPlaying = false;
    public void PlayTransition(string chapterName, string year)
    {
        _isPlaying = true;
        StartCoroutine(Run());
    }
    private IEnumerator Run() { yield return new WaitForSeconds(2f); _isPlaying = false; }
}
