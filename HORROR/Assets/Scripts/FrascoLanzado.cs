using UnityEngine;

// Va en el PREFAB del frasco que se lanza (no en el que se recoge del piso).
// Necesita un Rigidbody y un Collider que NO sea Trigger.
// Al chocar con cualquier cosa se rompe, y los fantasmas cercanos quedan aturdidos.
[RequireComponent(typeof(Rigidbody))]
public class FrascoLanzado : MonoBehaviour
{
    [Header("Efecto en fantasmas")]
    [Tooltip("Radio de la salpicadura, en metros. Los fantasmas dentro de este radio quedan aturdidos")]
    public float radioSalpicadura = 1.5f;

    [Tooltip("Segundos que el fantasma queda aturdido")]
    public float segundosAturdido = 8f;

    [Header("Rotura")]
    [Tooltip("Prefab de partículas opcional. Debe destruirse solo (Stop Action: Destroy)")]
    public GameObject efectoRotura;

    [Tooltip("Nombre de un audio del AudioManager que suena al romperse (opcional)")]
    public string sonidoRotura = "";

    [Tooltip("Si no choca con nada, desaparece pasado este tiempo")]
    public float segundosDeVida = 6f;

    bool roto;

    void Awake()
    {
        // Evita que atraviese paredes delgadas a alta velocidad
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        Destroy(gameObject, segundosDeVida);
    }

    void OnCollisionEnter(Collision colision)
    {
        Vector3 punto = colision.contactCount > 0 ? colision.GetContact(0).point : transform.position;
        Romper(punto);
    }

    // El collider del fantasma es un Trigger: el frasco lo atraviesa y se detecta aquí
    void OnTriggerEnter(Collider otro)
    {
        if (otro.GetComponentInParent<Fantasma>() != null)
        {
            Romper(transform.position);
        }
    }

    void Romper(Vector3 punto)
    {
        if (roto) return;
        roto = true;

        Collider[] cercanos = Physics.OverlapSphere(punto, radioSalpicadura, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider c in cercanos)
        {
            Fantasma fantasma = c.GetComponentInParent<Fantasma>();
            if (fantasma != null)
            {
                fantasma.Aturdir(segundosAturdido);
            }
        }

        if (efectoRotura != null)
        {
            Instantiate(efectoRotura, punto, Quaternion.identity);
        }

        SonidoSeguro.Reproducir(sonidoRotura);
        Destroy(gameObject);
    }
}