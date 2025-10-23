using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Knight job - defende a vila atacando monstros. Nunca dorme!
/// </summary>
public class KnightJob : VillagerJob
{
    [Header("Combat Stats")]
    public float attackDamage = 25f; // Dano que o cavaleiro causa
    public float attackRange = 2.5f; // Alcance do ataque
    public float attackCooldown = 1f; // Tempo entre ataques
    public float detectionRange = 15f; // Alcance para detectar inimigos

    [Header("Patrol")]
    public float patrolRadius = 10f; // Raio para patrulha
    public float patrolWaitTime = 2f; // Tempo de espera entre patrulhas

    // Estado de combate
    private Monster currentTarget; // Monstro que o cavaleiro está atacando
    private float lastAttackTime; // Hora do último ataque

    protected override void SetupBehaviorTree()
    {
        // Configura a árvore de comportamento do cavaleiro
        rootNode = new SelectorNode(
            // Prioridade 1: atacar se inimigo estiver no alcance
            new SequenceNode(
                new ConditionNode(IsEnemyInAttackRange),
                new ActionNode(AttackEnemy)
            ),

            // Prioridade 2: perseguir inimigo detectado
            new SequenceNode(
                new ConditionNode(HasTarget),
                new ActionNode(ChaseEnemy)
            ),

            // Prioridade 3: procurar o inimigo mais próximo
            new SequenceNode(
                new ConditionNode(FindNearestEnemy),
                new ActionNode(ChaseEnemy)
            ),

            // Prioridade 4: patrulhar se não houver inimigos
            new ActionNode(Patrol)
        );

        // Mensagem de debug no console
        Debug.Log($"🛡️ {villager.name} agora está defendendo a vila como Cavaleiro!");
    }

    protected override void CleanupJob()
    {
        // Limpa o alvo quando o trabalho termina
        currentTarget = null;
    }

    // ================= Condições =================

    bool HasTarget()
    {
        // Retorna true se o alvo atual ainda existe
        return currentTarget != null;
    }

    bool IsEnemyInAttackRange()
    {
        if (currentTarget == null) return false;

        // Calcula distância até o alvo
        float distance = Vector3.Distance(villager.transform.position, currentTarget.transform.position);
        return distance <= attackRange; // Retorna true se estiver dentro do alcance
    }

    bool FindNearestEnemy()
    {
        // Procura todos os monstros na cena
        Monster[] allMonsters = FindObjectsOfType<Monster>();

        if (allMonsters.Length == 0)
        {
            currentTarget = null;
            return false; // Nenhum inimigo encontrado
        }

        // Procura o monstro mais próximo dentro do alcance de detecção
        Monster nearest = null;
        float nearestDistance = detectionRange;

        foreach (Monster monster in allMonsters)
        {
            float distance = Vector3.Distance(villager.transform.position, monster.transform.position);
            if (distance < nearestDistance)
            {
                nearest = monster;
                nearestDistance = distance;
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest; // Define o alvo atual
            Debug.Log($"⚔️ {villager.name} detectou inimigo a {nearestDistance:F1}m!");
            return true; // Encontrou inimigo
        }

        currentTarget = null;
        return false; // Nenhum inimigo próximo
    }

    // ================= Ações =================

    NodeState ChaseEnemy()
    {
        if (currentTarget == null) return NodeState.FAILURE;

        // Move em direção ao inimigo
        bb.agent.isStopped = false;
        bb.agent.SetDestination(currentTarget.transform.position);
        villager.SetAnimatorMoving(true); // Ativa animação de movimento

        return NodeState.RUNNING;
    }

    NodeState AttackEnemy()
    {
        if (currentTarget == null) return NodeState.FAILURE;

        // Para de se mover
        bb.agent.isStopped = true;
        villager.SetAnimatorMoving(false);

        // Rotaciona para o inimigo
        Vector3 direction = (currentTarget.transform.position - villager.transform.position).normalized;
        villager.transform.rotation = Quaternion.Slerp(
            villager.transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 5f
        );

        // Ataca se o cooldown já passou
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            currentTarget.TakeDamage(attackDamage); // Causa dano
            lastAttackTime = Time.time;
            Debug.Log($"⚔️ {villager.name} atacou o monstro causando {attackDamage} de dano!");

            // Verifica se o inimigo morreu
            if (currentTarget.health <= 0)
            {
                Debug.Log($"🎯 {villager.name} derrotou um inimigo!");
                currentTarget = null;
                return NodeState.SUCCESS;
            }
        }

        return NodeState.RUNNING;
    }

    NodeState Patrol()
    {
        // Patrulha simples: escolhe ponto aleatório perto
        if (!bb.agent.hasPath || bb.agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = villager.transform.position + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = villager.transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                bb.agent.isStopped = false;
                bb.agent.SetDestination(hit.position);
                villager.SetAnimatorMoving(true);
            }
        }
        else
        {
            // Continua animando enquanto se move
            villager.SetAnimatorMoving(bb.agent.velocity.sqrMagnitude > 0.01f);
        }

        return NodeState.RUNNING;
    }

    // ================= Debug =================
    void OnDrawGizmosSelected()
    {
        if (villager == null) return;

        // Desenha alcance de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(villager.transform.position, detectionRange);

        // Desenha alcance de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(villager.transform.position, attackRange);

        // Desenha linha até o alvo atual
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(villager.transform.position, currentTarget.transform.position);
        }
    }
}
