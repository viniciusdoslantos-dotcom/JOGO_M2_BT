using UnityEngine;
using System.Collections;

// Trabalho do lenhador: cortar �rvores e coletar madeira
public class LumberjackJob : VillagerJob
{
    [Header("Configura��es do Lenhador")]
    public int foodCostPerTree = 20;    // Comida gasta por �rvore
    public int woodPerTree = 20;        // Madeira obtida por �rvore
    public float cooldownDuration = 30f; // Tempo de descanso
    public float chopDistance = 1.5f;    // Dist�ncia para cortar �rvore
    public int treesBeforeCooldown = 3;  // Quantas �rvores corta antes de descansar

    private int treesCutSinceCooldown = 0; // Contador de �rvores cortadas
    private bool isOnCooldown = false;     // Est� descansando?
    private bool isChopping = false;       // Est� cortando?

    protected override void SetupBehaviorTree()
    {
        // Cria Behavior Tree simples com uma �nica a��o
        rootNode = new ActionNode(() => LumberjackBehavior());
    }

    protected override void CleanupJob()
    {
        // Libera a �rvore atribu�da
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

    // Fun��o principal do lenhador
    private NodeState LumberjackBehavior()
    {
        // Se estiver descansando, n�o faz nada
        if (isOnCooldown)
        {
            villager.SetAnimatorMoving(false);
            return NodeState.RUNNING;
        }

        // Procura uma �rvore se n�o tiver ou se a atual estiver acabada
        if (bb.treeAssigned == null || bb.treeAssigned.IsDepleted())
        {
            var tree = GameManager.Instance.FindNearestAvailableTree(villager.transform.position);
            if (tree == null)
            {
                // Sem �rvores, fica parado
                villager.IdleBehavior();
                return NodeState.RUNNING;
            }

            // Marca �rvore como atribu�da
            bb.treeAssigned = tree;
            tree.AssignLumberjack(villager);
        }

        // Calcula dist�ncia at� a �rvore
        float dist = Vector3.Distance(villager.transform.position, bb.treeAssigned.transform.position);

        if (dist > chopDistance)
        {
            // Est� longe, anda at� a �rvore
            villager.agent.isStopped = false;
            villager.agent.SetDestination(bb.treeAssigned.transform.position);
            villager.SetAnimatorMoving(true);
            return NodeState.RUNNING;
        }
        else
        {
            // Chegou na �rvore, come�a a cortar
            villager.agent.isStopped = true;
            villager.SetAnimatorMoving(false);
            StartCoroutine(ChopRoutine());
            return NodeState.SUCCESS;
        }
    }

    // Corrotina para cortar �rvore
    private IEnumerator ChopRoutine()
    {
        if (isChopping || bb.treeAssigned == null) yield break;
        isChopping = true;

        // Verifica se tem comida
        if (!GameManager.Instance.SpendFood(foodCostPerTree))
        {
            Debug.Log($"{villager.name} n�o tem comida suficiente!");
            isChopping = false;
            yield break;
        }

        // Toca anima��o de corte
        if (villager.animator)
            villager.animator.SetTrigger("Chop");

        yield return new WaitForSeconds(1.2f); // Espera tempo da anima��o

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

        // Libera �rvore se acabou
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
        Debug.Log($"{villager.name} est� descansando por {cooldownDuration} segundos...");
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
        Debug.Log($"{villager.name} est� pronto para trabalhar novamente!");
    }
}
