using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages.Shared
{
    /// <summary>
    /// Representing input fata for submitting text-based content,
    /// such as creating a new cheep or comment
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// The text content provided by the user
        /// Must be non-empty and no longer than 160 characters
        /// </summary>
        [Required]
        [StringLength(160, ErrorMessage = "The maximum length is 160 characters.")]
        public string Text { get; set; } = string.Empty;

    }
}