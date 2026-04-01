using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    // This class basically holds assets that are used repeatedly like SFX and VFX

    public static AssetManager Instance { get; private set; }

    [SerializeField] private AssetLoadObject preloadAssets;
    public Dictionary<string, VFXObject> vfxAssets = new Dictionary<string, VFXObject>();
    public Dictionary<string, SFXObject> sfxAssets = new Dictionary<string, SFXObject>();
    public Dictionary<string, MusicObject> musicAssets = new Dictionary<string, MusicObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Preload
        LoadAssets(preloadAssets);
    }

    public void LoadAssets(AssetLoadObject assets)
    {
        if (assets == null)
        {
            Debug.LogWarning($"Could not load empty asset batch.");
            return;
        }

        // Music
        for (int i = 0; i < assets.music.Length; i++)
        {
            if (!musicAssets.ContainsKey(assets.music[i].name))
            {
                musicAssets.Add(assets.music[i].name, assets.music[i]);
                Debug.Log($"Loaded music track '{assets.music[i].name}'.");
            }
        }

        // SFX
        for (int i = 0; i < assets.sfx.Length; i++)
        {
            if (!sfxAssets.ContainsKey(assets.sfx[i].name))
            {
                sfxAssets.Add(assets.sfx[i].name, assets.sfx[i]);
                Debug.Log($"Loaded SFX '{assets.sfx[i].name}'.");
            }
        }

        // SFX
        for (int i = 0; i < assets.vfx.Length; i++)
        {
            if (!vfxAssets.ContainsKey(assets.vfx[i].name))
            {
                vfxAssets.Add(assets.vfx[i].name, assets.vfx[i]);
                Debug.Log($"Loaded VFX '{assets.vfx[i].name}'.");
            }
        }

        Debug.Log($"Loaded asset batch '{assets.name}' successfully.");
    }

    /*public IEnumerator LoadMusic(string music)
    {
        if (musicAssets.ContainsKey(music))
            yield break;

        ResourceRequest request = Resources.LoadAsync<MusicObject>("Music/" + music);
        yield return request;
        musicAssets.Add(music, request.asset as MusicObject);

        Debug.Log($"Loaded music track '{music}'.");
    }

    public IEnumerator LoadSFX(string sfx)
    {
        if (sfxAssets.ContainsKey(sfx))
            yield break;

        ResourceRequest request = Resources.LoadAsync<SFXObject>("SFX/" + sfx);
        yield return request;
        sfxAssets.Add(sfx, request.asset as SFXObject);

        Debug.Log($"Loaded SFX '{sfx}'.");
    }*/
}
