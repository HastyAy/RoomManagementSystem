using OfficeOpenXml;
using RoomManager.Shared.Entities;
using System.Net.Mail;

namespace RoomManagement.Frontend.Services
{
    public class ImportService
    {
        private readonly StudentService _studentService;
        private readonly ProfessorService _professorService;
        private readonly ILogger<ImportService> _logger;

        public ImportService(
            StudentService studentService,
            ProfessorService professorService,
            ILogger<ImportService> logger)
        {
            _studentService = studentService;
            _professorService = professorService;
            _logger = logger;
        }

        public async Task<ImportResult> ImportExcelFileAsync(byte[] fileContent, string fileName, string importType)
        {
            var result = new ImportResult();

            try
            {
                using var stream = new MemoryStream(fileContent);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add(new ImportError
                    {
                        Row = 0,
                        Message = "No worksheet found in the Excel file",
                        FileName = fileName
                    });
                    return result;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                {
                    result.Errors.Add(new ImportError
                    {
                        Row = 0,
                        Message = "No data rows found (only header or empty file)",
                        FileName = fileName
                    });
                    return result;
                }

                if (importType == "students")
                {
                    return await ImportStudentsFromWorksheet(worksheet, fileName);
                }
                else if (importType == "professors")
                {
                    return await ImportProfessorsFromWorksheet(worksheet, fileName);
                }
                else
                {
                    result.Errors.Add(new ImportError
                    {
                        Row = 0,
                        Message = $"Invalid import type: {importType}",
                        FileName = fileName
                    });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing Excel file {FileName}", fileName);
                result.Errors.Add(new ImportError
                {
                    Row = 0,
                    Message = $"Error processing file: {ex.Message}",
                    FileName = fileName
                });
                return result;
            }
        }

        private async Task<ImportResult> ImportStudentsFromWorksheet(ExcelWorksheet worksheet, string fileName)
        {
            var result = new ImportResult();
            var rowCount = worksheet.Dimension.Rows;

            // Get existing students to check for duplicates
            var existingStudents = await _studentService.GetAllAsync();
            var existingEmails = existingStudents.Select(s => s.Email?.ToLower()).ToHashSet();
            var existingMatriNumbers = existingStudents.Select(s => s.MatriNumber?.ToLower()).ToHashSet();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Check if row has any data
                    if (IsRowEmpty(worksheet, row, 4))
                        continue;

                    var firstName = worksheet.Cells[row, 1].Text?.Trim() ?? "";
                    var lastName = worksheet.Cells[row, 2].Text?.Trim() ?? "";
                    var matriNumber = worksheet.Cells[row, 3].Text?.Trim() ?? "";
                    var email = worksheet.Cells[row, 4].Text?.Trim() ?? "";

                    result.TotalCount++;

                    // Validate data
                    var validationErrors = ValidateStudent(firstName, lastName, matriNumber, email, existingEmails, existingMatriNumbers);
                    if (validationErrors.Any())
                    {
                        result.ErrorCount++;
                        result.Errors.AddRange(validationErrors.Select(error => new ImportError
                        {
                            Row = row,
                            Message = error,
                            FileName = fileName
                        }));
                        continue;
                    }

                    // Create and save student
                    var student = new Student
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        MatriNumber = matriNumber,
                        Email = email
                    };

                    await _studentService.AddAsync(student);

                    // Add to existing collections to prevent duplicates in the same file
                    existingEmails.Add(email.ToLower());
                    existingMatriNumbers.Add(matriNumber.ToLower());

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing student at row {Row} in file {FileName}", row, fileName);
                    result.ErrorCount++;
                    result.Errors.Add(new ImportError
                    {
                        Row = row,
                        Message = $"Error saving student: {ex.Message}",
                        FileName = fileName
                    });
                }
            }

            return result;
        }

        private async Task<ImportResult> ImportProfessorsFromWorksheet(ExcelWorksheet worksheet, string fileName)
        {
            var result = new ImportResult();
            var rowCount = worksheet.Dimension.Rows;

            // Get existing professors to check for duplicates
            var existingProfessors = await _professorService.GetAllAsync();
            var existingEmails = existingProfessors.Select(p => p.Email?.ToLower()).ToHashSet();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Check if row has any data
                    if (IsRowEmpty(worksheet, row, 3))
                        continue;

                    var firstName = worksheet.Cells[row, 1].Text?.Trim() ?? "";
                    var lastName = worksheet.Cells[row, 2].Text?.Trim() ?? "";
                    var email = worksheet.Cells[row, 3].Text?.Trim() ?? "";

                    result.TotalCount++;

                    // Validate data
                    var validationErrors = ValidateProfessor(firstName, lastName, email, existingEmails);
                    if (validationErrors.Any())
                    {
                        result.ErrorCount++;
                        result.Errors.AddRange(validationErrors.Select(error => new ImportError
                        {
                            Row = row,
                            Message = error,
                            FileName = fileName
                        }));
                        continue;
                    }

                    // Create and save professor
                    var professor = new Professor
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email
                    };

                    await _professorService.AddAsync(professor);

                    // Add to existing collection to prevent duplicates in the same file
                    existingEmails.Add(email.ToLower());

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing professor at row {Row} in file {FileName}", row, fileName);
                    result.ErrorCount++;
                    result.Errors.Add(new ImportError
                    {
                        Row = row,
                        Message = $"Error saving professor: {ex.Message}",
                        FileName = fileName
                    });
                }
            }

            return result;
        }

        private bool IsRowEmpty(ExcelWorksheet worksheet, int row, int columnCount)
        {
            for (int col = 1; col <= columnCount; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                {
                    return false;
                }
            }
            return true;
        }

        private List<string> ValidateStudent(string firstName, string lastName, string matriNumber, string email,
            HashSet<string> existingEmails, HashSet<string> existingMatriNumbers)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                errors.Add("Last name is required");

            if (string.IsNullOrWhiteSpace(matriNumber))
                errors.Add("Matrikelnummer is required");
            else if (existingMatriNumbers.Contains(matriNumber.ToLower()))
                errors.Add("Matrikelnummer already exists");

            if (string.IsNullOrWhiteSpace(email))
                errors.Add("Email is required");
            else if (!IsValidEmail(email))
                errors.Add("Invalid email format");
            else if (existingEmails.Contains(email.ToLower()))
                errors.Add("Email already exists");

            return errors;
        }

        private List<string> ValidateProfessor(string firstName, string lastName, string email, HashSet<string> existingEmails)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                errors.Add("Last name is required");

            if (string.IsNullOrWhiteSpace(email))
                errors.Add("Email is required");
            else if (!IsValidEmail(email))
                errors.Add("Invalid email format");
            else if (existingEmails.Contains(email.ToLower()))
                errors.Add("Email already exists");

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ImportResult
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