using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static Ease;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup mixer;
    private AudioSource musicSrc, fadeSrc;

    [Range(0.0f, 1.0f)] public float volume = 1f;
    private float fromVolume = 1f, toVolume = 1f, volumeProg = 1f, volumeSpd = 1f;

    [SerializeField] private float transTime = 0f;
    private float transMult = 1f;
    private Dictionary<string, float> songTimes = new Dictionary<string, float>();

    [SerializeField] private MusicObject[] musicPool = new MusicObject[3]; // Priority system
    [SerializeField] private int poolID = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        musicSrc = GetComponent<AudioSource>();
        fadeSrc = new GameObject("fadeMusic").AddComponent<AudioSource>();
        fadeSrc.transform.parent = transform;
        musicSrc.outputAudioMixerGroup = mixer;
        fadeSrc.outputAudioMixerGroup = mixer;
    }

    // Update is called once per frame
    void Update()
    {
        // Music volume and fading
        if (transTime > 0f)
        {
            transTime = Mathf.Max(transTime - Time.deltaTime * transMult, 0f);
            if (transTime <= 0f)
                fadeSrc.Stop();
        }

        if(volumeProg < 1f)
        {
            volumeProg = Mathf.Min(volumeProg + Time.deltaTime * volumeSpd, 1f);
            volume = Sine(fromVolume + (toVolume - fromVolume) * volumeProg, EasingDirection.In);

            if(volumeProg == 1f && volume == 0f)
            {
                RemoveAllSongs(true);
                volume = 1f;
            }
        }

        musicSrc.volume = volume * Sine((1f - transTime), EasingDirection.Out);
        fadeSrc.volume = volume * Sine(transTime, EasingDirection.Out);

        // Looping
        if (poolID >= 0 && poolID < musicPool.Length)
        {
            if (musicPool[poolID].loop)
            {
                // Loop
                if(musicPool[poolID].loopEnd > musicPool[poolID].loopStart)
                {
                    if(musicSrc.time >= musicPool[poolID].loopEnd)
                    {
                        musicSrc.time = musicPool[poolID].loopStart + (musicSrc.time - musicPool[poolID].loopEnd);
                    }
                }
            }
            else
            {
                // Check when song stops playing
                if (!musicSrc.isPlaying)
                    RemoveSong(poolID);
            }
        }
    }

    void StartSong(bool instant = false, bool startFromBeginning = false)
    {
        // Get the top song
        poolID = -1;
        for (int i = musicPool.Length - 1; i >= 0; i--)
        {
            if (musicPool[i] != null)
                poolID = i;
        }

        // Start playing the new song
        if (poolID != -1)
        {
            if (musicSrc.isPlaying && musicPool[poolID].music == musicSrc.clip)
            {
                // Keep playing the same song
                if(startFromBeginning)
                    musicSrc.time = 0f;
                return;
            }

            if (instant)
            {
                musicSrc.clip = musicPool[poolID].music;
                musicSrc.loop = musicPool[poolID].loop;
                if (!startFromBeginning)
                    musicSrc.time = songTimes[musicPool[poolID].name];
                else
                    musicSrc.time = 0f;

                musicSrc.Play();
            }
            else
            {
                fadeSrc.clip = musicSrc.clip;
                fadeSrc.loop = false;
                if(fadeSrc.clip)
                    fadeSrc.time = musicSrc.time;

                musicSrc.clip = musicPool[poolID].music;
                musicSrc.loop = musicPool[poolID].loop;
                if (!startFromBeginning)
                    musicSrc.time = songTimes[musicPool[poolID].name];
                else
                    musicSrc.time = 0f;

                fadeSrc.Play();
                musicSrc.Play();

                transTime = 1f;
            }
        }
        else
        {
            if (instant)
            {
                musicSrc.Stop();
                musicSrc.clip = null;
            }
            else
            {
                fadeSrc.clip = musicSrc.clip;
                fadeSrc.loop = false;
                if (fadeSrc.clip)
                    fadeSrc.time = musicSrc.time;

                fadeSrc.Play();
                musicSrc.Stop();
                musicSrc.clip = null;

                transTime = 1f;
            }
        }

        Debug.Log($"Started playing song at pool position #{poolID}.");
    }

    public void PlaySong(MusicObject song, int priority = 0, bool instant = false, bool startFromBeginning = false)
    {
        if(song != null && priority >= 0 && priority < musicPool.Length)
        {
            // Save the old tracks position
            if(poolID == priority && musicPool[priority] != null)
                songTimes[musicPool[priority].name] = musicSrc.time;

            // Insert the new track
            musicPool[priority] = song;

            // Handle saved song position
            if(!songTimes.ContainsKey(song.name))
                songTimes.Add(song.name, 0f);
            else if(startFromBeginning)
                songTimes[song.name] = 0f;
            
            // Handle song swap
            if(poolID <= priority)
                StartSong(instant, startFromBeginning);

            Debug.Log($"Added song '{song.name}' to the music pool at position #{priority}.");
        }
        else
        {
            if(song == null)
                Debug.LogWarning("Couldn't add song to the music pool: Music Object doesn't exist!");
            else
                Debug.LogWarning($"Couldn't add song '{song.name}' to the music pool: Out of range!");
        }
    }

    public void RemoveSong(int poolPosition, bool instant = false)
    {
        if (poolPosition >= 0 && poolPosition < musicPool.Length)
        {
            // Save the old tracks position
            if (poolID == poolPosition && musicPool[poolPosition] != null)
                songTimes[musicPool[poolPosition].name] = musicSrc.time;

            // Remove the track
            musicPool[poolPosition] = null;

            // Play the next available song
            if(poolID == poolPosition)
                StartSong(instant);

            Debug.Log($"Removed song #{poolPosition} from pool.");
        }
        else
            Debug.LogWarning($"Couldn't remove song #{poolPosition} from pool: Out of range!");
    }

    public void RemoveAllSongs(bool instant = false)
    {
        // Save the old tracks position
        if (poolID != -1 && musicPool[poolID] != null)
            songTimes[musicPool[poolID].name] = musicSrc.time;

        // Remove the tracks
        for (int i = 0; i < musicPool.Length; i++)
            musicPool[i] = null;

        StartSong(instant);

        Debug.Log("Cleared all songs from pool.");
    }

    public void FadeVolume(float newVolume = 0f, float speed = 1f)
    {
        fromVolume = volume;
        toVolume = newVolume;
        volumeSpd = speed;
        volumeProg = 0f;

        Debug.Log($"Fading volume towards {newVolume}.");
    }

    public void ClearSongPositions()
    {
        songTimes.Clear();
    }
}
