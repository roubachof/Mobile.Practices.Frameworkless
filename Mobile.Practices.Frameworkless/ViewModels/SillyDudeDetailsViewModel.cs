using System;
using System.Threading.Tasks;

using MetroLog;

using Mobile.Practices.Frameworkless.Core;
using Mobile.Practices.Frameworkless.Core.Nito;
using Mobile.Practices.Frameworkless.Models;
using Mobile.Practices.Frameworkless.Services;
using Mobile.Practices.Frameworkless.ViewModels.Extensions;

using SeLoger.Contracts;

namespace Mobile.Practices.Frameworkless.ViewModels
{
    public class SillyDudeDetailsViewModel
    {
        private static readonly ILogger Log = LoggerFactory.GetLogger(nameof(SillyDudeDetailsViewModel));
        
        private readonly ISillyFrontService _sillyFrontService;

        private NotifyTask<SillyDudeModel> _loadingTask;

        private int _sillyDudeId;

        private SillyDudeModel _model;

        public SillyDudeDetailsViewModel(ISillyFrontService sillyFrontService)
        {
            Log.Info("Constructing SillyDudeDetailsViewModel");

            _sillyFrontService = sillyFrontService;
        }        

        public string PhotoUrl => _model.ImageUrl;

        public string Name => _model.Name;

        public string Role => _model.Role;

        public string Description => _model.Description;

        public void Initialize(int sillyDudeId)
        {
            Contract.Requires(() => sillyDudeId > 0);

            _sillyDudeId = sillyDudeId;
        }

        /// <summary>
        /// Direct workflow View => ViewModel
        /// </summary>
        public void Load(Action<ViewModelState> onLoadCompleted)
        { 
            _loadingTask = new NotifyTaskBase.Builder<SillyDudeModel>(() => LoadSillyDude(_sillyDudeId))
                .WithWhenCompleted(() => onLoadCompleted(GetState()))
                .Build();

            _loadingTask.Start();
        }

        public ViewModelState GetState()
        {
            return _loadingTask.ToViewModelState();
        }

        private async Task<SillyDudeModel> LoadSillyDude(int id)
        {
            Contract.Requires(() => id > 0);

            Log.Info($"LoadSillyDude id {id}");

            return _model = await _sillyFrontService.GetSilly(id);
        }        
    }
}
