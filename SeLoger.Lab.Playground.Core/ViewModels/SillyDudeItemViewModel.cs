using SeLoger.Lab.Playground.Core.Models;

namespace SeLoger.Lab.Playground.Core.ViewModels
{
    public class SillyDudeItemViewModel
    {
        public SillyDudeItemViewModel(SillyDudeModel model)
        {
            ImageUrl = model.ImageUrl;
            Name = $"{model.Id}. {model.Name}";
            Description = model.Description;
        }

        public string ImageUrl { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
