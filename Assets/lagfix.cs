using UnityEngine;

public class lagfix : MonoBehaviour
{
    public float normalTimeScale = 1f;
    public float unfocusedTimeScale = 1f;

    void Update()
    {
        Application.targetFrameRate = 600;
    }
}
