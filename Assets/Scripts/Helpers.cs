using System.Collections.Generic;
using UnityEngine;

public static class Extend_TransformHelpers
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] trasnforms = transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in trasnforms)
        {
            if (t.gameObject.name.Equals(name))
                return t;
        }

        return null;
    }

    public static GameObject[] FindChildrenByComponentType<T>(this Transform transform) where T : Component
    {
        T[] trasnforms = transform.GetComponentsInChildren<T>();

        List<GameObject> gameObjects = new List<GameObject>(); 

        foreach(T t in trasnforms)
        {
            gameObjects.Add(t.gameObject);
        }

        return gameObjects.ToArray();
    }
}

public static class Extend_Vector3
{
    public static float GetAngle(Vector3 Start, Vector3 End)
    {
        Vector3 direction = End - Start;

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

}



public static class UIHelpers
{
    public static Canvas CreateBillboardCanvas(string resourceName, Transform transform, Camera camera)
    {
        GameObject prefab = Resources.Load<GameObject>(resourceName);
        GameObject obj = GameObject.Instantiate<GameObject>(prefab, transform);

        Canvas canvas = obj.GetComponent<Canvas>();
        canvas.worldCamera = camera;

        return canvas;
    }

}


public static class CameraHelpers
{
    public static bool GetCursorLocation(out Vector3 position, float distance, LayerMask mask)
    {

        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);

    }

    public static bool GetCursorLocation(float distance, LayerMask mask)
    {
        Vector3 position;
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    public static bool GetCursorLocation(out Vector3 position, out Vector3 normal, float distance, LayerMask mask)
    {
        position = Vector3.zero;
        normal = Vector3.zero;

        //Input.mousePosition ���ǻ��� => ��ǲ�ý��ۿ��� �ȸ���.. ��ǲ�ý��ۿ� �����դ���??
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            position = hit.point;
            normal = hit.normal;

            return true;
        }

        return false;
    }
}


public static class MathHelpers
{
    public static bool IsNearlyEqual(float a, float b, float tolarmace = 1e-6f)
    {
        return Mathf.Abs(a - b) <= tolarmace;
    }

    public static bool IsNearlyZero(float a, float b, float tolarmace = 1e-6f)
    {
        return Mathf.Abs(a) <= tolarmace;
    }

}

public static class BTNodeFactory
{
    public static SelectorNode CreateSelectorNode(params BTNode[]  children)
    {
        SelectorNode selectorNode = new SelectorNode(new System.Collections.Generic.List<BTNode>(children));
        return selectorNode;
    }

    public static SequenceNode CreateSequenceNode(params BTNode[] children)
    {
        SequenceNode sequenceNode= new SequenceNode(new System.Collections.Generic.List<BTNode>(children));
        return sequenceNode;
    }
}