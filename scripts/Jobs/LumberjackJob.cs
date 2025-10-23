using UnityEngine;
using System.Collections;

// Trabalho do lenhador: cortar árvores e coletar madeira
public class LumberjackJob : VillagerJob
{
    [Header("Configurações do Lenhador")]
    public int foodCostPerTree = 20;    // Comida gasta por árvore
    public int woodPerTree = 20;        // Madeira obtida por árvore
    public float cooldownDuration = 30f; // Tempo de descanso
    public float chopDistance = 1.5f;    // Distância para cortar árvore
    public int treesBeforeCooldown = 3;  // Quantas árvores corta antes de descansar

    private int treesCutSinceCooldown = 0; // Contador de árvores cortadas
    private bool isOnCooldown = false;     // Está descansando?
    private bool isChopping = false;       // Está cortando?

    protected override void SetupBehaviorTree()
    {
        // Cria Behavior Tree simples com uma única ação
        rootNode = new ActionNode(() => LumberjackBehavior());
    }

    protected override void CleanupJob()
    {
        // Libera a árvore atribuída
        if (bb.treeAssigned != null)
        {
            bb.treeAssigned.assigned = false;
            bb.treeAssigned = null;
        }

        // Para qualquer corrotina
        StopAllCoroutines();
        isChopping = false;
        isOnCooldown = false;
        treesCutSinceCooldown = 0;
    }

    // Função principal do lenhador
    private NodeState LumberjackBehavior()
    {
        // Se estiver descansando, não faz nada
        if (isOnCooldown)
        {
            villager.SetAnimatorMoving(false);
            return NodeState.RUNNING;
        }

        // Procura uma árvore se não tiver ou se a atual estiver acabada
        if (bb.treeAssigned == null || bb.treeAssigned.IsDepleted())
        {
            var tree = GameManager.Instance.FindNearestAvailableTree(villager.transform.position);
            if (tree == null)
            {
                // Sem árvores, fica parado
                villager.IdleBehavior();
                return NodeState.RUNNING;
            }

            // Marca árvore como atribuída
            bb.treeAssigned = tree;
            tree.AssignLumberjack(villager);
        }

        // Calcula distância até a árvore
        float dist = Vector3.Distance(villager.transform.position, bb.treeAssigned.transform.position);

        if (dist > chopDistance)
        {
            // Está longe, anda até a árvore
            villager.agent.isStopped = false;
            villager.agent.SetDestination(bb.treeAssigned.transform.position);
            villager.SetAnimatorMoving(true);
            return NodeState.RUNNING;
        }
        else
        {
            // Chegou na árvore, começa a cortar
            villager.agent.isStopped = true;
            villager.SetAnimatorMoving(false);
            StartCoroutine(ChopRoutine());
            return NodeState.SUCCESS;
        }
    }

    // Corrotina para cortar árvore
    private IEnumerator ChopRoutine()
    {
        if (isChopping || bb.treeAssigned == null) yield break;
        isChopping = true;

        // Verifica se tem comida
        if (!GameManager.Instance.SpendFood(foodCostPerTree))
        {
            Debug.Log($"{villager.name} não tem comida suficiente!");
            isChopping = false;
            yield break;
        }

        // Toca animação de corte
        if (villager.animator)
            villager.animator.SetTrigger("Chop");

        yield return new WaitForSeconds(1.2f); // Espera tempo da animação

        // Coleta madeira
        if (bb.treeAssigned != null)
        {
            int obtained = bb.treeAssigned.Harvest(woodPerTree);
            if (obtained > 0)
            {
                GameManager.Instance.AddWood(obtained);
                Debug.Log($"{villager.name} cortou {obtained} de madeira!");
            }
        }

        // Libera árvore se acabou
        if (bb.treeAssigned != null && bb.treeAssigned.IsDepleted())
            bb.treeAssigned = null;

        // Verifica cooldown
        treesCutSinceCooldown++;
        if (treesCutSinceCooldown >= treesBeforeCooldown)
        {
            treesCutSinceCooldown = 0;
            StartCoroutine(CooldownRoutine());
        }

        isChopping = false;
    }

    // Corrotina para descanso do lenhador
    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        Debug.Log($"{villager.name} está descansando por {cooldownDuration} segundos...");
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
        Debug.Log($"{villager.name} está pronto para trabalhar novamente!");
    }
}
