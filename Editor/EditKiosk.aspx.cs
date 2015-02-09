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
using RTMC;
using System.Security.Cryptography;
using System.IO;


namespace EVEditor
{
    public partial class EditKiosk : System.Web.UI.Page
    {
        // Connection string to access database.
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        // Columns to hide in the gridview/table.  To add or remove, simply put the HEADER name into this string array.
        string[] ColumnsToHide = {"KioskID", "CityGUID", "ParkingLotID", "Level", "GatewayID", "Max Current", "Max Voltage", "Max Station Current", "Max Station Voltage", "In Days", "Max Times In Day",
                                  "Source Current", "Source Voltage","TransactionServiceProviderID", "Current Threshold", "PrimaryEmail", "Left Current Threshold"  };


        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };
        //string[] strArrRolesToAllow = { "General Administrator", "Santa Monica Administrator" };
        string[] strArrRolesToAllow = { "General Administrator"};
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        string[] strArrTypesToAllow = { "Administrator" };

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {

            List<string> ListOfAdminCities = new List<string>();


            // Authenticate User
            //for test
            if (User.Identity.IsAuthenticated)//
            {
                RolePrincipal rp = (RolePrincipal)User;
                string[] roles = Roles.GetRolesForUser();
                List<string> ListOfRoles = new List<string>();
                for (int i = 0; i < roles.Count(); i++)
                {
                    ListOfRoles.Add(roles[i]); // Add all roles to a list, which we can easily access later in this function
                }

                //for test
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
                // Auto select the cb to only show activated gateways from the kiosk-gateway list
                cbShowActivated.Checked = true;
                Initialize();
            }
        }

        protected void Initialize()
        {
            PopulategvKiosk(string.Empty, string.Empty, string.Empty, cbShowActivated.Checked);
            PopulateddlModeCity();
            PopulateddlModeParkingLot();
            PopulateddlKiosk();
            PopulateddlGateway();
            PopulateddlTransactionProvider();
        }

        #region Init functions

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


        #endregion


        // Populate the gridview
        protected void PopulategvKiosk(string strOrgID, string strParkingLotID, string strKioskID, bool blnActivatedGateways)
        {
            if (strOrgID == "-1")
                strOrgID = string.Empty;

            if (strParkingLotID == "-1")
                strParkingLotID = string.Empty;

            if (strKioskID == "-1")
                strKioskID = string.Empty;

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
                // If no city is selected, then do NOT select all cities, 
                // only choose the city that is selected
                if (strOrgID == string.Empty)
                {
                    foreach (string str in listApprovedRoles)
                    {
                        // listCityGUID now holds the Organization GUIDS to all of the user's roles
                        listCityGUID.Add(ReturnGuidfromCityname(str));
                    }
                }
                // If the strOrgID is not chosen, then only that selected city should be used.
                else
                {
                    listCityGUID.Add(strOrgID);
                }
            }

            // Populate the gridview
            DataTable DT = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                string sqlQuery = "SELECT k.[KioskID] ,k.[KioskName],k.[ParkingLotID],k.[CreditCardPayment],k.[TransactionServiceProviderID],kl.GatewayID " +
                                  " ,  g.Name as GatewayName, g.[IP Address], g.[Charging Level], g.Level, g.[Max Current], g.[Max Station Current]," +
                                  " g.[Max Voltage], g.[Max Station Voltage], g.[Source Current], g.[Source Voltage], g.[Retrieve Interval], g.Enable, g.[Time Quantum], g.[Time Out]," +
                                  " g.[Retry Times], g.CurrentValve, g.NodeControlDelay, g.Activate, g.PrimaryEmail, g.InDays, g.MaxTimesInDay, g.LeftCurrentValve, g.HasSOC, g.Controllable, g.Note,  " +
                                  " pl2.Name as ParkingLotName, c.Name AS ChargingName, c.ID as ChargingID, " +
                                  " city.[Name] as CityName, city.[ID] as CityGUID, " +   
                                  " TSP.[ServiceProviderName] " +
                                  " FROM [EVDemo].[dbo].[Kiosk] as k  " +
                                  " INNER JOIN [EVDemo].[dbo].[KioskGatewayList] as kl ON kl.KioskID=k.KioskID " +
                                  " LEFT JOIN [EVDemo].[dbo].[Gateway] as g ON g.[ID] = kl.GatewayID " +
                                  " INNER JOIN [Parking Lot] AS pl2 ON pl2.ID = g.[Parking Lot ID] " +
                                  " INNER JOIN [Charging Algorithm] AS c ON c.ID = g.[Algorithm ID] " +
                                  " INNER JOIN [City] as city ON city.ID = pl2.[City ID] " +
                                  " INNER JOIN [TransactionServiceProvider] AS TSP ON TSP.[ServiceProviderID] = k.[TransactionServiceProviderID] ";
                
                // if this user is a Master user
                // the user may access all the information
                if(blnListhasMasterRole)
                {
                    // Kiosk is chosen. repopulate
                    if (strKioskID != string.Empty )
                    {
                        
                        sqlQuery += " WHERE k.[KioskName] = '" + strKioskID +"'";
                    }
                    // if a Parking lot is chosen
                    else if (strParkingLotID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[ID] = '" + strParkingLotID +"'";
                    }                    
                    // if an organization is chosen, populate the associated data
                    else if (strOrgID != string.Empty)
                    {                     
                        sqlQuery += " WHERE pl2.[City ID] = '" + strOrgID +"'";
                    }
                }
                // the user is not a master user.
                // listCityGUID will contain at least one organization GUID.
                else
                {

                    // if a  specific Kiosk is chosen
                    if (strKioskID != string.Empty)
                    {
                        sqlQuery += " WHERE k.[KioskName] = '" + strKioskID + "'";
                    }
                    else if (strParkingLotID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[ID] = '" + strParkingLotID + "'";
                    }
                    else if (strOrgID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[City ID] = '" + strOrgID + "'";
                    }
                    else
                    {
                        int listCount = listCityGUID.Count;

                        sqlQuery += " WHERE ";
                        for (int i = 0; i < listCount; i++)
                        {
                            sqlQuery += " pl2.[City ID] ='" + listCityGUID[i] + "'";
                            if (i < listCount - 1)
                            {                                
                                sqlQuery += " OR ";
                            }
                        }
                    }
                }

                if(blnActivatedGateways)
                    sqlQuery += " AND k.[Activate] = '1' ";        

                // Order by ascending names
                sqlQuery += " ORDER BY k.[KioskName] ASC ";

                // Perform SQL query.
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
                            lblCatchError.Text += " Error at PopulatGV: " + ex.Message;
                            lblCatchError.Visible = true;
                            btnHideCatchError.Visible = true;
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            ShowMessage("No Data in this selection");
                    }
                }
            }

            Session["data"] = DT;
            gvKiosk.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvKiosk.DataBind(); // Bind data
        }

        #region Return Functions, Return GUid from city name, etc

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
        #endregion

        #region Page Helper Functions

        // After selecting an index in the gvKiosk, update the information to the textboxes
        void fillInTxtBoxes()
        {
            // gvRow will hold the information of the selected indexed row.
            GridViewRow gvRow;

            try
            {
                gvRow = gvKiosk.Rows[gvKiosk.SelectedIndex];
            }
            // If error, clear all the textboxes and return.
            catch
            {
                voidClearAlltbs();                
                return;
            }
            tbKioskName.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Kiosk Name")].Text);
            ddlParkingLot.SelectedValue = gvRow.Cells[findGVcolumn("ParkingLotID")].Text;
            ddlGatewayName.SelectedValue = gvRow.Cells[findGVcolumn("GatewayID")].Text;
            
            ddlTransactionServiceProvider.SelectedValue = gvRow.Cells[findGVcolumn("TransactionServiceProviderID")].Text;
  
            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Credit Card Payment")].Controls[0];
            if (cbActivate.Checked)
                ddlCreditCardPayment.SelectedValue = "1";            
            else            
                ddlCreditCardPayment.SelectedValue = "0";
            
            // string strTransactionServiceProvider = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Transaction Service Provider ID")].Text);
            // string.IsNullOrWhiteSpace(strTransactionServiceProvider) ? string.Empty : strTransactionServiceProvider;            
        }

        // Show a string Message
        protected void ShowMessage(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        // Hide the Pre displayed message
        protected void HideError()
        {
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        //Popup error using javascript
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

        // Find the associated column in the gridview with the specified name
        // Return -1 if not found, otherwise return the int of the column.
        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvKiosk.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvKiosk.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }


        // Clear all the textboxe and DDL on the page
        protected void voidClearAlltbs()
        {
            tbKioskName.Text = string.Empty;
            ddlParkingLot.SelectedIndex = 0;            
            ddlCreditCardPayment.SelectedIndex = 0;
            ddlGatewayName.SelectedIndex = 0;
            ddlTransactionServiceProvider.SelectedIndex = 0;
        }

        // Clear all the label error message on the page
        protected void voidClearAlllblErrors()
        {
            lbltbKioskNameError.Text = string.Empty;            
            lblddlParkingLotError.Text = string.Empty;
            
            lblddlCreditCardPaymentError.Text = string.Empty;
            lblddlTransactionServiceProviderError.Text = string.Empty;
        }
        #endregion

        #region gvKiosk Tools (Paging, Deleting, Selected, Sorting)

        protected void gvKioskPaging(object sender, GridViewPageEventArgs e)
        {
            gvKiosk.SelectedIndex = -1;
            voidClearAlltbs();
            DataTable dataTable = Session["data"] as DataTable;

            gvKiosk.PageIndex = e.NewPageIndex;
            gvKiosk.DataSource = dataTable;
            gvKiosk.DataBind();
            

        }

        protected void gvKioskRowCreated(object sender, GridViewRowEventArgs e)
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
        protected void gvKioskSelectedIndex(object sender, EventArgs e)
        {
            HideError();
            voidClearAlllblErrors();
            voidClearAlltbs();

            ddlParkingLot.SelectedIndex = ddlModeParkingLot.SelectedIndex;
            ddlGatewayName.Items.Clear();
            PopulateddlGateway();

            ddlGatewayName.SelectedIndex = 0;            
            
            fillInTxtBoxes();

            btnUpdate.Visible = true;
            btnDelete.Visible = true;
        }

        protected void gvKioskSorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvKiosk.DataSource = dataTable.DefaultView;
                gvKiosk.DataBind();
            }
            gvKiosk.SelectedIndex = -1;
            voidClearAlltbs();
            
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvKiosk.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvKiosk.Columns.IndexOf(field);
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
            gvKiosk.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        #region populateDDL functions

        // Populate the ddlGateway dropdownlist.
        protected void PopulateddlGateway()
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


            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strZerothMessage = string.Empty;

                // If the user is a MASTER role
                if (blnListhasMasterRole)
                {
                    strQuery = "SELECT g.ID, g.Name FROM Gateway as g INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.[ID] "; 
                    
                    // Priority: Parking lot -> Organization -> General.


                    if (ddlParkingLot.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.ID = '" + ddlParkingLot.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "Associated Charging Boxes";
                    }   
                    else if (ddlModeParkingLot.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.ID = '" + ddlModeParkingLot.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "Associated Charging Boxes";
                    } 
                    else if (ddlModeCity.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.[City ID]='" + ddlModeCity.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";

                        // The different strZerothMessage are used to denote a selected city for aesthetic purposes.
                        strZerothMessage = "Associated Charging Boxes";
                    }                    
                    else
                    {
                        strQuery += " WHERE g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "All Charging Boxes";
                    }
                }
                // If the user is not a master role, then only populate with the associated gateways
                else
                {
                    strQuery = " SELECT g.ID, g.Name FROM Gateway as g INNER JOIN [Parking Lot] pl ON pl.ID = g.[Parking Lot ID] ";

                    // Priority: Parking lot -> Organization -> General.

                    if (ddlParkingLot.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.ID = '" + ddlParkingLot.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "Associated Charging Boxes";
                    }
                    else if (ddlModeParkingLot.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.ID = '" + ddlModeParkingLot.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "Associated Charging Boxes";
                    }
                    else if (ddlModeCity.SelectedValue == "-1")
                    {
                        int listCount = listCityGUID.Count;
                        strQuery += " WHERE ";
                        for (int i = 0; i < listCount; i++)
                        {
                            strQuery += " pl.[City ID] = '" + listCityGUID[i] + "' ";
                            if (i < listCount - 1)
                            {
                                strQuery += " OR ";
                            }
                        }
                    }
                    else // for all other cases
                    {
                        strQuery += " WHERE pl.[City ID] ='" + ddlModeCity.SelectedValue +"'";
                    }
                    strZerothMessage = "Associated Charging Boxes";
                }   

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();

                da.Fill(dt);

                if (dt.Rows.Count != 0)
                {
                    ddlGatewayName.DataSource = dt;
                    ddlGatewayName.DataValueField = "ID"; // DataValueField contains the GUID of the Gateway
                    ddlGatewayName.DataTextField = "Name"; // DataTextField contains the Name of the Gateway
                    ddlGatewayName.DataBind();
                }
                else
                {
                    strZerothMessage = "No Charging Boxes";
                }
                ListItem li = new ListItem(strZerothMessage, "-1");
                ddlGatewayName.Items.Insert(0, li);

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("PopulateddlGateway Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            try
            {
                ddlGatewayName.SelectedIndex = 0;
            }
            catch
            {
            }
        }


        // Populate the ddlModeCity and the ddlOrganization drop down lists 
        protected void PopulateddlModeCity()
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

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;            

            try
            {
                // Only populate the corresponding organizations associated with the
                // logged in user
                string strListItemInsert = "";
                if (blnListhasMasterRole)
                {
                    strQuery = "SELECT ID, Name FROM City WHERE Activate= 1 ORDER BY Name";
                    strListItemInsert = "All Cities";
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

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();

                da.Fill(dt);
                ddlModeCity.DataSource = dt;
                ddlModeCity.DataValueField = "ID"; // DataValueField contains the GUID of the City
                ddlModeCity.DataTextField = "Name"; // DataTextField contains the Name of the City
                ddlModeCity.DataBind();

                if (strListItemInsert != "")
                {
                    ListItem li = new ListItem(strListItemInsert, "-1");
                    ddlModeCity.Items.Insert(0, li);
                }
                
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("PopulateddlModeCity Error: " + ex.Message);

            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
        }

        // Populate ddlModeParkingLot
        protected void PopulateddlModeParkingLot() // Populate the Parking Lot Drop Down List
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

            // If no organization is selected, then select all parking lots
            // from the associated cities.
            // To do so, check if the user is a MASTER role or not.
            // Populate the DDL accordingly.           

            if (ddlModeCity.SelectedIndex == 0)
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                try
                {
                    // if the user has a MASTER role, then load all associated parking lots 
                    if (blnListhasMasterRole)
                    {
                        strQuery = "SELECT ID, Name FROM [Parking Lot] ORDER BY Name";                        
                    }
                    else // Not a Master ROle
                    {
                        // Only select the parking lots from the associated cities
                        strQuery = "SELECT ID, Name FROM [Parking Lot] WHERE ";
                        int listCount = listCityGUID.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            strQuery += " [City ID] = '" + listCityGUID[i] + "' ";

                            // Add the " OR " if there are multiple City IDS associated with the user
                            if (i < listCount - 1)
                            {
                                strQuery += " OR ";
                            }
                        }
                    }

                    // Open the connection, run the query, and retrieve the associated parking lots
                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();

                    // Two datatables must be filled in order to properly fill two DDLs.
                    da.Fill(dt);

                    if (dt.Rows.Count != 0)
                    {

                        ddlModeParkingLot.DataSource = dt; // Fill the Drop Down List
                        ddlModeParkingLot.DataValueField = "ID"; // DataValueFIeld contains the GUID of the Parking Lot
                        ddlModeParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlModeParkingLot.DataBind();

                        ddlParkingLot.DataSource = dt; // Fill the Drop Down List
                        ddlParkingLot.DataValueField = "ID"; // DataValueField contains the GUID of the Parking Lot
                        ddlParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlParkingLot.DataBind();

                        ListItem li = new ListItem("All Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("All Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLot.Items.Insert(0, li2);
                    }
                    else
                    {
                        ListItem li = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLot.Items.Insert(0, li2);
                    }


                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("PopulateddlModeParkingLot Error: " + ex.Message);
                }
                finally
                {
                    if (cnn != null)
                        cnn.Close();
                }
            }
            // Else if a specific Organization is selected, then only selected the associated
            // Parking lots connected to that organization.
            else
            {                
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                try
                {
                    strQuery = "SELECT ID, Name FROM [Parking Lot] WHERE [City ID]='" + ddlModeCity.SelectedValue  + "'";

                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();

                    // Two datatables must be used to properly fill two DDLs
                    da.Fill(dt);

                    // Check to see if there are no returned data sets
                    bool blnIsEmptyParkingLot = false;
                    if (dt.Rows.Count == 0)
                    {
                        blnIsEmptyParkingLot = true;
                    }

                    if (!blnIsEmptyParkingLot)
                    {
                        ddlModeParkingLot.DataSource = dt; // Fill the Drop Down List
                        ddlModeParkingLot.DataValueField = "ID"; // DataValueFIeld contains the GUID of the Parking Lot
                        ddlModeParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlModeParkingLot.DataBind();

                        ddlParkingLot.DataSource = dt; // Fill the Drop Down List
                        ddlParkingLot.DataValueField = "ID"; // DataValueField contains the GUID of the Parking Lot
                        ddlParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlParkingLot.DataBind();
                        
                        
                        ListItem li = new ListItem("Associated Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("Associated Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLot.Items.Insert(0, li2);
                    }
                    else
                    {
                        ListItem li = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLot.Items.Insert(0, li2);
                    }
                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("PopulateddlModeParkingLot2 Error: " + ex.Message);
                }
                finally
                {
                    if (cnn != null)
                        cnn.Close();
                }                
            }
        }

        // Populate the ddlTransactionServiceProvider
        protected void PopulateddlTransactionProvider()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT ServiceProviderID, ServiceProviderName FROM [TransactionServiceProvider] ";
                if (ddlCreditCardPayment.SelectedValue == "1")
                {
                    strQuery += "ORDER BY ServiceProviderName";
                }
                else
                {
                    strQuery += "ORDER BY ServiceProviderID";
                }
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();

                da.Fill(dt);
                if (dt.Rows.Count != 0)
                {
                    
                        ddlTransactionServiceProvider.DataSource = dt;
                        ddlTransactionServiceProvider.DataValueField = "ServiceProviderID"; // DataValueField contains the GUID of the City
                        ddlTransactionServiceProvider.DataTextField = "ServiceProviderName"; // DataTextField contains the Name of the City
                        ddlTransactionServiceProvider.DataBind();
                    

                }
                else
                {
                    ListItem li = new ListItem("No Transaction Providers", "-1");
                    ddlTransactionServiceProvider.Items.Insert(0, li);

                        // If there are no transaction providers, disable the update or new feature.
                        ShowMessage("<br> Please populate the TransactionServiceProvider to ensure database integrity");
                        btnNew.Visible = false;
                        btnUpdate.Visible = false;
                        btnDelete.Visible = false;
                    
                }
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("PopulateddlTransactionProvider Error: " + ex.Message);

            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
        }

        // Populate the ddlKiosk
        protected void PopulateddlKiosk()
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

            // Determine if the organization or parking lot is chosen so the
            // ddlKiosk can be populated accordingly.

            bool blnddlOrganizationChosen = false;
            bool blnddlParkingLotChosen = false;

            if (ddlModeCity.SelectedIndex != 0)
                blnddlOrganizationChosen = true;

            if (ddlModeParkingLot.SelectedIndex != 0)
                blnddlParkingLotChosen = true;



            // Start data retrieval

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                // if the user has a MASTER role, then load all associated parking lots 
                if (blnListhasMasterRole)
                {
                    strQuery = "SELECT DISTINCT kl.KioskName FROM [KIOSK] as kl ";                                       


                    // If a Parking lot was chosen, choose the associated Kiosks
                    if(blnddlParkingLotChosen)
                    {
                        strQuery += " WHERE kl.[ParkingLotID] = '"+ ddlModeParkingLot.SelectedValue + "'" ;
                    }  
                    // if a specific organization is chosen,
                    // then only select the Kiosks in that specific organization
                    else if (blnddlOrganizationChosen)
                    {
                        strQuery += " INNER JOIN [Parking Lot] as pl ON pl.[ID] = kl.[ParkingLotID] AND pl.[City ID] = '" + ddlModeCity.SelectedValue + "'";
                    }                                     
                }
                else // if the user is not a MASTER role. then only select specific roles
                {
                    strQuery = " SELECT DISTINCT kl.KioskName from [Kiosk] as kl " + 
                               " INNER JOIN " +
                               " [Parking Lot] as pl ON pl.[ID]= kl.[ParkingLotID] WHERE ";
                    
                    // If a parking lot is chosen, then only populate the associated Kiosks.
                    if(blnddlParkingLotChosen)
                    {
                        strQuery += " pl.[ID] ='" + ddlModeParkingLot.SelectedValue + "'";
                    }
                    else if (blnddlOrganizationChosen)
                    {
                        strQuery += " pl.[City ID] = '" + ddlModeCity.SelectedValue + "'";

                    }
                    else
                    {
                        int listCount = listCityGUID.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            strQuery += " pl.[City ID] ='" + listCityGUID[i] + "' ";

                            // Add the " OR " if there are multiple City IDS associated with the user
                            if (i < listCount - 1)
                            {
                                strQuery += " OR ";
                            }
                        }
                    }
                }

                // Open the connection, run the query, and retrieve the associated parking lots
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();

                da.Fill(dt);

                // Check to see if there are no returned data sets
                bool blnIsEmptyKiosk = false;
                if (dt.Rows.Count == 0)
                {
                    blnIsEmptyKiosk = true;
                }

                if (!blnIsEmptyKiosk)
                {
                    ddlKiosk.DataSource = dt; // Fill the Drop Down List
                    ddlKiosk.DataValueField = "KioskName"; // DataValueFIeld contains the GUID of the Parking Lot
                    ddlKiosk.DataTextField = "KioskName"; // DataTextField contains the Name of the Parking Lot
                    ddlKiosk.DataBind();

                    ListItem li = new ListItem("All Kiosks", "-1"); // Add the Text of "All Parking Lots" to position 0
                    ddlKiosk.Items.Insert(0, li);
                }                 
                else
                {
                    ListItem li = new ListItem("No Kiosks", "-1"); // Add the Text of "All Parking Lots" to position 0
                    ddlKiosk.Items.Insert(0, li);
                }

                ddlKiosk.SelectedIndex = 0;

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("PopulateddlKiosk Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
        }

        //
        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked);
        }


        // Upon the ddlGatewayName change, update the index on the Parking Lot to ensure database integrity.
        protected void ddlGatewayName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlGatewayName.SelectedIndex != 0)
            {
                SqlConnection cnn = new SqlConnection(connectionString);
                string strQuery;
                SqlCommand cmd;
                DataTable dt = null;
                SqlDataAdapter da;

                try
                {
                    strQuery = "SELECT [Parking Lot ID] FROM [Gateway] WHERE [ID] ='" + ddlGatewayName.SelectedValue + "'";


                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();

                    da.Fill(dt);

                    // Set value of ddlParkingLot to the value of the associated gateway/parking lot combo
                    ddlParkingLot.SelectedValue = dt.Rows[0][0].ToString();

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    ShowMessage("ddlGatewayName_SelectedIndexChanged Error: " + ex.Message);
                }
                finally
                {
                    if (cnn != null)
                        cnn.Close();
                }
            }
        }

        // Upon ddlParkingLot Change - > Repopulate the gateway ddl
        protected void ddlParkingLot_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Upon the change of the ddlParking Lot, repopulate the ddlGateway
            ddlGatewayName.Items.Clear();
            PopulateddlGateway();
            ddlGatewayName.SelectedIndex = 0;
        }
        
        // Upon DDL MODE PARKING LOT Change.
        protected void ddlModeParkingLot_SelectedIndexChanged(object sender, EventArgs e) 
        {
            voidClearAlltbs(); // Clear all Text Boxes when switching select range
            gvKiosk.SelectedIndex = -1; // Reset selection of gridview
            HideError();
            btnUpdate.Visible = false;
            btnDelete.Visible = false;

            // Clear all Kiosk items and repopulate the items
            // based on the parking lot change.
            ddlKiosk.Items.Clear();
            PopulateddlKiosk();
            ddlKiosk.SelectedIndex = 0;

            // Upon the change of the ddlParking Lot, repopulate the ddlGateway
            ddlGatewayName.Items.Clear();
            PopulateddlGateway();
            ddlGatewayName.SelectedIndex = 0;

            ddlParkingLot.SelectedIndex = ddlModeParkingLot.SelectedIndex;
            
            PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked); // Puts in City GUID, and Parking lot NAME
        }

        // Code for ddlModeCIty Change
        protected void ddlModeCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear all textboxes
            voidClearAlltbs();

            // Hide Error Messages
            HideError();

            // Reset the selected index of the gridview
            gvKiosk.SelectedIndex = -1;

            btnUpdate.Visible = false;
            btnDelete.Visible = false;
            
            // Populate the gridview with the selected Organization
            PopulategvKiosk(ddlModeCity.SelectedValue, string.Empty, string.Empty, cbShowActivated.Checked);

            // CLear previous items and repopulated based on the selection of the ddlModeCity
            ddlModeParkingLot.Items.Clear();
            ddlParkingLot.Items.Clear();
            PopulateddlModeParkingLot();
            ddlModeParkingLot.SelectedIndex = 0;
            ddlParkingLot.SelectedIndex = 0;

            ddlKiosk.Items.Clear();
            PopulateddlKiosk();
            ddlKiosk.SelectedIndex = 0;

            ddlGatewayName.Items.Clear();
            PopulateddlGateway();

            // Reset the ddl index for aesthetic reasons.
            
            
            ddlGatewayName.SelectedIndex = 0;
            
        }
        
        // Code for ddlKiosk Change
        protected void ddlKiosk_SelectedIndexChanged(object sender, EventArgs e)
        {
            voidClearAlltbs(); // Clear all Text Boxes when switching select range
            gvKiosk.SelectedIndex = -1; // Reset selection of gridview
            HideError();

            btnUpdate.Visible = false;
            btnUpdate.Visible = false;

            // Repopulate the gridview based on the new settings.
            PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked); // Puts in City GUID, and Parking lot NAME
         }


        #endregion

        #region button clicks
        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            HideError();
        }

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            // Clear all Error Message
            voidClearAlllblErrors();

            // Set up boolean to denote if the Update function passes or not
            bool blnPasses = true;

            // gvRow contains the row of the selected index
            GridViewRow gvRow = gvKiosk.Rows[gvKiosk.SelectedIndex];

            // Setup SQL connections and parameterse
            SqlConnection cnn = new SqlConnection(connectionString);
            SqlDataReader readerProfile = null;

            // Set up Transacation feature for ROLLBACK (just incase a query fails, the whole query should be reversed)
            SqlCommand command = cnn.CreateCommand();
            SqlTransaction transaction; // Required for rollback features 
            // Begin the transaction and rollback protection.
            cnn.Open();
            transaction = cnn.BeginTransaction("UpdateFunction");
            command.Connection = cnn;
            command.Transaction = transaction;

            try
            {

                // Update the Kiosk Table
                command.CommandText = "UPDATE [Kiosk] SET [KioskName] = @KioskName, [ParkingLotID] = @ParkingLotID, [CreditCardPayment] = @CreditCardPayment, "
                    + " [TransactionServiceProviderID] = @TransServiceProvider "
                    + " WHERE [KioskID] = @KioskID";

                // Add the paramter values
                command.Parameters.AddWithValue("@KioskID", gvRow.Cells[findGVcolumn("KioskID")].Text);
                command.Parameters.AddWithValue("@KioskName", tbKioskName.Text);
                command.Parameters.AddWithValue("@ParkingLotID", ddlParkingLot.SelectedValue);
                command.Parameters.AddWithValue("@CreditCardPayment", ddlCreditCardPayment.SelectedValue);
                command.Parameters.AddWithValue("@TransServiceProvider", ddlTransactionServiceProvider.SelectedValue);

                // Execute the query
                command.ExecuteNonQuery();

                // Update the Kiosk-Gateway Table
                command.CommandText = " UPDATE [KioskGatewayList] SET [GatewayID] = @GatewayID "
                                    + " WHERE [KioskID] = @KioskID";
                // Add the paramter values
                command.Parameters.AddWithValue("@GatewayID", ddlGatewayName.SelectedValue);                
                
                // Execute the query
                command.ExecuteNonQuery();

                // Commit the transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Update the flag
                blnPasses = false;
                // Show a message to the user
                ShowMessage("<br> Error while updating: " + ex.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    ShowMessage("<br> Transaction Rollback Error: " + ex2.Message);    
                }                
            }
            finally
            {
                // Close the readerProfile and the corresponding connection
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }            
                        
            // Notify user with a pop up alert window.
            if (blnPasses)
            {
                // Hide any errors, if any.
                HideError();
                // Fill in the textboxes and dropdownlists with the new information.

                // Repopulate the gridview/table with the updated information.
                PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked);

                // Reset index.
                gvKiosk.SelectedIndex = -1;
                voidClearAlltbs();
                
                PopUpMessage("Updated.");
            }
            else
            {
                PopUpMessage("Error while updating.  All queries reversed.");
            }
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            // Clear all Error Message
            voidClearAlllblErrors();

            bool blnPasses = true;

            // Setup SQL connections and parameterse
            SqlConnection cnn = new SqlConnection(connectionString);
            SqlDataReader readerProfile = null;

            // Set up Transacation feature for ROLLBACK (just incase a query fails, the whole query should be reversed)
            SqlCommand command = cnn.CreateCommand();
            SqlTransaction transaction; // Required for rollback features 
            // Begin the transaction and rollback protection.
            cnn.Open();
            transaction = cnn.BeginTransaction("UpdateFunction");
            command.Connection = cnn;
            command.Transaction = transaction;

            try
            {
                Guid guidKioskID = Guid.NewGuid();
                // Update the Kiosk Table
                command.CommandText = "INSERT INTO [Kiosk]([KioskID], [KioskName], [ParkingLotID], [CreditCardPayment], "
                    + " [TransactionServiceProviderID]) "
                    + " VALUES(@KioskID, @KioskName, @ParkingLotID, @CreditCardPayment, @TransServiceProvider)";
                                
                // Add the paramter values                
                command.Parameters.AddWithValue("@KioskID", guidKioskID);
                command.Parameters.AddWithValue("@KioskName", tbKioskName.Text);
                command.Parameters.AddWithValue("@ParkingLotID", ddlParkingLot.SelectedValue);
                command.Parameters.AddWithValue("@CreditCardPayment", ddlCreditCardPayment.SelectedValue);
                command.Parameters.AddWithValue("@TransServiceProvider", ddlTransactionServiceProvider.SelectedValue);

                // Execute the query
                command.ExecuteNonQuery();

                // Update the Kiosk-Gateway Table
                command.CommandText = " INSERT INTO [KioskGatewayList]([KioskID],[GatewayID]) "
                                    + " VALUES(@KioskID, @GatewayID)";
                // Add the paramter values
                command.Parameters.AddWithValue("@GatewayID", ddlGatewayName.SelectedValue);                                

                // Execute the query
                command.ExecuteNonQuery();

                // Commit the transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Update the flag
                blnPasses = false;
                // Show a message to the user
                ShowMessage("<br> Error while inserting: " + ex.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    ShowMessage("<br> Transaction Rollback Error: " + ex2.Message);
                }
            }
            finally
            {
                // Close the readerProfile and the corresponding connection
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }

            // Notify user with a pop up alert window.
            if (blnPasses)
            {
                // Hide any errors, if any.
                HideError();
                // Fill in the textboxes and dropdownlists with the new information.

                // Repopulate the gridview/table with the updated information.
                PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked);

                // Reset index.
                gvKiosk.SelectedIndex = -1;
                voidClearAlltbs();


                // Clear the kiosk items and repopulate them
                ddlKiosk.Items.Clear();
                PopulateddlKiosk();

                PopUpMessage("New Kiosk Added.");
            }
            else
            {
                PopUpMessage("Error while inserting.  All queries reversed.");
            }
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
            voidClearAlltbs();
            gvKiosk.SelectedIndex = -1;
            HideError();
        }
        #endregion      

        protected void ddlCreditCardPayment_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateddlTransactionProvider();
            ddlTransactionServiceProvider.SelectedIndex = 0;
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Clear all Error Message
            voidClearAlllblErrors();

            // Set up boolean to denote if the Update function passes or not
            bool blnPasses = true;

            // gvRow contains the row of the selected index
            GridViewRow gvRow = gvKiosk.Rows[gvKiosk.SelectedIndex];

            // Setup SQL connections and parameterse
            SqlConnection cnn = new SqlConnection(connectionString);
            SqlDataReader readerProfile = null;

            // Set up Transacation feature for ROLLBACK (just incase a query fails, the whole query should be reversed)
            SqlCommand command = cnn.CreateCommand();
            
            cnn.Open();

            try
            {

                // Update the Kiosk Table
                command.CommandText = "UPDATE [Kiosk] SET [Activate] = @Activate WHERE [KioskID] = @KioskID";

                // Add the paramter values
                command.Parameters.AddWithValue("@KioskID", gvRow.Cells[findGVcolumn("KioskID")].Text);
                command.Parameters.AddWithValue("@Activate", 0);
               
                // Execute the query
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                // Update the flag
                blnPasses = false;
                // Show a message to the user
                ShowMessage("<br> Error while updating: " + ex.Message);
                
            }
            finally
            {
                // Close the readerProfile and the corresponding connection
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }

            // Notify user with a pop up alert window.
            if (blnPasses)
            {
                // Hide any errors, if any.
                HideError();
                // Fill in the textboxes and dropdownlists with the new information.

                // Repopulate the gridview/table with the updated information.
                PopulategvKiosk(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlKiosk.SelectedValue, cbShowActivated.Checked);

                // Reset index.
                gvKiosk.SelectedIndex = -1;
                voidClearAlltbs();

                PopUpMessage("Deleted.");
            }
            else
            {
                PopUpMessage("Error while updating.  All queries reversed.");
            }
        }

       
    }
}
