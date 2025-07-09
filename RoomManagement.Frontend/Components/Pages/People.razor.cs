using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using RoomManagement.Frontend.Components.FormModels;
using RoomManagement.Frontend.Services;
using RoomManager.Shared.Entities;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class People
    {
        // Data collections
        private List<Student> students = new();
        private List<Student> filteredStudents = new();
        private List<Professor> professors = new();
        private List<Professor> filteredProfessors = new();

        // Grid references
        private RadzenDataGrid<Student> studentsGrid;
        private RadzenDataGrid<Professor> professorsGrid;

        // Editing state for students
        private string studentColumnEditing = "";
        private List<Student> studentsToUpdate = new();

        // Editing state for professors
        private string professorColumnEditing = "";
        private List<Professor> professorsToUpdate = new();

        // UI state
        private bool loading = true;
        private string viewMode = "both";
        private string globalSearchTerm = "";
        private int PageSize = 10;

        // Options
        private List<dynamic> viewModeOptions = new()
    {
        new { Text = "Both", Value = "both" },
        new { Text = "Students Only", Value = "students" },
        new { Text = "Professors Only", Value = "professors" }
    };

        private List<dynamic> pageSizeOptions = new()
    {
        new { Label = "10", Value = 10 },
        new { Label = "25", Value = 25 },
        new { Label = "50", Value = 50 },
        new { Label = "100", Value = 100 },
        new { Label = "All", Value = -1 }
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
                await LoadStudents();
                await LoadProfessors();
                ApplyGlobalFilter();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Loading Failed",
                    Detail = $"Error loading data: {ex.Message}",
                    Duration = 5000
                });
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
                    s.MatriNumber?.ToLower().Contains(searchLower) == true).ToList();

                filteredProfessors = professors.Where(p =>
                    p.FirstName?.ToLower().Contains(searchLower) == true ||
                    p.LastName?.ToLower().Contains(searchLower) == true ||
                    p.Email?.ToLower().Contains(searchLower) == true).ToList();
            }
            StateHasChanged();
        }

        // Dialog methods for adding/editing
        private async Task OpenAddStudentDialog()
        {
            var student = new StudentFormModel();

            var result = await DialogService.OpenAsync<StudentFormDialog>("Add New Student",
                new Dictionary<string, object>()
                    {
                { "Student", student },
                { "IsEdit", false }
                    },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "400px",
                    Resizable = true,
                    Draggable = true
                });
            var substudent = result as StudentFormModel;
            if (result != null && result is StudentFormModel submittedStudent)
            {
                await AddStudent(substudent);
            }
        }

        private async Task OpenEditStudentDialog(Student student)
        {
            var studentForm = new StudentFormModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                MatriNumber = student.MatriNumber
            };

            var result = await DialogService.OpenAsync<StudentFormDialog>("Edit Student",
                new Dictionary<string, object>()
                    {
                { "Student", studentForm },
                { "IsEdit", true }
                    },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "400px",
                    Resizable = true,
                    Draggable = true
                });
            var substudent = result as StudentFormModel;
            if (result != null && result is StudentFormModel submittedStudent)
            {
                await UpdateStudentFromForm(substudent);
            }
        }

        private async Task OpenAddProfessorDialog()
        {
            var professor = new ProfessorFormModel();

            var result = await DialogService.OpenAsync<ProfessorFormDialog>("Add New Professor",
                new Dictionary<string, object>()
                    {
                { "Professor", professor },
                { "IsEdit", false }
                    },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "350px",
                    Resizable = true,
                    Draggable = true
                });
            var subProf = result as ProfessorFormModel;
            if (result != null && result is ProfessorFormModel submittedProfessor)
            {
                await AddProfessor(subProf);
            }
        }

        private async Task OpenEditProfessorDialog(Professor professor)
        {
            var professorForm = new ProfessorFormModel
            {
                Id = professor.Id,
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                Email = professor.Email
            };

            var result = await DialogService.OpenAsync<ProfessorFormDialog>("Edit Professor",
                new Dictionary<string, object>()
                    {
                { "Professor", professorForm },
                { "IsEdit", true }
                    },
                new DialogOptions()
                {
                    Width = "500px",
                    Height = "350px",
                    Resizable = true,
                    Draggable = true
                });
            var subprof = result as ProfessorFormModel;
            if (result != null && result is ProfessorFormModel submittedProfessor)
            {
                await UpdateProfessorFromForm(subprof);
            }
        }

        // Add/Update methods
        private async Task AddStudent(StudentFormModel studentForm)
        {
            try
            {
                var student = new Student
                {
                    FirstName = studentForm.FirstName,
                    LastName = studentForm.LastName,
                    Email = studentForm.Email,
                    MatriNumber = studentForm.MatriNumber
                };

                await StudentService.AddAsync(student);
                await LoadStudents();
                ApplyGlobalFilter();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Student Added",
                    Detail = $"Student '{student.FirstName} {student.LastName}' added successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Add Failed",
                    Detail = $"Error adding student: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private async Task UpdateStudentFromForm(StudentFormModel studentForm)
        {
            try
            {
                var student = students.FirstOrDefault(s => s.Id == studentForm.Id);
                if (student != null)
                {
                    student.FirstName = studentForm.FirstName;
                    student.LastName = studentForm.LastName;
                    student.Email = studentForm.Email;
                    student.MatriNumber = studentForm.MatriNumber;

                    await StudentService.UpdateAsync(student);
                    ApplyGlobalFilter();

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Student Updated",
                        Detail = $"Student '{student.FirstName} {student.LastName}' updated successfully",
                        Duration = 3000
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Update Failed",
                    Detail = $"Error updating student: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private async Task AddProfessor(ProfessorFormModel professorForm)
        {
            try
            {
                var professor = new Professor
                {
                    FirstName = professorForm.FirstName,
                    LastName = professorForm.LastName,
                    Email = professorForm.Email
                };

                await ProfessorService.AddAsync(professor);
                await LoadProfessors();
                ApplyGlobalFilter();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Professor Added",
                    Detail = $"Professor '{professor.FirstName} {professor.LastName}' added successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Add Failed",
                    Detail = $"Error adding professor: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private async Task UpdateProfessorFromForm(ProfessorFormModel professorForm)
        {
            try
            {
                var professor = professors.FirstOrDefault(p => p.Id == professorForm.Id);
                if (professor != null)
                {
                    professor.FirstName = professorForm.FirstName;
                    professor.LastName = professorForm.LastName;
                    professor.Email = professorForm.Email;

                    await ProfessorService.UpdateAsync(professor);
                    ApplyGlobalFilter();

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Professor Updated",
                        Detail = $"Professor '{professor.FirstName} {professor.LastName}' updated successfully",
                        Duration = 3000
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Update Failed",
                    Detail = $"Error updating professor: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        // Legacy grid editing methods (keeping for inline editing)
        private async Task OnUpdateStudent(Student student)
        {
            try
            {
                await StudentService.UpdateAsync(student);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Student Updated",
                    Detail = $"Student '{student.LastName}' updated successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Update Failed",
                    Detail = $"Error updating student: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private void OnStudentCellClick(DataGridCellMouseEventArgs<Student> args)
        {
            var columnProperty = args.Column.Property;

            if (studentsToUpdate.Any())
            {
                OnUpdateStudent(studentsToUpdate.First());
            }

            studentColumnEditing = columnProperty;
            studentsToUpdate.Clear();
            studentsToUpdate.Add(args.Data);
            studentsGrid.EditRow(args.Data);
        }

        private bool IsStudentEditing(string columnName, Student student)
        {
            return studentColumnEditing == columnName && studentsToUpdate.Contains(student);
        }

        private async Task OnUpdateProfessor(Professor professor)
        {
            try
            {
                await ProfessorService.UpdateAsync(professor);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Professor Updated",
                    Detail = $"Professor '{professor.LastName}' updated successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Update Failed",
                    Detail = $"Error updating professor: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private void OnProfessorCellClick(DataGridCellMouseEventArgs<Professor> args)
        {
            var columnProperty = args.Column.Property;

            if (professorsToUpdate.Any())
            {
                OnUpdateProfessor(professorsToUpdate.First());
            }

            professorColumnEditing = columnProperty;
            professorsToUpdate.Clear();
            professorsToUpdate.Add(args.Data);
            professorsGrid.EditRow(args.Data);
        }

        private bool IsProfessorEditing(string columnName, Professor professor)
        {
            return professorColumnEditing == columnName && professorsToUpdate.Contains(professor);
        }

        // Delete methods
        private async Task DeleteStudent(Student student)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to delete student '{student.FirstName} {student.LastName}'?");

                if (!confirmed) return;

                await StudentService.DeleteAsync(student.Id);
                students.RemoveAll(s => s.Id == student.Id);
                ApplyGlobalFilter();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Student Deleted",
                    Detail = $"Student '{student.FirstName} {student.LastName}' deleted successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Delete Failed",
                    Detail = $"Error deleting student: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        private async Task DeleteProfessor(Professor professor)
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to delete professor '{professor.FirstName} {professor.LastName}'?");

                if (!confirmed) return;

                await ProfessorService.DeleteAsync(professor.Id);
                professors.RemoveAll(p => p.Id == professor.Id);
                ApplyGlobalFilter();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Professor Deleted",
                    Detail = $"Professor '{professor.FirstName} {professor.LastName}' deleted successfully",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Delete Failed",
                    Detail = $"Error deleting professor: {ex.Message}",
                    Duration = 5000
                });
            }
        }

        // Page size change
        private async Task OnPageSizeChanged(object selectedPageSize)
        {
            PageSize = (int)selectedPageSize == -1
                ? Math.Max(filteredStudents.Count, filteredProfessors.Count)
                : (int)selectedPageSize;

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
    }
}