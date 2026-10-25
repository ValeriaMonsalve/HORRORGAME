using UnityEngine;
using UnityEngine.AI;

// Fantasma que patrulla la casa, detecta al jugador con un cono de visión y lo ataca.
// Estados: Patrullando -> Persiguiendo -> Buscando -> Patrullando.
// Requiere un NavMesh horneado en la casa (AI Navigation).
[RequireComponent(typeof(NavMeshAgent))]
public class Fantasma : MonoBehaviour
{
    enum Estado { Patrullando, Persiguiendo, Buscando, Aturdido }

    [Header("Patrullaje")]
    [Tooltip("Puntos de la casa que puede visitar (objetos vacíos en el piso). Si queda vacío, elige puntos al azar alrededor de donde empieza")]
    public Transform[] puntosPatrullaje;

    [Tooltip("Radio de los puntos aleatorios cuando no hay puntos asignados")]
    public float radioPatrullaje = 15f;

    public float velocidadPatrulla = 1.5f;

    [Tooltip("Segundos que espera al llegar a cada punto")]
    public float esperaEnPunto = 2f;

    [Header("Visión")]
    public float distanciaVision = 12f;

    [Tooltip("Ángulo total del cono de visión, en grados")]
    [Range(10f, 360f)] public float anguloVision = 90f;

    [Tooltip("Altura de los ojos respecto al piso (el pivote del fantasma debe estar en sus pies)")]
    public float alturaOjos = 1.6f;

    [Tooltip("Capas que bloquean la vista (paredes, puertas...). Por defecto: todas")]
    public LayerMask capasObstaculos = ~0;

    [Header("Persecución")]
    [Tooltip("Tu Movement.cs usa 5 por defecto; con menos que eso el jugador puede escapar")]
    public float velocidadPersecucion = 4f;
    public float velocidadBusqueda = 2.5f;

    [Tooltip("Segundos que busca en el último lugar donde vio al jugador antes de rendirse")]
    public float tiempoBusqueda = 4f;

    [Tooltip("Desmarcado: deja de perseguir apenas el jugador sale de su cono de visión o se esconde tras una pared")]
    public bool perseguirFueraDelCono = false;

    [Tooltip("Desmarcado: al perder de vista al jugador vuelve a patrullar de inmediato, sin ir a donde lo vio por última vez")]
    public bool buscarAlPerderlo = false;

    public float velocidadGiro = 8f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.5f;
    public float danio = 20f;
    public float tiempoEntreAtaques = 1.5f;

    NavMeshAgent agent;
    Transform jugador;
    VidaJugador vidaJugador;
    Collider colJugador;

    [Header("Depuración (no lo cambies, solo mira)")]
    [SerializeField] Estado estado;
    Vector3 posicionInicial;
    Vector3 ultimaPosicionVista;
    int indicePunto = -1;
    bool esperando;
    float temporizador;
    float proximoAtaque;
    float temporizadorAturdido;

    [Header("Agua bendita")]
    [Tooltip("Objeto opcional (partículas, luz...) que se activa mientras el fantasma está aturdido")]
    public GameObject efectoAturdido;

    readonly RaycastHit[] impactos = new RaycastHit[16];

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        vidaJugador = FindAnyObjectByType<VidaJugador>();
        if (vidaJugador == null)
        {
            Debug.LogError("Fantasma: no encontré al jugador. Agrega el script VidaJugador al Player.", this);
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("Fantasma: no está sobre un NavMesh. Hornea el NavMesh de la casa y coloca al fantasma dentro.", this);
            enabled = false;
            return;
        }

        jugador = vidaJugador.transform;
        colJugador = jugador.GetComponent<Collider>();
        posicionInicial = transform.position;

        IniciarPatrulla();
    }

    void Update()
    {
        // Sin actividad en pausa ni cuando el jugador ya murió
        if (Time.timeScale == 0f || vidaJugador.EstaMuerto) return;

        // Mientras persigue ya no necesita tenerlo dentro del cono: solo línea de vista
        bool ve = VeAlJugador(perseguirFueraDelCono && estado == Estado.Persiguiendo);

        switch (estado)
        {
            case Estado.Patrullando:
                if (ve) IniciarPersecucion();
                else ActualizarPatrulla();
                break;

            case Estado.Persiguiendo:
                if (ve) ActualizarPersecucion();
                else PerderJugador();
                break;

            case Estado.Buscando:
                if (ve) IniciarPersecucion();
                else ActualizarBusqueda();
                break;

            case Estado.Aturdido:
                ActualizarAturdido();
                break;
        }
    }

    // ---------- PATRULLAJE ----------

    void IniciarPatrulla()
    {
        estado = Estado.Patrullando;
        agent.speed = velocidadPatrulla;
        agent.isStopped = false;
        IrAlSiguientePunto();
    }

    void IrAlSiguientePunto()
    {
        esperando = false;
        agent.SetDestination(ElegirPuntoPatrulla());
    }

    void ActualizarPatrulla()
    {
        if (esperando)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f) IrAlSiguientePunto();
        }
        else if (HaLlegado())
        {
            esperando = true;
            temporizador = esperaEnPunto;
        }
    }

    Vector3 ElegirPuntoPatrulla()
    {
        Vector3 candidato;

        if (puntosPatrullaje != null && puntosPatrullaje.Length > 0)
        {
            // Punto aleatorio de la lista, sin repetir el anterior
            int nuevo = Random.Range(0, puntosPatrullaje.Length);
            if (puntosPatrullaje.Length > 1)
            {
                while (nuevo == indicePunto)
                {
                    nuevo = Random.Range(0, puntosPatrullaje.Length);
                }
            }
            indicePunto = nuevo;
            candidato = puntosPatrullaje[nuevo].position;

            return PuntoEnNavMesh(candidato, out Vector3 snap) ? snap : candidato;
        }

        // Sin lista: punto aleatorio sobre el NavMesh alrededor de donde empezó
        for (int i = 0; i < 10; i++)
        {
            candidato = posicionInicial + Random.insideUnitSphere * radioPatrullaje;
            if (PuntoEnNavMesh(candidato, out Vector3 snap)) return snap;
        }

        return transform.position;
    }

    // ---------- PERSECUCIÓN Y ATAQUE ----------

    void IniciarPersecucion()
    {
        estado = Estado.Persiguiendo;
        agent.speed = velocidadPersecucion;
        agent.isStopped = false;
        esperando = false;
    }

    void ActualizarPersecucion()
    {
        ultimaPosicionVista = jugador.position;

        if (DistanciaPlana(jugador.position) <= distanciaAtaque)
        {
            // Lo tiene al alcance: se detiene, lo mira y ataca
            agent.isStopped = true;
            MirarAlJugador();
            Atacar();
        }
        else
        {
            agent.isStopped = false;
            if (PuntoEnNavMesh(jugador.position, out Vector3 destino))
            {
                agent.SetDestination(destino);
            }
        }
    }

    void Atacar()
    {
        if (Time.time < proximoAtaque) return;

        proximoAtaque = Time.time + tiempoEntreAtaques;
        vidaJugador.RecibirDanio(danio);
    }

    // ---------- BÚSQUEDA (perdió de vista al jugador) ----------

    // ---------- AGUA BENDITA ----------

    // Lo llama FrascoLanzado al romperse cerca del fantasma:
    // se queda quieto y no puede atacar durante unos segundos.
    public void Aturdir(float segundos)
    {
        estado = Estado.Aturdido;
        temporizadorAturdido = segundos;
        esperando = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (efectoAturdido != null) efectoAturdido.SetActive(true);
    }

    void ActualizarAturdido()
    {
        temporizadorAturdido -= Time.deltaTime;

        if (temporizadorAturdido <= 0f)
        {
            if (efectoAturdido != null) efectoAturdido.SetActive(false);
            IniciarPatrulla();
        }
    }

    void PerderJugador()
    {
        if (buscarAlPerderlo) IniciarBusqueda();
        else IniciarPatrulla();
    }

    void IniciarBusqueda()
    {
        estado = Estado.Buscando;
        agent.speed = velocidadBusqueda;
        agent.isStopped = false;
        esperando = false;

        if (PuntoEnNavMesh(ultimaPosicionVista, out Vector3 destino))
        {
            agent.SetDestination(destino);
        }
    }

    void ActualizarBusqueda()
    {
        if (esperando)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f) IniciarPatrulla();
        }
        else if (HaLlegado())
        {
            esperando = true;
            temporizador = tiempoBusqueda;
        }
    }

    // ---------- VISIÓN ----------

    bool VeAlJugador(bool ignorarAngulo)
    {
        Vector3 origen = transform.position + Vector3.up * alturaOjos;
        Vector3 objetivo = colJugador != null ? colJugador.bounds.center : jugador.position + Vector3.up;

        Vector3 haciaJugador = objetivo - origen;
        float distancia = haciaJugador.magnitude;

        if (distancia > distanciaVision) return false;
        if (distancia < 0.01f) return true;

        // Cono de visión (solo en horizontal)
        if (!ignorarAngulo)
        {
            Vector3 plano = new Vector3(haciaJugador.x, 0f, haciaJugador.z);
            if (plano.sqrMagnitude > 0.0001f && Vector3.Angle(transform.forward, plano) > anguloVision * 0.5f)
            {
                return false;
            }
        }

        // Línea de vista: si algo que no sea el fantasma ni el jugador se interpone, no lo ve
        int n = Physics.RaycastNonAlloc(origen, haciaJugador / distancia, impactos, distancia, capasObstaculos, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            Transform t = impactos[i].collider.transform;
            if (t.IsChildOf(transform) || t.IsChildOf(jugador)) continue;
            return false;
        }

        return true;
    }

    // ---------- UTILIDADES ----------

    bool HaLlegado()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance + 0.3f) return false;
        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }

    bool PuntoEnNavMesh(Vector3 punto, out Vector3 resultado)
    {
        if (NavMesh.SamplePosition(punto, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            resultado = hit.position;
            return true;
        }

        resultado = punto;
        return false;
    }

    float DistanciaPlana(Vector3 punto)
    {
        Vector3 d = punto - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    void MirarAlJugador()
    {
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), velocidadGiro * Time.deltaTime);
    }

    // Muestra el cono de visión y el alcance de ataque al seleccionar el fantasma
    void OnDrawGizmosSelected()
    {
        Vector3 origen = transform.position + Vector3.up * alturaOjos;

        Gizmos.color = Color.yellow;
        Vector3 izq = Quaternion.Euler(0f, -anguloVision * 0.5f, 0f) * transform.forward;
        Vector3 der = Quaternion.Euler(0f, anguloVision * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(origen, izq * distanciaVision);
        Gizmos.DrawRay(origen, der * distanciaVision);
        Gizmos.DrawRay(origen, transform.forward * distanciaVision);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}