using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

public class SFXManager : MonoBehaviour
{
    private static SFXManager _instance;
    public static SFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SFXManager>();
                if (_instance != null)
                    return _instance;
                SFXManager prefab = Resources.Load<SFXManager>("Prefabs/Managers/SFXManager");
                _instance = Instantiate(prefab.gameObject).GetComponent<SFXManager>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    [SerializeField] private AudioSource _audioSourcePrefab;
    private static Stack<AudioSource> _audioSourcePool = new Stack<AudioSource>();
    static Transform root;

    // Root is destroyed with its scene (or when leaving play mode), taking pooled objects with it.
    // A destroyed root reads as null, so stale entries are dropped lazily; no scene hooks needed.
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        root = new GameObject("SoundPool").transform;
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
            newSource = Instantiate(_audioSourcePrefab, root);
        else {
            newSource = _audioSourcePool.Pop();
            
        }
        newSource.transform.position = audioSource;
        newSource.clip = audioClip;
        newSource.volume = volume;
        newSource.Play();
        StartCoroutine(RecycleSource(newSource, newSource.clip.length));
    }

    private static IEnumerator RecycleSource(AudioSource audioSource, float timeToRecycle)
    {
        yield return new WaitForSeconds(timeToRecycle);
        audioSource.Stop();
        _audioSourcePool.Push(audioSource);
    }
}
