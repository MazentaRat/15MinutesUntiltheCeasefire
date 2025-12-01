using UnityEngine;

public class BOOMSound : MonoBehaviour
{
    AudioSource AudioSource;
    private void OnEnable()
    {
        AudioSource = GetComponent<AudioSource>();
    }
    public void Play()
    {
        if (AudioSource != null) 
            AudioSource.Play();
    }
}
