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
using System.Web.Mail;
using System.Net;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using RTMC;
using System.Security.Cryptography;
using System.IO;



namespace EVEditor
{
    public partial class EditOrganization : System.Web.UI.Page
    {    
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        // ColumnsToHide, makes the hiding of columns well visible to programmer.  To remove or add columns to hide, modify the string below
        string[] ColumnsToHide = { "ID", "Email Password", "Consumer Key", "Consumer Secret", "Access Token", "Access Token Secret", "Energy Price 1", "Energy Price 2", "Energy Price 3", "Energy Price 4", "Energy Price 5", "Energy Price 6", "Energy Price 7", "Energy Price 8", "Energy Price 9", "Energy Price 10", "Energy Price 11", "Energy Price 12", "Energy Price 13", "Energy Price 14", "Energy Price 15", "Energy Price 16", "Energy Price 17", "Energy Price 18", "Energy Price 19", "Energy Price 20", "Energy Price 21", "Energy Price 22", "Energy Price 23", "Energy Price 24", "Price Adjustment 1", "Price Adjustment 2", "Price Adjustment 3", "Price Adjustment 4", "Price Adjustment 5", "Price Adjustment 6", "Price Adjustment 7", "Price Adjustment 8", "Price Adjustment 9", "Price Adjustment 10", "Price Adjustment 11", "Price Adjustment 12", "Price Adjustment 13", "Price Adjustment 14", "Price Adjustment 15", "Price Adjustment 16", "Price Adjustment 17", "Price Adjustment 18", "Price Adjustment 19", "Price Adjustment 20", "Price Adjustment 21", "Price Adjustment 22", "Price Adjustment 23", "Price Adjustment 24", "Level1 Energy Retail Adjustment", "Level2 Energy Retail Adjustment" };
        string[] strArrRolesToAllow = { "General Administrator"};   
        string[] strArrTypesToAllow = { "Administrator", "Maintainer" }; //Make sure there are no white spaces before or after the string. i.e. " Maintainer"
        string strOrganization = string.Empty;

        
        protected void Page_Load(object sender, EventArgs e)
        {            
            if (!cbShowPassword.Checked)
            {
                tbPassword.TextMode = TextBoxMode.Password;
            }
            else
                cbShowPassword.Checked = true;

            if (User.Identity.IsAuthenticated)
            {
                RolePrincipal rp = (RolePrincipal)User;
                string[] roles = Roles.GetRolesForUser();
                List<string> ListOfRoles = new List<string>();
                for (int i = 0; i < roles.Count(); i++)
                {
                    ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
                }

                bool isGenAdministrator = blnHasPermission(ListOfRoles); // Check if atleast one of the roles is an admin role
                if (!isGenAdministrator) // only continue if the user is a city administrator
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
                Initialize();
                voidShowAllFunction();
            }
        }


        protected void Initialize()
        {
            voidPopulategvOrganization();
            voidPopulateddlState();
            cbShowActivated.Checked = true;
            PopulateddlEVUserAccountType();
            PopulateddlRTMCUserAccountType();
            PopulateddlPriceTransactionNode();
        }

        protected void PopulateddlPriceTransactionNode()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            ListItem li = new ListItem("Null", "-1");
            try
            {
                cnn.Open();
                strQuery = "SELECT [TransactionNodeID] FROM [CAISONodeList] ";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlPriceTransactionNode.DataSource = dt;
                ddlPriceTransactionNode.DataValueField = "TransactionNodeID";
                ddlPriceTransactionNode.DataTextField = "TransactionNodeID";
                ddlPriceTransactionNode.DataBind();
                ddlPriceTransactionNode.Items.Insert(0, li);
            }
            catch (Exception ex)
            {
                ShowMessage("Populate Price Transaction Node Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected void voidPopulategvOrganization()
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                string sqlQuery = " SELECT [ID], Name as [Organization], State, Latitude, Longitude, Activate, [Email Address] as Email, [Email Password], [Email Host], [EnableSSL], [Email Port], [CO2Index], [PriceTransactionNode], " +
                                  " [EnergyPrice1], [EnergyPrice2], [EnergyPrice3], [EnergyPrice4], [EnergyPrice5], [EnergyPrice6], [EnergyPrice7], [EnergyPrice8], [EnergyPrice9], [EnergyPrice10], [EnergyPrice11], [EnergyPrice12], [EnergyPrice13], [EnergyPrice14], [EnergyPrice15], [EnergyPrice16], [EnergyPrice17], [EnergyPrice18], [EnergyPrice19], [EnergyPrice20], [EnergyPrice21], [EnergyPrice22], [EnergyPrice23], [EnergyPrice24], [PriceAdjustment1], [PriceAdjustment2], [PriceAdjustment3], [PriceAdjustment4], [PriceAdjustment5], [PriceAdjustment6], [PriceAdjustment7], [PriceAdjustment8], [PriceAdjustment9], [PriceAdjustment10], [PriceAdjustment11], [PriceAdjustment12], [PriceAdjustment13], [PriceAdjustment14], [PriceAdjustment15], [PriceAdjustment16], [PriceAdjustment17], [PriceAdjustment18], [PriceAdjustment19], [PriceAdjustment20], [PriceAdjustment21], [PriceAdjustment22], [PriceAdjustment23], [PriceAdjustment24], [ConsumerKey], [ConsumerSecret], [AccessToken], [AccessTokenSecret], [AllowUserAccountExpiration], [EVUserAccountTypeID], [RTMCUserAccountTypeID], [Level1EnergyRetailAdjustment], [Level2EnergyRetailAdjustment] " +
                                  " FROM City ORDER BY Name";

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
                            lblCatchError.Text += ex.Message;
                            lblCatchError.Visible = true;
                            btnHideCatchError.Visible = true;
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }

            Session["data"] = DT;
            gvOrganization.DataSource = Session["data"];
            gvOrganization.DataBind();
            voidShowAllFunction();
        }

        protected void voidPopulateddlState()
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                string sqlQuery = "SELECT * From State";

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
                            ShowMessage("Error at voidPopulateddlState: " + ex.Message);
                            return;
                        }
                    }
                }
            }

            ddlState.DataSource = DT;
            ddlState.DataValueField = "State";
            ddlState.DataTextField = "State";
            ddlState.DataBind();

            ListItem li = new ListItem("Select...", "-1");
            ddlState.Items.Insert(0, li);
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
                ShowMessage("<br> PopulateddlEVUserAccount Error: " + ex.Message);
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
                ShowMessage("<br> PopulateddlRTMUserAccountExpirationWindow Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }

        protected bool blnHasPermission(List<string> RoleList)
        {
            for (int i = 0; i < RoleList.Count; i++)
            {
                for (int j = 0; j < strArrRolesToAllow.Count(); j++)
                {
                    if (RoleList[i].IndexOf(strArrRolesToAllow[j]) != -1)
                    {
                        for (int k = 0; k < strArrTypesToAllow.Count(); k++)
                        {
                            if (strArrRolesToAllow[j].IndexOf(strArrTypesToAllow[k]) != -1)
                                strOrganization = strArrRolesToAllow[j].Substring(0, strArrRolesToAllow[j].Length - strArrTypesToAllow[k].Length - 1);
                            // The long line of code above simply takes the organization area
                            // out of a role.  For Example, "SMERC Maintainer" -> strOrganization = "SMERC"                               
                        }
                        return true;
                    }
                }
            }
            return false; // Worst case runtime is n^3, but the time is negligible in this case since there are so few variables.
        }
        #region gvOrganization Tools (Paging, Deleting, Selected, Sorting)
        protected void gvOrganizationPaging(object sender, GridViewPageEventArgs e)
        {
            gvOrganization.SelectedIndex = -1;
            voidClearAlltbs();
            DataTable dataTable = Session["data"] as DataTable;

            gvOrganization.PageIndex = e.NewPageIndex;
            gvOrganization.DataSource = dataTable;
            gvOrganization.DataBind();
            voidShowAllFunction();

        }

        protected void gvOrganizationRowCreated(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < ColumnsToHide.Count(); i++)
            {
                int cur = findGVcolumn(ColumnsToHide[i]);
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    e.Row.Cells[cur].Visible = false;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Cells[cur].Visible = false;
                }
            }
        }
        protected void gvOrganizationSelectedIndex(object sender, EventArgs e)
        {
            lblCatchError.Text = string.Empty;
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
            voidClearAlllblErrors();
            voidClearAlltbs();

            if (!blnfillintb())
                return;
            btnUpdate.Visible = true;
        }

        protected void gvOrganizationSorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvOrganization.DataSource = dataTable.DefaultView;
                gvOrganization.DataBind();
            }
            gvOrganization.SelectedIndex = -1;
            voidClearAlltbs();
            voidShowAllFunction();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvOrganization.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvOrganization.Columns.IndexOf(field);
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
            gvOrganization.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        #endregion
        #region btnClicks

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            voidClearAlllblErrors();
            if (!blnupdateValidationCheck())
            {
                lblCatchError.Text = "<br>Errors shown on right.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                return; // Exit out of function if errors are found.
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            
            SqlCommand cmd; SqlDataReader readerProfile = null;
            GridViewRow gvRow = gvOrganization.Rows[gvOrganization.SelectedIndex]; // Obtain selected index of gvStation

            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            if (!cbActivate.Checked)  // if Deactivate to Activate, then recreate the deactivated roles.  
            {
                if (ddlActivate.SelectedValue == "1")
                {
                    try
                    {
                        Roles.CreateRole(tbOrganization.Text + " Administrator");
                        Roles.CreateRole(tbOrganization.Text + " Maintainer");
                        Roles.CreateRole(tbOrganization.Text + " Operator");
                        Roles.CreateRole(tbOrganization.Text + " User");
                    }
                    catch (Exception ex)
                    {
                        lblCatchError.Visible = true;
                        lblCatchError.Text = "btnNewClick Error: The roles for " + tbOrganization.Text + " already exists and the previous roles will be used";
                        btnHideCatchError.Visible = true;
                        //return;
                    }
                }
            }

            try
            {
                //Update Roles
                strQuery = "UPDATE [aspnet_Roles] SET [RoleName]='" + tbOrganization.Text.Replace("'", "''") + " Administrator'," +
                                                   "[LoweredRoleName]='"+tbOrganization.Text.Replace("'","''").ToLower() + " administrator' WHERE [RoleName]='" + gvRow.Cells[findGVcolumn("Organization")].Text.Replace("'", "''") + " Administrator" + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                strQuery = "UPDATE [aspnet_Roles] SET [RoleName]='" + tbOrganization.Text.Replace("'", "''") + " Maintainer'," +
                                           " [LoweredRoleName]='" + tbOrganization.Text.Replace("'", "''").ToLower() + " maintainer' WHERE [RoleName]='" + gvRow.Cells[findGVcolumn("Organization")].Text.Replace("'", "''") + " Maintainer" + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                strQuery = "UPDATE [aspnet_Roles] SET [RoleName]='" + tbOrganization.Text.Replace("'", "''") + " Operator', " +
                                           "[LoweredRoleName]='" + tbOrganization.Text.Replace("'", "''").ToLower() + " operator' WHERE [RoleName]='" + gvRow.Cells[findGVcolumn("Organization")].Text.Replace("'", "''") + " Operator" + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                strQuery = "UPDATE [aspnet_Roles] SET [RoleName]='" + tbOrganization.Text.Replace("'", "''") + " User', " +
                                           "[LoweredRoleName]='" + tbOrganization.Text.Replace("'", "''").ToLower() + " user' WHERE [RoleName]='" + gvRow.Cells[findGVcolumn("Organization")].Text.Replace("'", "''") + " User" + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                // Update City

                strQuery = "UPDATE [CITY] SET [Name] = @OrganizationName, [State] = @State, [Latitude] = @Latitude, " +
                           "[Longitude] = @Longitude, [Activate] = @Activate, [Email Address] = @Email, [Email Password] = @EmailPassword, [Email Host] = @EmailHost, [EnableSSL] = @EnableSSL, [Email Port] = @EmailPort, [CO2Index] = @CO2Index, [PriceTransactionNode] = @PriceTransactionNode, " +
                           "[EnergyPrice1] = @EnergyPrice1, [EnergyPrice2] = @EnergyPrice2, [EnergyPrice3] = @EnergyPrice3, [EnergyPrice4] = @EnergyPrice4, [EnergyPrice5] = @EnergyPrice5, [EnergyPrice6] = @EnergyPrice6, [EnergyPrice7] = @EnergyPrice7 , [EnergyPrice8] = @EnergyPrice8, [EnergyPrice9] = @EnergyPrice9, [EnergyPrice10] = @EnergyPrice10, [EnergyPrice11] = @EnergyPrice11, [EnergyPrice12] = @EnergyPrice12, [EnergyPrice13] = @EnergyPrice13, [EnergyPrice14] = @EnergyPrice14, [EnergyPrice15] = @EnergyPrice15, [EnergyPrice16] = @EnergyPrice16, [EnergyPrice17] = @EnergyPrice17, [EnergyPrice18] = @EnergyPrice18, [EnergyPrice19] = @EnergyPrice19, [EnergyPrice20] = @EnergyPrice20, [EnergyPrice21] = @EnergyPrice21, [EnergyPrice22] = @EnergyPrice22, [EnergyPrice23] = @EnergyPrice23, [EnergyPrice24] = @EnergyPrice24, [PriceAdjustment1] = @PriceAdjustment1, [PriceAdjustment2] = @PriceAdjustment2, [PriceAdjustment3] = @PriceAdjustment3, [PriceAdjustment4] = @PriceAdjustment4, [PriceAdjustment5] = @PriceAdjustment5, [PriceAdjustment6] = @PriceAdjustment6, [PriceAdjustment7] = @PriceAdjustment7, [PriceAdjustment8] = @PriceAdjustment8, [PriceAdjustment9] = @PriceAdjustment9, [PriceAdjustment10] = @PriceAdjustment10, [PriceAdjustment11] = @PriceAdjustment11, [PriceAdjustment12] = @PriceAdjustment12, [PriceAdjustment13] = @PriceAdjustment13, [PriceAdjustment14] = @PriceAdjustment14, [PriceAdjustment15] = @PriceAdjustment15, [PriceAdjustment16] = @PriceAdjustment16, [PriceAdjustment17] = @PriceAdjustment17, [PriceAdjustment18] = @PriceAdjustment18, [PriceAdjustment19] = @PriceAdjustment19, [PriceAdjustment20] = @PriceAdjustment20, [PriceAdjustment21] = @PriceAdjustment21, [PriceAdjustment22] = @PriceAdjustment22, [PriceAdjustment23] = @PriceAdjustment23, [PriceAdjustment24] = @PriceAdjustment24, [AllowUserAccountExpiration] = @AllowUserAccountExpiration, [EVUserAccountTypeID] = @EVUserAccountTypeID, [RTMCUserAccountTypeID] = @RTMCUserAccountTypeID, " + "[ConsumerKey] = @ConsumerKey, [ConsumerSecret] = @ConsumerSecret, [AccessToken] = @AccessToken, [AccessTokenSecret] = @AccessTokenSecret, [Level1EnergyRetailAdjustment] = @Level1EnergyRetailAdjustment, [Level2EnergyRetailAdjustment] = @Level2EnergyRetailAdjustment" +
                           " WHERE [ID] = @ID";

                cmd = new SqlCommand(strQuery, cnn);

                SqlParameter ParamOrganizationName = new SqlParameter();
                ParamOrganizationName.ParameterName = "@OrganizationName";
                ParamOrganizationName.Value = tbOrganization.Text;
                cmd.Parameters.Add(ParamOrganizationName);

                SqlParameter ParamState = new SqlParameter();
                ParamState.ParameterName = "@State";
                ParamState.Value = ddlState.SelectedValue;
                cmd.Parameters.Add(ParamState);

                SqlParameter ParamLatitude = new SqlParameter();
                ParamLatitude.ParameterName = "@Latitude";
                ParamLatitude.Value = tbLatitude.Text;
                cmd.Parameters.Add(ParamLatitude);

                SqlParameter ParamLongitude = new SqlParameter();
                ParamLongitude.ParameterName = "@Longitude";
                ParamLongitude.Value = tbLongitude.Text;
                cmd.Parameters.Add(ParamLongitude);

                SqlParameter ParamActivate = new SqlParameter();
                ParamActivate.ParameterName = "@Activate";
                ParamActivate.Value = ddlActivate.SelectedValue;
                cmd.Parameters.Add(ParamActivate);
          
                SqlParameter ParamEmail = new SqlParameter();
                ParamEmail.ParameterName = "@Email";
                ParamEmail.Value = tbEmail.Text;
                cmd.Parameters.Add(ParamEmail);

                SqlParameter ParamEmailHost = new SqlParameter();
                ParamEmailHost.ParameterName = "@EmailHost";
                ParamEmailHost.Value = tbEmailHost.Text;
                cmd.Parameters.Add(ParamEmailHost);
                
                SqlParameter ParamEnableSSL = new SqlParameter();
                ParamEnableSSL.ParameterName = "@EnableSSL";
                ParamEnableSSL.Value = ddlEnableSSL.SelectedValue;
                cmd.Parameters.Add(ParamEnableSSL);

                SqlParameter ParamEmailPort = new SqlParameter();
                ParamEmailPort.ParameterName = "@EmailPort";
                ParamEmailPort.Value = tbEmailPort.Text;
                cmd.Parameters.Add(ParamEmailPort);

                SqlParameter ParamCO2Index = new SqlParameter();
                ParamCO2Index.ParameterName = "@CO2Index";
                ParamCO2Index.Value = ddlCO2Index.SelectedValue;
                cmd.Parameters.Add(ParamCO2Index);

                SqlParameter ParamPriceTransactionNode = new SqlParameter();
                ParamPriceTransactionNode.ParameterName = "@PriceTransactionNode";
                if (ddlPriceTransactionNode.SelectedValue == "-1")
                    ParamPriceTransactionNode.Value = DBNull.Value;
                else
                    ParamPriceTransactionNode.Value = ddlPriceTransactionNode.SelectedValue;
                cmd.Parameters.Add(ParamPriceTransactionNode);

                SqlParameter ParamEnergyPrice1 = new SqlParameter();
                ParamEnergyPrice1.ParameterName = "@EnergyPrice1";
                ParamEnergyPrice1.Value = tbEnergyPrice1.Text;
                cmd.Parameters.Add(ParamEnergyPrice1);

                SqlParameter ParamEnergyPrice2 = new SqlParameter();
                ParamEnergyPrice2.ParameterName = "@EnergyPrice2";
                ParamEnergyPrice2.Value = tbEnergyPrice2.Text;
                cmd.Parameters.Add(ParamEnergyPrice2);

                SqlParameter ParamEnergyPrice3 = new SqlParameter();
                ParamEnergyPrice3.ParameterName = "@EnergyPrice3";
                ParamEnergyPrice3.Value = tbEnergyPrice3.Text;
                cmd.Parameters.Add(ParamEnergyPrice3);

                SqlParameter ParamEnergyPrice4 = new SqlParameter();
                ParamEnergyPrice4.ParameterName = "@EnergyPrice4";
                ParamEnergyPrice4.Value = tbEnergyPrice4.Text;
                cmd.Parameters.Add(ParamEnergyPrice4);

                SqlParameter ParamEnergyPrice5 = new SqlParameter();
                ParamEnergyPrice5.ParameterName = "@EnergyPrice5";
                ParamEnergyPrice5.Value = tbEnergyPrice5.Text;
                cmd.Parameters.Add(ParamEnergyPrice5);

                SqlParameter ParamEnergyPrice6 = new SqlParameter();
                ParamEnergyPrice6.ParameterName = "@EnergyPrice6";
                ParamEnergyPrice6.Value = tbEnergyPrice6.Text;
                cmd.Parameters.Add(ParamEnergyPrice6);

                SqlParameter ParamEnergyPrice7 = new SqlParameter();
                ParamEnergyPrice7.ParameterName = "@EnergyPrice7";
                ParamEnergyPrice7.Value = tbEnergyPrice7.Text;
                cmd.Parameters.Add(ParamEnergyPrice7);

                SqlParameter ParamEnergyPrice8 = new SqlParameter();
                ParamEnergyPrice8.ParameterName = "@EnergyPrice8";
                ParamEnergyPrice8.Value = tbEnergyPrice8.Text;
                cmd.Parameters.Add(ParamEnergyPrice8);

                cmd.Parameters.AddWithValue("@EnergyPrice9", tbEnergyPrice9.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice10", tbEnergyPrice10.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice11", tbEnergyPrice11.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice12", tbEnergyPrice12.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice13", tbEnergyPrice13.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice14", tbEnergyPrice14.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice15", tbEnergyPrice15.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice16", tbEnergyPrice16.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice17", tbEnergyPrice17.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice18", tbEnergyPrice18.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice19", tbEnergyPrice19.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice20", tbEnergyPrice20.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice21", tbEnergyPrice21.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice22", tbEnergyPrice22.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice23", tbEnergyPrice23.Text);
                cmd.Parameters.AddWithValue("@EnergyPrice24", tbEnergyPrice24.Text);

                cmd.Parameters.AddWithValue("@PriceAdjustment1", tbPriceAdjustment1.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment2", tbPriceAdjustment2.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment3", tbPriceAdjustment3.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment4", tbPriceAdjustment4.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment5", tbPriceAdjustment5.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment6", tbPriceAdjustment6.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment7", tbPriceAdjustment7.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment8", tbPriceAdjustment8.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment9", tbPriceAdjustment9.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment10", tbPriceAdjustment10.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment11", tbPriceAdjustment11.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment12", tbPriceAdjustment12.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment13", tbPriceAdjustment13.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment14", tbPriceAdjustment14.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment15", tbPriceAdjustment15.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment16", tbPriceAdjustment16.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment17", tbPriceAdjustment17.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment18", tbPriceAdjustment18.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment19", tbPriceAdjustment19.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment20", tbPriceAdjustment20.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment21", tbPriceAdjustment21.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment22", tbPriceAdjustment22.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment23", tbPriceAdjustment23.Text);
                cmd.Parameters.AddWithValue("@PriceAdjustment24", tbPriceAdjustment24.Text);
                cmd.Parameters.AddWithValue("@Level1EnergyRetailAdjustment", tbLevel1EnergyRetailAdjustment.Text);
                cmd.Parameters.AddWithValue("@Level2EnergyRetailAdjustment", tbLevel2EnergyRetailAdjustment.Text);

                cmd.Parameters.AddWithValue("@AllowUserAccountExpiration", ddlAllowUserAccountExpiration.SelectedValue);
                cmd.Parameters.AddWithValue("@EVUserAccountTypeID", ddlEVUserAccountType.SelectedValue);
                cmd.Parameters.AddWithValue("@RTMCUserAccountTypeID", ddlRTMCUserAccountType.SelectedValue);

                string strEncodedConsumerKey = tbConsumerKey.Text;
                string strEncodedConsumerSecret = tbConsumerSecret.Text;
                string strEncodedAccessToken = tbAccessToken.Text;
                string strEncodedAccessTokenSecret = tbAccessTokenSecret.Text;


                // encrypt all keys if checkbox is selected
                if (cbTwitterInput.Checked)
                {
                    using (RijndaelManaged myR = new RijndaelManaged())
                    {
                        byte[] byteRijKey = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                        byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                        byte[] byteEncryptKey = EncryptStringToBytes(tbConsumerKey.Text, byteRijKey, byteRijIV);
                        byte[] byteEncryptedSecret = EncryptStringToBytes(tbConsumerSecret.Text, byteRijKey, byteRijIV);
                        byte[] byteEncryptedToken = EncryptStringToBytes(tbAccessToken.Text, byteRijKey, byteRijIV);
                        byte[] byteEncryptedTokenSecret = EncryptStringToBytes(tbAccessTokenSecret.Text, byteRijKey, byteRijIV);

                        strEncodedConsumerKey = Convert.ToBase64String(byteEncryptKey);
                        strEncodedConsumerSecret = Convert.ToBase64String(byteEncryptedSecret);
                        strEncodedAccessToken = Convert.ToBase64String(byteEncryptedToken);
                        strEncodedAccessTokenSecret = Convert.ToBase64String(byteEncryptedTokenSecret);
                    }
                }



                SqlParameter ParamConsumerKey = new SqlParameter();
                ParamConsumerKey.ParameterName = "@ConsumerKey";
                ParamConsumerKey.Value = strEncodedConsumerKey;
                cmd.Parameters.Add(ParamConsumerKey);

                SqlParameter ParamConsumerSecret = new SqlParameter();
                ParamConsumerSecret.ParameterName = "@ConsumerSecret";
                ParamConsumerSecret.Value = strEncodedConsumerSecret;
                cmd.Parameters.Add(ParamConsumerSecret);

                SqlParameter ParamAccessToken = new SqlParameter();
                ParamAccessToken.ParameterName = "@AccessToken";
                ParamAccessToken.Value = strEncodedAccessToken;
                cmd.Parameters.Add(ParamAccessToken);

                SqlParameter ParamAccessTokenSecret = new SqlParameter();
                ParamAccessTokenSecret.ParameterName = "@AccessTokenSecret";
                ParamAccessTokenSecret.Value = strEncodedAccessTokenSecret;
                cmd.Parameters.Add(ParamAccessTokenSecret);

                string strPasswordText = string.Empty;
                string strEncodedPassword = string.Empty;

                if (gvRow.Cells[findGVcolumn("Email Password")].Text != tbPassword.Text)
                {//Password is changed
                    strPasswordText = tbPassword.Text;
                   // var varPlainText = Encoding.UTF8.GetBytes(strPasswordText);
                    //strEncodedPassword = MachineKey.Encode(varPlainText, MachineKeyProtection.All);
                    // meow
                    using (RijndaelManaged myR = new RijndaelManaged())
                    {
                        
                        byte[] byteRijKey =  Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                        byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                        byte[] byteEncryptText = EncryptStringToBytes(strPasswordText, byteRijKey, byteRijIV);

                        strEncodedPassword = Convert.ToBase64String(byteEncryptText);
                    }
                }
                else
                {
                    strEncodedPassword = gvRow.Cells[findGVcolumn("Email Password")].Text;
                }

                SqlParameter ParamPassword = new SqlParameter();
                ParamPassword.ParameterName = "@EmailPassword";
                ParamPassword.Value = strEncodedPassword;
                cmd.Parameters.Add(ParamPassword);

                SqlParameter ParamID = new SqlParameter();
                ParamID.ParameterName = "@ID";
                ParamID.Value = gvRow.Cells[findGVcolumn("ID")].Text;
                cmd.Parameters.Add(ParamID);

                readerProfile = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br> Error while Updating: " + ex.Message;

                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();

                return;
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }

            voidPopulategvOrganization();
            blnfillintb();
            PopUpError("Updated");
            lblCatchError.Visible = false;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            voidClearAlllblErrors();
            if(!blnNewValidationCheck())
            {
                lblCatchError.Visible = true;
                lblCatchError.Text = "<br>Errors shown on right.";
                btnHideCatchError.Visible = true;
                return;
            }


            // Beging SQL Connection

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; SqlDataReader readerProfile = null;

            SqlCommand command = cnn.CreateCommand();
            SqlTransaction transaction;
            
            string strAppID = string.Empty;

            try
            {
                strQuery = "SELECT [ApplicationId] " +
                           " From [aspnet_Applications]";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    strAppID = reader["ApplicationId"].ToString().Trim();
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at btnNewClick0: " + ex.Message);
            }
            finally
            {
                if(cnn!=null)
                    cnn.Close();
            }

            // Open Connection, and start transaction (for rollback)
            cnn.Open();
            transaction = cnn.BeginTransaction("btnNewClick");
            command.Connection = cnn;
            command.Transaction = transaction;
            bool blnPasses = true;

            try
            {
                Guid guidAdmin;
                guidAdmin = Guid.NewGuid();

                Guid guidMaintain;
                guidMaintain = Guid.NewGuid();

                Guid guidOperator;
                guidOperator = Guid.NewGuid();

                Guid guidUser;
                guidUser = Guid.NewGuid();

                // Administrator
                command.CommandText = "Insert into [aspnet_Roles](ApplicationId, RoleId, RoleName, LoweredRoleName) " +
                                      " VALUES(@appId, @RoleId, @RoleName, @LoweredRoleName)";

                command.Parameters.AddWithValue("@appId", strAppID);
                command.Parameters.AddWithValue("@RoleId", guidAdmin);
                command.Parameters.AddWithValue("@RoleName", tbOrganization.Text + " Administrator");
                command.Parameters.AddWithValue("LoweredRoleName", tbOrganization.Text.ToLower() + " administrator");
                command.ExecuteNonQuery();

                // Maintainer
                command.CommandText = "Insert into [aspnet_Roles](ApplicationId, RoleId, RoleName, LoweredRoleName) " +
                                      " VALUES(@appId2, @RoleId2, @RoleName2, @LoweredRoleName2)";

                command.Parameters.AddWithValue("@appId2", strAppID);                
                command.Parameters.AddWithValue("@RoleId2", guidMaintain);
                command.Parameters.AddWithValue("@RoleName2", tbOrganization.Text + " Maintainer");
                command.Parameters.AddWithValue("LoweredRoleName2", tbOrganization.Text.ToLower() + " maintainer");
                command.ExecuteNonQuery();

                // Operator

                command.CommandText = "Insert into [aspnet_Roles](ApplicationId, RoleId, RoleName, LoweredRoleName) " +
                                      " VALUES(@appId3, @RoleId3, @RoleName3, @LoweredRoleName3)";

                command.Parameters.AddWithValue("@appId3", strAppID);
                command.Parameters.AddWithValue("@RoleId3", guidOperator);
                command.Parameters.AddWithValue("@RoleName3", tbOrganization.Text + " Operator");
                command.Parameters.AddWithValue("LoweredRoleName3", tbOrganization.Text.ToLower() + " operator");
                command.ExecuteNonQuery();

                // User

                command.CommandText = "Insert into [aspnet_Roles](ApplicationId, RoleId, RoleName, LoweredRoleName) " +
                                      " VALUES(@appId4, @RoleId4, @RoleName4, @LoweredRoleName4)";

                command.Parameters.AddWithValue("@appId4", strAppID);
                command.Parameters.AddWithValue("@RoleId4", guidUser);
                command.Parameters.AddWithValue("@RoleName4", tbOrganization.Text + " User");
                command.Parameters.AddWithValue("LoweredRoleName4", tbOrganization.Text.ToLower() + " user");
                command.ExecuteNonQuery();

                // Finish Inserting Roles, Enter City Information now

                command.CommandText = "INSERT City(Name, State, Latitude, Longitude, Activate,[Administrator Role ID],[Maintainer Role ID],[Operator Role ID],[User Role ID], [Email Address], [Email Password], [Email Host], [EnableSSL], [Email Port], [CO2Index], [PriceTransactionNode],"+
                                      "[EnergyPrice1],[EnergyPrice2],[EnergyPrice3],[EnergyPrice4],[EnergyPrice5],[EnergyPrice6],[EnergyPrice7],[EnergyPrice8], [EnergyPrice9], [EnergyPrice10], [EnergyPrice11], [EnergyPrice12], [EnergyPrice13], [EnergyPrice14], [EnergyPrice15], [EnergyPrice16], [EnergyPrice17], [EnergyPrice18], [EnergyPrice19], [EnergyPrice20], [EnergyPrice21], [EnergyPrice22], [EnergyPrice23], [EnergyPrice24], [PriceAdjustment1], [PriceAdjustment2], [PriceAdjustment3], [PriceAdjustment4], [PriceAdjustment5], [PriceAdjustment6], [PriceAdjustment7], [PriceAdjustment8], [PriceAdjustment9], [PriceAdjustment10], [PriceAdjustment11], [PriceAdjustment12], [PriceAdjustment13], [PriceAdjustment14], [PriceAdjustment15], [PriceAdjustment16], [PriceAdjustment17], [PriceAdjustment18], [PriceAdjustment19], [PriceAdjustment20], [PriceAdjustment21], [PriceAdjustment22], [PriceAdjustment23], [PriceAdjustment24], [AllowUserAccountExpiration], [EVUserAccountTypeID], [RTMCUserAccountTypeID], [ConsumerKey],[ConsumerSecret],[AccessToken],[AccessTokenSecret], [Level1EnergyRetailAdjustment], [Level2EnergyRetailAdjustment])" +
                                      " VALUES (@OrganizationName, @State, @Latitude, @Longitude, @Activate, @AdminRole, @MaintainerRole, @OperatorRole, @UserRole, @Email, @EmailPassword, @EmailHost, @EnableSSL, @EmailPort, @CO2Index, @PriceTransactionNode, " +
                                      " @EnergyPrice1,@EnergyPrice2,@EnergyPrice3,@EnergyPrice4,@EnergyPrice5,@EnergyPrice6,@EnergyPrice7,@EnergyPrice8,@EnergyPrice9,@EnergyPrice10,@EnergyPrice11,@EnergyPrice12,@EnergyPrice13,@EnergyPrice14,@EnergyPrice15,@EnergyPrice16,@EnergyPrice17,@EnergyPrice18,@EnergyPrice19,@EnergyPrice20,@EnergyPrice21,@EnergyPrice22,@EnergyPrice23,@EnergyPrice24,@PriceAdjustment1,@PriceAdjustment2,@PriceAdjustment3,@PriceAdjustment4,@PriceAdjustment5,@PriceAdjustment6,@PriceAdjustment7,@PriceAdjustment8,@PriceAdjustment9,@PriceAdjustment10,@PriceAdjustment11,@PriceAdjustment12,@PriceAdjustment13,@PriceAdjustment14,@PriceAdjustment15,@PriceAdjustment16,@PriceAdjustment17,@PriceAdjustment18,@PriceAdjustment19,@PriceAdjustment20,@PriceAdjustment21,@PriceAdjustment22,@PriceAdjustment23,@PriceAdjustment24,@AllowUserAccountExpiration,@EVUserAccountTypeID,@RTMCUserAccountTypeID,@ConsumerKey,@ConsumerSecret,@AccessToken,@AccessTokenSecret,@Level1EnergyRetailAdjustment,@Level2EnergyRetailAdjustment)";

                command.Parameters.AddWithValue("@OrganizationName", tbOrganization.Text);
                command.Parameters.AddWithValue("@State", ddlState.SelectedValue);
                command.Parameters.AddWithValue("@Latitude", tbLatitude.Text);
                command.Parameters.AddWithValue("@Longitude", tbLongitude.Text);
                command.Parameters.AddWithValue("@Activate", ddlActivate.SelectedValue);
                command.Parameters.AddWithValue("@Email", tbEmail.Text);
                command.Parameters.AddWithValue("@EmailHost", tbEmailHost.Text);
                command.Parameters.AddWithValue("@EnableSSL", ddlEnableSSL.SelectedValue);
                command.Parameters.AddWithValue("@EmailPort", tbEmailPort.Text);
                command.Parameters.AddWithValue("@CO2Index", ddlCO2Index.SelectedValue);
                if (ddlPriceTransactionNode.SelectedValue == "-1")
                    command.Parameters.AddWithValue("@PriceTransactionNode", DBNull.Value);
                else
                    command.Parameters.AddWithValue("@PriceTransactionNode", ddlPriceTransactionNode.SelectedValue);
                command.Parameters.AddWithValue("@EnergyPrice1", tbEnergyPrice1.Text);
                command.Parameters.AddWithValue("@EnergyPrice2", tbEnergyPrice2.Text);
                command.Parameters.AddWithValue("@EnergyPrice3", tbEnergyPrice3.Text);
                command.Parameters.AddWithValue("@EnergyPrice4", tbEnergyPrice4.Text);
                command.Parameters.AddWithValue("@EnergyPrice5", tbEnergyPrice5.Text);
                command.Parameters.AddWithValue("@EnergyPrice6", tbEnergyPrice6.Text);
                command.Parameters.AddWithValue("@EnergyPrice7", tbEnergyPrice7.Text);
                command.Parameters.AddWithValue("@EnergyPrice8", tbEnergyPrice8.Text);
                command.Parameters.AddWithValue("@EnergyPrice9", tbEnergyPrice9.Text);
                command.Parameters.AddWithValue("@EnergyPrice10", tbEnergyPrice10.Text);
                command.Parameters.AddWithValue("@EnergyPrice11", tbEnergyPrice11.Text);
                command.Parameters.AddWithValue("@EnergyPrice12", tbEnergyPrice12.Text);
                command.Parameters.AddWithValue("@EnergyPrice13", tbEnergyPrice13.Text);
                command.Parameters.AddWithValue("@EnergyPrice14", tbEnergyPrice14.Text);
                command.Parameters.AddWithValue("@EnergyPrice15", tbEnergyPrice15.Text);
                command.Parameters.AddWithValue("@EnergyPrice16", tbEnergyPrice16.Text);
                command.Parameters.AddWithValue("@EnergyPrice17", tbEnergyPrice17.Text);
                command.Parameters.AddWithValue("@EnergyPrice18", tbEnergyPrice18.Text);
                command.Parameters.AddWithValue("@EnergyPrice19", tbEnergyPrice19.Text);
                command.Parameters.AddWithValue("@EnergyPrice20", tbEnergyPrice20.Text);
                command.Parameters.AddWithValue("@EnergyPrice21", tbEnergyPrice21.Text);
                command.Parameters.AddWithValue("@EnergyPrice22", tbEnergyPrice22.Text);
                command.Parameters.AddWithValue("@EnergyPrice23", tbEnergyPrice23.Text);
                command.Parameters.AddWithValue("@EnergyPrice24", tbEnergyPrice24.Text);

                command.Parameters.AddWithValue("@PriceAdjustment1", tbPriceAdjustment1.Text);
                command.Parameters.AddWithValue("@PriceAdjustment2", tbPriceAdjustment2.Text);
                command.Parameters.AddWithValue("@PriceAdjustment3", tbPriceAdjustment3.Text);
                command.Parameters.AddWithValue("@PriceAdjustment4", tbPriceAdjustment4.Text);
                command.Parameters.AddWithValue("@PriceAdjustment5", tbPriceAdjustment5.Text);
                command.Parameters.AddWithValue("@PriceAdjustment6", tbPriceAdjustment6.Text);
                command.Parameters.AddWithValue("@PriceAdjustment7", tbPriceAdjustment7.Text);
                command.Parameters.AddWithValue("@PriceAdjustment8", tbPriceAdjustment8.Text);
                command.Parameters.AddWithValue("@PriceAdjustment9", tbPriceAdjustment9.Text);
                command.Parameters.AddWithValue("@PriceAdjustment10", tbPriceAdjustment10.Text);
                command.Parameters.AddWithValue("@PriceAdjustment11", tbPriceAdjustment11.Text);
                command.Parameters.AddWithValue("@PriceAdjustment12", tbPriceAdjustment12.Text);
                command.Parameters.AddWithValue("@PriceAdjustment13", tbPriceAdjustment13.Text);
                command.Parameters.AddWithValue("@PriceAdjustment14", tbPriceAdjustment14.Text);
                command.Parameters.AddWithValue("@PriceAdjustment15", tbPriceAdjustment15.Text);
                command.Parameters.AddWithValue("@PriceAdjustment16", tbPriceAdjustment16.Text);
                command.Parameters.AddWithValue("@PriceAdjustment17", tbPriceAdjustment17.Text);
                command.Parameters.AddWithValue("@PriceAdjustment18", tbPriceAdjustment18.Text);
                command.Parameters.AddWithValue("@PriceAdjustment19", tbPriceAdjustment19.Text);
                command.Parameters.AddWithValue("@PriceAdjustment20", tbPriceAdjustment20.Text);
                command.Parameters.AddWithValue("@PriceAdjustment21", tbPriceAdjustment21.Text);
                command.Parameters.AddWithValue("@PriceAdjustment22", tbPriceAdjustment22.Text);
                command.Parameters.AddWithValue("@PriceAdjustment23", tbPriceAdjustment23.Text);
                command.Parameters.AddWithValue("@PriceAdjustment24", tbPriceAdjustment24.Text);

                command.Parameters.AddWithValue("@AllowUserAccountExpiration", ddlAllowUserAccountExpiration.SelectedValue);
                command.Parameters.AddWithValue("@EVUserAccountTypeID", ddlEVUserAccountType.SelectedValue);
                command.Parameters.AddWithValue("@RTMCUserAccountTypeID", ddlRTMCUserAccountType.SelectedValue);

                command.Parameters.AddWithValue("@ConsumerKey", tbConsumerKey.Text);
                command.Parameters.AddWithValue("@ConsumerSecret", tbConsumerSecret.Text);
                command.Parameters.AddWithValue("@AccessToken", tbAccessToken.Text);
                command.Parameters.AddWithValue("@AccessTokenSecret", tbAccessTokenSecret.Text);

                command.Parameters.AddWithValue("@Level1EnergyRetailAdjustment", tbLevel1EnergyRetailAdjustment.Text);
                command.Parameters.AddWithValue("@Level2EnergyRetailAdjustment", tbLevel2EnergyRetailAdjustment.Text);

                string strPasswordText = tbPassword.Text;
                string strEncodedPassword = string.Empty;
                
                using (RijndaelManaged myR = new RijndaelManaged())
                {
                    byte[] byteRijKey = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                    byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                    byte[] byteEncryptText = EncryptStringToBytes(strPasswordText, byteRijKey, byteRijIV);

                    strEncodedPassword = Convert.ToBase64String(byteEncryptText);
                }

                command.Parameters.AddWithValue("@EmailPassword", strEncodedPassword);
                command.Parameters.AddWithValue("@AdminRole", guidAdmin.ToString());                
                command.Parameters.AddWithValue("@MaintainerRole", guidMaintain.ToString());
                command.Parameters.AddWithValue("@OperatorRole", guidOperator.ToString());
                command.Parameters.AddWithValue("@UserRole", guidUser.ToString());
                                
                Guid guidID;
                guidID = Guid.NewGuid();

                command.Parameters.AddWithValue("@ID", guidID.ToString());

                command.ExecuteNonQuery();
                transaction.Commit();
                btnUpdate.Visible = false;
            }
            catch (Exception ex)
            {
                ShowMessage("btnNewClick2, Databases transactions have been Rolled back. Error Message: " + ex.Message);
                blnPasses = false;
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    ShowMessage("Transaction Rollback Error: " + ex2.GetType());
                }
               
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();

                if (readerProfile != null)
                    readerProfile.Close();
            }

            if (blnPasses)
            {

                voidPopulategvOrganization();
                blnfillintb();
                voidClearAlltbs();
                voidClearAlllblErrors();
                gvOrganization.SelectedIndex = -1;
                PopUpError("New Information Added");
                HideMessage();
            }
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            gvOrganization.SelectedIndex = -1;
            voidClearAlltbs();
            voidClearAlllblErrors();
            btnUpdate.Visible = false;
        }
        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }
        #endregion

        #region Helper Functions

        protected void ShowMessage(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void HideMessage()
        {
            lblCatchError.Text = string.Empty;
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected bool blnfillintb()
        {
            GridViewRow gvRow;

            try
            {
                gvRow = gvOrganization.Rows[gvOrganization.SelectedIndex];
            }
            catch
            {
                voidClearAlltbs();
                return false;
            }
            try
            {                
                tbOrganization.Text = gvRow.Cells[findGVcolumn("Organization")].Text;
                ddlState.SelectedValue = gvRow.Cells[findGVcolumn("State")].Text;
                string strLatitude = gvRow.Cells[findGVcolumn("Latitude")].Text;
                string strLongitude = gvRow.Cells[findGVcolumn("Longitude")].Text;
                string strEmail = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Email")].Text);                

                string strEmailHost = gvRow.Cells[findGVcolumn("Email Host")].Text;
                string strEmailPort = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Email Port")].Text);
                string strPriceTrans = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Transaction Node")].Text);
                string strEnergy1 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 1")].Text);
                string strEnergy2 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 2")].Text);
                string strEnergy3 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 3")].Text);
                string strEnergy4 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 4")].Text);
                string strEnergy5 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 5")].Text);
                string strEnergy6 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 6")].Text);
                string strEnergy7 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 7")].Text);
                string strEnergy8 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 8")].Text);
                string strEnergy9 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 9")].Text);
                string strEnergy10 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 10")].Text);
                string strEnergy11 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 11")].Text);
                string strEnergy12 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 12")].Text);
                string strEnergy13 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 13")].Text);
                string strEnergy14 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 14")].Text);
                string strEnergy15 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 15")].Text);
                string strEnergy16 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 16")].Text);
                string strEnergy17 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 17")].Text);
                string strEnergy18 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 18")].Text);
                string strEnergy19 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 19")].Text);
                string strEnergy20 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 20")].Text);
                string strEnergy21 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 21")].Text);
                string strEnergy22 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 22")].Text);
                string strEnergy23 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 23")].Text);
                string strEnergy24 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Energy Price 24")].Text);

                string strPriceAdjustment1 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 1")].Text);
                string strPriceAdjustment2 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 2")].Text);
                string strPriceAdjustment3 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 3")].Text);
                string strPriceAdjustment4 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 4")].Text);
                string strPriceAdjustment5 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 5")].Text);
                string strPriceAdjustment6 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 6")].Text);
                string strPriceAdjustment7 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 7")].Text);
                string strPriceAdjustment8 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 8")].Text);
                string strPriceAdjustment9 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 9")].Text);
                string strPriceAdjustment10 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 10")].Text);
                string strPriceAdjustment11 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 11")].Text);
                string strPriceAdjustment12 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 12")].Text);
                string strPriceAdjustment13 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 13")].Text);
                string strPriceAdjustment14 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 14")].Text);
                string strPriceAdjustment15 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 15")].Text);
                string strPriceAdjustment16 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 16")].Text);
                string strPriceAdjustment17 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 17")].Text);
                string strPriceAdjustment18 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 18")].Text);
                string strPriceAdjustment19 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 19")].Text);
                string strPriceAdjustment20 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 20")].Text);
                string strPriceAdjustment21 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 21")].Text);
                string strPriceAdjustment22 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 22")].Text);
                string strPriceAdjustment23 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 23")].Text);
                string strPriceAdjustment24 = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Price Adjustment 24")].Text);

                string strConsumerKey = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Consumer Key")].Text);
                string strConsumerSecret = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Consumer Secret")].Text);
                string strAccessToken = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Access Token")].Text);
                string strAccessTokenSecret = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Access Token Secret")].Text);

                string strAllowUserAccountExpiration = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Allow User Account Expiration")].Text);
                string strEVUserAccountType = Server.HtmlDecode(gvRow.Cells[findGVcolumn("EV User Account Type ID")].Text);
                string strRTMCUserAccountType = Server.HtmlDecode(gvRow.Cells[findGVcolumn("RTMC User Account Type ID")].Text);

                string strLevel1EnergyRetailAdjustment =
                    Server.HtmlDecode(gvRow.Cells[findGVcolumn("Level1 Energy Retail Adjustment")].Text);
                string strLevel2EnergyRetailAdjustment =
                    Server.HtmlDecode(gvRow.Cells[findGVcolumn("Level2 Energy Retail Adjustment")].Text);

                tbLatitude.Text = string.IsNullOrWhiteSpace(strLatitude) ? string.Empty : strLatitude;
                tbLongitude.Text = string.IsNullOrWhiteSpace(strLongitude) ? string.Empty : strLongitude;
                tbEmail.Text = string.IsNullOrWhiteSpace(strEmail) ? string.Empty : strEmail;
                tbEmailHost.Text = string.IsNullOrWhiteSpace(strEmailHost) ? string.Empty : strEmailHost;
                tbEmailPort.Text = string.IsNullOrWhiteSpace(strEmailPort) ? string.Empty : strEmailPort;
                ddlPriceTransactionNode.SelectedValue = string.IsNullOrWhiteSpace(strPriceTrans) ? "-1" : strPriceTrans;
                tbEnergyPrice1.Text = string.IsNullOrWhiteSpace(strEnergy1) ? "0" : strEnergy1;
                tbEnergyPrice2.Text = string.IsNullOrWhiteSpace(strEnergy2) ? "0" : strEnergy2;
                tbEnergyPrice3.Text = string.IsNullOrWhiteSpace(strEnergy3) ? "0" : strEnergy3;
                tbEnergyPrice4.Text = string.IsNullOrWhiteSpace(strEnergy4) ? "0" : strEnergy4;
                tbEnergyPrice5.Text = string.IsNullOrWhiteSpace(strEnergy5) ? "0" : strEnergy5;
                tbEnergyPrice6.Text = string.IsNullOrWhiteSpace(strEnergy6) ? "0" : strEnergy6;
                tbEnergyPrice7.Text = string.IsNullOrWhiteSpace(strEnergy7) ? "0" : strEnergy7;
                tbEnergyPrice8.Text = string.IsNullOrWhiteSpace(strEnergy8) ? "0" : strEnergy8;
                tbEnergyPrice9.Text = string.IsNullOrWhiteSpace(strEnergy9) ? "0" : strEnergy9;
                tbEnergyPrice10.Text = string.IsNullOrWhiteSpace(strEnergy10) ? "0" : strEnergy10;
                tbEnergyPrice11.Text = string.IsNullOrWhiteSpace(strEnergy11) ? "0" : strEnergy11;
                tbEnergyPrice12.Text = string.IsNullOrWhiteSpace(strEnergy12) ? "0" : strEnergy12;
                tbEnergyPrice13.Text = string.IsNullOrWhiteSpace(strEnergy13) ? "0" : strEnergy13;
                tbEnergyPrice14.Text = string.IsNullOrWhiteSpace(strEnergy14) ? "0" : strEnergy14;
                tbEnergyPrice15.Text = string.IsNullOrWhiteSpace(strEnergy15) ? "0" : strEnergy15;
                tbEnergyPrice16.Text = string.IsNullOrWhiteSpace(strEnergy16) ? "0" : strEnergy16;
                tbEnergyPrice17.Text = string.IsNullOrWhiteSpace(strEnergy17) ? "0" : strEnergy17;
                tbEnergyPrice18.Text = string.IsNullOrWhiteSpace(strEnergy18) ? "0" : strEnergy18;
                tbEnergyPrice19.Text = string.IsNullOrWhiteSpace(strEnergy19) ? "0" : strEnergy19;
                tbEnergyPrice20.Text = string.IsNullOrWhiteSpace(strEnergy20) ? "0" : strEnergy20;
                tbEnergyPrice21.Text = string.IsNullOrWhiteSpace(strEnergy21) ? "0" : strEnergy21;
                tbEnergyPrice22.Text = string.IsNullOrWhiteSpace(strEnergy22) ? "0" : strEnergy22;
                tbEnergyPrice23.Text = string.IsNullOrWhiteSpace(strEnergy23) ? "0" : strEnergy23;
                tbEnergyPrice24.Text = string.IsNullOrWhiteSpace(strEnergy24) ? "0" : strEnergy24;

                tbPriceAdjustment1.Text = string.IsNullOrWhiteSpace(strPriceAdjustment1) ? "0" : strPriceAdjustment1;
                tbPriceAdjustment2.Text = string.IsNullOrWhiteSpace(strPriceAdjustment2) ? "0" : strPriceAdjustment2;
                tbPriceAdjustment3.Text = string.IsNullOrWhiteSpace(strPriceAdjustment3) ? "0" : strPriceAdjustment3;
                tbPriceAdjustment4.Text = string.IsNullOrWhiteSpace(strPriceAdjustment4) ? "0" : strPriceAdjustment4;
                tbPriceAdjustment5.Text = string.IsNullOrWhiteSpace(strPriceAdjustment5) ? "0" : strPriceAdjustment5;
                tbPriceAdjustment6.Text = string.IsNullOrWhiteSpace(strPriceAdjustment6) ? "0" : strPriceAdjustment6;
                tbPriceAdjustment7.Text = string.IsNullOrWhiteSpace(strPriceAdjustment7) ? "0" : strPriceAdjustment7;
                tbPriceAdjustment8.Text = string.IsNullOrWhiteSpace(strPriceAdjustment8) ? "0" : strPriceAdjustment8;
                tbPriceAdjustment9.Text = string.IsNullOrWhiteSpace(strPriceAdjustment9) ? "0" : strPriceAdjustment9;
                tbPriceAdjustment10.Text = string.IsNullOrWhiteSpace(strPriceAdjustment10) ? "0" : strPriceAdjustment10;
                tbPriceAdjustment11.Text = string.IsNullOrWhiteSpace(strPriceAdjustment11) ? "0" : strPriceAdjustment11;
                tbPriceAdjustment12.Text = string.IsNullOrWhiteSpace(strPriceAdjustment12) ? "0" : strPriceAdjustment12;
                tbPriceAdjustment13.Text = string.IsNullOrWhiteSpace(strPriceAdjustment13) ? "0" : strPriceAdjustment13;
                tbPriceAdjustment14.Text = string.IsNullOrWhiteSpace(strPriceAdjustment14) ? "0" : strPriceAdjustment14;
                tbPriceAdjustment15.Text = string.IsNullOrWhiteSpace(strPriceAdjustment15) ? "0" : strPriceAdjustment15;
                tbPriceAdjustment16.Text = string.IsNullOrWhiteSpace(strPriceAdjustment16) ? "0" : strPriceAdjustment16;
                tbPriceAdjustment17.Text = string.IsNullOrWhiteSpace(strPriceAdjustment17) ? "0" : strPriceAdjustment17;
                tbPriceAdjustment18.Text = string.IsNullOrWhiteSpace(strPriceAdjustment18) ? "0" : strPriceAdjustment18;
                tbPriceAdjustment19.Text = string.IsNullOrWhiteSpace(strPriceAdjustment19) ? "0" : strPriceAdjustment19;
                tbPriceAdjustment20.Text = string.IsNullOrWhiteSpace(strPriceAdjustment20) ? "0" : strPriceAdjustment20;
                tbPriceAdjustment21.Text = string.IsNullOrWhiteSpace(strPriceAdjustment21) ? "0" : strPriceAdjustment21;
                tbPriceAdjustment22.Text = string.IsNullOrWhiteSpace(strPriceAdjustment22) ? "0" : strPriceAdjustment22;
                tbPriceAdjustment23.Text = string.IsNullOrWhiteSpace(strPriceAdjustment23) ? "0" : strPriceAdjustment23;
                tbPriceAdjustment24.Text = string.IsNullOrWhiteSpace(strPriceAdjustment24) ? "0" : strPriceAdjustment24;

                tbLevel1EnergyRetailAdjustment.Text = strLevel1EnergyRetailAdjustment;
                tbLevel2EnergyRetailAdjustment.Text = strLevel2EnergyRetailAdjustment;

                tbConsumerKey.Text = string.IsNullOrWhiteSpace(strConsumerKey) ? string.Empty : strConsumerKey;
                tbConsumerSecret.Text = string.IsNullOrWhiteSpace(strConsumerSecret) ? string.Empty : strConsumerSecret;
                tbAccessToken.Text = string.IsNullOrWhiteSpace(strAccessToken) ? string.Empty : strAccessToken;
                tbAccessTokenSecret.Text = string.IsNullOrWhiteSpace(strAccessTokenSecret) ? string.Empty : strAccessTokenSecret;

                ddlAllowUserAccountExpiration.SelectedValue = strAllowUserAccountExpiration;
                ddlEVUserAccountType.SelectedValue = strEVUserAccountType;
                ddlRTMCUserAccountType.SelectedValue = strRTMCUserAccountType;

                CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
                if (cbActivate.Checked)
                    ddlActivate.SelectedValue = "1";
                else
                    ddlActivate.SelectedValue = "0";

                CheckBox cbEnableSSL = (CheckBox)gvRow.Cells[findGVcolumn("Enable SSL")].Controls[0];
                if (cbEnableSSL.Checked)
                    ddlEnableSSL.SelectedValue = "1"; 
                else
                    ddlEnableSSL.SelectedValue = "0";

                CheckBox cbCO2Index = (CheckBox)gvRow.Cells[findGVcolumn("CO2 Index")].Controls[0];
                if (cbCO2Index.Checked)
                    ddlCO2Index.SelectedValue = "1";
                else
                    ddlCO2Index.SelectedValue = "0";
            }
            catch (Exception Ex)
            {
                lblCatchError.Visible = true;
                lblCatchError.Text = "blnfillintb Error: " + Ex.Message;
                btnHideCatchError.Visible = true;
                return false;
            }
            try
            {
                string strPassword = gvRow.Cells[findGVcolumn("Email Password")].Text;
                if (!string.IsNullOrEmpty(strPassword))
                {
                    //var varDecryptByte = MachineKey.Decode(strPassword, MachineKeyProtection.All);
                    // put error message if doesn't work
                   //string strEncodedPassword = Encoding.UTF8.GetString(varDecryptByte);

                    using (RijndaelManaged myR = new RijndaelManaged())
                    {

                        byte[] byteRijKey = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijKey"]);
                        byte[] byteRijIV = Convert.FromBase64String(WebConfigurationManager.AppSettings["RijIV"]);
                        byte[] bytePassword = Convert.FromBase64String(strPassword);
                        //byte[] byteEncryptText = EncryptStringToBytes(strPasswordText, byteRijKey, byteRijIV);

                       // strEncodedPassword = byteEncryptText.ToString();

                        string strEncodedPassword = DecryptStringFromBytes(bytePassword, byteRijKey, byteRijIV);

                        tbPassword.Text = strEncodedPassword;
                        tbPassword.Attributes.Add("value", strEncodedPassword);
                    }                  
                    
                }
                else
                {
                    tbPassword.Text = string.Empty;
                }
            }
            catch
            {
            }
            return true;
        }

        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            voidShowAllFunction();
        }

        protected void voidShowAllFunction()
        {
            if (cbShowActivated.Checked)
            {
                CheckBox cbActivate;
                for (int i = 0; i < gvOrganization.Rows.Count; i++)
                {
                    cbActivate = (CheckBox)gvOrganization.Rows[i].Cells[findGVcolumn("Activate")].Controls[0];
                    if (!cbActivate.Checked)
                    {
                        gvOrganization.Rows[i].Visible = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < gvOrganization.Rows.Count; i++)
                {
                    gvOrganization.Rows[i].Visible = true;
                }
            }
        }


        protected void voidClearAlltbs()
        {
            tbOrganization.Text = string.Empty;
            ddlState.SelectedIndex = 0;
            tbLatitude.Text = string.Empty;
            tbLongitude.Text = string.Empty;
            tbEmail.Text = string.Empty;
            tbPassword.Text = string.Empty;
            tbPassword.Attributes.Add("value", string.Empty);
            ddlActivate.SelectedIndex = 0;
            ddlEnableSSL.SelectedIndex = 0;
            ddlCO2Index.SelectedIndex = 0;

            tbEmailHost.Text = string.Empty;
            tbEmailPort.Text = string.Empty;
            //tbPriceTransactionNode.Text = string.Empty;
            tbEnergyPrice1.Text = string.Empty;
            tbEnergyPrice2.Text = string.Empty;
            tbEnergyPrice3.Text = string.Empty;
            tbEnergyPrice4.Text = string.Empty;
            tbEnergyPrice5.Text = string.Empty;
            tbEnergyPrice6.Text = string.Empty;
            tbEnergyPrice7.Text = string.Empty;
            tbEnergyPrice8.Text = string.Empty;
            tbEnergyPrice9.Text = string.Empty;
            tbEnergyPrice10.Text = string.Empty;
            tbEnergyPrice11.Text = string.Empty;
            tbEnergyPrice12.Text = string.Empty;
            tbEnergyPrice13.Text = string.Empty;
            tbEnergyPrice14.Text = string.Empty;
            tbEnergyPrice15.Text = string.Empty;
            tbEnergyPrice16.Text = string.Empty;
            tbEnergyPrice17.Text = string.Empty;
            tbEnergyPrice18.Text = string.Empty;
            tbEnergyPrice19.Text = string.Empty;
            tbEnergyPrice20.Text = string.Empty;
            tbEnergyPrice21.Text = string.Empty;
            tbEnergyPrice22.Text = string.Empty;
            tbEnergyPrice23.Text = string.Empty;
            tbEnergyPrice24.Text = string.Empty;

            tbPriceAdjustment1.Text = string.Empty;
            tbPriceAdjustment2.Text = string.Empty;
            tbPriceAdjustment3.Text = string.Empty;
            tbPriceAdjustment4.Text = string.Empty;
            tbPriceAdjustment5.Text = string.Empty;
            tbPriceAdjustment6.Text = string.Empty;
            tbPriceAdjustment7.Text = string.Empty;
            tbPriceAdjustment8.Text = string.Empty;
            tbPriceAdjustment9.Text = string.Empty;
            tbPriceAdjustment10.Text = string.Empty;
            tbPriceAdjustment11.Text = string.Empty;
            tbPriceAdjustment12.Text = string.Empty;
            tbPriceAdjustment13.Text = string.Empty;
            tbPriceAdjustment14.Text = string.Empty;
            tbPriceAdjustment15.Text = string.Empty;
            tbPriceAdjustment16.Text = string.Empty;
            tbPriceAdjustment17.Text = string.Empty;
            tbPriceAdjustment18.Text = string.Empty;
            tbPriceAdjustment19.Text = string.Empty;
            tbPriceAdjustment20.Text = string.Empty;
            tbPriceAdjustment21.Text = string.Empty;
            tbPriceAdjustment22.Text = string.Empty;
            tbPriceAdjustment23.Text = string.Empty;
            tbPriceAdjustment24.Text = string.Empty;
            tbConsumerKey.Text = string.Empty;
            tbConsumerSecret.Text = string.Empty;
            tbAccessToken.Text = string.Empty;
            tbAccessTokenSecret.Text = string.Empty;
            ddlEVUserAccountType.SelectedIndex = 0;
            ddlRTMCUserAccountType.SelectedIndex = 0;
            tbLevel1EnergyRetailAdjustment.Text = "0";
            tbLevel2EnergyRetailAdjustment.Text = "0";
        }

        protected void voidClearAlllblErrors()
        {
            lbltbOrganizationError.Text = string.Empty;
            lblddlStateError.Text = string.Empty;
            lbltbLatitudeError.Text = string.Empty;
            lbltbLongitudeError.Text = string.Empty;
            lbltbEmailError.Text = string.Empty;
            lbltbPasswordError.Text = string.Empty;
        }


        // Find the associated column in the gridview with the specified name
        // Return -1 if not found, otherwise return the int of the column.
        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvOrganization.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvOrganization.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }


        #endregion
        #region ValidationChecks
        protected bool blnupdateValidationCheck()
        {
            bool blnPasses = true;

            if (ddlState.SelectedIndex == 0)
            {
                blnPasses = false;
                lblddlStateError.Text = "Select a State.";
            }

            if (tbPassword.Text.Length < Membership.MinRequiredPasswordLength)
            {
                blnPasses = false;
                lbltbPasswordError.Text = "Password must be atleast 6 characters long";
            }

            if (!blnCheckForDuplicateName("Update"))
            {
                blnPasses = false;
                lbltbOrganizationError.Text = "Organization name must be unique.";
            }

            return blnPasses;
        }

        protected bool blnNewValidationCheck()
        {
            bool blnPasses = true;
            if (ddlState.SelectedIndex == 0)
            {
                blnPasses = false;
                lblddlStateError.Text = "Select a State";
            }

            if (tbPassword.Text.Length < Membership.MinRequiredPasswordLength)
            {
                blnPasses = false;
                lbltbPasswordError.Text = "Password must be atleast 6 characters long";
            }

            if (!blnCheckForDuplicateName("New"))
            {
                blnPasses = false;
                lbltbOrganizationError.Text = "Organization name must be unique.";
            }

            return blnPasses;
        }

        protected bool blnCheckForDuplicateName(string UpdateOrNew)
        {
            // Obtain Current Selected Name
            string strCurrentID = string.Empty;
            if (UpdateOrNew == "Update")
            {
                GridViewRow gvRow = gvOrganization.Rows[gvOrganization.SelectedIndex];
                string strCurrentName = gvRow.Cells[findGVcolumn("Organization")].Text; // IP Address of current
                strCurrentID = gvRow.Cells[findGVcolumn("ID")].Text;

                if (strCurrentName == tbOrganization.Text)
                {
                    return true;
                }
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            bool blnPasses = true;

            try
            {
                strQuery = "SELECT [Name], ID FROM [City]";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                da.Dispose();
                cmd.Dispose();

                if (UpdateOrNew == "Update")
                {
                    for (int i = 0; i < dt.Rows.Count; i++) // This For Loop checks for a duplicate Name within the database
                    {
                        if (dt.Rows[i][1].ToString() != strCurrentID)
                        {
                            if (dt.Rows[i][0].ToString() == tbOrganization.Text)
                            {
                                blnPasses = false;
                                break;
                            }
                        }
                    }
                }
                else // if UpdateOrNew == "New"
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() == tbOrganization.Text)
                        {
                            blnPasses = false;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                PopUpError("blnCheckForDuplicateName Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                {
                    cnn.Close();
                }
            }
            return blnPasses;
        }
        #endregion

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

        protected void cbShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (cbShowPassword.Checked)
            {
                tbPassword.TextMode = TextBoxMode.SingleLine;
            }
            else
                tbPassword.TextMode = TextBoxMode.Password;

            try
            {
                blnfillintb();
            }
            catch
            {
            }
        }
    }
}