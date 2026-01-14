using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    void Update()
    {
        Vector3 targetPosition = player.transform.position + new Vector3(0, 0, -10);
        transform.position = targetPosition;
    }
}
