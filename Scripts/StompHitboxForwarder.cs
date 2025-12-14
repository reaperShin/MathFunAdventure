using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StompHitboxForwarder : MonoBehaviour
{
    public Stompable owner;

    void OnTriggerEnter(Collider other)
    {
        if (owner != null)
        {
            owner.ProcessTrigger(other);
        }
    }
}
