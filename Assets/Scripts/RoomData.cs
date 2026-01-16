using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector2 playerStartPos;
    public Rect roomBounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = playerStartPos;
        }

        Camera.main.GetComponent<CameraManager>().bounds = roomBounds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3((roomBounds.x + roomBounds.width) / 2, (roomBounds.y + roomBounds.height) / 2), new Vector3(Mathf.Abs(roomBounds.width - roomBounds.x), Mathf.Abs(roomBounds.height - roomBounds.y), 0.01f));
    }
}
