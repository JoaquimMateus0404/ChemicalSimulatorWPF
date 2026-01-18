using System;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Representa um átomo individual com propriedades químicas.
    /// </summary>
    public class Atom
    {
        /// <summary>
        /// Símbolo do elemento (H, C, O, N, etc.)
        /// </summary>
        public string Element { get; set; } = string.Empty;

        /// <summary>
        /// Número de oxidação/carga formal do átomo
        /// </summary>
        public int Charge { get; set; } = 0;

        /// <summary>
        /// Número de elétrons de valência disponíveis
        /// </summary>
        public int ValenceElectrons { get; set; }

        /// <summary>
        /// Número máximo de ligações que o átomo pode formar
        /// </summary>
        public int MaxBonds { get; set; }

        /// <summary>
        /// Eletronegatividade (Escala de Pauling)
        /// </summary>
        public double Electronegativity { get; set; }

        /// <summary>
        /// ID único para identificação no grafo
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        public Atom(string element)
        {
            Element = element;
            SetAtomicProperties();
        }

        /// <summary>
        /// Define propriedades atômicas baseadas na tabela periódica
        /// </summary>
        private void SetAtomicProperties()
        {
            switch (Element)
            {
                case "H":
                    ValenceElectrons = 1;
                    MaxBonds = 1;
                    Electronegativity = 2.20;
                    break;
                case "C":
                    ValenceElectrons = 4;
                    MaxBonds = 4;
                    Electronegativity = 2.55;
                    break;
                case "N":
                    ValenceElectrons = 5;
                    MaxBonds = 3;
                    Electronegativity = 3.04;
                    break;
                case "O":
                    ValenceElectrons = 6;
                    MaxBonds = 2;
                    Electronegativity = 3.44;
                    break;
                case "F":
                    ValenceElectrons = 7;
                    MaxBonds = 1;
                    Electronegativity = 3.98;
                    break;
                case "Cl":
                    ValenceElectrons = 7;
                    MaxBonds = 1;
                    Electronegativity = 3.16;
                    break;
                case "Br":
                    ValenceElectrons = 7;
                    MaxBonds = 1;
                    Electronegativity = 2.96;
                    break;
                case "S":
                    ValenceElectrons = 6;
                    MaxBonds = 2;
                    Electronegativity = 2.58;
                    break;
                case "P":
                    ValenceElectrons = 5;
                    MaxBonds = 3;
                    Electronegativity = 2.19;
                    break;
                case "Na":
                    ValenceElectrons = 1;
                    MaxBonds = 1;
                    Electronegativity = 0.93;
                    break;
                case "K":
                    ValenceElectrons = 1;
                    MaxBonds = 1;
                    Electronegativity = 0.82;
                    break;
                case "Ca":
                    ValenceElectrons = 2;
                    MaxBonds = 2;
                    Electronegativity = 1.00;
                    break;
                case "Mg":
                    ValenceElectrons = 2;
                    MaxBonds = 2;
                    Electronegativity = 1.31;
                    break;
                default:
                    ValenceElectrons = 4;
                    MaxBonds = 4;
                    Electronegativity = 2.00;
                    break;
            }
        }

        /// <summary>
        /// Verifica se o átomo pode formar mais ligações
        /// </summary>
        public bool CanFormBond(int currentBonds)
        {
            return currentBonds < MaxBonds;
        }

        public override string ToString()
        {
            return $"{Element}({Charge:+0;-#})";
        }
    }
}
