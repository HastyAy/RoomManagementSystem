using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManagement.Frontend.Components.FormModels;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class StudentFormDialog
    {
        [Parameter] public StudentFormModel Student { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private void OnSubmit()
        {
            DialogService.Close(Student);
        }

        private void Cancel()
        {
            DialogService.Close(null);
        }
    }
}