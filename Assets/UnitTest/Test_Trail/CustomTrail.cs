using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTrail : MonoBehaviour
{
    public Material material;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;
    private Vector3 previousPosition;

    [SerializeField] private float trailWidth = 0.2f;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[4];
        triangles = new int[6];
        uv = new Vector2[4];

        previousPosition = transform.position;

        // 초기 UV 설정
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        // 버텍스 설정
        vertices[0] = previousPosition;
        vertices[1] = previousPosition + Vector3.up * trailWidth;
        vertices[2] = currentPosition;
        vertices[3] = currentPosition + Vector3.up * trailWidth;

        // 삼각형 설정
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // 메쉬 업데이트
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        previousPosition = currentPosition;
    }
}
