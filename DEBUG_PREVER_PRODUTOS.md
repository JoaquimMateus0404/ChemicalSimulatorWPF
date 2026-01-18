# 🔍 GUIA DE DEBUG - Botão "Prever Produtos"

## 🐛 Problema Reportado
O botão **"🔮 PREVER PRODUTOS AUTOMATICAMENTE"** não responde quando clicado.

## ✅ Correções Aplicadas

### 1. **Debugging Adicionado**
Agora o ViewModel imprime mensagens de debug detalhadas:

```csharp
// Quando você seleciona um reagente:
🔄 SelectedReactant1 alterado para: Metano
🔄 UpdateReaction() chamado!
✅ Atualizando reação com: Metano
📊 Reagentes atualizados: 1

// Quando você clica em "Prever Produtos":
🔮 PredictProducts() chamado!
✅ Reagente 1: Metano (CH4)
🔍 Analisando: CH4
🔥 Detectada combustão de hidrocarboneto!
  CO2 encontrado: Dióxido de Carbono
  H2O encontrado: Água
📊 Total de produtos gerados: 2
  ➕ Adicionado: Dióxido de Carbono
  ➕ Adicionado: Água
✅ Predição completa! Total produtos: 2
```

### 2. **CommandManager Atualização Forçada**
```csharp
CommandManager.InvalidateRequerySuggested();
```
Agora quando você seleciona um reagente, o comando é forçado a reavaliar seu estado.

### 3. **Verificações de Fórmulas Melhoradas**
Agora busca tanto `CO2` quanto `CO₂`, `H2O` e `H₂O`.

### 4. **Valores Padrão de Termodinâmica**
Adicionados valores para `EnthalpyChange` em cada tipo de reação.

## 🧪 Como Testar

### Passo 1: Compilar e Executar
```powershell
cd "c:\Users\Duarte Gauss\source\repos\ChemicalSimulator"
dotnet build
dotnet run
```

### Passo 2: Abrir Output Window
No Visual Studio:
1. Menu: **View** → **Output**
2. Selecione: **Show output from: Debug**

### Passo 3: Testar o Fluxo
1. **Selecione um reagente** (ex: Metano)
   - Você deve ver no Output:
   ```
   🔄 SelectedReactant1 alterado para: Metano
   🔄 UpdateReaction() chamado!
   ```

2. **Clique no botão "🔮 PREVER PRODUTOS"**
   - Você deve ver no Output:
   ```
   🔮 PredictProducts() chamado!
   🔍 Analisando: CH4
   🔥 Detectada combustão...
   ```

### Passo 4: Verificar Resultados Visuais
Após clicar, você deve ver:
- ✅ Produtos aparecem na seção de produtos
- ✅ Equação química é atualizada
- ✅ Tipo de reação aparece (ex: "Reação de Combustão")
- ✅ Valores termodinâmicos atualizados (ΔH, ΔG, Ea)

## 🔍 Possíveis Causas do Problema

### Causa 1: DataContext não está configurado
**Verificar:**
```csharp
// No construtor da View ou no App.xaml.cs
DataContext = new ReactionSimulatorViewModel();
```

### Causa 2: ComboBox não tem dados
**Verificar no Output:**
```
✅ Carregados X elementos e Y compostos
```

Se aparecer `0 compostos`, os dados não foram carregados!

### Causa 3: Binding está quebrado
**Teste manual:**
Adicione `Click` event handler temporário:

```xml
<Button Content="🔮 PREVER PRODUTOS"
        Command="{Binding PredictProductsCommand}"
        Click="Button_Click_Test"/>
```

```csharp
// No code-behind
private void Button_Click_Test(object sender, RoutedEventArgs e)
{
    MessageBox.Show("Botão clicado! Command funcionando?");
    var vm = DataContext as ReactionSimulatorViewModel;
    MessageBox.Show($"ViewModel: {vm != null}");
    MessageBox.Show($"Reagente1: {vm?.SelectedReactant1?.Name ?? "NULL"}");
}
```

### Causa 4: Comando está desabilitado
**Verificar:**
O botão está visualmente "disabled" (cinza)?

**CanPredictProducts retorna:**
```csharp
SelectedReactant1 != null
```

Se `SelectedReactant1` for `null`, o botão fica desabilitado!

## 🛠️ Soluções por Causa

### Se DataContext está null:
```csharp
// Em MainWindow.xaml.cs ou onde a View é criada
public MainWindow()
{
    InitializeComponent();
    
    // Se a View é parte de um TabControl/ContentControl
    reactionSimulatorView.DataContext = new ReactionSimulatorViewModel();
}
```

### Se compostos não carregam:
Verifique se os arquivos JSON existem:
```
ChemicalSimulator/Resources/Data/CommonCompoundsData.json
ChemicalSimulator/Resources/Data/ElementsData.json
```

### Se Command não dispara:
Use `RelayCommand` direto no teste:
```csharp
// Teste manual
var vm = new ReactionSimulatorViewModel();
vm.SelectedReactant1 = vm.AvailableCompounds.First();
vm.PredictProductsCommand.Execute(null);
```

## 📊 Checklist de Verificação

- [ ] Visual Studio Output mostra logs de debug?
- [ ] ComboBox mostra compostos disponíveis?
- [ ] Ao selecionar reagente, Output mostra "SelectedReactant1 alterado"?
- [ ] Botão está habilitado (não cinza)?
- [ ] Ao clicar, Output mostra "PredictProducts() chamado"?
- [ ] Produtos aparecem na View?
- [ ] Equação química é atualizada?

## 🎯 Teste Rápido

Adicione este botão de teste na View (temporário):

```xml
<!-- BOTÃO DE TESTE - REMOVER DEPOIS -->
<Button Content="🧪 TESTE DIRETO"
        Click="TestButton_Click"
        Margin="10"/>
```

```csharp
// No code-behind da View
private void TestButton_Click(object sender, RoutedEventArgs e)
{
    var vm = DataContext as ReactionSimulatorViewModel;
    
    if (vm == null)
    {
        MessageBox.Show("❌ ViewModel é NULL!");
        return;
    }
    
    if (vm.AvailableCompounds == null || !vm.AvailableCompounds.Any())
    {
        MessageBox.Show("❌ Nenhum composto carregado!");
        return;
    }
    
    if (vm.SelectedReactant1 == null)
    {
        MessageBox.Show("❌ Nenhum reagente selecionado!");
        return;
    }
    
    MessageBox.Show($"✅ Tudo OK!\nReagente: {vm.SelectedReactant1.Name}");
    
    // Chamar PredictProducts diretamente
    try
    {
        vm.PredictProductsCommand.Execute(null);
        MessageBox.Show($"✅ Comando executado!\nProdutos: {vm.Products.Count}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"❌ Erro: {ex.Message}");
    }
}
```

## 📞 Próximos Passos

1. **Execute o app** e abra o **Output Window**
2. **Selecione um reagente** e veja os logs
3. **Clique em "Prever Produtos"** e veja os logs
4. **Me envie os logs** se ainda não funcionar!

## 🔬 Compostos de Teste

Use estes compostos para testar diferentes tipos de reação:

### Combustão (deve funcionar):
- **Metano (CH₄)** + Oxigênio → CO₂ + H₂O
- **Etanol (C₂H₅OH)** → CO₂ + H₂O
- **Propano (C₃H₈)** → CO₂ + H₂O

### Ácido-Base (deve funcionar):
- **HCl** + **NaOH** → NaCl + H₂O
- **H₂SO₄** + **KOH** → K₂SO₄ + H₂O

### Decomposição (pode não gerar produtos específicos):
- **CaCO₃** → CaO + CO₂
- **H₂O₂** → H₂O + O₂

---

**Se ainda não funcionar, me envie:**
1. ✅ Logs do Output Window
2. ✅ Screenshot da tela
3. ✅ Resultado do botão de teste

Vamos resolver juntos! 🚀
