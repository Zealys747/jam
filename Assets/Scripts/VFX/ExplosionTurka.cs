using System.Collections;
using CartoonFX;
using UnityEngine;

public class ExplosionTurka : MonoBehaviour
{
    private Rigidbody rb;
    public CFXR_Effect explosionEffect;
    public float force;
    private bool isExploded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor") && !isExploded)
        {
            isExploded = true;

            explosionEffect.transform.SetParent(null);
            explosionEffect.gameObject.SetActive(true);

            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),   // X: случайное направление влево/вправо
                Random.Range(0.5f, 1f),  // Y: немного вверх (чтобы подпрыгнуло)
                Random.Range(-1f, 1f)    // Z: случайное направление вперёд/назад
            ).normalized;  // Нормализуем, чтобы сила была одинаковой по модулю

            rb.AddForce(randomDirection * force, ForceMode.Impulse);

            StartCoroutine(EndGame(2f));
        }
    }

    private IEnumerator EndGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        LevelManager.Instance.LoadScene(0);
    }
}
