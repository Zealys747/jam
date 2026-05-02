using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlackHole : MonoBehaviour
{
    
    public float pullForce = 15f;      
    public float pullRadius = 1f;    

    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            Vector3 toCenter = transform.position - rb.position;
            float distance = toCenter.magnitude;

            float strength = (1f - Mathf.Clamp01(distance / pullRadius)) * pullForce;

            rb.AddForce(toCenter.normalized * strength, ForceMode.Acceleration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}