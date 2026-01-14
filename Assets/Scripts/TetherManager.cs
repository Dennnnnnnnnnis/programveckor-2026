using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TetherManager : MonoBehaviour
{
    public List<GameObject> connections = new List<GameObject>();
    private LineRenderer line;

    [SerializeField] private float tetherLength = 4f;
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private float forcePoweredBy = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (connections.Count < 2)
            return;

        // Keep connections close
        for (int i = 0; i < connections.Count - 1; i++)
        {
            GameObject con1 = connections[i];
            GameObject con2 = connections[i + 1];

            // Check if we even should pull them together
            if(Vector3.Magnitude(con1.transform.position - con2.transform.position) > tetherLength)
            {
                float pullbackForce = Mathf.Pow((Vector3.Magnitude(con1.transform.position - con2.transform.position) - tetherLength), forcePoweredBy) * forceMultiplier;
                float weightDistribution = 0.5f; // This is how much connection 1 is affected by the force in % (connection 2 is the remaining %)

                // Get the things and see weight distribution
                Collision col1, col2;
                if (!con1.TryGetComponent<Collision>(out col1))
                    weightDistribution = 0f;

                if (con2.TryGetComponent<Collision>(out col2))
                {
                    if(col1 != null)
                    {
                        // If both have the collision component they can handle weight distribution
                        float totalWeight = col1.weight + col2.weight;
                        if(totalWeight > 0f)
                            weightDistribution = col2.weight / totalWeight;
                    }
                }
                else
                    weightDistribution = 1f;

                print(weightDistribution);

                // Apply the forces and stuff
                if (col1 != null)
                    col1.Velocity -= weightDistribution * pullbackForce * Vector2.Normalize(con1.transform.position - con2.transform.position);
                if(col2 != null)
                    col2.Velocity += (1f - weightDistribution) * pullbackForce * Vector2.Normalize(con1.transform.position - con2.transform.position);
            }
        }
    }

    void Update()
    {
        // Update visually
        line.positionCount = connections.Count;
        for(int i = 0; i < connections.Count; i++)
        {
            line.SetPosition(i, connections[i].transform.position);
        }
    }

    public bool IsWithinBounds(int ind)
    {
        // Find if the connection is part of the tether and within bounds (using a long return statement)
        if(ind >= 0 && ind < connections.Count)
            return (ind == 0 || Vector3.Magnitude(connections[ind - 1].transform.position - connections[ind].transform.position) <= tetherLength) && (ind == connections.Count - 1 || Vector3.Magnitude(connections[ind].transform.position - connections[ind + 1].transform.position) <= tetherLength);

        // The object isn't part of the tether (default to returning true)
        return true;
    }

    public bool IsWithinBoundsX(int ind)
    {
        // Find if the connection is part of the tether and within bounds (using a long return statement)
        if (ind >= 0 && ind < connections.Count)
            return (ind == 0 || Mathf.Abs(connections[ind - 1].transform.position.x - connections[ind].transform.position.x) <= tetherLength) && (ind == connections.Count - 1 || Mathf.Abs(connections[ind].transform.position.x - connections[ind + 1].transform.position.x) <= tetherLength);

        // The object isn't part of the tether (default to returning true)
        return true;
    }

    public int FindObjectConnectionIndex(GameObject obj)
    {
        // Find the object in the connection list
        for (int i = 0; i < connections.Count; i++)
        {
            if (obj == connections[i])
                return i;
        }

        return -1;
    }
}
