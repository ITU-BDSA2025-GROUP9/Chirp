using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages.Shared
{
    public class InputModel
    {
        [Required]
        [StringLength(160, ErrorMessage = "The maximum length is 160 characters.")]
        public string Text { get; set; } = string.Empty;

    }
}