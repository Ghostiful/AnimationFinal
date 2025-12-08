using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TailAnim : LimbAnimator
{
    public TailState state;

    [SerializeField] Transform pelvisTransform;
    [SerializeField] Transform leftWagConstraint;
    [SerializeField] Transform rightWagConstraint;
    [SerializeField] Transform tailFallConstraint;
    [SerializeField] Transform tailJumpConstraint;

    float tailWagTimer;
    int wagDirection; // 1 is right, -1 is left
    float fallTimer;
    float jumpTimer;
    Vector3 previousEffectorPos;

    // Start is called before the first frame update
    void Start()
    {
        tailWagTimer = 0;
        wagDirection = 1;
        fallTimer = 0;
        jumpTimer = 0;
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
                if (tailWagTimer > 1 || tailWagTimer < 0)
                {
                    wagDirection *= -1;
                }
                tailWagTimer += Time.deltaTime * wagDirection;
                
                return Vector3.Lerp(leftWagConstraint.position, rightWagConstraint.position, tailWagTimer);
            case TailState.FALLING:
                fallTimer += Time.deltaTime;
                if (fallTimer > 1)
                    fallTimer = 1;
                return Vector3.Lerp(previousEffectorPos, tailFallConstraint.transform.position, fallTimer);
            case TailState.JUMPING:
                jumpTimer += Time.deltaTime;
                if (jumpTimer > 1)
                    jumpTimer = 1;
                return Vector3.Lerp(previousEffectorPos, tailJumpConstraint.transform.position, jumpTimer);
            case TailState.WALKING:

                break;
            case TailState.RUNNING:

                break;
            default:
                return transform.position;
                

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
            default:
                return transform.position;
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

    public void SwitchTailState(TailState tailState)
    {
        state = tailState;
        previousEffectorPos = effector.transform.position;
        switch (state)
        {
            case TailState.FALLING:
                fallTimer = 0;
                break;
            case TailState.JUMPING:
                jumpTimer = 0;
                break;
            
        }
    }
}

public enum TailState
{
    BASE,
    FALLING,
    JUMPING,
    RUNNING,
    WALKING,
    TRANSITIONING
}
