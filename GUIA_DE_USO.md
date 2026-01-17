# 🎬 Guia de Uso - ReactionSimulator Avançado

## 🚀 Como Usar a Nova Interface

### 1️⃣ **ADICIONAR REAGENTES**

1. No painel esquerdo, selecione uma molécula no dropdown
2. Defina o coeficiente estequiométrico (padrão: 1)
3. Clique em "ADICIONAR"
4. Repita para todos os reagentes necessários
5. Para remover, clique no ícone de lixeira (🗑️) ao lado do reagente

### 2️⃣ **CONFIGURAR CONDIÇÕES**

#### Temperatura 🌡️
- Use o slider para ajustar de -100°C a 500°C
- Observe o indicador de estado:
  - 🧊 Congelamento
  - ✓ Temperatura Ambiente
  - 🔥 Aquecimento
  - 🔥🔥 Alta Temperatura

#### Pressão ⚡
- Ajuste de 10 kPa (vácuo) a 1000 kPa
- Influencia o estado físico dos reagentes

#### pH 🧪
- Escala de 0 a 14
- Visualização em cores:
  - Vermelho: Ácido forte
  - Amarelo: Neutro
  - Verde/Azul: Básico

#### Solvente 💧
- Escolha entre: Água, Etanol, Acetona, etc.
- Afeta a velocidade da reação

#### Catalisador ⚛️
- Opções: Pt, Pd, Ni, Enzimas, etc.
- Reduz a energia de ativação em 60%

### 3️⃣ **PREVER PRODUTOS**

1. Com reagentes adicionados, clique em "🔮 PREVER PRODUTOS"
2. O sistema analisa:
   - Elementos presentes
   - Tipo de reação
   - Condições termodinâmicas
3. Produtos aparecem automaticamente no painel direito
4. Equação balanceada exibida no centro

**Tipos de reação detectados:**
- 🔥 Combustão (C,H + O₂ → CO₂ + H₂O)
- ⚡ Decomposição térmica
- 🔬 Síntese
- 🔄 Dupla troca

### 4️⃣ **SIMULAR REAÇÃO**

1. Clique em "▶️ SIMULAR REAÇÃO"
2. Observe as 6 fases da simulação:

   **Fase 1**: Aproximação Molecular (0-15%)
   - Moléculas colidem efetivamente

   **Fase 2**: Quebra de Ligações (15-35%)
   - Energia de ativação consumida
   - Ligações antigas rompidas

   **Fase 3**: Complexo Ativado (35-55%)
   - Estado de transição
   - Maior energia do sistema

   **Fase 4**: Formação de Produtos (55-75%)
   - Novas ligações formadas
   - Produtos emergindo

   **Fase 5**: Transferência de Energia (75-90%)
   - ♨️ Exotérmica: Libera calor
   - 🧊 Endotérmica: Absorve calor

   **Fase 6**: Resultados (90-100%)
   - Dados finais calculados

3. **Animações visuais**:
   - Partículas flutuando no fundo
   - Átomos colidindo no centro
   - Explosão ao finalizar (se exotérmica)

### 5️⃣ **AJUSTAR VELOCIDADE**

- Use o slider no canto inferior direito
- Velocidades: 0.5x, 1.0x, 1.5x, 2.0x, 2.5x, 3.0x
- Útil para análise detalhada ou demonstrações rápidas

### 6️⃣ **BALANCEAR EQUAÇÃO**

1. Clique em "⚖️ BALANCEAR"
2. Sistema aplica conservação de massa
3. Coeficientes ajustados automaticamente
4. Indicador verde ✓ confirma balanceamento

### 7️⃣ **ANALISAR RESULTADOS**

#### Painel Direito - Termodinâmica

**Variação de Entalpia (ΔH)**
- 🔴 Card vermelho
- Negativo: Reação exotérmica
- Positivo: Reação endotérmica

**Energia Livre de Gibbs (ΔG)**
- 🟣 Card roxo
- Negativo: Reação espontânea
- Positivo: Reação não-espontânea

**Energia de Ativação (Ea)**
- 🟠 Card laranja
- Quanto maior, mais lenta a reação
- Catalisador reduz este valor

#### Cards de Características

**TIPO**
- Exotérmica ♨️
- Endotérmica 🧊

**ESPONTANEIDADE**
- Espontânea ✓
- Não-espontânea ✗

#### Cinética Reacional

**Constante de Velocidade (k)**
- Notação científica
- Maior = reação mais rápida

**Ordem da Reação**
- 0, 1, ou 2
- Determina como concentração afeta velocidade

**Meia-Vida (t½)**
- Tempo para 50% de conversão
- Em segundos, minutos, horas ou dias

### 8️⃣ **EXPORTAR DADOS**

#### 💾 Exportar JSON
1. Clique em "💾 EXPORTAR DADOS (JSON)"
2. Arquivo salvo com timestamp
3. Contém todos os dados da reação
4. Formato padronizado para análise

#### 📊 Gerar Relatório
1. Clique em "📊 GERAR RELATÓRIO COMPLETO"
2. Pop-up com resumo formatado
3. Copie para documentos
4. Compartilhe com colegas

#### 📈 Visualizar Gráficos
- Funcionalidade preparada
- Em breve: diagramas de energia, cinética

### 9️⃣ **LIMPAR SIMULAÇÃO**

1. Clique em "🔄 LIMPAR"
2. Todos os reagentes removidos
3. Produtos limpos
4. Condições mantidas
5. Pronto para nova reação

---

## 🎯 DICAS PROFISSIONAIS

### ✅ **Melhores Práticas**

1. **Comece Simples**: Teste com reações conhecidas
   - CH₄ + O₂ (combustão do metano)
   - 2H₂O → 2H₂ + O₂ (eletrólise)

2. **Explore Condições**: Varie temperatura e pressão
   - Veja como afetam ΔG e k

3. **Use Catalisadores**: Compare com e sem
   - Observe redução de Ea

4. **Ajuste Velocidade**: 
   - 0.5x para apresentações
   - 3.0x para testes rápidos

5. **Exporte Resultados**: Mantenha registro
   - Compare diferentes condições
   - Análise científica posterior

### ⚠️ **Limitações Conhecidas**

1. Balanceamento usa algoritmo simplificado
2. Alguns produtos são genéricos
3. Gráficos ainda não implementados
4. Database de moléculas limitado

### 🔬 **Casos de Uso Educacionais**

#### **Aula de Termodinâmica**
1. Configure reação de combustão
2. Mostre ΔH negativo grande
3. Explique liberação de energia
4. Use velocidade 1.5x para demonstração

#### **Aula de Cinética**
1. Configure mesma reação 2 vezes
2. Uma com catalisador, outra sem
3. Compare valores de k e Ea
4. Mostre diferença de velocidade

#### **Lab Virtual**
1. Alunos preveem produtos
2. Comparam com sistema
3. Analisam termodinâmica
4. Exportam relatórios

---

## 🆘 TROUBLESHOOTING

### Problema: Animações lentas
**Solução**: Aumente a velocidade de simulação

### Problema: Produtos não aparecem
**Solução**: Verifique se adicionou reagentes e clicou em PREVER

### Problema: Equação não balanceia
**Solução**: Alguns casos complexos ainda não suportados

### Problema: Valores irrealistas
**Solução**: Sistema usa aproximações - valores são educacionais

---

## 📞 SUPORTE

Para dúvidas ou sugestões:
1. Verifique a documentação técnica
2. Consulte exemplos de reações
3. Revise o código-fonte comentado

---

**Aproveite a experiência profissional de simulação química!** 🧪⚛️✨
