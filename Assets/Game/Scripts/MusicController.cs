using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    static MusicController instance;
    public static MusicController Instance => instance ? instance : instance = FindObjectOfType<MusicController>();
    
    AudioSource previous;
    AudioSource current;

    int currentIndex;
    AudioSource[] musics;

    AudioSource[] Musics => musics ?? (musics = GetComponentsInChildren<AudioSource>());

    [SerializeField] float crossFadeSpeed = 2;
    [SerializeField] float crossFade = 0;
    [SerializeField] float maxVolume = 1;

    public float MaxVolume
    {
        get { return maxVolume; }
        set { maxVolume = value; }
    }

    void Awake()
    {
        current = Musics[currentIndex];
        current.Play();
    }

    void Update()
    {
        crossFade += Time.deltaTime * crossFadeSpeed;
        crossFade = Mathf.Clamp01(crossFade);
        current.volume = crossFade * maxVolume;

        if(!previous) return;
        previous.volume = (1 - crossFade) * maxVolume;
        if (crossFade >= 0.99f) previous.Stop();
    }

    public void Play(int newIndex)
    {
        if(newIndex == currentIndex) return;
        currentIndex = newIndex;
        previous = current;
        current = Musics[newIndex];
        current.Play();
        crossFade = 0;
    }
}
