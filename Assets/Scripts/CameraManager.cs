using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public enum CameraState { Target, Events, None };

    private Camera cam;
    [HideInInspector] public CameraState state = CameraState.Target;

    [Header("Target")]
    public List<Transform> targets = new List<Transform>();
    [SerializeField] private Vector3 targetOffset = new Vector3();
    [SerializeField] private float targetSpeed = 4f;
    private Vector3 truePosition = new Vector3();

    [Space] public Rect bounds = new Rect(-10, 10, 10, -10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // The camera stuff runs here because the game looks really choppy and bad otherwise
        switch (state)
        {
            case CameraState.None:
                break; // Just in case another script moves the camera manually
            case CameraState.Target:
                if (targets.Count <= 0)
                    break;

                // Get the target position
                Vector3 targetPos = new Vector3();
                for (int i = 0; i < targets.Count; i++)
                {
                    targetPos += targets[i].position;
                }
                targetPos = new Vector3(targetPos.x, targetPos.y, 0) / targets.Count + targetOffset;

                // Move the camera towards target position
                truePosition = Vector3.Lerp(truePosition, targetPos, targetSpeed * Time.deltaTime);

                #region Stay in bounds

                // This could all be one line
                float xPos = Mathf.Clamp(truePosition.x, bounds.x + cam.orthographicSize * (16f / 9f), bounds.width - cam.orthographicSize * (16f / 9f));
                float yPos = Mathf.Clamp(truePosition.y, bounds.height + cam.orthographicSize, bounds.y - cam.orthographicSize);
                truePosition = new Vector3(xPos, yPos, targetOffset.z);

                #endregion

                transform.position = truePosition;
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (state == CameraState.Target)
                state = CameraState.None;
            else
                state = CameraState.Target;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3((bounds.x + bounds.width) / 2, (bounds.y + bounds.height) / 2), new Vector3(Mathf.Abs(bounds.width - bounds.x), Mathf.Abs(bounds.height - bounds.y), 0.01f));
    }
}