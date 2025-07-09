using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.DTOs;
using RoomManager.Shared.DTOs.UserDto;
using RoomManagement.Frontend.Components.FormDialogs;
namespace RoomManagement.Frontend.Components.Pages
{
    public partial class People
    {
        // Data collections
        private List<StudentDto> students = new();
        private List<StudentDto> filteredStudents = new();
        private List<ProfessorDto> professors = new();
        private List<ProfessorDto> filteredProfessors = new();

        // Grid references
        private RadzenDataGrid<StudentDto>? studentsGrid;
        private RadzenDataGrid<ProfessorDto>? professorsGrid;

        // UI state
        private bool loading = true;
        private string viewMode = "both";
        private string globalSearchTerm = "";
        private int PageSize = 10;

        // Options
        private readonly List<dynamic> viewModeOptions = new()
        {
            new { Text = "Both", Value = "both" },
            new { Text = "Students Only", Value = "students" },
            new { Text = "Professors Only", Value = "professors" }
        };

        private readonly List<dynamic> pageSizeOptions = new()
        {
            new { Label = "10", Value = 10 },
            new { Label = "25", Value = 25 },
            new { Label = "50", Value = 50 },
            new { Label = "100", Value = 100 }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            try
            {
                await Task.WhenAll(LoadStudents(), LoadProfessors());
                ApplyGlobalFilter();
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Loading Failed", $"Error loading data: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private async Task LoadStudents()
        {
            students = await StudentService.GetAllAsync();
        }

        private async Task LoadProfessors()
        {
            professors = await ProfessorService.GetAllAsync();
        }

        private void ApplyGlobalFilter()
        {
            if (string.IsNullOrEmpty(globalSearchTerm))
            {
                filteredStudents = students.ToList();
                filteredProfessors = professors.ToList();
            }
            else
            {
                var searchLower = globalSearchTerm.ToLower();
                filteredStudents = students.Where(s =>
                    s.FirstName?.ToLower().Contains(searchLower) == true ||
                    s.LastName?.ToLower().Contains(searchLower) == true ||
                    s.Email?.ToLower().Contains(searchLower) == true ||
                    s.MatriNumber?.ToLower().Contains(searchLower) == true ||
                    s.FullName?.ToLower().Contains(searchLower) == true).ToList();

                filteredProfessors = professors.Where(p =>
                    p.FirstName?.ToLower().Contains(searchLower) == true ||
                    p.LastName?.ToLower().Contains(searchLower) == true ||
                    p.Email?.ToLower().Contains(searchLower) == true ||
                    p.Department?.ToLower().Contains(searchLower) == true ||
                    p.Title?.ToLower().Contains(searchLower) == true ||
                    p.FullName?.ToLower().Contains(searchLower) == true).ToList();
            }
            StateHasChanged();
        }

        // Dialog methods for adding/editing students
        private async Task OpenAddStudentDialog()
        {
            var createRequest = new CreateStudentRequest();

            var result = await DialogService.OpenAsync<StudentFormDialog>("Add New Student",
                new Dictionary<string, object>
                {
                    { "Student", createRequest },
                    { "IsEdit", false }
                },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "450px",
                    Resizable = true,
                    Draggable = true
                });

            if (result is CreateStudentRequest submittedStudent)
            {
                await AddStudent(submittedStudent);
            }
        }

        private async Task OpenEditStudentDialog(StudentDto student)
        {
            var updateRequest = new CreateStudentRequest // Using CreateStudentRequest for updates too
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                MatriNumber = student.MatriNumber
            };

            var result = await DialogService.OpenAsync<StudentFormDialog>("Edit Student",
                new Dictionary<string, object>
                {
                    { "Student", updateRequest },
                    { "IsEdit", true }
                },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "450px",
                    Resizable = true,
                    Draggable = true
                });

            if (result is CreateStudentRequest submittedStudent)
            {
                await UpdateStudent(student.Id, submittedStudent);
            }
        }

        // Dialog methods for adding/editing professors
        private async Task OpenAddProfessorDialog()
        {
            var createRequest = new CreateProfessorRequest();

            var result = await DialogService.OpenAsync<ProfessorFormDialog>("Add New Professor",
                new Dictionary<string, object>
                {
                    { "Professor", createRequest },
                    { "IsEdit", false }
                },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "450px",
                    Resizable = true,
                    Draggable = true
                });

            if (result is CreateProfessorRequest submittedProfessor)
            {
                await AddProfessor(submittedProfessor);
            }
        }

        private async Task OpenEditProfessorDialog(ProfessorDto professor)
        {
            var updateRequest = new CreateProfessorRequest // Using CreateProfessorRequest for updates too
            {
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                Email = professor.Email,
                Department = professor.Department,
                Title = professor.Title
            };

            var result = await DialogService.OpenAsync<ProfessorFormDialog>("Edit Professor",
                new Dictionary<string, object>
                {
                    { "Professor", updateRequest },
                    { "IsEdit", true }
                },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "450px",
                    Resizable = true,
                    Draggable = true
                });

            if (result is CreateProfessorRequest submittedProfessor)
            {
                await UpdateProfessor(professor.Id, submittedProfessor);
            }
        }

        // CRUD operations for students
        private async Task AddStudent(CreateStudentRequest studentRequest)
        {
            try
            {
                var success = await StudentService.AddAsync(studentRequest);

                if (success)
                {
                    await LoadStudents();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Student Added", $"Student '{studentRequest.FirstName} {studentRequest.LastName}' added successfully");
                }
                else
                {
                    ShowErrorNotification("Add Failed", "Failed to add student");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Add Failed", $"Error adding student: {ex.Message}");
            }
        }

        private async Task UpdateStudent(Guid studentId, CreateStudentRequest studentRequest)
        {
            try
            {
                var success = await StudentService.UpdateAsync(studentId, studentRequest);

                if (success)
                {
                    await LoadStudents();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Student Updated", $"Student '{studentRequest.FirstName} {studentRequest.LastName}' updated successfully");
                }
                else
                {
                    ShowErrorNotification("Update Failed", "Failed to update student");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Update Failed", $"Error updating student: {ex.Message}");
            }
        }

        private async Task DeleteStudent(StudentDto student)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to delete student '{student.FullName}'? This action cannot be undone.");

                if (!confirmed) return;

                var success = await StudentService.DeleteAsync(student.Id);

                if (success)
                {
                    await LoadStudents();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Student Deleted", $"Student '{student.FullName}' deleted successfully");
                }
                else
                {
                    ShowErrorNotification("Delete Failed", "Failed to delete student");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Delete Failed", $"Error deleting student: {ex.Message}");
            }
        }

        // CRUD operations for professors
        private async Task AddProfessor(CreateProfessorRequest professorRequest)
        {
            try
            {
                var success = await ProfessorService.AddAsync(professorRequest);

                if (success)
                {
                    await LoadProfessors();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Professor Added", $"Professor '{professorRequest.FirstName} {professorRequest.LastName}' added successfully");
                }
                else
                {
                    ShowErrorNotification("Add Failed", "Failed to add professor");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Add Failed", $"Error adding professor: {ex.Message}");
            }
        }

        private async Task UpdateProfessor(Guid professorId, CreateProfessorRequest professorRequest)
        {
            try
            {
                var success = await ProfessorService.UpdateAsync(professorId, professorRequest);

                if (success)
                {
                    await LoadProfessors();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Professor Updated", $"Professor '{professorRequest.FirstName} {professorRequest.LastName}' updated successfully");
                }
                else
                {
                    ShowErrorNotification("Update Failed", "Failed to update professor");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Update Failed", $"Error updating professor: {ex.Message}");
            }
        }

        private async Task DeleteProfessor(ProfessorDto professor)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to delete professor '{professor.FullName}'? This action cannot be undone.");

                if (!confirmed) return;

                var success = await ProfessorService.DeleteAsync(professor.Id);

                if (success)
                {
                    await LoadProfessors();
                    ApplyGlobalFilter();
                    ShowSuccessNotification("Professor Deleted", $"Professor '{professor.FullName}' deleted successfully");
                }
                else
                {
                    ShowErrorNotification("Delete Failed", "Failed to delete professor");
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Delete Failed", $"Error deleting professor: {ex.Message}");
            }
        }

        // Page size change
        private async Task OnPageSizeChanged(object selectedPageSize)
        {
            PageSize = (int)selectedPageSize;

            if (studentsGrid != null)
            {
                await studentsGrid.GoToPage(0);
                await studentsGrid.Reload();
            }

            if (professorsGrid != null)
            {
                await professorsGrid.GoToPage(0);
                await professorsGrid.Reload();
            }
        }

        // Notification helpers
        private void ShowSuccessNotification(string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = summary,
                Detail = detail,
                Duration = 3000
            });
        }

        private void ShowErrorNotification(string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = summary,
                Detail = detail,
                Duration = 5000
            });
        }
    }
}