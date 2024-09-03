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

        // SerializedProperty 
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


        //
        if (previousTargetObject != targetObjectProperty.objectReferenceValue)
        {
            Debug.Log("TargetObject has changed!");
            previousTargetObject = targetObjectProperty.objectReferenceValue;

            serializedObject.ApplyModifiedProperties(); //  값 적용 

            ApplyValuesToAnimator();
        }


        if (clips == null || clips.Length == 0)
        {
            EditorGUILayout.HelpBox("Animator가 있는 오브젝트를 고르셔야합니다..", MessageType.Warning);
            return;
        }

        // 
        string[] clipNames = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clipNames[i] = clips[i].name;
        }

        selectedClipIndex = EditorGUILayout.Popup("Select Animation", selectedClipIndex, clipNames);

        // 
        frame = EditorGUILayout.Slider("Frame", frame, 0, clips[selectedClipIndex].length * clips[selectedClipIndex].frameRate);

        if (GUILayout.Button("Play Selected Frame"))
        {
            PlaySelectedFrame();
        }

        // 
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
            // 
            float normalizedTime = frame / (clips[selectedClipIndex].length * clips[selectedClipIndex].frameRate);
            animator.Play(clips[selectedClipIndex].name, 0, normalizedTime);
            animator.Update(0); // Animator 
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
        //
        GameObject newObject = new GameObject(original.name + "_Copy");

        // 
        CopyTransformValues(original.transform, newObject.transform);

        // 
        CopyComponents(original, newObject);

        // 
        foreach (Transform child in original.transform)
        {
            CopyChildTransforms(child, newObject.transform);
        }

        // 
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

        // Hierarchy 
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

    // 
    void CopyComponents(GameObject original, GameObject target)
    {
        // 
        foreach (var component in original.GetComponents<Component>())
        {
            // Component�� �����ϰ� Ÿ�� ������Ʈ�� �߰�
            if (component is Transform) continue; // Transform ������Ʈ�� �������� ����
            if (component is Animator) continue; // Animator ������Ʈ�� �������� ����
            Component newComponent = target.AddComponent(component.GetType());

            // MeshRenderer ����
            if (component is MeshRenderer)
            {
                CopyMeshRenderer(component as MeshRenderer, newComponent as MeshRenderer);
            }
            // MeshFilter ����
            else if (component is MeshFilter)
            {
                CopyMeshFilter(component as MeshFilter, newComponent as MeshFilter);
            }
            else
            {
                // ���� ������Ʈ�� ��� �ʵ� ���� ����
                var fields = component.GetType().GetFields();
                foreach (var field in fields)
                {
                    field.SetValue(newComponent, field.GetValue(component));
                }

                // ���� ������Ʈ�� ��� ������Ƽ ���� ����
                var properties = component.GetType().GetProperties();
                foreach (var property in properties)
                {
                    // Ư�� ������Ƽ�� �״�� �����ϸ� ������ �߻��ϴ� ���� 
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
            }
        }
    }

    private void CopySkinnedMeshRenderer(SkinnedMeshRenderer original, SkinnedMeshRenderer target)
    {
        // Mesh ����
        if (original.sharedMesh != null)
        {
            // ������ȭ�� �����Ƿ� 
            //Mesh newMesh = Instantiate(original.sharedMesh);
            target.sharedMesh = original.sharedMesh;
        }

        // Materials ����
        Material[] originalMaterials = original.sharedMaterials;
        Material[] newMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                //  newMaterials[i] = new Material(originalMaterials[i]);
                // ���� ��Ƽ������ ������ �ʿ� ���� �ٷ� ����
                newMaterials[i] = originalMaterials[i];
            }
        }
        target.sharedMaterials = newMaterials;


        // Bone �迭 ����
        // Bones ����
        Transform[] originalBones = original.bones;
        Transform[] newBones = new Transform[originalBones.Length];
        for (int i = 0; i < originalBones.Length; i++)
        {
            // ������ Bones Transform�� ���纻�� Transform���� ����
            if (originalBones[i] != null)
            {
                // ���� ����� ���� ������Ʈ�� �ڽ����� ���� Transform�� �����ؾ� �մϴ�.
                newBones[i] = target.transform.parent.FindChildByName(originalBones[i].name);
            }
        }
        target.bones = newBones;

        // rootBone ����
        if (original.rootBone != null)
        {

            target.rootBone = target.transform.parent.FindChildByName(original.rootBone.name);
        };
    }


    // �ڽ� Ʈ������ ����
    void CopyChildTransforms(Transform originalChild, Transform newParent)
    {
        GameObject newChild = new GameObject(originalChild.name);

        // ���ο� �ڽ� ������Ʈ�� Ʈ������ ���� ����
        newChild.transform.SetParent(newParent);
        CopyTransformValues(originalChild, newChild.transform);

        // ���� �ڽ��� ������Ʈ ���� 
        CopyComponents(originalChild.gameObject, newChild);

        // ��������� �ڽĵ��� Ʈ�������� ����
        foreach (Transform child in originalChild)
        {
            CopyChildTransforms(child, newChild.transform);
        }
    }

    // Ʈ������ ���� �����ϴ� �Լ�
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
            Material[] originalMaterials = original.sharedMaterials;
            Material[] newMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    // ���� ��Ƽ������ ������ �ʿ� ���� �ٷ� ����
                    //newMaterials[i] = new Material(originalMaterials[i]);
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

