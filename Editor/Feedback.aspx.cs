using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Web.Configuration;

using System.Net.Mail;

namespace RTMC.Account
{
    public partial class Feedback : System.Web.UI.Page
    {
        readonly string _connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        readonly string[] _ColumnsToHide = { "ID"};
        readonly string strOrganization = System.Web.Configuration.WebConfigurationManager.AppSettings["intOrganization"].ToString();

        private const string strSendFromOrganization = "General";

        protected void Page_Load(object sender, EventArgs e)
        {

            if (strOrganization == "0")
            {
                gvFeedbackList.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#f59630");
                Image1.ImageUrl = "~/images/MImage/icon_page_feedback.png";
                btnSubmit.CssClass = "button_o";
                btnSubmitResponse.CssClass = "button_o";
            }
            if (!IsPostBack)
            {
                string userId = Membership.GetUser().ProviderUserKey.ToString();
                PopulateFeedbackList(userId);
            }
        }

        protected bool saveFeedback(DateTime dtTime, string strFeedback, Guid userId)
        {
            if (strFeedback == string.Empty)
            {
                lblErrorMsg.Text = "Please do not submit empty feedback.";
                return false;
            }
            int rs = 0;
            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "insert into Feedback (Timestamp, UserID, Feedback) " +
                                        " values(@time, @userid, @feedback) ";
                    cmd.Parameters.Add("@time", dtTime);
                    cmd.Parameters.Add("@userid", userId);
                    cmd.Parameters.Add("@feedback", strFeedback);
                    cmd.Connection.Open();
                    rs = cmd.ExecuteNonQuery();
                }
            }
            return (rs > 0);
        }

        protected string strGetUserEmail(String userId)
        {
            string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            DataTable dt = new DataTable();
            string strQuery = "Select Email from aspnet_Membership where UserId = '" + userId + "'";
            SqlDataAdapter sdr = new SqlDataAdapter(strQuery, strCnn);
            sdr.Fill(dt);
            return dt.Rows[0][0].ToString();
        }

        //protected void retrieveEmailCredential(string strOrganization, out string strEmailAddress, out string strPassword, out bool blnSSL, out string strSmtpport, out string strSmtpHost)
        //{
        //    string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        //    DataTable dt = new DataTable();
        //    string strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strOrganization + "'";
        //    SqlDataAdapter sdr = new SqlDataAdapter(strQuery, strCnn);
        //    sdr.Fill(dt);
        //    strEmailAddress = dt.Rows[0][4].ToString().Trim();
        //    strPassword = dt.Rows[0][0].ToString().Trim();
        //    strSmtpHost = dt.Rows[0][1].ToString().Trim();
        //    blnSSL = bool.Parse(dt.Rows[0][2].ToString().Trim());
        //    strSmtpport = Server.HtmlDecode(dt.Rows[0][3].ToString().Trim());
        //}

        protected string strGetFeedbackPosterEmail(string feedbackId)
        {
            string strQuery = "SELECT UserID from Feedback where ID = '" + feedbackId + "'";
            using (var cnn = new SqlConnection(_connectionString))
            {
                var dt = new DataTable();
                var sdr = new SqlDataAdapter(strQuery, cnn);
                sdr.Fill(dt);
                return strGetUserEmail(dt.Rows[0][0].ToString());     
            }
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.

                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        protected string decodePassword(string strEmailPass)
        {
            using (RijndaelManaged myR = new RijndaelManaged())
            {

                byte[] byteRijKey = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                byte[] bytePassword = Convert.FromBase64String(strEmailPass);

                strEmailPass = DecryptStringFromBytes(bytePassword, byteRijKey, byteRijIV);
                return strEmailPass;
            }
        }

        /*
         * protected bool sendEmail(string strToAddress, string strContent, string strSubject, string strBccAddress = "", string sndToAddress = "")
        {
            //Send email to the user via SMERC email server
            string strCredentialAddress, strCredentialPassword;
            bool blnSSL;
            string strSmtpport, strSmtpHost;
            retrieveEmailCredential("General", out strCredentialAddress, out strCredentialPassword, out blnSSL, out strSmtpport, out strSmtpHost);
            strCredentialPassword = decodePassword(strCredentialPassword);
            MailMessage email = null;
            SmtpClient sc = null;
            bool ok = true;
            try
            {
                //smartgrid account
                email = new MailMessage();
                email.From = new MailAddress(strCredentialAddress, "EV Station System");
                sc = new SmtpClient();
                sc.Host = strSmtpHost;
                if (!string.IsNullOrWhiteSpace(strSmtpport))
                {
                    try
                    {
                        sc.Port = int.Parse(strSmtpport);
                    }
                    catch // This catch statement will usually catch errors related to a non integer port data.
                    {
                        //ShowMessage("Port information is incorrect.  Please check Edit Organization page to ensure the port is correct.");
                    }
                }
                sc.Credentials = new NetworkCredential(strCredentialAddress, strCredentialPassword);
                sc.EnableSsl = blnSSL;
                sc.Timeout = 10000;
                //Label1.Text = "bingyu.dsfish.li@gmail.com";
                email.To.Add(new MailAddress(strToAddress));
                if (strBccAddress != "")
                    email.Bcc.Add(new MailAddress(strBccAddress));
                if (sndToAddress != "")
                    email.To.Add(new MailAddress(sndToAddress));
                email.Subject = strSubject;
                email.IsBodyHtml = false;
                email.Body = strContent;

                sc.Send(email);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Our email server doesn't work temporarily. Please try it again later";
                ok = false;
            }
            finally
            {
                if (email != null)
                    email.Dispose();
                if (sc != null)
                    sc.Dispose();
            }
            return ok;

        }
   */

        protected bool SendMail(string strSendOrganization, string strSendTo, string strEmailMessage, string strEmailSubject, bool isFeedback = false, string addtionalEmail = "")
        {
            var sendResponse = System.Web.Configuration.WebConfigurationManager.AppSettings["SendFeedbackEmail"];
            var cnn = new SqlConnection(_connectionString);

            MailMessage email = null;
            SmtpClient sc = null;

            var list = GetRoleEmails("General Maintainer");

            string strSmtpHost = string.Empty;
            string strEmailPass = string.Empty;
            bool blnSSL = true;
            string strSmtpport = string.Empty;
            string strFromEmail = string.Empty;

            try
            {
                cnn.Open();
                string strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strSendOrganization + "'";
                var cmd = new SqlCommand(strQuery, cnn) { CommandType = CommandType.Text };

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    strEmailPass = reader["Email Password"].ToString().Trim();
                    strSmtpHost = reader["Email Host"].ToString().Trim();
                    blnSSL = bool.Parse(reader["EnableSSL"].ToString().Trim());
                    strSmtpport = Server.HtmlDecode(reader["Email Port"].ToString().Trim());
                    strFromEmail = reader["Email Address"].ToString().Trim();
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text += "Error at SendMail: " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }

            // Obtain Password using Rijndael Encryption/Decryption.  The keys are stored in the web.config file
            using (var myR = new RijndaelManaged())
            {
                try
                {
                    byte[] byteRijKey = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                    byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                    byte[] bytePassword = Convert.FromBase64String(strEmailPass);

                    strEmailPass = DecryptStringFromBytes(bytePassword, byteRijKey, byteRijIV);
                }
                catch
                {
                    lblErrorMsg.Text += "Error: Update Password of Organization email.  The Decryption algorithm does not recognize the encryption.";
                }
            }
            bool blnPassed = true;

            try
            {
                email = new MailMessage { From = new MailAddress(strFromEmail, "EV Station System") };

                sc = new SmtpClient { Host = strSmtpHost };
                //sc = new SmtpClient { Host = "smtp.gmail.com" };
                if (!string.IsNullOrWhiteSpace(strSmtpport))
                {
                    try
                    {
                        sc.Port = int.Parse(strSmtpport);
                    }
                    catch (Exception ex) // This catch statement will usually catch errors related to a non integer port data.
                    {
                        lblErrorMsg.Text += "Error at Parse Port: " + ex.Message;
                    }
                }
                else
                {
                    sc.Port = 587;
                }

                sc.Credentials = new NetworkCredential(strFromEmail, strEmailPass);
                sc.EnableSsl = blnSSL;
                sc.Timeout = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EmailServerTimeOut"].ToString());

                //strFromEmail = "support@smartgrid.ucla.edu";
                email.To.Add(new MailAddress(strSendTo));
                email.Bcc.Add(new MailAddress(strFromEmail));
                email.IsBodyHtml = false;
                email.Subject = strEmailSubject;
                email.Body = strEmailMessage;
                if (addtionalEmail != "")
                {
                    email.To.Add(new MailAddress(addtionalEmail));
                }
                if (!isFeedback)
                {

                }
                else
                {
                    foreach (var address in list)
                    {
                        email.Bcc.Add(new MailAddress(address));
                    }
                }

                sc.Send(email);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Visible = true;
                lblErrorMsg.Text += "Error at SendMail2:  " + ex.Message;
                //ShowMessage("Error at SendMail2:  " + ex.Message);
                blnPassed = false;
            }
            finally
            {
                if (email != null)
                    email.Dispose();
                if (sc != null)
                    sc.Dispose();
                //if (blnPassed)
                //{
                //    ShowMessage("Email sent!");
                //}
            }
            return blnPassed;
        }

        protected string strGetGeneralEmail()
        {
            string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            DataTable dt = new DataTable();
            string strQuery = "Select [Email Address] FROM [City] WHERE [Name] = 'General'";
            SqlDataAdapter sdr = new SqlDataAdapter(strQuery, strCnn);
            sdr.Fill(dt);
            return dt.Rows[0][0].ToString().Trim();
        }

        protected List<string> GetRoleEmails(string role)
        {
            var list = new List<string>();
            var strQuery = "SELECT Email From aspnet_UsersInRoles as uinr, aspnet_Membership as m, aspnet_Roles as r where uinr.UserId = m.UserId AND r.RoleId = uinr.RoleId AND RoleName = '" + role + "'";
            using (var cnn = new SqlConnection(_connectionString))
            {
                var dt = new DataTable();
                var sda = new SqlDataAdapter(strQuery, cnn);
                sda.Fill(dt);
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(dt.Rows[i][0].ToString().Trim());
                }
            }
            return list;
        }

        protected void PopulateFeedbackList(string userId)
        {
            var DT = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var sqlQuery =
                    "SELECT f.ID, f.Timestamp, f.Closed, ResponseNo, f.Feedback " +
                    "FROM [EVDemo].[dbo].[Feedback] as f " +
                    "INNER JOIN (SELECT f.ID, COUNT(FeedbackID) AS ResponseNo " +
                    "FROM [EVDemo].[dbo].[Feedback] as f " +
                    "LEFT JOIN [EVDemo].[dbo].[FeedbackResponse] as fr " +
                    "ON fr.FeedbackID = f.ID " +
                    "GROUP BY f.ID ) as temp1 " +
                    "ON temp1.ID = f.ID " +
                    "INNER JOIN (SELECT ID, MAX(Timestamp) as LatestTime " +
                    "FROM (SELECT ID, Timestamp FROM [EVDemo].[dbo].[Feedback] as f " +
                    "UNION ALL SELECT FeedbackID, Timestamp FROM [EVDemo].[dbo].[FeedbackResponse] as fr) as t " +
                    "GROUP BY ID) as temp2 " +
                    "ON temp2.ID = f.ID " + 
                    "WHERE f.UserID = '" + userId + "' " +
                    "ORDER BY temp2.LatestTime DESC ";
                using (var cmd = new SqlCommand(sqlQuery, conn))
                {
                    using (var AD = new SqlDataAdapter(cmd))
                    {
                        try
                        {
                            AD.Fill(DT);
                        }

                        catch (Exception ex)
                        {
                            lblErrorMsg.Text = "Error at populateMaintenanceRecords: " + ex.Message;
                            return;
                        }
                        if (DT.Rows.Count == 0)
                        {
                            lblErrorMsg.Text = "No data in this selection";
                        }
                    }
                }
                Session["data"] = DT;
                gvFeedbackList.DataSource = Session["data"];
                gvFeedbackList.DataBind();

            }
        }

        protected int FindGVcolumn(string name)
        {
            for (int j = 0; j < gvFeedbackList.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvFeedbackList.Columns[j].HeaderText == name)
                    return j;
            }
            return -1;
        }

        private IEnumerable<string> SelectResponseIDs(string feedbackId)
        {
            var cnn = new SqlConnection(_connectionString);
            var responseIDs = new List<string>();
            try
            {
                var strQuery = "SELECT ID FROM [FeedbackResponse] WHERE [FeedbackID] = '" + feedbackId + "'" + " ORDER BY [Timestamp] ";
                cnn.Open();
                var cmd = new SqlCommand(strQuery, cnn);
                var dt = new DataTable();
                var da = new SqlDataAdapter()
                {
                    SelectCommand = cmd,
                };
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
                var idNum = dt.Rows.Count;
                for (var i = 0; i < idNum; i++)
                {
                    responseIDs.Add(dt.Rows[i][0].ToString());
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Visible = true;
                lblErrorMsg.Text += "<br>Error from: SelectResponseIDs " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            return responseIDs;
        }

        protected void GenerateResponseTable(IEnumerable<string> feedbacks)
        {
            ResponseTable.CellSpacing = 0;
            var counter = 0;
            foreach (var id in feedbacks)
            {
                var tRowResponse = new TableRow();
                var cnn = new SqlConnection(_connectionString);
                try
                {
                    var strQuery =
                        "SELECT u.UserName, f.Timestamp, f.Response FROM [FeedbackResponse] as f, [aspnet_Users] as u, [aspnet_Profile] as p, [aspnet_Membership] as m, [aspnet_Roles] as r, [aspnet_UsersInRoles] as uir WHERE u.UserId = f.ResponderID AND p.UserId = f.ResponderID AND m.UserId = f.ResponderID AND r.RoleId = uir.RoleId AND uir.UserId = f.ResponderID AND f.ID = '" +
                        id + "'";
                    cnn.Open();
                    var cmd = new SqlCommand(strQuery, cnn);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter()
                    {
                        SelectCommand = cmd,
                    };
                    da.Fill(dt);
                    da.Dispose();
                    cmd.Dispose();
                    if (dt.Rows.Count == 0)
                        return;
                    var timestamp = dt.Rows[0][1].ToString();
                    var responseText = "<span style='font-weight:bold; font-size: 20pt'>" + dt.Rows[0][0] + " - " + timestamp.Substring(0, timestamp.Length-6) +
                                       "</span><br/>" + "<span style='font-size: 20pt'>" + dt.Rows[0][2] + "</span>";
                    var color = counter%2 != 0
                        ? System.Drawing.ColorTranslator.FromHtml("#CCCCCC")
                        : System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                    var tCellResponse = new TableCell
                    {
                        Text = responseText,
                        Width = new Unit("10%"),
                        BorderColor = Color.Black,
                        BorderWidth = 1,
                        BackColor = color
                    };
                    tRowResponse.Cells.Add(tCellResponse);
                    ResponseTable.Rows.Add(tRowResponse);
                    counter++;
                }
                catch (Exception ex)
                {
                    lblErrorMsg.Visible = true;
                    lblErrorMsg.Text += "<br>Error from: GenerateResponseTable " + ex.Message;
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        protected TableRow PlaceHolderRow()
        {
            var literal = new LiteralControl("<br /><br />");
            var placeholder = new TableCell();
            placeholder.Controls.Add(literal);
            var r = new TableRow();
            r.Cells.Add(placeholder);
            return r;
        }

        protected void FeedbackListSelectedChanged(object sender, EventArgs e)
        {
            //ShowButtons();
            btnSubmitResponse.Visible = true;
            var gvRow = gvFeedbackList.Rows[gvFeedbackList.SelectedIndex];
            var feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
            var timestamp = gvRow.Cells[FindGVcolumn("Date")].Text;
            var feedback = "<span style='font-weight:bold; font-size: 20pt'>" + "Feedback - " + timestamp + "</span><br/>" + "<span style='font-size: 20pt'>" + gvRow.Cells[FindGVcolumn("Feedback")].ToolTip + "</span>";
            var tRowFeedback = new TableRow();
            var tCellFeedback = new TableCell
            {
                Text = feedback,
                Width = new Unit("10%"),
                BorderColor = Color.Black,
                BorderWidth = 1,
                BackColor =  System.Drawing.ColorTranslator.FromHtml("#99CC99")
            };
            tRowFeedback.Cells.Add(tCellFeedback);
            ResponseTable.Rows.Add(tRowFeedback);
            var responseIDs = SelectResponseIDs(feedbackId);
            GenerateResponseTable(responseIDs);
        }

        protected void FeedbackListRowCreated(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < _ColumnsToHide.Count(); i++)
            {
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    e.Row.Cells[FindGVcolumn(_ColumnsToHide[i])].Visible = false;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Cells[FindGVcolumn(_ColumnsToHide[i])].Visible = false;
                }
            }
        }

        protected void FeedbackListDataBound(object sender, GridViewRowEventArgs e)
        {
            var i = 0;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvFeedbackList, "Select$" + e.Row.RowIndex);
                e.Row.ToolTip = "Click to select this row.";
                foreach (TableCell cell in e.Row.Cells)
                {
                    i++;
                    string s = cell.Text;
                    var limit = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["FeedbackLengthInList"]);
                    if (cell.Text.Length > limit && (i == 5))
                        cell.Text = cell.Text.Substring(0, limit) + "...";
                    cell.ToolTip = s;
                }
            }
        }

        protected void btnSubmitResponse_Click(object sender, EventArgs e)
        {
            if (tbFeedback.Text == string.Empty)
            {
                lblErrorMsg.Text = "Please do not submit empty response.";
                return;
            }
            DateTime dtTime = DateTime.Now;
            string strFeedback = tbFeedback.Text;
            var gvRow = gvFeedbackList.Rows[gvFeedbackList.SelectedIndex];
            //string userId = Membership.GetUser().ProviderUserKey.ToString();
            Guid userId = (Guid)Membership.GetUser().ProviderUserKey;
            const string strQuery =
                        "INSERT [FeedbackResponse] (ResponderID, Timestamp, FeedbackID, Response) VALUES (@UserID, @Timestamp, @FeedbackID, @Response) ";
            var feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var cmd = new SqlCommand(strQuery, cnn);

                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    var timestamp = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo);
                    cmd.Parameters.Add(new SqlParameter("@UserID", userId.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@Timestamp", timestamp));
                    cmd.Parameters.Add(new SqlParameter("@FeedbackID", feedbackId));
                    cmd.Parameters.Add(new SqlParameter("@Response", tbFeedback.Text));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    lblErrorMsg.Text = "Error at btnSubmitResponse_Click: " + ex.Message;
                }
            }

            string strUserEmail = strGetUserEmail(userId.ToString());

            string strSubject = "Response from user " + User.Identity.Name;
            //string strContent = "The following feedback has been successfully submitted for user " + User.Identity.Name +
            //                    " at " + dtTime.ToString() + ":\n\r" + strFeedback + "\n\r" + "http://wireless3.seas.ucla.edu/EVUser";
            string strContent = User.Identity.Name + " submitted a response for the feedback" +
                                " at " + dtTime.ToString();

            string sendFeedbackEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["SendFeedbackEmail"].ToString();
            string feedbackNotes = System.Web.Configuration.WebConfigurationManager.AppSettings["FeedbackNotes"].ToString();
            if (sendFeedbackEmail == "true")
            {
                strContent += ":\n\r" + strFeedback;
            }
            else
            {
                strContent += ":\n\r" + feedbackNotes;
            }
            string strFeedbackPosterEmail = strGetFeedbackPosterEmail(feedbackId);
            if (SendMail(strSendFromOrganization, strUserEmail, strContent, strSubject, false, strFeedbackPosterEmail))
            {
                PopulateFeedbackList(userId.ToString());
                gvFeedbackList.SelectRow(0);//always select newest modified feedback
                tbFeedback.Text = string.Empty;
                lblErrorMsg.Text = "Response submission succeeded.";
            }

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            lblErrorMsg.Text = "";

            DateTime dtTime = DateTime.Now;
            string strFeedback = tbFeedback.Text;
            Guid userId = (Guid)Membership.GetUser().ProviderUserKey;

            if (strFeedback.Length > 2000)
            {
                lblErrorMsg.Text = "* Failed to submit the feedback. Feedback should include less than 2000 characters";
                return;
            }

            if (!saveFeedback(dtTime, strFeedback, userId))
                return;

            string strUserEmail = strGetUserEmail(userId.ToString());
            //strUserEmail = "bingyu.dsfish.li@gmail.com"; // For Test Only

            string strGeneralEmail = strGetGeneralEmail();
            //strGeneralEmail = "averie.li@live.com"; // For Test Only

            string strSubject = "Feedback from user " + User.Identity.Name;
            //string strContent = "The following feedback has been successfully submitted for user " + User.Identity.Name +
            //                    " at " + dtTime.ToString() + ":\n\r" + strFeedback + "\n\r" + "http://wireless3.seas.ucla.edu/EVUser";
            string strContent = "The following feedback has been successfully submitted for user " + User.Identity.Name +
                                " at " + dtTime.ToString() + ":\n\r" + strFeedback;

            if (SendMail(strSendFromOrganization, strUserEmail, strContent, strSubject, true))
            {
                lblErrorMsg.Text = "Submission succeeded.";
                tbFeedback.Text = String.Empty;
                PopulateFeedbackList(userId.ToString());
            }                         
        }

    }
}
