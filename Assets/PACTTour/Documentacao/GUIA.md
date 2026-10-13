PACT — ATUALIZAÇÃO V4

1. Sai do Play, guarda o trabalho e fecha o Unity.
2. Extrai este ZIP.
3. Copia PACTTour e PACTTour.meta, da pasta Assets extraída, para
   C:\UnityProjects\Tour360\Assets. Aceita substituir ficheiros.
   Não apagues a pasta Generated que já existe no teu projeto.
4. Abre o Unity e aguarda a compilação.
5. Menu superior: PACT > Criar visita V4. Depois carrega em Play.

Não uses Import Package. É necessário criar a cena V4 para configurar
as fotografias, o menu e o novo guia. As cenas anteriores não são atualizadas.

COMO USAR
O menu apresenta as três fotografias exteriores em slideshow.
Seleciona Começar visita para ouvir as boas-vindas.
No computador: botão direito + arrastar para olhar; esquerdo para selecionar.
Em VR: apontar e selecionar com o comando, ou olhar fixamente para o alvo
quando não existir comando ativo. A configuração XR existente é preservada.

Mantêm-se três posições no corredor, ligadas por anéis brancos discretos.
Ordem corrigida: ponto01 > ponto03 > ponto02; regresso ponto02 > ponto03 > ponto01.
Os nomes dos ficheiros originais foram preservados; os destinos foram corrigidos.
O título agora é apenas PACT / Corredor, sem Future Lab.
No primeiro ponto, olha para trás e à esquerda, junto à porta C1.01:
Future Lab. Seleciona a pequena esfera sob a identificação para ouvir o guia.
A esfera só existe nessa posição do corredor. Não é um quarto ponto panorâmico.
O guia desaparece no fim da apresentação; Repetir volta a apresentar.
Menu regressa ao slideshow. Fechar guia interrompe voz e legendas.

O QUE ESTÁ IMPLEMENTADO
- Slideshow com três fotos reais enviadas, transição suave e botão inicial.
- Importação explícita das texturas e referências diretas às três panorâmicas.
- Identificação Future Lab e alvo independente dos pontos de navegação.
- Corpo 3D estilizado com roupa ajustada, cabeça com projeção da foto e boca animada
  pelo volume do áudio. Braços estáveis e movimento muito ligeiro da cabeça.
  É um protótipo, não uma reconstrução fiel do professor nem o realismo do vídeo.
- Legendas paginadas, com tamanho limitado e controlos separados.

LIMITES
A voz incluída é sintética e provisória; NÃO é a voz de Soumodip Sarkar.
Foi encontrada uma gravação pública, mas não foi clonada a voz. Ver FONTES_VOZ.txt.
Os movimentos de boca não são sincronização por fonemas.
Falta uma fotografia 360 do interior do Future Lab: não há entrada virtual
no interior. A proximidade é definida pela posição panorâmica do corredor,
não pela distância física que o utilizador percorre com os óculos.
O conteúdo sobre equipamentos do Future Lab é um guião provisório a confirmar.
Os shaders destinam-se ao Built-in Render Pipeline do projeto original.
Não foi possível executar Unity nem testar nos óculos neste ambiente.

EDITAR SALAS / EMPRESAS
Seleciona Assets/PACTTour/Generated/VisitaV4/VisitaPACT.asset (ou VisitaV4 1,
se já criaste outra versão). Cada Node representa uma posição panorâmica.
Room Signs contém título, subtítulo, yaw (direção), distância, altura,
texto e áudio da apresentação de cada sala ou empresa.
Para uma futura sala com interior, acrescenta outro Node, atribui uma
panorâmica Texture2D e liga-o pelos índices de Links. Voice aceita WAV/MP3
importado no Unity. A narração de sala pode ser automática com Show Guide,
Narration, Voice, Room Id e Narrate On Entry.

VERIFICAR NO UNITY
Menu com três imagens; Começar abre panorama; boas-vindas terminam;
percurso 0 > 1 > 2 > 1 > 0; ponto Future Lab apenas no ponto 0;
selecionar Future Lab; Repetir; Fechar guia; Menu; começar novamente.
Confirmar legibilidade e conforto em VR e ausência de erros na Console.
