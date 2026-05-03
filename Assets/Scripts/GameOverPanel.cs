using System;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private AudioSource gAudioSource;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        gAudioSource.Play();
    }
}
