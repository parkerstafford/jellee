using CommunityToolkit.Mvvm.Input;
using Jellee.Models;

namespace Jellee.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}