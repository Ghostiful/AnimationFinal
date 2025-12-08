using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LimbAnimator : MonoBehaviour 
{
    public abstract Vector3 FindEffectorPosition();
    public abstract Vector3 FindConstraintPosition(GameObject constraint);
}
