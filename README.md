 Scripts principais
GameManager.cs

Cuida das coisas gerais do jogo.
Guarda a quantidade de madeira, comida e o número do dia atual.

PlacementManager.cs

Usado pra colocar construções (como casas e fazendas) no mapa.
Verifica se o jogador tem recursos suficientes pra construir.

VillagerJob.cs

Script base pra os trabalhos dos aldeões.
Outros scripts de trabalho herdam dele.

LumberjackJob.cs

Faz o aldeão trabalhar como lenhador.
Corta árvores, gasta comida e dá madeira depois de um tempo.

VillagerController.cs

Controla o movimento e o comportamento dos aldeões.
Eles andam até o local de trabalho e voltam pra vila.

Building.cs

Script base pras construções.
Pode ter tipos diferentes como casa ou fazenda.

Recursos usados

UnityEngine

NavMesh (pra movimentar aldeões)

TextMeshPro (pra mostrar texto na tela)

 Como jogar

Use os botões pra construir casas ou fazendas

Tenha comida suficiente pros aldeões

Mantenha os recursos equilibrados

Faça a vila crescer o máximo que conseguir

 Feito por vinicius dis santos, amanda, kaio cezar, mikhael e lucas baron

Projeto criado pra estudo e prática com Unity e C#.
