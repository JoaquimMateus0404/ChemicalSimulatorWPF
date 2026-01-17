# 🏗️ Arquitetura do Chemical Simulator - Pós-Refatoração

## 📊 Diagrama de Camadas

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                       │
│                            (Views)                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ MainWindow   │  │ MoleculeView │  │SimulationView│          │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │
│         │                  │                  │                   │
└─────────┼──────────────────┼──────────────────┼──────────────────┘
          │                  │                  │
          │     Data Binding │                  │
          ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                        VIEW MODEL LAYER                          │
│                                                                   │
│  ┌────────────────────┐                                         │
│  │  ViewModelBase     │◄────────────────┐                      │
│  │  (Abstract Base)   │                 │                      │
│  └────────────────────┘                 │                      │
│           ▲                              │                      │
│           │ Inheritance                  │                      │
│           │                              │                      │
│  ┌────────┴────────┬──────────────┬─────┴──────────┐          │
│  │                 │              │                 │          │
│  │  MainViewModel  │  MoleculeVM  │  SimulationVM  │          │
│  │                 │              │                 │          │
│  └────────┬────────┴──────┬───────┴─────┬──────────┘          │
│           │               │             │                      │
└───────────┼───────────────┼─────────────┼──────────────────────┘
            │               │             │
            │   Dependency  │             │
            │   Injection   │             │
            ▼               ▼             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                        │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              ServiceLocator (Singleton)                   │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ Registered Services:                               │  │  │
│  │  │  • ChemistryEngine                                 │  │  │
│  │  │  • ReactionPredictor                               │  │  │
│  │  │  • ExportService                                   │  │  │
│  │  │  • ElementDataLoader                               │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Command Layer                          │  │
│  │  ┌─────────────┐        ┌──────────────┐                │  │
│  │  │ RelayCommand│        │RelayCommand<T>│                │  │
│  │  └─────────────┘        └──────────────┘                │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
└───────────────────────────────┬───────────────────────────────── ┘
                                │
                                │ Uses
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SERVICE LAYER                             │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Chemistry   │  │  Reaction    │  │   Export     │          │
│  │   Engine     │  │  Predictor   │  │   Service    │          │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │
│         │                  │                  │                   │
│         └──────────────────┼──────────────────┘                   │
│                            │                                      │
└────────────────────────────┼──────────────────────────────────────┘
                             │ Uses
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        HELPER LAYER                              │
│                                                                   │
│  ┌──────────────────────┐      ┌──────────────────────┐        │
│  │ ChemistryCalculator  │      │   ElementFactory     │        │
│  │  (Static Helper)     │      │  (Static Factory)    │        │
│  │                      │      │                      │        │
│  │  • CalculateMass    │      │  • CreateElement    │        │
│  │  • BondLength       │      │  • CreateMolecule   │        │
│  │  • BondEnergy       │      │  • DetermineCategory│        │
│  │  • Formula          │      │                      │        │
│  └──────────────────────┘      └──────────────────────┘        │
│                                                                   │
└───────────────────────────┬───────────────────────────────────── ┘
                            │ Operates on
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         MODEL LAYER                              │
│                                                                   │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ Molecule │  │ Element  │  │   Bond   │  │ Reaction │       │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘       │
│                                                                   │
│  ┌──────────┐  ┌──────────────────┐                            │
│  │   Atom   │  │ ReactionConditions│                            │
│  └──────────┘  └──────────────────┘                            │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Dados

```
User Interaction
       │
       ▼
┌─────────────┐
│    View     │
└──────┬──────┘
       │ Data Binding
       ▼
┌─────────────┐
│  ViewModel  │
└──────┬──────┘
       │ Gets Service
       ▼
┌─────────────────┐
│ ServiceLocator  │
└──────┬──────────┘
       │ Returns Instance
       ▼
┌─────────────┐
│   Service   │
└──────┬──────┘
       │ Uses Helper
       ▼
┌─────────────┐
│   Helper    │
└──────┬──────┘
       │ Operates on
       ▼
┌─────────────┐
│    Model    │
└─────────────┘
```

---

## 🎯 Padrões de Design Aplicados

### 1. MVVM (Model-View-ViewModel)
```
View ←→ ViewModel ←→ Model
  ↕                    ↕
Binding             Service
```

### 2. Factory Pattern
```
ElementFactory.CreateElement()
      │
      ├─→ Determina categoria
      ├─→ Define propriedades
      └─→ Retorna Element
```

### 3. Singleton Pattern
```
ServiceLocator.Instance
      │
      ├─→ Única instância
      ├─→ Thread-safe (Lazy)
      └─→ Gerencia serviços
```

### 4. Command Pattern
```
RelayCommand
      │
      ├─→ Execute(Action)
      ├─→ CanExecute(Func<bool>)
      └─→ ICommand implementation
```

### 5. Template Method Pattern
```
ViewModelBase
      │
      ├─→ OnPropertyChanged()
      ├─→ SetProperty<T>()
      └─→ Derived classes override
```

---

## 🔗 Dependências entre Camadas

```
┌─────────────────────────────────────────┐
│ Presentation Layer (Views)              │
└────────────┬────────────────────────────┘
             │ depends on
             ▼
┌─────────────────────────────────────────┐
│ ViewModel Layer                         │
└────────────┬────────────────────────────┘
             │ depends on
             ▼
┌─────────────────────────────────────────┐
│ Infrastructure (ServiceLocator)         │
└────────────┬────────────────────────────┘
             │ provides
             ▼
┌─────────────────────────────────────────┐
│ Service Layer                           │
└────────────┬────────────────────────────┘
             │ uses
             ▼
┌─────────────────────────────────────────┐
│ Helper Layer                            │
└────────────┬────────────────────────────┘
             │ operates on
             ▼
┌─────────────────────────────────────────┐
│ Model Layer                             │
└─────────────────────────────────────────┘
```

---

## 📦 Estrutura de Pastas Atualizada

```
ChemicalSimulator/
│
├── 📁 Commands/                    ✨ NOVO
│   └── RelayCommand.cs            # ICommand implementation
│
├── 📁 Converters/
│   ├── ValueConverterBase.cs      ✨ NOVO
│   ├── BooleanToColorConverter.cs
│   ├── ElementCategoryToColorConverter.cs
│   └── ... (outros converters)
│
├── 📁 Controls/
│   ├── MoleculeViewer3D.xaml
│   └── PeriodicTableControl.xaml
│
├── 📁 Helpers/                     ✨ NOVO
│   ├── ChemistryCalculator.cs     # Cálculos químicos
│   └── ElementFactory.cs          # Factory de elementos
│
├── 📁 Infrastructure/              ✨ NOVO
│   └── ServiceLocator.cs          # DI Container
│
├── 📁 Models/
│   ├── Atom.cs
│   ├── Bond.cs
│   ├── Element.cs
│   ├── Molecule.cs
│   ├── Reaction.cs
│   ├── ReactionComponent.cs
│   └── ReactionConditions.cs
│
├── 📁 Resources/
│   ├── Data/
│   │   └── ElementsData.json
│   └── Styles/
│       └── CustomStyles.xaml
│
├── 📁 Services/
│   ├── ChemistryEngine.cs
│   ├── ElementDataLoader.cs
│   ├── ExportService.cs
│   └── ReactionPredictor.cs
│
├── 📁 ViewModels/
│   ├── ViewModelBase.cs           ✨ NOVO
│   ├── MainViewModel.cs           🔧 REFATORADO
│   ├── MoleculeBuilderViewModel.cs 🔧 REFATORADO
│   └── SimulationViewModel.cs     🔧 REFATORADO
│
├── 📁 Views/
│   ├── MainWindow.xaml
│   ├── MoleculeBuilderView.xaml
│   └── SimulationView.xaml
│
├── App.xaml
├── App.xaml.cs
│
├── 📄 REFACTORING_GUIDE.md        ✨ NOVO
├── 📄 REFACTORING_SUMMARY.md      ✨ NOVO
└── 📄 ARCHITECTURE.md             ✨ NOVO (este arquivo)
```

---

## 🎨 Separação de Responsabilidades

### ViewModels
- ✅ Gerenciar estado da UI
- ✅ Expor dados para binding
- ✅ Implementar Commands
- ❌ Cálculos complexos
- ❌ Acesso direto a dados
- ❌ Lógica de negócio pesada

### Services
- ✅ Lógica de negócio
- ✅ Operações complexas
- ✅ Coordenação entre helpers
- ❌ Conhecimento da UI
- ❌ Dependência de Views

### Helpers
- ✅ Funções utilitárias
- ✅ Cálculos específicos
- ✅ Métodos estáticos reutilizáveis
- ❌ Estado mutável
- ❌ Dependências externas

### Models
- ✅ Estrutura de dados
- ✅ Propriedades simples
- ✅ Validação básica
- ❌ Lógica de negócio
- ❌ Cálculos complexos

---

## 🔐 Princípios de Design

### 1. Baixo Acoplamento
- ViewModels não conhecem Views específicas
- Services não conhecem ViewModels
- Helpers são independentes
- Models são POCOs (Plain Old CLR Objects)

### 2. Alta Coesão
- Cada classe tem uma responsabilidade clara
- Métodos relacionados agrupados
- Separação lógica por namespace

### 3. Testabilidade
- Serviços injetáveis via ServiceLocator
- Métodos estáticos facilmente testáveis
- ViewModels desacoplados de UI

### 4. Manutenibilidade
- Código organizado em camadas
- Documentação XML
- Nomes descritivos
- Estrutura clara de pastas

---

## 📈 Benefícios da Nova Arquitetura

### Performance
- ✅ Serviços singleton (menos objetos)
- ✅ Cálculos otimizados em helpers
- ✅ Menos duplicação de código

### Manutenibilidade
- ✅ Fácil localizar código
- ✅ Mudanças isoladas por camada
- ✅ Documentação clara

### Testabilidade
- ✅ Helpers estáticos facilmente testáveis
- ✅ Services injetáveis
- ✅ ViewModels sem dependências concretas

### Escalabilidade
- ✅ Fácil adicionar novos ViewModels
- ✅ Fácil adicionar novos Services
- ✅ Estrutura preparada para crescimento

---

**Arquitetura Versão 2.0**  
Chemical Simulator - Janeiro 2026  
Padrões Profissionais Aplicados ✨
