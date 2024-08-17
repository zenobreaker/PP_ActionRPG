using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.UI.Image;


internal class PoseCopy_Window : EditorWindow
{
    private static PoseCopy_Window window;
    private static Vector2 windowSize = new Vector2(319, 500);

    [MenuItem("Window/PoseCopy/Pose %&F3", priority = 1)]
    private static void OpenWindow()
    {
        if (window == null)
            window = EditorWindow.CreateInstance<PoseCopy_Window>();

        window.titleContent = new GUIContent("Create Pose Copy");
        window.minSize = windowSize;
        window.maxSize = windowSize;


        window.ShowUtility();
    }

    private PoseData poseData;

    private SerializedObject serializedObject;
    private SerializedProperty targetObjectProperty;
    private Object previousTargetObject;

    private Animator animator;
    private AnimationClip[] clips;
    private int selectedClipIndex = 0;
    private float frame = 0;

    private void OnEnable()
    {
        poseData = CreateInstance<PoseData>();
        serializedObject = new SerializedObject(poseData);

        // SerializedProperty 가져오기
        targetObjectProperty = serializedObject.FindProperty("TargetObject");
        previousTargetObject = targetObjectProperty.objectReferenceValue;

        Selection.activeGameObject = null;
    }

    private void OnGUI()
    {
        serializedObject.Update();

        // GameObject
        {
            SerializedProperty property = serializedObject.FindProperty("TargetObject");
            EditorGUILayout.PropertyField(property);
        }


        // 값이 변경되었는지 체크
        if (previousTargetObject != targetObjectProperty.objectReferenceValue)
        {
            Debug.Log("TargetObject has changed!");
            previousTargetObject = targetObjectProperty.objectReferenceValue;

            serializedObject.ApplyModifiedProperties(); // 수정 사항을 반영

            ApplyValuesToAnimator();
        }


        if (clips == null || clips.Length == 0)
        {
            EditorGUILayout.HelpBox("Animator에 애니메이션 클립이 없습니다.", MessageType.Warning);
            return;
        }

        // 애니메이션 클립 선택 드롭다운
        string[] clipNames = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clipNames[i] = clips[i].name;
        }

        selectedClipIndex = EditorGUILayout.Popup("Select Animation", selectedClipIndex, clipNames);

        // 프레임 선택 슬라이더
        frame = EditorGUILayout.Slider("Frame", frame, 0, clips[selectedClipIndex].length * clips[selectedClipIndex].frameRate);

        if (GUILayout.Button("Play Selected Frame"))
        {
            PlaySelectedFrame();
        }

        // 복사버튼 
        {
            if (GUILayout.Button("Copy and Create New"))
            {
                if (poseData.TargetObject != null)
                    CopyTransformWithChildren(poseData.TargetObject);
                else
                    Debug.LogWarning("No Object Selected");
            }
        }
    }

    private void PlaySelectedFrame()
    {
        if (animator != null && clips != null && clips.Length > 0)
        {
            // 특정 프레임으로 이동하여 애니메이션 재생
            float normalizedTime = frame / (clips[selectedClipIndex].length * clips[selectedClipIndex].frameRate);
            animator.Play(clips[selectedClipIndex].name, 0, normalizedTime);
            animator.Update(0); // 강제로 Animator 상태를 업데이트하여 프레임을 즉시 반영
        }
    }


    private void ApplyValuesToAnimator()
    {
        if (poseData.TargetObject == null)
            return;

        animator = poseData.TargetObject.GetComponent<Animator>();
        if (animator != null)
        {
            clips = animator.runtimeAnimatorController.animationClips;
            Debug.Log($"Updated Animator Parameter: to Value: ");
        }
        else
        {
            Debug.LogWarning("No Animator component found on the target object.");
        }
    }

    private void CopyTransformWithChildren(GameObject original)
    {
        // 원본 오브젝트 복사
        GameObject newObject = new GameObject(original.name + "_Copy");

        // 원본의 트랜스폼 값을 복사
        CopyTransformValues(original.transform, newObject.transform);

        // 컴포넌트 복사 
        CopyComponents(original, newObject);

        // 하위 오브젝트 복사
        foreach (Transform child in original.transform)
        {
            CopyChildTransforms(child, newObject.transform);
        }

        // 스킨드메시처리 
        foreach (Transform child in newObject.transform)
        {
            if (child == null)
                continue;

            foreach (var component in child.GetComponents<Component>())
            {
                if (component is not SkinnedMeshRenderer)
                    continue;

                CopyValueSkinnedMeshRenderer(original, (SkinnedMeshRenderer)component);
            }
        }

        // Hierarchy 창에서 새 오브젝트를 선택된 상태로 설정
        Selection.activeGameObject = newObject;

        Debug.Log("Created a new GameObject with children: " + newObject.name);
    }

    void CopyValueSkinnedMeshRenderer(GameObject original, SkinnedMeshRenderer target)
    {
        SkinnedMeshRenderer originSkinned = null;
        Transform originTr = original.transform.FindChildByName(target.transform.name);

        var origin = originTr.GetComponent<SkinnedMeshRenderer>();
        if (origin != null)
        {
            originSkinned = origin;
            CopySkinnedMeshRenderer(originSkinned, target);
        }

    }

    // 컴포넌트 복사
    void CopyComponents(GameObject original, GameObject target)
    {
        // 원본 오브젝트의 모든 컴포넌트를 순회
        foreach (var component in original.GetComponents<Component>())
        {
            // Component를 복사하고 타겟 오브젝트에 추가
            if (component is Transform) continue; // Transform 컴포넌트는 복사하지 않음
            if (component is Animator) continue; // Animator 컴포넌트는 복사하지 않음
            Component newComponent = target.AddComponent(component.GetType());

            // MeshRenderer 복사
            if (component is MeshRenderer)
            {
                CopyMeshRenderer(component as MeshRenderer, newComponent as MeshRenderer);
            }
            // MeshFilter 복사
            else if (component is MeshFilter)
            {
                CopyMeshFilter(component as MeshFilter, newComponent as MeshFilter);
            }
            else
            {
                // 원본 컴포넌트의 모든 필드 값을 복사
                var fields = component.GetType().GetFields();
                foreach (var field in fields)
                {
                    field.SetValue(newComponent, field.GetValue(component));
                }

                // 원본 컴포넌트의 모든 프로퍼티 값을 복사
                var properties = component.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.CanWrite &&
                        property.PropertyType != typeof(Material) &&
                        property.PropertyType != typeof(Material[]) &&
                        property.PropertyType != typeof(Mesh) &&
                        property.PropertyType != typeof(Shader))
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(newComponent, property.GetValue(component));
                        }
                    }
                }

                // SkinnedMeshRenderer 사용하는 컴포넌트의 처리
                if (component is SkinnedMeshRenderer renderer)
                {
                    SkinnedMeshRenderer newRenderer = (SkinnedMeshRenderer)newComponent;
                    CopySkinnedMeshRenderer(renderer, newRenderer);
                }
            }
        }
    }

    private void CopySkinnedMeshRenderer(SkinnedMeshRenderer original, SkinnedMeshRenderer target)
    {
        // Mesh 복사
        if (original.sharedMesh != null)
        {
            // 프리팹화시 빠지므로 
            //Mesh newMesh = Instantiate(original.sharedMesh);
            target.sharedMesh = original.sharedMesh;
        }

        // Materials 복사
        //Material[] originalMaterials = original.sharedMaterials;
        //Material[] newMaterials = new Material[originalMaterials.Length];
        //for (int i = 0; i < originalMaterials.Length; i++)
        //{
        //    if (originalMaterials[i] != null)
        //    {
        //        newMaterials[i] = new Material(originalMaterials[i]);
        //    }
        //}
        //target.sharedMaterials = newMaterials;
        Material[] originalMaterials = original.sharedMaterials;
        Material[] newMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                // 공유 머티리얼을 복사할 필요 없이 바로 설정
                newMaterials[i] = originalMaterials[i];
            }
        }
        target.sharedMaterials = newMaterials;


        // Bone 배열 복사
        // Bones 복사
        Transform[] originalBones = original.bones;
        Transform[] newBones = new Transform[originalBones.Length];
        for (int i = 0; i < originalBones.Length; i++)
        {
            // 원본의 Bones Transform을 복사본의 Transform으로 매핑
            if (originalBones[i] != null)
            {
                // 새로 복사된 게임 오브젝트의 자식으로 원본 Transform이 존재해야 합니다.
                newBones[i] = target.transform.parent.FindChildByName(originalBones[i].name);
            }
        }
        target.bones = newBones;

        // rootBone 설정
        if (original.rootBone != null)
        {

            target.rootBone = target.transform.parent.FindChildByName(original.rootBone.name);
        };
    }


    // 자식 트랜스폼 복사
    void CopyChildTransforms(Transform originalChild, Transform newParent)
    {
        GameObject newChild = new GameObject(originalChild.name);

        // 새로운 자식 오브젝트의 트랜스폼 값을 설정
        newChild.transform.SetParent(newParent);
        CopyTransformValues(originalChild, newChild.transform);

        // 원본 자식의 컴포넌트 복사 
        CopyComponents(originalChild.gameObject, newChild);

        // 재귀적으로 자식들의 트랜스폼도 복사
        foreach (Transform child in originalChild)
        {
            CopyChildTransforms(child, newChild.transform);
        }
    }

    // 트랜스폼 값을 복사하는 함수
    void CopyTransformValues(Transform source, Transform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.localScale = source.localScale;
    }

    private void CopyMeshRenderer(MeshRenderer original, MeshRenderer target)
    {
        if (original.sharedMaterials != null)
        {
            //Material[] originalMaterials = original.sharedMaterials;
            //Material[] newMaterials = new Material[originalMaterials.Length];
            //for (int i = 0; i < originalMaterials.Length; i++)
            //{
            //    if (originalMaterials[i] != null)
            //    {
            //        newMaterials[i] = new Material(originalMaterials[i]);
            //    }
            //}
            //target.sharedMaterials = newMaterials;

            Material[] originalMaterials = original.sharedMaterials;
            Material[] newMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    // 공유 머티리얼을 복사할 필요 없이 바로 설정
                    newMaterials[i] = originalMaterials[i];
                }
            }
            target.sharedMaterials = newMaterials;
        }
    }

    private void CopyMeshFilter(MeshFilter original, MeshFilter target)
    {
        if (original.sharedMesh != null)
        {
            //Mesh newMesh = Instantiate(original.sharedMesh);
            target.sharedMesh = original.sharedMesh;
        }
    }

}

