using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ChapterTransition : MonoBehaviour
{
    [SerializeField] private bool _isPlaying = false;
    [SerializeField] private float _duration = 2f;
    [SerializeField] private string _chapterName;
    public UnityEvent OnTransitionComplete = new UnityEvent();

    public void PlayTransition(string chapterName, string year)
    {
        if (_isPlaying) return;
        _isPlaying = true;
        _chapterName = chapterName;
        StartCoroutine(Run());
    }
    private IEnumerator Run() 
    { 
        yield return new WaitForSeconds(_duration); 
        _isPlaying = false; 
        OnTransitionComplete.Invoke();
    }
}
