using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // guarda uma cópia única desse script (padrão Singleton)

    [Header("Recursos")]
    public int food = 200; // quantidade de comida
    public int wood = 0;   // quantidade de madeira

    [Header("Dias / Tempo")]
    public int currentDay = 1; // dia atual
    public int maxDays = 5;    // número máximo de dias

    [Header("Relógio do Jogo")]
    [Tooltip("Quantos segundos reais = 24 horas no jogo")]
    public float dayLengthSeconds = 240f; // 4 minutos reais = 1 dia no jogo
    public int currentHour = 6;  // começa às 6 da manhã
    public int currentMinute = 0;
    [Tooltip("Hora que o dia começa (aldeões saem de casa)")]
    public int dayStartHour = 6;  // 6:00
    [Tooltip("Hora que a noite começa (aldeões voltam pra casa)")]
    public int nightStartHour = 19; // 19:00

    private float timeAccumulator = 0f;  // acumula o tempo
    private float secondsPerGameMinute;  // quantos segundos reais valem 1 minuto no jogo
    public bool isNight = false;         // indica se é noite
    private bool wasNight = false;       // guarda se estava de noite antes

    [Header("UI")]
    public TMP_Text foodText;  // texto que mostra a comida
    public TMP_Text woodText;  // texto que mostra a madeira
    public TMP_Text dayText;   // texto que mostra o dia
    public TMP_Text clockText; // texto que mostra o horário (☀️ ou 🌙)

    [Header("Prefabs")]
    public GameObject villagerPrefab; // prefab do aldeão
    public Transform villagersParent; // objeto onde os aldeões ficam organizados
    public List<Farm> farms = new List<Farm>(); // lista de fazendas
    public List<Tree> trees = new List<Tree>(); // lista de árvores

    [Header("Objetos Dinâmicos")]
    public List<VillagerController> allVillagers = new List<VillagerController>(); // todos os aldeões
    public List<House> allHouses = new List<House>(); // todas as casas

    public VillagerController selectedVillager; // aldeão selecionado

    void Awake()
    {
        Instance = this; // guarda a instância do GameManager
    }

    void Start()
    {
        // Calcula quantos segundos reais equivalem a 1 minuto do jogo
        secondsPerGameMinute = dayLengthSeconds / 1440f;

        // Atualiza o estado inicial do dia/noite e da interface
        UpdateDayNightState();
        UpdateUI();
    }

    void Update()
    {
        UpdateClock(); // atualiza o relógio constantemente
        UpdateUI();    // atualiza os textos da interface
    }

    void UpdateClock()
    {
        timeAccumulator += Time.deltaTime; // soma o tempo real passado

        // se passou o tempo de 1 minuto do jogo
        if (timeAccumulator >= secondsPerGameMinute)
        {
            timeAccumulator -= secondsPerGameMinute;
            currentMinute++;

            // se passou de 60 minutos, vira uma hora
            if (currentMinute >= 60)
            {
                currentMinute = 0;
                currentHour++;

                // se passou de 24 horas, recomeça o dia
                if (currentHour >= 24)
                {
                    currentHour = 0;
                }
            }

            // checa se virou dia ou noite
            CheckDayNightTransition();
        }

        UpdateVisualTimeOfDay(); // atualiza visualmente o céu/luz
    }

    void CheckDayNightTransition()
    {
        wasNight = isNight; // guarda o valor anterior
        UpdateDayNightState(); // atualiza se é noite ou dia

        // só verifica transição quando é hora exata
        if (currentMinute == 0)
        {
            if (isNight && !wasNight)
            {
                OnNightFalls(); // começou a noite
            }
            else if (!isNight && wasNight)
            {
                OnDayBreaks(); // começou o dia
            }
        }
    }

    void UpdateDayNightState()
    {
        // define se é noite com base nas horas
        if (currentHour >= nightStartHour || currentHour < dayStartHour)
        {
            isNight = true;
        }
        else
        {
            isNight = false;
        }
    }

    void OnNightFalls()
    {
        Debug.Log(" A noite chegou! Aldeões voltando pra casa...");
        // os aldeões verificam isNight e vão pra casa
    }

    void OnDayBreaks()
    {
        Debug.Log(" Amanheceu! Aldeões saindo das casas...");

        currentDay++;
        if (currentDay > maxDays)
        {
            WinGame(); // ganhou o jogo se passou todos os dias
        }
        else
        {
            StartCoroutine(ReleaseVillagersRoutine()); // libera os aldeões
        }
    }

    void UpdateVisualTimeOfDay()
    {
        // procura o script que muda o céu/luz
        DayNightCycle dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle != null)
        {
            // converte hora:minuto para valor entre 0 e 1
            float normalizedTime = (currentHour + currentMinute / 60f) / 24f;
            dayNightCycle.SetTimeOfDay(normalizedTime);
        }
    }

    IEnumerator ReleaseVillagersRoutine()
    {
        // solta os aldeões das casas, um por um
        foreach (var house in allHouses)
        {
            if (house != null)
            {
                yield return StartCoroutine(house.ReleaseVillagersOneByOne());
            }
        }
    }

    void WinGame()
    {
        Debug.Log("🎉 Você sobreviveu todos os dias — PARABÉNS!");
    }

    // ==== Gerenciamento de recursos ====
    public void AddFood(int amount)
    {
        food += amount; // adiciona comida
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        wood += amount; // adiciona madeira
        UpdateUI();
    }

    public bool SpendFood(int amount)
    {
        if (food >= amount)
        {
            food -= amount; // gasta comida
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool SpendWood(int amount)
    {
        if (wood >= amount)
        {
            wood -= amount; // gasta madeira
            UpdateUI();
            return true;
        }
        return false;
    }

    public void UpdateUI()
    {
        if (foodText) foodText.text = "Food: " + food;
        if (woodText) woodText.text = "Wood: " + wood;
        if (dayText)
        {
            string texto = "Day: " + Mathf.Min(currentDay, maxDays) + "/" + maxDays;
            texto += isNight ? " (Night)" : " (Day)";
            dayText.text = texto;
        }

        // atualiza o relógio visual
        if (clockText)
        {
            string simbolo = isNight ? "🌙" : "☀️";
            clockText.text = simbolo + " " + currentHour.ToString("00") + ":" + currentMinute.ToString("00");
        }
    }

    // ==== Funções extras ====
    public string GetTimeString()
    {
        return currentHour.ToString("00") + ":" + currentMinute.ToString("00");
    }

    public float GetTimeAsFloat()
    {
        // retorna o horário como número (ex: 19.5 = 19:30)
        return currentHour + (currentMinute / 60f);
    }

    // ==== Procuras ====
    public Farm GetAvailableFarm()
    {
        foreach (var f in farms)
        {
            if (!f.HasFarmer) return f; // acha fazenda sem fazendeiro
        }
        return null;
    }

    public Tree FindNearestAvailableTree(Vector3 pos)
    {
        Tree best = null;
        float bestDist = float.MaxValue;

        foreach (var t in trees)
        {
            if (t.IsDepleted()) continue; // ignora árvore cortada
            float d = Vector3.Distance(pos, t.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    // ==== Interação com aldeões ====
    public void SelectVillager(VillagerController v)
    {
        selectedVillager = v;
        UIManager.Instance.OpenVillagerPanel(v);
    }

    public VillagerController SpawnVillager(Vector3 pos)
    {
        GameObject go = Instantiate(villagerPrefab, pos, Quaternion.identity, villagersParent);
        var v = go.GetComponent<VillagerController>();
        RegisterVillager(v);
        return v;
    }

    // ==== Registro automático ====
    public void RegisterVillager(VillagerController v)
    {
        if (!allVillagers.Contains(v))
        {
            allVillagers.Add(v);
        }
    }

    public void RegisterHouse(House h)
    {
        if (!allHouses.Contains(h))
        {
            allHouses.Add(h);
        }
    }
}
