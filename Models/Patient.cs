using System.ComponentModel.DataAnnotations;

namespace PreskriptorAPI
{
    public class Patient
    {
            public string PatientID { get; set; }
            [Required]
            public string PatientName { get; set; }
            public string Title { get; set; }
            public int Age { get; set; }
            public string BloodGroup { get; set; }
            public string Parity { get; set; }
            public string ContactNumber { get; set; }
            public string Email { get; set; }
    }
}
