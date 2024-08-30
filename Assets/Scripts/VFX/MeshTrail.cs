using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private float originRate;
    private Material originMat;

    private void Awake()
    {
        originRate = meshRefreshRate;
        originMat = material;

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


    public void StartActiveTrail(float time)
    {
        if (bTrailActive == true)
            return;

        bTrailActive = true;

        StartCoroutine(Start_MeshTrail(time));
    }


    public void StartActiveTrail(float time, float rate)
    {
        originRate = meshRefreshRate;
        meshRefreshRate = rate;

        StartActiveTrail(time);
    }

    public void StartActiveTrail(float time, float rate, Material material , bool bForce = false)
    {
        if (material != null)
        {
            originMat = this.material;
            this.material = material;
        }
        if (bForce == true)
            bTrailActive = false; 

        StartActiveTrail(time, rate);
    }

    

    private void ReturnOriginData()
    {
        meshRefreshRate = originRate;
        material = originMat;
    }

    private IEnumerator Start_MeshTrail(float activeTime)
    {
        while (activeTime > 0)
        {
            activeTime -= meshRefreshRate;


            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            GameObject parent = new GameObject();

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
                // mesh�� skinnedMeshRenderers�� mesh�� �״�� �����Ѵ�.
                skinnedMeshRenderers[i].BakeMesh(mesh);

                // �޽� ���Ϳ� ��� ������ �޽� ������ �����Ѵ�. 
                mf.mesh = mesh;
                mr.material = this.material;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                obj.transform.SetParent(parent.transform);
            }

            // ���� �ð��� ������ �ش� ������Ʈ ���� 
            //TODO: ������Ʈ Ǯ������ ó���ϱ� 
            Destroy(parent, destroyDelayTime);

            yield return new WaitForSeconds(meshRefreshRate);
        }

        bTrailActive = false;
        ReturnOriginData();
    }

    // ������ �̸����� ���׸����� �Ӽ��� �����ؼ� goal ������ �����ϴ� �ڷ�ƾ 
    private IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        float elapsedTime = 0.0f; 
        float duration = refreshRate; 
        while (valueToAnimate > goal)
        {
            //valueToAnimate -= rate;
            valueToAnimate = Mathf.Lerp(valueToAnimate, goal, elapsedTime / duration);
            mat.SetFloat(shaderVarRef, valueToAnimate);

            elapsedTime += Time.deltaTime * rate;
            //yield return new WaitForSeconds(refreshRate);
            yield return null;
        }

    }

 
}
