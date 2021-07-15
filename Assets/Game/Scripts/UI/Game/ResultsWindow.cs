using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsWindow : MonoBehaviour
{
    int current = 0;
    AudioSource source;
    
    [SerializeField] GuestInfo[] guests;
    [SerializeField] Image[] portraits;
    [SerializeField] Text[] reactionsLabels;
    [SerializeField] Text yourScoreLabel;
    [SerializeField] Text himScoreLabel;
    [SerializeField] Text resultLabel;
    [SerializeField] AudioSource[] musics;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Show(int[] yourScores, int[] himScores)
    {
        current = 0;
        
        var yourScore = 0;
        for (var i = 0; i < yourScores.Length; i++)
            yourScore += yourScores[i];

        var himScore = 0;
        for (var i = 0; i < himScores.Length; i++)
            himScore += himScores[i];

        resultLabel.text = yourScore > himScore ? "Victory" : "You Lose";
        yourScoreLabel.text = yourScore.ToString();
        himScoreLabel.text = himScore.ToString();

        for (var i = 0; i < guests.Length; i++)
        {
            var youWinGuest = yourScores[i] > himScores[i];
            guests[i].Satisfied = youWinGuest;
            reactionsLabels[i].text = youWinGuest ? "Like" : "Dislike";
            portraits[i].sprite = youWinGuest ? guests[i].Nationality.HappyFace : guests[i].Nationality.SadFace;
        }
        
        gameObject.SetActive(true);

        for (var i = 0; i < musics.Length; i++)
            musics[i].volume = 0.25f;
    }

    void Update()
    {
        MusicController.Instance.MaxVolume = Mathf.Lerp(
            MusicController.Instance.MaxVolume,
            current <= 2 ? 0.2f : 1,
            0.1f);
        
        if (current <= 2 && !source.isPlaying)
        {
            source.clip = guests[current].Satisfied
                ? guests[current].Nationality.HappySound
                : guests[current].Nationality.SadSound;
            source.Play();
            current++;
        }
    }
}