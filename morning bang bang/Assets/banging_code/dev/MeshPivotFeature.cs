using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPivotFeature : MonoBehaviour
{
    private MeshFilter filter;
    private Bounds meshBounds;
    [SerializeField] private Vector3 meshBoundsCenter;

    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponentInChildren<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        meshBounds = filter.mesh.bounds;
        meshBounds.center = meshBoundsCenter;
        filter.mesh.bounds = meshBounds;
    }
}
