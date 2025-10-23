using UnityEngine;
using UnityEngine.AI;

// Garante que o componente NavMeshAgent esteja presente
[RequireComponent(typeof(NavMeshAgent))]
public class VillagerController : MonoBehaviour
{
    [Header("Stats")]
    public int id; // ID do aldeão
    public float hungerPerDay = 20f; // Fome diária

    [Header("Home")]
    public House home; // Referência da casa do aldeão
    private bool isGoingHome = false; // Está indo pra casa?
    private bool isInsideHome = false; // Está dentro da casa?

    [Header("Components")]
    [HideInInspector] public NavMeshAgent agent; // Componente de movimento da Unity
    [HideInInspector] public Animator animator; // Controla as animações

    [Header("Movement")]
    private float wanderRadius = 4f; // Distância máxima do movimento aleatório
    private float wanderTimer = 3f; // Tempo entre mudanças de destino
    private float timer; // Contador de tempo

    // Referência ao trabalho atual do aldeão
    private VillagerJob currentJobComponent;

    void Start()
    {
        // Pega o componente de navegação
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Faltando NavMeshAgent!");
            return;
        }

        // Pega o animador (caso exista)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Faltando Animator!");
        }

        // Registra o aldeão no GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterVillager(this);
        }

        // Pega o componente de trabalho (se tiver)
        currentJobComponent = GetComponent<VillagerJob>();
    }

    void Update()
    {
        // Se o aldeão está dentro de casa, não faz nada
        if (isInsideHome)
            return;

        // Verifica se é cavaleiro (Knight)
        bool isKnight = false;
        if (currentJobComponent != null && currentJobComponent is KnightJob)
            isKnight = true;

        // Se for noite e não for cavaleiro, vai pra casa
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isNight && !isKnight)
            {
                if (!isGoingHome && !isInsideHome)
                    GoHome();
                return;
            }
        }

        // Durante o dia
        if (!isGoingHome)
        {
            // Se tiver um trabalho ativo, executa
            if (currentJobComponent != null && currentJobComponent.enabled)
            {
                currentJobComponent.ExecuteJob();
            }
            else
            {
                // Caso contrário, anda aleatoriamente
                DoIdleWander();
            }
        }

        timer += Time.deltaTime;
        CheckIfAtHome(); // Verifica se chegou em casa
    }

    // Faz o aldeão ir para casa
    public void GoHome()
    {
        if (isGoingHome || isInsideHome)
            return;

        home = FindHouse(); // Procura uma casa disponível

        if (home == null)
        {
            Debug.LogWarning("Nenhuma casa encontrada!");
            return;
        }

        Debug.Log("Indo pra casa!");

        isGoingHome = true;
        agent.ResetPath();
        agent.isStopped = false;
        agent.speed = agent.speed * 1.5f; // Acelera para correr
        agent.SetDestination(home.doorPosition.position);

        if (animator)
            animator.SetBool("isMoving", true);
    }

    // Verifica se o aldeão chegou na casa
    void CheckIfAtHome()
    {
        if (!isGoingHome || home == null)
            return;

        float distance = Vector3.Distance(transform.position, home.doorPosition.position);

        // Chegou na porta
        if (distance < 2f || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f))
        {
            Debug.Log("Chegou em casa!");
            EnterHomeNow();
        }
    }

    // Faz o aldeão "entrar" na casa
    void EnterHomeNow()
    {
        isGoingHome = false;
        isInsideHome = true;

        if (animator)
            animator.SetBool("isMoving", false);

        agent.ResetPath();
        agent.isStopped = true;
        agent.speed = 3.5f;

        if (home != null)
            home.OnVillagerEnter(this);

        Debug.Log("Entrou em casa!");
    }

    // Sai de casa pela manhã
    public void LeaveHome()
    {
        Debug.Log("Saindo de casa!");

        isGoingHome = false;
        isInsideHome = false;

        if (home != null)
        {
            home.RemoveVillager(this);
            home = null;
        }

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.speed = 3.5f;
        }

        if (animator)
            animator.SetBool("isMoving", false);
    }

    // Procura uma casa livre
    private House FindHouse()
    {
        if (GameManager.Instance == null || GameManager.Instance.allHouses == null)
            return null;

        House closestHouse = null;
        float closestDistance = 99999f;

        // Procura a casa mais próxima com espaço
        foreach (House h in GameManager.Instance.allHouses)
        {
            if (h != null && h.HasSpace)
            {
                float dist = Vector3.Distance(transform.position, h.doorPosition.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestHouse = h;
                }
            }
        }

        // Se achou, tenta ocupar
        if (closestHouse != null)
        {
            bool assigned = closestHouse.AssignVillager(this);
            if (assigned)
            {
                Debug.Log("Casa encontrada: " + closestHouse.name);
                return closestHouse;
            }
        }

        Debug.LogWarning("Nenhuma casa disponível!");
        return null;
    }

    // Movimento aleatório (idle wander)
    public void DoIdleWander()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            // Escolhe uma direção aleatória
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            Vector3 newPos = transform.position + randomDirection;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newPos, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);

                if (animator)
                    animator.SetBool("isMoving", true);
            }

            timer = 0;
        }

        // Atualiza animação de movimento
        if (agent.remainingDistance > 0.1f && !agent.pathPending)
            animator?.SetBool("isMoving", true);
        else
            animator?.SetBool("isMoving", false);
    }

    // Mesmo comportamento de idle wander (usado por outras classes)
    public void IdleBehavior()
    {
        DoIdleWander();

        if (agent.remainingDistance > 0.1f && !agent.pathPending)
            animator?.SetBool("isMoving", true);
        else
            animator?.SetBool("isMoving", false);
    }

    // Atualiza animação de movimento manualmente
    public void SetAnimatorMoving(bool moving)
    {
        if (animator)
            animator.SetBool("isMoving", moving);
    }

    // Atribui um novo trabalho
    public void AssignJob<T>() where T : VillagerJob
    {
        // Remove o trabalho antigo
        if (currentJobComponent != null)
        {
            currentJobComponent.OnJobEnd();
            currentJobComponent.enabled = false;
        }

        // Adiciona novo trabalho
        T jobComponent = GetComponent<T>();
        if (jobComponent == null)
            jobComponent = gameObject.AddComponent<T>();

        jobComponent.enabled = true;
        jobComponent.OnJobStart();
        currentJobComponent = jobComponent;

        Debug.Log("Trabalho atribuído: " + typeof(T).Name);

        // Atualiza a interface
        if (GameManager.Instance != null && GameManager.Instance.selectedVillager == this)
        {
            UIManager.Instance?.RefreshVillagerPanel();
        }
    }

    // Remove o trabalho atual
    public void RemoveJob()
    {
        if (currentJobComponent != null)
        {
            currentJobComponent.OnJobEnd();
            currentJobComponent.enabled = false;
            currentJobComponent = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.selectedVillager == this)
        {
            UIManager.Instance?.RefreshVillagerPanel();
        }
    }

    // Retorna o componente de trabalho atual
    public VillagerJob GetCurrentJob() => currentJobComponent;

    // Retorna o nome do trabalho atual
    public string GetJobName()
    {
        if (currentJobComponent == null)
            return "Sem trabalho";

        string jobType = currentJobComponent.GetType().Name;
        if (jobType.EndsWith("Job"))
            jobType = jobType.Substring(0, jobType.Length - 3);

        return jobType;
    }

    // Verifica se o aldeão tem um trabalho
    public bool HasJob() => currentJobComponent != null;

    // Verifica se está se movendo
    public bool IsMoving() => agent.velocity.sqrMagnitude > 0.01f;

    // Quando o jogador clica no aldeão
    void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectVillager(this);
        }
    }
}
