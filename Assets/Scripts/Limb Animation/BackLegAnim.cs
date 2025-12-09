using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BackLegAnim : LimbAnimator
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEffectorAndConstraints();
    }

    public override Vector3 FindConstraintPosition(a3_Constraint constraint)
    {
        switch (constraint.type)
        {
            case ConstraintType.PAW:

                break;
            case ConstraintType.ANKLE:

                break;
            case ConstraintType.KNEE:

                break;
        }

        // fallback case
        return constraint.constraint.transform.position;
    }

    public override Vector3 FindEffectorPosition()
    {
        //RaycastHit hit;
        //LayerMask layerMask = LayerMask.GetMask("Ground");
        //if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask))
        //{
        //    return hit.point;
        //}

        // fallback case
        return effector.transform.position;
    }
}
