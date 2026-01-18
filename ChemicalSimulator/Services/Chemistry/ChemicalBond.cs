using System;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Tipos de ligações químicas
    /// </summary>
    public enum BondType
    {
        Single = 1,    // Ligação simples (-)
        Double = 2,    // Ligação dupla (=)
        Triple = 3,    // Ligação tripla (≡)
        Aromatic = 4,  // Ligação aromática (ressonância)
        Ionic = 5      // Ligação iônica (transferência de elétrons)
    }

    /// <summary>
    /// Representa uma ligação química entre dois átomos
    /// </summary>
    public class ChemicalBond
    {
        /// <summary>
        /// ID do primeiro átomo
        /// </summary>
        public Guid Atom1Id { get; set; }

        /// <summary>
        /// ID do segundo átomo
        /// </summary>
        public Guid Atom2Id { get; set; }

        /// <summary>
        /// Tipo de ligação
        /// </summary>
        public BondType Type { get; set; }

        /// <summary>
        /// Energia de dissociação da ligação (kJ/mol)
        /// </summary>
        public double BondEnergy { get; set; }

        /// <summary>
        /// Comprimento da ligação (picômetros)
        /// </summary>
        public double BondLength { get; set; }

        /// <summary>
        /// Polaridade da ligação (0 = apolar, 1 = totalmente polar)
        /// </summary>
        public double Polarity { get; set; }

        public ChemicalBond(Guid atom1Id, Guid atom2Id, BondType type)
        {
            Atom1Id = atom1Id;
            Atom2Id = atom2Id;
            Type = type;
            SetBondProperties();
        }

        /// <summary>
        /// Define propriedades da ligação baseadas no tipo
        /// </summary>
        private void SetBondProperties()
        {
            switch (Type)
            {
                case BondType.Single:
                    BondEnergy = 350; // Média
                    BondLength = 154; // C-C típico
                    break;
                case BondType.Double:
                    BondEnergy = 611; // C=C
                    BondLength = 134;
                    break;
                case BondType.Triple:
                    BondEnergy = 837; // C≡C
                    BondLength = 120;
                    break;
                case BondType.Aromatic:
                    BondEnergy = 518; // Entre simples e dupla
                    BondLength = 140;
                    break;
                case BondType.Ionic:
                    BondEnergy = 800; // Muito forte
                    BondLength = 250; // Mais longa
                    break;
            }
        }

        /// <summary>
        /// Verifica se a ligação conecta um átomo específico
        /// </summary>
        public bool InvolveAtom(Guid atomId)
        {
            return Atom1Id == atomId || Atom2Id == atomId;
        }

        /// <summary>
        /// Retorna o outro átomo da ligação
        /// </summary>
        public Guid GetOtherAtom(Guid atomId)
        {
            return Atom1Id == atomId ? Atom2Id : Atom1Id;
        }

        public override string ToString()
        {
            string symbol = Type switch
            {
                BondType.Single => "-",
                BondType.Double => "=",
                BondType.Triple => "≡",
                BondType.Aromatic => "~",
                BondType.Ionic => "⋯",
                _ => "?"
            };
            return $"{Atom1Id:N}  {symbol}  {Atom2Id:N}";
        }
    }
}
