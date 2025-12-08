using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontLegAnim : LimbAnimator
{
    [SerializeField] GameObject effector;
    [SerializeField] GameObject constraint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        effector.transform.position = FindEffectorPosition();
    }

    public override Vector3 FindEffectorPosition()
    {
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask))
        {
            return hit.point;
        }

        // fallback case
        return transform.position;
    }

    public override Vector3 FindConstraintPosition(GameObject constraint)
    {


        // fallback case
        return constraint.transform.position;
    }
    
}
