using UnityEngine; // Importa a biblioteca principal do Unity
using UnityEngine.AI; // Importa o sistema de navegação (NavMeshAgent)

public class Monster : MonoBehaviour // Classe principal do monstro
{
    [Header("References")] // Cabeçalho para organizar no Inspector
    public NavMeshAgent agent; // Referência ao agente de navegação
    private BTNode behaviorTree; // Árvore de comportamento (IA)
    private MonsterBlackboard blackboard; // "Quadro negro" (memória do monstro)

    [Header("Stats")] // Cabeçalho das estatísticas
    public float health = 100f; // Vida do monstro
    public float damage = 10f; // Dano causado por ataque
    public float attackRange = 2f; // Distância para poder atacar
    public float attackCooldown = 1.5f; // Tempo de espera entre ataques

    [Header("State")] // Cabeçalho do estado atual
    public Vector3 spawnPoint; // Ponto de nascimento do monstro
    public int spawnPointIndex; // Índice do ponto de spawn (para o spawner saber)
    public bool isReturningHome = false; // Se o monstro está voltando para casa

    void Start()
    {
        // Pega o componente NavMeshAgent se não tiver sido atribuído
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Cria o blackboard e armazena informações nele
        blackboard = new MonsterBlackboard();
        blackboard.agent = agent;
        blackboard.spawnPoint = spawnPoint;
        blackboard.spawnPointIndex = spawnPointIndex;
        blackboard.isReturningHome = false;
        blackboard.lastAttackTime = 0f;

        // Encontra o prédio da prefeitura (TownHall)
        TownHallHealth townHall = FindObjectOfType<TownHallHealth>();
        if (townHall != null)
        {
            // Guarda o alvo no blackboard
            blackboard.target = townHall.transform;
        }

        // Cria a árvore de comportamento do monstro
        MakeBehaviorTree();
    }

    void Update()
    {
        // Atualiza o estado de "voltando pra casa"
        blackboard.isReturningHome = isReturningHome;

        // Executa a árvore de comportamento a cada frame
        if (behaviorTree != null)
        {
            behaviorTree.Tick(); // Tick = processa os nós da árvore
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Detecta colisão com a Prefeitura
        if (other.CompareTag("TownHall"))
        {
            TownHallHealth townHall = other.GetComponent<TownHallHealth>();
            if (townHall != null)
            {
                // Causa dano à Prefeitura
                townHall.TakeDamage(damage);
                Debug.Log("Monster hit Town Hall!"); // Mensagem no console
            }
        }
    }

    void MakeBehaviorTree()
    {
        // Cria a árvore de comportamento do monstro

        // Nó de condição: verifica se é dia
        ConditionNode dayCheck = new ConditionNode(CheckIfDaytime);

        // Nó de ação: volta para o spawn
        ActionNode goHomeAction = new ActionNode(GoBackToSpawn);

        // Sequência: se for dia → vai pra casa
        SequenceNode goHomeSequence = new SequenceNode(dayCheck, goHomeAction);

        // Nó de condição: verifica se está perto da Prefeitura
        ConditionNode nearCheck = new ConditionNode(CheckIfNearTownHall);

        // Nó de ação: ataca a Prefeitura
        ActionNode attackAction = new ActionNode(AttackTheTownHall);

        // Sequência: se estiver perto → ataca
        SequenceNode attackSequence = new SequenceNode(nearCheck, attackAction);

        // Nó de ação: mover-se até a Prefeitura
        ActionNode moveAction = new ActionNode(MoveToTheTownHall);

        // Monta a árvore: tenta ir pra casa → senão atacar → senão mover
        behaviorTree = new SelectorNode(goHomeSequence, attackSequence, moveAction);
    }

    // Verifica se é dia (para decidir se deve voltar pra casa)
    bool CheckIfDaytime()
    {
        // Se já está voltando pra casa, retorna verdadeiro
        if (blackboard.isReturningHome)
        {
            return true;
        }

        // Checa se o GameManager existe e se não é noite
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.isNight)
            {
                return true; // Está de dia
            }
        }

        return false; // Continua sendo noite
    }

    // Verifica se o monstro está perto da Prefeitura
    bool CheckIfNearTownHall()
    {
        // Se não tem alvo, falha
        if (blackboard.target == null)
        {
            return false;
        }

        // Calcula a distância até a Prefeitura
        float distance = Vector3.Distance(transform.position, blackboard.target.position);

        // Se está dentro do alcance de ataque → sucesso
        if (distance <= attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Ação: voltar para o ponto de spawn
    NodeState GoBackToSpawn()
    {
        // Calcula a distância até o spawn
        float distanceToSpawn = Vector3.Distance(transform.position, blackboard.spawnPoint);

        // Se já chegou no spawn
        if (distanceToSpawn < 1f)
        {
            // Remove o monstro do controle do spawner
            if (MonsterSpawner.Instance != null)
            {
                MonsterSpawner.Instance.UnregisterMonster(this);
            }

            // Destroi o monstro
            Destroy(gameObject);
            return NodeState.SUCCESS; // Tarefa concluída
        }

        // Ainda está longe → continua indo
        blackboard.agent.isStopped = false;
        blackboard.agent.SetDestination(blackboard.spawnPoint);
        return NodeState.RUNNING; // Ainda executando
    }

    // Ação: mover-se até a Prefeitura
    NodeState MoveToTheTownHall()
    {
        if (blackboard.target == null)
        {
            return NodeState.FAILURE; // Falha se não tem alvo
        }

        // Calcula a distância até a Prefeitura
        float distance = Vector3.Distance(transform.position, blackboard.target.position);

        // Se está longe → anda até lá
        if (distance > attackRange)
        {
            blackboard.agent.isStopped = false;
            blackboard.agent.SetDestination(blackboard.target.position);
        }
        else
        {
            // Se chegou perto → para de andar
            blackboard.agent.isStopped = true;
        }

        return NodeState.RUNNING; // Continua executando
    }

    // Ação: atacar a Prefeitura
    NodeState AttackTheTownHall()
    {
        if (blackboard.target == null)
        {
            return NodeState.FAILURE; // Falha se não tem alvo
        }

        // Para de andar antes de atacar
        blackboard.agent.isStopped = true;

        // Faz o monstro olhar para a Prefeitura
        Vector3 direction = blackboard.target.position - transform.position;
        direction.Normalize();
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;

        // Calcula o tempo desde o último ataque
        float timeSinceLastAttack = Time.time - blackboard.lastAttackTime;

        // Se já passou o tempo de recarga → pode atacar
        if (timeSinceLastAttack >= attackCooldown)
        {
            // Causa dano na Prefeitura
            TownHallHealth townHall = blackboard.target.GetComponent<TownHallHealth>();
            if (townHall != null)
            {
                townHall.TakeDamage(damage);
                blackboard.lastAttackTime = Time.time; // Atualiza tempo do último ataque
                Debug.Log("Monster attacks Town Hall!"); // Mensagem no console
            }
        }

        return NodeState.RUNNING; // Continua repetindo (mantém atacando)
    }

    // Monstro leva dano
    public void TakeDamage(float amount)
    {
        health = health - amount; // Diminui a vida

        if (health <= 0)
        {
            MonsterDie(); // Se a vida chegou a 0 → morre
        }
    }

    // Morte do monstro
    void MonsterDie()
    {
        // Remove o monstro do controle do spawner
        if (MonsterSpawner.Instance != null)
        {
            MonsterSpawner.Instance.UnregisterMonster(this);
        }

        // Destroi o objeto do monstro
        Destroy(gameObject);
    }
}
