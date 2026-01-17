using System;
using System.Collections.Generic;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.Infrastructure
{
    /// <summary>
    /// Service Locator para gerenciamento de dependências (Singleton Pattern)
    /// </summary>
    public sealed class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> _instance = 
            new Lazy<ServiceLocator>(() => new ServiceLocator());

        private readonly Dictionary<Type, object> _services;

        public static ServiceLocator Instance => _instance.Value;

        private ServiceLocator()
        {
            _services = new Dictionary<Type, object>();
            RegisterServices();
        }

        /// <summary>
        /// Registra todos os serviços da aplicação
        /// </summary>
        private void RegisterServices()
        {
            // Registrar serviços como Singleton
            Register(new ChemistryEngine());
            Register(new ReactionPredictor());
            Register(new ExportService());
            Register(new ElementDataLoader());
        }

        /// <summary>
        /// Registra um serviço
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        /// <summary>
        /// Obtém uma instância do serviço
        /// </summary>
        public T GetService<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            
            throw new InvalidOperationException($"Serviço {type.Name} não foi registrado.");
        }

        /// <summary>
        /// Tenta obter uma instância do serviço
        /// </summary>
        public bool TryGetService<T>(out T service) where T : class
        {
            service = null;
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }
            
            return false;
        }

        /// <summary>
        /// Remove um serviço registrado
        /// </summary>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Limpa todos os serviços registrados
        /// </summary>
        public void Clear()
        {
            _services.Clear();
        }
    }
}
