using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// REGRA: Simples Troca (A + BC → AC + B)
    /// Exemplo: Zn + HCl → ZnCl₂ + H₂
    /// </summary>
    public class SingleDisplacementRule : ChemicalRule
    {
        public SingleDisplacementRule(ElementClassifier elementClassifier) : base(elementClassifier)
        {
            Name = "🔄 Simples Troca";
            Priority = 50;
            Type = ReactionType.SingleDisplacement;
            ActivationEnergy = 80; // kJ/mol
            EnthalpyChange = -50;  // Levemente exotérmica
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            // ✅ Condição: 1 elemento simples + 1 composto
            if (reactants.Count != 2)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Simples troca requer 2 reagentes (tem {reactants.Count})");
                return false;
            }

            bool hasElement = reactants[0].Atoms.Count <= 2 || reactants[1].Atoms.Count <= 2;
            bool hasCompound = reactants[0].Atoms.Count > 2 || reactants[1].Atoms.Count > 2;

            if (!hasElement || !hasCompound)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Não tem elemento + composto");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"    ✅ Simples troca válida");
            return true;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            var products = new List<MoleculeGraph>();

            System.Diagnostics.Debug.WriteLine($"🔥 Aplicando SIMPLES TROCA");

            // Identificar elemento e composto
            MoleculeGraph element, compound;
            
            if (reactants[0].Atoms.Count <= 2)
            {
                element = reactants[0];
                compound = reactants[1];
            }
            else
            {
                element = reactants[1];
                compound = reactants[0];
            }

            System.Diagnostics.Debug.WriteLine($"   Elemento: {element.Name} ({element.Atoms.Count} átomos)");
            System.Diagnostics.Debug.WriteLine($"   Composto: {compound.Name} ({compound.Atoms.Count} átomos)");

            // 🧪 CASO ESPECIAL: Metal + Sal → Novo Sal + Metal deslocado
            // Exemplo: Cu + AgNO₃ → Cu(NO₃)₂ + Ag
            if (IsMetal(element) && IsSalt(compound))
            {
                // Extrair metal do composto (primeiro átomo geralmente)
                var displacedMetal = compound.Atoms.FirstOrDefault(a => IsMetal(a.Element));
                var newMetal = element.Atoms.FirstOrDefault();

                if (displacedMetal != null && newMetal != null)
                {
                    // Produto 1: Metal deslocado (forma elemento puro ou diatômico)
                    var pureMetalProduct = new MoleculeGraph
                    {
                        Name = displacedMetal.Element,
                        Formula = displacedMetal.Element
                    };
                    pureMetalProduct.AddAtom(displacedMetal.Element);
                    products.Add(pureMetalProduct);

                    // Produto 2: Novo sal (metal reagente + ânion do composto)
                    // Simplificado: copiar todos átomos do composto exceto o metal original
                    var newSalt = new MoleculeGraph
                    {
                        Name = $"{newMetal.Element} + ânion",
                        Formula = $"{newMetal.Element}X"
                    };

                    // Adicionar novo metal
                    newSalt.AddAtom(newMetal.Element);

                    // Adicionar ânions (todos átomos que não são o metal deslocado)
                    foreach (var atom in compound.Atoms.Where(a => a.Element != displacedMetal.Element))
                    {
                        newSalt.AddAtom(atom.Element);
                    }

                    products.Add(newSalt);

                    System.Diagnostics.Debug.WriteLine($"  ✅ {element.Name} + {compound.Name} → {newSalt.Formula} + {pureMetalProduct.Formula}");
                    return products;
                }
            }

            // 🧪 CASO: Metal + Ácido → Sal + H₂
            // Exemplo: Zn + HCl → ZnCl₂ + H₂
            if (IsMetal(element) && IsAcid(compound))
            {
                var metal = element.Atoms.First();

                // H₂ gasoso
                var h2 = new MoleculeGraph { Name = "Hidrogênio", Formula = "H₂" };
                var h1 = h2.AddAtom("H");
                var h2atom = h2.AddAtom("H");
                h2.AddBond(h1.Id, h2atom.Id, BondType.Single);
                products.Add(h2);

                // Sal metálico (metal + ânion do ácido)
                var salt = new MoleculeGraph
                {
                    Name = $"Sal de {metal.Element}",
                    Formula = $"{metal.Element}X"
                };

                salt.AddAtom(metal.Element);
                
                // Adicionar ânions (todos átomos exceto H)
                foreach (var atom in compound.Atoms.Where(a => a.Element != "H"))
                {
                    salt.AddAtom(atom.Element);
                }

                products.Add(salt);

                System.Diagnostics.Debug.WriteLine($"  ✅ {element.Name} + {compound.Name} → {salt.Formula} + H₂");
                return products;
            }

            // Fallback: não conseguiu processar
            System.Diagnostics.Debug.WriteLine("  ⚠️ Simples troca não implementada para este caso específico");
            return products;
        }

        /// <summary>
        /// <summary>
        /// Verifica se elemento é um metal usando ElementClassifier
        /// </summary>
        private bool IsMetal(MoleculeGraph molecule)
        {
            if (molecule.Atoms.Count == 0) return false;
            var firstElement = molecule.Atoms.First().Element;
            return ElementClassifier.IsMetal(firstElement);
        }

        private bool IsMetal(string symbol)
        {
            return ElementClassifier.IsMetal(symbol);
        }

        /// <summary>
        /// Verifica se é um sal (contém metal + não-metal)
        /// </summary>
        private bool IsSalt(MoleculeGraph molecule)
        {
            bool hasMetal = molecule.Atoms.Any(a => ElementClassifier.IsMetal(a.Element));
            bool hasNonMetal = molecule.Atoms.Any(a => ElementClassifier.IsNonMetal(a.Element));
            return hasMetal && hasNonMetal;
        }

        /// <summary>
        /// Verifica se é um ácido (começa com H)
        /// </summary>
        private bool IsAcid(MoleculeGraph molecule)
        {
            return molecule.Atoms.Any(a => a.Element == "H") && 
                   !molecule.Atoms.Any(a => ElementClassifier.IsMetal(a.Element));
        }
    }
}
