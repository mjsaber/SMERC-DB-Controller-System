using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System.Windows;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.IO;

using RTMC;

namespace EVEditor
{
    public partial class EmailValidation : System.Web.UI.Page
    {
        // connectionString is the string to connect to the SQL database.
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };

        // choose the Organization to send the email from.
        string strSendFromOrganization = "General";

        //Other than the General Administrator, all other allowed roles may only access their Organization's Gateways
        string[] strArrRolesToAllow = {};

        //strArrTypesToAllow is the role types that will be inserted in the string array above ^, into strArrRolesToAllow
        //The "EditGateway" function assumes that only "General Administrators" may have full accessibility to all organizations.
        string[] strArrTypesToAllow = { "Administrator", "Operator" }; //Make sure there are no white spaces before or after the string. i.e. " Maintainer"



        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> ListOfAdminCities = new List<string>();
            if (User.Identity.IsAuthenticated)
            {
                RolePrincipal rp = (RolePrincipal)User;
                string[] roles = Roles.GetRolesForUser();
                List<string> ListOfRoles = new List<string>();
                for (int i = 0; i < roles.Count(); i++)
                {
                    ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
                }
                ListOfAdminCities = FindAssociatedRoles(ListOfRoles);
                bool isAdministrator = IsUserAdministrator(ListOfRoles); // Check if atleast one of the roles is an admin role
                bool isOperator = IsUserOperator(ListOfRoles);

                if (isAdministrator || isOperator) // only continue if the user is a city administrator
                {
                    voidLockOutFeatures(ListOfAdminCities, isAdministrator);
                }
                else
                {
                    Response.Redirect("~/Info.aspx?ErrMsg=Privilege", false);
                    return;
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }

            // If the page_load was not triggered by a postback,
            if (!IsPostBack)
            {
                InitializePage(ListOfAdminCities);
            }
        }

        protected List<string> FindAssociatedRoles(List<string> ListOfRoles)
        {
            List<string> ListOfAdminCities = new List<string>();

            if (strArrRolesToAllow.Length > 0)
            {
                for (int j = 0; j < strArrTypesToAllow.Count(); j++) // At most 4 iterations
                {
                    for (int i = 0; i < ListOfRoles.Count; i++) // At most around 3-4
                    {
                        if (ListOfRoles[i].IndexOf(strArrTypesToAllow[j]) != -1) // current index contains allowed Type
                        {
                            for (int k = 0; k < strArrRolesToAllow.Count(); k++)
                            {
                                // Substring(0, (total length) - (length of the allowed type)
                                // i.e. "General Administrator", take the substring to obtain only the "General" string.
                                // the + 1 to the length is to account for the " " , space.
                                if (ListOfRoles[i] == strArrRolesToAllow[k])
                                {
                                    ListOfAdminCities.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));
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
                    for (int j = 0; j < strArrTypesToAllow.Count(); j++) // At most 4 iterations
                    {
                        if (ListOfRoles[i].IndexOf(strArrTypesToAllow[j]) != -1) // current index contains allowed Type
                        {
                            // Substring(0, (total length) - (length of the allowed type)
                            // i.e. "General Administrator", take the substring to obtain only the "General" string.
                            // the + 1 to the length is to account for the " " , space.
                            ListOfAdminCities.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));

                        }
                    }
                }
            }
            return ListOfAdminCities;
        }

        protected bool IsUserAdministrator(List<string> RoleList)
        { // RoleList.BinarySearch returns -1 if not found
            for (int i = 0; i < RoleList.Count; i++)
            {
                if (RoleList[i].IndexOf("Administrator") != -1) // if "Administrator" is not found, return false
                    return true;
            }
            return false;
        }

        protected bool IsUserOperator(List<string> RoleList)
        { // RoleList.BinarySearch returns -1 if not found
            for (int i = 0; i < RoleList.Count; i++)
            {
                if (RoleList[i].IndexOf("Operator") != -1) // if "Operator" is not found, return false
                    return true;
            }
            return false;
        }

        //protected bool blnFindAssociatedRoles(List<string> ListOfRoles)
        //{
        //    bool blnHasRole = false;

        //    if (strArrRolesToAllow.Length > 0)
        //    {
        //        for (int j = 0; j < strArrTypesToAllow.Count(); j++) // At most 4 iterations
        //        {
        //            for (int i = 0; i < ListOfRoles.Count; i++) // At most around 3-4
        //            {
        //                if (ListOfRoles[i].IndexOf(strArrTypesToAllow[j]) != -1) // current index contains allowed Type
        //                {
        //                    for (int k = 0; k < strArrRolesToAllow.Count(); k++)
        //                    {
        //                        // Substring(0, (total length) - (length of the allowed type)
        //                        // i.e. "General Administrator", take the substring to obtain only the "General" string.
        //                        // the + 1 to the length is to account for the " " , space.
        //                        if (ListOfRoles[i] == strArrRolesToAllow[k])
        //                        {
        //                            listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));
        //                            blnHasRole = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else // strArrAllowedRoles is emtpy, thus just allow all role types in the strArrAllowedTypes string
        //    {
        //        for (int i = 0; i < ListOfRoles.Count; i++)
        //        {
        //            for (int j = 0; j < strArrTypesToAllow.Count(); j++) // At most 4 iterations
        //            {
        //                if (ListOfRoles[i].IndexOf(strArrTypesToAllow[j]) != -1) // current index contains allowed Type
        //                {
        //                    // Substring(0, (total length) - (length of the allowed type)
        //                    // i.e. "General Administrator", take the substring to obtain only the "General" string.
        //                    // the + 1 to the length is to account for the " " , space.
        //                    listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));
        //                    blnHasRole = true;
        //                }
        //            }
        //        }
        //    }
        //    return blnHasRole;
        //} 

        protected void InitializePage(List<string> ListOfAdminCities)
        {            
            voidPopulateddlOrganization(ListOfAdminCities);
            voidPopulateddlEVUserAccountType();
            voidPopulateddlEVUserAccountExpirationWindow();
            voidPopulateddlMaxVehicles();
            voidHideCatchError();
        }


        #region Email Functions
        
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

        protected bool SendMail(string strOrganization, string strSendTo, string strEmailMessage, string strEmailSubject)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            MailMessage email = null;
            SmtpClient sc = null;


            // strEmailPass = The Email accounts password.
            // strSmtpHost = The SMTP host of the email host.
            // blnSSL = The SSL security settings of the email host.  (either T/F)
            // strSmtpport = The Port of the email host. (can be null)

            string strSmtpHost = string.Empty;
            string strEmailPass = string.Empty;
            bool blnSSL = true;
            string strSmtpport = string.Empty;
            string strFromEmail = string.Empty;

            try
            {
                cnn.Open();
                strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strOrganization + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

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
                //ShowMessage("Error at SendMail: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
                return false;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }



            // Obtain Password using Rijndael Encryption/Decryption.  The keys are stored in the web.config file
            using (RijndaelManaged myR = new RijndaelManaged())
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
                    //ShowMessage("Error: Update Password of Organization email.  The Decryption algorithm does not recognize the encryption.");
                    return false;
                }
            }
            bool blnPassed = true;

            try
            {
                email = new MailMessage();
                //strFromEmail = "support@smartgrid.ucla.edu";
                email.From = new MailAddress(strFromEmail, "EV Monitoring Center");

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
                       // ShowMessage("Port information is incorrect.  Please check Edit Organization page to ensure the port is correct.");
                    }
                }

                sc.Credentials = new NetworkCredential(strFromEmail, strEmailPass);
                sc.EnableSsl = blnSSL;
                sc.Timeout = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EmailServerTimeOut"].ToString());
                //email.To.Add(strFromEmail);
                email.Bcc.Add(strSendTo);

                string strEmailBody = strEmailMessage;

                strEmailBody = strEmailBody.Replace("\n", "<br/>");
                strEmailBody = strEmailBody.Replace(" ", "&nbsp;");

                email.Subject = strEmailSubject;
                email.IsBodyHtml = true;
                email.Body = "<table>";
                email.Body += "<tr><td> " + strEmailBody + "</td></tr>";
                email.Body += "</table>";
                sc.Send(email);
            }
            catch (Exception ex)
            {
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
        #endregion

        #region Authentication HelperFunctions

        // Check to see if the user has privelage to view the page.
        // For each allowed type, (i.e. "Administrator"), cycle through each of the roles
        // and check to see that the user is allowed.

        protected void voidLockOutFeatures(List<string> ListOfAdminCities, bool isAdministrator)
        {
            // Check if the ListOfAdminCities contains any of the
            // specific master roles in, strArrMasterOrgs.
            //foreach (string strOrg in strArrMasterOrgs)
            //{
            //    // if the ListOfAdminCities does contain a master role,
            //    // do not restrict any features, else...
                
            //    if (ListOfAdminCities.IndexOf(strOrg) != -1)
            //    {
            //        ddlOrganization.Enabled = true;
            //        return;
            //    }
            //}

            // Up this point, the code will have returned if the ListOfAdminCities
            // contained a master role.  Thus the ListOfAdminCities does not 
            // contain a master role.
            if (ListOfAdminCities.IndexOf("General") != -1)
            {
                if (!isAdministrator)
                {
                    ddlEVUserAccountType.Enabled = false;
                    ddlEVUserAccountType.SelectedValue = "0";
                }
                //tbEmailAddress.Text = "general";
                //foreach (string str in ListOfAdminCities)
                //{
                //    tbEmailAddress.Text = tbEmailAddress.Text+" "+str;
                //}
                

            }
            else
            {
                //tbEmailAddress.Text = "not general";
                //foreach (string str in ListOfAdminCities)
                //{
                //    tbEmailAddress.Text = tbEmailAddress.Text + " " + str;
                //}

                if (ListOfAdminCities.Count == 1)
                {
                    ddlOrganization.Enabled = false;
                    ddlOrganization.SelectedValue = ObtainCityGUIDfromUserCity(ListOfAdminCities[0]);
                }
                ddlEVUserAccountType.Enabled = false;
                ddlEVUserAccountType.SelectedValue = "0";
            }

        }


        #endregion

        #region Populate Functions

        protected void voidPopulateddlOrganization(List<string> ListOfAdminCities)
        {           
            List<ComboOrgAndGuidClass> ComboGuid = ReturnUniqueCombinedGUID();
            
            if (ListOfAdminCities.Count <= 1)
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;
                
                try
                {
                    cnn.Open();
                    strQuery = "SELECT NAME, ID FROM [City]";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);

                    ddlOrganization.DataSource = dt;
                    ddlOrganization.DataValueField = "ID";
                    ddlOrganization.DataTextField = "Name";
                    ddlOrganization.DataBind();                    

                    ListItem li2 = new ListItem("Select...", "-1");
                    ddlOrganization.Items.Insert(0, li2);

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
                    voidShowError("Error when Populating Organization: " + ex.Message);
                }
                finally
                {
                    if (cnn != null)
                    {
                        cnn.Close();
                    }
                }
            }
            else // if ListOfAdminCities >1 (as in the roles are combinated cities)
            {
                
                ddlOrganization.Items.Clear();
                List<string> CopyOfList = new List<string>(ListOfAdminCities);
                ListItem li1 = new ListItem("Select...", "-1");
                
                ddlOrganization.Items.Insert(0, li1);
                ListItem liCityandguid; // = new ListItem("All Users", "-1");                
                for (int i = 0; i < ListOfAdminCities.Count; i++)
                {
                    liCityandguid = new ListItem(ListOfAdminCities[i], ObtainCityGUIDfromUserCity(ListOfAdminCities[i]));
                    
                    ddlOrganization.Items.Add(liCityandguid);                   
                }

                string UserName = ReturnUserGUIDfromUsername(Page.User.Identity.Name);
                
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                string ComboCity = string.Empty; ;

                try
                {
                    cnn.Open();
                    strQuery = "SELECT [RoleCityID] FROM [aspnet_Profile] where [UserId] = '" + UserName + "'";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    ComboCity = dt.Rows[0][0].ToString();
                    
                    da.Dispose();
                    cmd.Dispose();
                }
                catch
                {
                    voidShowError("Error while entering Combo City ID");                    
                }
                finally
                {
                    if (cnn != null)
                    {
                        cnn.Close();
                    }
                }
                if (ComboGuid != null)
                {
                    for (int i = 0; i < ComboGuid.Count; i++)
                    {
                        if (ComboCity == ComboGuid[i].Guid)
                        {
                            ddlOrganization.Items.Add(new ListItem(ComboGuid[i].ComboOrgString, ComboGuid[i].Guid));                            
                        }
                    }
                }
            }
        }

        protected void voidPopulateddlEVUserAccountType()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT * FROM [EVUserAccountType] ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlEVUserAccountType.DataSource = dt;
                ddlEVUserAccountType.DataValueField = "ID";
                ddlEVUserAccountType.DataTextField = "AccountType";
                ddlEVUserAccountType.DataBind();
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                voidShowError("<br> PopulateddlEVUserAccount Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void voidPopulateddlEVUserAccountExpirationWindow()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT * FROM [EVUserAccountExpirationWindow] ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlEVUserAccountExpirationWindow.DataSource = dt;
                ddlEVUserAccountExpirationWindow.DataValueField = "ID";
                ddlEVUserAccountExpirationWindow.DataTextField = "ExpirationWindow";
                ddlEVUserAccountExpirationWindow.DataBind();
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                voidShowError("<br> PopulateddlEVUserAccountExpirationWindow Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            ddlEVUserAccountExpirationWindow.SelectedIndex = 7;
        }

        protected void voidPopulateddlMaxVehicles()
        {
            ddlMaxVehicles.Items.AddRange(Enumerable.Range(1, 256).Select(e => new ListItem(e.ToString())).ToArray());
        }
        #endregion


        #region btnClick and btnClick Related functions

        protected void btnValidateEmails_Click(object sender, EventArgs e)
        {
            // Hide Errors
            voidHideCatchError();
            
            // ListOfEmailsToValidate contains a list of Emails to validate.
            List<string> ListOfEmailsToValidate = listReturnListFromCSVEmails(tbEmailAddress.Text);
            
            int EmailCount = ListOfEmailsToValidate.Count;

            // Setup SQL parameters
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            
            SqlDataReader readerProfile = null;
            bool blnPasses = true;

            bool blnEmailExists = false;

            for (int i = 0; i < EmailCount; i++)
            {
                DataTable DT = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string strQuery1 = " SELECT * " +
                            " FROM [NewUserAuthorization] " +
                            " WHERE [EmailAddress] ='" + ListOfEmailsToValidate[i].Replace("'", "''") + "'";

                    using (SqlCommand cmd1 = new SqlCommand(strQuery1, conn))
                    {
                        using (SqlDataAdapter AD = new SqlDataAdapter(cmd1))
                        {
                            try
                            {
                                AD.Fill(DT);
                            }
                            catch (Exception ex)
                            {
                                voidShowError("Error at btnValidateEmails_Click0: " +ex.Message);
                                return;
                            }
                            if(DT.Rows.Count > 0)
                            {
                                blnEmailExists = true;
                            }
                            else
                            {
                                blnEmailExists = false;
                            }
                        }                        
                    }
                }

                // if Email already exists then just revalidate the email.
                if (blnEmailExists)
                {
                    try
                    {
                        strQuery = " UPDATE [NewUserAuthorization] " +
                                   " SET [Valid] ='1', [Organization] = @Organization, [EVUserAccountTypeID] = @EVUserAccountType, [EVUserAccountExpirationWindowID] = @EVUserAccountExpirationWindow, [MaximumVehicles] = @MaximumVehicles" +
                                   " WHERE [EmailAddress] = @EmailAddress";

                        cmd = new SqlCommand(strQuery, cnn);
                        cnn.Open();

                        SqlParameter ParamOrganization = new SqlParameter();
                        ParamOrganization.ParameterName = "@Organization";
                        ParamOrganization.Value = ddlOrganization.SelectedValue;
                        cmd.Parameters.Add(ParamOrganization);

                        SqlParameter ParamEVUserAccountType = new SqlParameter();
                        ParamEVUserAccountType.ParameterName = "@EVUserAccountType";
                        ParamEVUserAccountType.Value = ddlEVUserAccountType.SelectedValue;
                        cmd.Parameters.Add(ParamEVUserAccountType);

                        SqlParameter ParamEVUserAccountExpirationWindow = new SqlParameter();
                        ParamEVUserAccountExpirationWindow.ParameterName = "@EVUserAccountExpirationWindow";
                        ParamEVUserAccountExpirationWindow.Value = ddlEVUserAccountExpirationWindow.SelectedValue;
                        cmd.Parameters.Add(ParamEVUserAccountExpirationWindow);

                        SqlParameter ParamEmailAddress = new SqlParameter();
                        ParamEmailAddress.ParameterName = "@EmailAddress";
                        ParamEmailAddress.Value = ListOfEmailsToValidate[i];
                        cmd.Parameters.Add(ParamEmailAddress);

                        cmd.Parameters.Add(new SqlParameter("@MaximumVehicles", ddlMaxVehicles.SelectedValue));
                        readerProfile = cmd.ExecuteReader();
                    }
                    catch (Exception ex1)
                    {
                        voidShowError("Error at btnValidateEmails_Click2: " + ex1.Message);
                        blnPasses = false;
                    }
                    finally
                    {
                        if (readerProfile != null)
                        {
                            readerProfile.Close();
                        }
                        if (cnn != null)
                        {
                            cnn.Close();
                        }
                    }
                }
                else // if(!blnEmailExists), create a new record of the Email
                {
                    try
                    {
                        strQuery = " INSERT INTO [NewUserAuthorization](EmailAddress, Organization, Valid, EVUserAccountTypeID, EVUserAccountExpirationWindowID, MaximumVehicles) " +
                                   " VALUES(@EmailAddress, @Organization, @Valid, @EVUserAccountType, @EVUserAccountExpirationWindow, @MaximumVehicles)";
                        
                        cmd = new SqlCommand(strQuery, cnn);
                        cnn.Open();

                        SqlParameter ParamEmail = new SqlParameter();
                        ParamEmail.ParameterName = "@EmailAddress";
                        ParamEmail.Value = ListOfEmailsToValidate[i];
                        cmd.Parameters.Add(ParamEmail);

                        SqlParameter ParamOrganization = new SqlParameter();
                        ParamOrganization.ParameterName = "@Organization";
                        ParamOrganization.Value = ddlOrganization.SelectedValue;
                        cmd.Parameters.Add(ParamOrganization);

                        SqlParameter ParamValid = new SqlParameter();
                        ParamValid.ParameterName = "@Valid";
                        ParamValid.Value = true;
                        cmd.Parameters.Add(ParamValid);

                        SqlParameter ParamEVUserAccountExpirationWindow = new SqlParameter();
                        ParamEVUserAccountExpirationWindow.ParameterName = "@EVUserAccountExpirationWindow";
                        ParamEVUserAccountExpirationWindow.Value = ddlEVUserAccountExpirationWindow.SelectedValue;
                        cmd.Parameters.Add(ParamEVUserAccountExpirationWindow);

                        SqlParameter ParamEVUserAccountType = new SqlParameter();
                        ParamEVUserAccountType.ParameterName = "@EVUserAccountType";
                        ParamEVUserAccountType.Value = ddlEVUserAccountType.SelectedValue;
                        cmd.Parameters.Add(ParamEVUserAccountType);

                        cmd.Parameters.Add(new SqlParameter("@MaximumVehicles", ddlMaxVehicles.SelectedValue));
                        readerProfile = cmd.ExecuteReader();                                   
                    }
                    catch (Exception ex2)
                    {
                        voidShowError("Error at btnValidateEmails_Click3: " + ex2.Message);
                        blnPasses = false;
                    }
                    finally
                    {
                        if (readerProfile != null)
                        {
                            readerProfile.Close();
                        }
                        if (cnn != null)
                        {
                            cnn.Close();
                        }
                    }
                }
            }

            // Clear tb and ddl
            voidClearTBandDDL();

            // If no errors,
            if (blnPasses)
            {
                string strEmailBodyLocation = WebConfigurationManager.AppSettings["EmailValidationEmailBodyLocation"].ToString();
                string strEmailBodyText = string.Empty;
                string line = string.Empty;

                System.IO.StreamReader file = new System.IO.StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strEmailBodyLocation));

                while ((line = file.ReadLine()) != null)
                {
                    // Read each line, and put a space after each line to preserve spacing.
                    strEmailBodyText += line + "<br>";
                }

                
                file.Close();            
                
                // Send Mail to all users confirming their validation
                foreach (string SendToEmail in ListOfEmailsToValidate)
                {
                    SendMail(strSendFromOrganization, SendToEmail, strEmailBodyText, "UCLA SMERC EV Email Authorization");
                }

                PopUpMessage("Emails Validated and email confirmation sent.");
            }
            else
            {
                PopUpMessage("Error while validating.");
            }
        }


        protected List<string> listReturnListFromCSVEmails(string strCSVEmails)
        {
            
            // Take the textbox of CSV (Comma separated..) emails and convert them to a list.
            return (from e in strCSVEmails.Replace(" ", "").Split(',')
                    select (string)Convert.ChangeType(e, typeof(string))).ToList();
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            voidHideCatchError();
        }

        protected void voidHideCatchError()
        {
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void voidShowError(string strMessage)
        {
            lblCatchError.Text += "<br> " + strMessage;
            btnHideCatchError.Visible = true;
        }

        #endregion
        #region HelperFunctions

        protected void voidClearTBandDDL()
        {
            tbEmailAddress.Text = string.Empty;
            ddlOrganization.SelectedIndex = 0;
            ddlEVUserAccountType.SelectedIndex = 0;
            ddlEVUserAccountExpirationWindow.SelectedIndex = 7;
            ddlMaxVehicles.SelectedIndex = 0;
        }

        protected void PopUpMessage(string strMessage)
        {
            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.Append("<script type = 'text/javascript'>");
            sb2.Append("window.onload=function(){");
            sb2.Append("alert('");
            sb2.Append(strMessage);
            sb2.Append("')};");
            sb2.Append("</script>");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", sb2.ToString());
        }

        protected string ReturnUserGUIDfromUsername(string UserName)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [UserID] FROM [aspnet_Users] WHERE [UserName] ='" + UserName + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                voidShowError("Return UserGUID ERROR:" + ex.Message);
                if (cnn != null)
                {
                    cnn.Close();
                }
                return null;
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.Close();
                }
            }
            return dt.Rows[0][0].ToString();
        }

        protected string ObtainCityGUIDfromUserCity(string UserCity)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            cnn.Open();
            try
            {
                strQuery = "SELECT Id FROM [City] WHERE [Name] = '" + UserCity + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();

                string userCityGUID = dt.Rows[0][0].ToString();
                cnn.Close();
                return userCityGUID;
            }
            catch
            {
                List<ComboOrgAndGuidClass> ComboCityAndGuid = ReturnUniqueCombinedGUID();
                for (int i = 0; i < ComboCityAndGuid.Count; i++)
                {
                    if (UserCity == ComboCityAndGuid[i].ComboOrgString)
                    {
                        cnn.Close();
                        return ComboCityAndGuid[i].Guid;
                    }
                }
                cnn.Close();
                return null;
            }
        }


        protected List<ComboOrgAndGuidClass> ReturnUniqueCombinedGUID()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
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
        #endregion
        #region Classes

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

        #endregion

        


    }
}