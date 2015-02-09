using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using RTMC;

namespace EVEditor
{
    public partial class EditFleet : System.Web.UI.Page
    {
        string connectionString =   WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };

        string[] strArrRolesToAllow = { "General Administrator", "Santa Monica Administrator" };
        //  string[] strArrRolesToAllow = { };
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        string[] strArrTypesToAllow = { "Administrator" };

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        string[] ColumnsToHide = { "UserID", "EVModileID", "License No.", "Principal Driver Name" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                RolePrincipal rp = (RolePrincipal)User;
                string[] roles = Roles.GetRolesForUser();
                List<string> ListOfRoles = new List<string>();
                for (int i = 0; i < roles.Count(); i++)
                {
                    ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
                }

                if (!blnFindAssociatedRoles(ListOfRoles)) // only continue if the user is a city administrator
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
                populateOrganization();
                populateUserName(ddlOrganization.SelectedValue);
                populateEvInfo();
                populateFleet(ddlOrganization.SelectedValue);
            }
        }

        protected void populateOrganization()
        {

             // Check if the User's role contains the master role
            // If it does NOT contain
            bool blnListhasMasterRole = false;
            foreach (string strRole in listApprovedRoles)
            {
                if (strArrMasterOrgs.Contains(strRole))
                {
                    blnListhasMasterRole = true;
                    break;
                }
            }

            // List to hold a set of city GUIDs based on the user's roles.
            List<string> listCityGUID = new List<string>();

            // If the users role does not have a master's role,
            // then only populate the user's specific cities.
            if (!blnListhasMasterRole)
            {
                foreach (string str in listApprovedRoles)
                {
                    // listCityGUID now holds the Organization GUIDS to all of the user's roles
                    
                    listCityGUID.Add(ReturnGuidfromCityname(str));
                }
            }

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strListItemInsert="";
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
                        int listCount = listCityGUID.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            strQuery += " ID = '" + listCityGUID[i] + "' ";
                            if (i < listCount - 1)
                            {
                                strQuery += " OR ";
                            }
                        }
        
                    }
                    // Order the data by the name
                    strQuery += " ORDER BY Name";

                    
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    DataTable DT = new DataTable();
                    SqlDataAdapter DA = new SqlDataAdapter();
                    DA.SelectCommand = cmd;
                    DA.Fill(DT);
                    ddlOrganization.DataSource = DT;
                    ddlOrganization.DataValueField = "ID";
                    ddlOrganization.DataTextField = "Name";
                    ddlOrganization.DataBind();
                    if (strListItemInsert != "")
                    {
                        ListItem l1 = new ListItem(strListItemInsert, "-1");
                        ddlOrganization.Items.Insert(0, l1);
                    }
                    DA.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at populateOrg: " + ex.Message);
                    return;
                }
                

            }           
      }

        protected void populateUserName(string strOrgId)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                if (strOrgId == "-1")
                {
                    strOrgId = string.Empty;
                }
                try
                {
                    string strQuery = "SELECT u.UserId, UserName FROM aspnet_Users AS u INNER JOIN [aspnet_Profile] AS p ON u.[UserId] = p.[UserId] ";

                    if (strOrgId != string.Empty)
                    {
                        strQuery += "WHERE p.[RoleCityID] = '" + strOrgId + "'";
                    }
                    //strQuery += " ORDER BY UserName";
                    string strListItemInsert = "Select...";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    DataTable DT = new DataTable();
                    SqlDataAdapter DA = new SqlDataAdapter();
                    DA.SelectCommand = cmd;
                    DA.Fill(DT);
                    if (DT.Rows.Count != 0)
                    {
                        ddlUserName.DataSource = DT;
                        ddlUserName.DataValueField = "UserId";
                        ddlUserName.DataTextField = "UserName";
                        ddlUserName.DataBind();
                        ListItem l1 = new ListItem(strListItemInsert, "-1");
                        ddlUserName.Items.Insert(0, l1);
                        ddlUserName.SelectedIndex = 0;

                    }
                    else
                    {
                        ddlUserName.Items.Clear();
                        ListItem l1 = new ListItem(strListItemInsert, "-1");
                        ddlUserName.Items.Insert(0, l1);
                    }
                    DA.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at populateUserName: " + ex.Message);
                    return;
                }
            }
        }

        protected void populateEvInfo()
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "SELECT ID, Manufacturer+' '+Model AS [EV Info] FROM [EV Model] ORDER BY [EV Info]";
                    string strListItemInsert = "Select...";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    DataTable DT = new DataTable();
                    SqlDataAdapter DA = new SqlDataAdapter();
                    DA.SelectCommand = cmd;
                    DA.Fill(DT);
                    ddlEvInfo.DataSource = DT;
                    ddlEvInfo.DataValueField = "ID";
                    ddlEvInfo.DataTextField = "EV Info";
                    ddlEvInfo.DataBind();
                    ListItem l1 = new ListItem(strListItemInsert, "-1");
                    ddlEvInfo.Items.Insert(0, l1);
                    DA.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at populateEvInfo: " + ex.Message);
                    return;
                }
            }
        }

        protected void populateFleet(string strOrgId)
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to VehicleID selected
            {
                if (strOrgId == "-1")
                {
                    strOrgId = string.Empty;
                }

                string sqlQuery = "SELECT fv.[VehicleID], u.[UserName], u.UserId, ev.ID, ev.[Manufacturer] + ' ' + ev.[Model] AS [EV Info], fv.LicenseNo, fv.PrincipalDriverName " +
                                    "FROM [FleetVehicles] AS fv "+
                                    "INNER JOIN [aspnet_Users] AS u ON fv.[UserID] = u.[UserId] "+
                                     "INNER JOIN [EV Model] AS ev ON fv.EVModelID = ev.[ID] " +
                                     "INNER JOIN [aspnet_Profile] AS p ON p.[UserId] = u.[UserId] "+
                                     "WHERE fv.Activate = 1 ";

                if (strOrgId != string.Empty)
                {
                    sqlQuery += "AND p.[RoleCityID] = '" + strOrgId + "'";
                }

                sqlQuery += "ORDER BY fv.VehicleID DESC";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        try
                        {
                            AD.Fill(DT);
                        }

                        catch (Exception ex)
                        {
                            ShowError("Error at populategvFleet: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopUpMessage for clarity to note there are no Gateways for given selection.
                        {
                            ShowError("No data in this selection");                            
                        }
                    }
                }
                
                Session["data"] = DT;
                gvFleet.DataSource = Session["data"];
                gvFleet.DataBind();

            }
        }

        protected void gvFleetRowCreated(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < ColumnsToHide.Count(); i++)
            {
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    e.Row.Cells[findGVcolumn(ColumnsToHide[i])].Visible = false;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Cells[findGVcolumn(ColumnsToHide[i])].Visible = false;
                }
            }
        }

        protected void gvFleetSorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvFleet.DataSource = dataTable.DefaultView;
                gvFleet.DataBind();
            }
            gvFleet.SelectedIndex = -1;
            clearAll();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvFleet.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvFleet.Columns.IndexOf(field);
                }
            }
            Image sortImage2 = new Image();
            if (getSortDirectionString1("Ascending") == "ASC")
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
            gvFleet.HeaderRow.Cells[index].Controls.Add(sortImage2);
        }

        private string getSortDirectionString(string column)
        {
            // By default, set the sort direction to ascending.
            string sortDirection = "ASC";

            // Retrieve the last column that was sorted.
            string sortExpression = ViewState["SortExpression"] as string;

            if (sortExpression != null)
            {
                // Check if the same column is being sorted.
                // Otherwise, the default value can be returned.
                if (sortExpression == column)
                {
                    string lastDirection = ViewState["SortDirection"] as string;
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


        private string getSortDirectionString1(string column)
        {
            // By default, set the sort direction to ascending.
            string sortDirection = "ASC";

            // Retrieve the last column that was sorted.
            string sortExpression = ViewState["SortExpression1"] as string;

            if (sortExpression != null)
            {
                // Check if the same column is being sorted.
                // Otherwise, the default value can be returned.
                if (sortExpression == column)
                {
                    string lastDirection = ViewState["SortDirection1"] as string;
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

        protected bool blnFindAssociatedRoles(List<string> ListOfRoles)
        {
            bool blnHasRole = false;

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
                                    listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));
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
                    for (int j = 0; j < strArrTypesToAllow.Count(); j++) // At most 4 iterations
                    {
                        if (ListOfRoles[i].IndexOf(strArrTypesToAllow[j]) != -1) // current index contains allowed Type
                        {
                            // Substring(0, (total length) - (length of the allowed type)
                            // i.e. "General Administrator", take the substring to obtain only the "General" string.
                            // the + 1 to the length is to account for the " " , space.
                            listApprovedRoles.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - (strArrTypesToAllow[j].Length + 1)));
                            blnHasRole = true;
                        }
                    }
                }
            }
            return blnHasRole;
        }

        protected string ReturnGuidfromCityname(string UserCity)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            string userCityGUID = string.Empty;
            try
            {
                strQuery = "SELECT ID FROM [City] WHERE [Name] = '" + UserCity + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
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
                if (cnn != null)
                {
                    cnn.Close();
                }
            }
            return userCityGUID;
        }

        protected void clearAll()
        {
            tbVehicleId.Text = string.Empty;
            ddlUserName.SelectedIndex = 0;
            ddlEvInfo.SelectedIndex = 0;
            gvFleet.SelectedIndex = -1;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
            tbLicenseNo.Text = string.Empty;
            tbDriverName.Text = string.Empty;
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
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
            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.Append("<script type = 'text/javascript'>");
            sb2.Append("window.onload=function(){");
            sb2.Append("alert('");
            sb2.Append(message2);
            sb2.Append("')};");
            sb2.Append("</script>");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", sb2.ToString());
        }

        protected void gvFleetPaging(object sender, GridViewPageEventArgs e)
        {

            gvFleet.SelectedIndex = -1;

            //ClearAllTbs();
            //ClearAllErrorLbl();
            //ClearImage();
            DataTable dataTable = Session["data"] as DataTable;

            gvFleet.PageIndex = e.NewPageIndex;
            gvFleet.DataSource = dataTable;
            gvFleet.DataBind();
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvFleet.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvFleet.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void fillInfo()
        {
            GridViewRow gvRow;
            gvRow = gvFleet.Rows[gvFleet.SelectedIndex];


            tbVehicleId.Text = gvRow.Cells[findGVcolumn("Vehicle ID")].Text;

            ddlUserName.SelectedValue = gvRow.Cells[findGVcolumn("UserID")].Text;
            ddlEvInfo.SelectedValue = gvRow.Cells[findGVcolumn("EVModileID")].Text;
            tbLicenseNo.Text = gvRow.Cells[findGVcolumn("License No.")].Text.Replace("&nbsp;", " "); ;
            tbDriverName.Text = gvRow.Cells[findGVcolumn("Principal Driver Name")].Text.Replace("&nbsp;", " "); ;

        }

        protected void gvFleetSelectedIndex(object sender, EventArgs e)
        {
    
            fillInfo();
            //HideError();
            
            btnUpdate.Visible = true;
            btnDelete.Visible = true;
            //showcbClearImage();
            //cbClearImage.Checked = false;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "UPDATE [FleetVehicles] SET [UserID] = @UserID, [EVModileID] = @EVModileID, LicenseNo = @LicenseNo, PrincipalDriverName = @PrincipalDriverName WHERE [VehicleID] = @VehicleID ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    //GridViewRow gvRow = gvFleet.Rows[gvFleet.SelectedIndex];

                    cnn.Open();

                    SqlParameter ParamFleetUserId = new SqlParameter();
                    ParamFleetUserId.ParameterName = "@UserID";
                    ParamFleetUserId.Value = new Guid(ddlUserName.SelectedItem.Value);
                    cmd.Parameters.Add(ParamFleetUserId);
                    SqlParameter ParamFleetEvId = new SqlParameter();
                    ParamFleetEvId.ParameterName = "@EVModileID";
                    ParamFleetEvId.Value = new Guid(ddlEvInfo.SelectedItem.Value);//ddlEvInfo.SelectedValue;
                    cmd.Parameters.Add(ParamFleetEvId);

                    SqlParameter ParamFleetVid = new SqlParameter();
                    ParamFleetVid.ParameterName = "@VehicleID";
                    ParamFleetVid.Value = tbVehicleId.Text;
                    cmd.Parameters.Add(ParamFleetVid);

                    cmd.Parameters.Add(new SqlParameter("@LicenseNo", tbLicenseNo.Text));
                    cmd.Parameters.Add(new SqlParameter("@PrincipalDriverName", tbDriverName.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnUpdate_Click: " + ex.Message);
                    return;
                }
                
            }
            populateFleet(ddlOrganization.SelectedValue);
            PopUpMessage("Information Updated");
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "INSERT INTO [FleetVehicles] (VehicleID, UserID, EVModileID, LicenseNo, PrincipalDriverName) VALUES(@VehicleID, @UserID, @EVModileID, @LicenseNo, @PrincipalDriverName) ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    SqlParameter ParamFleetUserId = new SqlParameter();
                    ParamFleetUserId.ParameterName = "@UserID";
                    ParamFleetUserId.Value = new Guid(ddlUserName.SelectedItem.Value);
                    cmd.Parameters.Add(ParamFleetUserId);
                    SqlParameter ParamFleetEvId = new SqlParameter();
                    ParamFleetEvId.ParameterName = "@EVModileID";
                    ParamFleetEvId.Value = new Guid(ddlEvInfo.SelectedItem.Value);//ddlEvInfo.SelectedValue;
                    cmd.Parameters.Add(ParamFleetEvId);

                    SqlParameter ParamFleetVid = new SqlParameter();
                    ParamFleetVid.ParameterName = "@VehicleID";
                    ParamFleetVid.Value = tbVehicleId.Text;
                    cmd.Parameters.Add(ParamFleetVid);

                    cmd.Parameters.Add(new SqlParameter("@LicenseNo", tbLicenseNo.Text));
                    cmd.Parameters.Add(new SqlParameter("@PrincipalDriverName", tbDriverName.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at NewBottonClick: " + ex.Message);
                    return;
                }
          
                
            }
            gvFleet.SelectedIndex = -1;
            populateFleet(ddlOrganization.SelectedValue);
            PopUpMessage("Information Added");

        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            gvFleet.SelectedIndex = -1;
            clearAll();
            btnUpdate.Visible = false;
            btnDelete.Visible = false;

        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "UPDATE [FleetVehicles] SET [Activate] = @Activate WHERE [VehicleID] = @VehicleID ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    //GridViewRow gvRow = gvFleet.Rows[gvFleet.SelectedIndex];

                    cnn.Open();

                    SqlParameter ParamFleetActivate = new SqlParameter();
                    ParamFleetActivate.ParameterName = "@Activate";
                    ParamFleetActivate.Value = "0";
                    cmd.Parameters.Add(ParamFleetActivate);

                    SqlParameter ParamFleetVid = new SqlParameter();
                    ParamFleetVid.ParameterName = "@VehicleID";
                    ParamFleetVid.Value = tbVehicleId.Text;
                    cmd.Parameters.Add(ParamFleetVid);

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnDelete_Click: " + ex.Message);
                    return;
                }

            }
            clearAll();
            populateFleet(ddlOrganization.SelectedValue);
            PopUpMessage("Information Deleted");
        }

        protected void ddlOrganization_SelectedIndexChanged(object sender, EventArgs e)
        {

            // Reset the gridview selection
            gvFleet.SelectedIndex = -1;
            clearAll();
            // Hide the btnUpdate
            btnUpdate.Visible = false;
            btnDelete.Visible = false;

            // Repopulate the gridview with the new settings.
            populateFleet(ddlOrganization.SelectedValue);
            populateUserName(ddlOrganization.SelectedValue);
        }

    }
}