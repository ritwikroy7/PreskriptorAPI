using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PreskriptorAPI
{
    public class Prescription
    {
        public string PrescriptionID {get; set;}
        public string PrescriptionDate { get; set; }
        [Required]
        public Patient PatientInfo {get; set;}
        public Findings Findings { get; set; }
        public List<string> Tests { get; set; }
        public List<Medication> Medications { get; set; }
        public List<string> FollowUp { get; set; } 			
        public List<string> PatientResponse { get; set; }
        public Letterhead Letterhead { get; set; }
    }

    public class Findings
    {
        public List<string> ChiefComplaints { get; set; }
        public List<string> PersonalHistory { get; set; }
        public List<string> FamilyHistory { get; set; }
        public List<string> Examinations { get; set; }
        public List<string> AdditionalFindings { get; set; }
    }

    public class Medication : Drug
    {
        public string Dosage { get; set; }
    }
}