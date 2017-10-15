using System.ComponentModel.DataAnnotations;
public class Letterhead
    {      
        public string DoctorName { get; set; }        
        public string Degree { get; set; }
        public string Specialization { get; set; }
        [Required]              
        public string ChamberName { get; set; }
        public string ChamberAddressLine1 { get; set; }
        public string ChamberAddressLine2 { get; set; }
        public string ChamberAddressLine3 { get; set; }
        public string ChamberPhone { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }   
        public string Website { get; set; }
        public string Timings { get; set; }
    }