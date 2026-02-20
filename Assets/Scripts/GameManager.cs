using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        STANDARD
    };

    public GameState state = GameState.STANDARD;

    private PauseMenu pause;

    [Header("SFX and VFX")]
    public AudioMixerGroup mixerSFX;

    [SerializeField] private int sfxPoolSize = 40;
    [SerializeField] private int vfxPoolSize = 80;

    private AudioSource[] sfxPool;
    private SpriteRenderer[] vfxPool;
    private int sfxPoolIndex = -1, vfxPoolIndex = -1;

    private float[] sfxInCommission, vfxInCommission; // In here we can store floats that keep track of their lifetime
    private Dictionary<int, PlayableGraph> vfxGraphs = new Dictionary<int, PlayableGraph>(); // For VFX animation

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

        // Pause system
        pause = GetComponent<PauseMenu>();

        #region Pooling

        // Create the SFX and VFX object pools
        sfxPool = new AudioSource[sfxPoolSize];
        sfxInCommission = new float[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            // Create gameobject
            GameObject newObj = new GameObject("pooledSFX" + i);
            AudioSource audio = newObj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.maxDistance = 5f;
            audio.outputAudioMixerGroup = mixerSFX;

            // Store the object away
            DontDestroyOnLoad(newObj);
            newObj.transform.parent = transform;
            sfxPool[i] = audio;
            newObj.SetActive(false);
        }

        vfxPool = new SpriteRenderer[vfxPoolSize];
        vfxInCommission = new float[vfxPoolSize];
        for (int i = 0; i < vfxPoolSize; i++)
        {
            // Create gameobject
            GameObject newObj = new GameObject("pooledVFX" + i);
            SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
            Rigidbody2D rb = newObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            newObj.AddComponent<Animator>();

            // Store the object away
            DontDestroyOnLoad(newObj);
            newObj.transform.parent = transform;
            vfxPool[i] = sr;
            newObj.SetActive(false);
        }

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        PoolUpdate();
    }

    #region SFX and VFX

    public AudioSource PlaySFX(string soundName, Vector2 position, float pitchMult = 1f)
    {
        if (AssetManager.Instance == null)
        {
            Debug.LogWarning("Could not play SFX: AssetManager couldn't be found.");
            return null;
        }
        else if (!AssetManager.Instance.sfxAssets.ContainsKey(soundName))
        {
            Debug.LogWarning($"Could not play SFX: '{soundName}' is not a valid key.");
            return null;
        }

        SFXObject data = AssetManager.Instance.sfxAssets[soundName];

        #region Pooling

        AudioSource src = null;

        // Pull object from pool
        for (int i = 0; i < sfxPool.Length; i++)
        {
            // Current index
            sfxPoolIndex++;
            if (sfxPoolIndex >= sfxPool.Length)
                sfxPoolIndex = 0;

            // Check if index is available
            if(sfxInCommission[sfxPoolIndex] == 0f)
            {
                src = sfxPool[sfxPoolIndex];
                break;
            }
        }

        // If nothing could be found from the pool we force the next SFX source in line
        if(src == null)
        {
            // Current index
            sfxPoolIndex++;
            if (sfxPoolIndex >= sfxPool.Length)
                sfxPoolIndex = 0;

            // Force reset and get
            DecommissionSFX(sfxPoolIndex);
            src = sfxPool[sfxPoolIndex];
        }

        #endregion

        // Init the SFX
        src.gameObject.SetActive(true);
        src.gameObject.name = $"sfx_{soundName}";
        src.transform.parent = null;
        src.transform.position = position;
        
        src.clip = data.sfx[UnityEngine.Random.Range(0, data.sfx.Length)];
        src.volume = UnityEngine.Random.Range(data.minVolume, data.maxVolume);
        src.pitch = UnityEngine.Random.Range(data.minPitch, data.maxPitch) * pitchMult;
        src.spatialBlend = data.spatialBlend;
        src.Play();

        if (data.loop)
        {
            src.loop = true;
            sfxInCommission[sfxPoolIndex] = -1f;
        }
        else
            sfxInCommission[sfxPoolIndex] = src.clip.length / src.pitch;

        Debug.Log($"Played SFX '{soundName}'.");
        return src;
    }

    public void StopSFX(AudioSource sfx)
    {
        // Find the sfx object in the pool
        for(int i = 0; i < sfxPool.Length; i++)
        {
            if(sfxPool[i] == sfx)
            {
                DecommissionSFX(i);
                Debug.Log("Stopped SFX.");
                return;
            }
        }

        // If it's not in the pool it's just some random audio source, so we remove it
        Destroy(sfx.gameObject);
        Debug.Log("Destroyed SFX.");
    }

    void DecommissionSFX(int index)
    {
        // Reset the audio source (kind of, most things don't need to be reset)
        AudioSource src = sfxPool[index];
        src.Stop();
        src.clip = null;
        src.loop = false;

        // Pooling stuff
        sfxInCommission[index] = 0f;
        src.gameObject.name = "pooledSFX" + index;
        src.transform.parent = transform;
        src.gameObject.SetActive(false);
    }

    public SpriteRenderer PlayVFX(string effectName, Vector2 position)
    {
        if (AssetManager.Instance == null)
        {
            Debug.LogWarning("Could not play VFX: AssetManager couldn't be found.");
            return null;
        }
        else if (!AssetManager.Instance.vfxAssets.ContainsKey(effectName))
        {
            Debug.LogWarning($"Could not play VFX: '{effectName}' is not a valid key.");
            return null;
        }

        VFXObject data = AssetManager.Instance.vfxAssets[effectName];

        // Get the sprite/animation
        int sprID = UnityEngine.Random.Range(0, data.sprites.Length + data.anims.Length);
        if (data.lifetime <= 0f && !data.manualRemove)
        {
            if (sprID < data.sprites.Length || data.anims[sprID - data.sprites.Length].isLooping)
                return null;
        }

        #region Pooling

        SpriteRenderer src = null;

        // Pull object from pool
        for (int i = 0; i < vfxPool.Length; i++)
        {
            // Current index
            vfxPoolIndex++;
            if (vfxPoolIndex >= vfxPool.Length)
                vfxPoolIndex = 0;

            // Check if index is available
            if (vfxInCommission[vfxPoolIndex] == 0f)
            {
                src = vfxPool[vfxPoolIndex];
                break;
            }
        }

        // If nothing could be found from the pool we force the next VFX source in line
        if (src == null)
        {
            // Current index
            vfxPoolIndex++;
            if (vfxPoolIndex >= vfxPool.Length)
                vfxPoolIndex = 0;

            // Force reset and get
            DecommissionVFX(vfxPoolIndex);
            src = vfxPool[vfxPoolIndex];
        }

        #endregion

        // Init the VFX
        src.gameObject.SetActive(true);
        src.gameObject.name = $"vfx_{effectName}";
        src.transform.parent = null;
        src.transform.position = position + new Vector2(UnityEngine.Random.Range(data.minOffset.x, data.maxOffset.x), UnityEngine.Random.Range(data.minOffset.y, data.maxOffset.y));

        float angle = Mathf.Deg2Rad * UnityEngine.Random.Range(data.minAngle, data.maxAngle);
        src.transform.right = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        src.sortingLayerName = data.sortingLayer;
        src.sortingOrder = data.orderInLayer;

        // Set the sprite
        if (sprID < data.sprites.Length)
            src.sprite = data.sprites[sprID];
        else
        {
            // Animation
            Animator anim = src.GetComponent<Animator>();

            // Create a "PlayableGraph", which you apparently need if you just want to play a single animation clip.
            PlayableGraph graph = PlayableGraph.Create("PlayAnimationClip");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            AnimationPlayableOutput.Create(graph, "Animation", anim).SetSourcePlayable(AnimationClipPlayable.Create(graph, data.anims[sprID - data.sprites.Length]));
            graph.Play();

            // We need to destroy the graph later on, so we keep it in a dictionary until decommission time
            vfxGraphs.Add(vfxPoolIndex, graph);
        }

        // Physics
        if (data.usePhysics)
        {
            Rigidbody2D rb = src.GetComponent<Rigidbody2D>();
            rb.gravityScale = data.gravity;
            rb.linearVelocity = new Vector2(UnityEngine.Random.Range(data.minVelocity.x, data.maxVelocity.x), UnityEngine.Random.Range(data.minVelocity.y, data.maxVelocity.y));
            rb.angularVelocity = UnityEngine.Random.Range(data.minAngularVelocity, data.maxAngularVelocity);
        }

        // Lifetime
        if (!data.manualRemove)
        {
            if (data.lifetime <= 0f && sprID >= data.sprites.Length)
                vfxInCommission[vfxPoolIndex] = data.anims[sprID - data.sprites.Length].length;
            else
                vfxInCommission[vfxPoolIndex] = data.lifetime;
        }
        else
        {
            vfxInCommission[vfxPoolIndex] = -1f;
        }

        Debug.Log($"Played VFX '{effectName}'.");
        return src;
    }
    
    public void StopVFX(SpriteRenderer vfx)
    {
        // Find the vfx object in the pool
        for (int i = 0; i < vfxPool.Length; i++)
        {
            if (vfxPool[i] == vfx)
            {
                DecommissionVFX(i);
                Debug.Log("Stopped VFX.");
                return;
            }
        }

        // If it's not in the pool it's just some random sprite renderer, so we remove it
        Destroy(vfx.gameObject);
        Debug.Log("Destroyed VFX.");
    }

    void DecommissionVFX(int index)
    {
        // Reset the VFXs components
        SpriteRenderer src = vfxPool[index];
        src.transform.localScale = Vector3.one;
        src.transform.up = Vector3.up;

        if (vfxGraphs.ContainsKey(index))
        {
            vfxGraphs[index].Stop();
            vfxGraphs[index].Destroy();

            vfxGraphs.Remove(index);
        }
        src.sprite = null;

        Rigidbody2D rb = src.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Pooling stuff
        vfxInCommission[index] = 0f;
        src.gameObject.name = "pooledVFX" + index;
        src.transform.parent = transform;
        src.gameObject.SetActive(false);
    }

    void PoolUpdate()
    {
        // SFX
        for(int i = 0; i < sfxInCommission.Length; i++)
        {
            if(sfxInCommission[i] > 0f)
            {
                sfxInCommission[i] -= Time.deltaTime;
                if(sfxInCommission[i] <= 0f)
                {
                    DecommissionSFX(i);
                }
            }
        }

        // VFX
        for (int i = 0; i < vfxInCommission.Length; i++)
        {
            if (vfxInCommission[i] > 0f)
            {
                vfxInCommission[i] -= Time.deltaTime;
                if (vfxInCommission[i] <= 0f)
                {
                    DecommissionVFX(i);
                }
            }
        }
    }

    #endregion

    public void TogglePause()
    {
        if (pause.isPaused)
            pause.ResumeGame();
        else
            pause.PauseGame();
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitLevel()
    {
        Destroy(Camera.main.gameObject);
        Destroy(FindAnyObjectByType<TetherManager>().gameObject);
        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
            Destroy(players[i].gameObject);
        Canvas[] can = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < can.Length; i++)
            Destroy(can[i].gameObject);
        SceneManager.LoadScene(0);
    }
}
