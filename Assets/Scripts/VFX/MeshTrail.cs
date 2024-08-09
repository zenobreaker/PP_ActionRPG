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
                // skinnedMeshRenderers리스트에 추가한다. 
                skinnedMeshRenderers[i].BakeMesh(mesh);

                // 메쉬 필터에 방금 생성한 메쉬 정보를 대입한다. 
                mf.mesh = mesh;
                mr.material = this.material;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                // 일정 시간이 지나면 해당 오브젝트 삭제 
                //TODO: 오브젝트 풀링으로 처리하기 
                Destroy(obj, destroyDelayTime);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        bTrailActive = false;
    }

    // 참조된 이름으로 머테리얼의 속성에 접근해서 goal 값으로 변경하는 코루틴 
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
