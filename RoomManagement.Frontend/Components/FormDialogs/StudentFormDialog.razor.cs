using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Components.FormDialogs
{
    public partial class StudentFormDialog
    {
        [Parameter] public CreateStudentRequest Student { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private bool saving = false;

        private async Task OnSubmit()
        {
            saving = true;

            try
            {
                // Additional validation if needed
                if (string.IsNullOrWhiteSpace(Student.FirstName) ||
                    string.IsNullOrWhiteSpace(Student.LastName) ||
                    string.IsNullOrWhiteSpace(Student.MatriNumber) ||
                    string.IsNullOrWhiteSpace(Student.Email))
                {
                    return;
                }

                // Trim whitespace
                Student.FirstName = Student.FirstName.Trim();
                Student.LastName = Student.LastName.Trim();
                Student.MatriNumber = Student.MatriNumber.Trim();
                Student.Email = Student.Email.Trim().ToLower();

                DialogService.Close(Student);
            }
            finally
            {
                saving = false;
            }
        }

        private void Cancel()
        {
            DialogService.Close();
        }

        private string GetFullName()
        {
            var firstName = Student.FirstName?.Trim() ?? "";
            var lastName = Student.LastName?.Trim() ?? "";

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return "No name entered";

            return $"{firstName} {lastName}".Trim();
        }
    }
}