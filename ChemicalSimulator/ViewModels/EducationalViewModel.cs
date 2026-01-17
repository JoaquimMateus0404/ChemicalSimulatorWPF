using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSimulator.Commands;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para modo educacional com tutoriais e gamificação
    /// </summary>
    public class EducationalViewModel : ViewModelBase
    {
        private string _currentTopic = "Introdução à Química";
        private string _currentContent = "";
        private int _currentStep = 0;
        private int _totalSteps = 5;
        private int _userScore = 0;

        public ObservableCollection<LessonTopic> AvailableTopics { get; }
        public ObservableCollection<string> LessonSteps { get; }

        public string CurrentTopic
        {
            get => _currentTopic;
            set => SetProperty(ref _currentTopic, value);
        }

        public string CurrentContent
        {
            get => _currentContent;
            set => SetProperty(ref _currentContent, value);
        }

        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public int TotalSteps
        {
            get => _totalSteps;
            set => SetProperty(ref _totalSteps, value);
        }

        public int UserScore
        {
            get => _userScore;
            set => SetProperty(ref _userScore, value);
        }

        public double ProgressPercentage => TotalSteps > 0 ? (CurrentStep / (double)TotalSteps) * 100 : 0;

        // Comandos
        public ICommand SelectTopicCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PreviousStepCommand { get; }
        public ICommand ResetProgressCommand { get; }

        public EducationalViewModel()
        {
            AvailableTopics = new ObservableCollection<LessonTopic>
            {
                new LessonTopic { Title = "🔬 Estrutura Atômica", Description = "Aprenda sobre átomos, prótons, nêutrons e elétrons", Difficulty = "Básico" },
                new LessonTopic { Title = "🔗 Ligações Químicas", Description = "Entenda ligações iônicas, covalentes e metálicas", Difficulty = "Intermediário" },
                new LessonTopic { Title = "⚗️ Reações Químicas", Description = "Tipos de reações e como prevê-las", Difficulty = "Intermediário" },
                new LessonTopic { Title = "🌡️ Termodinâmica", Description = "Energia em reações químicas", Difficulty = "Avançado" },
                new LessonTopic { Title = "⚡ Cinética Química", Description = "Velocidade das reações", Difficulty = "Avançado" },
                new LessonTopic { Title = "📐 Geometria Molecular", Description = "Formas das moléculas (VSEPR)", Difficulty = "Intermediário" },
                new LessonTopic { Title = "⚖️ Balanceamento", Description = "Como balancear equações químicas", Difficulty = "Básico" },
                new LessonTopic { Title = "🧪 Ácidos e Bases", Description = "pH e reações ácido-base", Difficulty = "Intermediário" }
            };

            LessonSteps = new ObservableCollection<string>();

            SelectTopicCommand = new RelayCommand<LessonTopic>(SelectTopic);
            NextStepCommand = new RelayCommand(NextStep, CanGoNext);
            PreviousStepCommand = new RelayCommand(PreviousStep, CanGoPrevious);
            ResetProgressCommand = new RelayCommand(ResetProgress);

            LoadDefaultTopic();
        }

        private void LoadDefaultTopic()
        {
            SelectTopic(AvailableTopics[0]);
        }

        private void SelectTopic(LessonTopic? topic)
        {
            if (topic == null) return;

            CurrentTopic = topic.Title;
            CurrentStep = 0;
            LessonSteps.Clear();

            // Carregar conteúdo baseado no tópico
            switch (topic.Title)
            {
                case "🔬 Estrutura Atômica":
                    LoadAtomicStructureLesson();
                    break;
                case "🔗 Ligações Químicas":
                    LoadChemicalBondsLesson();
                    break;
                case "⚗️ Reações Químicas":
                    LoadChemicalReactionsLesson();
                    break;
                case "🌡️ Termodinâmica":
                    LoadThermodynamicsLesson();
                    break;
                case "⚡ Cinética Química":
                    LoadKineticsLesson();
                    break;
                case "📐 Geometria Molecular":
                    LoadMolecularGeometryLesson();
                    break;
                case "⚖️ Balanceamento":
                    LoadBalancingLesson();
                    break;
                case "🧪 Ácidos e Bases":
                    LoadAcidBaseLesson();
                    break;
            }

            TotalSteps = LessonSteps.Count;
            UpdateCurrentContent();
        }

        private void LoadAtomicStructureLesson()
        {
            LessonSteps.Add(@"
📚 ESTRUTURA ATÔMICA - Introdução

Os átomos são as unidades fundamentais da matéria.

Componentes principais:
• Prótons (+): Partículas com carga positiva no núcleo
• Nêutrons (0): Partículas neutras no núcleo
• Elétrons (-): Partículas com carga negativa orbitando o núcleo

Número Atômico (Z) = número de prótons
Massa Atômica (A) = prótons + nêutrons
");

            LessonSteps.Add(@"
⚛️ CAMADAS ELETRÔNICAS

Os elétrons se organizam em camadas (níveis de energia):
• K (n=1): até 2 elétrons
• L (n=2): até 8 elétrons
• M (n=3): até 18 elétrons
• N (n=4): até 32 elétrons

Exemplo - Oxigênio (O):
Z = 8 → 8 prótons, 8 elétrons
Configuração: 1s² 2s² 2p⁴
Distribuição: K=2, L=6
");

            LessonSteps.Add(@"
🎯 ELÉTRONS DE VALÊNCIA

São os elétrons da última camada - determinam as propriedades químicas!

Regra do Octeto:
Átomos tendem a ter 8 elétrons na última camada (como gases nobres)

Exemplos:
• Na (11): 1 elétron de valência → tende a PERDER 1e⁻
• Cl (17): 7 elétrons de valência → tende a GANHAR 1e⁻
• Ne (10): 8 elétrons de valência → ESTÁVEL (gás nobre)
");

            LessonSteps.Add(@"
🔬 EXERCÍCIO PRÁTICO

Analise o elemento Carbono (C):
• Número atômico: 6
• Configuração eletrônica: 1s² 2s² 2p²

Perguntas:
1. Quantos prótons tem o carbono?
2. Quantos elétrons de valência?
3. Quantas ligações o carbono pode fazer?

💡 Dica: A camada de valência é a camada mais externa!
");

            LessonSteps.Add(@"
✅ RESUMO - Estrutura Atômica

Você aprendeu:
✓ Componentes do átomo (prótons, nêutrons, elétrons)
✓ Número atômico e massa atômica
✓ Camadas eletrônicas e distribuição
✓ Elétrons de valência e regra do octeto

🎖️ Parabéns! +50 pontos

Próximo tópico: Ligações Químicas
Descubra como os átomos se conectam!
");
        }

        private void LoadChemicalBondsLesson()
        {
            LessonSteps.Add(@"
🔗 LIGAÇÕES QUÍMICAS

Átomos se unem formando ligações para alcançar estabilidade.

Tipos principais:
1. Ligação Iônica - transferência de elétrons
2. Ligação Covalente - compartilhamento de elétrons
3. Ligação Metálica - mar de elétrons

A eletronegatividade determina o tipo de ligação!
");

            LessonSteps.Add(@"
⚡ LIGAÇÃO IÔNICA

Ocorre entre metal e não-metal (ΔEN > 1.7)

Exemplo: NaCl (sal de cozinha)
• Na perde 1 elétron → Na⁺ (cátion)
• Cl ganha 1 elétron → Cl⁻ (ânion)
• Atração eletrostática forma o cristal

Propriedades:
✓ Sólidos cristalinos
✓ Alto ponto de fusão
✓ Conduzem eletricidade quando dissolvidos
");

            LessonSteps.Add(@"
🤝 LIGAÇÃO COVALENTE

Compartilhamento de pares de elétrons (ΔEN < 1.7)

Tipos:
• Simples: 1 par compartilhado (H-H)
• Dupla: 2 pares compartilhados (O=O)
• Tripla: 3 pares compartilhados (N≡N)

Exemplo: H₂O (água)
   H-O-H
Oxigênio compartilha elétrons com 2 hidrogênios
");

            LessonSteps.Add(@"
📊 ELETRONEGATIVIDADE

Mede a tendência de atrair elétrons:

Escala de Pauling:
F (4.0) > O (3.5) > N (3.0) > C (2.5) > H (2.1)

Diferença de eletronegatividade (ΔEN):
• ΔEN < 0.4: Covalente apolar
• 0.4 < ΔEN < 1.7: Covalente polar
• ΔEN > 1.7: Iônica

💡 Quanto maior ΔEN, mais polar a ligação!
");

            LessonSteps.Add(@"
✅ RESUMO - Ligações Químicas

Você aprendeu:
✓ Tipos de ligações químicas
✓ Ligação iônica vs covalente
✓ Eletronegatividade e polaridade
✓ Ligações simples, duplas e triplas

🎖️ +60 pontos conquistados!

Continue explorando as reações químicas!
");
        }

        private void LoadChemicalReactionsLesson()
        {
            LessonSteps.Add(@"
⚗️ REAÇÕES QUÍMICAS

Transformação de substâncias:
Reagentes → Produtos

Lei de Lavoisier:
'Na natureza nada se cria, nada se perde, tudo se transforma'

A massa total dos reagentes = massa total dos produtos
");

            LessonSteps.Add(@"
🔥 TIPOS DE REAÇÕES

1. Síntese: A + B → AB
   Ex: 2H₂ + O₂ → 2H₂O

2. Decomposição: AB → A + B
   Ex: 2H₂O → 2H₂ + O₂

3. Simples Troca: A + BC → AC + B
   Ex: Zn + 2HCl → ZnCl₂ + H₂

4. Dupla Troca: AB + CD → AD + CB
   Ex: NaCl + AgNO₃ → NaNO₃ + AgCl

5. Combustão: CₓHᵧ + O₂ → CO₂ + H₂O
");

            LessonSteps.Add(@"
⚖️ BALANCEAMENTO

Ajustar coeficientes para igualar átomos dos dois lados.

Exemplo: Combustão do metano
CH₄ + O₂ → CO₂ + H₂O (desbalanceada)

Passo a passo:
1. C: 1 → 1 ✓
2. H: 4 → 2 (precisa de 2H₂O)
3. O: 2 → 4 (precisa de 2O₂)

Balanceada:
CH₄ + 2O₂ → CO₂ + 2H₂O ✓
");

            LessonSteps.Add(@"
✅ RESUMO - Reações Químicas

Você aprendeu:
✓ Conceito de reação química
✓ Lei de conservação da massa
✓ 5 tipos principais de reações
✓ Como balancear equações

🎖️ +70 pontos!

Agora explore a termodinâmica das reações!
");
        }

        private void LoadThermodynamicsLesson()
        {
            LessonSteps.Add(@"
🌡️ TERMODINÂMICA QUÍMICA

Estuda energia em reações químicas.

Conceitos-chave:
• Entalpia (H): conteúdo de calor
• Entropia (S): desordem do sistema
• Energia Livre de Gibbs (G): espontaneidade

ΔG = ΔH - TΔS
");

            LessonSteps.Add(@"
🔥 REAÇÕES EXOTÉRMICAS

Liberam energia (calor) para o ambiente
ΔH < 0 (negativo)

Exemplos:
• Combustão: CH₄ + 2O₂ → CO₂ + 2H₂O + CALOR
• Neutralização: HCl + NaOH → NaCl + H₂O + CALOR
• Respiração celular

Características:
✓ Produtos têm menos energia que reagentes
✓ Temperatura aumenta
✓ Geralmente espontâneas
");

            LessonSteps.Add(@"
🧊 REAÇÕES ENDOTÉRMICAS

Absorvem energia (calor) do ambiente
ΔH > 0 (positivo)

Exemplos:
• Fotossíntese: 6CO₂ + 6H₂O + LUZ → C₆H₁₂O₆ + 6O₂
• Decomposição térmica: CaCO₃ + CALOR → CaO + CO₂
• Dissolução de NH₄NO₃ em água

Características:
✓ Produtos têm mais energia que reagentes
✓ Temperatura diminui
✓ Requerem energia externa
");

            LessonSteps.Add(@"
⚡ ENERGIA LIVRE DE GIBBS

Determina se reação é espontânea:

ΔG = ΔH - TΔS

ΔG < 0: Espontânea (ocorre naturalmente)
ΔG > 0: Não-espontânea (precisa de energia)
ΔG = 0: Equilíbrio

Exemplo:
Gelo derretendo a 25°C
ΔH > 0 (absorve calor)
ΔS > 0 (aumento de desordem)
ΔG < 0 → Espontâneo!
");

            LessonSteps.Add(@"
✅ RESUMO - Termodinâmica

Você aprendeu:
✓ Entalpia, entropia e energia de Gibbs
✓ Reações exotérmicas vs endotérmicas
✓ Critério de espontaneidade
✓ Aplicações práticas

🎖️ +80 pontos! Nível avançado!

Explore a cinética química a seguir!
");
        }

        private void LoadKineticsLesson()
        {
            LessonSteps.Add(@"
⚡ CINÉTICA QUÍMICA

Estuda a velocidade das reações.

Velocidade média = Δconcentração / Δtempo

Fatores que afetam:
• Temperatura
• Concentração
• Catalisadores
• Superfície de contato
");

            LessonSteps.Add(@"
🌡️ TEORIA DAS COLISÕES

Para reagir, moléculas devem:
1. Colidir
2. Com energia suficiente (Ea)
3. Na orientação correta

Energia de Ativação (Ea):
Energia mínima necessária para iniciar a reação

Catalisadores:
Reduzem Ea, acelerando a reação!
");

            LessonSteps.Add(@"
📈 LEI DE ARRHENIUS

k = A · e^(-Ea/RT)

k: constante de velocidade
A: fator pré-exponencial
Ea: energia de ativação
R: constante dos gases (8.314 J/mol·K)
T: temperatura (K)

Conclusão:
Aumentar T → Aumenta k → Reação mais rápida!
");

            LessonSteps.Add(@"
✅ RESUMO - Cinética Química

Você aprendeu:
✓ Conceito de velocidade de reação
✓ Teoria das colisões
✓ Energia de ativação
✓ Lei de Arrhenius
✓ Efeito da temperatura

🎖️ +90 pontos! Expert!

Continue dominando a química!
");
        }

        private void LoadMolecularGeometryLesson()
        {
            LessonSteps.Add(@"
📐 GEOMETRIA MOLECULAR

Teoria VSEPR (Repulsão dos Pares de Elétrons)
Pares eletrônicos se repelem → geometria específica

Determina:
• Forma 3D da molécula
• Polaridade
• Propriedades físicas
");

            LessonSteps.Add(@"
🔷 GEOMETRIAS COMUNS

Linear (180°): CO₂, HCN
   O=C=O

Angular (~105°): H₂O
   H-O-H

Trigonal Planar (120°): BF₃
      F
     /
  B-F
     \
      F

Tetraédrica (~109°): CH₄
      H
      |
  H-C-H
      |
      H
");

            LessonSteps.Add(@"
💧 POLARIDADE MOLECULAR

Molécula polar: distribuição assimétrica de cargas

Exemplos:
• H₂O: polar (angular + O muito eletronegativo)
• CO₂: apolar (linear, vetores se anulam)
• NH₃: polar (piramidal)
• CH₄: apolar (tetraédrica simétrica)

Polaridade afeta:
✓ Solubilidade
✓ Ponto de ebulição
✓ Interações intermoleculares
");

            LessonSteps.Add(@"
✅ RESUMO - Geometria Molecular

Você aprendeu:
✓ Teoria VSEPR
✓ Principais geometrias moleculares
✓ Relação entre geometria e polaridade
✓ Impacto nas propriedades

🎖️ +75 pontos!

Excelente progresso!
");
        }

        private void LoadBalancingLesson()
        {
            LessonSteps.Add(@"
⚖️ BALANCEAMENTO DE EQUAÇÕES

Lei de Lavoisier: massa se conserva!

Objetivo: Igualar número de átomos de cada elemento nos dois lados

Método:
1. Contar átomos
2. Ajustar coeficientes
3. Verificar balanceamento
");

            LessonSteps.Add(@"
📝 MÉTODO DAS TENTATIVAS

Exemplo: Fe + O₂ → Fe₂O₃

Passo 1: Contar átomos
Fe: 1 → 2 (desbalanceado)
O: 2 → 3 (desbalanceado)

Passo 2: Ajustar
2Fe + O₂ → Fe₂O₃ (Fe ok, O não)
2Fe + 3/2 O₂ → Fe₂O₃

Passo 3: Eliminar fração
4Fe + 3O₂ → 2Fe₂O₃ ✓
");

            LessonSteps.Add(@"
🔥 EXEMPLO: COMBUSTÃO

C₃H₈ + O₂ → CO₂ + H₂O

Estratégia:
1. Balancear C: C₃H₈ + O₂ → 3CO₂ + H₂O
2. Balancear H: C₃H₈ + O₂ → 3CO₂ + 4H₂O
3. Balancear O: C₃H₈ + 5O₂ → 3CO₂ + 4H₂O ✓

Verificação:
C: 3 = 3 ✓
H: 8 = 8 ✓
O: 10 = 10 ✓
");

            LessonSteps.Add(@"
✅ RESUMO - Balanceamento

Você aprendeu:
✓ Importância do balanceamento
✓ Método das tentativas
✓ Estratégias para diferentes tipos
✓ Verificação do resultado

🎖️ +65 pontos!

Pratique com diferentes reações!
");
        }

        private void LoadAcidBaseLesson()
        {
            LessonSteps.Add(@"
🧪 ÁCIDOS E BASES

Definição de Arrhenius:
• Ácido: libera H⁺ em água
• Base: libera OH⁻ em água

Exemplos:
Ácidos: HCl, H₂SO₄, HNO₃
Bases: NaOH, KOH, Ca(OH)₂
");

            LessonSteps.Add(@"
📊 ESCALA DE pH

pH = -log[H⁺]

0 ─────── 7 ─────── 14
Ácido   Neutro   Básico

Exemplos:
pH 1: HCl (ácido forte)
pH 7: H₂O pura (neutro)
pH 14: NaOH (base forte)

Relação:
pH + pOH = 14
");

            LessonSteps.Add(@"
⚗️ NEUTRALIZAÇÃO

Ácido + Base → Sal + Água

Exemplo:
HCl + NaOH → NaCl + H₂O
H⁺ + OH⁻ → H₂O

Características:
✓ Exotérmica (libera calor)
✓ pH tende a 7
✓ Forma sal + água
");

            LessonSteps.Add(@"
✅ RESUMO - Ácidos e Bases

Você aprendeu:
✓ Definição de ácidos e bases
✓ Escala de pH
✓ Reações de neutralização
✓ Aplicações práticas

🎖️ +70 pontos!

Parabéns por concluir todos os tópicos!
");
        }

        private bool CanGoNext() => CurrentStep < TotalSteps - 1;

        private void NextStep()
        {
            if (!CanGoNext()) return;

            CurrentStep++;
            UpdateCurrentContent();

            // Gamificação: ganhar pontos
            UserScore += 10;
        }

        private bool CanGoPrevious() => CurrentStep > 0;

        private void PreviousStep()
        {
            if (!CanGoPrevious()) return;

            CurrentStep--;
            UpdateCurrentContent();
        }

        private void UpdateCurrentContent()
        {
            if (CurrentStep >= 0 && CurrentStep < LessonSteps.Count)
            {
                CurrentContent = LessonSteps[CurrentStep];
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        private void ResetProgress()
        {
            CurrentStep = 0;
            UserScore = 0;
            UpdateCurrentContent();
        }
    }

    public class LessonTopic
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Difficulty { get; set; } = "";
    }
}
