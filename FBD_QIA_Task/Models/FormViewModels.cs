using System.ComponentModel.DataAnnotations;

namespace FBD_QIA_Task.Models
{
    public class FormViewModels
    {
        public class FormFieldViewModel
        {
            public int FieldId { get; set; } 

            [Required(ErrorMessage = "Label is required")]
            public string Label { get; set; }

            public bool IsRequired { get; set; }
            public string SelectedOption { get; set; }
        }

        public class FormViewModel
        {
            public int FormId { get; set; }

            [Required(ErrorMessage = "Form title is required")]
            [StringLength(200)]
            public string Title { get; set; }

            // dynamic fields
            public List<FormFieldViewModel> Fields { get; set; } = new List<FormFieldViewModel>();
        }
    }
}
