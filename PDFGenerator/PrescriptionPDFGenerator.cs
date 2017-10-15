using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PreskriptorAPI.PDFGenerator
{
    public class PDFGeneratorException : Exception
    {
        public   PDFGeneratorException (string ErrorMessage) : base (ErrorMessage) {}

    }
    public interface IPrescriptionPDFGenerator
    {
        byte[] GeneratePDF(Prescription prescription);
    } 
    public class PrescriptionPDFGenerator : IPrescriptionPDFGenerator
    {
        private readonly ILogger<PrescriptionPDFGenerator> _log;
        public PrescriptionPDFGenerator(ILogger<PrescriptionPDFGenerator> log)
        {
            _log=log;
        }
        public byte[] GeneratePDF(Prescription prescription)
        {
            // FONT Style declaration
            string str_FontName = "Helvetica";
            string str_HeaderFontName = "Verdana";
            MemoryStream pobjMemoryStream = new MemoryStream();

            try
            {
                iTextSharp.text.Font f_Doctor_Name = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES, 16, iTextSharp.text.Font.BOLDITALIC, iTextSharp.text.BaseColor.Blue);
                iTextSharp.text.Font f_Header = iTextSharp.text.FontFactory.GetFont(str_HeaderFontName, 10, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.Black);
                iTextSharp.text.Font f_TableHeader = iTextSharp.text.FontFactory.GetFont(str_FontName, 8, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.Black);
                iTextSharp.text.Font f_SubHeader = iTextSharp.text.FontFactory.GetFont(str_FontName, 7, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.Blue);
                iTextSharp.text.Font f_data = iTextSharp.text.FontFactory.GetFont(str_FontName, 7, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.Black);

                float[] widths = new float[] { 25f, 75f };

                //Set Margin = 36
                Document document = new Document(PageSize.A4, 36, 36, 36, 36);
                document.AddTitle("Prescription - " + prescription.PatientInfo.PatientID);

                //New Line declaration
                Phrase p_BlankLine = new Phrase();
                p_BlankLine.Add(Environment.NewLine);

                //PDF declaration
                string FileName = "Prescription_" + prescription.PatientInfo.PatientID + ".pdf";                

                try
                {
                    //FileStream pobjFileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    //PdfWriter.GetInstance(document, pobjFileStream);
                    
                    PdfWriter.GetInstance(document, pobjMemoryStream);
                    document.Open();
                }
                catch (Exception eEx)
                {
                    _log.LogError("Unhandled Exception: "+eEx.Message);
                    throw new PDFGeneratorException ("An Error Occured While Generating Prescription PDF"); 
                }

                #region Letterhead            
                if (prescription.Letterhead != null)
                {
                    Chunk c_Doc_Name = new Chunk(prescription.Letterhead.DoctorName + "\n", f_Doctor_Name);
                    Chunk c_Degree = new Chunk(prescription.Letterhead.Degree + "\n", f_Header);
                    Chunk c_Specialization = new Chunk(prescription.Letterhead.Specialization + "\n", f_Header);

                    Phrase Letterhead = new Phrase();
                    Letterhead.Add(c_Doc_Name);
                    Letterhead.Add(c_Degree);
                    Letterhead.Add(c_Specialization);

                    PdfPTable t_TotalHeader = new PdfPTable(3);
                    t_TotalHeader.WidthPercentage = 100f;
                    t_TotalHeader.DefaultCell.Border = 0;
                    t_TotalHeader.DefaultCell.BorderColor = iTextSharp.text.BaseColor.White;

                    PdfPTable t_headerDetails = new PdfPTable(1);
                    t_headerDetails.DefaultCell.Border = 0;
                    t_headerDetails.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_headerDetails.DefaultCell.VerticalAlignment = Element.ALIGN_BASELINE;
                    t_headerDetails.WidthPercentage = 45f;

                    PdfPCell t_headerData = new PdfPCell(Letterhead);
                    t_headerData.Border = 0;
                    t_headerData.VerticalAlignment = Element.ALIGN_BASELINE;
                    t_headerData.BorderColor = iTextSharp.text.BaseColor.White;
                    t_headerDetails.AddCell(t_headerData);

                    t_TotalHeader.AddCell(t_headerDetails);

                    //Middle Space
                    PdfPTable t_HeaderMiddleSpace1 = new PdfPTable(1);
                    t_HeaderMiddleSpace1.DefaultCell.Border = 0;
                    t_HeaderMiddleSpace1.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_HeaderMiddleSpace1.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    t_HeaderMiddleSpace1.WidthPercentage = 20f;

                    PdfPCell t_headerSpace1 = new PdfPCell();
                    t_headerSpace1.Border = 0;
                    t_headerSpace1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    t_headerSpace1.BorderColor = iTextSharp.text.BaseColor.White;
                    t_HeaderMiddleSpace1.AddCell(t_headerSpace1);

                    t_TotalHeader.AddCell(t_HeaderMiddleSpace1);

                    //Chamber Details
                    PdfPTable t_Chamber = new PdfPTable(1);
                    t_Chamber.DefaultCell.Border = 0;
                    t_Chamber.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;
                    t_Chamber.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Chunk c_Chamber_Name = new Chunk(prescription.Letterhead.ChamberName.ToUpper() + "\n", f_Header);
                    Chunk c_Chamber_Address = new Chunk(prescription.Letterhead.ChamberAddressLine1 + "\n" + prescription.Letterhead.ChamberAddressLine2 + "\n" + prescription.Letterhead.ChamberAddressLine3 + "\n", f_data);
                    Chunk c_Chamber_Phone = new Chunk("Phone : " + prescription.Letterhead.ChamberPhone + "\n", f_data);
                    Chunk c_Chamber_Fax = new Chunk("Fax : " + prescription.Letterhead.Fax + "\n", f_data);
                    Chunk c_Mobile = new Chunk("Mobile : " + prescription.Letterhead.Mobile + "\n", f_data);
                    Chunk c_Email = new Chunk("E - mail : " + prescription.Letterhead.Email + "\n", f_data);
                    Chunk c_Website = new Chunk("Website : " + prescription.Letterhead.Website + "\n", f_data);
                    Chunk c_Timing = new Chunk("Day & Time : " + prescription.Letterhead.Timings + "\n", f_data);

                    Chunk c_Date = new Chunk("PrescriptionDate : " + prescription.PrescriptionDate, f_TableHeader);

                    Phrase ChamberDetails = new Phrase();
                    ChamberDetails.Add(c_Chamber_Name);
                    ChamberDetails.Add(c_Chamber_Address);
                    ChamberDetails.Add(c_Chamber_Phone);
                    if (!string.IsNullOrEmpty(prescription.Letterhead.Fax))
                    {
                        ChamberDetails.Add(c_Chamber_Fax);
                    }
                    ChamberDetails.Add(c_Mobile);
                    if (!string.IsNullOrEmpty(prescription.Letterhead.Email))
                    {
                        ChamberDetails.Add(c_Email);
                    }
                    if (!string.IsNullOrEmpty(prescription.Letterhead.Website))
                    {
                        ChamberDetails.Add(c_Website);
                    }
                    ChamberDetails.Add(c_Timing);
                    ChamberDetails.Add(p_BlankLine);
                    ChamberDetails.Add(c_Date);

                    PdfPCell c_Chamber = new PdfPCell(ChamberDetails);
                    c_Chamber.HorizontalAlignment = Element.ALIGN_LEFT;
                    c_Chamber.VerticalAlignment = Element.ALIGN_TOP;
                    c_Chamber.BorderColor = iTextSharp.text.BaseColor.White;

                    t_Chamber.AddCell(c_Chamber);
                    t_Chamber.WidthPercentage = 35f;

                    t_TotalHeader.AddCell(t_Chamber);

                    document.Add(t_TotalHeader);
                    document.Add(p_BlankLine);
                }
                #endregion

                #region Patient Information            

                //Outer Table
                PdfPTable t_PatintInfoMainTable = new PdfPTable(1);//3
                t_PatintInfoMainTable.WidthPercentage = 100f;
                t_PatintInfoMainTable.HorizontalAlignment = Element.ALIGN_LEFT;                
                float[] OuterTablewidth;

                //Patient Table
                PdfPTable t_patientInfo = new PdfPTable(4);
                t_patientInfo.WidthPercentage = 100f;                
                t_patientInfo.HorizontalAlignment = Element.ALIGN_LEFT;

                PdfPCell infoTbHeader1 = new PdfPCell(new Paragraph("PATIENT DETAILS", f_TableHeader));
                infoTbHeader1.GrayFill = 0.7f;
                //Alignment
                infoTbHeader1.HorizontalAlignment = Element.ALIGN_LEFT;
                infoTbHeader1.VerticalAlignment = Element.ALIGN_MIDDLE;
                t_PatintInfoMainTable.AddCell(new PdfPCell(new Paragraph("PATIENT DETAILS", f_TableHeader))
                {
                    Colspan = 4,
                    GrayFill = 0.7f,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Border = PdfPCell.NO_BORDER
                });
                
                //Patient Info Table Sub Letterhead
                PdfPCell TableHeaderName = new PdfPCell(new Paragraph("NAME", f_SubHeader));
                PdfPCell TableHeaderAge = new PdfPCell(new Paragraph("AGE", f_SubHeader));
                PdfPCell TableHeaderBloodGroup = new PdfPCell(new Paragraph("BLOOD GROUP", f_SubHeader));
                PdfPCell TableHeaderParity = new PdfPCell(new Paragraph("PARITY", f_SubHeader));

                TableHeaderName.HorizontalAlignment = Element.ALIGN_LEFT;
                TableHeaderName.VerticalAlignment = Element.ALIGN_MIDDLE;
                TableHeaderName.Border = PdfPCell.NO_BORDER;
                TableHeaderAge.HorizontalAlignment = Element.ALIGN_LEFT;
                TableHeaderAge.VerticalAlignment = Element.ALIGN_MIDDLE;
                TableHeaderAge.Border = PdfPCell.NO_BORDER;
                TableHeaderBloodGroup.HorizontalAlignment = Element.ALIGN_LEFT;
                TableHeaderBloodGroup.VerticalAlignment = Element.ALIGN_MIDDLE;
                TableHeaderBloodGroup.Border = PdfPCell.NO_BORDER;
                TableHeaderParity.HorizontalAlignment = Element.ALIGN_LEFT;
                TableHeaderParity.VerticalAlignment = Element.ALIGN_MIDDLE;
                TableHeaderParity.Border = PdfPCell.NO_BORDER;

                //Adding Sub Letterhead to the Table
                t_patientInfo.AddCell(TableHeaderName);
                t_patientInfo.AddCell(TableHeaderAge);
                t_patientInfo.AddCell(TableHeaderBloodGroup);
                t_patientInfo.AddCell(TableHeaderParity);

                PdfPCell C_Patient_Name;
                PdfPCell C_Patient_Age;
                PdfPCell C_Patient_BloodGroup;
                PdfPCell C_Patient_Parity;
            
                C_Patient_Name = new PdfPCell(new Paragraph(prescription.PatientInfo.PatientName, f_data));
                C_Patient_Age = new PdfPCell(new Paragraph(prescription.PatientInfo.Age.ToString(), f_data));
                C_Patient_BloodGroup = new PdfPCell(new Paragraph(prescription.PatientInfo.BloodGroup, f_data));
                C_Patient_Parity = new PdfPCell(new Paragraph(prescription.PatientInfo.Parity, f_data));

                //Aligning the Cells
                C_Patient_Name.HorizontalAlignment = Element.ALIGN_LEFT;
                C_Patient_Name.VerticalAlignment = Element.ALIGN_CENTER;
                C_Patient_Name.Border = PdfPCell.NO_BORDER;
                C_Patient_Age.HorizontalAlignment = Element.ALIGN_LEFT;
                C_Patient_Age.VerticalAlignment = Element.ALIGN_CENTER;
                C_Patient_Age.Border = PdfPCell.NO_BORDER;
                C_Patient_BloodGroup.HorizontalAlignment = Element.ALIGN_LEFT;
                C_Patient_BloodGroup.VerticalAlignment = Element.ALIGN_CENTER;
                C_Patient_BloodGroup.Border = PdfPCell.NO_BORDER;
                C_Patient_Parity.HorizontalAlignment = Element.ALIGN_LEFT;
                C_Patient_Parity.VerticalAlignment = Element.ALIGN_CENTER;
                C_Patient_Parity.Border = PdfPCell.NO_BORDER;

                //ADD the cell to Table
                t_patientInfo.AddCell(C_Patient_Name);
                t_patientInfo.AddCell(C_Patient_Age);
                t_patientInfo.AddCell(C_Patient_BloodGroup);
                t_patientInfo.AddCell(C_Patient_Parity);                

                PdfPCell c_PatientInfoTable = new PdfPCell(t_patientInfo);
                c_PatientInfoTable.Border = PdfPCell.NO_BORDER;

                //Assiging table to the Master table
                t_PatintInfoMainTable.AddCell(c_PatientInfoTable);                
                document.Add(t_PatintInfoMainTable);
                document.Add(p_BlankLine);

                #endregion

                #region Investigations            

                //Outer Table
                PdfPTable t_InvMainTable = new PdfPTable(1);//3
                t_InvMainTable.WidthPercentage = 100f;
                t_InvMainTable.HorizontalAlignment = Element.ALIGN_LEFT;
                t_InvMainTable.DefaultCell.Border = 0;

                //Outer Table Letterhead
                // Setting Width of the Column to Outer Table            
                OuterTablewidth = new float[] { 6.1f };
                t_InvMainTable.SetWidths(OuterTablewidth);
                t_InvMainTable.HorizontalAlignment = 0;

                PdfPCell invTbHeader1 = new PdfPCell(new Paragraph("INVESTIGATIONS", f_TableHeader));
                invTbHeader1.GrayFill = 0.7f;
                //Alignment
                invTbHeader1.HorizontalAlignment = Element.ALIGN_LEFT;
                invTbHeader1.VerticalAlignment = Element.ALIGN_MIDDLE;
                t_InvMainTable.AddCell(new PdfPCell(new Paragraph("INVESTIGATIONS", f_TableHeader))
                {
                    Colspan = 2,
                    GrayFill = 0.7f,
                    Border = PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_CENTER
                });


                #region Chief Complaints
                //Chief Complaints Table

                if (prescription.Findings != null && prescription.Findings.ChiefComplaints != null && prescription.Findings.ChiefComplaints.Count > 0)
                {
                    PdfPTable t_chiefComp = new PdfPTable(2);//2
                    t_chiefComp.WidthPercentage = 100f;
                    t_chiefComp.HorizontalAlignment = Element.ALIGN_LEFT;                    
                    t_chiefComp.SetWidths(widths);

                    Chunk c_SubHeading_ChiefComplaints = new Chunk("CHIEF COMPLAINTS", f_SubHeader);
                    //t_chiefComp.AddCell(new Phrase(c_SubHeading_ChiefComplaints));
                    t_chiefComp.AddCell(new PdfPCell(new Paragraph("CHIEF COMPLAINTS", f_SubHeader))
                    {
                        Rowspan = prescription.Findings.ChiefComplaints.Count,
                        Border=PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_ChiefComp_Value;

                    for (int i = 0; i < prescription.Findings.ChiefComplaints.Count; i++)
                    {
                        C_ChiefComp_Value = new PdfPCell(new Paragraph(prescription.Findings.ChiefComplaints.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_ChiefComp_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_ChiefComp_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_ChiefComp_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_ChiefComp_Value.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_chiefComp.AddCell(C_ChiefComp_Value);
                    }

                    PdfPCell c_ChiefCompTable = new PdfPCell(t_chiefComp);
                    c_ChiefCompTable.Border = PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_InvMainTable.AddCell(c_ChiefCompTable);
                }
                #endregion

                #region Personal History
                //Personal History Table
                if (prescription.Findings != null && prescription.Findings.PersonalHistory != null && prescription.Findings.PersonalHistory.Count > 0)
                {
                    PdfPTable t_personal = new PdfPTable(2);//2
                    t_personal.WidthPercentage = 100f;
                    t_personal.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_personal.SetWidths(widths);

                    t_personal.AddCell(new PdfPCell(new Paragraph("PERSONAL HISTORY", f_SubHeader))
                    {
                        Rowspan = prescription.Findings.PersonalHistory.Count,
                        Border = PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_PersonalHistory_Value;

                    for (int i = 0; i < prescription.Findings.PersonalHistory.Count; i++)
                    {
                        C_PersonalHistory_Value = new PdfPCell(new Paragraph(prescription.Findings.PersonalHistory.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_PersonalHistory_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_PersonalHistory_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_PersonalHistory_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_PersonalHistory_Value.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_personal.AddCell(C_PersonalHistory_Value);
                    }
                    PdfPCell c_PersonalHistoryTable = new PdfPCell(t_personal);
                    c_PersonalHistoryTable.Border = PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_InvMainTable.AddCell(c_PersonalHistoryTable);
                }
                #endregion

                #region Family History
                //Family History Table
                if (prescription.Findings != null && prescription.Findings.FamilyHistory != null && prescription.Findings.FamilyHistory.Count > 0)
                {
                    PdfPTable t_family = new PdfPTable(2);//2
                    t_family.WidthPercentage = 100f;
                    t_family.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_family.SetWidths(widths);

                    t_family.AddCell(new PdfPCell(new Paragraph("FAMILY HISTORY", f_SubHeader))
                    {
                        Rowspan = prescription.Findings.FamilyHistory.Count,
                        Border = PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_FamilyHistory_Value;

                    for (int i = 0; i < prescription.Findings.FamilyHistory.Count; i++)
                    {
                        C_FamilyHistory_Value = new PdfPCell(new Paragraph(prescription.Findings.FamilyHistory.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_FamilyHistory_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_FamilyHistory_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_FamilyHistory_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_FamilyHistory_Value.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_family.AddCell(C_FamilyHistory_Value);
                    }
                    PdfPCell c_FamilyHistoryTable = new PdfPCell(t_family);
                    c_FamilyHistoryTable.Border = PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_InvMainTable.AddCell(c_FamilyHistoryTable);
                }
                #endregion

                #region Examinations
                //Examinations Table
                if (prescription.Findings != null && prescription.Findings.Examinations != null && prescription.Findings.Examinations.Count > 0)
                {
                    PdfPTable t_exam = new PdfPTable(2);//2
                    t_exam.WidthPercentage = 100f;
                    t_exam.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_exam.SetWidths(widths);

                    t_exam.AddCell(new PdfPCell(new Paragraph("EXAMINATIONS", f_SubHeader))
                    {
                        Rowspan = prescription.Findings.Examinations.Count,
                        Border = PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_Examinations_Value;
                    for (int i = 0; i < prescription.Findings.Examinations.Count; i++)
                    {
                        C_Examinations_Value = new PdfPCell(new Paragraph(prescription.Findings.Examinations.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_Examinations_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_Examinations_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Examinations_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_Examinations_Value.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_exam.AddCell(C_Examinations_Value);
                    }
                    PdfPCell c_ExaminationsTable = new PdfPCell(t_exam);
                    c_ExaminationsTable.Border = PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_InvMainTable.AddCell(c_ExaminationsTable);
                }
                #endregion

                #region Additional Findings
                //Additional Findings Table

                if (prescription.Findings != null && prescription.Findings.AdditionalFindings != null && prescription.Findings.AdditionalFindings.Count > 0)
                {
                    PdfPTable t_additional = new PdfPTable(2);//2
                    t_additional.WidthPercentage = 100f;
                    t_additional.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_additional.SetWidths(widths);

                    t_additional.AddCell(new PdfPCell(new Paragraph("ADDITIONAL FINDINGS", f_SubHeader))
                    {
                        Rowspan = prescription.Findings.AdditionalFindings.Count,
                        Border = PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_AdditionalFindings_Value;

                    for (int i = 0; i < prescription.Findings.AdditionalFindings.Count; i++)
                    {
                        C_AdditionalFindings_Value = new PdfPCell(new Paragraph(prescription.Findings.AdditionalFindings.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_AdditionalFindings_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_AdditionalFindings_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_AdditionalFindings_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_AdditionalFindings_Value.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_additional.AddCell(C_AdditionalFindings_Value);
                    }
                    PdfPCell c_AdditionalFindingsTable = new PdfPCell(t_additional);
                    c_AdditionalFindingsTable.Border = PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_InvMainTable.AddCell(c_AdditionalFindingsTable);
                }
                #endregion

                document.Add(t_InvMainTable);
                document.Add(p_BlankLine);

                #endregion

                #region Medication            

                //Outer Table           
                PdfPTable t_MedicationMainTable = new PdfPTable(1);
                t_MedicationMainTable.WidthPercentage = 100f;
                t_MedicationMainTable.HorizontalAlignment = Element.ALIGN_LEFT;
                t_MedicationMainTable.DefaultCell.Border = 0;

                //Drug Table
                PdfPTable t_medication = new PdfPTable(3);
                t_medication.WidthPercentage = 100f;
                t_medication.HorizontalAlignment = Element.ALIGN_LEFT;

                PdfPCell medTbHeader1 = new PdfPCell(new Paragraph("MEDICATION", f_TableHeader));
                medTbHeader1.GrayFill = 0.7f;
                //Alignment
                medTbHeader1.HorizontalAlignment = Element.ALIGN_LEFT;
                medTbHeader1.VerticalAlignment = Element.ALIGN_MIDDLE;
                t_MedicationMainTable.AddCell(new PdfPCell(new Paragraph("MEDICATION", f_TableHeader))
                {
                    Colspan = 3,
                    GrayFill = 0.7f,
                    Border = PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                if (prescription.Medications != null && prescription.Medications.Count > 0)
                {
                    //Medication Table Sub Letterhead
                    PdfPCell TableHeaderDrug = new PdfPCell(new Paragraph("TRADE NAME", f_SubHeader));
                    PdfPCell TableHeaderComp = new PdfPCell(new Paragraph("COMPOSITION", f_SubHeader));
                    PdfPCell TableHeaderDosage = new PdfPCell(new Paragraph("DOSAGE", f_SubHeader));

                    TableHeaderDrug.HorizontalAlignment = Element.ALIGN_LEFT;
                    TableHeaderDrug.VerticalAlignment = Element.ALIGN_MIDDLE;
                    TableHeaderDrug.Border=PdfPCell.NO_BORDER;
                    TableHeaderComp.HorizontalAlignment = Element.ALIGN_LEFT;
                    TableHeaderComp.VerticalAlignment = Element.ALIGN_MIDDLE;
                    TableHeaderComp.Border=PdfPCell.NO_BORDER;
                    TableHeaderDosage.HorizontalAlignment = Element.ALIGN_LEFT;
                    TableHeaderDosage.VerticalAlignment = Element.ALIGN_MIDDLE;
                    TableHeaderDosage.Border=PdfPCell.NO_BORDER;

                    //Adding Letterhead to the Table
                    t_medication.AddCell(TableHeaderDrug);
                    t_medication.AddCell(TableHeaderComp);
                    t_medication.AddCell(TableHeaderDosage);

                    PdfPCell C_Medication_Drug;
                    PdfPCell C_Medication_Composition;
                    PdfPCell C_Medication_Dosage;

                    for (int i = 0; i < prescription.Medications.Count; i++)
                    {
                        List<string> compList = prescription.Medications.ElementAt(i).Composition;
                        StringBuilder composition = new StringBuilder();
                        if(compList.Count>0)
                        {
                            for (int count = 0; count < compList.Count - 1; count++)
                            {
                                composition.Append(compList[count] + ", ");
                            }
                            composition.Append(compList[compList.Count - 1]);
                        }

                        C_Medication_Drug = new PdfPCell(new Paragraph(prescription.Medications.ElementAt(i).TradeName.ToString(), f_data));
                        C_Medication_Composition = new PdfPCell(new Paragraph(composition.ToString(), f_data));
                        C_Medication_Dosage = new PdfPCell(new Paragraph(prescription.Medications.ElementAt(i).Dosage.ToString(), f_data));

                        //Aligning the Cells
                        C_Medication_Drug.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Medication_Drug.VerticalAlignment = Element.ALIGN_CENTER;
                        C_Medication_Composition.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Medication_Composition.VerticalAlignment = Element.ALIGN_CENTER;
                        C_Medication_Dosage.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Medication_Dosage.VerticalAlignment = Element.ALIGN_CENTER;

                        C_Medication_Drug.Border = PdfPCell.NO_BORDER;
                        C_Medication_Composition.Border = PdfPCell.NO_BORDER;
                        C_Medication_Dosage.Border = PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_medication.AddCell(C_Medication_Drug);
                        t_medication.AddCell(C_Medication_Composition);
                        t_medication.AddCell(C_Medication_Dosage);
                    }

                    PdfPCell c_MedicationTable = new PdfPCell(t_medication);
                    c_MedicationTable.Border= PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_MedicationMainTable.AddCell(c_MedicationTable);

                }

                t_MedicationMainTable.KeepTogether = true;
                document.Add(t_MedicationMainTable);
                document.Add(p_BlankLine);

                #endregion

                #region Tests            

                //Outer Table
                PdfPTable t_TestMainTable = new PdfPTable(1);//3
                t_TestMainTable.WidthPercentage = 100f;
                t_TestMainTable.HorizontalAlignment = Element.ALIGN_LEFT;
                t_TestMainTable.DefaultCell.Border = 0;

                //Outer Table Letterhead
                // Setting Width of the Column to Outer Table            
                OuterTablewidth = new float[] { 6.1f };
                t_TestMainTable.SetWidths(OuterTablewidth);
                t_TestMainTable.HorizontalAlignment = 0;

                PdfPCell testTbHeader1 = new PdfPCell(new Paragraph("TESTS PRESCRIBED", f_TableHeader));
                testTbHeader1.GrayFill = 0.7f;
                //Alignment
                testTbHeader1.HorizontalAlignment = Element.ALIGN_LEFT;
                testTbHeader1.VerticalAlignment = Element.ALIGN_MIDDLE;
                t_TestMainTable.AddCell(new PdfPCell(new Paragraph("TESTS PRESCRIBED", f_TableHeader))
                {
                    Colspan = 2,
                    GrayFill = 0.7f,
                    Border=PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_CENTER
                });


                #region TestTypes
                //Follow Up Table
                if (prescription.Tests != null && prescription.Tests.Count > 0)
                {
                    PdfPTable t_tests = new PdfPTable(1);//2
                    t_tests.WidthPercentage = 100f;
                    t_tests.HorizontalAlignment = Element.ALIGN_LEFT;

                    PdfPCell C_Tests_Value;

                    for (int i = 0; i < prescription.Tests.Count; i++)
                    {
                        C_Tests_Value = new PdfPCell(new Paragraph(prescription.Tests.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_Tests_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_Tests_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Tests_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_Tests_Value.Border=PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_tests.AddCell(C_Tests_Value);
                    }

                    PdfPCell c_TestTable = new PdfPCell(t_tests);
                    c_TestTable.Border=PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_TestMainTable.AddCell(c_TestTable);
                }
                #endregion

                t_TestMainTable.KeepTogether = true;
                document.Add(t_TestMainTable);
                document.Add(p_BlankLine);

                #endregion

                #region Misc            

                //Outer Table
                PdfPTable t_MiscMainTable = new PdfPTable(1);//3
                t_MiscMainTable.WidthPercentage = 100f;
                t_MiscMainTable.HorizontalAlignment = Element.ALIGN_LEFT;
                t_MiscMainTable.DefaultCell.Border = 0;

                //Outer Table Letterhead
                // Setting Width of the Column to Outer Table            
                OuterTablewidth = new float[] { 6.1f };
                t_MiscMainTable.SetWidths(OuterTablewidth);
                t_MiscMainTable.HorizontalAlignment = 0;

                PdfPCell miscTbHeader1 = new PdfPCell(new Paragraph("MISCELLANEOUS", f_TableHeader));
                miscTbHeader1.GrayFill = 0.7f;
                //Alignment
                miscTbHeader1.HorizontalAlignment = Element.ALIGN_LEFT;
                miscTbHeader1.VerticalAlignment = Element.ALIGN_MIDDLE;
                t_MiscMainTable.AddCell(new PdfPCell(new Paragraph("MISCELLANEOUS", f_TableHeader))
                {
                    Colspan = 2,
                    GrayFill = 0.7f,
                    Border=PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_CENTER
                });


                #region Follow Up
                //Follow Up Table
                if (prescription.FollowUp != null && prescription.FollowUp.Count > 0)
                {
                    PdfPTable t_followUp = new PdfPTable(2);//2
                    t_followUp.WidthPercentage = 100f;
                    t_followUp.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_followUp.SetWidths(widths);

                    t_followUp.AddCell(new PdfPCell(new Paragraph("FOLLOW UP", f_SubHeader))
                    {
                        Rowspan = prescription.FollowUp.Count,
                        Border=PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_FollowUp_Value;

                    for (int i = 0; i < prescription.FollowUp.Count; i++)
                    {
                        C_FollowUp_Value = new PdfPCell(new Paragraph(prescription.FollowUp.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_FollowUp_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_FollowUp_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_FollowUp_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_FollowUp_Value.Border=PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_followUp.AddCell(C_FollowUp_Value);
                    }

                    PdfPCell c_FollowUpTable = new PdfPCell(t_followUp);
                    c_FollowUpTable.Border=PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_MiscMainTable.AddCell(c_FollowUpTable);
                }
                #endregion

                #region Patient Response
                //Patient Response Table
                if (prescription.PatientResponse != null && prescription.PatientResponse.Count > 0)
                {
                    PdfPTable t_response = new PdfPTable(2);//2
                    t_response.WidthPercentage = 100f;
                    t_response.HorizontalAlignment = Element.ALIGN_LEFT;
                    t_response.SetWidths(widths);

                    t_response.AddCell(new PdfPCell(new Paragraph("PATIENT RESPONSE", f_SubHeader))
                    {
                        Rowspan = prescription.PatientResponse.Count,
                        Border=PdfPCell.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    PdfPCell C_Response_Value;

                    for (int i = 0; i < prescription.PatientResponse.Count; i++)
                    {
                        C_Response_Value = new PdfPCell(new Paragraph(prescription.PatientResponse.ElementAt(i).ToString(), f_data));

                        //Set the Cell Height
                        C_Response_Value.FixedHeight = 15f;

                        //Aligning the Cells
                        C_Response_Value.HorizontalAlignment = Element.ALIGN_LEFT;
                        C_Response_Value.VerticalAlignment = Element.ALIGN_CENTER;
                        C_Response_Value.Border=PdfPCell.NO_BORDER;
                        //ADD the cell to Table
                        t_response.AddCell(C_Response_Value);
                    }

                    PdfPCell c_ResponseTable = new PdfPCell(t_response);
                    c_ResponseTable.Border=PdfPCell.NO_BORDER;
                    //Assiging child table to the Master table
                    t_MiscMainTable.AddCell(c_ResponseTable);
                }
                #endregion

                t_MiscMainTable.KeepTogether = true;
                document.Add(t_MiscMainTable);
                document.Add(p_BlankLine);

                #endregion

                document.Close();
                //pobjMemoryStream.Close();

                return pobjMemoryStream.ToArray();
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception: "+eEx.Message);
                throw new PDFGeneratorException ("An Error Occured While Generating Prescription PDF"); 
            }
            finally
            {
                pobjMemoryStream.Dispose();
            }
        }

    }
}