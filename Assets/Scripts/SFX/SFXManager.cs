using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    [SerializeField] private AudioSource _audioSourcePrefab;
    private static Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlaySFXClip(AudioClip audioClip, Vector3 audioSource, float volume)
    {
        Instance.LocalPlaySFXClip(audioClip, audioSource, volume);
    }
    public static void PlaySFXClip(AudioClip[] audioClips, Vector3 audioSource, float volume)
    {
        if (audioClips.Length == 0)
            return;
        Instance.LocalPlaySFXClip(audioClips[Random.Range(0, audioClips.Length)], audioSource, volume);
    }
    private void LocalPlaySFXClip(AudioClip audioClip, Vector3 audioSource, float volume) // probably another thing for the audio type
    {
        AudioSource newSource = null;
        if (_audioSourcePool.Count == 0)
            newSource = Instantiate(_audioSourcePrefab, audioSource, quaternion.identity);
        else
            newSource = _audioSourcePool.Dequeue();

        newSource.clip = audioClip;
        newSource.volume = volume;
        newSource.Play();
        StartCoroutine(RecycleSource(newSource, newSource.clip.length));
    }

    private static IEnumerator RecycleSource(AudioSource audioSource, float timeToRecycle)
    {
        yield return new WaitForSeconds(timeToRecycle);
        audioSource.Stop();
        _audioSourcePool.Enqueue(audioSource);
    }
}
