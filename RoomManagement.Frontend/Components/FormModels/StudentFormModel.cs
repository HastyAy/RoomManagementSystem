using System.ComponentModel.DataAnnotations;

namespace RoomManagement.Frontend.Components.FormModels
{
    public class StudentFormModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Matrikelnummer is required")]
        [StringLength(20, ErrorMessage = "Matrikelnummer cannot exceed 20 characters")]
        public string MatriNumber { get; set; } = "";
    }
}
