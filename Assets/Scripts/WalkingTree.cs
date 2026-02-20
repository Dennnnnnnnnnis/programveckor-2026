using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WalkingTree : MonoBehaviour
{
    private Animator anim;

    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float walkSpeed = 2f;
    private float timer, timerAgain;

    bool walk = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        anim = GetComponent<Animator>();
        timer = UnityEngine.Random.Range(3f, 20f);
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("run", walk);
    }

    void FixedUpdate()
    {
        // Timer stuff
        timer -= Time.fixedDeltaTime;
        if(timer <= 0f)
        {
            walk = !walk;
            if(walk)
                timer = UnityEngine.Random.Range(0.5f, 2.5f);
            else
                timer = UnityEngine.Random.Range(3f, 20f);

            timerAgain = 0.17f;
        }

        // This is for the little get-up animation the tree does, we need to wait until the walking animation
        if (timerAgain > 0f)
            timerAgain -= Time.fixedDeltaTime;

        // Do walking
        if (walk && timerAgain <= 0f)
        {
            bool hasTurned = false;

            // Check for wall
            if(Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.right * transform.localScale.x, 2f, whatIsGround))
            {
                transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
                hasTurned = true;
            }

            // Check for edge
            if (!Physics2D.Raycast(transform.position + Vector3.up * 0.1f + Vector3.right * transform.localScale.x * 2f, Vector2.down, 0.2f, whatIsGround))
            {
                if (hasTurned)
                    return; // To at least try and avoid the tree walking weirdly, if both checks come back true then it's probably stuck
                else
                    transform.localScale = new Vector3(-transform.localScale.x, 1f, 1f);
            }

            // Walk
            transform.position += Vector3.right * transform.localScale.x * walkSpeed * Time.fixedDeltaTime;
        }
    }
}
