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
using System.Text;
using System.Web.UI.HtmlControls;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using RTMC;

namespace EVEditor
{
    public partial class EmailForm : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        string[] ColumnsToHide = {};
        string[] strAllowedRoles = { "Administrator", "Maintainer", "Operator"}; // The allowed roles to access this page.
        string strMasterRole = "General";
        List<string> listchkRows = new List<string>();
        
        Dictionary<string, string> dictCbState = new Dictionary<string, string>();
        Dictionary<string, string> dictcbAllUsers = new Dictionary<string, string>();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ViewState["dictCbState"] != null) // Save state of checked boxes.
            {
                dictCbState = (Dictionary<string, string>)ViewState["dictCbState"];                
            }
            else
            {
                dictCbState.Clear();
            }
            if (ViewState["dictcbAllUsers"] != null) // Save state of checked boxes.
                dictcbAllUsers = (Dictionary<string, string>)ViewState["dictcbAllUsers"];
            else
                dictcbAllUsers.Clear();
            
            List<string> ListOfAdminCities = new List<string>();
            bool blnisMultiAdmin = false;

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
                bool blnisAdministrator = ListOfAdminCities.Count>0; // Check if atleast one of the roles is an admin role
                blnisMultiAdmin = ListOfAdminCities.Count > 1; // Check is the admin is a multi role admin.
                if (!blnisAdministrator) // only continue if the user is a city administrator
                {
                    Response.Redirect("~/Default.aspx"); // Reroute un verified user to home page.
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }

            if (!IsPostBack) // Initialize the Data only once per page load (not postback)
            {
                Initialize(ListOfAdminCities, blnisMultiAdmin); // enter in the list of admin cities so that we can readjust the DDL_SelectByCity items.

            }
            else
            {
                voidFindChecked();
            }
        }
                
        protected void Initialize(List<string> ListOfAdminCities, bool blnisMultiAdmin)
        {
            populateddlSelectByCity(ListOfAdminCities, blnisMultiAdmin);
            if (cbShowActivated.Checked)
            {
                populategvUserList(ListOfAdminCities, true);
            }
            else
            {
                populategvUserList(ListOfAdminCities,false);
            }
            
            
            populateddlFromEmail();
            //voidddlSelectByCity_Changed();
        }

        #region PageLoad Helper Functions

        protected void populategvUserList(List<string> ListOfRoles, bool blnActivate)
        {
            
            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof (string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof (string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
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
                    ShowError(" ERROR in Populating Role Gridview ");
                    return null;
                }

            }
            catch (Exception ex)
            {
                cnn.Close();

                ShowError("Error in ReturnRoleID " + ex.Message);
                return null;
            }
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
                    ShowError("RETURN EV NAME ERROR : (Most Likely, there is no EV Selected Yet) " + ex.Message);
                    cnn.Close();
                    return null;
                }
            }
        }

        protected List<string> ListFindDuplicateUsernameForEmail()
        {
            List<string> listReturnString = new List<string>();
            List<string> listUsers = new List<string>();
            foreach (GridViewRow row in gvUserList.Rows)
            {
                listUsers.Add(row.Cells[findGVcolumn("User Name")].Text);
            }
            

            listUsers.Sort();


            for (int i = 1; i < listUsers.Count-1; i++)
            {
                if (listUsers[i] == listUsers[i - 1])
                {                    
                    if (listReturnString.IndexOf(listUsers[i]) == -1)
                    {
                        
                        listReturnString.Add(listUsers[i]);
                    }
                }
            }
            return listReturnString;
        }

        protected bool blnFindIfTrulyUnchecked(string strUsername)
        {
            bool blnUnchecked = true;
            
            foreach (GridViewRow row in gvUserList.Rows)
            {
                CheckBox chk = row.Cells[0].Controls[0] as CheckBox;
                if (chk != null && chk.Checked)
                {
                    blnUnchecked = false;
                }
            }
            return blnUnchecked;
        }

        protected void voidFindChecked()
        {
            string strToEmail = string.Empty;

            List<string> listDuplicateUser = ListFindDuplicateUsernameForEmail();

            foreach (GridViewRow row in gvUserList.Rows)
            {
                // Return column number containing the username
                int intGvColumn = findGVcolumn("User Name");

                CheckBox chk = row.Cells[0].Controls[0] as CheckBox;

                if (chk != null && chk.Checked)
                {
                    try
                    {
                        dictCbState.Add(row.Cells[intGvColumn].Text, row.Cells[findGVcolumn("Email")].Text);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    if (listDuplicateUser.IndexOf(row.Cells[intGvColumn].Text) != -1)
                    {
                        if (blnFindIfTrulyUnchecked(row.Cells[intGvColumn].Text))
                        {
                            dictCbState.Remove(row.Cells[intGvColumn].Text);
                        }
                    }
                    else
                    {
                        dictCbState.Remove(row.Cells[findGVcolumn("User Name")].Text);
                    }
                }
            }
            int intdictcount = 0;

            foreach (KeyValuePair<string, string> entry in dictCbState)
            {
                strToEmail += entry.Value;

                if (intdictcount < dictCbState.Count() - 1)
                    strToEmail += ", ";
                intdictcount++;
            }
            tbSendToEmail.Text = strToEmail;
        }

        protected List<string> FindAssociatedRoles(List<string> ListOfRoles)
        {
            List<string> ListOfAdminCities = new List<string>();

            for (int i = 0; i < ListOfRoles.Count; i++)
            {
                for (int j = 0; j < strAllowedRoles.Count(); j++)
                {
                    if (ListOfRoles[i].IndexOf(strAllowedRoles[j]) != -1) 
                    {
                        ListOfAdminCities.Add(ListOfRoles[i].Substring(0, ListOfRoles[i].Length - strAllowedRoles[j].Length - 1)); 
                        // The above line of code adds the role areas of the account holder that has permission to access this page.
                    }
                }
            } 
            return ListOfAdminCities;
        }
        #endregion

        #region Viewstate functions
        void Page_PreRender(object sender, EventArgs e)
        {
            ViewState.Add("dictCbState", dictCbState);
            ViewState.Add("dictcbAllUsers", dictcbAllUsers);
        } 
        #endregion

        #region gvUserList functions,  (Sorting, Paging, Index)

        protected void voidFindChkcbs()
        {
            foreach (GridViewRow row in gvUserList.Rows)
            {
                CheckBox chk = row.Cells[findGVcolumn("Select")].Controls[0] as CheckBox;
                foreach (KeyValuePair<string, string> entry in dictCbState)
                {
                    if (row.Cells[findGVcolumn("User Name")].Text == entry.Key)
                        chk.Checked = true;
                }
            }
        }


        protected void gvUserList_Sorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            { 
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvUserList.DataSource = dataTable.DefaultView;
                gvUserList.DataBind();
            }

            //gvUserList.SelectedIndex = -1;
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvUserList.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvUserList.Columns.IndexOf(field);
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
            gvUserList.HeaderRow.Cells[index].Controls.Add(sortImage2);
            
            voidFindChkcbs();
            voidFindChecked();

        }
        void AddSortImage(int columnIndex, GridViewRow headerRow)
        {
            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (gvUserList.SortDirection == SortDirection.Ascending)
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

        protected void gvUserList_Paging(Object sender, GridViewPageEventArgs e) 
        {
            DataTable dataTable = Session["data"] as DataTable;
            gvUserList.PageIndex = e.NewPageIndex;
            gvUserList.DataSource = dataTable;
            gvUserList.DataBind();

            voidFindChkcbs();
            voidFindChecked();
        }

        protected void gvUserList_RowCreated(object sender, GridViewRowEventArgs e)
        {
            foreach (GridViewRow row in gvUserList.Rows)
            {
                CheckBox chk = row.Cells[findGVcolumn("Select")].Controls[0] as CheckBox;
                //if (chk != null && chk.Checked)
                //if (chk.Checked)
                {
                   // lblTest.Text += chk.Checked.ToString() + "<br>";
                   // lblTest.Text += " Meow";
                }
            }
            //for (int i = 0; i < ColumnsToHide.Count(); i++)
            //{
            //    int cur = findGVcolumn(ColumnsToHide[i]);
            //    if (e.Row.RowType == DataControlRowType.Header)
            //    {
            //        e.Row.Cells[cur].Visible = false;
            //    }
            //    if (e.Row.RowType == DataControlRowType.DataRow)
            //    {
            //        e.Row.Cells[cur].Visible = false;
            //    }
            //}
        }

        protected void ddlSelectByCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            voidddlSelectByCity_Changed();
        }

        protected void voidddlSelectByCity_Changed()
        {
            voidHideSMTPErrors();
            voidClearCbsandTb();
            //List<string> listSelectedList = new List<string>();

            //if (ddlSelectByCity.SelectedValue != "-1" || ddlSelectByCity.Items.Count == 1)
            //    listSelectedList.Add(ddlSelectByCity.SelectedItem.Text);
            //else
            //{
            //    for (int i = 1; i < ddlSelectByCity.Items.Count; i++)
            //    {
            //        listSelectedList.Add(ddlSelectByCity.Items[i].Text);
            //    }
            //}
            RolePrincipal rp = (RolePrincipal)User; // meow
            string[] roles = Roles.GetRolesForUser();
            List<string> ListOfRoles = new List<string>();

            for (int i = 0; i < roles.Count(); i++)
            {
                ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
            }
            if (ddlSelectByCity.SelectedIndex == 0)
            {
                List<string> ListOfAdminCities = new List<string>();
                ListOfAdminCities = FindAssociatedRoles(ListOfRoles);
                populategvUserList(ListOfAdminCities, cbShowActivated.Checked);
                populateddlFromEmail();
            }
            else
            {
                populategvUserList(ReturnSelectedCities(), cbShowActivated.Checked);
                populateddlFromEmail();
            }
        }
        #endregion

        #region populateDDL

        protected void populateddlFromEmail()
        {
            int intddlCount = ddlSelectByCity.Items.Count;
            List<string> listCityGuids = new List<string>();
            if (ddlSelectByCity.SelectedIndex == 0)
            {
                if (intddlCount == 1)
                    listCityGuids.Add(ddlSelectByCity.Items[0].Value);
                else
                {
                    intddlCount--; // Subtract to take into account the "Select..." value
                    for (int i = 1; i <= intddlCount; i++)
                    {
                        listCityGuids.Add(ddlSelectByCity.Items[i].Value);
                    }
                }
                intddlCount--;
            }
            else
            {
                intddlCount = 0;
                listCityGuids.Add(ddlSelectByCity.SelectedItem.Value);
            }

            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                string sqlQuery = string.Empty;

                while (intddlCount >= 0)
                {
                    sqlQuery += "SELECT ([Name] + ' - ' + [Email Address]) as [NameEmail], [Email Address]" +
                                " FROM City " +
                                " WHERE ID = '" + listCityGuids[intddlCount] + "'";
                    if (intddlCount > 0)
                        sqlQuery += " UNION ";
                    intddlCount--;
                }
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
                            ShowError("Error at populateddlFromEmail: " + ex.Message);
                            return;
                        }
                    }
                }
            }

            ddlFromEmail.DataSource = DT;
            ddlFromEmail.DataValueField = "Email Address";
            ddlFromEmail.DataTextField = "NameEmail";
            ddlFromEmail.DataBind();

            ListItem li = new ListItem("Select From Emails:", "-1");
            ListItem li2 = new ListItem("Combined City - smercev@gmail.com", "smercev@gmail.com");
            ddlFromEmail.Items.Insert(0, li);
            ddlFromEmail.Items.Insert(ddlFromEmail.Items.Count, li2);

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

        protected void populateddlSelectByCity(List<string> ListOfAdminCities, bool blnisMultipleAdmin)
        {
            List<ComboCityAndGuidClass> ComboGuid = ReturnUniqueCombinedGUID();
            if (ListOfAdminCities.Count == 1) // If the account holder is a single role accesser, then, 
            {               
                if (ListOfAdminCities.IndexOf(strMasterRole) == -1) // General is not found, then
                {
                    if (!blnisMultipleAdmin)
                        ddlSelectByCity.Enabled = false;
                    if (null == ddlSelectByCity.Items.FindByValue(strReturnCityGUIDFromName(ListOfAdminCities[0])))
                    {
                        ListItem liCity = new ListItem(ListOfAdminCities[0], strReturnCityGUIDFromName(ListOfAdminCities[0])); // Text: Name, Value = City ID
                        ddlSelectByCity.Items.Insert(0, liCity);
                    }
                }
                else 
                {
                    int intcurrentind = ddlSelectByCity.SelectedIndex;
                    SqlConnection cnn = new SqlConnection(connectionString);
                    string strQuery;
                    SqlCommand cmd;
                    DataTable dt = null;
                    SqlDataAdapter da;

                    try
                    {
                        cnn.Open();
                        strQuery = "SELECT Name, ID FROM [City]";
                        cmd = new SqlCommand(strQuery, cnn);
                        cmd.CommandType = CommandType.Text;
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);

                        ddlSelectByCity.DataSource = dt;
                        ddlSelectByCity.DataValueField = "ID";
                        ddlSelectByCity.DataTextField = "Name";
                        ddlSelectByCity.DataBind();

                        ListItem li1 = new ListItem("All Organizations", "-1");
                        ddlSelectByCity.Items.Insert(0, li1);
                        ddlSelectByCity.SelectedIndex = intcurrentind;

                        int ddlRoleAreaSize = ddlSelectByCity.Items.Count;  
                        if (ComboGuid != null)
                        {
                            ListItem ComboCityList2 = new ListItem();
                            for (int i = 0; i < ComboGuid.Count; i++) // Add
                            {
                                ComboCityList2 = new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid);

                                ddlSelectByCity.Items.Insert(ddlRoleAreaSize + i, ComboCityList2);
                            }
                        }

                       // ddlSelectByCity.SelectedValue = strReturnCityGUIDFromName(strMasterRole);
                    }
                    catch (Exception ex)
                    {
                        ShowError("Error at populateddlSelectByCity " + ex.Message);
                    }
                    finally
                    {
                        if (cnn != null)
                            cnn.Close();
                    }
                }
            }
            else // Account holder is a multi role accesor
            {
                ddlSelectByCity.Enabled = true;
                ddlSelectByCity.Items.Clear();
                ListItem liSelect = new ListItem("All Organizations", "-1");
                ddlSelectByCity.Items.Insert(0, liSelect);

                ListItem liCities;
                for (int i = 0; i < ListOfAdminCities.Count; i++)
                {
                    liCities = new ListItem(ListOfAdminCities[i], ObtainCityGUIDfromUserCity(ListOfAdminCities[i]));
                    ddlSelectByCity.Items.Add(liCities);
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

                    ShowError("Error while entering Combo City ID");
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
                            ddlSelectByCity.Items.Add(new ListItem(ComboGuid[i].ComboCityString, ComboGuid[i].Guid));
                        }
                    }
                }
            }


            if (!IsPostBack)
            {
                if (ListOfAdminCities.IndexOf(strMasterRole) != -1)
                {
                    ddlSelectByCity.SelectedValue = strReturnCityGUIDFromName(strMasterRole);
                }
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
                ShowError(" <br> Return UserGUID ERROR:" + ex.Message);
                cnn.Close();
                return null;
            }
            finally
            {
                cnn.Close();
            }
            return userGUID;
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

        #endregion

        #region buttons

        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            int intSelectedEmail = ddlFromEmail.SelectedIndex;

            List<string> listSelectedList = new List<string>();

            if (ddlSelectByCity.SelectedValue != "-1" || ddlSelectByCity.Items.Count == 1)
                listSelectedList.Add(ddlSelectByCity.SelectedItem.Text);
            else
            {
                for (int i = 1; i < ddlSelectByCity.Items.Count; i++)
                {
                    listSelectedList.Add(ddlSelectByCity.Items[i].Text);
                }
            }
            voidHideSMTPErrors();
            tbSendToEmail.Text = string.Empty;

            Initialize(listSelectedList, ddlSelectByCity.Items.Count > 1);

            ddlFromEmail.SelectedIndex = intSelectedEmail;
        }
        protected void btnHideError_Click(object sender, EventArgs e)
        {
            btnHideErrorClear();
        }

        protected void btnHideErrorClear()
        {
            lblErrorMessage.Text = string.Empty;
            lblErrorMessage.Visible = false;
            btnHideError.Visible = false;
        }

        protected void btnCheckAll_Click(object sender, EventArgs e)
        {
            dictCbState = new Dictionary<string, string>(dictcbAllUsers);
            voidFindChkcbs();
            voidFindChecked();
        }

        protected void btnUncheckAll_Click(object sender, EventArgs e)
        {
            voidClearCbsandTb();
            voidFindChecked();
            tbUpdatedTextbox.Content = string.Empty;
            tbSubject.Text = string.Empty;
        }

        protected void voidClearCbsandTb()
        {
            tbSendToEmail.Text = string.Empty;
            foreach (GridViewRow row in gvUserList.Rows)
            {
                CheckBox chk = row.Cells[0].Controls[0] as CheckBox;
                chk.Checked = false;
            }
            dictCbState.Clear();
        }

        #endregion

        #region ReturnFunctions

        protected string strReturnCityGUIDFromName(string City)
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
                return dt.Rows.Count == 0 ? "Combined City":dt.Rows[0][0].ToString();
             }
            catch (Exception ex)
            {                
                ShowError("Obtain City GUID ERROR:  "+ ex.Message);
                
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
        #endregion

        #region HelperFunctions // PopUpError , ShowError, SendMail, etc.

        protected void ddlFromEmail_SelectedIndexChanged(object sender, EventArgs e)
        {
            voidHideSMTPErrors();
        }

        protected void voidHideSMTPErrors()
        {
            btnHideErrorClear();
        }

        protected bool blnSendMailValidation()
        {
            bool blnPasses = true;
            return blnPasses;
        }

        protected string strReturnCityFromddlSelection(string strddlSelection)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            string strCityName = string.Empty;

            try
            {
                cnn.Open();
                strQuery = "SELECT Name From City";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (strddlSelection.IndexOf(dt.Rows[i][0].ToString()) != -1)
                        strCityName = dt.Rows[i][0].ToString();
                }
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at strReturnCityFromddlSelection: " + ex.Message);
                if (cnn != null)
                {
                    cnn.Close();
                }
                return null;
            }
            finally
            {
                if(cnn!= null)
                    cnn.Close();
            }
            return strCityName;
        }
        protected void SendMail()
        {
            if (!blnSendMailValidation())
                return;

            // Obtain Email Password
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            MailMessage email = null;
            SmtpClient sc = null;

            string strFromEmail = ddlFromEmail.SelectedValue;
            string strSmtpHost = string.Empty;
            string strEmailPass = string.Empty;
            bool blnSSL = true;
            string strSmtpport = string.Empty;

            string strEmailCity = strReturnCityFromddlSelection(ddlFromEmail.SelectedItem.Text);
 
            try
            {
                cnn.Open();
                strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port] FROM [City] WHERE [Name] = '" + strEmailCity + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                strEmailPass = dt.Rows[0][0].ToString();
                strSmtpHost = dt.Rows[0][1].ToString();
                blnSSL = bool.Parse(dt.Rows[0][2].ToString());

                strSmtpport = Server.HtmlDecode(dt.Rows[0][3].ToString());


                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at SendMail1: " + ex.Message);
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
                    ShowError("Error: Update Password of Organization email.  The Decryption algorithm does not recognize the encryption.");
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
                        ShowError("Port information is incorrect.  Please check Edit Organization page to ensure the port is correct.");
                    }
                }

                sc.Credentials = new NetworkCredential(strFromEmail, strEmailPass);
                sc.EnableSsl = blnSSL;
                sc.Timeout = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["EmailServerTimeOut"].ToString());
                email.To.Add(strFromEmail);
                email.Bcc.Add(tbSendToEmail.Text);

                string strEmailBody = tbUpdatedTextbox.Content;
                strEmailBody = strEmailBody.Replace("\n", "<br/>");
                strEmailBody = strEmailBody.Replace(" ", "&nbsp;");

                email.Subject = tbSubject.Text;
                email.IsBodyHtml = true;

                email.Body = tbUpdatedTextbox.Content;
                

                //email.Body = "<table>";
                //email.Body += "<tr><td> " + strEmailBody + "</td></tr>";
                //email.Body += "</table>";


                sc.Send(email);
               
            }
            catch (Exception ex)
            {
                ShowError("Error at SendMail2:  " +  ex.Message);
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
                    ShowError("Email sent!");                   
                }
                else
                {
                    
                }
            }
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvUserList.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvUserList.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
        {
            lblErrorMessage.Visible = true;
            lblErrorMessage.Text = Message;
            btnHideError.Visible = true;
        }

        protected void PopUpError(string Message) // Popup Error  with the Message
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

        #endregion

        #region HelperClasses

        public class ComboCityAndGuidClass // Class used to obtain and maintain the Cities
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

        /// <summary>
        /// A simple class just to save the state of our two CheckBoxes
        /// Code From http://forums.asp.net/t/1295845.aspx/1
        /// </summary>
        public class CheckBoxState
        {
            public CheckBoxState(bool blnSelected)
            {
                this._blnSelected = blnSelected;
            }


            private bool _blnSelected;
            public bool blnSelected
            {
                get { return _blnSelected; }
                set { _blnSelected = value; }
            }
        }


        #endregion

        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            if (ddlFromEmail.SelectedIndex == 0)
            {
                ShowError("Choose an email");
                return;
            }

            SendMail();
        }

        protected void btnClearEmailFields_Click(object sender, EventArgs e)
        {
            tbUpdatedTextbox.Content = string.Empty;
            tbSubject.Text = string.Empty;
            voidClearCbsandTb();
            voidFindChecked();
        }

        #region Encryption
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

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
        #endregion

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
                case "roleName":
                    populateSearchByRoleName(keyword, cbShowActivated.Checked);
                    break;
            }
        }

        protected void populateSearchByEmailAddress(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof(string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof(string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
        }

        protected void populateSearchByUserName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof(string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof(string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
        }

        protected void populateSearchByFirstName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof(string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof(string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
        }

        protected void populateSearchByLastName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof(string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof(string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
        }

        protected void populateSearchByRoleName(string keyword, bool blnActivate)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            dictcbAllUsers.Clear();
            int intTotalUsers = 0;

            string strQuery = string.Empty;
            DataTable DT = new DataTable();

            List<string> SelectedCities = ReturnSelectedCities(); // Return a list from of the cities in the ddlSelectByCity

            int listCount = SelectedCities.Count;

            List<string> Username = new List<string>();
            List<string> EVID = new List<string>();
            var FirstName = new List<string>();
            var LastName = new List<string>();
            //List<string> CityID = new List<string>();
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
                                   " m.[PasswordAnswer], (ev.Manufacturer + ' '+ ev.Model) as EVID, p.[RoleCItyID], p.[FirstName], p.[LastName]   " +
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
            newDT.Columns.Add(new DataColumn("EVName", typeof(string)));
            newDT.Columns.Add(new DataColumn("FirstName", typeof(string)));
            newDT.Columns.Add(new DataColumn("LastName", typeof(string)));
            //newDT.Columns.Add(new DataColumn("RoleArea", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsApproved", typeof(string)));
            newDT.Columns.Add(new DataColumn("IsLockedOut", typeof(string)));
            newDT.Columns.Add(new DataColumn("Activate", typeof(string)));

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
                //CityID.Add(ObtainUserCityFromGUID(DT.Rows[i][9].ToString()));
                FirstName.Add(DT.Rows[i][10].ToString());
                LastName.Add(DT.Rows[i][11].ToString());
                newDT.Rows.Add(Username[i], UserGUID[i], Email[i], RoleName[i], EVID[i], FirstName[i], LastName[i], IsApproved[i], IsLockedOut[i], Activate[i]);

            }

            string SelectedCity = string.Empty;
            if (ddlSelectByCity.SelectedValue == "-1")
            {
            }
            else
            {
                SelectedCity = "in " + ddlSelectByCity.SelectedItem.ToString();
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
                try
                {
                    dictcbAllUsers.Add(DT.Rows[i][0].ToString(), DT.Rows[i][3].ToString());
                }
                catch
                {
                }
            }


            lblTotalUsers.Text = "Users in this area: " + intTotalUsers;
            Session["data"] = newDT;
            gvUserList.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvUserList.DataBind(); // Bind data 
        }

        protected void btnSearchClearClick(object sender, EventArgs e)
        {
            tbSearchKeywords.Text = "";
            ddlSearchKeywords.SelectedIndex = 0;
            var listOfRoles = new List<string>();
            var listOfAdminCities = FindAssociatedRoles(listOfRoles);
            populategvUserList(listOfAdminCities, cbShowActivated.Checked);
        }
    }

}