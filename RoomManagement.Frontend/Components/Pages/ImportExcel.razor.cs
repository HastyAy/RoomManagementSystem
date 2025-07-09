using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using RoomManagement.Frontend.Services;
using System.Collections.Generic;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class ImportExcel
    {
        private RadzenUpload? uploadComponent;
        private List<UploadedFile> uploadedFiles = new();
        private ImportResults? importResults;
        private bool isImporting = false;
        private double importProgress = 0;
        private string importStatus = "";

        private string _importType = "students";
        private string importType
        {
            get => _importType;
            set
            {
                if (_importType != value)
                {
                    _importType = value;
                    OnImportTypeChanged();
                }
            }
        }

        private readonly List<dynamic> importTypeOptions = new()
        {
            new { Text = "Students", Value = "students" },
            new { Text = "Professors", Value = "professors" }
        };

        // Add this method to handle the change
        private void OnImportTypeChanged()
        {
            // Clear results when import type changes
            importResults = null;
            StateHasChanged();
        }
        private string GetCardCssClass(UploadedFile file)
        {
            return file.IsSelected ? "file-card file-card-selected" : "file-card";
        }
        private async Task OnFileChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                try
                {
                    // Check file type
                    if (!IsValidExcelFile(file.Name))
                    {
                        ShowErrorNotification("Invalid File Type", $"{file.Name} is not a valid Excel file. Please upload .xlsx or .xls files only.");
                        continue;
                    }

                    // Check if file already exists
                    if (uploadedFiles.Any(f => f.Name == file.Name))
                    {
                        ShowWarningNotification("Duplicate File", $"{file.Name} is already in the upload list.");
                        continue;
                    }

                    var ms = new MemoryStream();
                    await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);

                    uploadedFiles.Add(new UploadedFile
                    {
                        Name = file.Name,
                        Size = file.Size,
                        Content = ms.ToArray(),
                        IsSelected = true
                    });

                    ShowSuccessNotification("File Added", $"{file.Name} has been added to the upload queue.");
                }
                catch (Exception ex)
                {
                    ShowErrorNotification("Upload Error", $"Failed to upload {file.Name}: {ex.Message}");
                }
            }
            StateHasChanged();
        }


        private void SelectAllFiles()
        {
            uploadedFiles.ForEach(f => f.IsSelected = true);
            StateHasChanged();
        }

        private void ClearAllFiles()
        {
            uploadedFiles.ForEach(f => f.IsSelected = false);
            StateHasChanged();
        }

        private void RemoveFile(UploadedFile file)
        {
            uploadedFiles.Remove(file);
            StateHasChanged();
        }

        private void ClearResults()
        {
            importResults = null;
            uploadedFiles.Clear();
            importProgress = 0;
            importStatus = "";
            StateHasChanged();
        }

        private async Task ImportSelectedFiles()
        {
            var selectedFiles = uploadedFiles.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any())
            {
                ShowWarningNotification("No Files Selected", "Please select at least one file to import.");
                return;
            }

            isImporting = true;
            importProgress = 0;
            importResults = new ImportResults();

            try
            {
                for (int i = 0; i < selectedFiles.Count; i++)
                {
                    var file = selectedFiles[i];
                    importStatus = $"Processing {file.Name}... ({i + 1} of {selectedFiles.Count})";
                    importProgress = (double)(i * 100) / selectedFiles.Count;
                    StateHasChanged();

                    // Add delay for better UX
                    await Task.Delay(100);

                    var fileResult = await ImportService.ImportExcelFileAsync(file.Content, file.Name, importType);

                    // Merge results
                    importResults.TotalCount += fileResult.TotalCount;
                    importResults.SuccessCount += fileResult.SuccessCount;
                    importResults.ErrorCount += fileResult.ErrorCount;

                    importProgress = (double)((i + 1) * 100) / selectedFiles.Count;
                    StateHasChanged();
                }

                importStatus = "Import completed successfully!";
                importProgress = 100;

                // Show completion notification
                if (importResults.ErrorCount > 0)
                {
                    ShowWarningNotification("Import Completed with Errors",
                        $"Imported {importResults.SuccessCount} of {importResults.TotalCount} records. {importResults.ErrorCount} records failed.");
                }
                else
                {
                    ShowSuccessNotification("Import Completed",
                        $"Successfully imported all {importResults.SuccessCount} records!");
                }

                // Remove successfully imported files from the list
                foreach (var file in selectedFiles)
                {
                    uploadedFiles.Remove(file);
                }
            }
            catch (Exception ex)
            {
                ShowErrorNotification("Import Error", $"Error during import process: {ex.Message}");
                importStatus = "Import failed due to an error.";
            }
            finally
            {
                isImporting = false;
                StateHasChanged();
            }
        }

        private async Task ShowTemplateDialog()
        {
            var message = importType == "students"
                ? "Student template should contain: FirstName, LastName, MatriNumber, Email"
                : "Professor template should contain: FirstName, LastName, Email, Department (optional), Title (optional)";

            await JSRuntime.InvokeVoidAsync("alert", $"Excel Template Format:\n\n{message}\n\nCreate an Excel file with these column headers in the first row, then add your data in the subsequent rows.");
        }

        private static bool IsValidExcelFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".xlsx" || extension == ".xls";
        }

        private static string FormatFileSize(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return $"{decimal.Divide(bytes, max):##.##} {order}";

                max /= scale;
            }
            return "0 Bytes";
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

        private void ShowWarningNotification(string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = summary,
                Detail = detail,
                Duration = 4000
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

        // Data classes
        private class UploadedFile
        {
            public string Name { get; set; } = "";
            public long Size { get; set; }
            public byte[] Content { get; set; } = Array.Empty<byte>();
            public bool IsSelected { get; set; } = true;
        }

        public class ImportResults
        {
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int ErrorCount { get; set; }
            public List<ImportError> Errors { get; set; } = new();
        }

        public class ImportError
        {
            public int Row { get; set; }
            public string Message { get; set; } = "";
            public string FileName { get; set; } = "";
        }
    }
}