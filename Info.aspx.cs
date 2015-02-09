using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EVUser
{
    public partial class Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string strErrMsg = Request.QueryString["ErrMsg"];
            switch (strErrMsg)
            {
                case "Privilege":
                    lblErrMsg.Text = "Your account's privilege doesn't allow you to access the function";
                    break;
            }

        }
    }
}