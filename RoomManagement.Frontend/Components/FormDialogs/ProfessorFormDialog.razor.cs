using Microsoft.AspNetCore.Components;
using Radzen;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManagement.Frontend.Components.FormDialogs
{
    public partial class ProfessorFormDialog
    {
        [Parameter] public CreateProfessorRequest Professor { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private bool saving = false;

        private readonly List<string> departmentOptions = new()
    {
        "Computer Science",
        "Mathematics",
        "Physics",
        "Chemistry",
        "Biology",
        "Engineering",
        "Business Administration",
        "Economics",
        "Psychology",
        "Philosophy",
        "History",
        "Literature",
        "Art",
        "Music",
        "Medicine",
        "Law",
        "Education",
        "Social Sciences",
        "Environmental Sciences",
        "Architecture"
    };

        private readonly List<string> titleOptions = new()
    {
        "Professor",
        "Associate Professor",
        "Assistant Professor",
        "Lecturer",
        "Senior Lecturer",
        "Adjunct Professor",
        "Visiting Professor",
        "Professor Emeritus",
        "Research Professor",
        "Clinical Professor",
        "Professor of Practice",
        "Instructor"
    };

        private async Task OnSubmit()
        {
            saving = true;

            try
            {
                // Additional validation if needed
                if (string.IsNullOrWhiteSpace(Professor.FirstName) ||
                    string.IsNullOrWhiteSpace(Professor.LastName) ||
                    string.IsNullOrWhiteSpace(Professor.Email))
                {
                    return;
                }

                // Trim whitespace
                Professor.FirstName = Professor.FirstName.Trim();
                Professor.LastName = Professor.LastName.Trim();
                Professor.Email = Professor.Email.Trim().ToLower();
                Professor.Department = string.IsNullOrWhiteSpace(Professor.Department) ? null : Professor.Department.Trim();
                Professor.Title = string.IsNullOrWhiteSpace(Professor.Title) ? null : Professor.Title.Trim();

                DialogService.Close(Professor);
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
            var firstName = Professor.FirstName?.Trim() ?? "";
            var lastName = Professor.LastName?.Trim() ?? "";

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return "No name entered";

            return $"{firstName} {lastName}".Trim();
        }
    }
}