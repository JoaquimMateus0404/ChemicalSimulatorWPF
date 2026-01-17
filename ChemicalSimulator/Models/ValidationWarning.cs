using System;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa um aviso de valência ou validação
    /// </summary>
    public class ValidationWarning
    {
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning"; // Error, Warning, Info, Success
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string AtomId { get; set; } = string.Empty;
        
        public ValidationWarning() { }
        
        public ValidationWarning(string message, string severity = "Warning")
        {
            Message = message;
            Severity = severity;
        }
    }
}
