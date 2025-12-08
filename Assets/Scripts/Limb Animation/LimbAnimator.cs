using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LimbAnimator : MonoBehaviour 
{
    public GameObject effector;
    public a3_Constraint[] constraints;

    public abstract Vector3 FindEffectorPosition();
    public abstract Vector3 FindConstraintPosition(a3_Constraint constraint);

    public virtual void UpdateEffectorAndConstraints()
    {
        effector.transform.position = FindEffectorPosition();
        for (int i = 0; i < constraints.Length; i++)
        {
            constraints[i].constraint.transform.position = FindConstraintPosition(constraints[i]);
        }
    }
}

[System.Serializable]
public struct a3_Constraint
{
    public GameObject constraint;
    public ConstraintType type;
}

// Cats have a lot of joints
public enum ConstraintType
{
    ANKLE,
    KNEE,
    PAW,
    WRIST,
    ELBOW,
    TAIL_UP,
    TAIL_DOWN,
    TAIL_LEFT,
    TAIL_RIGHT,
    TAIL_IK
}
