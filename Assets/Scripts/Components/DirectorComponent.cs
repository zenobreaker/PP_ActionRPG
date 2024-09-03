using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorComponent : MonoBehaviour
{
    [SerializeField] private float bEanbleDistance; // ���� ��� �Ÿ� 
    [SerializeField] private float bSightAngle;     // ���� ��� ����
    

    private void Start()
    {
        
    }    


    public bool CheckArroundObstacle()
    {


        return false; 
    }

}
