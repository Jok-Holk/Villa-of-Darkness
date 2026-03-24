using UnityEngine;
using UnityEngine.Events;

public class GazeTrigger : MonoBehaviour
{
    [SerializeField] private float _gazeThreshold = 3f;
    [SerializeField] private float _gazeTimer = 0f;
    public UnityEvent OnGazeComplete;
}
