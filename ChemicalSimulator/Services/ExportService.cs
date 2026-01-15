using System;
using System.IO;
using System.Text;
using ChemicalSimulator.Models;
using Microsoft.Win32;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Serviço para exportação de resultados
    /// </summary>
    public class ExportService
    {
        public void ExportToPdf(Reaction reaction, Molecule molecule, ReactionConditions conditions)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"ReactionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                GeneratePdfReport(dialog.FileName, reaction, molecule, conditions);
            }
        }

        public void ExportToCsv(Reaction reaction, Molecule molecule)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"ReactionData_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() == true)
            {
                GenerateCsvReport(dialog.FileName, reaction, molecule);
            }
        }

        private void GeneratePdfReport(string filePath, Reaction reaction, Molecule molecule, ReactionConditions conditions)
        {
            // Aqui você integraria com QuestPDF ou iTextSharp
            // Por enquanto, vou criar um exemplo simplificado

            var reportContent = GenerateReportContent(reaction, molecule, conditions);

            // Placeholder - substituir por geração real de PDF
            File.WriteAllText(filePath.Replace(".pdf", ".txt"), reportContent);
        }

        private void GenerateCsvReport(string filePath, Reaction reaction, Molecule molecule)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Propriedade,Valor,Unidade");
            csv.AppendLine($"Nome da Molécula,{molecule.Name},");
            csv.AppendLine($"Fórmula,{molecule.Formula},");
            csv.AppendLine($"Massa Molar,{molecule.MolarMass:F2},g/mol");
            csv.AppendLine($"Número de Átomos,{molecule.Atoms.Count},");
            csv.AppendLine($"Número de Ligações,{molecule.Bonds.Count},");
            csv.AppendLine();
            csv.AppendLine("Propriedades Termodinâmicas");
            csv.AppendLine($"ΔH (Entalpia),{reaction.EnthalpyChange:F2},kJ/mol");
            csv.AppendLine($"ΔS (Entropia),{reaction.EntropyChange:F2},J/(mol·K)");
            csv.AppendLine($"ΔG (Gibbs),{reaction.GibbsFreeEnergy:F2},kJ/mol");
            csv.AppendLine($"Energia de Ativação,{reaction.ActivationEnergy:F2},kJ/mol");
            csv.AppendLine($"Constante de Velocidade,{reaction.RateConstant:E2},s⁻¹");
            csv.AppendLine($"Reação Espontânea,{reaction.IsSpontaneous},");
            csv.AppendLine($"Reação Exotérmica,{reaction.IsExothermic},");

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private string GenerateReportContent(Reaction reaction, Molecule molecule, ReactionConditions conditions)
        {
            var sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("     RELATÓRIO DE SIMULAÇÃO QUÍMICA");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("─── MOLÉCULA ───");
            sb.AppendLine($"Nome: {molecule.Name}");
            sb.AppendLine($"Fórmula Molecular: {molecule.Formula}");
            sb.AppendLine($"Massa Molar: {molecule.MolarMass:F2} g/mol");
            sb.AppendLine($"Geometria: {molecule.Geometry}");
            sb.AppendLine($"Número de Átomos: {molecule.Atoms.Count}");
            sb.AppendLine($"Número de Ligações: {molecule.Bonds.Count}");
            sb.AppendLine($"Polar: {(molecule.IsPolar ? "Sim" : "Não")}");
            sb.AppendLine();

            sb.AppendLine("─── CONDIÇÕES DA REAÇÃO ───");
            sb.AppendLine($"Temperatura: {conditions.Temperature:F2} K ({conditions.Temperature - 273.15:F2} °C)");
            sb.AppendLine($"Pressão: {conditions.Pressure:F2} kPa");
            sb.AppendLine($"pH: {conditions.pH:F2}");
            sb.AppendLine($"Solvente: {conditions.Solvent}");
            sb.AppendLine($"Catalisador: {conditions.Catalyst}");
            sb.AppendLine();

            sb.AppendLine("─── TERMODINÂMICA ───");
            sb.AppendLine($"ΔH (Variação de Entalpia): {reaction.EnthalpyChange:F2} kJ/mol");
            sb.AppendLine($"  {(reaction.IsExothermic ? "EXOTÉRMICA (libera calor)" : "ENDOTÉRMICA (absorve calor)")}");
            sb.AppendLine();
            sb.AppendLine($"ΔS (Variação de Entropia): {reaction.EntropyChange:F2} J/(mol·K)");
            sb.AppendLine($"  {(reaction.EntropyChange > 0 ? "Aumento de desordem" : "Diminuição de desordem")}");
            sb.AppendLine();
            sb.AppendLine($"ΔG (Energia Livre de Gibbs): {reaction.GibbsFreeEnergy:F2} kJ/mol");
            sb.AppendLine($"  {(reaction.IsSpontaneous ? "ESPONTÂNEA (ocorre naturalmente)" : "NÃO-ESPONTÂNEA (requer energia)")}");
            sb.AppendLine();

            sb.AppendLine("─── CINÉTICA QUÍMICA ───");
            sb.AppendLine($"Energia de Ativação: {reaction.ActivationEnergy:F2} kJ/mol");
            sb.AppendLine($"Constante de Velocidade: {reaction.RateConstant:E3} s⁻¹");
            sb.AppendLine($"Ordem da Reação: {reaction.ReactionOrder}");
            if (reaction.ReactionOrder == 1)
                sb.AppendLine($"Tempo de Meia-Vida: {reaction.HalfLife:F2} s");
            sb.AppendLine();

            sb.AppendLine("─── EQUAÇÃO QUÍMICA ───");
            sb.AppendLine(GenerateChemicalEquation(reaction));
            sb.AppendLine();

            sb.AppendLine("─── INTERPRETAÇÃO ───");
            sb.AppendLine(GenerateInterpretation(reaction));
            sb.AppendLine();

            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("       Gerado por Chemical Simulator v1.0");
            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }

        private string GenerateChemicalEquation(Reaction reaction)
        {
            var sb = new StringBuilder();

            // Reagentes
            for (int i = 0; i < reaction.Reactants.Count; i++)
            {
                var r = reaction.Reactants[i];
                if (r.Coefficient > 1)
                    sb.Append($"{r.Coefficient} ");
                sb.Append($"{r.Molecule.Formula} {r.Phase}");

                if (i < reaction.Reactants.Count - 1)
                    sb.Append(" + ");
            }

            sb.Append(" → ");

            // Produtos
            for (int i = 0; i < reaction.Products.Count; i++)
            {
                var p = reaction.Products[i];
                if (p.Coefficient > 1)
                    sb.Append($"{p.Coefficient} ");
                sb.Append($"{p.Molecule.Formula} {p.Phase}");

                if (i < reaction.Products.Count - 1)
                    sb.Append(" + ");
            }

            return sb.ToString();
        }

        private string GenerateInterpretation(Reaction reaction)
        {
            var sb = new StringBuilder();

            if (reaction.IsSpontaneous && reaction.IsExothermic)
            {
                sb.AppendLine("Esta reação é FAVORÁVEL termodinamicamente:");
                sb.AppendLine("• Libera energia (exotérmica)");
                sb.AppendLine("• Ocorre espontaneamente");
                sb.AppendLine("• Ideal para processos energéticos");
            }
            else if (!reaction.IsSpontaneous && !reaction.IsExothermic)
            {
                sb.AppendLine("Esta reação é DESFAVORÁVEL termodinamicamente:");
                sb.AppendLine("• Requer energia externa (endotérmica)");
                sb.AppendLine("• Não ocorre espontaneamente");
                sb.AppendLine("• Necessita de condições especiais");
            }
            else
            {
                sb.AppendLine("Esta reação apresenta características mistas:");
                sb.AppendLine($"• {(reaction.IsExothermic ? "Libera energia" : "Absorve energia")}");
                sb.AppendLine($"• {(reaction.IsSpontaneous ? "Espontânea" : "Não-espontânea")}");
            }

            if (reaction.ActivationEnergy < 50)
                sb.AppendLine("• Baixa barreira energética (reação rápida)");
            else if (reaction.ActivationEnergy > 150)
                sb.AppendLine("• Alta barreira energética (reação lenta)");

            return sb.ToString();
        }

        public void ExportMoleculeToChemDraw(Molecule molecule)
        {
            // Exportar em formato compatível com ChemDraw (MOL file)
            var dialog = new SaveFileDialog
            {
                Filter = "MDL MOL Files (*.mol)|*.mol",
                FileName = $"{molecule.Name}.mol",
                DefaultExt = ".mol"
            };

            if (dialog.ShowDialog() == true)
            {
                GenerateMolFile(dialog.FileName, molecule);
            }
        }

        private void GenerateMolFile(string filePath, Molecule molecule)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine(molecule.Name);
            sb.AppendLine("  Chemical Simulator");
            sb.AppendLine();

            // Counts line
            sb.AppendLine($"{molecule.Atoms.Count,3}{molecule.Bonds.Count,3}  0  0  0  0  0  0  0  0999 V2000");

            // Atom block
            foreach (var atom in molecule.Atoms)
            {
                sb.AppendLine($"{atom.Position.X,10:F4}{atom.Position.Y,10:F4}{atom.Position.Z,10:F4} {atom.Element.Symbol,-3}  0  0  0  0  0");
            }

            // Bond block
            foreach (var bond in molecule.Bonds)
            {
                int type = bond.BondOrder;
                sb.AppendLine($"{bond.Atom1.Id + 1,3}{bond.Atom2.Id + 1,3}{type,3}  0  0  0");
            }

            sb.AppendLine("M  END");

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}