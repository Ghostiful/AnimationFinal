using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TailAnim : LimbAnimator
{
    public TailState state;

    [SerializeField] Transform pelvisTransform;
    [SerializeField] float tailWagRadius;
    [SerializeField] Transform leftWagConstraint;
    [SerializeField] Transform rightWagConstraint;
    [SerializeField] float tailWagTime;

    float tailWagTimer;
    int wagDirection; // 1 is right, -1 is left

    // Start is called before the first frame update
    void Start()
    {
        tailWagTimer = 0;
        wagDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
        UpdateEffectorAndConstraints();

    }

    public override Vector3 FindEffectorPosition()
    {
        switch (state)
        {
            case TailState.BASE:
                tailWagTimer += Time.deltaTime * wagDirection;
                if (tailWagTimer > 1)
                {
                    wagDirection *= -1;
                }
                effector.transform.position = Vector3.Lerp(leftWagConstraint.position, rightWagConstraint.position, tailWagTimer);
                break;
            case TailState.FALLING:

                break;
            case TailState.JUMPING:

                break;
            case TailState.WALKING:

                break;
            case TailState.RUNNING:

                break;
        }

        // fallback case
        return transform.position;
    }

    public override Vector3 FindConstraintPosition(a3_Constraint constraint)
    {
        switch (constraint.type)
        {
            case ConstraintType.TAIL_UP:

                break;
            case ConstraintType.TAIL_DOWN:

                break;
            case ConstraintType.TAIL_LEFT:

                break;
            case ConstraintType.TAIL_RIGHT:

                break;
            case ConstraintType.TAIL_IK:

                break;
        }

        // fallback case
        return constraint.constraint.transform.position;
    }

    // Needs to calculate constraints first so tail wags correctly
    public override void UpdateEffectorAndConstraints()
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            constraints[i].constraint.transform.position = FindConstraintPosition(constraints[i]);
        }
        effector.transform.position = FindEffectorPosition();
    }
}

public enum TailState
{
    BASE,
    FALLING,
    JUMPING,
    RUNNING,
    WALKING
}
