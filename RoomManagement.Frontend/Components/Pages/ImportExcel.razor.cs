using Radzen;
using Radzen.Blazor;
using RoomManagement.Frontend.Services;

namespace RoomManagement.Frontend.Components.Pages
{
    public partial class ImportExcel
    {
        private RadzenUpload uploadComponent;
        private List<UploadedFile> uploadedFiles = new();
        private ImportResults? importResults;

        private bool isImporting = false;
        private double importProgress = 0;
        private string importStatus = "";

        private string importType = "students";

        private List<dynamic> importTypeOptions = new()
    {
        new { Text = "Students", Value = "students" },
        new { Text = "Professors", Value = "professors" }
    };

        private async Task OnFileChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                try
                {
                    var ms = new MemoryStream();
                    await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);

                    uploadedFiles.Add(new UploadedFile
                    {
                        Name = file.Name,
                        Size = file.Size,
                        Content = ms.ToArray(),
                        IsSelected = true
                    });
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Upload Error",
                        Detail = $"Failed to upload {file.Name}: {ex.Message}",
                        Duration = 5000
                    });
                }
            }
            StateHasChanged();
        }

        private void SelectAllFiles()
        {
            uploadedFiles.ForEach(f => f.IsSelected = true);
        }

        private void ClearAllFiles()
        {
            uploadedFiles.ForEach(f => f.IsSelected = false);
        }

        private void RemoveFile(UploadedFile file)
        {
            uploadedFiles.Remove(file);
        }

        private void ClearResults()
        {
            importResults = null;
            uploadedFiles.Clear();
            StateHasChanged();
        }

        private async Task ImportSelectedFiles()
        {
            var selectedFiles = uploadedFiles.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any()) return;

            isImporting = true;
            importProgress = 0;
            importResults = new ImportResults();

            try
            {
                for (int i = 0; i < selectedFiles.Count; i++)
                {
                    var file = selectedFiles[i];
                    importStatus = $"Importing {file.Name}...";
                    importProgress = (double)(i * 100) / selectedFiles.Count;
                    StateHasChanged();

                    var fileResult = await ImportService.ImportExcelFileAsync(file.Content, file.Name, importType);

                    // Merge results
                    importResults.TotalCount += fileResult.TotalCount;
                    importResults.SuccessCount += fileResult.SuccessCount;
                    importResults.ErrorCount += fileResult.ErrorCount;

                    importProgress = (double)((i + 1) * 100) / selectedFiles.Count;
                    StateHasChanged();
                }

                importStatus = "Import complete!";

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Import Complete",
                    Detail = $"Successfully imported {importResults.SuccessCount} of {importResults.TotalCount} records",
                    Duration = 5000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Import Error",
                    Detail = $"Error importing files: {ex.Message}",
                    Duration = 5000
                });
            }
            finally
            {
                isImporting = false;
                importProgress = 0;
                importStatus = "";
            }
        }

        private string FormatFileSize(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
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