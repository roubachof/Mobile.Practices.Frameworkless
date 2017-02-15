// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoreEntryPoint.cs" company="The Silly Company">
//   The Silly Company 2016. All rights reserved.
// </copyright>
// <summary>
//   The core entry point.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using SeLoger.Lab.Playground.Core.Services;

namespace SeLoger.Lab.Playground.Core
{
    /// <summary>
    /// The core entry point.
    /// </summary>
    public class CoreEntryPoint
    {
        /// <summary>
        /// The project assembly.
        /// </summary>
        private static readonly Assembly ProjectAssembly = typeof(CoreEntryPoint).GetTypeInfo().Assembly;

        public async Task RegisterDependenciesAsync()
        {
            await Task.Run(() => this.RegisterDependencies());
        }
        
        private void RegisterDependencies()
        {
            var container = DependencyContainer.Instance;
            
            container.RegisterSingleton<ISillyFrontService, SillyFrontService>();

            // Register all views by convention
            foreach (var pageType in 
                ProjectAssembly.ExportedTypes.Where(
                    type =>
                    type.Namespace.StartsWith("SillyCompany.Mobile.Practices.Views") && !type.GetTypeInfo().IsAbstract
                    && type.Name.EndsWith("Page")))
            {
                container.Register(pageType);
            }

            // Register all view models by convention
            foreach (var viewModelType in 
                ProjectAssembly.ExportedTypes.Where(
                    type =>
                    type.Namespace.StartsWith("SeLoger.Lab.Playground.Core.ViewModels")
                    && !type.GetTypeInfo().IsAbstract && type.Name.EndsWith("ViewModel")))
            {
                container.Register(viewModelType);
            }
        }
    }
}