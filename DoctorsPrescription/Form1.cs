using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Data;
using System.Linq;

namespace DoctorsPrescription
{
    public partial class PrescriptionForm : Form
    {
        private PrintDocument printDocument;
        private string prescriptionData;
        private Image hospitalLogo;
        private string doctorName;
        private Image doctorSignatureImage;

        private DataTable defaultDataTable;

        public PrescriptionForm()
        {
            InitializeComponent();

            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            LoadHospitalLogo();
            LoadGenderOptions();
            SetupDataBinding();

            // Register the event handler for the CheckedChanged event of radio buttons
            rbMale.CheckedChanged += GenderRadioButton_CheckedChanged;
            rbFemale.CheckedChanged += GenderRadioButton_CheckedChanged;

            // Call the YourForm_Load method to load the default data
            YourForm_Load(null, EventArgs.Empty);
        }

        private void YourForm_Load(object sender, EventArgs e)
        {
            // Retrieve the default data from the database and assign it to the defaultDataTable
            string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
            string query = "SELECT * FROM PrescriptionTable";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                defaultDataTable = new DataTable();
                adapter.Fill(defaultDataTable);

                // Bind the default data to the DataGridView
                dataGridRecords.DataSource = defaultDataTable;
            }
        }

        private void LoadHospitalLogo()
        {
            string logoFileName = "1.jpg";
            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string logoPath = Path.Combine(downloadsFolder, logoFileName);
            if (File.Exists(logoPath))
            {
                Image originalImage = Image.FromFile(logoPath);
                int maxWidth = pbHospitalLogo.Width;
                int maxHeight = pbHospitalLogo.Height;
                Image resizedImage = ScaleImage(originalImage, maxWidth, maxHeight);
                hospitalLogo = resizedImage;
                pbHospitalLogo.Image = resizedImage;
            }
        }

        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            int newWidth, newHeight;
            double aspectRatio;

            if (image.Width > image.Height)
            {
                newWidth = maxWidth;
                aspectRatio = (double)image.Height / image.Width;
                newHeight = (int)(newWidth * aspectRatio);
            }
            else
            {
                newHeight = maxHeight;
                aspectRatio = (double)image.Width / image.Height;
                newWidth = (int)(newHeight * aspectRatio);
            }

            Image resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtPatientName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPatientAge_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPatientAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPatientContact_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbMedicalHistory_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbPrescription_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Save the prescription data to the database
                string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
                // Replace "YourServerName", "YourDatabaseName", "YourUsername", and "YourPassword" with the appropriate values for your database connection.

                int patientID = Convert.ToInt32(txtPatientID.Text);
                string patientName = txtPatientName.Text;
                int patientAge = Convert.ToInt32(txtPatientAge.Text);
                string patientAddress = txtPatientAddress.Text;
                string patientContactNumber = txtPatientContact.Text;
                string patientGender = GetSelectedGender();
                string diagnosis = rtbDiagnosis.Text;
                string medicalHistory = rtbMedicalHistory.Text;
                string prescriptionText = rtbPrescription.Text;
                string doctorName = txtDoctorName.Text;

                byte[] doctorSignature;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    picDoctorSignature.Image.Save(memoryStream, ImageFormat.Png); // Assuming the image is in PNG format
                    doctorSignature = memoryStream.ToArray();
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO PrescriptionTable (PatientID, PatientName, PatientAge, PatientAddress, PatientContactNumber, PatientGender, Diagnosis, MedicalHistory, PrescriptionText, DoctorName, DoctorSignature) " +
                        "VALUES (@PatientID, @PatientName, @PatientAge, @PatientAddress, @PatientContactNumber, @PatientGender, @Diagnosis, @MedicalHistory, @PrescriptionText, @DoctorName, @DoctorSignature)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PatientID", patientID);
                    command.Parameters.AddWithValue("@PatientName", patientName);
                    command.Parameters.AddWithValue("@PatientAge", patientAge);
                    command.Parameters.AddWithValue("@PatientAddress", patientAddress);
                    command.Parameters.AddWithValue("@PatientContactNumber", patientContactNumber);
                    command.Parameters.AddWithValue("@PatientGender", patientGender);
                    command.Parameters.AddWithValue("@Diagnosis", diagnosis);
                    command.Parameters.AddWithValue("@MedicalHistory", medicalHistory);
                    command.Parameters.AddWithValue("@PrescriptionText", prescriptionText);
                    command.Parameters.AddWithValue("@DoctorName", doctorName);
                    command.Parameters.AddWithValue("@DoctorSignature", doctorSignature);
                    command.ExecuteNonQuery();
                }

                prescriptionData = prescriptionText;

                MessageBox.Show("Prescription saved successfully!");
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while saving the prescription: " + ex.Message);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(prescriptionData))
                {
                    // Define fonts and brushes for text
                    Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                    Font sectionFont = new Font("Arial", 12, FontStyle.Bold);
                    Font contentFont = new Font("Arial", 12);
                    Brush brush = Brushes.SkyBlue;

                    // Define the position and spacing for printing text
                    float x = e.MarginBounds.Left;
                    float y = e.MarginBounds.Top;

                    // Calculate the available printing area with margins
                    float availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    float availableHeight = e.MarginBounds.Height;

                    // Print the hospital logo if it is available
                    if (hospitalLogo != null)
                    {
                        // Adjust the position for logo printing
                        float logoX = e.MarginBounds.Right - hospitalLogo.Width;
                        float logoY = e.MarginBounds.Top;

                        // Draw the logo image
                        e.Graphics.DrawImage(hospitalLogo, logoX, logoY);

                        // Adjust the available width
                        availableWidth -= hospitalLogo.Width;
                    }

                    // Print prescription title
                    string title = "DOCTOR'S PRESCRIPTION";
                    float titleWidth = e.Graphics.MeasureString(title, titleFont).Width;
                    float titleX = e.MarginBounds.Left + (e.MarginBounds.Width - titleWidth) / 2;
                    e.Graphics.DrawString(title, titleFont, Brushes.DarkBlue, titleX, y);
                    y += titleFont.Height * 2;

                    // Print patient's information section
                    string patientInfoSection = "PATIENT'S INFORMATION";
                    e.Graphics.DrawString(patientInfoSection, sectionFont, brush, x, y);
                    y += sectionFont.Height;

                    // Print patient details
                    e.Graphics.DrawString("Patient's ID:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString("  " + txtPatientID.Text, contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height;

                    e.Graphics.DrawString("Patient's Name:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString("  " + txtPatientName.Text, contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height;

                    e.Graphics.DrawString("Age:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString(txtPatientAge.Text, contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height;

                    e.Graphics.DrawString("Address:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString(txtPatientAddress.Text, contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height;

                    e.Graphics.DrawString("Contact Number:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString("  " + txtPatientContact.Text, contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height;

                    e.Graphics.DrawString("Gender:   ", sectionFont, Brushes.Black, x, y);
                    e.Graphics.DrawString(GetSelectedGender(), contentFont, Brushes.Black, x + 120, y);
                    y += contentFont.Height * 2;

                    // Calculate the remaining available width and height
                    availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    availableHeight = e.MarginBounds.Height - y;

                    // Print medical history section
                    string medicalHistorySection = "MEDICAL HISTORY";
                    e.Graphics.DrawString(medicalHistorySection, sectionFont, brush, x, y);
                    y += sectionFont.Height;

                    // Print medical history content
                    string medicalHistoryContent = "According to " + txtPatientName.Text + "'s medical history, he/she has:";
                    e.Graphics.DrawString(medicalHistoryContent, contentFont, Brushes.Black, x, y);
                    y += contentFont.Height * 2;

                    // Calculate the remaining available width and height
                    availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    availableHeight = e.MarginBounds.Height - y;

                    // Print medical history details
                    string medicalHistoryDetails = rtbMedicalHistory.Text;
                    int linesFitted;
                    int charactersFitted;
                    e.Graphics.MeasureString(medicalHistoryDetails, contentFont, new SizeF(e.MarginBounds.Width, availableHeight), StringFormat.GenericDefault, out charactersFitted, out linesFitted);
                    e.Graphics.DrawString(medicalHistoryDetails.Substring(0, charactersFitted), contentFont, Brushes.Black, x, y);
                    y += contentFont.Height * linesFitted;

                    // Calculate the remaining available width and height
                    availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    availableHeight = e.MarginBounds.Height - y;

                    // Print diagnosis section
                    string diagnosisSection = "THE DIAGNOSIS";
                    e.Graphics.DrawString(diagnosisSection, sectionFont, brush, x, y);
                    y += sectionFont.Height;

                    // Print diagnosis content
                    string diagnosisContent = txtPatientName.Text + " has " + rtbDiagnosis.Text + " after the medical test";
                    e.Graphics.DrawString(diagnosisContent, contentFont, Brushes.Black, x, y);
                    y += contentFont.Height * 2;

                    // Calculate the remaining available width and height
                    availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    availableHeight = e.MarginBounds.Height - y;

                    // Print prescription section
                    string prescriptionSection = "THE PRESCRIPTION";
                    e.Graphics.DrawString(prescriptionSection, sectionFont, brush, x, y);
                    y += sectionFont.Height;

                    // Print prescription content
                    string prescriptionContent = txtPatientName.Text + " medication should be the following:";
                    e.Graphics.DrawString(prescriptionContent, contentFont, Brushes.Black, x, y);
                    y += contentFont.Height * 2;

                    // Calculate the remaining available width and height
                    availableWidth = e.MarginBounds.Width - (e.MarginBounds.Right - x);
                    availableHeight = e.MarginBounds.Height - y;

                    // Print the prescription data
                    int lines = (int)Math.Floor(availableHeight / contentFont.Height);
                    string trimmedPrescription = prescriptionData.Substring(0, Math.Min(prescriptionData.Length, lines * 100));

                    e.Graphics.DrawString(trimmedPrescription, contentFont, Brushes.Black, x, y);
                    y += contentFont.Height * lines;

                    // Print doctor's name and signature
                    if (!string.IsNullOrEmpty(doctorName) && doctorSignatureImage != null)
                    {
                        // Adjust the position for doctor's name and signature
                        float signatureX = e.MarginBounds.Right - 150; // Adjust the value to set the desired width
                        float signatureY = e.MarginBounds.Bottom - 100; // Adjust the value to set the desired height

                        // Resize the signature image
                        Image resizedSignature = ResizeImage(doctorSignatureImage, new Size(120, 80)); // Adjust the size as per your requirement

                        // Format and print doctor's name
                        string doctorText = "Doctor:";
                        string doctorNameText = doctorText + " " + doctorName;
                        SizeF doctorTextSize = e.Graphics.MeasureString(doctorText, new Font(contentFont, FontStyle.Bold));
                        SizeF doctorNameSize = e.Graphics.MeasureString(doctorNameText, contentFont);
                        float doctorX = e.MarginBounds.Right - doctorTextSize.Width - doctorNameSize.Width;
                        float doctorY = signatureY - doctorNameSize.Height;

                        // Print "Doctor:" in bold font
                        e.Graphics.DrawString(doctorText, new Font(contentFont, FontStyle.Bold), Brushes.Black, doctorX, doctorY);

                        // Print the doctor's name in normal font
                        e.Graphics.DrawString(doctorName, contentFont, Brushes.Black, doctorX + doctorTextSize.Width, doctorY);

                        // Draw doctor's signature image
                        e.Graphics.DrawImage(resizedSignature, signatureX, signatureY);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while printing: " + ex.Message);
            }
        }

        private Image ResizeImage(Image image, Size newSize)
        {
            Bitmap resizedImage = new Bitmap(newSize.Width, newSize.Height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.DrawImage(image, new Rectangle(Point.Empty, newSize));
            }
            return resizedImage;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Create an instance of PrintDocument
                PrintDocument printDocument = new PrintDocument();

                // Assign the PrintPage event handler
                printDocument.PrintPage += PrintDocument_PrintPage;

                // Create an instance of PrintDialog
                PrintDialog printDialog = new PrintDialog();

                // Set the document to be printed
                printDialog.Document = printDocument;

                // Show the print dialog and check if the user clicked the Print button
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    // Print the document
                    printDocument.Print();
                }

                // Unregister the event handler to prevent memory leaks
                printDocument.PrintPage -= PrintDocument_PrintPage;
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while printing: " + ex.Message);
            }
        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            try
            {
                PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;
                prescriptionData = rtbPrescription.Text; // Use the prescription data from the form
                printPreviewDialog.Document = printDocument;

                // Set other print preview dialog properties as needed
                printPreviewDialog.StartPosition = FormStartPosition.CenterScreen;
                printPreviewDialog.Width = 800;
                printPreviewDialog.Height = 600;
                printPreviewDialog.Icon = this.Icon;
                printPreviewDialog.Text = "Print Preview";

                // Show the print preview dialog
                printPreviewDialog.ShowDialog();

                // Unregister the event handler to prevent memory leaks
                printDocument.PrintPage -= PrintDocument_PrintPage;
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while showing print preview: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Unregister the event handler to prevent memory leaks
            printDocument.PrintPage -= PrintDocument_PrintPage;

            // Close the form
            Close();
        }

        private void pbHospitalLogo_Click(object sender, EventArgs e)
        {

        }

        private void rtbDiagnosis_TextChanged(object sender, EventArgs e)
        {

        }

        private void LoadGenderOptions()
        {
            // Set the default selection for gender radio buttons
            rbMale.Checked = true;
            rbFemale.Checked = true;
        }

        private void GenderRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            // Clear the selection of the other radio button
            if (sender == rbMale && rbMale.Checked)
                rbFemale.Checked = false;
            else if (sender == rbFemale && rbFemale.Checked)
                rbMale.Checked = false;
        }

        private string GetSelectedGender()
        {
            if (rbMale.Checked)
                return "Male";
            else if (rbFemale.Checked)
                return "Female";
            else
                return string.Empty;
        }

        private void PrescriptionForm_Load(object sender, EventArgs e)
        {

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtPatientID.Text = string.Empty;
            txtPatientName.Text = string.Empty;
            txtPatientAge.Text = string.Empty;
            txtPatientAddress.Text = string.Empty;
            txtPatientContact.Text = string.Empty;
            rbMale.Checked = true;
            rbFemale.Checked = false;
            rtbDiagnosis.Text = string.Empty;
            rtbMedicalHistory.Text = string.Empty;
            rtbPrescription.Text = string.Empty;
            txtDoctorName.Text = string.Empty;

            ResetPictureBoxImage(picDoctorSignature);
        }

        private void ResetPictureBoxImage(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }

            // Load the image from file and assign it to the PictureBox
            pictureBox.Image = Image.FromFile(@"C:\Users\shadd\Downloads\IMG - Doctor's Prescription\14.png");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the patient ID for deletion
                string patientID = txtPatientID.Text;

                // Confirm the deletion action with the user
                DialogResult confirmationResult = MessageBox.Show("Are you sure you want to delete the prescription of the selected patient?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmationResult == DialogResult.Yes)
                {
                    // Retrieve the prescription data to be moved to the recycle bin
                    string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Retrieve the prescription data
                        string selectQuery = "SELECT * FROM PrescriptionTable WHERE PatientID = @PatientID";
                        SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                        selectCommand.Parameters.AddWithValue("@PatientID", patientID);

                        using (SqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Store the prescription data in variables
                                string patientName = reader["PatientName"].ToString();
                                int patientAge = Convert.ToInt32(reader["PatientAge"]);
                                string patientAddress = reader["PatientAddress"].ToString();
                                string patientContactNumber = reader["PatientContactNumber"].ToString();
                                string patientGender = reader["PatientGender"].ToString();
                                string diagnosis = reader["Diagnosis"].ToString();
                                string medicalHistory = reader["MedicalHistory"].ToString();
                                string prescriptionText = reader["PrescriptionText"].ToString();
                                string doctorName = reader["DoctorName"].ToString();
                                byte[] doctorSignature = (byte[])reader["DoctorSignature"];

                                // Close the SqlDataReader before executing the deleteCommand
                                reader.Close();

                                // Insert the prescription data into the recycle bin table
                                string insertQuery = "INSERT INTO RecycleBinTable (PatientID, PatientName, PatientAge, PatientAddress, PatientContactNumber, PatientGender, Diagnosis, MedicalHistory, PrescriptionText, DoctorName, DoctorSignature) VALUES (@PatientID, @PatientName, @PatientAge, @PatientAddress, @PatientContactNumber, @PatientGender, @Diagnosis, @MedicalHistory, @PrescriptionText, @DoctorName, @DoctorSignature)";
                                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                                insertCommand.Parameters.AddWithValue("@PatientID", patientID);
                                insertCommand.Parameters.AddWithValue("@PatientName", patientName);
                                insertCommand.Parameters.AddWithValue("@PatientAge", patientAge);
                                insertCommand.Parameters.AddWithValue("@PatientAddress", patientAddress);
                                insertCommand.Parameters.AddWithValue("@PatientContactNumber", patientContactNumber);
                                insertCommand.Parameters.AddWithValue("@PatientGender", patientGender);
                                insertCommand.Parameters.AddWithValue("@Diagnosis", diagnosis);
                                insertCommand.Parameters.AddWithValue("@MedicalHistory", medicalHistory);
                                insertCommand.Parameters.AddWithValue("@PrescriptionText", prescriptionText);
                                insertCommand.Parameters.AddWithValue("@DoctorName", doctorName);
                                insertCommand.Parameters.AddWithValue("@DoctorSignature", doctorSignature);

                                insertCommand.ExecuteNonQuery();

                                // Delete the prescription data from the original table
                                string deleteQuery = "DELETE FROM PrescriptionTable WHERE PatientID = @PatientID";
                                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                                deleteCommand.Parameters.AddWithValue("@PatientID", patientID);
                                int rowsAffected = deleteCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Prescription moved to the recycle bin successfully!");
                                    ClearForm();
                                }
                                else
                                {
                                    MessageBox.Show("No prescription found for the specified patient ID.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("No prescription found for the specified patient ID.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while moving the prescription to the recycle bin: " + ex.Message);
            }
        }


        private void ClearForm()
        {
            // Clear the input fields
            txtPatientID.Text = string.Empty;
            txtPatientName.Text = string.Empty;
            txtPatientAge.Text = string.Empty;
            txtPatientAddress.Text = string.Empty;
            txtPatientContact.Text = string.Empty;
            rtbMedicalHistory.Text = string.Empty;
            rtbDiagnosis.Text = string.Empty;
            rtbPrescription.Text = string.Empty;
            txtDoctorName.Text = string.Empty;

            // Reset the gender selection
            LoadGenderOptions();

            // Reload the doctor's signature image if available
            if (picDoctorSignature.Image != null)
            {
                string imagePath = @"C:\Users\shadd\Downloads\IMG - Doctor's Prescription\14.png"; // Replace with the actual image path
                picDoctorSignature.Image = Image.FromFile(imagePath);
            }
        }


        private void txtDoctorName_TextChanged(object sender, EventArgs e)
        {
            doctorName = txtDoctorName.Text;
        }

        private void picDoctorSignature_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png, *.jpg, *.jpeg, *.gif, *.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                doctorSignatureImage = Image.FromFile(openFileDialog.FileName);
                picDoctorSignature.Image = doctorSignatureImage;
            }
        }

        private bool shouldShowPrintPreview = false; // Flag to indicate if print preview should be shown

        private void btnPrintOldRecord_Click(object sender, EventArgs e)
        {
            string patientName = txtPatientName.Text.Trim(); // Get the patient name from the input textbox
            string patientID = txtPatientID.Text.Trim(); // Get the patient number from the input textbox

            if (!string.IsNullOrEmpty(patientName) || !string.IsNullOrEmpty(patientID))
            {
                PrintOldRecord(patientName, patientID);
            }
            else
            {
                MessageBox.Show("Please enter the patient's name or ID!");
            }
        }

        private void PrintOldRecord(string patientName, string patientID)
        {
            try
            {
                // Retrieve the old record from the database using the patient's name or number
                string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
                string query = "SELECT * FROM PrescriptionTable WHERE PatientName LIKE @PatientName OR PatientID LIKE @PatientID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PatientName", "%" + patientName + "%");
                    command.Parameters.AddWithValue("@PatientID", "%" + patientID + "%");

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Retrieve the necessary data from the record
                        string retrievedPatientID = reader["PatientID"].ToString();
                        string retrievedPatientName = reader["PatientName"].ToString();
                        string patientAge = reader["PatientAge"].ToString();
                        string patientAddress = reader["PatientAddress"].ToString();
                        string patientContactNumber = reader["PatientContactNumber"].ToString();
                        string patientGender = reader["PatientGender"].ToString();
                        string diagnosis = reader["Diagnosis"].ToString();
                        string medicalHistory = reader["MedicalHistory"].ToString();
                        string prescriptionText = reader["PrescriptionText"].ToString();
                        string doctorName = reader["DoctorName"].ToString();
                        byte[] doctorSignature = (byte[])reader["DoctorSignature"];

                        if (retrievedPatientID == patientID)
                        {
                            // Convert the doctorSignature byte array back to an Image
                            using (MemoryStream memoryStream = new MemoryStream(doctorSignature))
                            {
                                doctorSignatureImage = Image.FromStream(memoryStream);
                            }

                            // Set the necessary fields and controls with the retrieved data
                            txtPatientID.Text = retrievedPatientID;
                            txtPatientName.Text = retrievedPatientName;
                            txtPatientAge.Text = patientAge;
                            txtPatientAddress.Text = patientAddress;
                            txtPatientContact.Text = patientContactNumber;
                            rtbMedicalHistory.Text = medicalHistory;
                            rtbPrescription.Text = prescriptionText;
                            rtbDiagnosis.Text = diagnosis;
                            txtDoctorName.Text = doctorName;
                            picDoctorSignature.Image = doctorSignatureImage; // Set the PictureBox image

                            // Set the flag to indicate that print preview should be shown
                            shouldShowPrintPreview = true;

                            // Exit the method after finding a matching record
                            return;
                        }
                    }

                    MessageBox.Show("Record not found!");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while printing the old record: " + ex.Message);
            }
        }

        private BindingSource bindingSource;
        private DataTable dataTable;

        private void SetupDataBinding()
        {
            // Create a new BindingSource
            bindingSource = new BindingSource();

            // Bind the BindingSource to the DataGridView
            dataGridRecords.DataSource = bindingSource;

            // Retrieve the data from the database and assign it to the DataTable
            string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
            string query = "SELECT * FROM PrescriptionTable";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
            }

            // Assign the DataTable to the BindingSource
            bindingSource.DataSource = dataTable;
        }

        private void RefreshData()
        {
            // Clear the defaultDataTable and re-fill it with updated data from the database
            defaultDataTable.Clear();

            string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
            string query = "SELECT * FROM PrescriptionTable";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(defaultDataTable);
            }

            // Refresh the DataGridView to reflect the updated data
            dataGridRecords.Refresh();
        }


        private void dataGridRecords_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle any cell content click events here
            // You can access the selected row's data using the BindingSource
            // For example:
            int selectedRowIndex = e.RowIndex;
            if (selectedRowIndex >= 0 && selectedRowIndex < bindingSource.Count)
            {
                DataRowView selectedRow = (DataRowView)bindingSource[selectedRowIndex];
                // Access the data using the column names or indices
                string patientName = selectedRow["PatientName"].ToString();
                string patientID = selectedRow["PatientID"].ToString();
                // Do something with the data
                MessageBox.Show("Selected Patient: " + patientName + " (ID: " + patientID + ")");
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchID = txtSearchID.Text.Trim();
            string searchName = txtSearchName.Text.Trim();

            // Retrieve the data from the database based on the search criteria
            string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";
            string query = "SELECT * FROM PrescriptionTable WHERE ";

            if (!string.IsNullOrEmpty(searchID) && !string.IsNullOrEmpty(searchName))
            {
                // Search by both ID and name
                query += "PatientID LIKE @PatientID AND PatientName LIKE @PatientName";
            }
            else if (!string.IsNullOrEmpty(searchID))
            {
                // Search by ID only
                query += "PatientID LIKE @PatientID";
            }
            else if (!string.IsNullOrEmpty(searchName))
            {
                // Search by name only
                query += "PatientName LIKE @PatientName";
            }
            else
            {
                // No search criteria provided
                MessageBox.Show("Please enter a patient ID or name to search.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                if (!string.IsNullOrEmpty(searchID))
                {
                    command.Parameters.AddWithValue("@PatientID", "%" + searchID + "%");
                }
                if (!string.IsNullOrEmpty(searchName))
                {
                    command.Parameters.AddWithValue("@PatientName", "%" + searchName + "%");
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                dataTable.Clear();
                adapter.Fill(dataTable);
                dataGridRecords.DataSource = dataTable;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // Clear any previous search criteria
            txtSearchID.Text = "";
            txtSearchName.Text = "";

            // Reset the DataGridView to display the default data
            dataGridRecords.DataSource = defaultDataTable;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void RestorePrescription(string patientID)
        {
            try
            {
                string connectionString = @"Data Source=DESKTOP-V1FNQN6\SQLEXPRESS;Initial Catalog=PRESCRIPTION;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Retrieve the prescription data from the recycle bin
                    string selectQuery = "SELECT * FROM RecycleBinTable WHERE PatientID = @PatientID";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@PatientID", patientID);

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Store the prescription data in variables
                            string patientName = reader["PatientName"].ToString();
                            int patientAge = Convert.ToInt32(reader["PatientAge"]);
                            string patientAddress = reader["PatientAddress"].ToString();
                            string patientContactNumber = reader["PatientContactNumber"].ToString();
                            string patientGender = reader["PatientGender"].ToString();
                            string diagnosis = reader["Diagnosis"].ToString();
                            string medicalHistory = reader["MedicalHistory"].ToString();
                            string prescriptionText = reader["PrescriptionText"].ToString();
                            string doctorName = reader["DoctorName"].ToString();
                            byte[] doctorSignature = (byte[])reader["DoctorSignature"];

                            // Close the SqlDataReader before executing the deleteCommand
                            reader.Close();

                            // Insert the prescription data back into the original table
                            string insertQuery = "INSERT INTO PrescriptionTable (PatientID, PatientName, PatientAge, PatientAddress, PatientContactNumber, PatientGender, Diagnosis, MedicalHistory, PrescriptionText, DoctorName, DoctorSignature) VALUES (@PatientID, @PatientName, @PatientAge, @PatientAddress, @PatientContactNumber, @PatientGender, @Diagnosis, @MedicalHistory, @PrescriptionText, @DoctorName, @DoctorSignature)";
                            SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                            insertCommand.Parameters.AddWithValue("@PatientID", patientID);
                            insertCommand.Parameters.AddWithValue("@PatientName", patientName);
                            insertCommand.Parameters.AddWithValue("@PatientAge", patientAge);
                            insertCommand.Parameters.AddWithValue("@PatientAddress", patientAddress);
                            insertCommand.Parameters.AddWithValue("@PatientContactNumber", patientContactNumber);
                            insertCommand.Parameters.AddWithValue("@PatientGender", patientGender);
                            insertCommand.Parameters.AddWithValue("@Diagnosis", diagnosis);
                            insertCommand.Parameters.AddWithValue("@MedicalHistory", medicalHistory);
                            insertCommand.Parameters.AddWithValue("@PrescriptionText", prescriptionText);
                            insertCommand.Parameters.AddWithValue("@DoctorName", doctorName);
                            insertCommand.Parameters.AddWithValue("@DoctorSignature", doctorSignature);

                            insertCommand.ExecuteNonQuery();

                            // Delete the prescription data from the recycle bin table
                            string deleteQuery = "DELETE FROM RecycleBinTable WHERE PatientID = @PatientID";
                            SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                            deleteCommand.Parameters.AddWithValue("@PatientID", patientID);
                            int rowsAffected = deleteCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Prescription restored successfully!");
                                // Perform any additional actions after restoration if needed
                            }
                            else
                            {
                                MessageBox.Show("No prescription found in the recycle bin for the specified patient ID.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("No prescription found in the recycle bin for the specified patient ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while restoring the prescription: " + ex.Message);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the patient ID for restoration
                string patientID = txtRestorePatientID.Text;

                // Call the RestorePrescription method
                RestorePrescription(patientID);

                // Perform any additional actions after restoration if needed

            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show("An error occurred while restoring the prescription: " + ex.Message);
            }
        }
    }
}
