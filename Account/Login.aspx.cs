using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;


namespace RTMC.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void Login1_LoggedIn(object sender, EventArgs e) // This code allows any Administrator, Maintainer, or Operator to 
        {
            string[] roles = Roles.GetRolesForUser(Login1.UserName);

            List<string> ListOfRoles = new List<string>();
            bool blnPassValidation = false;
            for (int i = 0; i < roles.Count(); i++) // Check all User's administrative powers.  If pass, then allow.
            {
                if (roles[i].IndexOf("Administrator") != -1 || roles[i].IndexOf("Maintainer") != -1 || roles[i].IndexOf("Operator") != -1)
                    blnPassValidation = true;
            }

            if (!blnPassValidation) // If the user is not an administrator/Maintainer/Operator, then...
            {
                FormsAuthentication.SignOut(); // Sign out user
                Session.Abandon(); // Destroy all objects stored in Session object and release resources. 
                Session.Contents.Clear();
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();  


                Response.Redirect("~/Account/Login.aspx"); // Redirect to Log In Page.

            }
        }

    }
}