using System;
using UnityEngine;

public class AimIK : MonoBehaviour
{
    public Transform aimTarget;
    public float bodyWeight = 0.2f;
    public float headWeight = 1f;
    public float eyesWeight = 1f;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();

    }
    

    void OnAnimatorIK(int layerIndex)
    {
        if (!aimTarget) return;

        anim.SetLookAtWeight(
            1f,
            bodyWeight,
            headWeight,
            eyesWeight,
            0.5f
        );

        anim.SetLookAtPosition(aimTarget.position);
    }
}