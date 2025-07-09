using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManagement.Frontend.Components.FormModels;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class ProfessorFormDialog
    {
        [Parameter] public ProfessorFormModel Professor { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private void OnSubmit()
        {
            DialogService.Close(Professor);
        }

        private void Cancel()
        {
            DialogService.Close(null);
        }
    }
}