# 🎨 Guia de Funcionalidades 3D - Molecule Builder

## 📋 Visão Geral

O Molecule Builder agora possui funcionalidades 3D avançadas para visualização e análise de moléculas em três dimensões.

---

## ✨ Funcionalidades Implementadas

### 1️⃣ **Controles Interativos de Câmera**
- ✅ **Rotação**: Arraste com o mouse para rotacionar a molécula
- ✅ **Zoom**: Use a roda do mouse para aproximar/afastar
- ✅ **Pan**: Clique com o botão direito e arraste para mover
- ✅ **ViewCube**: Cubo de navegação no canto superior direito
- ✅ **Sistema de Coordenadas**: Eixos X, Y, Z visíveis

### 2️⃣ **Estilos de Visualização**
Escolha entre 4 estilos diferentes:

#### ⚛️ **Ball and Stick** (Padrão)
- Átomos como esferas de tamanho médio
- Ligações como cilindros
- Melhor para ver a estrutura geral

#### 🔮 **Space Filling**
- Átomos grandes (raio de van der Waals)
- Mostra o volume real da molécula
- Útil para ver impedimento estérico

#### 📏 **Wireframe**
- Átomos pequenos, ligações finas
- Visual minimalista
- Bom para moléculas grandes

#### 🎯 **CPK Colors**
- Cores padrão CPK para átomos
- H = Branco, C = Preto, N = Azul, O = Vermelho, etc.

### 3️⃣ **Controles de Tamanho**
- **Slider de Tamanho dos Átomos**: 0.2 - 2.0 unidades
- **Slider de Espessura das Ligações**: 0.05 - 0.3 unidades
- Ajustes em tempo real

### 4️⃣ **Auto-Rotação** 🔄
- Ative/desative rotação automática
- Velocidade constante e suave
- Útil para apresentações

### 5️⃣ **Medição de Distâncias** 📏
Como usar:
1. Clique no botão "📏 Medir Distância"
2. Clique em 2 átomos na visualização 3D
3. A distância será exibida em Angstroms (Å)
4. Exemplo: `C-O: 1.43 Å`

### 6️⃣ **Medição de Ângulos** 📐
Como usar:
1. Clique no botão "📐 Medir Ângulo"
2. Clique em 3 átomos (o segundo é o vértice)
3. O ângulo será exibido em graus (°)
4. Exemplo: `H-O-H: 104.5°`

### 7️⃣ **Etiquetas de Átomos** 🏷️
- Checkbox "Mostrar Etiquetas"
- Exibe o símbolo químico sobre cada átomo
- Texto orientado sempre para a câmera (billboard)
- Útil para identificação rápida

### 8️⃣ **Reset de Câmera** 🎯
- Botão "Reset Câmera"
- Retorna para a vista padrão
- Posição: (10, 10, 10)
- Útil quando você se perde na navegação

---

## 🎮 Controles do Mouse

| Ação | Controle |
|------|----------|
| **Rotacionar** | Clique esquerdo + Arrastar |
| **Zoom** | Roda do mouse |
| **Pan (Mover)** | Clique direito + Arrastar |
| **Selecionar Átomo** | Clique esquerdo (no modo medição) |
| **Reset Vista** | Botão no meio do mouse |

---

## ⚙️ Configurações Recomendadas

### Para Moléculas Pequenas (H₂O, NH₃, CH₄)
- **Estilo**: Ball and Stick
- **Tamanho Átomos**: 0.5
- **Espessura Ligações**: 0.1
- **Etiquetas**: Ativadas

### Para Moléculas Médias (Etanol, Acetona)
- **Estilo**: Ball and Stick ou CPK
- **Tamanho Átomos**: 0.4
- **Espessura Ligações**: 0.08
- **Etiquetas**: Opcional

### Para Moléculas Grandes (Proteínas, Polímeros)
- **Estilo**: Wireframe ou Space Filling
- **Tamanho Átomos**: 0.3
- **Espessura Ligações**: 0.05
- **Etiquetas**: Desativadas

---

## 🎨 Cores dos Elementos (CPK)

| Elemento | Cor | Hex |
|----------|-----|-----|
| H - Hidrogênio | ⚪ Branco | #FFFFFF |
| C - Carbono | ⚫ Preto | #000000 |
| N - Nitrogênio | 🔵 Azul | #0000FF |
| O - Oxigênio | 🔴 Vermelho | #FF0000 |
| F - Flúor | 🟢 Verde | #00FF00 |
| Cl - Cloro | 🟢 Verde Claro | #90EE90 |
| Br - Bromo | 🟤 Marrom | #A52A2A |
| I - Iodo | 🟣 Roxo | #800080 |
| P - Fósforo | 🟠 Laranja | #FFA500 |
| S - Enxofre | 🟡 Amarelo | #FFFF00 |

---

## 📊 Exemplos de Medições

### Água (H₂O)
- **Distância O-H**: ~0.96 Å
- **Ângulo H-O-H**: ~104.5°

### Metano (CH₄)
- **Distância C-H**: ~1.09 Å
- **Ângulo H-C-H**: ~109.47° (tetraédrico)

### Dióxido de Carbono (CO₂)
- **Distância C=O**: ~1.16 Å (ligação dupla)
- **Ângulo O=C=O**: ~180° (linear)

### Amônia (NH₃)
- **Distância N-H**: ~1.01 Å
- **Ângulo H-N-H**: ~107° (piramidal trigonal)

---

## 🔧 Dicas e Truques

### 💡 **Melhor Visualização**
1. Use "Auto-Rotação" para ver todos os ângulos
2. Ative "Etiquetas" para identificação rápida
3. Ajuste o tamanho dos átomos conforme necessário

### 📏 **Medições Precisas**
1. Use a otimização de geometria 3D antes de medir
2. Aproxime o zoom para clicar exatamente no átomo desejado
3. As medições são baseadas nas posições otimizadas

### 🎯 **Navegação Eficiente**
1. Use o ViewCube para saltar para vistas ortogonais
2. Duplo-clique no ViewCube para vistas isométricas
3. Use "Reset Câmera" se perder a orientação

### 🚀 **Performance**
1. Desative etiquetas em moléculas com muitos átomos
2. Use estilo "Wireframe" para moléculas muito grandes
3. Reduza o tamanho dos átomos para melhor framerate

---

## 🐛 Resolução de Problemas

### ❓ "Não consigo rotacionar a molécula"
- Certifique-se de clicar com o botão esquerdo
- Verifique se está no modo 3D (toggle ativo)
- Tente resetar a câmera

### ❓ "As medições não aparecem"
- Verifique se há pelo menos 2 átomos na molécula
- Certifique-se de clicar no modo correto (distância ou ângulo)
- Espere até a medição desaparecer (5 segundos) antes de fazer outra

### ❓ "A molécula está muito pequena/grande"
- Use o slider de tamanho de átomos
- Ajuste o zoom com a roda do mouse
- Use "Reset Câmera" para voltar ao padrão

### ❓ "Não vejo as ligações duplas/triplas"
- As ligações múltiplas são desenhadas com cilindros paralelos
- Aumente a espessura das ligações para melhor visualização
- Use o estilo "Ball and Stick" para melhor clareza

---

## 🎓 Casos de Uso Educacionais

### **Aula 1: Geometria Molecular**
1. Construa CH₄ (metano)
2. Ative otimização 3D
3. Meça os ângulos H-C-H
4. Observe a geometria tetraédrica

### **Aula 2: Polaridade**
1. Construa H₂O e CO₂
2. Compare as geometrias
3. Relacione com polaridade (painel direito)
4. Use auto-rotação para apresentação

### **Aula 3: Comprimentos de Ligação**
1. Construa C₂H₄ (eteno - ligação C=C)
2. Construa C₂H₆ (etano - ligação C-C)
3. Meça as distâncias C-C
4. Compare ligação simples vs. dupla

---

## 🔮 Funcionalidades Futuras (Planejadas)

- [ ] Superfícies de densidade eletrônica
- [ ] Orbitais moleculares
- [ ] Animação de vibrações moleculares
- [ ] Exportação de imagens 3D
- [ ] Modo estereoscópico (3D glasses)
- [ ] Sombras e iluminação avançada
- [ ] Representação de ligações de hidrogênio
- [ ] Marcação de centros quirais

---

## 📝 Notas Técnicas

### Coordenadas
- Sistema cartesiano direito (X, Y, Z)
- Unidade: Angstroms (Å) para distâncias
- Conversão: 1 pixel canvas ≈ 0.025 Å

### Algoritmo de Otimização
- Baseado em teoria VSEPR
- Ângulos ideais calculados matematicamente
- Raios covalentes de referência

### Renderização
- Engine: Helix Toolkit WPF
- Primitivas: SphereVisual3D, PipeVisual3D
- Atualização: Em tempo real via CollectionChanged

---

**Desenvolvido com ❤️ para educação em Química**

*Última atualização: 17 de janeiro de 2026*
