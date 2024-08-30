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
    [SerializeField] private Material material;         // 생성할 머테리얼 
    [SerializeField] private string shaderVarRef;       // 셰이더의 참조할 변수 이름 
    [SerializeField] private float shaderVarRate = 0.1f; // 셰이더 생성 주기 
    [SerializeField] private float shaderVarRefreshRate = 0.05f; // 셰이더 갱신 주기 
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
                // 별도의 게임오브젝트 생성 
                GameObject obj = new GameObject();
                // 생성한 오브젝트의 위치와 회전 값을 positionToSpawn에 있는 값으로 처리. 
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);
                obj.transform.localScale = new Vector3(shaderScaleRate, shaderScaleRate, shaderScaleRate);

                // 현재 모델의 메쉬 정보를 가져온다.
                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                // Mesh 클래스를 생성 후 
                Mesh mesh = new Mesh();
                // mesh에 skinnedMeshRenderers에 mesh를 그대로 생성한다.
                skinnedMeshRenderers[i].BakeMesh(mesh);

                // 메쉬 필터에 방금 생성한 메쉬 정보를 대입한다. 
                mf.mesh = mesh;
                mr.material = this.material;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                obj.transform.SetParent(parent.transform);
            }

            // 일정 시간이 지나면 해당 오브젝트 삭제 
            //TODO: 오브젝트 풀링으로 처리하기 
            Destroy(parent, destroyDelayTime);

            yield return new WaitForSeconds(meshRefreshRate);
        }

        bTrailActive = false;
        ReturnOriginData();
    }

    // 참조된 이름으로 머테리얼의 속성에 접근해서 goal 값으로 변경하는 코루틴 
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
