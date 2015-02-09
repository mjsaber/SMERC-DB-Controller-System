using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Text; // for StringBuilder
using System.Web.UI.HtmlControls; // for HTMLcontent usageu
//using System.Web.Mail;
using System.Net;
using System.Security.Cryptography;
using System.IO;

using System.Net.Mail;

namespace RTMC.Account
{    
    public partial class Retrieve : System.Web.UI.Page
    {
        private string strResetPassword = "Please check your email for username and password information.We recommend to reset the password after login at";
        private string strUserAccountInfo;
        protected void Page_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        protected void Initialize()
        {
            lblMessageText.Text = string.Empty;
            strUserAccountInfo = string.Empty;
            //bool val1 = System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            //if(val1)
            //    ChangePassword1.Visible = true;
        }

        protected void PasswordRecovery2_SendingMail(object sender, MailMessageEventArgs e)
        {
        }

        protected void retrieveEmailCredential(string strOrganization, out string strEmailAddress, out string strPassword, out bool blnSSL, out string strSmtpport, out string strSmtpHost)
        {
            string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            DataTable dt = new DataTable();
            string strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strOrganization + "'";
            SqlDataAdapter sdr = new SqlDataAdapter(strQuery, strCnn);
            sdr.Fill(dt);
            strEmailAddress = dt.Rows[0][4].ToString().Trim();
            strPassword = dt.Rows[0][0].ToString().Trim();
            strSmtpHost = dt.Rows[0][1].ToString().Trim();
            blnSSL = bool.Parse(dt.Rows[0][2].ToString().Trim());
            strSmtpport = Server.HtmlDecode(dt.Rows[0][3].ToString().Trim());
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

        protected void btnEmailMe_Click(object sender, EventArgs e)
        {
            lblMessageText.Text = string.Empty;
            strUserAccountInfo = string.Empty;

            if (!String.IsNullOrEmpty(tbNameOrEmail.Text.ToString())) // Check that text is entered
            {
                string checkString = tbNameOrEmail.Text;
                List<passwordClass> Passwords = new List<passwordClass>();
                List<string> AllUsersCollection_ = new List<string>();
                List<usernameClass> UsernameCol = new List<usernameClass>();

                if (checkString.IndexOf("@") != -1) // if an @ sign is found, the input is probably an email address
                {
                    bool isThereHashed = false;
                    List<int> userIter = new List<int>(); // userIter 
                    int iter = 0;
                    MembershipUserCollection muc = Membership.FindUsersByEmail(tbNameOrEmail.Text);
                    if (muc.Count == 0)
                    {
                        lblMessageText.Text = "The email, " + tbNameOrEmail.Text + ", doesn't exist in our system";
                        return;
                    }

                    foreach (MembershipUser ee in muc)
                    {
                        AllUsersCollection_.Add(ee.UserName);
                    }

                    string[] AllUsers_ = AllUsersCollection_.ToArray();
                    foreach (string item in AllUsersCollection_)
                    {
                        try
                        {
                            Passwords.Add(new passwordClass(Membership.Provider.GetPassword(item, null)));
                            strUserAccountInfo += "Username:" + item + "   Password:" + Membership.Provider.GetPassword(item, null) + " \n";
                        }
                        catch
                        {
                            isThereHashed = true;
                            userIter.Add(iter);
                            string RandomPassword = Membership.GeneratePassword(10, 0);
                            Passwords.Add(new passwordClass(RandomPassword));
                            try
                            {
                                muc[item].ChangePassword(muc[item].ResetPassword(), RandomPassword);
                                strUserAccountInfo += "Username:" + item + "   Password:" + RandomPassword + " \n";
                            }
                            catch(Exception ex)
                            {
                                string strErr = ex.Message;
                                strUserAccountInfo = "Your account," + item + ", is locked by our system";
                                strResetPassword = "\nPlease contact our system operator to reactive it";
                            }
                        }
                        
                        finally
                        {
                            iter++;
                        }
                       
                    }
                    if (isThereHashed)
                    {
                        //lblMessageText.Text = "One or more of your accounts used a hashed password format and thus the password had to be reset.<br>";
                        lblMessageText.Text = "<table>";
                        foreach (int item in userIter)
                        {
                            lblMessageText.Text += "<tr><td>The password for account, <b>" + AllUsersCollection_[item] + "</b>, was reset.</td></tr>";
                        }
                        lblMessageText.Text += "<tr><td>Please check your email for the username and password. We recommend to reset the password after login</td></tr></table>";    
                    }
                    
                    lblEmail.Text = tbNameOrEmail.Text; 
                    SendMail();
                }
                else // if the textbox is not an email, then assume as an username input
                {
                    string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                    SqlConnection cnn = new SqlConnection(connectionString);
                    string strQuery;
                    SqlCommand cmd;
                    DataTable dt = null;
                    SqlDataAdapter da;
                    string email = string.Empty;
                    string UserID = string.Empty;
                    try
                    {
                        cnn.Open(); // Open the Connection
                        strQuery = "SELECT [UserId] FROM [EVDemo].[dbo].[aspnet_Users] WHERE [UserName]='" + tbNameOrEmail.Text + "'";
                        cmd = new SqlCommand(strQuery, cnn);
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);
                        da.Dispose();
                        cmd.Dispose();

                        if (dt.Rows.Count != 1)
                        {
                            lblMessageText.Text = "The username, " + tbNameOrEmail.Text + ", doesn't exist in our system";
                            return;
                        }

                        UserID = dt.Rows[0][0].ToString();

                        strQuery = "SELECT [Email] FROM [EVDemo].[dbo].[aspnet_Membership] WHERE [UserId]='" + UserID + "'";
                        cmd = new SqlCommand(strQuery, cnn);
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);
                        da.Dispose();
                        cmd.Dispose();

                        email = dt.Rows[0][0].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        lblEmail.Text = email;
                        cnn.Close();
                    }
                    
                    UsernameCol.Add(new usernameClass(tbNameOrEmail.Text));
                    string RandomPassword = string.Empty;

                    MembershipUserCollection muc = Membership.FindUsersByName(tbNameOrEmail.Text); 
                    try
                    {                        
                        Passwords.Add(new passwordClass(Membership.Provider.GetPassword(tbNameOrEmail.Text, null)));
                        strUserAccountInfo = "Username:" + tbNameOrEmail.Text + "  Password:" + Membership.Provider.GetPassword(tbNameOrEmail.Text, null) + "";                       
                    }
                    catch (Exception)
                    {
                        lblMessageText.Text = "Please check your email for the username and password. We recommend to reset the password after login";
                        RandomPassword = Membership.GeneratePassword(10, 0);

                        Passwords.Add(new passwordClass(RandomPassword));
                        strUserAccountInfo = "Username:" + tbNameOrEmail.Text + "  Password:" + RandomPassword + "";
                        try
                        {
                            muc[tbNameOrEmail.Text].ChangePassword(muc[tbNameOrEmail.Text].ResetPassword(), RandomPassword);
                        }
                        catch (Exception ex)
                        {
                            lblMessageText.Text += ex.Message;
                        }
                    }
                    finally
                    {
                        SendMail();
                    }
                }
                //lblMessageText.Text += "<br><br> Please check your email for username and password information";
            }
            else
            {
                lblMessageText.Text = "Please enter a email or username";
            }
        }

        protected void tbNameOrEmail_TextChanged(object sender, EventArgs e)
        {
        }

        protected void UserGrid_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void SendMail()
        {
            //// Gmail Address from where you send the mail
            //var fromAddress = "teste8562@gmail.com";
            //// any address where the email will be sending
            //var toAddress = lblEmail.Text;
            ////Password of your gmail address
            //const string fromPassword = "progamer";
            //// Passing the values and make a email formate to display
            //string subject = "Your Usernames";
            //string body = string.Empty;

            //body = "From: SMERC Server  \n";
            //body += strUserAccountInfo + "\n ";
            //body += strResetPassword;


            //// smtp settings
            //var smtp = new System.Net.Mail.SmtpClient();
            //{
            //    smtp.Host = "smtp.gmail.com";
            //    smtp.Port = 587;
            //    smtp.EnableSsl = true;
            //    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            //    smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
            //    smtp.Timeout = 20000;
            //}

            //// Passing values to smtp object
            //try
            //{
            //    smtp.Send(fromAddress, toAddress, subject, body);
            //}
            //catch(Exception ex)
            //{
            //}


            //Send email to the user via SMERC email server
            string strCredentialAddress, strCredentialPassword;
            bool blnSSL;
            string strSmtpport, strSmtpHost;
            retrieveEmailCredential("General", out strCredentialAddress, out strCredentialPassword, out blnSSL, out strSmtpport, out strSmtpHost);
            strCredentialPassword = decodePassword(strCredentialPassword);
            MailMessage email = null;
            SmtpClient sc = null;
            try
            {
                //smartgrid account
                email = new MailMessage();
                email.From = new MailAddress(strCredentialAddress, "EV Station System, SMERC UCLA");
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
                lblEmail.Text = "bingyu.dsfish.li@gmail.com";
                email.To.Add(new MailAddress(lblEmail.Text));
                email.Subject = "Your new password at ";
                email.IsBodyHtml = true;
                email.Body = "<table>";
                email.Body += "<tr><td>" + strUserAccountInfo + "</td></tr>";
                email.Body += "<tr><td>" + strResetPassword + "</td></tr>";
                email.Body += "<tr><td>http://wireless3.seas.ucla.edu/RTMC</td></tr>";

                email.Body += "</table>";
                sc.Send(email);
            }
            catch (Exception ex)
            {
                lblMessageText.Text = "Our email server doesn't work temporarily. Please try it again later";
            }
            finally
            {
                if (email != null)
                    email.Dispose();
                if (sc != null)
                    sc.Dispose();
            }

        }
    }


    /// <summary>
    /// Classes needed to make code easier for storing username and passwords within this retrieve.cs functions
    /// </summary>
    public class Client
    {

        public Client(string Username, string Password)
        {
            _Username = Username;
            _Password = Password;
        }

        private string _Password;

        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        private string _Username;

        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }
    }

    public class passwordClass
    {

        public passwordClass(string Password)
        {
            _Password = Password;
        }

        private string _Password;

        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }
    }

    public class usernameClass
    {

        public usernameClass(string Username)
        {
            _Username = Username;
        }

        private string _Username;

        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }
    }
}