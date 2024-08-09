using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshTrail : MonoBehaviour
{
    #region Shader Property 
    [SerializeField] private float activeTime = 2.0f;

    [Header("Mesh Related")]
    [SerializeField] private float meshRefreshRate = 0.1f;
    [SerializeField] private Transform positionToSpawn;
    [SerializeField] private float destroyDelayTime = 0.1f;

    [Header("Shader Related")]
    [SerializeField] private Material material;         // ������ ���׸��� 
    [SerializeField] private string shaderVarRef;       // ���̴��� ������ ���� �̸� 
    [SerializeField] private float shaderVarRate = 0.1f; // ���̴� ���� �ֱ� 
    [SerializeField] private float shaderVarRefreshRate = 0.05f; // ���̴� ���� �ֱ� 
    [SerializeField] private float shaderScaleRate = 1.0f; 


    private bool bTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    #endregion

    #region Component
    private Player player;

    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
        Debug.Assert(player != null, "No Player");

        player.OnEvadeState += StartActiveTrail;
    }

    public void StartActiveTrail()
    {
        if (bTrailActive == true)
            return;

        bTrailActive = true; 

        StartCoroutine(Start_MeshTrail(activeTime));

    }


    private IEnumerator Start_MeshTrail(float activeTime)
    {
        while (activeTime > 0)
        {
            activeTime -= meshRefreshRate;


            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                // ������ ���ӿ�����Ʈ ���� 
                GameObject obj = new GameObject();
                // ������ ������Ʈ�� ��ġ�� ȸ�� ���� positionToSpawn�� �ִ� ������ ó��. 
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);
                obj.transform.localScale = new Vector3(shaderScaleRate, shaderScaleRate, shaderScaleRate);

                // ���� ���� �޽� ������ �����´�.
                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                // Mesh Ŭ������ ���� �� 
                Mesh mesh = new Mesh();
                // skinnedMeshRenderers����Ʈ�� �߰��Ѵ�. 
                skinnedMeshRenderers[i].BakeMesh(mesh);

                // �޽� ���Ϳ� ��� ������ �޽� ������ �����Ѵ�. 
                mf.mesh = mesh;
                mr.material = this.material;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                // ���� �ð��� ������ �ش� ������Ʈ ���� 
                //TODO: ������Ʈ Ǯ������ ó���ϱ� 
                Destroy(obj, destroyDelayTime);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        bTrailActive = false;
    }

    // ������ �̸����� ���׸����� �Ӽ��� �����ؼ� goal ������ �����ϴ� �ڷ�ƾ 
    private IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }

    }
}
