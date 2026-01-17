# 🧪 Molecule Builder - Documentação Completa

## 📋 Visão Geral

O **Molecule Builder** é um módulo profissional e educativo para construção, análise e validação de moléculas químicas. Implementado com arquitetura MVVM e integrado ao **ElementDataLoader** e **CompoundDataLoader** existentes.

---

## ✨ Funcionalidades Implementadas

### 🎯 **Funcionalidades Essenciais**

#### ✅ **Criação de Moléculas**
- Adição de átomos por clique no canvas
- Seleção de elementos da tabela periódica (H, C, N, O, F, P, S, Cl, Br, I, Na, K, Ca, Mg)
- Criação de ligações entre átomos

#### ✅ **Tipos de Ligação**
- ⚪ **Simples** (Single)
- ⚪⚪ **Dupla** (Double)
- ⚪⚪⚪ **Tripla** (Triple)
- ⚛️ **Iônica** (Ionic)

#### ✅ **Validação de Valência**
- Verificação automática da valência de cada átomo
- Prevenção de ligações inválidas
- Avisos em tempo real

#### ✅ **Edição de Moléculas**
- Remoção de átomos
- Remoção de ligações
- Seleção e exclusão de componentes

#### ✅ **Visualização**
- **2D**: Canvas interativo com renderização vetorial
- **3D**: Visualizador Helix Toolkit com rotação e zoom
- Alternância entre vistas 2D/3D

#### ✅ **Zoom, Pan e Rotação**
- Zoom In/Out
- Reset de Zoom
- Rotação 3D automática

---

### 🎓 **Funcionalidades Profissionais**

#### ⚛️ **Análise Química Automática**

| Propriedade | Descrição | Exemplo |
|-------------|-----------|---------|
| **Fórmula Molecular** | Fórmula de Hill (C, H, alfabética) | C₂H₆O |
| **Massa Molar** | Soma das massas atômicas | 46.07 g/mol |
| **Número de Ligações** | Total de bonds | 8 |
| **Polaridade** | Calculada via momento dipolar | Polar/Apolar |
| **Geometria Molecular** | VSEPR | Tetrahedral |
| **Tipo de Molécula** | Classificação | Orgânica/Inorgânica |

#### 🧠 **Validação Inteligente**

**Níveis de Severidade:**
- ❌ **Error**: Valência excedida (molécula inválida)
- ⚠️ **Warning**: Átomos desconectados
- ℹ️ **Info**: Pode aceitar mais ligações

**Avisos Específicos:**
- Valência de átomos individuais
- Ligações impossíveis
- Estruturas instáveis
- Destaque visual em vermelho para erros

#### 🧩 **Templates de Moléculas**

**Carregados do CompoundDataLoader:**
- Água (H₂O)
- Dióxido de Carbono (CO₂)
- Metano (CH₄)
- Etanol (C₂H₆O)
- Amônia (NH₃)
- Ácido Clorídrico (HCl)
- Cloreto de Sódio (NaCl)
- Oxigênio (O₂)
- Nitrogênio (N₂)

**Características:**
- Carregamento com 1 clique
- Geometria pré-configurada
- Descrição educacional

---

### 🎨 **Funcionalidades Criativas**

#### 💡 **Modo Didático**
- Explicações em tempo real
- Feedback educacional sobre:
  - Por que uma ligação não é possível
  - Valências típicas de elementos
  - Propriedades das moléculas criadas
- Mensagens contextuais

#### 🎨 **Cores por Elemento (CPK)**
| Elemento | Cor |
|----------|-----|
| H | Branco |
| C | Preto |
| N | Azul |
| O | Vermelho |
| F | Verde |
| S | Amarelo |
| Cl | Verde Claro |
| P | Laranja |
| Br | Marrom |
| I | Roxo |

#### 🔁 **Histórico de Alterações (Undo/Redo)**
- Stack-based implementation
- Suporta:
  - Adição/remoção de átomos
  - Adição/remoção de ligações
- Atalhos: **Ctrl+Z** (Undo), **Ctrl+Y** (Redo)

---

## 🏗️ Arquitetura

### **MVVM Pattern**

```
Views/
├── MoleculeBuilderView.xaml          (UI)
└── MoleculeBuilderView.xaml.cs       (Code-behind para renderização)

ViewModels/
└── MoleculeBuilderViewModel.cs       (Lógica de negócio)

Models/
├── Molecule.cs
├── Atom.cs
├── Bond.cs
├── Element.cs
├── ValidationWarning.cs
└── MoleculeAction.cs

Services/
├── ChemistryEngine.cs                (Cálculos químicos)
├── ElementDataLoader.cs              (Carrega elementos)
└── CompoundDataLoader.cs             (Carrega compostos)
```

---

## 🔧 Integração com Data Loaders

### **ElementDataLoader**
```csharp
// Carrega elementos do JSON ou fallback
var allElements = _elementDataLoader.LoadElements();

// Busca elemento específico
var hydrogen = _elementDataLoader.GetElementBySymbol("H");
var carbon = _elementDataLoader.GetElementByAtomicNumber(6);
```

### **CompoundDataLoader**
```csharp
// Carrega compostos do JSON
var compounds = _compoundDataLoader.LoadCompounds();

// Filtra templates adequados
var templates = compounds
    .Where(c => IsGoodTemplate(c))
    .Take(20);
```

**Benefícios:**
- ✅ Dados centralizados
- ✅ Manutenção simplificada
- ✅ Suporte a fallback
- ✅ Cache automático

---

## ⌨️ Atalhos de Teclado

| Atalho | Ação |
|--------|------|
| **Ctrl+Z** | Desfazer |
| **Ctrl+Y** | Refazer |
| **Ctrl+S** | Salvar molécula |
| **Ctrl+N** | Nova molécula |
| **Ctrl+E** | Exportar |
| **F1** | Tutorial |
| **Delete** | Excluir selecionado |

---

## 🎨 Layout da Interface

### **Painel Esquerdo (280px)**
- 🎓 Modo Didático (Toggle)
- ⚛️ Paleta de Elementos (Grid)
- 🔗 Tipo de Ligação (Radio Buttons)
- 📚 Templates de Moléculas (ListBox)
- ⚡ Ações Rápidas (Undo/Redo/Clear/Tutorial)

### **Área Central (Expansível)**
- 🛠️ Toolbar (Salvar, Exportar, Zoom, 2D/3D)
- 🖼️ Canvas 2D/Viewport 3D
- 📊 Status Bar (Contadores)

### **Painel Direito (320px)**
- 📊 **Análise Química**
  - Fórmula molecular
  - Massa molar
  - Polaridade
  - Geometria
  - Tipo de molécula
- ✅ **Validação**
  - Lista de avisos/erros
  - Validação manual
- 📍 **Átomo Selecionado**
  - Informações detalhadas
  - Botão de remoção
- 💾 **Salvar & Exportar**
  - Salvar molécula
  - Salvar como template
  - Exportar

---

## 🎯 Fluxo de Uso

### **1. Criar Molécula**
1. Ativar Modo Didático (opcional)
2. Selecionar elemento (ex: C)
3. Clicar no canvas para adicionar átomo
4. Repetir para outros átomos

### **2. Adicionar Ligações**
1. Selecionar tipo de ligação (Simples/Dupla/Tripla)
2. Clicar no primeiro átomo
3. Clicar no segundo átomo
4. Ligação é validada e criada

### **3. Analisar**
- Verificar painel direito para:
  - Fórmula calculada
  - Massa molar
  - Avisos de validação
  - Geometria molecular

### **4. Usar Templates**
1. Selecionar template na lista
2. Clicar em "Carregar Template"
3. Molécula é criada automaticamente

---

## 🧪 Validação Química

### **Regras de Valência**

| Elemento | Valência Típica | Exemplo |
|----------|-----------------|---------|
| H | 1 | H-H, H-O |
| C | 4 | CH₄ |
| N | 3 | NH₃ |
| O | 2 | H₂O |
| F, Cl, Br, I | 1 | HCl |
| S | 2 | H₂S |
| P | 3 | PH₃ |

### **Cálculo de Ordem de Ligação**
- Simples = 1
- Dupla = 2
- Tripla = 3

**Exemplo:** C=O (ordem 2), C≡N (ordem 3)

---

## 🔬 Geometria Molecular (VSEPR)

| # Ligações | # Pares Livres | Geometria |
|------------|----------------|-----------|
| 2 | 0 | Linear (CO₂) |
| 2 | 1-2 | Angular (H₂O) |
| 3 | 0 | Trigonal Planar |
| 3 | 1 | Piramidal (NH₃) |
| 4 | 0 | Tetraédrica (CH₄) |
| 5 | 0 | Bipiramidal Trigonal |
| 6 | 0 | Octaédrica |

---

## 📊 Renderização

### **Canvas 2D**
- **Átomos**: Círculos coloridos (40px diâmetro)
- **Ligações**:
  - Simples: 1 linha
  - Dupla: 2 linhas paralelas (±3px)
  - Tripla: 3 linhas paralelas (±5px)
- **Interatividade**: Hover, Click, Drag

### **Viewport 3D (Helix Toolkit)**
- **Átomos**: SphereVisual3D (raio 0.5)
- **Ligações**: PipeVisual3D (diâmetro 0.1)
- **Câmera**: Perspectiva (45° FOV)
- **Iluminação**: DefaultLights

---

## 🚀 Melhorias Futuras

### **Em Desenvolvimento**
- [ ] Exportação para formatos:
  - MOL/SDF (chemical file formats)
  - PNG/SVG (imagens)
  - JSON (dados)
- [ ] Salvamento de templates personalizados
- [ ] Biblioteca de moléculas salvas
- [ ] Cálculos termodinâmicos avançados
- [ ] Otimização de geometria 3D
- [ ] Animações de formação/quebra de ligações

### **Ideias Futuras**
- [ ] Isômeros (estrutural, geométrico)
- [ ] Ressonância
- [ ] Orbitais moleculares
- [ ] Simulação de reações
- [ ] Integração com ChemDraw
- [ ] Modo multiplayer (colaborativo)

---

## 📚 Recursos Educacionais

### **Mensagens Didáticas**
O modo educacional fornece feedback contextual:

**Exemplo 1: Valência**
```
💡 O Oxigênio geralmente forma 2 ligação(ões).
Você tentou formar 3 ligação(ões).
```

**Exemplo 2: Sucesso**
```
✅ Ligação Double criada!
Energia: 799.0 kJ/mol
Comprimento: 1.21 Å
```

**Exemplo 3: Template**
```
📚 Template carregado: Água (H₂O)
Molécula de água - essencial para a vida
```

---

## 🎓 Tutorial Integrado

Acessível via **F1** ou botão "Tutorial":

```
🎓 TUTORIAL - MOLECULE BUILDER

1️⃣ Selecione um elemento na paleta
2️⃣ Clique no canvas para adicionar átomos
3️⃣ Clique em dois átomos para criar ligações
4️⃣ Use o tipo de ligação (Simples/Dupla/Tripla)
5️⃣ Analise as propriedades no painel direito
6️⃣ Verifique avisos de validação

⌨️ ATALHOS:
Ctrl+Z: Desfazer
Ctrl+Y: Refazer
Ctrl+S: Salvar
Ctrl+N: Nova molécula
Delete: Excluir selecionado
```

---

## 💻 Exemplo de Código

### **Adicionar Átomo Programaticamente**
```csharp
var carbon = _elementDataLoader.GetElementBySymbol("C");
SelectedElement = carbon;
AddAtom(new Point(200, 200));
```

### **Criar Ligação**
```csharp
SelectedBondType = BondType.Double;
CreateBond(Atoms[0], Atoms[1]);
```

### **Validar Molécula**
```csharp
ValidateMolecule();
var isValid = IsValid; // bool
var warnings = ValidationWarnings; // ObservableCollection
```

---

## 📞 Suporte

Para questões ou sugestões sobre o Molecule Builder:
- 📧 Criar issue no repositório
- 📖 Consultar EDUCATIONAL_MODE_GUIDE.md
- 🔧 Ver MOLECULE_BUILDER_ENHANCEMENTS.md

---

**Versão:** 1.0  
**Data:** Janeiro 2026  
**Status:** ✅ Produção  
**Licença:** MIT
