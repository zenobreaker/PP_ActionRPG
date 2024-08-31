using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorComponent : MonoBehaviour
{
    [SerializeField] private float bEanbleDistance; // 연출 허용 거리 
    [SerializeField] private float bSightAngle;     // 연출 허용 각도
    

    private void Start()
    {
        
    }    


    public bool CheckArroundObstacle()
    {


        return false; 
    }

}
