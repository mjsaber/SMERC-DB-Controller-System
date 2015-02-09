using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;



namespace RTMC
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.User.Identity.IsAuthenticated)
            {
                string[] roles = Roles.GetRolesForUser(Page.User.Identity.Name);

                List<string> ListOfRoles = new List<string>();
                bool blnPassValidation = false;
                for (int i = 0; i < roles.Count(); i++) // Check all User's administrative powers.  If pass, then allow.
                {
                    if (roles[i].IndexOf("Administrator") != -1 || roles[i].IndexOf("Maintainer") != -1 || roles[i].IndexOf("Operator") != -1)
                        blnPassValidation = true;
                }

                if (!blnPassValidation) // If the user is not an administrator/Maintainer/Operator, then...
                {
                    HttpCookie Cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
                    Cookie.Expires = DateTime.Now.AddDays(-1); // Expire the authentication Ticket
                    Page.Response.Cache.SetCacheability(HttpCacheability.NoCache); // Destroy cachce - delete previous session/screens
                    Page.Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                    Page.Response.Cache.SetNoStore();
                    Page.Response.Cache.SetAllowResponseInBrowserHistory(false);

                    Response.Redirect("~/Account/Login.aspx"); // Redirect to Log In Page.
                    FormsAuthentication.SignOut(); // Sign out user
                    Session.Abandon(); // Destroy all objects stored in Session object and release resources. 
                    Session.Clear();
                    
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx"); // Redirect to Log In Page.
            }
        }
    }
}
