using UnityEngine;
using UnityEngine.UIElements;

public class Test_Rigid : MonoBehaviour
{
    [SerializeField] private ForceModeObj[] testObjs;
    [SerializeField] private Vector3[] transforms;

    [SerializeField] private ForceModeObj animCurve;

    public float power = 1.0f;

    private void Start()
    {
        transforms = new Vector3[testObjs.Length];

        for (int i = 0; i  < testObjs.Length; i++) 
        {
            transforms[i] = testObjs[i].transform.position;
        }
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            foreach(var obj in testObjs)
            {
                obj.DoAddForce(power);
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (var obj in testObjs)
            {
                obj.DoAddForceWithMass(power);
            }
        }


        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (var obj in testObjs)
            {
                obj.DoUpAddForce(power);
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            foreach (var obj in testObjs)
            {
                obj.DoUpAddForceWithMass(power);
            }
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            if(animCurve != null)
                animCurve.LaunchObject();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (animCurve != null)
                animCurve.Launch2Object();
        }


        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                testObjs[i].RestRigid();
                testObjs[i].transform.position = transforms[i];
            }

        }

    }

    
}
