using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Gun_Trigger : MonoBehaviour
{
    GameObject rootObject; 
    public GameObject RootObject { set => rootObject = value; }

    [SerializeField] GameObject mainObject;
    [SerializeField] GameObject muzzlePosObj;
 
}
