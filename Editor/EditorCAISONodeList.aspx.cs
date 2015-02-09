using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EVEditor
{
    public partial class EditorCAISONodeList : System.Web.UI.Page
    {
        readonly string _connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };

        readonly string[] _strArrRolesToAllow = { "General Administrator"};
        //  string[] strArrRolesToAllow = { };
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        readonly string[] _strArrTypesToAllow = { "Administrator" };

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        readonly string[] _strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        readonly string[] _ColumnsToHide = {"CityID"};

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
                PopulateOrganization();
                populateCAISONodeList(ddlOrganization.SelectedValue);
                //populateUserName(ddlOrganization.SelectedValue);
                //populateEvInfo();
                //populateFleet(ddlOrganization.SelectedValue);
            }
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

        protected void PopulateOrganization()
        {

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
                    var l2 = new ListItem("Select...", "-1");
                    ddlCity.DataSource = dt;
                    ddlCity.DataValueField = "ID";
                    ddlCity.DataTextField = "Name";
                    ddlCity.DataBind();
                    ddlCity.Items.Insert(0,l2);
                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError("Error at populateOrg: " + ex.Message);
                    return;
                }
            }
        }

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void ddlOrganizationSelectedIndexChanged(object sender, EventArgs e)
        {

            //// Reset the gridview selection
            gvCAISONodeList.SelectedIndex = -1;
            ClearAll();
            // Hide the btnUpdate
            btnUpdate.Visible = false;
            //btnDelete.Visible = false;

            //// Repopulate the gridview with the new settings.
            populateCAISONodeList(ddlOrganization.SelectedValue);
            //populateUserName(ddlOrganization.SelectedValue);
        }

        protected void btnHideCatchErrorClick(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected void GvCAISONodeListPaging(object sender, GridViewPageEventArgs e)
        {

            gvCAISONodeList.SelectedIndex = -1;

            //ClearAllTbs();
            //ClearAllErrorLbl();
            //ClearImage();
            var dataTable = Session["data"] as DataTable;

            gvCAISONodeList.PageIndex = e.NewPageIndex;
            gvCAISONodeList.DataSource = dataTable;
            gvCAISONodeList.DataBind();
        }

        protected void GvCAISONodeListSelectedIndex(object sender, EventArgs e)
        {

            fillInfo();
            //HideError();

            btnUpdate.Visible = true;
            //btnDelete.Visible = true;
            //showcbClearImage();
            //cbClearImage.Checked = false;
        }

        protected void GvCAISONodeListRowCreated(object sender, GridViewRowEventArgs e)
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
            for (int j = 0; j < gvCAISONodeList.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvCAISONodeList.Columns[j].HeaderText == name)
                    return j;
            }
            return -1;
        }

        protected void GvCAISONodeListSorting(object sender, GridViewSortEventArgs e)
        {
            var dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                var dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + GetSortDirectionString(e.SortDirection.ToString());
                gvCAISONodeList.DataSource = dataTable.DefaultView;
                gvCAISONodeList.DataBind();
            }
            gvCAISONodeList.SelectedIndex = -1;
            ClearAll();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvCAISONodeList.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvCAISONodeList.Columns.IndexOf(field);
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
            gvCAISONodeList.HeaderRow.Cells[index].Controls.Add(sortImage2);
        }

        protected void ClearAll()
        {
            gvCAISONodeList.SelectedIndex = -1;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
            tbTransactionNodeID.Text = string.Empty;
            ddlCity.SelectedIndex = 0;
            tbNote.Text = string.Empty;
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

        protected void populateCAISONodeList(string strOrgId)
        {
            var DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (strOrgId == "-1")
                {
                    strOrgId = string.Empty;
                }

                string sqlQuery = "SELECT ca.[TransactionNodeID], ca.[CityID], ci.[Name], ca.Note " +
                                    "FROM [CAISONodeList] AS ca " +
                                    "INNER JOIN [City] AS ci ON ca.[CityID] = ci.[ID] ";


                if (strOrgId != string.Empty)
                {
                    sqlQuery += "AND ca.[CityID] = '" + strOrgId + "'";
                }

                sqlQuery += " ORDER BY ca.TransactionNodeID DESC";

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
                            ShowError("Error at populateCAISONodeList: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopUpMessage for clarity to note there are no Gateways for given selection.
                        {
                            ShowError("No data in this selection");
                        }
                    }
                }

                Session["data"] = DT;
                gvCAISONodeList.DataSource = Session["data"];
                gvCAISONodeList.DataBind();

            }
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

        protected void btnNew_Click(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    string strQuery = "INSERT INTO [CAISONodeList] ([TransactionNodeID], [CityID], Note) VALUES (@TransactionNodeID, @CityID, @Note) ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;

                    cnn.Open();

                    cmd.Parameters.Add(new SqlParameter("@TransactionNodeID", tbTransactionNodeID.Text));
                    cmd.Parameters.Add(new SqlParameter("@CityID", ddlCity.SelectedValue));
                    if (string.IsNullOrEmpty(tbNote.Text))
                        cmd.Parameters["@Note"].Value = DBNull.Value;
                    else
                        cmd.Parameters.Add(new SqlParameter("@Note", tbNote.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnUpdate_Click: " + ex.Message);
                    return;
                }

            }
            gvCAISONodeList.SelectedIndex = -1;
            populateCAISONodeList(ddlOrganization.SelectedValue);
            PopUpMessage("Information added");
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                try
                {
                    string strQuery = "UPDATE [CAISONodeList] SET [CityID] = @CityID, Note = @Note WHERE [TransactionNodeID] = @TransactionNodeID";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;

                    cnn.Open();

                    cmd.Parameters.Add(new SqlParameter("@TransactionNodeID", tbTransactionNodeID.Text));
                    cmd.Parameters.Add(new SqlParameter("@CityID", ddlCity.SelectedValue));
                    if (string.IsNullOrEmpty(tbNote.Text))
                        cmd.Parameters["@Note"].Value = DBNull.Value;
                    else
                        cmd.Parameters.Add(new SqlParameter("@Note", tbNote.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnUpdate_Click: " + ex.Message);
                    return;
                }

            }
            populateCAISONodeList(ddlOrganization.SelectedValue);
            PopUpMessage("Information Updated");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearAll();
            btnUpdate.Visible = false;
        }

        protected void fillInfo()
        {
            GridViewRow gvRow;
            gvRow = gvCAISONodeList.Rows[gvCAISONodeList.SelectedIndex];
            tbTransactionNodeID.Text = gvRow.Cells[FindGVcolumn("Traction Node ID")].Text;
            ddlCity.SelectedValue = gvRow.Cells[FindGVcolumn("CityID")].Text;
            tbNote.Text = gvRow.Cells[FindGVcolumn("Note")].Text.Replace("&nbsp;", " ");
        }
    }
}