﻿// --------------------------------------------------------------------------------------------------------------------
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

using MetroLog;
using MetroLog.Targets;

using Mobile.Practices.Frameworkless.Core;
using Mobile.Practices.Frameworkless.Services;

using SeLoger.Mobile.ToolKit.Core;

namespace Mobile.Practices.Frameworkless
{
    public class CoreEntryPoint
    {
        private static readonly Assembly ProjectAssembly = typeof(CoreEntryPoint).GetTypeInfo().Assembly;

        public async Task InitializeCoreAsync()
        {
            await Task.Run(() => RegisterDependencies()).ConfigureAwait(false);

            InitializeLogTargets();
            ToolKit.Initialize();
        }
        
        private void RegisterDependencies()
        {
            var container = DependencyContainer.Instance;
            
            container.RegisterSingleton<ISillyFrontService, SillyFrontService>();
            
            // Register all view models by convention
            // THIS IS WRONG => it will reduce startup time: register them statically
            foreach (var viewModelType in 
                ProjectAssembly.ExportedTypes.Where(
                    type =>
                    type.Namespace.StartsWith("Mobile.Practices.Frameworkless.ViewModels")
                    && !type.GetTypeInfo().IsAbstract && type.Name.EndsWith("ViewModel")))
            {
                container.Register(viewModelType);
            }
        }

        private void InitializeLogTargets()
        {
            var config = new LoggingConfiguration();

#if DEBUG
            config.AddTarget(LogLevel.Info, LogLevel.Fatal, new DebugTarget());
#else
            config.AddTarget(LogLevel.Info, LogLevel.Fatal, new MemoryTarget(50));
#endif

            LoggerFactory.Initialize(config);
        }
    }
}