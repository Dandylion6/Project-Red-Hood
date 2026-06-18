using System;
using UnityEngine;

public class Stunnable : MonoBehaviour
{
    public float StunSecondsLeft => stunSecondsLeft;
    public bool IsStunned => isStunned;

    private Func<bool> canStun = null;
    private float stunSecondsLeft = 0.0f;
    private bool isStunned = false;


    public void SubscribeToCanStun(Func<bool> callback) => canStun += callback;
    public void UnsubscribeToCanStun(Func<bool> callback) => canStun -= callback;


    public void Stun(float stunSeconds = 1.0f)
    {
        stunSecondsLeft = stunSeconds;
        isStunned = true;
    }


    public bool CanStun()
    {
        if (canStun == null) return true;
        return canStun.Invoke();
    }


    private void Update()
    {
        if (isStunned)
            RecoverFromStun();
    }


    private void RecoverFromStun()
    {
        stunSecondsLeft -= Time.deltaTime;
        if (stunSecondsLeft > float.Epsilon) return;

        isStunned = false;
        stunSecondsLeft = 0.0f;
    }
}
