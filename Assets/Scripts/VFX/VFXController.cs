using UnityEngine;

public class VFXController : MonoBehaviour
{
    [SerializeField] private GameObject prefab;


    public void ControllParticleSystem(bool inverse = false)
    {
        if (prefab == null)
            return;

        if (inverse)
        {
            prefab.transform.localRotation *= Quaternion.Euler(0, 0, 180);
        }

    }

}
