using UnityEngine; // Importa as classes e fun��es b�sicas do Unity (GameObject, MonoBehaviour, Debug, etc).

// Classe base para todos os trabalhos (jobs) dos alde�es
public abstract class VillagerJob : MonoBehaviour
{
    // Guarda uma refer�ncia para o controlador do alde�o
    protected VillagerController villager;

    // Guarda o n� raiz da �rvore de comportamento (Behavior Tree)
    protected BTNode rootNode;

    // Guarda o "quadro negro" (dados compartilhados entre os n�s)
    protected Blackboard bb;

    // Fun��o chamada automaticamente quando o objeto � criado na cena
    protected virtual void Awake()
    {
        // Pega o componente VillagerController no mesmo GameObject
        villager = GetComponent<VillagerController>();

        // Se o alde�o n�o tiver esse componente, mostra erro no console
        if (villager == null)
        {
            Debug.LogError($"{GetType().Name} precisa do componente VillagerController!");
            // Desativa o script para evitar erros futuros por falta de depend�ncia
            enabled = false;
            return; // Sai da fun��o Awake para n�o continuar executando
        }

        // Cria um novo Blackboard (um tipo de "banco de dados" usado pela IA)
        bb = new Blackboard
        {
            agent = villager.agent,     // Atribui ao Blackboard o agente de navega��o do alde�o
            villager = villager         // Atribui ao Blackboard a refer�ncia do pr�prio alde�o
        };
    }

    // Chamado quando o alde�o come�a este trabalho
    public virtual void OnJobStart()
    {
        // Monta a �rvore de comportamento espec�fica desse trabalho
        SetupBehaviorTree();

        // Mostra no console que o trabalho come�ou (�til para debug)
        Debug.Log($"{villager.name} come�ou o trabalho {GetType().Name}");
    }

    // Chamado quando o alde�o termina este trabalho
    public virtual void OnJobEnd()
    {
        // Limpa os dados ou comportamentos espec�ficos do trabalho
        CleanupJob();

        // Mostra no console que o trabalho terminou (�til para debug)
        Debug.Log($"{villager.name} terminou o trabalho {GetType().Name}");
    }

    // Executado a cada frame durante o dia (quando o alde�o est� ativo)
    public virtual void ExecuteJob()
    {
        // Se a �rvore de comportamento existir, atualiza (executa) o n� raiz
        if (rootNode != null)
        {
            rootNode.Tick(); // Chama o Tick() do n� raiz para processar a IA
        }
    }

    // Fun��o que cada tipo de trabalho deve implementar para criar sua pr�pria �rvore de comportamento
    protected abstract void SetupBehaviorTree();

    // Fun��o que cada tipo de trabalho deve implementar para limpar recursos espec�ficos do trabalho
    protected abstract void CleanupJob();
}
