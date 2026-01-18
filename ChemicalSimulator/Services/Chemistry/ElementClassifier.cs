using System.Collections.Generic;
using System.Linq;
using ChemicalSimulator.Models;
using ModelElement = ChemicalSimulator.Models.Element;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Serviço centralizado para classificação de elementos químicos
    /// Usa dados reais do ElementDataLoader em vez de arrays hardcoded
    /// </summary>
    public class ElementClassifier
    {
        private readonly Dictionary<string, ModelElement> _elementsBySymbol;
        private readonly HashSet<string> _metals;
        private readonly HashSet<string> _nonMetals;
        private readonly HashSet<string> _metalloids;

        public ElementClassifier(IEnumerable<ModelElement> elements)
        {
            _elementsBySymbol = elements.ToDictionary(e => e.Symbol, e => e);
            
            // Pré-computar conjuntos para performance
            _metals = new HashSet<string>();
            _nonMetals = new HashSet<string>();
            _metalloids = new HashSet<string>();

            foreach (var element in elements)
            {
                if (IsMetalCategory(element.Category))
                {
                    _metals.Add(element.Symbol);
                }
                else if (IsMetalloidCategory(element.Category))
                {
                    _metalloids.Add(element.Symbol);
                }
                else
                {
                    _nonMetals.Add(element.Symbol);
                }
            }

            System.Diagnostics.Debug.WriteLine($"📊 ElementClassifier inicializado:");
            System.Diagnostics.Debug.WriteLine($"   🔵 Metais: {_metals.Count}");
            System.Diagnostics.Debug.WriteLine($"   🟡 Metaloides: {_metalloids.Count}");
            System.Diagnostics.Debug.WriteLine($"   🔴 Não-metais: {_nonMetals.Count}");
        }

        /// <summary>
        /// Verifica se um símbolo é de um metal
        /// </summary>
        public bool IsMetal(string symbol)
        {
            return _metals.Contains(symbol);
        }

        /// <summary>
        /// Verifica se um símbolo é de um não-metal
        /// </summary>
        public bool IsNonMetal(string symbol)
        {
            return _nonMetals.Contains(symbol);
        }

        /// <summary>
        /// Verifica se um símbolo é de um metaloide
        /// </summary>
        public bool IsMetalloid(string symbol)
        {
            return _metalloids.Contains(symbol);
        }

        /// <summary>
        /// Verifica se um símbolo é de um halogênio (F, Cl, Br, I, At)
        /// </summary>
        public bool IsHalogen(string symbol)
        {
            if (!_elementsBySymbol.TryGetValue(symbol, out var element))
                return false;

            return element.Category == ElementCategory.Halogen;
        }

        /// <summary>
        /// Verifica se um símbolo é de um gás nobre
        /// </summary>
        public bool IsNobleGas(string symbol)
        {
            if (!_elementsBySymbol.TryGetValue(symbol, out var element))
                return false;

            return element.Category == ElementCategory.NobleGas;
        }

        /// <summary>
        /// Obtém a eletronegatividade de um elemento
        /// </summary>
        public double GetElectronegativity(string symbol)
        {
            if (!_elementsBySymbol.TryGetValue(symbol, out var element))
                return 0;

            return element.Electronegativity ?? 0;
        }

        /// <summary>
        /// Obtém informações completas de um elemento
        /// </summary>
        public ModelElement? GetElement(string symbol)
        {
            _elementsBySymbol.TryGetValue(symbol, out var element);
            return element;
        }

        /// <summary>
        /// Classifica categoria do elemento em metal/não-metal/metaloide
        /// </summary>
        private bool IsMetalCategory(ElementCategory category)
        {
            return category switch
            {
                ElementCategory.AlkaliMetal => true,        // Li, Na, K, Rb, Cs, Fr
                ElementCategory.AlkalineEarthMetal => true, // Be, Mg, Ca, Sr, Ba, Ra
                ElementCategory.TransitionMetal => true,    // Fe, Cu, Zn, Ag, Au, etc.
                ElementCategory.PostTransitionMetal => true,// Al, Ga, In, Sn, Pb, etc.
                ElementCategory.Lanthanide => true,         // La, Ce, Pr, etc.
                ElementCategory.Actinide => true,           // Ac, Th, U, etc.
                _ => false
            };
        }

        /// <summary>
        /// Verifica se é metaloide
        /// </summary>
        private bool IsMetalloidCategory(ElementCategory category)
        {
            return category == ElementCategory.Metalloid; // B, Si, Ge, As, Sb, Te
        }

        /// <summary>
        /// Obtém todos os símbolos de metais
        /// </summary>
        public IEnumerable<string> GetAllMetals()
        {
            return _metals;
        }

        /// <summary>
        /// Obtém todos os símbolos de não-metais
        /// </summary>
        public IEnumerable<string> GetAllNonMetals()
        {
            return _nonMetals;
        }
    }
}
