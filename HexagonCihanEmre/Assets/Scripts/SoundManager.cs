using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource soundEffectPlayer;
    [SerializeField] private AudioClip swipe;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip bombAlert;
    [SerializeField] private AudioClip bombDestroyedNotification;

    private void Awake()
    {
        instance = this;
    }

    //Method for playing a sound effect when player swiped to rotate the hexagons
    public void PlaySwipeEffect()
    {
        soundEffectPlayer.PlayOneShot(swipe);      
    }

    //Method for playing a sound effect when hexagons are blown
    public void PlayExplosion()
    {
        soundEffectPlayer.PlayOneShot(explosion);      
    }

    //Method for playing a sound effect when a bomb is dropped to the grid
    public void PlayBombAlert()
    {
        soundEffectPlayer.PlayOneShot(bombAlert);      
    }

    //Method for PlayBombDestroyedNotifyCoroutine() 
    public void PlayBombDestroyedNotify()
    {
       soundEffectPlayer.PlayOneShot(bombDestroyedNotification);      
    }

    //Coroutine for playing a sound effect when player destroyed a bomb hexagon
    public IEnumerator PlayBombDestroyedNotifyCoroutine()
    {
        yield return new WaitForSeconds(0.85f);
        PlayBombDestroyedNotify();
    }
}