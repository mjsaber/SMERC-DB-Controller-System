using System; 
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Text; 
using System.Web.UI.HtmlControls; 
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.IO;

using RTMC;
using ListBox = System.Windows.Forms.ListBox;

/* NOTE: When adding new information from database to the User Editor page, you must:
 * 1) Modify ClearAll()
 * 2) Modify FillInTxtBoxExistingUser()
 * 3) Modify UpdateFunction() to account to update new information.     
 * 4) Check for required/Unrequired information.  If required information, then the CheckforNewUserVerification() must be changed accordingly
 * 5) The format for adding a new column in the .aspx file is, <tr><td></td><td></td><td></td></tr>
 * 
 * 11-14-12 - DHK
 */

namespace RTMC
{
    public partial class EVUserEditorHashed : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        string[] ColumnsToHide = {"UserId"};
        readonly string[] EvListColumnsToHide = {"EvModelID", "ID"};
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> ListOfAdminCities = new List<string>();
            bool isGeneralAdmin = true;
            if (User.Identity.IsAuthenticated)
            {

                //tb.Text = username;
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

                isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);

                if (isAdministrator || isOperator) // only continue if the user is a city administrator
                {
                    LockOutFeatures(ListOfAdminCities, isAdministrator);
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
            
            if (!IsPostBack) // Initialize the Data only once per page load (not postback)
            {
                cbShowActivated.Checked = true;
                Initialize(ListOfAdminCities, isGeneralAdmin); // enter in the list of admin cities so that we can readjust the ddlSelectByCity items.
                showAllFunction();                         
            }
        }

        protected void LockOutFeatures(List <string> ListOfAdminCities, bool isAdministrator)
        {
            //if(!CheckBox_SelectByCity.Checked)  meow
            //ddlSelectByCity.Enabled = false;
            var currentUser = Membership.GetUser(User.Identity.Name);
            string username = currentUser.UserName; //** get UserName
            string userGUID = ReturnUserGUIDfromUsername(username);
            if (isAdministrator && ListOfAdminCities.IndexOf("General") != -1) // General is found
            {
                ddlRoleArea.Enabled = true;
                ddlRTMCUserAccountType.Enabled = true;
                ddlEVUserAccountType.Enabled = true;
                ddlRTMCChartAndReport.Enabled = true;
                // do nothing
            }
            else // Not General admin
            {
                //tb.Text = GetRTMCReportIntervals(userGUID);
                ddlRTMCChartAndReport.Enabled = false;
                ddlRTMCChartAndReport.SelectedValue = "1";
                ddlEVUserAccountType.Enabled = false;
                ddlEVUserAccountType.SelectedValue = "0";
                ddlRTMCUserAccountType.Enabled = false;
                ddlRTMCUserAccountType.SelectedValue = "1";
                ddlRTMCReportIntervals.Enabled = false;
                ddlRTMCReportIntervals.SelectedValue = GetRTMCReportIntervals(userGUID);
                if (ListOfAdminCities.Count == 1 && ListOfAdminCities.IndexOf("General") == -1) // if the user only has one associated Administrator role,
                {

                    ddlSelectByCity.Enabled = false;
                    ddlRoleArea.Enabled = false;
                    //ddlRoleArea.SelectedValue = ListOfAdminCities[0];
                    //ddlSelectByCity.SelectedValue = ListOfAdminCities[0];
                    ddlSelectByCity.SelectedValue = ObtainCityGUIDfromUserCity(ListOfAdminCities[0]);
                    ddlRoleArea.SelectedValue = ObtainCityGUIDfromUserCity(ListOfAdminCities[0]);
                    //tbEVUserSessionTimeout.Text = ddlRoleArea.SelectedValue;
                    //PopulateThecblRoleName("UCLA");
                }
                else // if there are more than one admin cities.
                {
                }
            }
        }

        protected bool UserRoleIsGeneralAdmin(List<string> listOfRoles)
        {
            for (int i = 0; i < listOfRoles.Count(); i++)
            {
                if (listOfRoles[i].IndexOf("General Administrator")!=-1) // != -1 means that the string contains "General Administrator"
                    return true;
            }
            return false;
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

        protected List<string> FindAssociatedRoles(List<string> ListOfRoles)
        {
            List<string> ListOfAdminCities = new List<string>();

            for (int i = 0; i < ListOfRoles.Count; i++)
            {
                if (ListOfRoles[i].IndexOf("Administrator") != -1) // current index contains administrator
                {
                    ListOfAdminCities.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - 14)); // 14 is length of "Administrator" string + 1 space key
                }
                if (ListOfRoles[i].IndexOf("Operator") != -1) // current index contains operator
                {
                    ListOfAdminCities.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - 9)); // 14 is length of "Administrator" string + 1 space key
                }
            }
            return ListOfAdminCities;
        }

        protected void Initialize(List<string> ListOfAdminCities, bool isGeneralAdmin)
        {
            AddInGeneralCity(); // If needed.  Code with check if General is already populated. 
            PopulateModels(ListOfAdminCities, isGeneralAdmin);
            ErrorMessage.Text = string.Empty;
            //cbShowActivated.Checked = true;
        }

        #region PopulateFunctions


        protected void PopulateModels(List<string> ListOfAdminCities, bool isGeneralAdmin)
        {
           // string SelectCity = ddlSelectByCity.SelectedValue;
            //string SelectCity = ddlSelectByCity.SelectedItem.Text;
            bool boolSelectedCity = ddlSelectByCity.Enabled;
            ddlMonitorRefreshInterval.SelectedValue = "300";
            HideAllVerificationText();            
            PopulateEVDDL();
            PopulateddlRoleArea(ListOfAdminCities);
            if (ListOfAdminCities.Count == 1)
            {
                PopulateThecblRoleName(ddlSelectByCity.SelectedItem.Text, isGeneralAdmin);
            }
            else
                PopulatecblRoleName(isGeneralAdmin);
           // PopulateDDL_UserCity();
            PopulateddlUserState();
            PopulateEVMODEL();
            PopulateddlIsApproved();
            PopulateddlIsLockedOut();
            PopulateddlIsActivated();
            PopulateddlPriority();
            PopulateddlRTMCReportIntervals();
            // Addition of SmartPhoneOS and PhoneServiceCarrier on 11-14-2012 -dhk
            Populate_ddlSmartPhoneOS();
            Populate_ddlPhoneServiceCarrier();
            PopulateddlEVUserAccountExpirationWindow();
            PopulateddlEVUserAccountType();
            PopulateddlRTMCChartAndReport();
            PopulateddlRTMCUserAccountExpirationWindow();
            PopulateddlRTMCUserAccountType();
            // end 11-14-2012 - dhk
            PopulateddlMaxVehicles();
            ddlSelectByCity.Enabled = boolSelectedCity;
            PopulateGridview(ListOfAdminCities, cbShowActivated.Checked);
        }

        protected void PopulateGridview(List <string> ListOfAdminCities, bool blnActivate)
        {
            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;
            
            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {                
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " + 
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" + 
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] +"')";

                    if (blnActivate)
                        strQuery += " WHERE u.[Activate] ='1'";
                    
                   // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                       strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }

            

            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
         //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        #endregion            
        #region ControlHelperFunctions(selected,sorting,paging,deleted)

        protected void ddlSelectByCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  string SelectedCity = ddlSelectByCity.SelectedValue;
            string SelectedCity = ddlSelectByCity.SelectedItem.Text;
            clearAllText();
            ddlRoleArea.SelectedValue = ddlSelectByCity.SelectedValue; // Orig loc 1.
           // ddlRoleArea.Enabled = false; // disable the User city selection.  Only can choose the city that is selected meow
            
            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);

            if (ddlSelectByCity.SelectedIndex==0) // If user selects "All Users", then uncheck the checkbox            
            {
                    List<string> ListOfAdminCities = new List<string>();
                    ListOfAdminCities = FindAssociatedRoles(ListOfRoles); // ListOfAdminCities is a list of user city that have admin roles  
                    bool isAdministrator = IsUserAdministrator(ListOfRoles);

                    PopulateGridview(ListOfAdminCities, cbShowActivated.Checked);
                    LockOutFeatures(ListOfAdminCities, isAdministrator);
                 
            }
            else
            {
                PopulateGridview(ReturnSelectedCities(), cbShowActivated.Checked);// Re-initialize with only selected cities
                
            }
            PopulateThecblRoleName(ddlSelectByCity.SelectedItem.Text, isGeneralAdmin);

            //   Initialize(ReturnSelectedCities());
          //  PopulateThecblRoleName(ddlSelectByCity.SelectedValue);
//            PopulateThecblRoleName(ObtainUserCityFromGUID(ddlSelectByCity.SelectedValue));    // meoww  
          //  lblTest.Text = ddlSelectByCity.SelectedValue;
        } 

        protected List<string> ReturnSelectedCities()
        {
            List<string> ListOfAdminCities = new List<string>();
            if (ddlSelectByCity.SelectedValue != "-1")
            {
                ListOfAdminCities.Add(ddlSelectByCity.SelectedValue);
                return ListOfAdminCities;
            }
            else
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                try
                {
                    cnn.Open();
                    strQuery = "SELECT [ID] FROM [City] " + 
                               " UNION " +
                               "SELECT [ID] FROM [CombinatedCity]";                       
                        
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < ddlSelectByCity.Items.Count; j++)
                        {
                            if (ddlSelectByCity.Items[j].Value == dt.Rows[i][0].ToString())
                            {
                                ListOfAdminCities.Add(dt.Rows[i][0].ToString());
                                break;
                            }
                        }
                    }

                    da.Dispose();
                    cmd.Dispose();
                    cnn.Close();
                    return ListOfAdminCities;

                }
                catch (Exception ex)
                {
                    ShowMessage("RETURN EV NAME ERROR : (Most Likely, there is no EV Selected Yet) " + ex.Message);
                    cnn.Close();
                    return null;
                }
            }
        }

        protected void ShowMessage(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void HideError()
        {
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
            lblCatchError.Text = string.Empty;
        }

        protected void showAllFunction() // Obsolete, take out, and error check for later
        {
            //if (cbShowActivated.Checked)
            //{
            //    for (int i = 0; i < GV_UserEditor.Rows.Count; i++)
            //    {

            //        if (0 == String.Compare("False", GV_UserEditor.Rows[i].Cells[findGVcolumn("Activated?")].Text))
            //        {
            //            GV_UserEditor.Rows[i].Visible = false;
            //        }
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < GV_UserEditor.Rows.Count; i++)
            //    {
            //        GV_UserEditor.Rows[i].Visible = true;
            //    }
            //}


        }
        protected void GV_UserEditor_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideAll();
            HideAllTextBoxes();
            HideAllVerificationText();
            HideError();
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            FillInTxtBoxExistingUser(ReturnUserGUIDfromUsername(gvRow.Cells[findGVcolumn("User Name")].Text)); 
            PopulateEvList(gvRow.Cells[findGVcolumn("UserId")].Text);
            PopulateddlGvEvListModel();
            btnGvEvListAdd.Visible = true;
            btnUpdate.Visible = true;
            ddlEvModelList.Enabled = false;
            ErrorMessage.Text = string.Empty;
            
        }

        int GetSortColumnIndex()
        {
            // Iterate through the Columns collection to determine the index
            // of the column being sorted.
            foreach (DataControlField field in GV_UserEditor.Columns)
            {
                if (GV_UserEditor.Columns.IndexOf(field) != 0 && GV_UserEditor.Columns.IndexOf(field) != 1)
                {
                    if (field.SortExpression == GV_UserEditor.SortExpression)
                    {
                        return GV_UserEditor.Columns.IndexOf(field);
                    }
                }
            }
            return -1;
        }  
        void AddSortImage(int columnIndex, GridViewRow headerRow)
        {
            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (GV_UserEditor.SortDirection == SortDirection.Ascending)
            {
                sortImage.ImageUrl = "~/Images/asc.gif";
                sortImage.AlternateText = "Ascending Order";
            }
            else
            {
                sortImage.ImageUrl = "~/Images/desc.gif";
                sortImage.AlternateText = "Descending Order";
            }

            // Add the image to the appropriate header cell.
            headerRow.Cells[columnIndex].Controls.Add(sortImage);
        }

        private int GetColumnIndex(string SortExpression)
        {
            int i = 0;
            foreach (DataControlField c in GV_UserEditor.Columns)
            {
                if (c.SortExpression == SortExpression)
                    break;
                i++;
            }
            return i;
        }

        protected void GV_UserEditor_Sorting(object sender, GridViewSortEventArgs e)
        {          
            string CurrentSelectCity = ddlSelectByCity.SelectedItem.Text;


            clearAllText();
            // DataTable dataTable = GV_UserEditor.DataSource as DataTable;
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                GV_UserEditor.DataSource = dataTable.DefaultView;
                GV_UserEditor.DataBind();
            }

            showAllFunction();
            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);
            PopulateThecblRoleName(CurrentSelectCity, isGeneralAdmin);
            if (!ddlRoleArea.Enabled)
            {
                ddlRoleArea.SelectedValue = ddlSelectByCity.SelectedValue;
            }

            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in GV_UserEditor.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = GV_UserEditor.Columns.IndexOf(field);
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
            GV_UserEditor.HeaderRow.Cells[index].Controls.Add(sortImage2);          
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

        protected void Deactivate_user()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            try
            {
                GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
                string userGUID = ReturnUserGUIDfromUsername(gvRow.Cells[findGVcolumn("User Name")].Text);
                cnn.Open();

                strQuery = "UPDATE [aspnet_Users] SET [Activate] ='False' WHERE [UserId] = '" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                PopUpError("User Deactivated");
                FillInTxtBoxExistingUser(userGUID);
            }

            catch// (Exception ex)
            {
                ShowMessage("Error: First select a row in the gridview");
            }

            finally
            {
               // PopulateGridview(new List<string>()); // CHECK THIS
                cnn.Close();
            }
        }


        protected void GV_UserEditor_Paging(Object sender, GridViewPageEventArgs e) // Code for Paging 
        {
            string CurrentSelectCity = ddlSelectByCity.SelectedValue;
          
            clearAllText();

            DataTable dataTable = Session["data"] as DataTable;

            GV_UserEditor.PageIndex = e.NewPageIndex;
            GV_UserEditor.DataSource = dataTable;
            GV_UserEditor.DataBind();  

            ddlSelectByCity.SelectedValue = CurrentSelectCity;
            ddlRoleArea.SelectedValue = CurrentSelectCity;

            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);

            //List<string> ListOfAdminCities = new List<string>();
            //if (User.Identity.IsAuthenticated)
            //{
            //    RolePrincipal rp = (RolePrincipal)User;
            //    string[] roles = Roles.GetRolesForUser();

            //    List<string> ListOfRoles = new List<string>();
            //    for (int i = 0; i < roles.Count(); i++)
            //    {
            //        ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            //    }
            //    ListOfAdminCities = FindAssociatedRoles(ListOfRoles); // ListOfAdminCities is a list of user city that have admin roles  
            //}

            //PopulateGridview(ListOfAdminCities);
           //Initialize(ListOfAdminCities);
            PopulateThecblRoleName(ddlSelectByCity.SelectedItem.Text, isGeneralAdmin);
            showAllFunction();
        }
        #endregion 
        #region OtherHelperFunctions (Fill in Textboxes, Sort Directions)
        protected string[] strReturnEmailandPass(string strAdminRole)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; 
            DataTable dt = null;
            SqlDataAdapter da;
            string[] strArrayEmailPass = new string[2];
            try
            {
                strQuery = "SELECT [Email Address], [Email Password] FROM [City] WHERE Name= '" + strAdminRole + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                
                strArrayEmailPass[0] = dt.Rows[0][0].ToString();
                strArrayEmailPass[1] = dt.Rows[0][1].ToString();

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                strArrayEmailPass[0] = "-1";
                strArrayEmailPass[1] = "-1";
                return strArrayEmailPass;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strArrayEmailPass;
        }

        protected void SendMail(string strSendAddress, string strBodyText, string strSubject)
        {
            // Obtain Email Password
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            MailMessage email = null;
            SmtpClient sc = null;


            string strFromEmail = string.Empty;
            string strSmtpHost = string.Empty;
            string strEmailPass = string.Empty;
            bool blnSSL = true;
            string strSmtpport = string.Empty;

            string strEmailCity = ddlRoleArea.SelectedItem.Text;

            try
            {
                cnn.Open();
                strQuery = "Select [Email Address], [Email Password], [Email Host], [EnableSSL], [Email Port] FROM [City] WHERE [Name] = '" + strEmailCity + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                strFromEmail = dt.Rows[0][0].ToString();
                strEmailPass = dt.Rows[0][1].ToString();
                strSmtpHost = dt.Rows[0][2].ToString();
                blnSSL = bool.Parse(dt.Rows[0][3].ToString());

                strSmtpport = Server.HtmlDecode(dt.Rows[0][4].ToString());


                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at SendMail1: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
                return;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }


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
                    ShowMessage("Error: Update Password of Organization email.  The Decryption algorithm does not recognize the encryption.");
                    return;
                }
            }

            bool blnPassed = true;

            try
            {
                email = new MailMessage();
                email.From = new MailAddress(strFromEmail, "EV Monitoring Center");

                sc = new SmtpClient();
                sc.Host = strSmtpHost;

                if (!string.IsNullOrWhiteSpace(strSmtpport))
                {
                    try
                    {
                        sc.Port = int.Parse(strSmtpport);
                    }
                    catch
                    {
                        ShowMessage("Port information is incorrect.  Please check Edit Organization page to ensure the port is correct.");
                    }
                }

                sc.Credentials = new NetworkCredential(strFromEmail, strEmailPass);
                sc.EnableSsl = blnSSL;
                sc.Timeout = 10000;
                email.To.Add(strFromEmail);
                email.Bcc.Add(strSendAddress);

                string strEmailBody = strBodyText;

                strEmailBody = strEmailBody.Replace("\n", "<br/>");
                strEmailBody = strEmailBody.Replace(" ", "&nbsp;");

                email.Subject = strSubject;
                email.IsBodyHtml = true;
                email.Body = "<table>";
                email.Body += "<tr><td> " + strEmailBody + "</td></tr>";
                email.Body += "</table>";
                sc.Send(email);

            }
            catch (Exception ex)
            {
                ShowMessage("Error at SendMail2:  " + ex.Message);
                blnPassed = false;
            }
            finally
            {
                if (email != null)
                    email.Dispose();
                if (sc != null)
                    sc.Dispose();
                if (blnPassed)
                {
                    PopUpError("Email sent!");
                }
                else
                {

                }
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

        //protected void SendMail(string SendAddress, string BodyText)
        //{
        //    // Gmail Address from where you send the mail
        //    var fromAddress = "teste8562@gmail.com";
        //    // any address where the email will be sending
        //    var toAddress = SendAddress;
        //    //Password of your gmail address
        //    const string fromPassword = "progamer";
        //    // Passing the values and make a email formate to display
        //    string subject = "Your Usernames";
        //    string body = string.Empty;

        //    body = "From: SMERC Server  \n";
        //    body += BodyText + "\n ";
        //    body += "\nTo change your password, please log into the web server, and change your password";

            
        //    // smtp settings
        //    var smtp = new System.Net.Mail.SmtpClient();
        //    {
        //        smtp.Host = "smtp.gmail.com";
        //        smtp.Port = 587;
        //        smtp.EnableSsl = true;
        //        smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
        //        smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
        //        smtp.Timeout = 20000;
        //    }

        //    // Passing values to smtp object
        //    smtp.Send(fromAddress, toAddress, subject, body);
        //}

        private string ConvertSortDirection(SortDirection sortDirection)
        {
            string newSortDirection = String.Empty;

            switch (sortDirection)
            {
                case SortDirection.Ascending:
                    newSortDirection = "ASC";
                    break;

                case SortDirection.Descending:
                    newSortDirection = "DESC";
                    break;
            }
            return newSortDirection;
        }

         protected void PopUpError(string Message)
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

         protected string GetRTMCReportIntervals(string userGUID)
         {
             SqlConnection cnn = new SqlConnection(connectionString);
             string strQuery;
             SqlCommand cmd;
             DataTable dt = null;
             SqlDataAdapter da;
             string intervals = "lalaal";
             try
             {
                 cnn.Open();
                 strQuery = "SELECT [RTMCReportIntervals] FROM [aspnet_Profile] where [UserId] = '" + userGUID + "'";
                 cmd = new SqlCommand(strQuery, cnn);
                 cmd.CommandType = CommandType.Text;
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);
                 intervals = "fuck";
                 intervals = dt.Rows[0][0].ToString();
             }
             catch (Exception ex)
             {
                 lblCatchError.Text += "<br> Error In GetRTMCReportIntervals " + ex.Message;
             }
             return intervals;
         }

         protected bool CheckforNewUserVerification()
         {
             bool pass = true;

             int numbOfUsers = 0;
             List<string> Username = new List<string>();
             SqlConnection cnn = new SqlConnection(connectionString);
             string strQuery;
             SqlCommand cmd;
             DataTable dt = null;
             SqlDataAdapter da;

             try
             {
                 cnn.Open();
                 strQuery = "SELECT COUNT(*) FROM [aspnet_Users]";
                 cmd = new SqlCommand(strQuery, cnn);
                 cmd.CommandType = CommandType.Text;
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);

                 numbOfUsers = int.Parse(dt.Rows[0][0].ToString());

                 strQuery = "SELECT [Username] FROM [aspnet_Users]";
                 cmd = new SqlCommand(strQuery, cnn);
                 cmd.CommandType = CommandType.Text;
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);

                 for (int i = 0; i < numbOfUsers; i++)
                 {
                     Username.Add(dt.Rows[i][0].ToString());
                     if (tbUsername.Text == dt.Rows[i][0].ToString())
                     {
                         pass = false;
                         UserNameVerifyText.Text = "Enter a unique username";
                     }
                 }

                 strQuery = "SELECT [SMERCID] FROM [aspnet_Profile]";
                 cmd = new SqlCommand(strQuery, cnn);
                 cmd.CommandType = CommandType.Text;
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);

                 for (int j = 0; j < dt.Rows.Count; j++)
                 {
                     if (tbSMERCID.Text == dt.Rows[j][0].ToString())
                     {
                         LBL_SMERCID_Verify.Text += "<br> Repeat SMERCID.  Enter a unique SMERCID";
                         pass = false;
                     }
                 }

                 da.Dispose();
                 cmd.Dispose();
             }
             catch (Exception ex)
             {
                 ErrorMessage.Text += "<br> Error In Create User " + ex.Message;
             }
             finally
             {
                 cnn.Close();
             }

             if (tbUsername.Text.IndexOf("'") != -1)
             {
                 UserNameVerifyText.Text = "No quotation marks allowed in Username";
                 pass = false;
             }

             if (tbUsername.Text == string.Empty)
             {
                 UserNameVerifyText.Text = "Enter a username";
                 pass = false;
             }
             if (tbUsername.Text.IndexOf(".") != -1)
             {
                 UserNameVerifyText.Text = "Invalid Username. Remove Period.";
                 pass = false;
             }
             if (TxtBox_Password.Text == string.Empty)
             {
                 PasswordVerifyLabel.Text = "Enter a Password.";
                 pass = false;
             }
             if (0 != string.Compare(TxtBox_Password.Text, TxtBox_PasswordRepeat.Text))
             {
                 PasswordVerifyLabel.Text = "You must enter the same password twice!";
                 pass = false;
             }
             if (TxtBox_Password.Text != TxtBox_PasswordRepeat.Text)
             {
                 PasswordVerifyLabel.Text = "Passwords must be the same!";
                 pass = false;
             }
             if (tbEmail.Text == "")
             {
                 lblEmailVerify.Text = "Email Required";
                 pass = false;
             }
             
             if (TxtBox_Password.Text != TxtBox_PasswordRepeat.Text)
             {
                 PasswordVerifyLabel.Text = "Please enter the same password";
                 pass = false;
             }

             if (ddlRoleArea.SelectedIndex == 0)
             {
                 City_Verify_label.Text = "Role Area Required";
                 pass = false;
             }

             if (ddlEvModelList.SelectedIndex == 0)
             {
                 lblEvVerify.Text = "EV Required";
                 pass = false;
             }
             if (ddlRTMCUserAccountType.SelectedIndex == 0)
             {
                 lblRTMCUserAccountTypeVerify.Text = "Required";
                 pass = false;
             }
             if (ddlRTMCUserAccountExpirationWindow.SelectedIndex == 0)
             {
                 lblRTMCUserAccountExpirationWindow.Text = "Required";
                 pass = false;
             }
             if (ddlEVUserAccountType.SelectedIndex == 0)
             {
                 lblEVUserAccountType.Text = "Required";
                 pass = false;
             }
             if (ddlRTMCChartAndReport.SelectedIndex == 0)
             {
                 lblRTMCChartAndReport.Text = "Required";
                 pass = false;
             }
             if (ddlEVUserAccountExpirationWindow.SelectedIndex == 0)
             {
                 lblEVUserAccountExpirationWindow.Text = "Required";
                 pass = false;
             }

             if(tbCity.Text == string.Empty)
             {
                 lblCityVerify.Text = "City Required"; 
                 pass = false;
             }



             int count = 0;
             for (int i = 0; i < cblRoleName.Items.Count; i++)
             {
                 if (cblRoleName.Items[i].Selected)
                     count++;
             }

             if (count == 0)
             {
                 lblRoleNameVerify.Text = "You must select atleast one role name";
                 pass = false;
             }
             return pass;
         }       

         protected void FillInTxtBoxExistingUser(string userGUID)
         {
             GridViewRow gvRow;

             try
             {
                 gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
             }
             catch
             {
                 clearAllText();
                 return;
             }


             tbUsername.Text = gvRow.Cells[findGVcolumn("User Name")].Text;
             tbEmail.Text = gvRow.Cells[findGVcolumn("Email")].Text;
             ddlIsApproved.SelectedValue = gvRow.Cells[findGVcolumn("Is Approved")].Text;
             ddlIsLockedOut.SelectedValue = gvRow.Cells[findGVcolumn("Is Locked Out")].Text;
             ddlIsActivated.SelectedValue = gvRow.Cells[findGVcolumn("Activated")].Text;


             DataTable dt = null;
             dt = ReturnASPNETUSERS(userGUID); // Return Table from ASPUSERS table
             dt = ReturnMembershipTable(userGUID); // Return Table from Membership Table

             MembershipUser CurrentUser = Membership.GetUser(ObtainUserNameFromGuid(userGUID)); // MembershipUser is for obtaining/resetting the password



             SqlConnection cnn = new SqlConnection(connectionString);
             string strQuery;
             SqlCommand cmd;
             DataTable dt2 = null;
             SqlDataAdapter da;
             try
             {
                 cnn.Open();
                 strQuery = "SELECT [FirstName], [LastName], [PhoneNo], [Address1], [Address2], [ZipCode], [Priority], [City], [State], [SMERCID], [SmartPhoneOS], [PhoneServiceCarrier], [SmartPhoneModelNo], [RTMCUserAccountTypeID],[RTMCUserAccountExpirationWindowID],[EVUserAccountTypeID],[EVUserAccountExpirationWindowID], [ChargingPoints], [RTMCReportIntervals], [RTMCChartAndReportType],[EVUserSessionTimeout],[RTMCSessionTimeout], [MaximumVehicles], [AllowTextMessage], [MonitorRefreshInterval] "
                           + "FROM [aspnet_Profile]"
                           + "WHERE [UserId] ='" + userGUID + "'";

                 cmd = new SqlCommand(strQuery, cnn);
                 cmd.CommandType = CommandType.Text;
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt2 = new DataTable();
                 da.Fill(dt2);



                 tbFirstName.Text = dt2.Rows[0][0].ToString();
                 tbLastName.Text = dt2.Rows[0][1].ToString();
                 tbPhoneNumber.Text = dt2.Rows[0][2].ToString();
                 tbAddress1.Text = dt2.Rows[0][3].ToString();
                 tbAddress2.Text = dt2.Rows[0][4].ToString();
                 tbZipCode.Text = dt2.Rows[0][5].ToString();
                 ddlPriority.SelectedValue = dt2.Rows[0][6].ToString();
                 // addition 11-14-2012 - dhk
                 tbCity.Text = dt2.Rows[0][7].ToString();
                 try
                 {
                     ddlUserState.SelectedValue = dt2.Rows[0][8].ToString();
                 }
                 catch
                 {
                     ddlUserState.SelectedIndex = 0;
                 }

                 tbSMERCID.Text = dt2.Rows[0][9].ToString();
                 tb_SmartPhoneModelNo.Text = dt2.Rows[0][12].ToString();
                 ddlSmartPhoneOS.SelectedIndex = int.Parse(dt2.Rows[0][10].ToString());
                 //int.Parse(dt2.Rows[0][11].ToString());
                 ddlPhoneServiceCarrier.SelectedIndex = int.Parse(dt2.Rows[0][11].ToString());
                 ddlRTMCUserAccountType.SelectedValue = dt2.Rows[0][13].ToString();
                 ddlRTMCUserAccountExpirationWindow.SelectedValue = dt2.Rows[0][14].ToString();
                 ddlEVUserAccountType.SelectedValue = dt2.Rows[0][15].ToString();
                 ddlEVUserAccountExpirationWindow.SelectedValue = dt2.Rows[0][16].ToString();
                 tbChargingPoints.Text = dt2.Rows[0][17].ToString();
                 ddlRTMCReportIntervals.SelectedValue = dt2.Rows[0][18].ToString();
                 ddlRTMCChartAndReport.SelectedValue = dt2.Rows[0][19].ToString();
                 tbEVUserSessionTimeout.Text = dt2.Rows[0][20].ToString();
                 tbRTMCSessionTimeout.Text = dt2.Rows[0][21].ToString();
                 ddlMaxVehicles.SelectedValue = dt2.Rows[0][22].ToString();
                 ddlAllowTextMsg.SelectedValue = dt2.Rows[0][23].ToString() == "True" ? "1":"0";
                 ddlMonitorRefreshInterval.SelectedValue = dt2.Rows[0][24].ToString();

                 da.Dispose();
                 cmd.Dispose();
                 dt2.Dispose();
             }
             catch (Exception ex)
             {
                 ErrorMessage.Text += "Error at FillinTxt: " + ex.Message;
                 PopUpError("Fill out missing information on right.");
             }

             finally
             {
                 cnn.Close();
             }

             // end addition

             string attemptPassword = string.Empty;
             try
             {
                 attemptPassword = CurrentUser.GetPassword();
                 // CHANGEDONCE
                 //TxtBox_Password.Text = attemptPassword;
                 //TxtBox_PasswordRepeat.Text = attemptPassword;
                 TxtBox_Password.Attributes.Add("value", attemptPassword);
                 TxtBox_PasswordRepeat.Attributes.Add("value", attemptPassword);
             }
             catch
             {
                 if (ddlIsLockedOut.SelectedValue == "True")
                 {
                     
                    // RepeatPasswordVerifyLabel.Text= "User is either Locked out, or the password is hashed.  To check if the password is encrypted, verify that 'Is locked out' is set to false.";
                 }
                 if(ddlIsLockedOut.SelectedValue == "False")
                 {
                     btnResetPass.Visible = true;
                     PasswordVerifyLabel.Text = "Hashed Password.";
                     RepeatPasswordVerifyLabel.Text = "After reset, a new password is sent to the user's email.";
                 }
             }

             if (!string.IsNullOrEmpty(dt.Rows[0][1].ToString()))
                 tbPassQuestion.Text = dt.Rows[0][1].ToString();
             else
                 tbPassQuestion.Text = string.Empty;
             if (!string.IsNullOrEmpty(dt.Rows[0][2].ToString()))
                 tbPassAnswer.Text = dt.Rows[0][2].ToString();
             else
                 tbPassAnswer.Text = string.Empty;
          
             ddlEvModelList.SelectedValue = returnEVNAME(userGUID);

             
             ddlRoleArea.SelectedValue = ObtainCityGUIDfromUserCity(returnRoleArea(userGUID));
           //  DDL_UserCity.SelectedValue = returnUserCity(userGUID);
         //    PopulateThecblRoleName(ddlRoleArea.SelectedItem.Text);

             string strRoleArea = gvRow.Cells[findGVcolumn("Role Area")].Text;

             RolePrincipal rp = (RolePrincipal)User; // meow
             string[] roles = Roles.GetRolesForUser();
             List<string> ListOfRoles = new List<string>();

             for (int i = 0; i < roles.Count(); i++)
             {
                 ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
             }
             bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);

             PopulateThecblRoleName(strRoleArea, isGeneralAdmin);
             FillInRoleCheckList(userGUID);            
             
         }
         protected void FillInRoleCheckList(string UserGuid)
         {
             SqlConnection cnn = new SqlConnection(connectionString);
             string strQuery;
             SqlCommand cmd;
             DataTable dt = null;
             SqlDataAdapter da;

             try
             {
                 cnn.Open();
                 strQuery = "SELECT COUNT(*) FROM [aspnet_UsersInRoles] WHERE [UserId] = '" + UserGuid + "'";
                 cmd = new SqlCommand(strQuery, cnn);
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);
                 
                 int numbOfUsers = int.Parse(dt.Rows[0][0].ToString());

                 List<string> NamesOfRoles = ReturnRolesOfUserGuid(UserGuid, numbOfUsers);

                 for (int i = 0; i < numbOfUsers; i++)
                 {
                     ListItem item = cblRoleName.Items.FindByValue(NamesOfRoles[i]);
                     item.Selected = true;
                 }
                 da.Dispose();
                 cmd.Dispose();

             }
             catch
             {
                 ErrorMessage.Text += " <br> Error: Most likely the city name and role area is different.  To fix, first set the User City to the same area as the Role";               
             }
             finally
             {
                 cnn.Close();
             }
         }

        #endregion
        #region ReturnFunctions

         protected List<string> ReturnRolesOfUserGuid(string UserGuid, int numbOfUsers)
         {
             SqlConnection cnn = new SqlConnection(connectionString);
             string strQuery;
             SqlCommand cmd;
             DataTable dt = null;
             SqlDataAdapter da;
             try
             {
                 cnn.Open();
                 strQuery = "SELECT RoleId FROM [aspnet_UsersInRoles] WHERE [UserId] = '" + UserGuid + "'";
                 cmd = new SqlCommand(strQuery, cnn);
                 da = new SqlDataAdapter();
                 da.SelectCommand = cmd;
                 dt = new DataTable();
                 da.Fill(dt);

                 List<string> roleNameList = new List<string>();
                 for (int i = 0; i < numbOfUsers; i++)
                 {
                     roleNameList.Add(returnRoleName(dt.Rows[i][0].ToString()));
                 }

                 da.Dispose();
                 cmd.Dispose();
                 cnn.Close();
                 return roleNameList;

             }
             catch (Exception ex)
             {
                 
                 ErrorMessage.Text += " <br> Obtain RoleName  ERROR2 " + ex.Message;
                 cnn.Close();
                 return null;
             }

         }
        protected List<string> returnListofEvCars()
        {
            List<string> EVList = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            try
            {
                cnn.Open();
                strQuery = "SELECT [Manufacturer] + ' ' + [Model] AS [Name] FROM [EV Model] ORDER BY Manufacturer";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                da.Dispose();
                cmd.Dispose();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    EVList.Add(dt.Rows[i][0].ToString());
                }
            }
            catch
            {
                ErrorMessage.Text += "<br> EV Error ";
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return EVList;
        }

        protected List<string> ReturnListOfRoleArea()
        {
            List<string> WholeNameOfRoles = new List<string>();


            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [Name] FROM [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    WholeNameOfRoles.Add(dt.Rows[i][0].ToString());
                }

                da.Dispose();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += " <br> Return Role Area ERROR:" + ex.Message;
                cnn.Close();
                return null;
            }
            finally
            {
                cnn.Close();
            }
            return WholeNameOfRoles;
        }
        protected List<string> ReturnRoleNames(bool isGeneralAdmin) // Return all of the types of Roles, Add Roles here if the types change
        {
            if (isGeneralAdmin)
            {
                List<string> ListOfRoleNames = new List<string>();
                ListOfRoleNames.Add("Administrator");
                ListOfRoleNames.Add("Operator");
                ListOfRoleNames.Add("Maintainer");
                ListOfRoleNames.Add("User");

                return ListOfRoleNames;
            }
            else
            {
                List<string> ListOfRoleNames = new List<string>();
                ListOfRoleNames.Add("Operator");
                ListOfRoleNames.Add("Maintainer");
                ListOfRoleNames.Add("User");

                return ListOfRoleNames;
            }

        }
        protected List<string> ReturnRoleNames(string City, bool isGeneralAdmin) // Return all of the types of Roles, Add Roles here if the types change
        {
            if (isGeneralAdmin)
            {
                List<string> ListOfRoleNames = new List<string>();
                ListOfRoleNames.Add(City + " Administrator");
                ListOfRoleNames.Add(City + " Operator");
                ListOfRoleNames.Add(City + " Maintainer");
                ListOfRoleNames.Add(City + " User");
                return ListOfRoleNames;
            }
            else
            {
                List<string> ListOfRoleNames = new List<string>();
                ListOfRoleNames.Add(City + " Operator");
                ListOfRoleNames.Add(City + " Maintainer");
                ListOfRoleNames.Add(City + " User");
                return ListOfRoleNames;
            }

        }


        protected string ReturnUserGUIDfromUsername(string UserName)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            string userGUID;
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
                userGUID = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {                
                ErrorMessage.Text += " <br> Return UserGUID ERROR:" +  ex.Message;
                cnn.Close();
                return null;
            }
            finally
            {
                cnn.Close();
            }
            return userGUID;
        }

        protected string ReturnRoleNameAppendString(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                List<string> RoleIDs = new List<string>();
                strQuery = "SELECT [RoleID] FROM [aspnet_UsersInRoles] WHERE [UserID] ='" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                int count = dt.Rows.Count;

                da.Dispose();
                cmd.Dispose();
               
                try
                {
                    string ReturnString = string.Empty;

                    for (int i = 0; i < count; i++)
                    {
                        RoleIDs.Add(dt.Rows[i][0].ToString());
                    }

                    for (int i = 0; i < count; i++)
                    {
                        strQuery = "SELECT [RoleName] FROM [aspnet_Roles] WHERE [RoleID] ='" + RoleIDs[i] + "'";
                        cmd = new SqlCommand(strQuery, cnn);
                        cmd.CommandType = CommandType.Text;
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);

                        ReturnString += dt.Rows[0][0].ToString();
                        if (i != count - 1)
                            ReturnString += " -- ";
                    }                   

                    cnn.Close();
                    return ReturnString;
                }
                catch
                {
                    cnn.Close();
                    
                    ErrorMessage.Text = " ERROR in Populating Role Gridview ";
                    return null;
                }

            }
            catch (Exception ex)
            {
                cnn.Close();
                
                ErrorMessage.Text += "<br> Error in ReturnRoleID " + ex.Message;
                return null;
            }
        }

        protected string ReturnRoleID(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [RoleID] FROM [aspnet_UsersInRoles] WHERE [UserID] ='" + userGUID + "'";
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
                
                ErrorMessage.Text += "<br> Error in ReturnRoleID "+ ex.Message;
                cnn.Close();
                return null;
            }
            finally
            {
                cnn.Close();
            }
            try
            {
                return dt.Rows[0][0].ToString();
            }
            catch
            {
                return "Null";
            }
        }



        protected string returnRoleName(string RoleID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [RoleName] FROM [aspnet_Roles] WHERE [RoleID] ='" + RoleID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
                string roleName = dt.Rows[0][0].ToString();
                cnn.Close();
                return roleName;
            }
            catch (Exception)
            {
                //  ErrorMessage.Text += "Rolename is Empty<br>";
                cnn.Close();
                return null;
            }
        }


        protected DataTable ReturnMembershipTable(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT [Email], [PasswordQuestion], [PasswordAnswer] ,[IsApproved] ,[IsLockedOut] FROM [aspnet_Membership] WHERE [UserID] ='" + userGUID + "'";
                cnn.Open();
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
                
                ErrorMessage.Text += "<br> Return Membership Error: " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            return dt;
        }

        protected string returnUserCity(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [CityID] FROM [aspnet_Profile] WHERE [UserID] ='" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                string CityGUID = dt.Rows[0][0].ToString();
                da.Dispose();
                cmd.Dispose();

                cnn.Close();
                string CityName = ObtainUserCityFromGUID(CityGUID);
                return CityName;
            }
            catch //(Exception ex)
            {
                //  ErrorMessage.Text += "Return user City Error: " + ex.Message;
                cnn.Close();
                return null;
            }
        }

        protected string returnRoleArea(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [RoleCityID] FROM [aspnet_Profile] WHERE [UserID] ='" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                string CityGUID = dt.Rows[0][0].ToString();
                da.Dispose();
                cmd.Dispose();

                cnn.Close();
                string CityName = ObtainUserCityFromGUID(CityGUID);
                return CityName;
            }
            catch //(Exception ex)
            {
                //  ErrorMessage.Text += "Return user City Error: " + ex.Message;
                cnn.Close();
                return null;
            }

        }

        protected string returnEVNAME(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [EV ID] FROM [aspnet_Profile] WHERE [UserID] ='" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                string EVGUID = dt.Rows[0][0].ToString();
                string EVNAME = ObtainEVnameFromGUID(EVGUID);                

                da.Dispose();
                cmd.Dispose();
                cnn.Close();
                return EVNAME;

            }
            catch //(Exception ex)
            {
             //   ErrorMessage.Text += "<br> RETURN EV NAME ERROR : (Probably there is no EV Selected Yet) " + ex.Message;
                cnn.Close();
                return null;
            }
        }

        protected List<string> ReturnStringOfCombinatedCities()
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

                // This dictionary, CityIdNameRelation, stores the ID and name of each city in the City Database
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
                // TO DO: Implement function to find non activated combo cities, so that we can exclude them.
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
                    ReturnCityList.Add(subcities); // Add the maincity - subcity combo to this list<string>
                    //   ComboCityWithGuid.Add(new ComboCityAndGuidClass(subcities, uniqueGUIDs[i]));
                }

                da.Dispose();
                cmd.Dispose();
                cnn.Close();
                return ReturnCityList;

            }
            catch
            {
                cnn.Close();
                return null;
            }
        }

        protected List<ComboCityAndGuidClass> ReturnUniqueCombinedGUID()
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
                List<ComboCityAndGuidClass> ComboCityWithGuid = new List<ComboCityAndGuidClass>();
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
                    ComboCityWithGuid.Add(new ComboCityAndGuidClass(subcities, uniqueGUIDs[i]));
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


        protected DataTable ReturnASPNETUSERS(string userGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [Username], [Activate] FROM [aspnet_Users] WHERE [UserID] ='" + userGUID + "'";
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
                
                ErrorMessage.Text += "<br> RETURN ASPUSER ERROR: " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            return dt;
        }

        #endregion
        #region Buttons, Update Function, New User Function, Password Reset Function

        protected void btnNewUser_Click(object sender, EventArgs e)
        {
            HideError();
            if (string.IsNullOrEmpty(TxtBox_Password.Text))
            {
                TxtBox_Password.Text = "123456";
                TxtBox_PasswordRepeat.Text = "123456"; // temp pass if TB's are empty
            }

            if (!CheckforNewUserVerification()) // true = passed, false = not pass
            {                
                ErrorMessage.Text += "<br> Errors:";
                PopUpError("Error when inserting new user.  User: " + tbUsername.Text);
                return;
            }                               
            
            //MembershipCreateStatus status;
            //if (tbPassQuestion.Text == "")
            //{
            //     Membership.CreateUser(tbUsername.Text, TxtBox_Password.Text, tbEmail.Text);
            //}
            //else
            //{
            //     Membership.CreateUser(tbUsername.Text, TxtBox_Password.Text, tbEmail.Text, tbPassQuestion.Text, tbPassAnswer.Text, true, out status);
            //}

            bool blnPass = UpdateFunction(tbUsername.Text, string.Empty, string.Empty, true);
            //UpdateFunction(string.Empty, string.Empty, string.Empty);
            PopulateGridview(ReturnListOfAdminCities(), cbShowActivated.Checked);

            if (blnPass)
                PopUpError("New user created, User: " + tbUsername.Text);
            else
                PopUpError("Error while creating user.");

            clearAllText();
        }

        protected void FillInPassword()
        {
            TxtBox_Password.Text = "123456";
            TxtBox_PasswordRepeat.Text = "123456";
            PasswordVerifyLabel.Text = "'123456' is the default password when creating a new user.";
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            clearAllText();
            FillInPassword();
        }
        protected void btnResetPass_Click(object sender, EventArgs e)
        {            
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex]; // retrieve selected row  % Add SendMail(SendAddress, Body Text)
            MembershipUser CurrentUser = Membership.GetUser(gvRow.Cells[findGVcolumn("User Name")].Text);
            string RandomPassword = Membership.GeneratePassword(Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters);
            CurrentUser.ChangePassword(CurrentUser.ResetPassword(), RandomPassword);

            FillInTxtBoxExistingUser(ReturnUserGUIDfromUsername(gvRow.Cells[findGVcolumn("User Name")].Text));
            TxtBox_Password.Text = RandomPassword;
            TxtBox_PasswordRepeat.Text = RandomPassword;
            PasswordVerifyLabel.Text = "Reset password on left, and also emailed to " + tbEmail.Text;

            string strSubject= "Reset Password Information";
            string emailBody = "Here is your reset password. \n";
            emailBody += "Username: " + CurrentUser.UserName + "   Password: " + RandomPassword + " \n ";
            SendMail(tbEmail.Text, emailBody, strSubject); // Send Address   
            PopUpError("New password sent to: " + tbEmail.Text);
        }
              


        protected void btnUpdate_Click(object sender, EventArgs e)
        {   
            HideAllVerificationText();
            HideError();

            if(GV_UserEditor.SelectedIndex == -1)
            {     
                PopUpError("Error when updating.  Please first select a user.");
                return;
            }

            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            MembershipUser CurrentUser = Membership.GetUser(gvRow.Cells[findGVcolumn("User Name")].Text);

            string strPreviousRole = gvRow.Cells[findGVcolumn("Role Area")].Text;
            bool blnChangedRoles = (strPreviousRole != ddlRoleArea.SelectedItem.Text);

            if (!UpdateUserRepeatVerification(CurrentUser.UserName))
            {                
                ErrorMessage.Text += "<br> Errors Below ";
                PopUpError("Look below for errors.  Failed: ");
                return; // return!
            }

            string attemptPassword = string.Empty;
            try
            {
                attemptPassword = CurrentUser.GetPassword();
            }
            catch
            {
                attemptPassword = "Encrypted";
            }

            // Update!
            bool blnPass = UpdateFunction(string.Empty, attemptPassword, CurrentUser.UserName, false);

            // Check whether to uncheck or check the CheckBox_SelectByCity

            //if (AdminCities.IndexOf("General") != -1)
            //    CheckBox_SelectByCity.Checked = true;

            
            PopulateGridview(ReturnListOfAdminCities(), cbShowActivated.Checked);

            if (blnPass)
                PopUpError("Successfully updated User: " + tbUsername.Text);
            else
                PopUpError("Error while updating.");
          
            FillInTxtBoxExistingUser(ReturnUserGUIDfromUsername(tbUsername.Text));
            ErrorMessage.Text = string.Empty;

            TxtBox_Password.Text = string.Empty;
            TxtBox_PasswordRepeat.Text = string.Empty;

            if (blnChangedRoles)
            {
                clearAllText();
                GV_UserEditor.SelectedIndex = -1;
            }
        }
        protected void showAllFunction(List<string> ListOfAdminCity)
        {
            for (int i = 0; i < GV_UserEditor.Rows.Count; i++)
            {
                GV_UserEditor.Rows[i].Visible = true; // make visible again, then check for matched city, then change it below
                if (GV_UserEditor.Rows[i].Cells[findGVcolumn("Is Approved")].Text.IndexOf(ddlSelectByCity.SelectedValue) == -1) // if the Cell Row contains (indexof != -1) then display the city
                {
                    GV_UserEditor.Rows[i].Visible = false;
                }
                if (GV_UserEditor.Rows[i].Cells[findGVcolumn("Is Approved")].Text.Length != ddlSelectByCity.SelectedValue.Length)
                {
                    GV_UserEditor.Rows[i].Visible = false;
                }
            }
        }
        protected List<string> ReturnListOfAdminCities()
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
                ListOfAdminCities = FindAssociatedRoles(ListOfRoles); // ListOfAdminCities is a list of user city that have admin roles               
            }
            return ListOfAdminCities;
        }

        protected string ObtainUserNameFromGuid(string UserGuid)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT UserName FROM [aspnet_Users] WHERE [UserId] = '" + UserGuid + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
                string userName = dt.Rows[0][0].ToString();
                cnn.Close();
                return userName;

            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += " <br> Obtain UserNameGuid  ERROR " + ex.Message;
                cnn.Close();
                return null;
            }

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
                List<ComboCityAndGuidClass> ComboCityAndGuid = ReturnUniqueCombinedGUID();
                for (int i = 0; i < ComboCityAndGuid.Count; i++)
                {
                    if (UserCity == ComboCityAndGuid[i].ComboCityString)
                    {
                        cnn.Close();
                        return ComboCityAndGuid[i].Guid;
                    }
                }
                cnn.Close();
                return null;
            }
        }

        protected string ObtainUserCityFromGUID(string CityGUID)
        {
            if (CityGUID == "-1")
            {
                return string.Empty;
            }
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT Name FROM [City] WHERE [ID] = '" + CityGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
                string UserCityName = dt.Rows[0][0].ToString();
                cnn.Close();
                return UserCityName;

            }
            catch
            {
                List<ComboCityAndGuidClass> ComboCityAndGuid = ReturnUniqueCombinedGUID();
                for (int i = 0; i < ComboCityAndGuid.Count; i++)
                {
                    if (ComboCityAndGuid[i].Guid == CityGUID)
                    {
                        cnn.Close();
                        return ComboCityAndGuid[i].ComboCityString;
                    }
                }
                cnn.Close();
                return null;
            }
        }

        protected string ObtainEVnameFromGUID(string EVGUID)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT Manufacturer, Model FROM [EV Model] WHERE [Id] = '" + EVGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                string combinatedEVname = dt.Rows[0][0].ToString() + " " + dt.Rows[0][1].ToString();
                da.Dispose();
                cmd.Dispose();
                cnn.Close();
                return combinatedEVname;             
            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += " <br> ObtainEV NAME ERROR " + ex.Message;
                cnn.Close();
                return null;
            }
        }

        protected string ObtainGUIDofCity(string City)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da; 

            try
            {                               
                cnn.Open();
                strQuery = "SELECT DISTINCT ID FROM [City] WHERE [Name] = '" + City + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                cnn.Close(); // close connection
                return dt.Rows[0][0].ToString();
             }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += " <br> Obtain City GUID ERROR:  "+ ex.Message;
                cnn.Close();
                return null;
            }            
        }

        protected List<string> ReturnAllCities()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "select [Name] from [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                List<string> ReturnList = new List<string>();

                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    ReturnList.Add(dt.Rows[i][0].ToString());
                }

                cnn.Close();
                return ReturnList;
                
            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when return City " + ex.Message;
                cnn.Close();
                return null;
            }
        }

        protected bool UpdateFunction(string CheckForInsert, string CheckForPasswordEntry, string USERname, Boolean isNew)
        {
            string CurrentUserName = string.Empty;
            bool blnNewInsert = false;
            bool blnFuncPass = true;
                       
            if (CheckForInsert == string.Empty)
            {          
                CurrentUserName = USERname;
            }
            else
            {
                blnNewInsert = true;
                MembershipCreateStatus status; // Create User
                if (tbPassQuestion.Text == "")
                {
                    Membership.CreateUser(tbUsername.Text, TxtBox_Password.Text, tbEmail.Text);
                }
                else
                {
                    Membership.CreateUser(tbUsername.Text, TxtBox_Password.Text, tbEmail.Text, tbPassQuestion.Text, tbPassAnswer.Text, true, out status);
                }
                CurrentUserName = CheckForInsert; // Check if this function is called from update or insert.  IF update, then retrieve username from table, if insert, username is inputted into function
            }

            // orig location for pass change
            string userGUID = ReturnUserGUIDfromUsername(CurrentUserName);
            string RoleName = string.Empty;
            string RoleNameAndCity = string.Empty;                  

            string RoleGUID = string.Empty;

            int NumbOfRolesChecked = 0; // Find how many Role name check boxes are checked
            List<string> CheckedRoles = new List<string>();
         //   var RoleNames = new List<string>();

            for (int i = 0; i < cblRoleName.Items.Count; i++)
            {
                if (cblRoleName.Items[i].Selected)
                {
                    NumbOfRolesChecked++;
                    CheckedRoles.Add(cblRoleName.Items[i].Text);
                }
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            SqlCommand command = cnn.CreateCommand();
            SqlTransaction transaction; // Required for rollback features 


            // Update variable declarations

            string userGUIDforRE = string.Empty;
            MembershipUser Username1 = Membership.GetUser(USERname);
            List<string> ListOfRoleGuid = new List<string>();
            int count = 0;
            List<string> models = new List<string>();

            string EVCARname = ddlEvModelList.SelectedValue; // Ev car name is two words, manufacturer and model thus need to separate the terms
            string SelectedModel = string.Empty;
            string EVGUID = string.Empty;

            string SelectedCity = ddlRoleArea.SelectedItem.Text; // GUID of the Role Area ( diff than user city)                                    
            string RoleCityGUID = string.Empty; // Changed 11-14-2012 -- shifting to RoleCityGUID from CityGUID

            try
            {
                cnn.Open();
                if (NumbOfRolesChecked == 1)
                {
                    strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = '" + CheckedRoles[0] + "'";
                    cmd = new SqlCommand(strQuery, cnn);
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);

                    RoleGUID = dt.Rows[0][0].ToString(); // Obtain the GUID of the RoleName 
                }
                else
                {                    
                    for (int i = 0; i < NumbOfRolesChecked; i++)
                    {
                        strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = '" + CheckedRoles[i] + "'";
                        cmd = new SqlCommand(strQuery, cnn);
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);

                        RoleGUID = dt.Rows[0][0].ToString(); // Obtain the GUID of the RoleName 
                        ListOfRoleGuid.Add(RoleGUID);
                    }
                }
                strQuery = "SELECT COUNT(*) FROM [aspnet_Profile] WHERE [UserId] = '" + userGUID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                count = int.Parse(dt.Rows[0][0].ToString());  // count is the number of profile associated with a UserID.  either 1 or 0                

                strQuery = "SELECT [Model] FROM [EV Model]";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    models.Add(dt.Rows[i][0].ToString());
                }
               
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (EVCARname.IndexOf(models[i]) != -1)
                        SelectedModel = models[i];
                }


                strQuery = "SELECT ID FROM [EV Model] WHERE MODEL='" + SelectedModel + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                EVGUID = dt.Rows[0][0].ToString();
                                
                strQuery = "SELECT COUNT(*) FROM [City] WHERE Name = '" + SelectedCity + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                int cityCheck = int.Parse(dt.Rows[0][0].ToString());

                if (cityCheck == 1)
                {
                    strQuery = "SELECT ID FROM [City] WHERE NAME='" + SelectedCity + "'";
                    cmd = new SqlCommand(strQuery, cnn);
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    RoleCityGUID = dt.Rows[0][0].ToString();
                }

                else // if (cityCheck == 0) aka the City is a combined city
                {
                    List<ComboCityAndGuidClass> ComboAndGuid = ReturnUniqueCombinedGUID(); // list of comboCities || Combo GUIDs
                    for (int i = 0; i < ComboAndGuid.Count; i++)
                    {
                        if (ComboAndGuid[i].ComboCityString == SelectedCity)
                        {
                            RoleCityGUID = ComboAndGuid[i].Guid;
                        }
                    }
                }
            }
            catch
            {
                blnFuncPass = false;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }            

            // ///// BEGIN TRANSCATION // ROLLBACK PROTECTION

            cnn.Open();
            transaction = cnn.BeginTransaction("UpdateFunction");
            command.Connection = cnn;
            command.Transaction = transaction;

            bool blnPasses = true;  // Have a marker.  If the transaction fails, we need to also delete the user/membership that we created earlier for the new user.       


            try 
            {   
                // Update Roles
                if (NumbOfRolesChecked == 1) // if only one role is checked
                {                   
                    
                    command.CommandText = "Delete from [aspnet_UsersInRoles] WHERE [UserId] = @userGUID";
                    command.Parameters.AddWithValue("@userGUID", userGUID);
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO [aspnet_UsersInRoles](UserId, RoleId) VALUES(@userGUID,@RoleGUID)";                  
                    command.Parameters.AddWithValue("@RoleGUID", RoleGUID);
                    command.ExecuteNonQuery();

                }
                else // Number of roles checked is more than 1 (as in there are multiple roles checked)
                {   
                    command.CommandText = "DELETE FROM [aspnet_UsersInRoles] WHERE UserId=@userGUID"; // start clean, delete all associated data, and insert new.
                    command.Parameters.AddWithValue("@userGUID", userGUID);
                    command.ExecuteNonQuery();

                    string strRole = string.Empty;
                    command.Parameters.AddWithValue("@ListOfRoleGuid", strRole);
                    for (int i = 0; i < NumbOfRolesChecked; i++)
                    {
                        strRole = ListOfRoleGuid[i];
                        command.CommandText = "INSERT INTO [aspnet_UsersInRoles](UserId, RoleId) VALUES(@userGUID, @ListOfRoleGuid)";
                        command.Parameters["@ListOfRoleGuid"].Value = strRole;
                        command.ExecuteNonQuery();
                    }
                }
                // UPDATE/INSERT PROFILE DATA       
                if (count == 0) // INSERT
                {
                    command.CommandText = "INSERT INTO [aspnet_Profile](UserId, LastUpdatedDate, FirstName, LastName, Address1, Address2, ZipCode, City, State, RoleCityID, PhoneNo, [EV ID], Priority, SMERCID, SmartPhoneOS, PhoneServiceCarrier, SmartPhoneModelNo, [RTMCUserAccountTypeID],[RTMCUserAccountExpirationWindowID],[EVUserAccountTypeID],[EVUserAccountExpirationWindowID], [ChargingPoints],[RTMCReportIntervals], [RTMCChartAndReportType],[EVUserSessionTimeout],[RTMCSessionTimeout],[MaximumVehicles], [AllowTextMessage], [MonitorRefreshInterval]) " +
                            "VALUES(@userGUID ,@DateTime,@FirstName,@LastName,@Address1 ,@Address2, @ZipCode, @City, @State,@RoleCityGUID,@PhoneNo,@EVGUID, @Priority, @SMERCID, @SmartPhoneOS, @PhoneServiceCarrier, @SmartPhoneModelNo, @RTMCUserAccountTypeID, @RTMCUserAccountExpirationWindowID,@EVUserAccountTypeID, @EVUserAccountExpirationWindowID, @ChargingPoints, @RTMCReportIntervals, @RTMCChartAndReportType, @EVUserSessionTimeout, @RTMCSessionTimeout, @MaximumVehicles, @AllowTextMessage, @MonitorRefreshInterval) ";
                                      
                    command.Parameters.AddWithValue("@DateTime", DateTime.Now);                  
                    command.Parameters.AddWithValue("@FirstName", tbFirstName.Text);
                    command.Parameters.AddWithValue("@LastName", tbLastName.Text);
                    command.Parameters.AddWithValue("@Address1", tbAddress1.Text);
                    command.Parameters.AddWithValue("@Address2", tbAddress2.Text);
                    command.Parameters.AddWithValue("@City", tbCity.Text);
                    command.Parameters.AddWithValue("@State", ddlUserState.SelectedValue);
                    command.Parameters.AddWithValue("@ZipCode", tbZipCode.Text);
                    command.Parameters.AddWithValue("@RoleCityGUID", RoleCityGUID);
                    command.Parameters.AddWithValue("@EVGUID", EVGUID);
                    command.Parameters.AddWithValue("@PhoneNo", tbPhoneNumber.Text);
                    command.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);                    
                    command.Parameters.AddWithValue("@SmartPhoneOS", ddlSmartPhoneOS.SelectedIndex);
                    command.Parameters.AddWithValue("@PhoneServiceCarrier", ddlPhoneServiceCarrier.SelectedIndex);
                    command.Parameters.AddWithValue("@SmartPhoneModelNo", tb_SmartPhoneModelNo.Text);
                    command.Parameters.AddWithValue("@RTMCUserAccountTypeID", ddlRTMCUserAccountType.SelectedValue);
                    command.Parameters.AddWithValue("@RTMCUserAccountExpirationWindowID", ddlRTMCUserAccountExpirationWindow.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserAccountTypeID", ddlEVUserAccountType.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserAccountExpirationWindowID", ddlEVUserAccountExpirationWindow.SelectedValue);
                    command.Parameters.AddWithValue("@ChargingPoints", tbChargingPoints.Text);
                    command.Parameters.AddWithValue("@RTMCReportIntervals", ddlRTMCReportIntervals.SelectedValue);
                    command.Parameters.AddWithValue("@RTMCChartAndReportType", ddlRTMCChartAndReport.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserSessionTimeout", tbEVUserSessionTimeout.Text);
                    command.Parameters.AddWithValue("@RTMCSessionTimeout", tbRTMCSessionTimeout.Text);
                    command.Parameters.AddWithValue("@MaximumVehicles", ddlMaxVehicles.SelectedValue);
                    command.Parameters.AddWithValue("@AllowTextMessage", ddlAllowTextMsg.SelectedValue);
                    command.Parameters.AddWithValue("@MonitorRefreshInterval", ddlMonitorRefreshInterval.SelectedValue);

                    string strSmercID = string.Empty;
                    command.Parameters.AddWithValue("@SMERCID", strSmercID);

                    if (!string.IsNullOrEmpty(tbSMERCID.Text))
                    {
                        strSmercID = tbSMERCID.Text;
                        command.Parameters["@SMERCID"].Value = strSmercID;                        
                    }
                    else
                    {
                        strSmercID = tbUsername.Text;
                        command.Parameters["@SMERCID"].Value = strSmercID;     
                    }
                    command.ExecuteNonQuery();
                }
                if (count == 1)//if records exist, then update the records
                {
                    command.CommandText = "UPDATE [aspnet_Profile] SET [LastUpdatedDate]= @DateTime, [RoleCityID] =@RoleCityGUID, [EV ID]=@EVGUID ,"
                            + "[FirstName]= @FirstName, [LastName] = @LastName, [Address1] = @Address1 , [Address2] = @Address2, [City] = @City, [State] = @State, [PhoneNo] = @PhoneNo, [ZipCode] = @ZipCode, [Priority] = @Priority, [SMERCID] = @SMERCID, [SmartPhoneOS] = @SmartPhoneOS, [PhoneServiceCarrier] = @PhoneServiceCarrier, [SmartPhoneModelNo] = @SmartPhoneModelNo ,[RTMCUserAccountTypeID] = @RTMCUserAccountTypeID, [RTMCUserAccountExpirationWindowID] = @RTMCUserAccountExpirationWindowID, [EVUserAccountTypeID] = @EVUserAccountTypeID, [EVUserAccountExpirationWindowID] = @EVUserAccountExpirationWindowID, [ChargingPoints] = @ChargingPoints, [RTMCReportIntervals] = @RTMCReportIntervals, [RTMCChartAndReportType] = @RTMCChartAndReportType, [EVUserSessionTimeout] = @EVUserSessionTimeout,[RTMCSessionTimeout] = @RTMCSessionTimeout, [MaximumVehicles] = @MaximumVehicles, [AllowTextMessage] = @AllowTextMessage, [MonitorRefreshInterval] = @MonitorRefreshInterval "
                            + " WHERE [UserId] = @userGUID"; 

                    command.Parameters.AddWithValue("@DateTime", DateTime.Now);                   
                    command.Parameters.AddWithValue("@FirstName", tbFirstName.Text);
                    command.Parameters.AddWithValue("@LastName", tbLastName.Text);
                    command.Parameters.AddWithValue("@Address1", tbAddress1.Text);
                    command.Parameters.AddWithValue("@Address2", tbAddress2.Text);
                    command.Parameters.AddWithValue("@City", tbCity.Text);
                    command.Parameters.AddWithValue("@State", ddlUserState.SelectedValue);
                    command.Parameters.AddWithValue("@ZipCode", tbZipCode.Text);
                    command.Parameters.AddWithValue("@PhoneNo", tbPhoneNumber.Text);
                    command.Parameters.AddWithValue("@RoleCityGUID", RoleCityGUID);
                    command.Parameters.AddWithValue("@EVGUID", EVGUID);                    
                    command.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);
                    command.Parameters.AddWithValue("@SmartPhoneOS", ddlSmartPhoneOS.SelectedIndex);
                    command.Parameters.AddWithValue("@PhoneServiceCarrier", ddlPhoneServiceCarrier.SelectedIndex);
                    command.Parameters.AddWithValue("@SmartPhoneModelNo", tb_SmartPhoneModelNo.Text);
                    command.Parameters.AddWithValue("@RTMCUserAccountTypeID", ddlRTMCUserAccountType.SelectedValue);
                    command.Parameters.AddWithValue("@RTMCUserAccountExpirationWindowID", ddlRTMCUserAccountExpirationWindow.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserAccountTypeID", ddlEVUserAccountType.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserAccountExpirationWindowID", ddlEVUserAccountExpirationWindow.SelectedValue);
                    command.Parameters.AddWithValue("@ChargingPoints", tbChargingPoints.Text);
                    command.Parameters.AddWithValue("@RTMCReportIntervals", ddlRTMCReportIntervals.SelectedValue);
                    command.Parameters.AddWithValue("@RTMCChartAndReportType", ddlRTMCChartAndReport.SelectedValue);
                    command.Parameters.AddWithValue("@EVUserSessionTimeout", tbEVUserSessionTimeout.Text);
                    command.Parameters.AddWithValue("@RTMCSessionTimeout", tbRTMCSessionTimeout.Text);
                    command.Parameters.AddWithValue("@MaximumVehicles", ddlMaxVehicles.SelectedValue);
                    command.Parameters.AddWithValue("@AllowTextMessage", ddlAllowTextMsg.SelectedValue);
                    command.Parameters.AddWithValue("@MonitorRefreshInterval", ddlMonitorRefreshInterval.SelectedValue);

                    command.Parameters.AddWithValue("@SMERCID", tbSMERCID.Text);  // Possible BUG CHECK OVER THIS!!!                  
                    command.ExecuteNonQuery();                    
                }
                // UPDATE MEMBERSHIP

                command.CommandText = "UPDATE [aspnet_Membership] SET [Email]= @TB_Email , [LoweredEmail] = @TB_EmailLowered, [IsApproved]= @ddlIsApprovedValue, [IsLockedOut] =@ddlIsLockedOutValue, [PasswordQuestion] = @TB_PassQuestion, [PasswordAnswer] = @TB_PassAnswer WHERE [UserId] =@userGUID";
                
                command.Parameters.AddWithValue("@TB_Email", tbEmail.Text);
                command.Parameters.AddWithValue("@TB_EmailLowered", tbEmail.Text.ToLower());
                command.Parameters.AddWithValue("@TB_PassQuestion", tbPassQuestion.Text);
                command.Parameters.AddWithValue("@TB_PassAnswer", tbPassAnswer.Text);
                command.Parameters.AddWithValue("@ddlIsApprovedValue", ddlIsApproved.SelectedValue);
                command.Parameters.AddWithValue("@ddlIsLockedOutValue", ddlIsLockedOut.SelectedValue);
                             
                command.ExecuteNonQuery();
                
                // UPDATE USERS
      
                command.CommandText="UPDATE [aspnet_Users] SET [Activate] =@ddlIsActivatedSelectedValue  WHERE [UserId] = @userGUID";
                command.Parameters.AddWithValue("@ddlIsActivatedSelectedValue", ddlIsActivated.SelectedValue);
                command.ExecuteNonQuery();
                
                if (USERname != string.Empty)
                {
                    if (tbUsername.Text != USERname)
                    {
                        command.CommandText= "UPDATE [aspnet_Users] SET [UserName] = @TB_Username, [LoweredUserName]= @TB_UsernameLowered  WHERE [UserId] =@userGUID";
                        
                        command.Parameters.AddWithValue("@TB_Username", tbUsername.Text);
                        command.Parameters.AddWithValue("@TB_UsernameLowered", tbUsername.Text.ToLower());
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                ShowMessage("Error Updating Roles Table.  All database queries reversed.  Error Message: " + ex.Message);
                blnPasses = false;
                blnFuncPass = false;    
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    ShowMessage("Transaction Rollback error: " + ex2.Message);
                }

            }
            finally
            {
                if(cnn!=null)
                    cnn.Close();
            }

            MembershipUser CurrentUser = Membership.GetUser(CurrentUserName);

            //Change Password
            if (CheckForPasswordEntry != string.Empty && blnPasses)
            {
                if (CheckForPasswordEntry != "Encrypted")
                {
                    if (CheckForPasswordEntry != TxtBox_Password.Text)
                    {
                        CurrentUser.ChangePassword(CheckForPasswordEntry, TxtBox_Password.Text);
                    }
                }
            }

            // IF ROLLBACK, DELETE THE NEW USER FROM THE DATABASE
            if (blnNewInsert && !blnPasses) // If this function was called for a new insert, and the full insert did not complete, then delete the user
            {
                Membership.DeleteUser(tbUsername.Text);
            }

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
                ListOfAdminCities = FindAssociatedRoles(ListOfRoles); // ListOfAdminCities is a list of user city that have admin roles
                bool isAdministrator = IsUserAdministrator(ListOfRoles);
                LockOutFeatures(ListOfAdminCities, isAdministrator); // Lock out parts of the code depending on the user's role privelages                    
            }

            if (blnFuncPass && isNew)
            {
                insertPrimaryEvToUserEvList(userGUID, EVGUID);
            }

            return blnFuncPass;
        }


        protected bool UpdateUserRepeatVerification(string USERname) // true = pass, false = fail // For Update Function
        {
            bool smercId = false;
            bool pass = true;
            if (tbUsername.Text.IndexOf("'") != -1)
            {
                UserNameVerifyText.Text = "No quotation marks allowed in username";
                pass = false;
                smercId = true;
            }            
            
            int numbOfUsers = 0;
            List<string> Username = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT COUNT(*) FROM [aspnet_Users]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                numbOfUsers = int.Parse(dt.Rows[0][0].ToString());

                strQuery = "SELECT [Username] FROM [aspnet_Users]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < numbOfUsers; i++)
                {
                    Username.Add(dt.Rows[i][0].ToString());
                }
                Username.Remove(USERname);
                if (Username.Contains(tbUsername.Text))
                {
                    pass = false;
                    UserNameVerifyText.Text = "<br>Username already in database.  Choose a new one.";
                }

                if (!smercId)
                {
                    if (!string.IsNullOrEmpty(tbSMERCID.Text))
                    {
                        strQuery = "SELECT [SMERCID], [UserId] FROM [aspnet_Profile]";
                        cmd = new SqlCommand(strQuery, cnn);
                        cmd.CommandType = CommandType.Text;
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);

                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if (tbSMERCID.Text == dt.Rows[j][0].ToString())
                            {
                                if (dt.Rows[j][1].ToString() != ReturnUserGUIDfromUsername(USERname)) // if userguid is not equal to current userguid, we know that 
                                {
                                    LBL_SMERCID_Verify.Text = "<br> Repeat SMERCID.  Enter a unique SMERCID";
                                    pass = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                ErrorMessage.Text += "<br> Error In Create User1 " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            string userName1 = gvRow.Cells[findGVcolumn("User Name")].Text;

            if (userName1 == User.Identity.Name)
            {
                if (tbUsername.Text != User.Identity.Name)
                {
                    UserNameVerifyText.Text = "You cannot change your own username";
                    pass = false;
                }
            }

            if (tbSMERCID.Text == "")
            {
                LBL_SMERCID_Verify.Text = "Empty SMERC ID is not allowed.";
                pass = false;
            }
            if (tbUsername.Text == "")
            {
                UserNameVerifyText.Text = "Username Required";
                pass = false;
            }
            if (tbUsername.Text.ToString().IndexOf(".") != -1)
            {
                UserNameVerifyText.Text = "Invalid Username.   Remove Period.";
                pass = false;
            }
            if(0!=string.Compare(TxtBox_Password.Text, TxtBox_PasswordRepeat.Text))
            {
                PasswordVerifyLabel.Text = " You must enter the same password twice";
                pass = false;
            }
            if (TxtBox_Password.Text != TxtBox_PasswordRepeat.Text)
            {
                PasswordVerifyLabel.Text = " Passwords must be the same!";
                pass = false;
            }
            if (tbEmail.Text == "")
            {
                lblEmailVerify.Text = "Email Required";
                pass = false;
            }

            if (ddlRoleArea.SelectedIndex == 0)
            {
                City_Verify_label.Text = "City Required";
                pass = false;
            }

            if (ddlEvModelList.SelectedIndex == 0)
            {
                lblEvVerify.Text = "EV Required";
                pass = false;
            }
            if (ddlRTMCUserAccountType.SelectedIndex == 0)
            {
                lblRTMCUserAccountTypeVerify.Text = "Required";
                pass = false;
            }
            if (ddlRTMCUserAccountExpirationWindow.SelectedIndex == 0)
            {
                lblRTMCUserAccountExpirationWindow.Text = "Required";
                pass = false;
            }
            if (ddlEVUserAccountType.SelectedIndex == 0)
            {
                lblEVUserAccountType.Text = "Required";
                pass = false;
            }
            if (ddlRTMCChartAndReport.SelectedIndex == 0)
            {
                lblRTMCChartAndReport.Text = "Required";
            }
            if (ddlEVUserAccountExpirationWindow.SelectedIndex == 0)
            {
                lblEVUserAccountExpirationWindow.Text = "Required";
                pass = false;
            }
            if (tbCity.Text == string.Empty)
            {
                lblCityVerify.Text = "City Required";
                pass = false;
            }

            int count = 0;
            for (int i = 0; i < cblRoleName.Items.Count; i++)
            {
                if (cblRoleName.Items[i].Selected)
                    count++;
            }

            if (count == 0)
            {
                lblRoleNameVerify.Text = "You must select atleast one role name";
                pass = false;
            }

            if (!pass)
            {                
                ErrorMessage.Text += "<br> Error when attempting to update user.  Check below";
                return false;
            }
            return true;
        }


        #endregion
        #region HiderFunctions, Control Functions

        protected void clearAllText()
        {
            HideAll();
            HideAllTextBoxes();
            HideAllVerificationText();
            GV_UserEditor.SelectedIndex = -1; // Reset selecter   
            ddlEvModelList.Enabled = true;
            clearGvEvList();
        }

        protected void HideAllVerificationText()
        {
         //   ErrorMessage.Text = string.Empty;
            UpdateErrorLabel.Text = string.Empty;
            Message1.Text = string.Empty;
            UserNameVerifyText.Text = string.Empty;
            PasswordVerifyLabel.Text = string.Empty;
            RepeatPasswordVerifyLabel.Text = string.Empty;
            lblEmailVerify.Text = string.Empty;
            lblActivateVerify.Text = string.Empty;
            City_Verify_label.Text = string.Empty;
            lblEvVerify.Text = string.Empty;
            lblRTMCUserAccountTypeVerify.Text = string.Empty;
            lblRTMCUserAccountExpirationWindow.Text = string.Empty;
            lblEVUserAccountType.Text = string.Empty;
            lblRTMCChartAndReport.Text = string.Empty;
            lblEVUserAccountExpirationWindow.Text = string.Empty;
            lblRoleNameVerify.Text = string.Empty;
            lblIsLockedOutVerify.Text = string.Empty;
            lblIsApproved.Text = string.Empty;

            // Addition 11-15-2012 - dhk
            LBL_SMERCID_Verify.Text = string.Empty;
            lblCityVerify.Text = string.Empty;
            lblUserStateVerify.Text = string.Empty;



        }
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (ctrlname != null && ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }

        protected void HideAll()
        {
       //     ErrorMessage.Text = string.Empty;
            Message1.Text = string.Empty;
            btnUpdate.Visible = false;
            btnResetPass.Visible = false;

            HideAllTextBoxes();
        }

        protected void HideAllTextBoxes()
        {
            tbUsername.Text = string.Empty;
            tbFirstName.Text = string.Empty;
            tbLastName.Text = string.Empty;
            tbPhoneNumber.Text = string.Empty;
            TxtBox_Password.Text = string.Empty;
            TxtBox_PasswordRepeat.Text = string.Empty ;
            tbAddress1.Text = string.Empty;
            tbAddress2.Text = string.Empty;
            tbZipCode.Text = string.Empty;
            tbCity.Text = string.Empty;
           // ddlUserState.ClearSelection();
            ddlUserState.SelectedIndex = 4;

            tbPassAnswer.Text = string.Empty;
            tbPassQuestion.Text = string.Empty;
            TxtBox_Password.Attributes.Add("value", string.Empty);
            TxtBox_PasswordRepeat.Attributes.Add("value", string.Empty);
      //      ddlIsActivated.ClearSelection();
            ddlIsActivated.SelectedIndex = 0;
            ddlEvModelList.ClearSelection();

            ddlPriority.ClearSelection();
            tbEmail.Text = string.Empty;
           // ddlIsApproved.ClearSelection();
            ddlIsApproved.SelectedIndex = 0;
          //  ddlIsLockedOut.ClearSelection();
            ddlIsLockedOut.SelectedIndex = 1; // Set to False by default

            //Addition 11-14-12 - dhk
            ddlPhoneServiceCarrier.ClearSelection();
            ddlEVUserAccountExpirationWindow.SelectedIndex = 0;
            ddlEVUserAccountType.SelectedIndex = 0;
            ddlRTMCChartAndReport.SelectedIndex = 0;
            ddlRTMCUserAccountExpirationWindow.SelectedIndex = 0;
            ddlRTMCUserAccountType.SelectedIndex = 0;
            ddlSmartPhoneOS.ClearSelection();
            tb_SmartPhoneModelNo.Text = string.Empty;
            tbSMERCID.Text = string.Empty;
            tbChargingPoints.Text = "0";
            tbEVUserSessionTimeout.Text = "1";
            tbRTMCSessionTimeout.Text = "1";
            ddlMaxVehicles.SelectedIndex = 0;
            ddlAllowTextMsg.SelectedIndex = 0;
            ddlMonitorRefreshInterval.SelectedValue = "300";
            //DDL_UserCity.ClearSelection();
            // end additions

            List<string> ListOfAdminCities = new List<string>();
            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            ListOfAdminCities = FindAssociatedRoles(ListOfRoles); // ListOfAdminCities is a list of user city that have admin roles
            bool isAdministrator = IsUserAdministrator(ListOfRoles);
            bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);
            if (!isAdministrator || ListOfAdminCities.IndexOf("General") == -1) // General is not found
            {
                //ddlRoleArea.Enabled = true;
                ddlRTMCChartAndReport.SelectedValue = "1";
                ddlEVUserAccountType.SelectedValue = "0";
                ddlRTMCUserAccountType.SelectedValue = "1";
            }
            else
            {
                ddlRTMCReportIntervals.ClearSelection();
                ddlRoleArea.ClearSelection();
            }

            ClearcblRoleNameList();
            PopulateThecblRoleName(ddlSelectByCity.SelectedItem.Text, isGeneralAdmin);
        }

        protected void ClearcblRoleNameList()
        {
            for (int i = 0; i < cblRoleName.Items.Count; i++)
            {
                cblRoleName.Items[i].Selected = false;
            }
        }

        #endregion             
        #region gridview fns
        protected void GV_UserEditor_rowCreated(object sender, GridViewRowEventArgs e)
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

        protected void GvEvListRowCreated(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < EvListColumnsToHide.Count(); i++)
            {
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    e.Row.Cells[findGVcolumn(EvListColumnsToHide[i], GvEvList)].Visible = false;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Cells[findGVcolumn(EvListColumnsToHide[i], GvEvList)].Visible = false;
                }
            }
        }

        #endregion
        protected void ddlRoleArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            bool isGeneralAdmin = UserRoleIsGeneralAdmin(ListOfRoles);

            if(ddlRoleArea.SelectedIndex !=0)
                PopulateThecblRoleName(ddlRoleArea.SelectedItem.Text, isGeneralAdmin);
            else
                PopulateThecblRoleName("All Users", isGeneralAdmin);
        }      

        
        #region Populate Functions(DDLs, Check box, random variables)

        protected string strReturnGUIDfromSelectedCity(string strSelectedCity)
        {
            string strReturnGuid = string.Empty;

            for (int i = 0; i < ddlSelectByCity.Items.Count; i++)
            {
                if (strSelectedCity == ddlSelectByCity.Items[i].Text)
                {
                    strReturnGuid = ddlSelectByCity.Items[i].Value;
                    break;
                }
            }
            return strReturnGuid;
        }

        protected List<string> listReturnOrganizations(string strOrganizationGUID)
        {
            List<string> listOrganizationNames = new List<string>();
            if (strOrganizationGUID != "-1")
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                try
                {
                    strQuery = " SELECT [Name] FROM City " +
                               " WHERE ID ='" + strOrganizationGUID + "'";
                    //" Where ID = 'b033893c-a86d-4c87-a134-f2a155b4e2aa'";

                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count != 0)
                    {
                        listOrganizationNames.Add(dt.Rows[0][0].ToString());
                    }
                    else // If dt.rows.count == 0, then the GUID represented a combinated city, thus search the combinated table.
                    {
                        strQuery = " SELECT [CombinatedCityID], [MainCityID] " +
                                   " FROM [CombinatedCity] " +
                                   " WHERE [ID] = '" + strOrganizationGUID + "'";

                        cmd = new SqlCommand(strQuery, cnn);
                        cmd.CommandType = CommandType.Text;
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);

                        string strMainCityGUID = dt.Rows[0][1].ToString();

                        listOrganizationNames.Add(ObtainUserCityFromGUID(strMainCityGUID));

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            listOrganizationNames.Add(ObtainUserCityFromGUID(dt.Rows[i][0].ToString()));
                        }
                    }

                    cmd.Dispose();
                    da.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at listReturnOrganizations: " + ex.Message);
                }
                finally
                {
                    if (cnn != null)
                        cnn.Close();
                }
            }
            return listOrganizationNames;
        }


        protected void PopulateThecblRoleName(string SelectedCity, bool isGeneralAdmin) // Fill in the Rolename Checkbox, given the selected city
        {
            //lblTest.Text = strReturnGUIDfromSelectedCity(SelectedCity) + "  This is the guid for: " + SelectedCity;

            if (SelectedCity == "All Users") // If null city is entered, just Select the default roles.
            {
                PopulatecblRoleName(isGeneralAdmin);
            }
            else
            {
                string strRoleAreaGUID = strReturnGUIDfromSelectedCity(SelectedCity);
                //tbEVUserSessionTimeout.Text = SelectedCity;
                List<string> ListRoleArea = listReturnOrganizations(strRoleAreaGUID);

                List<string> ListDataSource = new List<string>();

                for (int i = 0; i < ListRoleArea.Count; i++)
                {
                    ListDataSource.AddRange(ReturnRoleNames(ListRoleArea[i], isGeneralAdmin));

                }
                cblRoleName.DataSource = ListDataSource; // Meow
                cblRoleName.DataBind();
                //lblTest.Text = string.Empty;
                //foreach (string Cat in ListRoleArea)
                //{
                //    lblTest.Text += " " + Cat;
                //}
            }            
            //else
            //{
            //    List<string> RoleAreas = ReturnListOfRoleArea();
            //    int numbOfRoleAreas = RoleAreas.Count; // numbOfRoleAreas is the number of Role Areas in our environment

            //    int[] ContainRole = new int[numbOfRoleAreas]; // i.e.  { 0, 0 , 0 ,0 ,0, 0} etc, where 0 = false, 1 = true

            //    foreach (int check in ContainRole) // Set all index values to 0 (false)
            //    {
            //        ContainRole[check] = 0;
            //    }
            //    List<int> ContainingRoles = new List<int>(); // basically an Array of 1s and 0s to specify which roles are selected
                
            //    bool blnIsCombinedArea = ddlSelectByCity.SelectedIndex > numbOfRoleAreas;

            //    for (int i = 0; i < numbOfRoleAreas; i++)
            //    {
            //       if (SelectedCity.IndexOf(RoleAreas[i]) != -1) // 13-3-27a - check for possible error here "ERRORCHECK"                   
            //        {
            //            if (!blnIsCombinedArea)
            //            {
            //                if (SelectedCity == RoleAreas[i])
            //                    ContainRole[i] = 1;
            //            }
            //            else
            //            {
            //                ContainRole[i] = 1;
            //            }
            //           //if (SelectedCity.Length > RoleAreas[i].Length)
            //           //     CombinedCity_Selected = true;
            //        }
            //    }

            //    if (!blnIsCombinedArea)
            //    {
            //        List<string> ListOfRoleNames = ReturnRoleNames(SelectedCity);
            //        cblRoleName.DataSource = ListOfRoleNames; // Populate cblRoleName
            //        cblRoleName.DataBind();
            //    }
            //    else // if(CombinedCity_Selected)
            //    {
            //        List<string> ListOfRoleNames = new List<string>();

            //        for (int j = 0; j < numbOfRoleAreas; j++)
            //        {
            //            if (ContainRole[j] == 1)
            //                ListOfRoleNames.AddRange(ReturnRoleNames(RoleAreas[j]));
            //        }

            //        cblRoleName.DataSource = ListOfRoleNames; // Populate cblRoleName
            //        cblRoleName.DataBind();
            //    }
            //}
        }

        protected void PopulateddlPriority()
        {
            List<string> Numbers = new List<string>();
            Numbers.Add("1");
            Numbers.Add("2");
            Numbers.Add("3");
            Numbers.Add("4");
            Numbers.Add("5");
            Numbers.Add("6");
            Numbers.Add("7");
            Numbers.Add("8");
            Numbers.Add("9");
            Numbers.Add("10");
            ddlPriority.DataSource = Numbers;
            ddlPriority.DataBind();
        }
        protected void PopulateddlRTMCReportIntervals()
        {
            //ddlRTMCReportIntervals.Items.AddRange(Enumerable.Range(0, 101).Select(e => new ListItem(e.ToString())).ToArray());
            List<string> Numbers = new List<string>();
            for (int i = 0; i < 101; i++)
            {
                Numbers.Add(i.ToString());
            }
            ddlRTMCReportIntervals.DataSource = Numbers;
            ddlRTMCReportIntervals.DataBind();
        }
        protected void AddInGeneralCity() // Check the City table.  If there is no "General", then add it.
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT count(*) FROM [City] WHERE Name = 'General'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                int count = int.Parse(dt.Rows[0][0].ToString());

                if (count == 0)
                {
                    Guid GeneralGUID = System.Guid.NewGuid();
                    Guid GeneralAdmin = System.Guid.NewGuid();
                    Guid GeneralMainter = System.Guid.NewGuid();
                    Guid GeneralOperator = System.Guid.NewGuid();
                    Guid GeneralUser = System.Guid.NewGuid();

                    strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = 'General Administrator'";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    GeneralAdmin = Guid.Parse(dt.Rows[0][0].ToString());

                    strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = 'General Maintainer'";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    GeneralMainter = Guid.Parse(dt.Rows[0][0].ToString());

                    strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = 'General Operator'";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    GeneralOperator = Guid.Parse(dt.Rows[0][0].ToString());

                    strQuery = "SELECT RoleId FROM [aspnet_Roles] WHERE RoleName = 'General User'";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();
                    da.Fill(dt);
                    GeneralUser = Guid.Parse(dt.Rows[0][0].ToString());

                    strQuery = "INSERT INTO [City]([ID],[Name],[State],[Latitude],[Longitude],[Activate], [Administrator Role ID], [Maintainer Role ID], [Operator Role ID],[User Role ID]) VALUES('"
                        + GeneralGUID + "','General', 'CA', '0','0','1','" + GeneralAdmin + "','" + GeneralMainter + "','" + GeneralOperator + "','" + GeneralUser + "')";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    if (da != null)
                        da.Dispose();
                }
            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when populating City MODEL with General " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
        }
        protected void PopulateEVMODEL() // Check the EV MODEL table.  IF there is no records, then add the Nissan Leaf and Chevorlet Volt so the program can run
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT count(*) FROM [EV Model]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                int count = int.Parse(dt.Rows[0][0].ToString());

                if (count == 0)
                {
                    Guid LeafGUID = System.Guid.NewGuid();
                    Guid VoltGUID = System.Guid.NewGuid();

                    strQuery = "INSERT INTO [EV Model]([ID],[Manufacturer],[Model],[Charging Current],[Charging Voltage]) VALUES('" + LeafGUID + "','Nissan', 'Leaf', null,null)";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    strQuery = "INSERT INTO [EV Model]([ID],[Manufacturer],[Model],[Charging Current],[Charging Voltage]) VALUES('" + VoltGUID + "','Chevorlet', 'Volt', null,null)";
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }

                da.Dispose();
               
            }
            catch (Exception ex)
            {
                ShowMessage("Error when populating EV MODEL " + ex.Message);
                
            }
            finally
            {
                cnn.Close();
            }
        }
        protected void PopulateddlIsApproved()
        {
            List<string> ApprovedList = new List<string>();
            ApprovedList.Add("True");
            ApprovedList.Add("False");
            ddlIsApproved.DataSource = ApprovedList;
            ddlIsApproved.DataBind();
        }
        protected void PopulateddlIsLockedOut()
        {
            List<string> TrueFalse = new List<string>();
            TrueFalse.Add("True");
            TrueFalse.Add("False");
            ddlIsLockedOut.DataSource = TrueFalse;
            ddlIsLockedOut.DataBind();

            ddlIsLockedOut.SelectedIndex = 1; // Defaults to "False"
        }
        protected void PopulateddlIsActivated()
        {
            List<string> TrueFalse = new List<string>();
            TrueFalse.Add("True");
            TrueFalse.Add("False");
            ddlIsActivated.DataSource = TrueFalse;
            ddlIsActivated.DataBind();
        }

        // Populate DDL_smartPhonesOS and WirelessServiceCarrier addition on 11-14-2012 - dhk

        protected void Populate_ddlSmartPhoneOS() // Check the EV MODEL table.  IF there is no records, then add the Nissan Leaf and Chevorlet Volt so the program can run
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [OS Name] FROM [SmartPhoneOS]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                ddlSmartPhoneOS.DataSource = dt;
                ddlSmartPhoneOS.DataValueField = "OS Name";
                ddlSmartPhoneOS.DataTextField = "OS Name";
                ddlSmartPhoneOS.DataBind();

                cmd.Dispose();
                da.Dispose();
                
            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when populating ddlSmartPhoneOS " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
        }

        protected void Populate_ddlPhoneServiceCarrier() // Check the EV MODEL table.  IF there is no records, then add the Nissan Leaf and Chevorlet Volt so the program can run
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT [ProviderName] FROM [WirelessServiceCarrier]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                ddlPhoneServiceCarrier.DataSource = dt;
                ddlPhoneServiceCarrier.DataValueField = "ProviderName";
                ddlPhoneServiceCarrier.DataTextField = "ProviderName";
                ddlPhoneServiceCarrier.DataBind();

                da.Dispose();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when populating ddlPhoneServiceCarrier " + ex.Message;
            }
            finally
            {
                cnn.Close();
            }
        }

        protected void PopulateddlEVUserAccountType()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
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
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlEVUserAccountType.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text += "<br> PopulateddlEVUserAccount Error: " + ex.Message;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void PopulateddlRTMCChartAndReport()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
                strQuery = "SELECT * FROM [ChartAndReportType] ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlRTMCChartAndReport.DataSource = dt;
                ddlRTMCChartAndReport.DataValueField = "ID";
                ddlRTMCChartAndReport.DataTextField = "Type";
                ddlRTMCChartAndReport.DataBind();
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlRTMCChartAndReport.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text += "<br> PopulateddlRTMCChartAndReport Error: " + ex.Message;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void PopulateddlEVUserAccountExpirationWindow()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
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
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlEVUserAccountExpirationWindow.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text += "<br> PopulateddlEVUserAccountExpirationWindow Error: " + ex.Message;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void PopulateddlRTMCUserAccountType()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
                strQuery = "SELECT * FROM [RTMCUserAccountType] ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlRTMCUserAccountType.DataSource = dt;
                ddlRTMCUserAccountType.DataValueField = "ID";
                ddlRTMCUserAccountType.DataTextField = "AccountType";
                ddlRTMCUserAccountType.DataBind();
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlRTMCUserAccountType.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text += "<br> PopulateddlRTMUserAccountExpirationWindow Error: " + ex.Message;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void PopulateddlRTMCUserAccountExpirationWindow()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
                strQuery = "SELECT * FROM [RTMCUserAccountExpirationWindow] ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlRTMCUserAccountExpirationWindow.DataSource = dt;
                ddlRTMCUserAccountExpirationWindow.DataValueField = "ID";
                ddlRTMCUserAccountExpirationWindow.DataTextField = "ExpirationWindow";
                ddlRTMCUserAccountExpirationWindow.DataBind();
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlRTMCUserAccountExpirationWindow.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text += "<br> PopulateddlRTMCUserAccountExpirationWindow Error: " + ex.Message;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void PopulateddlUserState() // This function fills in the apropriate cities for the admin roles.
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT State FROM [State]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                ddlUserState.DataSource = dt;
                ddlUserState.DataValueField = "State";
                ddlUserState.DataTextField = "State";
                ddlUserState.DataBind();

                cmd.Dispose();
                da.Dispose();

            }

            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when Populating User State DDL" + ex.Message;
            }
            finally
            {
                cnn.Close();
                ddlUserState.SelectedIndex = 4; // Default chosen to "CA" - California
            }
        }

        protected void PopulateDDL_UserCity() // This function fills in the apropriate cities for the admin roles.
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                cnn.Open();
                strQuery = "SELECT NAME FROM [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);


                //DDL_UserCity.DataSource = dt;
              //  DDL_UserCity.DataValueField = "Name";
              //  DDL_UserCity.DataTextField = "Name";
             //   DDL_UserCity.DataBind();

                ListItem li2 = new ListItem("Select...");
                da.Dispose();
                cmd.Dispose();
            //    DDL_UserCity.Items.Insert(0, li2);
            }

            catch (Exception ex)
            {
                
                ErrorMessage.Text += "<br> Error when Populating City DDL" + ex.Message;
            }
            finally
            {
                cnn.Close();
            }        
        }

        protected void PopulateddlRoleArea(List<string> ListOfAdminCities) // This function fills in the apropriate cities for the admin roles.
        {
            List<ComboCityAndGuidClass> ComboGuid = ReturnUniqueCombinedGUID();
            if (ListOfAdminCities.Count <= 1)
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;
                DataTable dt2 = null;
                SqlDataAdapter da2;

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

                    da2 = new SqlDataAdapter();
                    da2.SelectCommand = cmd;
                    dt2 = new DataTable();
                    da2.Fill(dt2);

                    ddlRoleArea.DataSource = dt;
                    ddlRoleArea.DataValueField = "ID";
                    ddlRoleArea.DataTextField = "Name";
                    ddlRoleArea.DataBind();
                    

                    ddlSelectByCity.DataSource = dt2;
                    ddlSelectByCity.DataValueField = "ID";
                    ddlSelectByCity.DataTextField = "Name";
                    ddlSelectByCity.DataBind();

                    ListItem li2 = new ListItem("All Users", "-1");
                    ListItem li1 = new ListItem("Select...", "-1");

                    ddlRoleArea.Items.Insert(0, li1);
                    ddlSelectByCity.Items.Insert(0, li2);

                    int ddlRoleAreaSize = ddlRoleArea.Items.Count;                   

                    if (ComboGuid != null)
                    {
                        ListItem ComboCityList = new ListItem();
                        ListItem ComboCityList2 = new ListItem();
                        for (int i = 0; i < ComboGuid.Count; i++) // Add
                        {                           
                            ComboCityList = new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid);
                            ComboCityList2 = new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid);

                            ddlRoleArea.Items.Insert(ddlRoleAreaSize + i, ComboCityList);
                            ddlSelectByCity.Items.Insert(ddlRoleAreaSize + i, ComboCityList2);
                        }
                    }

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {                    
                    ErrorMessage.Text += "<br> Error when Populating City DDL" + ex.Message;
                }
                finally
                {
                    cnn.Close();
                }
            }
            else // if ListOfAdminCities >1 (as in the roles are combinated cities)
            {
                ddlRoleArea.Items.Clear();
                ddlSelectByCity.Items.Clear();

                List<string> CopyOfList = new List<string>(ListOfAdminCities);
                ListItem li2 = new ListItem("All Users", "-1");
                ListItem li1 = new ListItem("Select...", "-1");

                ddlRoleArea.Items.Insert(0, li1);
                ddlSelectByCity.Items.Insert(0, li2); // Insert the Select

                ListItem liCityandguid; // = new ListItem("All Users", "-1");
                ListItem liCityandguid2;
                for (int i = 0; i < ListOfAdminCities.Count; i++)
                {
                    liCityandguid = new ListItem(ListOfAdminCities[i], ObtainCityGUIDfromUserCity(ListOfAdminCities[i]));
                    liCityandguid2 = new ListItem(ListOfAdminCities[i], ObtainCityGUIDfromUserCity(ListOfAdminCities[i]));
                    ddlSelectByCity.Items.Add(liCityandguid);
                    ddlRoleArea.Items.Add(liCityandguid2);
                }

                string UserName = ReturnUserGUIDfromUsername(Page.User.Identity.Name);
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;
                string ComboCity = string.Empty;;

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
                    
                    ErrorMessage.Text += "<br> Error while entering Combo City ID";
                }
                finally
                {
                    cnn.Close();
                }
                if (ComboGuid != null)
                {
                    for (int i = 0; i < ComboGuid.Count; i++)
                    {
                        if (ComboCity == ComboGuid[i].Guid)
                        {
                            ddlRoleArea.Items.Add(new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid));
                            ddlSelectByCity.Items.Add(new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid));                   
                        }
                    }
                }
            }
        }

        protected void PopulateEVDDL()
        {
            List<string> EVLIST = returnListofEvCars();
            ddlEvModelList.DataSource = EVLIST;
            ddlEvModelList.DataBind();
            ListItem li = new ListItem("Select...", "-1");
            ddlEvModelList.Items.Insert(0, li);
        }
        protected void PopulatecblRoleName(bool isGeneralAdmin)
        {
            List<string> ListOfRoleNames = ReturnRoleNames(isGeneralAdmin);
            cblRoleName.DataSource = ListOfRoleNames;
            cblRoleName.DataBind();
        }
    #endregion  

        protected void cblRoleName_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> GeneralCityCheck = new List<string>();
            int GeneralSize = 0;
            for (int i = 0; i < cblRoleName.Items.Count; i++)
            {
                if (cblRoleName.Items[i].Selected)
                {
                    if (cblRoleName.Items[i].Value.IndexOf("General") != -1)
                    {
                        GeneralSize++;
                    }
                }
                if (GeneralSize > 1) // If there is more than one general check list checked,
                {
                    PopUpError("You may only choose one General role");
                    foreach (ListItem li in cblRoleName.Items)
                    {
                        li.Selected = false;
                    }
                }
            }
        }

        protected void PopulateddlMaxVehicles()
        {
            ddlMaxVehicles.Items.AddRange(Enumerable.Range(1, 256).Select(e => new ListItem(e.ToString())).ToArray());
        }

        protected void BTN_HideError_Click(object sender, EventArgs e)
        {
            ErrorMessage.Text = string.Empty;
            
        }

        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            //showAllFunction();
            List<string> ListOfAdminCities = new List<string>();

            ListOfAdminCities = ReturnSelectedCities();
            PopulateGridview(ListOfAdminCities, cbShowActivated.Checked);
        }
        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < GV_UserEditor.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (GV_UserEditor.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void PopulateddlGvEvListModel()
        {
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    ddlGvEvListModel.Visible = true;
                    tbNickname.Visible = true;
                    string strQuery = "SELECT ID, Manufacturer+' '+Model AS [EV Info] FROM [EV Model] ORDER BY [EV Info]";
                    string strListItemInsert = "Select...";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    DataTable DT = new DataTable();
                    SqlDataAdapter DA = new SqlDataAdapter();
                    DA.SelectCommand = cmd;
                    DA.Fill(DT);
                    ddlGvEvListModel.DataSource = DT;
                    ddlGvEvListModel.DataValueField = "ID";
                    ddlGvEvListModel.DataTextField = "EV Info";
                    ddlGvEvListModel.DataBind();
                    ListItem l1 = new ListItem(strListItemInsert, "-1");
                    ddlGvEvListModel.Items.Insert(0, l1);
                    DA.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at populateEvInfo: " + ex.Message);
                }
            }
        }

        protected void PopulateEvList(string userId)
        {
            var DT = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            {
                var sqlQuery =
                    "SELECT list.ID, list.EvModelID, Manufacturer+' '+Model AS [EVName], list.Nickname " +
                    "FROM [EVDemo].[dbo].[EV Model] as info, [EVDemo].[dbo].[UsersEVList] as list " +
                    "WHERE info.ID = list.EVModelID " + "AND " + "list.UserID = '" + userId + "' " +
                    "ORDER BY ID ";

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
                            ShowMessage("Error at PopulateEvList: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0)
                        {
                            ShowMessage("Please add at least one EV");
                        }
                    }
                }
                var newDT = new DataTable();
                newDT.Columns.Add(new DataColumn("ID", typeof(string)));
                newDT.Columns.Add(new DataColumn("EvModelID", typeof(string)));
                newDT.Columns.Add(new DataColumn("Number", typeof(string)));
                newDT.Columns.Add(new DataColumn("EvName", typeof(string)));
                newDT.Columns.Add(new DataColumn("Nickname", typeof (string)));
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    newDT.Rows.Add(DT.Rows[i][0].ToString(), DT.Rows[i][1].ToString(), (i + 1).ToString(), DT.Rows[i][2].ToString(), DT.Rows[i][3]);
                }
                Session["data"] = newDT;
                GvEvList.DataSource = Session["data"];
                GvEvList.DataBind();
                GvEvList.Visible = true;
                GvEvList.SelectedIndex = -1;
                tbNickname.Text = String.Empty;
            }
        }

        protected void GvEvListSelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow gvRow = GvEvList.Rows[GvEvList.SelectedIndex];
            btnGvEvListModify.Visible = true;
            btnGvEvListDelete.Visible = true;
            ddlGvEvListModel.SelectedValue = gvRow.Cells[findGVcolumn("EvModelID", GvEvList)].Text;
            tbNickname.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Nickname", GvEvList)].Text);
        }

        protected int findGVcolumn(string Name, GridView gv)
        {
            for (int j = 0; j < gv.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gv.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void GvEvListDataBound(object sender, GridViewRowEventArgs e)
        {
            var i = 0;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Center;
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(GvEvList, "Select$" + e.Row.RowIndex);
                e.Row.ToolTip = "Click to select this row.";
                foreach (TableCell cell in e.Row.Cells)
                {
                    cell.Width = 30;
                    cell.ToolTip = cell.Text;
                }
            }
        }

        protected Boolean canAddNewEV(string userid, bool allowEqual)
        {
            string maximumVehicles = string.Empty;
            string currentVehicles = string.Empty;
            using (var cnn = new SqlConnection(connectionString))
            {

                try
                {
                    string strQuery = "SELECT [MaximumVehicles] FROM [aspnet_Profile] WHERE UserId = '"
                        + userid + "' ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    var da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    var dt = new DataTable();
                    da.Fill(dt);
                    maximumVehicles = dt.Rows[0][0].ToString();

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at get maximum vehicles number: " + ex.Message);
                }

                try
                {
                    string strQuery = "SELECT COUNT(EVModelID) FROM [UsersEVList] WHERE UserId = '"
                        + userid + "' ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    var da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    var dt = new DataTable();
                    da.Fill(dt);
                    currentVehicles = dt.Rows[0][0].ToString();

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at get current vehicles number: " + ex.Message);
                }
            }
            if (!allowEqual)
            {
                if (Int32.Parse(maximumVehicles) <= Int32.Parse(currentVehicles))
                {
                    PopUpError("You can only add up to " + Int32.Parse(maximumVehicles) + " EVs");
                    return false;
                }
                return true;
            }
            else
            {
                if (Int32.Parse(maximumVehicles) < Int32.Parse(currentVehicles))
                {
                    PopUpError("You can only add up to " + Int32.Parse(maximumVehicles) + " EVs");
                    return false;
                }
                return true;
            }

        }

        protected Boolean canDeleteExistEV(string userid)
        {
            string currentVehicles = string.Empty;
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "SELECT COUNT(EVModelID) FROM [UsersEVList] WHERE UserId = '"
                        + userid + "' ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    var da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    var dt = new DataTable();
                    da.Fill(dt);
                    currentVehicles = dt.Rows[0][0].ToString();

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at get current vehicles number: " + ex.Message);
                }
            }
            if (Int32.Parse(currentVehicles) == 1)
            {
                PopUpError("You have to keep at least 1 EV"); 
                return false;
            }
            return true;
        }

        protected void btnGvEvListAddClick(object sender, EventArgs e)
        {
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            string userid = gvRow.Cells[findGVcolumn("UserId")].Text;
            if (!canAddNewEV(userid, false))
                return;
            if (ddlGvEvListModel.SelectedIndex == 0)
            {
                PopUpError("Must select an EV");
                return;
            }
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "INSERT INTO [UsersEVList] (UserID, EVModelID, Nickname) VALUES(@UserID, @EVModelID, @Nickname) ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@UserID", userid));
                    cmd.Parameters.Add(new SqlParameter("@EVModelID", ddlGvEvListModel.SelectedValue));
                    cmd.Parameters.Add(new SqlParameter("@Nickname", tbNickname.Text));
                    //cmd.Parameters.Add(new SqlParameter("@Nickname", tbNickname.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at add button click: " + ex.Message);
                    return;
                }


            }
            GvEvList.SelectedIndex = -1;
            PopulateEvList(userid);
            btnGvEvListModify.Visible = false;
            btnGvEvListDelete.Visible = false;
            ShowMessage("Information Added");
        }

        protected void btnGvEvListModifyClick(object sender, EventArgs e)
        {
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            string userid = gvRow.Cells[findGVcolumn("UserId")].Text;
            GridViewRow gvEvRow = GvEvList.Rows[GvEvList.SelectedIndex];
            string id = gvEvRow.Cells[findGVcolumn("ID", GvEvList)].Text;
            if (!canAddNewEV(userid, true))
                return;
            if (ddlGvEvListModel.SelectedIndex == 0)
            {
                PopUpError("Must select an EV");
                return;
            }
            if (GvEvList.SelectedIndex == 0)
            {
                using (var cnn = new SqlConnection(connectionString))
                {
                    try
                    {
                        string strQuery = "UPDATE [aspnet_Profile] SET [EV ID] = @EVModelID WHERE UserId = @UserId ";
                        var cmd = new SqlCommand(strQuery, cnn);
                        SqlDataReader readerProfile = null;
                        cnn.Open();
                        cmd.Parameters.Add(new SqlParameter("@UserId", userid));
                        cmd.Parameters.Add(new SqlParameter("@EVModelID", ddlGvEvListModel.SelectedValue));

                        readerProfile = cmd.ExecuteReader();
                        readerProfile.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Error at modify button click: " + ex.Message);
                        return;
                    }
                }
            }
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "UPDATE [UsersEVList] SET EVModelID = @EVModelID, Nickname = @Nickname WHERE ID = @ID ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@ID", id));
                    cmd.Parameters.Add(new SqlParameter("@EVModelID", ddlGvEvListModel.SelectedValue));
                    cmd.Parameters.Add(new SqlParameter("@Nickname", tbNickname.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at modify button click: " + ex.Message);
                    return;
                }
            }
            GvEvList.SelectedIndex = -1;
            PopulateEvList(userid);
            btnGvEvListModify.Visible = false;
            btnGvEvListDelete.Visible = false;
            ShowMessage("Information Modified");
        }

        protected void btnGvEvListDeleteClick(object sender, EventArgs e)
        {
            GridViewRow gvRow = GV_UserEditor.Rows[GV_UserEditor.SelectedIndex];
            string userid = gvRow.Cells[findGVcolumn("UserId")].Text;
            if (!canDeleteExistEV(userid))
                return;
            GridViewRow gvEvRow = GvEvList.Rows[GvEvList.SelectedIndex];
            string id = gvEvRow.Cells[findGVcolumn("ID", GvEvList)].Text;
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "DELETE FROM [UsersEVList] WHERE ID = @ID ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@ID", id));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at delete button click: " + ex.Message);
                    return;
                }
            }
            GvEvList.SelectedIndex = -1;
            PopulateEvList(userid);
            btnGvEvListModify.Visible = false;
            btnGvEvListDelete.Visible = false;
            ShowMessage("Information Deleted");
        }

        protected void clearGvEvList()
        {
            GvEvList.Visible = false;
            ddlGvEvListModel.Visible = false;
            tbNickname.Visible = false;
            tbNickname.Text = string.Empty;
            btnGvEvListAdd.Visible = false;
            btnGvEvListModify.Visible = false;
            btnGvEvListDelete.Visible = false;
        }

        protected void insertPrimaryEvToUserEvList(string userid, string evid)
        {
            using (var cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "INSERT INTO [UsersEVList] (UserID, EVModelID) VALUES(@UserID, @EVModelID) ";
                    var cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@UserID", userid));
                    cmd.Parameters.Add(new SqlParameter("@EVModelID", evid));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error at insert Primary Ev To UserEvList: " + ex.Message);
                    return;
                }
            }

        }

        protected void btnSearchClearClick(object sender, EventArgs e)
        {
            tbSearchKeywords.Text = "";
            ddlSearchKeywords.SelectedIndex = 0;
            var listOfRoles = new List<string>();
            var listOfAdminCities = FindAssociatedRoles(listOfRoles);
            PopulateGridview(listOfAdminCities, cbShowActivated.Checked);
        }

        protected void btnSearchClick(object sender, EventArgs e)
        {
            var listOfRoles = new List<string>();
            string searchBy = ddlSearchKeywords.SelectedValue;
            string keyword = tbSearchKeywords.Text.ToLower();
            switch (searchBy)
            {
                case "emailAddress":
                    populateSearchByEmailAddress(keyword, cbShowActivated.Checked);
                    break;
                case "userName":
                    populateSearchByUserName(keyword, cbShowActivated.Checked);
                    break;
                case "firstName":
                    populateSearchByFirstName(keyword, cbShowActivated.Checked);
                    break;
                case "lastName":
                    populateSearchByLastName(keyword, cbShowActivated.Checked);
                    break;
                case "phoneNo":
                    populateSearchByPhoneNo(keyword, cbShowActivated.Checked);
                    break;
                case "zipCode":
                    populateSearchByZipCode(keyword, cbShowActivated.Checked);
                    break;
                case "state":
                    populateSearchByState(keyword, cbShowActivated.Checked);
                    break;
                case "roleName":
                    populateSearchByRoleName(keyword, cbShowActivated.Checked);
                    break;
            }
        }

        protected void populateSearchByEmailAddress(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;
            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND m.[Email] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE m.[Email] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByUserName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND u.[Username] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE u.[Username] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByFirstName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND p.[FirstName] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE p.[FirstName] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByLastName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND p.[LastName] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE p.[LastName] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByPhoneNo(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND p.[PhoneNo] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE p.[PhoneNo] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByZipCode(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND p.[ZipCode] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE p.[ZipCode] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByState(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND p.[State] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE p.[State] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }

        protected void populateSearchByRoleName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            List<string> CityID = new List<string>();
            List<string> Activate = new List<string>();
            List<string> Email = new List<string>();
            List<string> IsApproved = new List<string>();
            List<string> IsLockedOut = new List<string>();
            List<string> RoleName = new List<string>();
            List<string> UserGUID = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                for (int i = 0; i < SelectedCities.Count; i++)
                {
                    strQuery += "SELECT DISTINCT u.[Username], u.[UserId], u.[Activate] as Activated, m.[Email], m.[IsApproved], m.[IsLockedOut], m.[PasswordQuestion]," +
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID]   " +
                                   " FROM aspnet_Users AS u " +
                                   " INNER JOIN aspnet_Membership as m ON " +
                                   " u.[UserId] = m.[UserId] " +
                                   " INNER JOIN aspnet_Profile as p ON p.[UserId] = u.[UserId] " +
                                   " INNER JOIN aspnet_UsersInRoles as uir ON uir.[UserId] = u.[UserId] " +
                                   " LEFT JOIN [CombinatedCity] as cc on cc.ID = p.[RoleCityID]" +
                                   " INNER JOIN aspnet_Roles as r ON r.[RoleId] = uir.[RoleId] " +
                                   " INNER JOIN [EV Model] as ev ON ev.[ID] = p.[EV ID] " +
                                   " AND (p.[RoleCityID] = '" + SelectedCities[i] + "' OR cc.[MainCityID] ='" + SelectedCities[i] + "' OR cc.[CombinatedCityID] = '" + SelectedCities[i] + "')";

                    if (blnActivate)
                    {
                        strQuery += " WHERE u.[Activate] ='1' AND r.[RoleName] = '" + keyword + "' ";
                    }
                    else
                    {
                        strQuery += " WHERE r.[RoleName] = '" + keyword + "' ";
                    }

                    // strQuery += " GROUP BY uir.UserId ";

                    if (i < listCount - 1)
                        strQuery += " UNION ";

                }
                strQuery += " Order by u.username ";
                using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }
            DataTable newDT = new DataTable();

            newDT.Columns.Add(new DataColumn("UserName", typeof(string)));
            newDT.Columns.Add(new DataColumn("UserId", typeof(string)));
            newDT.Columns.Add(new DataColumn("Email", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleName", typeof(string)));
            newDT.Columns.Add(new DataColumn("EVID", typeof(string)));
            newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activated", typeof(string)));

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Username.Add(DT.Rows[i][0].ToString());
                UserGUID.Add(DT.Rows[i][1].ToString());
                Activate.Add(DT.Rows[i][2].ToString());
                RoleName.Add(ReturnRoleNameAppendString(UserGUID[i]));
                Email.Add(DT.Rows[i][3].ToString());
                IsApproved.Add(DT.Rows[i][4].ToString());
                IsLockedOut.Add(DT.Rows[i][5].ToString());
                EVID.Add(DT.Rows[i][8].ToString());
                CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], CityID[i], IsApproved[i], IsLockedOut[i], Activate[i]);
            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }
            lblTotalUsers.Text = "Total Users in this area: " + intTotalUsers;
            //   lblTotalUsers.Text = "Number of Users " + SelectedCity + ":  " + count;
            Session["data"] = newDT;
            FillInPassword();
            GV_UserEditor.DataSource = Session["data"];
            GV_UserEditor.DataBind();
            //showAllFunction();
        }
    }

    public class ComboCityAndGuidClass // Class used to obtain and maintain the 
    {
        public ComboCityAndGuidClass(string ComboCityString, string Guid)
        {
            _ComboCityString = ComboCityString;
            _Guid = Guid;
        }

        private string _Guid;

        public string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }
        }

        private string _ComboCityString;

        public string ComboCityString
        {
            get { return _ComboCityString; }
            set { _ComboCityString = value; }
        }
    }
}