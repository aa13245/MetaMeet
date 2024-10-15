using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase : MonoBehaviour
{
    public float moveSpeed = 5;
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float jumpPower = 3;
    protected float gravity = -9.81f;
    protected int jumpMaxCount = 1;
    protected int jumpCurrCount;

    public float rotSpeed = 250;
    public bool useRotX;
    //public bool useRotY;

    public float interactionDistance = 3f;





    void Start()
    {
        
    }

}
