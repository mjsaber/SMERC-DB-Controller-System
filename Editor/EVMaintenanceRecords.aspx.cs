using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;

namespace EVEditor
{
    public partial class EVMaintenanceRecords : System.Web.UI.Page
    {
        readonly string _connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };

        readonly string[] _strArrRolesToAllow = {};
        //  string[] strArrRolesToAllow = { };
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        readonly string[] _strArrTypesToAllow = { "Administrator","Maintainer","Operator" };
        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        readonly string[] _strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        readonly string[] _ColumnsToHide = {"ID", "UserID", "FixerID"};

        private const string strSendFromOrganization = "General";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                var rp = (RolePrincipal)User;
                string[] roles = Roles.GetRolesForUser();
                var listOfRoles = new List<string>();
                for (int i = 0; i < roles.Count(); i++)
                {
                    listOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
                }

                if (!BlnFindAssociatedRoles(listOfRoles)) // only continue if the user is a city administrator
                {
                    Response.Redirect("~/Info.aspx?ErrMsg=Privilege", false);
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }
            if (!IsPostBack)
            {
                cbShowUnclosedRecords.Checked = true;
                cbShowUnresolvedRecords.Checked = true;
                HideButtons();
                PopulateOrganization();
                PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked, cbShowUnresolvedRecords.Checked);

                //populateUserName(ddlOrganization.SelectedValue);
                //populateEvInfo();
                //populateFleet(ddlOrganization.SelectedValue);
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
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
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

        protected string GetFeedbackResponse(string feedbackId)
        {
            var strResponse = string.Empty;
            var cnn = new SqlConnection(_connectionString);
            try
            {
                cnn.Open();
                var strQuery = "SELECT [Response] FROM [FeedbackResponse] WHERE FeedbackID = '" + feedbackId + "'" + "ORDER BY [Timestamp] ";

                var cmd = new SqlCommand(strQuery, cnn) { CommandType = CommandType.Text };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    strResponse += "<tr><td><br>";
                    strResponse += reader["Response"].ToString().Trim();
                    strResponse += "</td></tr>";
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at GetFeedbackResponse: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }

            return strResponse;
        }
        
        protected bool SendMail(string strOrganization, string strSendTo, string strEmailMessage, string strEmailSubject, bool isFeedback = false, string addtionalEmail = "", bool addRoleEmails = false)
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
                string strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strOrganization + "'";
                var cmd = new SqlCommand(strQuery, cnn) {CommandType = CommandType.Text};

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
                lblCatchError.Text += "Error at SendMail: " + ex.Message;
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
                    lblCatchError.Text += "Error: Update Password of Organization email.  The Decryption algorithm does not recognize the encryption.";
                }
            }
            bool blnPassed = true;

            try
            {
                email = new MailMessage { From = new MailAddress(strFromEmail, "EV Monitoring Center") };

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
                        lblCatchError.Text += "Error at Parse Port: " + ex.Message;
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

                if (!isFeedback)
                {
                    string strEmailBody = strEmailMessage;

                    strEmailBody = strEmailBody.Replace("\n", "<br/>");
                    strEmailBody = strEmailBody.Replace(" ", "&nbsp;");

                    var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
                    var feedback = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("Feedback")].Text);
                    string userId = Membership.GetUser().ProviderUserKey.ToString();
                    var username = GetUsername(userId);

                    email.Subject = strEmailSubject;
                    email.IsBodyHtml = true;
                    if (sendResponse == "false")
                    {
                        strEmailBody = System.Web.Configuration.WebConfigurationManager.AppSettings["FeedbackNotes"];
                    }
                    email.Body = "<table>";
                    email.Body += "<b>Your Feedback </b>";
                    email.Body += "<tr><td>" + feedback + "</td></tr>";
                    email.Body += "<b>is closed by " + username + ".</b>";
                    email.Body += "</table><br>";
                    email.Body += "<table>";
                    email.Body += "<b>Here are the responses: </b>";
                    email.Body += "<tr><td> " + strEmailBody + "</td></tr>";
                    email.Body += "</table>";
                }
                else
                {
                    email.IsBodyHtml = false;
                    email.Subject = strEmailSubject;
                    email.Body = strEmailMessage;
                    if (addRoleEmails)
                    {
                        foreach (var address in list)
                        {
                            email.Bcc.Add(new MailAddress(address));
                        }
                    }

                    if (addtionalEmail != "")
                    {
                        email.To.Add(new MailAddress(addtionalEmail));
                    }
                }

                sc.Send(email);
            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                lblCatchError.Text += "Error at SendMail2:  " + ex.Message;
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

        protected bool BlnFindAssociatedRoles(List<string> ListOfRoles)
        {
            bool blnHasRole = false;

            if (_strArrRolesToAllow.Length > 0)
            {
                for (int j = 0; j < _strArrTypesToAllow.Count(); j++) // At most 4 iterations
                {
                    for (int i = 0; i < ListOfRoles.Count; i++) // At most around 3-4
                    {
                        if (ListOfRoles[i].IndexOf(_strArrTypesToAllow[j]) != -1) // current index contains allowed Type
                        {
                            for (int k = 0; k < _strArrRolesToAllow.Count(); k++)
                            {
                                // Substring(0, (total length) - (length of the allowed type)
                                // i.e. "General Administrator", take the substring to obtain only the "General" string.
                                // the + 1 to the length is to account for the " " , space.
                                if (ListOfRoles[i] == _strArrRolesToAllow[k])
                                {
                                    listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (_strArrTypesToAllow[j].Length + 1)));
                                    blnHasRole = true;
                                }
                            }
                        }
                    }
                }
            }
            else // strArrAllowedRoles is emtpy, thus just allow all role types in the strArrAllowedTypes string
            {
                for (int i = 0; i < ListOfRoles.Count; i++)
                {
                    for (int j = 0; j < _strArrTypesToAllow.Count(); j++) // At most 4 iterations
                    {
                        if (ListOfRoles[i].IndexOf(_strArrTypesToAllow[j]) != -1) // current index contains allowed Type
                        {
                            // Substring(0, (total length) - (length of the allowed type)
                            // i.e. "General Administrator", take the substring to obtain only the "General" string.
                            // the + 1 to the length is to account for the " " , space.
                            listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (_strArrTypesToAllow[j].Length + 1)));
                            blnHasRole = true;
                        }
                    }
                }
            }
            return blnHasRole;
        }

        protected bool IsUserAdministratorOrOperator(List<string> RoleList)
        { // RoleList.BinarySearch returns -1 if not found
            bool isAdmin = false;
            bool isOperator = false;
            for (int i = 0; i < RoleList.Count; i++)
            {
                if (RoleList[i].IndexOf("Administrator") != -1) // if "Administrator" is not found, return false
                    isAdmin = true;
            }
            for (int i = 0; i < RoleList.Count; i++)
            {
                if (RoleList[i].IndexOf("Operator") != -1) // if "Administrator" is not found, return false
                    isOperator = true;
            }
            return isAdmin || isOperator;
        }

        protected List<ComboOrgAndGuidClass> ReturnUniqueCombinedGUID()
        {
            SqlConnection cnn = new SqlConnection(_connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            DataTable dt2 = null;
            DataTable dt3 = null;

            try
            {
                cnn.Open(); // Open the Connection

                /// The code below stores the CityID and the city Name into a table.  This table will be accessed 
                /// later when connecting the relationship between the IDs and names in the ComboCitiesList
                /// 
                strQuery = "SELECT [ID], [Name] FROM [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt2 = new DataTable();
                da.Fill(dt2);
                da.Dispose();
                cmd.Dispose();

                // This ditionary, CityIdNameRelation, stores the ID and name of each city in the City Database
                Dictionary<string, string> CityIdNameRelation = new Dictionary<string, string>();

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    CityIdNameRelation.Add(dt2.Rows[i][0].ToString(), dt2.Rows[i][1].ToString());
                }

                strQuery = "SELECT * From CombinatedCity";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt); // Fill a datagrid with Data we can retrive them later
                int iterator = 0; // iterator is used to keep track of which index of the list differs from the previous index


                List<string> uniqueGUIDs = new List<string>(); // Create a list of string that the unique GUIDs will go into
                List<string> ActivatedCheck = new List<string>(); // Create a list of string that we can check if the city is activated

                if ((dt.Rows.Count == 0)) // Make sure that the Table is populated
                {
                    return null;
                }
                uniqueGUIDs.Add(dt.Rows[0][0].ToString()); // Add first GUID. Each from here will be the same or unique
                ActivatedCheck.Add(dt.Rows[0][3].ToString());
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    if (0 != String.Compare(uniqueGUIDs[iterator].ToString(), dt.Rows[i][0].ToString()))
                    {
                        uniqueGUIDs.Add(dt.Rows[i][0].ToString());
                        ActivatedCheck.Add(dt.Rows[i][3].ToString());
                        iterator++; // increment the iterator only if two adjacent index match.
                    }
                }

                List<int> LengthsOfMainCities = new List<int>(); // Create list to store the lengths of each combo city

                // List<ComboCityAndGuidClass> ComboCityWithGuid = new List<ComboCityAndGuidClass>();

                int adder = 0;

                for (int i = 0; i < uniqueGUIDs.Count; i++)
                {
                    strQuery = "SELECT COUNT(*) FROM [CombinatedCity] WHERE ID='" + dt.Rows[adder][0] + "'";

                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt3 = new DataTable();
                    da.Fill(dt3);
                    LengthsOfMainCities.Add(Convert.ToInt32(dt3.Rows[0][0].ToString()));
                    adder += LengthsOfMainCities[i];
                }
                da.Dispose();
                cmd.Dispose();

                List<string> ComboCitiesList = new List<string>();   // This list will contain all of the Main City + combo cities.  
                //                                                   //  The main city will always be the 0 index. i.e. [0]

                iterator = 0;
                bool MainCityRead = false;

                for (int j = 0; j < dt.Rows.Count; j++) // This for loop creates the ComboCitiesList variable which stores all of the combocity/main city IDs, we will need to retrieve the actual names later
                {
                    if (0 == String.Compare(dt.Rows[j][0].ToString(), uniqueGUIDs[iterator]) && !MainCityRead)
                    {
                        MainCityRead = true;
                        ComboCitiesList.Add(dt.Rows[j][2].ToString());
                    }
                    if (0 == String.Compare(dt.Rows[j][0].ToString(), uniqueGUIDs[iterator]))
                    {
                        ComboCitiesList.Add(dt.Rows[j][1].ToString());
                        if (j < dt.Rows.Count - 1 && 0 != (String.Compare(dt.Rows[j + 1][0].ToString(), uniqueGUIDs[iterator])))
                        {
                            iterator++;
                            MainCityRead = false;
                        }
                    }
                }
                List<ComboOrgAndGuidClass> ComboCityWithGuid = new List<ComboOrgAndGuidClass>();
                List<string> ComboCitiesWithCityNames = new List<string>(); // Create a list of all the Actual City names

                for (int i = 0; i < ComboCitiesList.Count; i++)
                {
                    ComboCitiesWithCityNames.Add(CityIdNameRelation[ComboCitiesList[i]]);
                }

                List<List<string>> CityList = new List<List<string>>(); // 2D nest Store all City values
                List<string> ReturnCityList = new List<string>(); // The second 2D nest to return.
                iterator = 0;
                for (int i = 0; i < LengthsOfMainCities.Count; i++)
                {
                    // Populate the sublist with the main city and sub city combos
                    List<string> sublist = new List<string>(); // Create a sublist to enter in the main-subcity data

                    for (int v = iterator; v <= iterator + LengthsOfMainCities[i]; v++)
                    {
                        sublist.Add(ComboCitiesWithCityNames[v]);
                    }
                    //
                    // Add the sublist to the top-level List reference.
                    //
                    CityList.Add(sublist);
                    iterator += LengthsOfMainCities[i] + 1;
                }

                string subcities;

                for (int i = 0; i < LengthsOfMainCities.Count; i++)
                {
                    subcities = string.Empty;
                    for (int v = 0; v <= LengthsOfMainCities[i]; v++)
                    {
                        subcities += CityList[i][v];
                        if (v != LengthsOfMainCities[i])
                            subcities += " - ";
                    }
                    //ReturnCityList.Add(subcities); // Add the maincity - subcity combo to this list<string>
                    ComboCityWithGuid.Add(new ComboOrgAndGuidClass(subcities, uniqueGUIDs[i]));
                }

                da.Dispose();
                cmd.Dispose();
                cnn.Close();
                return ComboCityWithGuid;

            }
            catch
            {
                cnn.Close();
                return null;
            }
        }

        protected void PopulateOrganization()
        {
            List<ComboOrgAndGuidClass> ComboGuid = ReturnUniqueCombinedGUID();
            // Check if the User's role contains the master role
            // If it does NOT contain
            var blnListhasMasterRole = false;
            foreach (string strRole in listApprovedRoles)
            {
                if (_strArrMasterOrgs.Contains(strRole))
                {
                    blnListhasMasterRole = true;
                    break;
                }
            }

            // List to hold a set of city GUIDs based on the user's roles.
            var listCityGuid = new List<string>();

            // If the users role does not have a master's role,
            // then only populate the user's specific cities.
            if (!blnListhasMasterRole)
            {
                foreach (string str in listApprovedRoles)
                {
                    // listCityGUID now holds the Organization GUIDS to all of the user's roles

                    listCityGuid.Add(ReturnGuidfromCityname(str));
                }
            }

            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var strListItemInsert = "";
                    string strQuery;
                    if (blnListhasMasterRole)
                    {
                        strQuery = "SELECT ID, Name FROM City";
                        strListItemInsert = "All Organizations";
                    }
                    // else If the user is NOT a master role, then only populate the associated roles.
                    else
                    {
                        strQuery = "SELECT ID, Name FROM CITY WHERE ";
                        int listCount = listCityGuid.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            strQuery += " ID = '" + listCityGuid[i] + "' ";
                            if (i < listCount - 1)
                            {
                                strQuery += " OR ";
                            }
                        }

                    }
                    // Order the data by the name
                    strQuery += " ORDER BY Name";


                    var cmd = new SqlCommand(strQuery, cnn);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter()
                    {
                        SelectCommand = cmd,
                    };
                    da.Fill(dt);
                    ddlOrganization.DataSource = dt;
                    ddlOrganization.DataValueField = "ID";
                    ddlOrganization.DataTextField = "Name";
                    ddlOrganization.DataBind();
                    if (strListItemInsert != "")
                    {
                        var l1 = new ListItem(strListItemInsert, "-1");
                        ddlOrganization.Items.Insert(0, l1);
                    }
                    int ddlOrganizationSize = ddlOrganization.Items.Count;

                    if (ComboGuid != null)
                    {
                        ListItem ComboCityList = new ListItem();
                        ListItem ComboCityList2 = new ListItem();
                        for (int i = 0; i < ComboGuid.Count; i++) // Add
                        {
                            ComboCityList = new ListItem(ComboGuid[i].ComboOrgString, ComboGuid[i].Guid);
                            ddlOrganization.Items.Insert(ddlOrganizationSize + i, ComboCityList);
                        }
                    }
                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at populateOrg: " + ex.Message);
                }
            }
        }

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void PopUpMessage(string Message)
        {
            string message2 = Message;
            var sb2 = new System.Text.StringBuilder();
            sb2.Append("<script type = 'text/javascript'>");
            sb2.Append("window.onload=function(){");
            sb2.Append("alert('");
            sb2.Append(message2);
            sb2.Append("')};");
            sb2.Append("</script>");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", sb2.ToString());
        }

        protected void ddlOrganizationSelectedIndexChanged(object sender, EventArgs e)
        {
            gvMaintenanceRecords.SelectedIndex = -1;
            ClearAll();
            HideButtons();
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked, cbShowUnresolvedRecords.Checked);
        }

        protected void ClearAll()
        {
            gvMaintenanceRecords.SelectedIndex = -1;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
            lblUsername.Text = string.Empty;
            lblRole.Text = string.Empty;
            lblPhone.Text = string.Empty;
            lblIssueNo.Text = string.Empty;
            lblIssueDate.Text = string.Empty;
            tbFeedback.Text = string.Empty;
            lblEmail.Text = string.Empty;
            lblFullName.Text = string.Empty;
            ddlEvInfo.Items.Clear();
            tbNewAction.Text = string.Empty;
            tbNewResolution.Text = string.Empty;
            tbNewResponse.Text = string.Empty;
            HideButtons();
        }

        protected void btnHideCatchErrorClick(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected void gvMaintenanceRecordsPaging(object sender, GridViewPageEventArgs e)
        {
            gvMaintenanceRecords.SelectedIndex = -1;
            var dataTable = Session["data"] as DataTable;
            gvMaintenanceRecords.PageIndex = e.NewPageIndex;
            gvMaintenanceRecords.DataSource = dataTable;
            gvMaintenanceRecords.DataBind();
        }

        protected string ReturnGuidfromCityname(string UserCity)
        {
            var cnn = new SqlConnection(_connectionString);
            var userCityGUID = string.Empty;
            try
            {
                var strQuery = "SELECT ID FROM [City] WHERE [Name] = '" + UserCity + "'";
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
                userCityGUID = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br>Error from: ReturnGuidfromCityname " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            return userCityGUID;
        }

        private void ShowButtons()
        {
            var listOfRoles = new List<string>();
            string[] roles = Roles.GetRolesForUser();
            for (int i = 0; i < roles.Count(); i++)
            {
                listOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            btnSubmit.Visible = true;
            btnResolve.Visible = true;
            if (IsUserAdministratorOrOperator(listOfRoles))
            {
                btnClose.Visible = true;
                btnResolveAndClose.Visible = true;
            }

            btnCancel.Visible = true;
        }

        private void HideButtons()
        {
            btnSubmit.Visible = false;
            btnResolve.Visible = false;
            btnClose.Visible = false;
            btnCancel.Visible = false;
            btnResolveAndClose.Visible = false;
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
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br>Error from: SelectResponseIDs " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            return responseIDs;
        }

        protected GridViewRow FillFeedback()
        {
            var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
            var userId = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("UserID")].Text);
            var userName = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("Username")].Text);
            var issueNo = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("Issue No")].Text);
            var issueDate = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("Timestamp")].Text);
            var feedback = Server.HtmlDecode(gvRow.Cells[FindGVcolumn("Feedback")].Text);
            FillRoleName(userId);
            FillPhoneNumber(userId);
            FillEvInfo(userId);
            FillEmail(userId);
            lblUsername.Text = userName;
            lblIssueNo.Text = issueNo;
            lblIssueDate.Text = issueDate;
            tbFeedback.Text = feedback;
            return gvRow;
        }

        protected void GenerateResponseTable(string id)
        {
            var cnn = new SqlConnection(_connectionString);
            try
            {
                var strQuery = "SELECT u.UserName, f.Timestamp, f.Response, f.Action, f.Resolution, p.FirstName, p.LastName, p.PhoneNo, m.Email, r.RoleName FROM [FeedbackResponse] as f, [aspnet_Users] as u, [aspnet_Profile] as p, [aspnet_Membership] as m, [aspnet_Roles] as r, [aspnet_UsersInRoles] as uir WHERE u.UserId = f.ResponderID AND p.UserId = f.ResponderID AND m.UserId = f.ResponderID AND r.RoleId = uir.RoleId AND uir.UserId = f.ResponderID AND f.ID = '" + id + "'";
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
                var tRowName = new TableRow();
                var tCellName = new TableCell { Text = "Responder Name: " + dt.Rows[0][0].ToString(), Width = 310};
                tRowName.Cells.Add(tCellName);
                var tCellResponseLabel = new TableCell { Text = "Response:"};
                tRowName.Cells.Add(tCellResponseLabel);
                rightSideTable.Rows.Add(tRowName);

                var tRowDate = new TableRow();
                var tCellDate = new TableCell { Text = "Date: " + dt.Rows[0][1].ToString()};
                var tCellResponse = new TableCell();
                var tbResponse = new TextBox
                {
                    Text = dt.Rows[0][2].ToString(),
                    Width = 350,
                    TextMode = TextBoxMode.MultiLine,
                    Rows = 2
                };
                tCellResponse.Controls.Add(tbResponse);
                tRowDate.Cells.Add(tCellDate);
                tRowDate.Cells.Add(tCellResponse);
                rightSideTable.Rows.Add(tRowDate);

                var tRowFullName = new TableRow();
                var tCellFullName = new TableCell { Text = "Full Name: " + dt.Rows[0][5].ToString() + " " + dt.Rows[0][6].ToString() };
                tRowFullName.Cells.Add(tCellFullName);
                var tCellActionLabel = new TableCell { Text = "Action:" };
                tRowFullName.Cells.Add(tCellActionLabel);
                rightSideTable.Rows.Add(tRowFullName);

                var tRowEmail = new TableRow();
                var tCellEmail = new TableCell { Text = "Email: " + dt.Rows[0][8].ToString() };
                var tCellAction = new TableCell();
                var tbAction = new TextBox
                {
                    Text = dt.Rows[0][3].ToString(),
                    Width = 350,
                    TextMode = TextBoxMode.MultiLine,
                    Rows = 2
                };
                tCellAction.Controls.Add(tbAction);
                tRowEmail.Cells.Add(tCellEmail);
                tRowEmail.Cells.Add(tCellAction);
                rightSideTable.Rows.Add(tRowEmail);

                var tRowRoleName = new TableRow();
                var tCellRoleName = new TableCell { Text = "Role Name: " + dt.Rows[0][9].ToString()};
                tRowRoleName.Cells.Add(tCellRoleName);
                var tCellResolutionLabel = new TableCell { Text = "Resolution:" };
                tRowRoleName.Cells.Add(tCellResolutionLabel);
                rightSideTable.Rows.Add(tRowRoleName);

                var tRowPhoneNo = new TableRow();
                var tCellPhoneNo = new TableCell { Text = "Phone No: " + dt.Rows[0][7].ToString() };
                var tCellResolution = new TableCell();
                var tbResolution = new TextBox
                {
                    Text = dt.Rows[0][4].ToString(),
                    Width = 350,
                    TextMode = TextBoxMode.MultiLine,
                    Rows = 2
                };
                tCellResolution.Controls.Add(tbResolution);
                tRowPhoneNo.Cells.Add(tCellPhoneNo);
                tRowPhoneNo.Cells.Add(tCellResolution);
                rightSideTable.Rows.Add(tRowPhoneNo);

                rightSideTable.Rows.Add(PlaceHolderRow());
            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br>Error from: GenerateResponseTable " + ex.Message;
            }
            finally
            {
                cnn.Close();
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

        protected void gvMaintenanceRecordsSelectedIndex(object sender, EventArgs e)
        {
            ShowButtons();
            var gvRow = FillFeedback();
            var feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
            var responseIDs = SelectResponseIDs(feedbackId);
            foreach (var id in responseIDs)
            {
                GenerateResponseTable(id);
            }
        }

        protected void gvMaintenanceRecordsRowCreated(object sender, GridViewRowEventArgs e)
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

        protected int FindGVcolumn(string name)
        {
            for (int j = 0; j < gvMaintenanceRecords.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvMaintenanceRecords.Columns[j].HeaderText == name)
                    return j;
            }
            return -1;
        }

        protected void gvMaintenanceRecordsSorting(object sender, GridViewSortEventArgs e)
        {
            var dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                var dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + GetSortDirectionString(e.SortDirection.ToString());
                gvMaintenanceRecords.DataSource = dataTable.DefaultView;
                gvMaintenanceRecords.DataBind();
            }
            gvMaintenanceRecords.SelectedIndex = -1;
            ClearAll();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvMaintenanceRecords.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvMaintenanceRecords.Columns.IndexOf(field);
                }
            }
            var sortImage2 = new Image();
            if (GetSortDirectionString1("Ascending") == "ASC")
            {
                sortImage2.ImageUrl = "~/Images/asc.gif";
                sortImage2.AlternateText = "Ascending Order";
            }
            else
            {
                sortImage2.ImageUrl = "~/Images/desc.gif";
                sortImage2.AlternateText = "Descending Order";
            }
            // Add the image to the appropriate header cell.            
            gvMaintenanceRecords.HeaderRow.Cells[index].Controls.Add(sortImage2);
        }

        private string GetSortDirectionString(string column)
        {
            // By default, set the sort direction to ascending.
            var sortDirection = "ASC";

            // Retrieve the last column that was sorted.
            var sortExpression = ViewState["SortExpression"] as string;

            if (sortExpression != null)
            {
                // Check if the same column is being sorted.
                // Otherwise, the default value can be returned.
                if (sortExpression == column)
                {
                    var lastDirection = ViewState["SortDirection"] as string;
                    if ((lastDirection != null) && (lastDirection == "ASC"))
                    {
                        sortDirection = "DESC";
                    }
                }
            }
            // Save new values in ViewState.
            ViewState["SortDirection"] = sortDirection;
            ViewState["SortExpression"] = column;

            return sortDirection;
        }

        private string GetSortDirectionString1(string column)
        {
            // By default, set the sort direction to ascending.
            var sortDirection = "ASC";

            // Retrieve the last column that was sorted.
            var sortExpression = ViewState["SortExpression1"] as string;

            if (sortExpression != null)
            {
                // Check if the same column is being sorted.
                // Otherwise, the default value can be returned.
                if (sortExpression == column)
                {
                    var lastDirection = ViewState["SortDirection1"] as string;
                    if ((lastDirection != null) && (lastDirection == "ASC"))
                    {
                        sortDirection = "DESC";
                    }
                }
            }
            // Save new values in ViewState.
            ViewState["SortDirection1"] = sortDirection;
            ViewState["SortExpression1"] = column;

            return sortDirection;
        }

        //protected void PopulateUsername()
        //{
        //    using (var cnn = new SqlConnection(_connectionString))
        //    {
        //        try
        //        {
        //            var strQuery = "SELECT UserId, UserName FROM aspnet_Users";
        //            const string strListItemInsert = "Select...";
        //            strQuery += " ORDER BY UserName";
        //            var cmd = new SqlCommand(strQuery, cnn);
        //            var dt = new DataTable();
        //            var da = new SqlDataAdapter()
        //            {
        //                SelectCommand = cmd,
        //            };
        //            da.Fill(dt);
        //            ddlUsername.DataSource = dt;
        //            ddlUsername.DataValueField = "UserId";
        //            ddlUsername.DataTextField = "UserName";
        //            ddlUsername.DataBind();

        //            var l1 = new ListItem(strListItemInsert, "-1");
        //            ddlUsername.Items.Insert(0, l1);
                    
        //            da.Dispose();
        //            cmd.Dispose();
        //        }
        //        catch (Exception ex)
        //        {
        //            ShowError("Error at populateUsername: " + ex.Message);
        //        }
        //    }
        //}

        protected string GetUserEmail(String userId)
        {
            string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            var dt = new DataTable();
            var strQuery = "Select Email from aspnet_Membership where UserId = '" + userId + "'";
            var sdr = new SqlDataAdapter(strQuery, strCnn);
            sdr.Fill(dt);
            return dt.Rows[0][0].ToString();
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

        protected string GetFeedbackPosterEmail(string feedbackId)
        {
            var strQuery = "SELECT UserID from Feedback where ID = '" + feedbackId + "'";
            using (var cnn = new SqlConnection(_connectionString))
            {
                var dt = new DataTable();
                var sdr = new SqlDataAdapter(strQuery, cnn);
                sdr.Fill(dt);
                return GetUserEmail(dt.Rows[0][0].ToString());
            }
        }

        protected string GetUsername(string userId)
        {
            var username = string.Empty;
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var strQuery = "SELECT UserName " +
                                   "From aspnet_Users " +
                                   "WHERE UserId = '" + userId + "'";
                    var cmd = new SqlCommand(strQuery, cnn);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter()
                    {
                        SelectCommand = cmd
                    };
                    da.Fill(dt);
                    username= dt.Rows[0][0].ToString();
                }
                catch (Exception ex)
                {
                    ShowError("Error at FillUserName: " + ex.Message);
                }
            }
            return username;
        }

        protected void FillRoleName(string userId)
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var strQuery = "SELECT RoleName " +
                                   "From aspnet_Roles as r, aspnet_UsersInRoles as i " +
                                   "WHERE r.RoleId = i.RoleId AND i.UserId = '" + userId + "'";
                    var cmd = new SqlCommand(strQuery, cnn);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter()
                    {
                        SelectCommand = cmd
                    };
                    da.Fill(dt);
                    lblRole.Text = dt.Rows[0][0].ToString();
                }
                catch (Exception ex)
                {
                    ShowError("Error at FillRoleName: " + ex.Message);
                }
            }
        }

        protected void FillPhoneNumber(string userId)
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var strQuery = "SELECT PhoneNo, FirstName, LastName " +
                                   "From [aspnet_Profile] " +
                                   "WHERE UserId = '" + userId + "'";
                    var cmd = new SqlCommand(strQuery, cnn);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter()
                    {
                        SelectCommand = cmd
                    };
                    da.Fill(dt);
                    lblPhone.Text = dt.Rows[0][0].ToString();
                    lblFullName.Text = dt.Rows[0][1].ToString() + " " + dt.Rows[0][2].ToString();
                }
                catch (Exception ex)
                {
                    ShowError("Error at FillPhoneNumber: " + ex.Message);
                }
            }
        }

        protected void FillEmail(string userId)
        {
            var cnn = new SqlConnection(_connectionString);
            try
            {
                cnn.Open();
                var strQuery = "SELECT Email FROM [aspnet_Membership] WHERE UserId = '" + userId + "'";

                var cmd = new SqlCommand(strQuery, cnn) { CommandType = CommandType.Text };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lblEmail.Text = reader["Email"].ToString().Trim();
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at FillEmail: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }
        }

        protected void FillEvInfo(string userId)
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    var strQuery = "SELECT DISTINCT ID, Manufacturer+' '+Model AS [EV Info] FROM [EV Model] as e, [FleetVehicles] as f WHERE e.ID = f.EVModelID AND f.UserID = '" + userId + "'";
                    var cmd = new SqlCommand(strQuery, cnn);
                    var DT = new DataTable();
                    var DA = new SqlDataAdapter {SelectCommand = cmd};
                    DA.Fill(DT);
                    ddlEvInfo.DataSource = DT;
                    ddlEvInfo.DataValueField = "ID";
                    ddlEvInfo.DataTextField = "EV Info";
                    ddlEvInfo.DataBind();
                    var l1 = new ListItem("NULL", "-1");
                    if (DT.Rows.Count == 0)
                        ddlEvInfo.Items.Insert(0, l1);
                    DA.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at FillEvInfo: " + ex.Message);
                }
            }
        }

        protected void PopulateMaintenanceRecords(string strOrgId, bool onlyShowUnclosed, bool onlyShowUnresolved)
        {
            var DT = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                if (strOrgId == "-1")
                {
                    strOrgId = string.Empty;
                }

                var sqlQuery =
                    "SELECT temp1.id1 as ID, temp1.[issue no] as [Issue No], temp1.timestamp as Timestamp, temp1.UserId as UserID, temp1.UserName as Name, temp1.feedback as Feedback, temp1.closed as Closed, temp1.resolved as Resolved, temp1.[resolved date] as [Resolved Date], temp1.fixer as Fixer, temp4.MSG " +
                    "FROM (Select f.[issue no], f.Timestamp, f.feedback, f.closed, f.resolved, f.[Resolved Date], f.id as id1, f.userid, f.Fixer, u.username " +
                    "From Feedback as f " +
                    "INNER JOIN aspnet_Users as u ON f.userid = u.UserId) as temp1 " + 
                    "INNER JOIN (SELECT f.ID, COUNT(fr.FeedbackID) AS MSG " +
                    "FROM [Feedback] as f LEFT JOIN [FeedbackResponse] as fr ON fr.FeedbackID = f.ID " +
                    "GROUP BY f.ID ) as temp4 ON temp4.ID = temp1.id1 " +
                    "INNER JOIN (SELECT ID, MAX(Timestamp) as LatestTime " +
                                "FROM (SELECT ID, Timestamp FROM [EVDemo].[dbo].[Feedback] as f "+
                                        "UNION ALL SELECT FeedbackID, Timestamp FROM [EVDemo].[dbo].[FeedbackResponse] as fr) as t "+
                                "GROUP BY ID) as temp5 " + 
                    "ON temp5.ID = temp1.id1 ";

                if (strOrgId != string.Empty)
                {
                    sqlQuery += "INNER JOIN (Select UserId, RoleCityID " +
                                "From aspnet_profile p Where RoleCityID = '" + strOrgId +
                                "') as temp3 on temp3.UserId = temp1.userid ";
                }

                if (onlyShowUnclosed)
                {
                    sqlQuery += "WHERE Closed = '0' ";
                    if (onlyShowUnresolved)
                    {
                        sqlQuery += "AND Resolved = '0'";
                    }
                }
                else
                {
                    if (onlyShowUnresolved)
                    {
                        sqlQuery += "WHERE Resolved = '0'";
                    }
                }
                sqlQuery += " ORDER by temp5.LatestTime DESC";

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
                            ShowError("Error at populateMaintenanceRecords: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0)
                        {
                            ShowError("No data in this selection");
                        }
                    }
                }

                Session["data"] = DT;
                gvMaintenanceRecords.DataSource = Session["data"];
                gvMaintenanceRecords.DataBind();

            }
        }

        protected void cbShowUnresolvedRecordsCheckedChanged(object sender, EventArgs e)
        {
            ClearAll();
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
        }

        protected void cbShowUnclosedRecordsCheckedChanged(object sender, EventArgs e)
        {
            ClearAll();
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
        }

        protected void btnSubmitClick(object sender, EventArgs e)
        {
            var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
            string userId = Membership.GetUser().ProviderUserKey.ToString();
            var timestamp = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo);
            var feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;

            using (var cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    const string strQuery =
                        "INSERT [FeedbackResponse] (ResponderID, Timestamp, FeedbackID, Response, Action, Resolution) VALUES (@UserID, @Timestamp, @FeedbackID, @Response, @Action, @Resolution) ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@UserID", userId));
                    cmd.Parameters.Add(new SqlParameter("@Timestamp", timestamp));
                    cmd.Parameters.Add(new SqlParameter("@FeedbackID", feedbackId));
                    cmd.Parameters.Add(new SqlParameter("@Response", tbNewResponse.Text));
                    cmd.Parameters.Add(new SqlParameter("@Action", tbNewAction.Text));
                    cmd.Parameters.Add(new SqlParameter("@Resolution", tbNewResolution.Text));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnSubmitClick: " + ex.Message);
                    return;
                }
            }
            string strUserEmail = GetUserEmail(userId);

            string strSubject = "Response from user " + User.Identity.Name;

            string strContent = User.Identity.Name + " submitted a response for the feedback" +
                                " at " + timestamp;

            string sendFeedbackEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["SendFeedbackEmail"].ToString();
            string feedbackNotes = System.Web.Configuration.WebConfigurationManager.AppSettings["FeedbackNotes"].ToString();
            if (sendFeedbackEmail == "true")
            {
                strContent += ":\n\r" + tbNewResponse.Text;
            }
            else
            {
                strContent += ":\n\r" + feedbackNotes;
            }
            string strFeedbackPosterEmail = GetFeedbackPosterEmail(feedbackId);
            if (SendMail(strSendFromOrganization, strUserEmail, strContent, strSubject, true, strFeedbackPosterEmail))
            {
                PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
                gvMaintenanceRecords.SelectRow(0);//when submitting new response, it should always appear as the first in the table
                tbNewAction.Text = string.Empty;
                tbNewResolution.Text = string.Empty;
                tbNewResponse.Text = string.Empty;
                PopUpMessage("Response Submitted");
            }
        }

        protected void btnResolveClick(object sender, EventArgs e)
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
                try
                {
                    const string strQuery = "UPDATE [Feedback] SET [Resolved] = @Resolved, [Resolved Date] = @ResolvedDate WHERE [ID] = @ID";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    var feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
                    cmd.Parameters.Add(new SqlParameter("@ID", feedbackId));
                    cmd.Parameters.Add(new SqlParameter("@Resolved", "1"));
                    var resolvedDate = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo);
                    cmd.Parameters.Add(new SqlParameter("@ResolvedDate", resolvedDate));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnResolveClick: " + ex.Message);
                    return;
                }
            }
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
            ClearAll();
            PopUpMessage("Feedback Resolved");
        }


        protected void btnResolveAndCloseClick(object sender, EventArgs e)
        {
            string feedbackId;
            using (var cnn = new SqlConnection(_connectionString))
            {
                var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
                try
                {
                    const string strQuery = "UPDATE [Feedback] SET [Resolved] = @Resolved, [Resolved Date] = @ResolvedDate, [Closed] = @Closed WHERE [ID] = @ID";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
                    cmd.Parameters.Add(new SqlParameter("@ID", feedbackId));
                    cmd.Parameters.Add(new SqlParameter("@Resolved", "1"));
                    cmd.Parameters.Add(new SqlParameter("@Closed", "1"));
                    var resolvedDate = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo);
                    cmd.Parameters.Add(new SqlParameter("@ResolvedDate", resolvedDate));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnResolveClick: " + ex.Message);
                    return;
                }
            }
            if (SendMail(strSendFromOrganization, lblEmail.Text, GetFeedbackResponse(feedbackId),
                "UCLA SMERC EV Feedback Closed"))
                ClearAll();
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
            PopUpMessage("Feedback Resolved and Closed");
        }

        protected void btnCloseClick(object sender, EventArgs e)
        {
            string feedbackId;
            using (var cnn = new SqlConnection(_connectionString))
            {
                var gvRow = gvMaintenanceRecords.Rows[gvMaintenanceRecords.SelectedIndex];
                try
                {
                    const string strQuery = "UPDATE [Feedback] SET [Closed] = @Closed WHERE [ID] = @ID";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    feedbackId = gvRow.Cells[FindGVcolumn("ID")].Text;
                    cmd.Parameters.Add(new SqlParameter("@ID", feedbackId));
                    cmd.Parameters.Add(new SqlParameter("@Closed", "1"));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnCloseClick: " + ex.Message);
                    return;
                }
            }
            if(SendMail(strSendFromOrganization, lblEmail.Text, GetFeedbackResponse(feedbackId),
                "UCLA SMERC EV Feedback Closed"))
                ClearAll();
            PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked,
                cbShowUnresolvedRecords.Checked);
            PopUpMessage("Feedback Closed");
        }

        protected void btnCancelClick(object sender, EventArgs e)
        {
            ClearAll();
        }

        protected void btnFeedbackClick(object sender, EventArgs e)
        {
            var timestamp = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo);
            string userId = Membership.GetUser().ProviderUserKey.ToString();

            using (var cnn = new SqlConnection(_connectionString))
            {

                try
                {
                    const string strQuery =
                        "INSERT [Feedback] (UserID, Timestamp, Feedback) VALUES (@UserID, @Timestamp, @Feedback) ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@UserID", userId));
                    cmd.Parameters.Add(new SqlParameter("@Timestamp", timestamp));
                    cmd.Parameters.Add(new SqlParameter("@Feedback", tbFeedback.Text));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnSubmitClick: " + ex.Message);
                    return;
                }
            }
            string strUserEmail = GetUserEmail(userId);

            string strSubject = "Feedback from user " + User.Identity.Name;

            string strContent = "The following feedback has been successfully submitted for user " + User.Identity.Name +
                                " at " + timestamp + ":\n\r" + tbFeedback.Text;

            if (SendMail(strSendFromOrganization, strUserEmail, strContent, strSubject, true, "", true))
            {
                ClearAll();
                PopulateMaintenanceRecords(ddlOrganization.SelectedValue, cbShowUnclosedRecords.Checked, cbShowUnresolvedRecords.Checked);
                PopUpMessage("New Feedback Submitted");
            }            
        }
    }
    public class ComboOrgAndGuidClass // Class used to obtain and maintain the CombintedOrganization
    {
        public ComboOrgAndGuidClass(string ComboOrgString, string Guid)
        {
            _ComboOrgString = ComboOrgString;
            _Guid = Guid;
        }

        private string _Guid;

        public string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }
        }

        private string _ComboOrgString;

        public string ComboOrgString
        {
            get { return _ComboOrgString; }
            set { _ComboOrgString = value; }
        }
    }
}