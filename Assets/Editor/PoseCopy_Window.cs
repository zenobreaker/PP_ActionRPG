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

        string[] clipNames = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clipNames[i] = clips[i].name;
        }

        selectedClipIndex = EditorGUILayout.Popup("Select Animation", selectedClipIndex, clipNames);

        frame = EditorGUILayout.Slider("Frame", frame, 0, clips[selectedClipIndex].length * clips[selectedClipIndex].frameRate);

        if (GUILayout.Button("Play Selected Frame"))
        {
            PlaySelectedFrame();
        }

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
        GameObject newObject = new GameObject(original.name + "_Copy");

        CopyTransformValues(original.transform, newObject.transform);

        CopyComponents(original, newObject);

        foreach (Transform child in original.transform)
        {
            CopyChildTransforms(child, newObject.transform);
        }

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
            if (component is Transform) continue; 
            if (component is Animator) continue; 
            Component newComponent = target.AddComponent(component.GetType());

            if (component is MeshRenderer)
            {
                CopyMeshRenderer(component as MeshRenderer, newComponent as MeshRenderer);
            }
            else if (component is MeshFilter)
            {
                CopyMeshFilter(component as MeshFilter, newComponent as MeshFilter);
            }
            else
            {
                var fields = component.GetType().GetFields();
                foreach (var field in fields)
                {
                    field.SetValue(newComponent, field.GetValue(component));
                }

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
            }
        }
    }

    private void CopySkinnedMeshRenderer(SkinnedMeshRenderer original, SkinnedMeshRenderer target)
    {
        if (original.sharedMesh != null)
        {
            //Mesh newMesh = Instantiate(original.sharedMesh);
            target.sharedMesh = original.sharedMesh;
        }

        Material[] originalMaterials = original.sharedMaterials;
        Material[] newMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                newMaterials[i] = originalMaterials[i];
            }
        }
        target.sharedMaterials = newMaterials;


        Transform[] originalBones = original.bones;
        Transform[] newBones = new Transform[originalBones.Length];
        for (int i = 0; i < originalBones.Length; i++)
        {
            if (originalBones[i] != null)
            {
                newBones[i] = target.transform.parent.FindChildByName(originalBones[i].name);
            }
        }
        target.bones = newBones;

        if (original.rootBone != null)
        {

            target.rootBone = target.transform.parent.FindChildByName(original.rootBone.name);
        };
    }


    void CopyChildTransforms(Transform originalChild, Transform newParent)
    {
        GameObject newChild = new GameObject(originalChild.name);

        newChild.transform.SetParent(newParent);
        CopyTransformValues(originalChild, newChild.transform);

        CopyComponents(originalChild.gameObject, newChild);

        foreach (Transform child in originalChild)
        {
            CopyChildTransforms(child, newChild.transform);
        }
    }

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

