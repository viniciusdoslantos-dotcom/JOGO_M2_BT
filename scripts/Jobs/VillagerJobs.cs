using UnityEngine; // Importa as classes e funções básicas do Unity (GameObject, MonoBehaviour, Debug, etc).

// Classe base para todos os trabalhos (jobs) dos aldeões
public abstract class VillagerJob : MonoBehaviour
{
    // Guarda uma referência para o controlador do aldeão
    protected VillagerController villager;

    // Guarda o nó raiz da árvore de comportamento (Behavior Tree)
    protected BTNode rootNode;

    // Guarda o "quadro negro" (dados compartilhados entre os nós)
    protected Blackboard bb;

    // Função chamada automaticamente quando o objeto é criado na cena
    protected virtual void Awake()
    {
        // Pega o componente VillagerController no mesmo GameObject
        villager = GetComponent<VillagerController>();

        // Se o aldeão não tiver esse componente, mostra erro no console
        if (villager == null)
        {
            Debug.LogError($"{GetType().Name} precisa do componente VillagerController!");
            // Desativa o script para evitar erros futuros por falta de dependência
            enabled = false;
            return; // Sai da função Awake para não continuar executando
        }

        // Cria um novo Blackboard (um tipo de "banco de dados" usado pela IA)
        bb = new Blackboard
        {
            agent = villager.agent,     // Atribui ao Blackboard o agente de navegação do aldeão
            villager = villager         // Atribui ao Blackboard a referência do próprio aldeão
        };
    }

    // Chamado quando o aldeão começa este trabalho
    public virtual void OnJobStart()
    {
        // Monta a árvore de comportamento específica desse trabalho
        SetupBehaviorTree();

        // Mostra no console que o trabalho começou (útil para debug)
        Debug.Log($"{villager.name} começou o trabalho {GetType().Name}");
    }

    // Chamado quando o aldeão termina este trabalho
    public virtual void OnJobEnd()
    {
        // Limpa os dados ou comportamentos específicos do trabalho
        CleanupJob();

        // Mostra no console que o trabalho terminou (útil para debug)
        Debug.Log($"{villager.name} terminou o trabalho {GetType().Name}");
    }

    // Executado a cada frame durante o dia (quando o aldeão está ativo)
    public virtual void ExecuteJob()
    {
        // Se a árvore de comportamento existir, atualiza (executa) o nó raiz
        if (rootNode != null)
        {
            rootNode.Tick(); // Chama o Tick() do nó raiz para processar a IA
        }
    }

    // Função que cada tipo de trabalho deve implementar para criar sua própria árvore de comportamento
    protected abstract void SetupBehaviorTree();

    // Função que cada tipo de trabalho deve implementar para limpar recursos específicos do trabalho
    protected abstract void CleanupJob();
}
