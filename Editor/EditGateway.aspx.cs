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
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RTMC;
using System.ServiceProcess;


/*
 *  TODO LIST: 
 *  Search this phrase in this document to go to the location:
 *  Search: "TODO LIST"
 *  -Must Turn off Station, stop charging for associated Users, and send email.
 *  -The Framework for the function is laid out, but still must implement function 
 *   stop charging and send email necessary information. 
*/


namespace EVEditor
{

    public partial class EditGateway : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        string serviceString = WebConfigurationManager.AppSettings["WindowsService"];

        // ColumnsToHide, makes the hiding of columns well visible to programmer.  To remove or add columns to hide, modify the string below
        string[] ColumnsToHide = { "PrimaryEmail", "Parking Lot ID", "Max Current", "Max Station Current", "Max Voltage",
                                   "Max Station Voltage", "In Days", "Max Times In Day", "Left Current Threshold", "Source Current",
                                   "Source Voltage", "ChargingID", "NetworkID", "ChargingTypeID" ,"MinPoint", "MaxDutyCycle", "MinDutyCycle", "TransactionNodeID", "UseOrganizationPriceList", "StationHasSwitch", "DepartureTimeStop", "GatewayUsername", "GatewayPassword", "GatewayEncryptID", "AllowStopChargeAfterFull", "ActiveChargeCount", "ZigBeeID", "StopChargeDelayLoopCount", "StopChargeIfNotSubmit"};
        
        //Other than the General Administrator, all other allowed roles may only access their Organization's Gateways
        string[] strArrRolesToAllow = { "General Administrator" };

        //strArrTypesToAllow is the role types that will be inserted in the string array above ^, into strArrRolesToAllow
        //The "EditGateway" function assumes that only "General Administrators" may have full accessibility to all organizations.
        string[] strArrTypesToAllow = { "Administrator" }; //Make sure there are no white spaces before or after the string. i.e. " Maintainer"

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };
        
        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();


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
                    return;
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }
            if (!IsPostBack)
            {

                cbShowActivated.Checked = true;
                Initialize();
            }
        }

        protected void SetProgress(int current, int max)
        {
            string Percent = (current * 100 / max).ToString();
            
            if (Percent != "0" && Percent != "100")
            {
                lblProgress.Text = Percent + "% complete (" + current.ToString() + " of " + max.ToString() + " emails sent)";
            }

            lblProgressTable.Text = "<TABLE cellspacing=0 cellpadding=0 border=1 width=200 ID=tblProgressBar><TR><TD bgcolor=#000066 height='20' width=" + Percent + "%> </TD><TD bgcolor=#FFF7CE></TD></TR></TABLE>";
          //  lblProgressTable.Text = "<TABLE cellspacing=0 cellpadding=0 border=1 width=200 ID=tblProgressBar><TR><TD bgcolor=#000066 width=30% height='20'> </TD><TD bgcolor=#FFF7CE></TD></TR></TABLE>";
           
        }

        protected void Initialize()
        {
            PopulategvGateway(string.Empty, string.Empty, cbShowActivated.Checked); // Initialize the gridview with all data points in the server.
            PopulateddlModeCity();
            PopulateddlModeParkingLot();
            Populateinfoddl();
            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            if (enable == "true") return;
            btnRestartWindowsService.Visible = false;
            lblRestartWindowsService.Visible = false;
        }

        #region gvGateway - Functions

        protected void PopulategvGateway(string strOrgID, string ParkingLotID, bool blnActivate) // City is either .Empty (for no City pref), or has a city, to filter by city name
        {
            if (strOrgID == "-1") // This is to account for a empty City Selection.  (If no city is checked, but a parking lot is checked, this if code allows to properly populate the gridview)
                strOrgID = string.Empty;

            if (ParkingLotID == "-1")
                ParkingLotID = string.Empty;


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


            DataTable DT = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                // sqlQuery is the default SQL query that will selected all of the data in the table.  The subsequent += additions will add in constraints to select specific data based on the DDL selections
                string sqlQuery = "SELECT DISTINCT g.ID, g.Name, g.[IP Address], g.[Charging Level], g.[Parking Lot ID], g.Level, g.[Max Current], g.[Max Station Current],"+
                    " g.[Max Voltage], g.[Max Station Voltage], g.[Source Current], g.[Source Voltage], g.[Retrieve Interval], g.Enable, g.[Time Quantum], g.[Time Out],"+
                    " g.[Retry Times], g.CurrentValve, g.NodeControlDelay, g.Activate, g.PrimaryEmail, g.InDays, g.MaxTimesInDay, g.LeftCurrentValve, g.HasSOC, g.Controllable, g.AggregateControl, AllowReboot, PowerSources, g.Note, n.Network, n.NetworkID, " +
                    " c.Name AS ChargingName, c.ID as ChargingID, pl2.Name AS ParkingLotName , ct.ID AS ChargingTypeID, ct.Type AS ChargingType, g.MinPoint, g.MaxDutyCycle, g.MinDutyCycle, g.TransactionNodeID, g.UseOrganizationPriceList, g.StationHasSwitch, g.DepartureTimeStop, g.GatewayUsername, g.GatewayPassword, g.GatewayEncryptID, g.AllowStopChargeAfterFull, g.ActiveChargeCount, g.ZigBeeID, g.StopChargeDelayLoopCount, g.StopChargeIfNotSubmit" + 
                    " FROM Gateway AS g INNER JOIN Network AS n ON g.NetworkID = n.NetworkID"+
                    " INNER JOIN [Parking Lot] AS pl2 ON pl2.ID = g.[Parking Lot ID] INNER JOIN [Charging Algorithm] AS c ON c.ID = g.[Algorithm ID] INNER JOIN [ChargingType] AS ct ON ct.ID = g.ChargingTypeID";


                // if this user is a Master user
                // the user may access all the information
                if (blnListhasMasterRole)
                {
                    // if a Parking lot is chosen
                    if (ParkingLotID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[ID] = '" + ParkingLotID + "'";
                    }
                    // if an organization is chosen, populate the associated data
                    else if (strOrgID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[City ID] = '" + strOrgID + "'";
                    }
                }
                // the user is not a master user.
                // listCityGUID will contain at least one organization GUID.
                else
                {
                    if (ParkingLotID != string.Empty)
                    {
                        sqlQuery += " WHERE pl2.[ID] = '" + ParkingLotID + "'";
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

                if (blnActivate)
                    sqlQuery += " AND g.[Activate] ='1'";

                // Order by ascending names
                sqlQuery += " ORDER BY g.[Name] ASC ";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    using (SqlDataAdapter AD = new SqlDataAdapter(cmd))
                    {
                        AD.Fill(DT);
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            PopUpError("No Data.");
                    }
                }
            }           
            
            Session["data"] = DT;
            gvGateway.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvGateway.DataBind(); // Bind data
            int intTotalUsers = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalUsers++;
            }            
                
            lblTotalUsers.Text = "Gateways in this area: " + intTotalUsers;
        }


        protected void gvGatewaySelectedIndex(object sender, EventArgs e)
        {
            hidelblErrors();
            HideError();
            ClearAlllblError();
            fillInTxtBoxes();
            btnUpdate.Visible = true;            
        }

        protected void hidelblErrors()
        {
            lblCatchError.Text = string.Empty;
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected void gvGatewayPaging(Object sender, GridViewPageEventArgs e)
        {
            // Save selected values prior to the page change.
            int intddlmodecityindex = ddlModeCity.SelectedIndex;
            int intddlparkingindex = ddlModeParkingLot.SelectedIndex;

            ClearAllTbs();
            gvGateway.SelectedIndex = -1;

            DataTable dataTable = Session["data"] as DataTable;
            gvGateway.PageIndex = e.NewPageIndex;
            gvGateway.DataSource = dataTable;
            gvGateway.DataBind();

            ddlModeCity.SelectedIndex = intddlmodecityindex;
            ddlModeParkingLot.SelectedIndex = intddlparkingindex;
        }

        #endregion
        #region PopulateDDL and DDL functions

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
                        strQuery += " ORDER BY Name ";
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

                        ddlParkingLotNames.DataSource = dt; // Fill the Drop Down List
                        ddlParkingLotNames.DataValueField = "ID"; // DataValueField contains the GUID of the Parking Lot
                        ddlParkingLotNames.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlParkingLotNames.DataBind();

                        ListItem li = new ListItem("All Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("All Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLotNames.Items.Insert(0, li2);
                    }
                    else
                    {
                        ListItem li = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLotNames.Items.Insert(0, li2);
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
                    strQuery = "SELECT ID, Name FROM [Parking Lot] WHERE [City ID]='" + ddlModeCity.SelectedValue + "'" + " ORDER BY Name";

                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();

                    // Two datatables must be used to properly fill two DDLs
                    da.Fill(dt);

                    // Check to see if there are no returned data sets
                    bool blnIsEmptyParkingLot = dt.Rows.Count == 0;

                    if (!blnIsEmptyParkingLot)
                    {
                        ddlModeParkingLot.DataSource = dt; // Fill the Drop Down List
                        ddlModeParkingLot.DataValueField = "ID"; // DataValueFIeld contains the GUID of the Parking Lot
                        ddlModeParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlModeParkingLot.DataBind();

                        ddlParkingLotNames.DataSource = dt; // Fill the Drop Down List
                        ddlParkingLotNames.DataValueField = "ID"; // DataValueField contains the GUID of the Parking Lot
                        ddlParkingLotNames.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                        ddlParkingLotNames.DataBind();


                        ListItem li = new ListItem("Associated Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("Associated Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLotNames.Items.Insert(0, li2);
                    }
                    else
                    {
                        ListItem li = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);

                        ListItem li2 = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlParkingLotNames.Items.Insert(0, li2);
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
                if (blnListhasMasterRole)
                {
                    strQuery = "SELECT ID, Name FROM City WHERE Activate= 1 ORDER BY Name";
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


                ListItem li = new ListItem("All Cities", "-1");
                ddlModeCity.Items.Insert(0, li);

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

        protected void Populateinfoddl()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            try
            {
                cnn.Open();
                //strQuery = "SELECT ID, Name FROM [Parking Lot] ";
                //cmd = new SqlCommand(strQuery, cnn);
                //cmd.CommandType = CommandType.Text;
                //da = new SqlDataAdapter();
                //da.SelectCommand = cmd;
                //dt = new DataTable();
                //da.Fill(dt);

                //ddlParkingLotNames.DataSource = dt;
                //ddlParkingLotNames.DataValueField = "ID";
                //ddlParkingLotNames.DataTextField = "Name";
                //ddlParkingLotNames.DataBind();

                ListItem li = new ListItem("Select...","-1");
                ListItem li2 = new ListItem("Select...","-1");
                ListItem li3 = new ListItem("Select...", "-1");
                ListItem li4 = new ListItem("NULL", "&nbsp;");
                //ddlParkingLotNames.Items.Insert(0, li);

                strQuery = "SELECT ID,Name FROM [Charging Algorithm] WHERE ENABLE ='1' ORDER BY ID "; // Select only enabled algorithms
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlAlgorithm.DataSource = dt;
                ddlAlgorithm.DataValueField = "ID";
                ddlAlgorithm.DataTextField = "Name";
                ddlAlgorithm.DataBind();

                ddlAlgorithm.Items.Insert(0, li);

                strQuery = "SELECT ID, Type FROM [ChargingType] ";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlChargingType.DataSource = dt;
                ddlChargingType.DataValueField = "ID";
                ddlChargingType.DataTextField = "Type";
                ddlChargingType.DataBind();
                ddlChargingType.Items.Insert(0, li3);

                strQuery = "SELECT NetworkID, Network FROM [Network]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                ddlNetworkID.DataSource = dt;
                ddlNetworkID.DataValueField = "NetworkID";
                ddlNetworkID.DataTextField = "Network";
                ddlNetworkID.DataBind();

                ddlNetworkID.Items.Insert(0, li2);

                strQuery = "SELECT TransactionNodeID FROM [CAISONodeList]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                ddlTransactionNodeID.DataSource = dt;
                ddlTransactionNodeID.DataValueField = "TransactionNodeID";
                ddlTransactionNodeID.DataTextField = "TransactionNodeID";
                ddlTransactionNodeID.DataBind();

                ddlTransactionNodeID.Items.Insert(0, li4);

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Populateinfoddl Error: " + ex.Message);
                
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            ddlMinPoint.Items.AddRange(Enumerable.Range(0, 256).Select(e => new ListItem(e.ToString())).ToArray());
            ddlMaxDutyCycle.Items.AddRange(Enumerable.Range(0, 101).Select(e => new ListItem(e.ToString())).ToArray());
            ddlMinDutyCycle.Items.AddRange(Enumerable.Range(0, 101).Select(e => new ListItem(e.ToString())).ToArray());
        }

        protected void ddlModeCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset the gridview index
            gvGateway.SelectedIndex = -1;
            
            // Clear all textboxes/ddl and hide the errors
            ClearAllTbs();
            hidelblErrors();
            
            // Hide the btnUpdate since there is nothing yet to update 
            btnUpdate.Visible = false;

            PopulategvGateway(ddlModeCity.SelectedValue, string.Empty, cbShowActivated.Checked);

            // Clear the ddl items and repopulate them.
            ddlModeParkingLot.Items.Clear();
            ddlParkingLotNames.Items.Clear();

            PopulateddlModeParkingLot();
            ddlModeParkingLot.SelectedIndex = 0;
            ddlParkingLotNames.SelectedIndex = 0;
            
        }
        #endregion

        #region Helper Functions

        protected bool SendMail(string strOrganization, string strSendTo, string strEmailMessage, string strEmailSubject)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            MailMessage email = null;
            SmtpClient sc = null;


            // strEmailPass = The Email accounts password.
            // strSmtpHost = The SMTP host of the email host.
            // blnSSL = The SSL security settings of the email host.  (either T/F)
            // strSmtpport = The Port of the email host. (can be null)

            string strSmtpHost = string.Empty;
            string strEmailPass = string.Empty;
            bool blnSSL = true;
            string strSmtpport = string.Empty;
            string strFromEmail = string.Empty;

            try
            {
                cnn.Open();
                strQuery = "Select [Email Password], [Email Host], [EnableSSL], [Email Port], [Email Address] FROM [City] WHERE [Name] = '" + strOrganization + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    strEmailPass = reader["Email Password"].ToString().Trim();
                    strSmtpHost = reader["Email Host"].ToString().Trim();
                    blnSSL = bool.Parse(reader["EnableSSL"].ToString().Trim());
                    strSmtpport = Server.HtmlDecode(reader["Email Port"].ToString().Trim());
                    strFromEmail = reader["Email Address"].ToString().Trim();
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at SendMail: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
                return false;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }



            // Obtain Password using Rijndael Encryption/Decryption.  The keys are stored in the web.config file
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
                    return false;
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
                    catch // This catch statement will usually catch errors related to a non integer port data.
                    {
                        ShowMessage("Port information is incorrect.  Please check Edit Organization page to ensure the port is correct.");
                    }
                }

                sc.Credentials = new NetworkCredential(strFromEmail, strEmailPass);
                sc.EnableSsl = blnSSL;
                sc.Timeout = 10000;
                email.To.Add(strFromEmail);
                email.Bcc.Add(strSendTo);

                string strEmailBody = strEmailMessage;

                strEmailBody = strEmailBody.Replace("\n", "<br/>");
                strEmailBody = strEmailBody.Replace(" ", "&nbsp;");

                email.Subject = strEmailSubject;
                email.IsBodyHtml = true;
                email.Body = "<table>";
                email.Body += "<tr><td> " + strEmailBody + "</td></tr>";
                email.Body += "</table>";
                sc.Send(email);
            }
            catch (Exception ex)
            {
                ShowMessage("Error at SendMail2:"  + ex.Message);
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
                    ShowMessage("Email sent!");
                }
            }
            return blnPassed;
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

        void fillInTxtBoxes()
        {
            GridViewRow gvRow;

            try
            {
                gvRow = gvGateway.Rows[gvGateway.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                return;
            }
            tbID.Text = gvRow.Cells[findGVcolumn("ID")].Text;
            tbName.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Name")].Text);
            tbIPaddr.Text = gvRow.Cells[findGVcolumn("IP Address")].Text;
       //     tbChargingLvl.Text = gvRow.Cells[findGVcolumn("Charging Level")].Text;

            ddlChargingLevel.SelectedValue = gvRow.Cells[findGVcolumn("Charging Level")].Text;
            ddlPowerSources.SelectedValue = gvRow.Cells[findGVcolumn("Power Sources")].Text;
            ddlNetworkID.SelectedValue = gvRow.Cells[findGVcolumn("NetworkID")].Text;
            ddlParkingLotNames.SelectedValue = gvRow.Cells[findGVcolumn("Parking Lot ID")].Text;
            try
            {
                ddlAlgorithm.SelectedValue = gvRow.Cells[findGVcolumn("ChargingID")].Text;
            }
            catch
            {
                lblddlAlgorithmError.Text = "Choose a new Algorithm";
                ddlAlgorithm.SelectedIndex = 0;
                lblCatchError.Text += "<br> This Charging Algorithm no longer exists or is deactivated.  <br> Please select a new Charging Algorithm.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
            }

            try
            {
                ddlChargingType.SelectedValue = gvRow.Cells[findGVcolumn("ChargingTypeID")].Text;
            }
            catch
            {
                lblddlChargingTypeError.Text = "Choose a new Charging Type";
                ddlChargingType.SelectedIndex = 0;
                lblCatchError.Text += "<br> This Charging Type no longer exists or is deactivated.  <br> Please select a new Charging Tyoe.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
            }

            tbLevel.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Level")].Text);
            tbMaxCurrent.Text = gvRow.Cells[findGVcolumn("Max Current")].Text;
            tbMaxStationCurrent.Text = gvRow.Cells[findGVcolumn("Max Station Current")].Text;
            tbMaxVoltage.Text = gvRow.Cells[findGVcolumn("Max Voltage")].Text;
            tbMaxStationVoltage.Text = gvRow.Cells[findGVcolumn("Max Station Voltage")].Text;
            tbSourceCurrent.Text = gvRow.Cells[findGVcolumn("Source Current")].Text;
            tbSourceVoltage.Text = gvRow.Cells[findGVcolumn("Source Voltage")].Text;
            tbRetrieveInterval.Text = gvRow.Cells[findGVcolumn("Retrieve Interval")].Text;
            tbTimeQuantum.Text = gvRow.Cells[findGVcolumn("Time Quantum")].Text;
            tbTimeOut.Text = gvRow.Cells[findGVcolumn("Time Out")].Text;
            tbRetryTime.Text = gvRow.Cells[findGVcolumn("Retry Times")].Text;
            tbCurrentThreshold.Text = gvRow.Cells[findGVcolumn("Current Threshold")].Text;
            tbNodeControlDelay.Text = gvRow.Cells[findGVcolumn("Node Control Delay")].Text;
            tbInDays.Text = KillNull(gvRow.Cells[findGVcolumn("In Days")].Text);
            tbMaxTimesPerDay.Text = KillNull(gvRow.Cells[findGVcolumn("Max Times In Day")].Text);
            tbLeftCurrentThreshold.Text = gvRow.Cells[findGVcolumn("Left Current Threshold")].Text;
            tbPrimaryEmail.Text = KillNull(gvRow.Cells[findGVcolumn("PrimaryEmail")].Text);           

            string strNote = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Note")].Text);
            tbNote.Text = string.IsNullOrWhiteSpace(strNote) ? string.Empty : strNote;

            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            if (cbActivate.Checked == true)
                ddlActivate.SelectedValue = "1";
            else
                ddlActivate.SelectedValue = "0";

            CheckBox cbEnabled = (CheckBox)gvRow.Cells[findGVcolumn("Enable")].Controls[0];
            if (cbEnabled.Checked == true)
                ddlEnable.SelectedValue = "1";
            else
                ddlEnable.SelectedValue = "0";
            
            CheckBox cbhasSOC = (CheckBox)gvRow.Cells[findGVcolumn("State of Charge")].Controls[0];
            if (cbhasSOC.Checked == true) 
                ddlhasSOC.SelectedValue = "1"; 
            else
                ddlhasSOC.SelectedValue = "0";

            CheckBox cbControllable = (CheckBox)gvRow.Cells[findGVcolumn("Controllable")].Controls[0];
            if (cbControllable.Checked == true) 
                ddlControllable.SelectedValue = "1"; 
            else
                ddlControllable.SelectedValue = "0";

            CheckBox cbAggregateControl = (CheckBox)gvRow.Cells[findGVcolumn("Aggregate Control")].Controls[0];
            ddlAggregateControl.SelectedValue = cbAggregateControl.Checked == true ? "1" : "0";

            CheckBox cbAllowReboot = (CheckBox)gvRow.Cells[findGVcolumn("Allow Reboot")].Controls[0];
            if (cbAllowReboot.Checked == true)
                ddlAllowReboot.SelectedValue = "1";
            else
                ddlAllowReboot.SelectedValue = "0";

            ddlMinPoint.SelectedValue = gvRow.Cells[findGVcolumn("MinPoint")].Text;
            ddlMaxDutyCycle.SelectedValue = gvRow.Cells[findGVcolumn("MaxDutyCycle")].Text;
            ddlMinDutyCycle.SelectedValue = gvRow.Cells[findGVcolumn("MinDutyCycle")].Text;
            ddlTransactionNodeID.SelectedValue = gvRow.Cells[findGVcolumn("TransactionNodeID")].Text;
            //string traction = gvRow.Cells[findGVcolumn("TransactionNodeID")].Text;
            //if (traction == "&nbsp;")
            //    ddlTransactionNodeID.SelectedValue = "";
            //else
            //    ddlTransactionNodeID.SelectedValue = traction;
            //traction == "&nbsp;" ? "" : traction

            ddlUseOrganizationPriceList.SelectedValue = gvRow.Cells[findGVcolumn("UseOrganizationPriceList")].Text == "True" ? "1" : "0";

            ddlStationHasSwitch.SelectedValue = gvRow.Cells[findGVcolumn("StationHasSwitch")].Text == "True" ? "1" : "0";
            ddlDepartureTimeStop.SelectedValue = gvRow.Cells[findGVcolumn("DepartureTimeStop")].Text == "True"
                ? "1"
                : "0";
            tbGatewayUsername.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("GatewayUsername")].Text);
            tbGatewayPassword.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("GatewayPassword")].Text);
            tbGatewayEncryptID.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("GatewayEncryptID")].Text);
            ddlAllowStopChargeAfterFull.SelectedValue = gvRow.Cells[findGVcolumn("AllowStopChargeAfterFull")].Text ==   "True" ? "1" : "0";
            ddlActiveChargeCount.SelectedValue = gvRow.Cells[findGVcolumn("ActiveChargeCount")].Text;
            tbZigbeeId.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("ZigBeeID")].Text);
            ddlStopChargeDelayLoopCount.SelectedValue = gvRow.Cells[findGVcolumn("StopChargeDelayLoopCount")].Text;
            ddlStopChargeIfNotSubmit.SelectedValue = Server.HtmlDecode(gvRow.Cells[findGVcolumn("StopChargeIfNotSubmit")].Text) == "True" ? "1" : "0";

            //tbTest.Text = gvRow.Cells[findGVcolumn("StationHasSwitch")].Text;
        }

        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            PopulategvGateway(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, cbShowActivated.Checked);
        }

        protected string KillNull(string s) // Take off the "&nbsp;" in the string
        {
            if (s == "&nbsp;")
                return string.Empty;
            else
                return s;
        }

        // Initialize functions.  Functions to help with Authentication.

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

        protected void PopUpError(string Message)
        {            
            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.Append("<script type = 'text/javascript'>");
            sb2.Append("window.onload=function(){");
            sb2.Append("alert('");
            sb2.Append(Message);
            sb2.Append("')};");
            sb2.Append("</script>");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", sb2.ToString());
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvGateway.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvGateway.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void gvGatewayRowCreated(object sender, GridViewRowEventArgs e)
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

        #endregion
        #region ValidationChecks


        protected bool blnUpdateGWValidationCheck()
        {
            bool blnPassesValidation = true;
            if (ddlNetworkID.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlNetworkIDError.Text = "Error, Select a Network.";
            }

            if (ddlParkingLotNames.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlParkingLotNamesError.Text = "Error, Select a Parking Lot";
            }

            if (!blnCheckForDuplicateIPAddr("Update")) // If a duplicat IP Addr is being entered, then error out.
            {
                blnPassesValidation = false;
                lblIPaddrError.Text += "IP Addresses must be unique.";
            }

            if (!blnCheckForDuplicateName("Update"))
            {
                blnPassesValidation = false;
                lbltbNameError.Text = "Name must be unique.";
            }

            if (ddlAlgorithm.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlAlgorithmError.Text = "Error, Select an Algorithm";
            }

            if (ddlChargingType.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlChargingTypeError.Text = "Error, Select a Charging Type";
            }

            return blnPassesValidation;
        }

        protected bool blnNewGatewayValidCheck()
        {
            bool blnPassesValidation = true;
            if (ddlNetworkID.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlNetworkIDError.Text = "Error, Select a Network.";
            }

            if (ddlParkingLotNames.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlParkingLotNamesError.Text = "Error, Select a Parking Lot";
            }

            if (ddlAlgorithm.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlAlgorithmError.Text = "Error, Select an Algorithm";
            }

            if (ddlChargingType.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlChargingTypeError.Text = "Error, Select a Charging Type";
            }

            if (!blnCheckForDuplicateIPAddr("New")) // If a duplicat IP Addr is being entered, then error out.
            {
                blnPassesValidation = false;
                lblIPaddrError.Text += "IP Addresses must be unique.";
            }
            if (!blnCheckForDuplicateName("New"))
            {
                blnPassesValidation = false;
                lbltbNameError.Text = "Name must be unique.";
            }
            

            return blnPassesValidation;
        }

        protected bool blnCheckForDuplicateName(string UpdateOrNew)
        {
            // Obtain Current Selected Name
            string strCurrentID = string.Empty;
            if (UpdateOrNew == "Update")
            {
                GridViewRow gvRow = gvGateway.Rows[gvGateway.SelectedIndex];
                string strCurrentName = gvRow.Cells[findGVcolumn("Name")].Text; // IP Address of current
                strCurrentID = gvRow.Cells[findGVcolumn("ID")].Text;

                if (strCurrentName == tbName.Text) // If IPAddr is unchanged, then return true.  else, Check if the NEW IP Addr already exists TODO: Check this.
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
                strQuery = "SELECT [Name], ID FROM [Gateway]";
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
                            if (dt.Rows[i][0].ToString() == tbName.Text) 
                            {
                                blnPasses = false;
                                break;
                            }
                        }
                    }
                }
                else // if UpdateOrNew == "New"{
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() == tbName.Text)
                        {
                            blnPasses = false;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ShowMessage("blnCheckForDuplicateName Error: " + ex.Message);
                
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
        protected bool blnCheckForDuplicateIPAddr(string UpdateOrNew)
        {
            // Obtain Current Selected IP Address
            string strCurrentID = string.Empty;
            if (UpdateOrNew == "Update")
            {
                GridViewRow gvRow = gvGateway.Rows[gvGateway.SelectedIndex];
                string strCurrentIPAddr = gvRow.Cells[findGVcolumn("IP Address")].Text; // IP Address of current
                strCurrentID = gvRow.Cells[findGVcolumn("ID")].Text;

                if (strCurrentIPAddr == tbIPaddr.Text) // If IPAddr is unchanged, then return true.  else, Check if the NEW IP Addr already exists
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
                strQuery = "SELECT [IP Address], ID FROM [Gateway]";
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
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][1].ToString() != strCurrentID) // Check that the current IP being checked is not the current selected ID
                        {
                            if (dt.Rows[i][0].ToString() == tbIPaddr.Text) // If there is a match in IP in different data, then return false
                            {
                                blnPasses = false;
                                break;
                            }
                        }
                    }
                }
                else // if UpdateOrNew == "New"{
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() == tbIPaddr.Text)
                        {
                            blnPasses = false;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ShowMessage("blnCheckForDuplicateIPAddr Error: " + ex.Message);
                
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

        #region HelperFunctions for Gateway Update Check

        protected bool blnStopChargingUsersAndEmail()
        {
            bool blnPasses = true;

            GridViewRow gvRow = gvGateway.Rows[gvGateway.SelectedIndex]; // Obtain selected index of gvGateway

            //PopUpError("Error Check");
            if (blnUpdateCheckForAlgEnableActivate())
            {
                string strGatewayID = gvRow.Cells[findGVcolumn("ID")].Text;      // Gateway ID
                string strOrgName = strReturnOrgNameFromGatewayID(strGatewayID); // Name or Gateway Organization - For use in the SendMail Function

                List<string> listStations = listReturnStationIDfromGatewayID(strGatewayID); // Returns a list of Station IDs associated with the Gateway
                List<string> listUserID = listReturnUserID(listStations);                  // Returns a list of UserIDs associated with all of the stations
                List<string> listEmails = new List<string>();
                List<string> listUserInfo = new List<string>();

                if (listUserID.Count > 0)
                {
                    listEmails = listReturnListOfEmail(listUserID);
                }

                for (int i = 0; i < listUserID.Count; i++)
                {
                    lblTest.Text += listEmails[i] + " ";
                }


                string strEmailBody = "Your EV has stopped charging. <br> More information: "; // TODO Finish this.
                string strEmailSubject = "EV Control Center";

                //Action<object> action = (object obj) =>
                //{
                //    Console.WriteLine("Task={0}, obj={1}, Thread={2}", Task.CurrentId, obj.ToString(), Thread.CurrentThread.ManagedThreadId);
                //};

               // Task t3;

                //var taskList = new List<Task>();

                int intListEmailsCount = listEmails.Count;

                

                //System.Threading.Tasks.Parallel.For(0,intListEmailsCount,i =>
                //    {
                //        SetProgress(i, intListEmailsCount);
                //        SendMail(strOrgName, listEmails[i], strEmailBody, strEmailSubject);
                //    }
                //);


                
                for (int i = 0; i < intListEmailsCount; i++)
                {
                    //listUserInfo = listChargingRecordInfoForUser(listUserID[i]); // ListUserInfo has each specific charging record information associated with each email account

                    //SetProgress(i, intListEmailsCount);
                    //Task<int> t1 = new Task<int>(SetProgress(i, intListEmailsCount));

                    //var t1 = Task.Factory.StartNew(() => SetProgress(i, intListEmailsCount));
                    //var t2 = Task.Factory.StartNew(() => SendMail(strOrgName, listEmails[i], strEmailBody, strEmailSubject));
                    // SendMail(strOrgName, listEmails[i], strEmailBody, strEmailSubject); 
                    //if (SendMail(strOrgName, listEmails[i], strEmailBody, strEmailSubject)) // TODO  WRITE A MESSAGE HERE!!!!
                    //{
                    //}
                    //else
                    //{
                    //    ShowMessage("Error at blnStopChargingUsersAndEmail");                                                                                                                                                                                                                                               
                    //    blnPasses = false;
                    //}
                }
            }

            else
            {
                blnPasses = false;
            }

            return blnPasses;
        }

        protected bool blnUpdateCheckForAlgEnableActivate()
        {
            GridViewRow gvRow = gvGateway.Rows[gvGateway.SelectedIndex]; // Obtain selected index of gvGateway

            string strPrevChargingID = gvRow.Cells[findGVcolumn("ChargingID")].Text;
            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            CheckBox cbEnabled = (CheckBox)gvRow.Cells[findGVcolumn("Enable")].Controls[0];
            
            int intEnable=0;
            if(cbEnabled.Checked)
                intEnable = 1;
            else
                intEnable = 0;

            int intActivate=0;
            if(cbActivate.Checked)
                intActivate=1;
            else
                intActivate=0;


            if (ddlActivate.SelectedValue != intActivate.ToString())
                return true;
           
            if (ddlEnable.SelectedValue != intEnable.ToString())
                return true;
                       

            if (ddlAlgorithm.SelectedValue != strPrevChargingID)
                return true;           

            return false;
        }

        protected List<string> listReturnUserName(List<string> listUserID)
        {
            List<string> listUserEmails = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT DISTINCT [UserName] " +
                           "FROM aspnet_Users " +
                           " WHERE ";
                int intlistCount = listUserID.Count;
                for (int i = 0; i < intlistCount; i++)
                {
                    strQuery += " UserId = '" + listUserID[i] + "'";
                    if (i < intlistCount - 1)
                        strQuery += " OR ";
                }

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    listUserEmails.Add(dt.Rows[i][0].ToString());
                }

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at listReturnUserName: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

            return listUserEmails;
        }

        protected string strReturnEmailFromUserID(string strUserID)
        {
            string strUserEmail = string.Empty;

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            try
            {
                cnn.Open();
                strQuery = " SELECT [Email] from [aspnet_Membership] WHERE [UserId] = '" + strUserID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    strUserEmail = reader["Email"].ToString().Trim();
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strReturnEmailFromUserID: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strUserEmail;
        }

        protected List<string> listReturnListOfEmail(List<string> listUserID)
        {
            List<string> ListOfEmail = new List<string>();
            
            for (int i = 0; i < listUserID.Count; i++)
            {
                ListOfEmail.Add(strReturnEmailFromUserID(listUserID[i]));
            }          

            return ListOfEmail;

        }

        protected string strReturnCSVEmails(List<string> listUserID)
        {
            string strCSVemail = string.Empty;
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT DISTINCT [Email] " + 
                           "FROM aspnet_Membership " + 
                           " WHERE ";
                int intlistCount = listUserID.Count;
                for (int i = 0; i < intlistCount; i++)
                {
                    strQuery += " UserId = '" + listUserID[i] + "'";
                    if (i < intlistCount - 1)
                        strQuery += " OR ";
                }

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                int intdataCount = dt.Rows.Count; 

                for (int i = 0; i < intdataCount; i++)
                {
                    strCSVemail += dt.Rows[i][0].ToString();
                    if (i < intdataCount - 1)
                        strCSVemail += ", ";
                }

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at listReturnUserEmail: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strCSVemail;
        }

        protected List<string> listReturnUserID(List<string> listStationID)
        {
            // Can either return UserID or User Email address based on strEmailorID preferences
            List<string> listUserInfo = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT Distinct [UserID] FROM [ChargingRecords] WHERE ";
                int intListCount = listStationID.Count;
                for (int i = 0; i < intListCount; i++)
                {
                    strQuery += " [StationID] = '" + listStationID[i] + "'";
                    if (i < intListCount - 1)
                        strQuery += " OR ";
                }

               // strQuery += " Where isEnd = '0'";

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    listUserInfo.Add(dt.Rows[i][0].ToString());
                }

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at listReturnUserID: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();                   
            }

            return listUserInfo;
        }

        protected bool blnStartChargingUsers(List<string> ListUserIDs)
        {
            bool blnPasses = true;

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            int intListUserIDcount = ListUserIDs.Count;

            try
            {
                strQuery = "UPDATE [ChargingRecords] SET [IsEnd] = '0' WHERE ";
                for (int i = 0; i < intListUserIDcount; i++)
                {
                    strQuery += " UserID = '" + ListUserIDs[i] + "'";
                    if (i < intListUserIDcount - 1)
                        strQuery += " OR ";
                }
                cnn.Open();

                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {                
                blnPasses = false;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();

            }
            return blnPasses;
        }

        protected bool blnStopChargingUsers(List<string> ListUserIDs)
        {
            bool blnPasses = true;

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            int intListUserIDcount = ListUserIDs.Count;

            try
            {
                strQuery = "UPDATE [ChargingRecords] SET [IsEnd] = '1' WHERE";
                for (int i = 0; i < intListUserIDcount; i++)
                {
                    strQuery += " UserID = '" + ListUserIDs[i] + "'";
                    if (i < intListUserIDcount - 1)
                        strQuery += " OR ";
                }
                cnn.Open();

                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at blnStopChargingUsers: " + ex.Message);
                blnPasses = false;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
                
            }

            return blnPasses;
        }

        protected List<string> listReturnStationIDfromGatewayID(string strGatewayID)
        {
            List <string> listStationID = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT [ID] FROM Station WHERE [Gateway ID] = '" + strGatewayID + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    listStationID.Add(dt.Rows[i][0].ToString());
                }

                dt.Dispose();
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);   
            }
            finally
            {
                if(cnn!=null)
                    cnn.Close();
            }

            return listStationID;
        }

        protected string strReturnOrgNameFromGatewayID(string strGatewayID)
        {
            string strOrgName = string.Empty;
            
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            try
            {
                cnn.Open();
                strQuery = "SELECT c.Name FROM [EVDemo].[dbo].[Gateway] as g INNER JOIN [EVDemo].[dbo].[Parking Lot] as p " +
                           " ON p.ID = g.[Parking Lot ID] INNER JOIN [EVDemo].[dbo].[City] as c  ON " +
                           " c.[ID] = p.[City ID] where g.[ID] = '" + strGatewayID + "'";
                
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    strOrgName = reader["Name"].ToString().Trim();
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strReturnOrgNameFromGatewayID: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();               
            }

            return strOrgName;
        }


        protected List<string> listChargingRecordInfoForUser(string strUserID)
        {
            List<string> listOfChargingRecordInfo = new List<string>();

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            try
            {
                cnn.Open();
                strQuery = "Select [EndTime], [EndVoltage], [EndCurrent], [EndPF], [EndActivePower],[EndApparentPower],[EndMainPower] FROM [ChargingRecords] WHERE [UserID] = '" + strUserID + "'";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    listOfChargingRecordInfo.Add(reader["EndTime"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndVoltage"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndCurrent"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndPF"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndActivePower"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndApparentPower"].ToString().Trim());
                    listOfChargingRecordInfo.Add(reader["EndMainPower"].ToString().Trim());
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at listChargingRecordInfoForUser: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

            return listOfChargingRecordInfo;
        }

        #endregion // End for Gateway Update Check

        #region btn - rbl - Functions        

        protected void btnUpdateClick(object sender, EventArgs e)
        {  
            ClearAlllblError(); // Clear the lblError Messages
            bool blnPasses = blnUpdateGWValidationCheck();

            if (!blnPasses)
            {
                lblCatchError.Text = "<br>Errors shown on right.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                return; // Exit out of function if errors are found.
            }
            GridViewRow gvRow = gvGateway.Rows[gvGateway.SelectedIndex]; // Obtain selected index of gvGateway


            //PopUpError("Sending Mail");
            //if (!blnStopChargingUsersAndEmail()) // Find users that are affected by a gateway change (Alg change, enable/activate/controllable change, etc), Stop charging those users, then Send and email with the Charging Record
            //{
            //    ShowMessage("Error at blnStopChargingUsersAndEmail");
            //}
                
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;

            SqlCommand cmd; SqlDataReader readerProfile = null;            

            try
            {
                strQuery = "UPDATE [Gateway] SET [Name] = @gatewayName, [IP Address] = @IPAddress, NetworkID = @NetworkID, "
                    +" [Charging Level] = @ChargingLevel, [Parking Lot ID] = @ParkingLotID, Level = @Level, [Max Current] = @MaxCurrent, "
                    +" [Max Station Current] = @MaxStationCurrent, [Max Voltage] = @MaxVoltage, [Max Station Voltage] = @MaxStationVoltage, "
                    +" [Source Current] = @SourceCurrent, [Source Voltage] = @SourceVoltage, [Algorithm ID] = @AlgorithmID, "
                    +" [Retrieve Interval] = @RetrieveInterval, [Enable] = @Enable, [Time Quantum] = @TimeQuantum, [Time Out] = @TimeOut, "
                    +" [Retry Times] = @RetryTimes, [CurrentValve] = @CurrentValve, [NodeControlDelay] = @NodeControlDelay, Activate = @Activate, "
                    + " [PrimaryEmail] = @PrimaryEmail, InDays = @InDays, MaxTimesInDay = @MaxTimesInDay, LeftCurrentValve = @LeftCurrentValve, HasSOC = @SOC,PowerSources = @PowerSources, Controllable = @Controllable, AggregateControl = @AggregateControl, AllowReboot = @AllowReboot, ChargingTypeID = @ChargingTypeID, Note = @Note , MinPoint = @MinPoint, MaxDutyCycle = @MaxDutyCycle, MinDutyCycle = @MinDutyCycle, TransactionNodeID = @TransactionNodeID, UseOrganizationPriceList = @UseOrganizationPriceList, StationHasSwitch = @StationHasSwitch, DepartureTimeStop = @DepartureTimeStop, GatewayUsername = @GatewayUsername, GatewayPassword = @GatewayPassword, GatewayEncryptID = @GatewayEncryptID, AllowStopChargeAfterFull = @AllowStopChargeAfterFull, ActiveChargeCount = @ActiveChargeCount, ZigBeeID = @ZigBeeID, StopChargeDelayLoopCount = @StopChargeDelayLoopCount, StopChargeIfNotSubmit = @StopChargeIfNotSubmit"
                    +" WHERE [ID] = @GWID";

                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();

                SqlParameter ParamgatewayName = new SqlParameter();
                ParamgatewayName.ParameterName = "@gatewayName";
                ParamgatewayName.Value = tbName.Text;
                cmd.Parameters.Add(ParamgatewayName);

                SqlParameter ParamIPAddress = new SqlParameter();
                ParamIPAddress.ParameterName = "@IPAddress";
                ParamIPAddress.Value = tbIPaddr.Text;
                cmd.Parameters.Add(ParamIPAddress);

                SqlParameter ParamNetworkID = new SqlParameter();
                ParamNetworkID.ParameterName = "@NetworkID";
                ParamNetworkID.Value = ddlNetworkID.SelectedValue ;
                cmd.Parameters.Add(ParamNetworkID);

                SqlParameter ParamChargingLevel = new SqlParameter();
                ParamChargingLevel.ParameterName = "@ChargingLevel";
                ParamChargingLevel.Value = ddlChargingLevel.SelectedValue;
                cmd.Parameters.Add(ParamChargingLevel);

                SqlParameter ParamPowerSources = new SqlParameter();
                ParamPowerSources.ParameterName = "@PowerSources";
                ParamPowerSources.Value = ddlPowerSources.SelectedValue;
                cmd.Parameters.Add(ParamPowerSources);

                SqlParameter ParamParkingLotID = new SqlParameter();
                ParamParkingLotID.ParameterName = "@ParkingLotID";
                ParamParkingLotID.Value = ddlParkingLotNames.SelectedValue;
                cmd.Parameters.Add(ParamParkingLotID);

                SqlParameter ParamLevel = new SqlParameter();
                ParamLevel.ParameterName = "@Level";
                ParamLevel.Value = tbLevel.Text;
                cmd.Parameters.Add(ParamLevel);

                SqlParameter ParamMaxCurrent = new SqlParameter();
                ParamMaxCurrent.ParameterName = "@MaxCurrent";
                ParamMaxCurrent.Value = tbMaxCurrent.Text;
                cmd.Parameters.Add(ParamMaxCurrent);

                SqlParameter ParamMaxStationCurrent = new SqlParameter();
                ParamMaxStationCurrent.ParameterName = "@MaxStationCurrent";
                ParamMaxStationCurrent.Value = tbMaxStationCurrent.Text;
                cmd.Parameters.Add(ParamMaxStationCurrent);

                SqlParameter ParamMaxVoltage = new SqlParameter();
                ParamMaxVoltage.ParameterName = "@MaxVoltage";
                ParamMaxVoltage.Value = tbMaxVoltage.Text;
                cmd.Parameters.Add(ParamMaxVoltage);

                SqlParameter ParamMaxStationVoltage = new SqlParameter();
                ParamMaxStationVoltage.ParameterName = "@MaxStationVoltage";
                ParamMaxStationVoltage.Value = tbMaxStationVoltage.Text;
                cmd.Parameters.Add(ParamMaxStationVoltage);

                SqlParameter ParamSourceCurrent = new SqlParameter();
                ParamSourceCurrent.ParameterName = "@SourceCurrent";
                ParamSourceCurrent.Value = tbSourceCurrent.Text;
                cmd.Parameters.Add(ParamSourceCurrent);

                SqlParameter ParamSourceVoltage = new SqlParameter();
                ParamSourceVoltage.ParameterName = "@SourceVoltage";
                ParamSourceVoltage.Value = tbSourceVoltage.Text;
                cmd.Parameters.Add(ParamSourceVoltage);
                
                SqlParameter ParamAlgorithmID = new SqlParameter();
                ParamAlgorithmID.ParameterName = "@AlgorithmID";
                ParamAlgorithmID.Value = ddlAlgorithm.SelectedValue;
                cmd.Parameters.Add(ParamAlgorithmID);

                SqlParameter ParamRetrieveInterval = new SqlParameter();
                ParamRetrieveInterval.ParameterName = "@RetrieveInterval";
                ParamRetrieveInterval.Value = tbRetrieveInterval.Text;
                cmd.Parameters.Add(ParamRetrieveInterval);

                SqlParameter ParamEnable = new SqlParameter();
                ParamEnable.ParameterName = "@Enable";
                ParamEnable.Value = ddlEnable.SelectedValue;
                cmd.Parameters.Add(ParamEnable);

                SqlParameter ParamSOC = new SqlParameter();
                ParamSOC.ParameterName = "@SOC";
                ParamSOC.Value = ddlhasSOC.SelectedValue;
                cmd.Parameters.Add(ParamSOC);
                                
                SqlParameter ParamControllable = new SqlParameter();
                ParamControllable.ParameterName = "@Controllable";
                ParamControllable.Value = ddlControllable.SelectedValue;
                cmd.Parameters.Add(ParamControllable);

                SqlParameter ParamAggregateControl = new SqlParameter();
                ParamAggregateControl.ParameterName = "@AggregateControl";
                ParamAggregateControl.Value = ddlAggregateControl.SelectedValue;
                cmd.Parameters.Add(ParamAggregateControl);

                SqlParameter ParamAllowReboot = new SqlParameter();
                ParamAllowReboot.ParameterName = "@AllowReboot";
                ParamAllowReboot.Value = ddlAllowReboot.SelectedValue;
                cmd.Parameters.Add(ParamAllowReboot);

                SqlParameter ParamTimeQuantum = new SqlParameter();
                ParamTimeQuantum.ParameterName = "@TimeQuantum";
                ParamTimeQuantum.Value = tbTimeQuantum.Text;
                cmd.Parameters.Add(ParamTimeQuantum);

                SqlParameter ParamTimeOut = new SqlParameter();
                ParamTimeOut.ParameterName = "@TimeOut";
                ParamTimeOut.Value = tbTimeOut.Text;
                cmd.Parameters.Add(ParamTimeOut);

                SqlParameter ParmRetryTimes = new SqlParameter();
                ParmRetryTimes.ParameterName = "@RetryTimes";
                ParmRetryTimes.Value = tbRetryTime.Text;
                cmd.Parameters.Add(ParmRetryTimes);

                SqlParameter ParamCurrentValve = new SqlParameter();
                ParamCurrentValve.ParameterName = "@CurrentValve";
                ParamCurrentValve.Value = tbCurrentThreshold.Text;
                cmd.Parameters.Add(ParamCurrentValve);

                SqlParameter ParamNodeControlDelay = new SqlParameter();
                ParamNodeControlDelay.ParameterName = "@NodeControlDelay";
                ParamNodeControlDelay.Value = tbNodeControlDelay.Text;
                cmd.Parameters.Add(ParamNodeControlDelay);

                SqlParameter ParamActivate = new SqlParameter();
                ParamActivate.ParameterName = "@Activate";
                ParamActivate.Value = ddlActivate.SelectedValue;
                cmd.Parameters.Add(ParamActivate);

                SqlParameter ParamPrimaryEmail = new SqlParameter();
                ParamPrimaryEmail.ParameterName = "@PrimaryEmail";
                ParamPrimaryEmail.Value = tbPrimaryEmail.Text;
                cmd.Parameters.Add(ParamPrimaryEmail);

                SqlParameter ParamInDays = new SqlParameter();
                ParamInDays.ParameterName = "@InDays";
                ParamInDays.Value = tbInDays.Text;
                cmd.Parameters.Add(ParamInDays);

                SqlParameter ParamMaxTimesInDay = new SqlParameter();
                ParamMaxTimesInDay.ParameterName = "@MaxTimesInDay";
                ParamMaxTimesInDay.Value = tbMaxTimesPerDay.Text;
                cmd.Parameters.Add(ParamMaxTimesInDay);

                SqlParameter ParamLeftCurrentValve = new SqlParameter();
                ParamLeftCurrentValve.ParameterName = "@LeftCurrentValve";
                ParamLeftCurrentValve.Value = tbLeftCurrentThreshold.Text;
                cmd.Parameters.Add(ParamLeftCurrentValve);

                SqlParameter ParamNote = new SqlParameter();
                ParamNote.ParameterName = "@Note";
                ParamNote.Value = tbNote.Text;
                cmd.Parameters.Add(ParamNote);

                SqlParameter ParamGWID = new SqlParameter();
                ParamGWID.ParameterName = "@GWID";
                ParamGWID.Value = tbID.Text;
                cmd.Parameters.Add(ParamGWID);

                SqlParameter ParamChargingTypeID = new SqlParameter();
                ParamChargingTypeID.ParameterName = "@ChargingTypeID";
                ParamChargingTypeID.Value = ddlChargingType.SelectedValue;
                cmd.Parameters.Add(ParamChargingTypeID);

                cmd.Parameters.AddWithValue("@MinPoint", ddlMinPoint.SelectedValue);
                cmd.Parameters.AddWithValue("@MaxDutyCycle", ddlMaxDutyCycle.SelectedValue);
                cmd.Parameters.AddWithValue("@MinDutyCycle", ddlMinDutyCycle.SelectedValue);

                if (ddlTransactionNodeID.SelectedValue == "&nbsp;")
                    cmd.Parameters.AddWithValue("@TransactionNodeID", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@TransactionNodeID", ddlTransactionNodeID.SelectedValue);

                cmd.Parameters.AddWithValue("@UseOrganizationPriceList", ddlUseOrganizationPriceList.SelectedValue);
                cmd.Parameters.AddWithValue("@StationHasSwitch", ddlStationHasSwitch.SelectedValue);
                cmd.Parameters.AddWithValue("@DepartureTimeStop", ddlDepartureTimeStop.SelectedValue);
                cmd.Parameters.AddWithValue("@GatewayUsername", tbGatewayUsername.Text);
                cmd.Parameters.AddWithValue("@GatewayPassword", tbGatewayPassword.Text);
                cmd.Parameters.AddWithValue("@GatewayEncryptID", tbGatewayEncryptID.Text);
                cmd.Parameters.AddWithValue("@AllowStopChargeAfterFull", ddlAllowStopChargeAfterFull.SelectedValue);
                cmd.Parameters.AddWithValue("@ActiveChargeCount", ddlActiveChargeCount.SelectedValue);
                cmd.Parameters.AddWithValue("@ZigBeeID", tbZigbeeId.Text);
                cmd.Parameters.AddWithValue("@StopChargeDelayLoopCount", ddlStopChargeDelayLoopCount.SelectedValue);
                cmd.Parameters.AddWithValue("@StopChargeIfNotSubmit", ddlStopChargeIfNotSubmit.SelectedValue);
                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br> Error while Updating: " + ex.Message;
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }
            string ParkingLot = string.Empty;

            if (ddlModeParkingLot.SelectedIndex == 0)
            {
            }
            else
                ParkingLot = ddlModeParkingLot.SelectedValue;


            PopulategvGateway(ddlModeCity.SelectedValue, ParkingLot, cbShowActivated.Checked);
            //updated = true;
            //Response.Redirect("EditGateway.aspx", true);
            fillInTxtBoxes();

            // Enable Service
            btnRestartWindowsService.Enabled = true;

            HideError();
            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            var str = enable == "true"
                ? "Please do not forget to restart Windows Services before leaving this page."
                : "";
            PopUpError("Updated. " + str);

        }

        protected void btnNewClick(object sender, EventArgs e)
        {

            ClearAlllblError(); // Clear all lbl Error Messages
            bool blnPassValidCheck = blnNewGatewayValidCheck();

            if (!blnPassValidCheck)
            {
                PopUpError("Error: See right for details.");
                return;
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; SqlDataReader readerProfile = null;

            try
            {
                strQuery = "INSERT INTO [Gateway](Name, [IP Address], NetworkID, [Charging Level], [Parking Lot ID], [Level], [Max Current], [Max Station Current], "
                         + " [Max Voltage], [Max Station Voltage], [Source Current], [Source Voltage], [Algorithm ID], [Retrieve Interval], Enable, [Time Quantum], "
                         + " [Time Out], [Retry Times], [CurrentValve], NodeControlDelay, Activate, PrimaryEmail, InDays, MaxTimesInDay, LeftCurrentValve, HasSOC, PowerSources, Controllable, AggregateControl, AllowReboot, ChargingTypeID, Note, MinPoint, MaxDutyCycle, MinDutyCycle, TransactionNodeID, UseOrganizationPriceList, StationHasSwitch, DepartureTimeStop, GatewayUsername, GatewayPassword, GatewayEncryptID, AllowStopChargeAfterFull, ActiveChargeCount, ZigBeeID, StopChargeDelayLoopCount, StopChargeIfNotSubmit) "
                         + " VALUES(@Name, @IPAddress, @NetworkID, @ChargingLevel, @ParkingLotID, @Level, @MaxCurrent, @MaxStationCurrent, @MaxVoltage, @MaxStationVoltage, "
                         + " @SourceCurrent, @SourceVoltage, @AlgorithmID, @RetrieveInterval, @Enable, @TimeQuantum, @TimeOut, @RetryTimes, @CurrentValve, @NodeControlDelay, @Activate, "
                         + " @PrimaryEmail, @InDays, @MaxTimesInDay, @LeftCurrentValve, @SOC,@PowerSources, @Controllable, @AggregateControl, @AllowReboot, @ChargingTypeID, @Note, @MinPoint, @MaxDutyCycle, @MinDutyCycle, @TransactionNodeID, @UseOrganizationPriceList, @StationHasSwitch, @DepartureTimeStop, @GatewayUsername, @GatewayPassword, @GatewayEncryptID, @AllowStopChargeAfterFull, @ActiveChargeCount, @ZigBeeID, @StopChargeDelayLoopCount, @StopChargeIfNotSubmit)";   

                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();


                SqlParameter ParamgatewayName = new SqlParameter();
                ParamgatewayName.ParameterName = "@Name";
                ParamgatewayName.Value = tbName.Text;
                cmd.Parameters.Add(ParamgatewayName);

                SqlParameter ParamIPAddress = new SqlParameter();
                ParamIPAddress.ParameterName = "@IPAddress";
                ParamIPAddress.Value = tbIPaddr.Text;
                cmd.Parameters.Add(ParamIPAddress);

                SqlParameter ParamNetworkID = new SqlParameter();
                ParamNetworkID.ParameterName = "@NetworkID";
                ParamNetworkID.Value = ddlNetworkID.SelectedValue;
                cmd.Parameters.Add(ParamNetworkID);

                SqlParameter ParamChargingLevel = new SqlParameter();
                ParamChargingLevel.ParameterName = "@ChargingLevel";
                ParamChargingLevel.Value = ddlChargingLevel.SelectedValue;
                cmd.Parameters.Add(ParamChargingLevel);

                SqlParameter ParamPowerSources = new SqlParameter();
                ParamPowerSources.ParameterName = "@PowerSources";
                ParamPowerSources.Value = ddlPowerSources.SelectedValue;
                cmd.Parameters.Add(ParamPowerSources);

                SqlParameter ParamParkingLotID = new SqlParameter();
                ParamParkingLotID.ParameterName = "@ParkingLotID";
                ParamParkingLotID.Value = ddlParkingLotNames.SelectedValue;
                cmd.Parameters.Add(ParamParkingLotID);

                SqlParameter ParamLevel = new SqlParameter();
                ParamLevel.ParameterName = "@Level";
                ParamLevel.Value = tbLevel.Text;
                cmd.Parameters.Add(ParamLevel);

                SqlParameter ParamMaxCurrent = new SqlParameter();
                ParamMaxCurrent.ParameterName = "@MaxCurrent";
                ParamMaxCurrent.Value = tbMaxCurrent.Text;
                cmd.Parameters.Add(ParamMaxCurrent);

                SqlParameter ParamMaxStationCurrent = new SqlParameter();
                ParamMaxStationCurrent.ParameterName = "@MaxStationCurrent";
                ParamMaxStationCurrent.Value = tbMaxStationCurrent.Text;
                cmd.Parameters.Add(ParamMaxStationCurrent);

                SqlParameter ParamMaxVoltage = new SqlParameter();
                ParamMaxVoltage.ParameterName = "@MaxVoltage";
                ParamMaxVoltage.Value = tbMaxVoltage.Text;
                cmd.Parameters.Add(ParamMaxVoltage);

                SqlParameter ParamMaxStationVoltage = new SqlParameter();
                ParamMaxStationVoltage.ParameterName = "@MaxStationVoltage";
                ParamMaxStationVoltage.Value = tbMaxStationVoltage.Text;
                cmd.Parameters.Add(ParamMaxStationVoltage);

                SqlParameter ParamSourceCurrent = new SqlParameter();
                ParamSourceCurrent.ParameterName = "@SourceCurrent";
                ParamSourceCurrent.Value = tbSourceCurrent.Text;
                cmd.Parameters.Add(ParamSourceCurrent);


                SqlParameter ParamSourceVoltage = new SqlParameter();
                ParamSourceVoltage.ParameterName = "@SourceVoltage";
                ParamSourceVoltage.Value = tbSourceVoltage.Text;
                cmd.Parameters.Add(ParamSourceVoltage);

                SqlParameter ParamAlgorithmID = new SqlParameter();
                ParamAlgorithmID.ParameterName = "@AlgorithmID";
                ParamAlgorithmID.Value = ddlAlgorithm.SelectedValue;
                cmd.Parameters.Add(ParamAlgorithmID);

                SqlParameter ParamRetrieveInterval = new SqlParameter();
                ParamRetrieveInterval.ParameterName = "@RetrieveInterval";
                ParamRetrieveInterval.Value = tbRetrieveInterval.Text;
                cmd.Parameters.Add(ParamRetrieveInterval);

                SqlParameter ParamEnable = new SqlParameter();
                ParamEnable.ParameterName = "@Enable";
                ParamEnable.Value = ddlEnable.SelectedValue;
                cmd.Parameters.Add(ParamEnable);

                SqlParameter ParamSOC = new SqlParameter();
                ParamSOC.ParameterName = "@SOC";
                ParamSOC.Value = ddlhasSOC.SelectedValue;
                cmd.Parameters.Add(ParamSOC);

                SqlParameter ParamControllable = new SqlParameter();
                ParamControllable.ParameterName = "@Controllable";
                ParamControllable.Value = ddlControllable.SelectedValue;
                cmd.Parameters.Add(ParamControllable);

                SqlParameter ParamAggregateControl = new SqlParameter();
                ParamAggregateControl.ParameterName = "@AggregateControl";
                ParamAggregateControl.Value = ddlAggregateControl.SelectedValue;
                cmd.Parameters.Add(ParamAggregateControl);

                SqlParameter ParamAllowReboot = new SqlParameter();
                ParamAllowReboot.ParameterName = "@AllowReboot";
                ParamAllowReboot.Value = ddlAllowReboot.SelectedValue;
                cmd.Parameters.Add(ParamAllowReboot);

                SqlParameter ParamTimeQuantum = new SqlParameter();
                ParamTimeQuantum.ParameterName = "@TimeQuantum";
                ParamTimeQuantum.Value = tbTimeQuantum.Text;
                cmd.Parameters.Add(ParamTimeQuantum);

                SqlParameter ParamTimeOut = new SqlParameter();
                ParamTimeOut.ParameterName = "@TimeOut";
                ParamTimeOut.Value = tbTimeOut.Text;
                cmd.Parameters.Add(ParamTimeOut);

                SqlParameter ParmRetryTimes = new SqlParameter();
                ParmRetryTimes.ParameterName = "@RetryTimes";
                ParmRetryTimes.Value = tbRetryTime.Text;
                cmd.Parameters.Add(ParmRetryTimes);

                SqlParameter ParamCurrentValve = new SqlParameter();
                ParamCurrentValve.ParameterName = "@CurrentValve";
                ParamCurrentValve.Value = tbCurrentThreshold.Text;
                cmd.Parameters.Add(ParamCurrentValve);

                SqlParameter ParamNodeControlDelay = new SqlParameter();
                ParamNodeControlDelay.ParameterName = "@NodeControlDelay";
                ParamNodeControlDelay.Value = tbNodeControlDelay.Text;
                cmd.Parameters.Add(ParamNodeControlDelay);

                SqlParameter ParamActivate = new SqlParameter();
                ParamActivate.ParameterName = "@Activate";
                ParamActivate.Value = ddlActivate.SelectedValue;
                cmd.Parameters.Add(ParamActivate);

                SqlParameter ParamPrimaryEmail = new SqlParameter();
                ParamPrimaryEmail.ParameterName = "@PrimaryEmail";
                ParamPrimaryEmail.Value = tbPrimaryEmail.Text;
                cmd.Parameters.Add(ParamPrimaryEmail);

                SqlParameter ParamInDays = new SqlParameter();
                ParamInDays.ParameterName = "@InDays";
                ParamInDays.Value = tbInDays.Text;
                cmd.Parameters.Add(ParamInDays);

                SqlParameter ParamMaxTimesInDay = new SqlParameter();
                ParamMaxTimesInDay.ParameterName = "@MaxTimesInDay";
                ParamMaxTimesInDay.Value = tbMaxTimesPerDay.Text;
                cmd.Parameters.Add(ParamMaxTimesInDay);

                SqlParameter ParamLeftCurrentValve = new SqlParameter();
                ParamLeftCurrentValve.ParameterName = "@LeftCurrentValve";
                ParamLeftCurrentValve.Value = tbLeftCurrentThreshold.Text;
                cmd.Parameters.Add(ParamLeftCurrentValve);

                SqlParameter ParamNote = new SqlParameter();
                ParamNote.ParameterName = "@Note";
                ParamNote.Value = tbNote.Text;
                cmd.Parameters.Add(ParamNote);

                SqlParameter ParamGWID = new SqlParameter();
                ParamGWID.ParameterName = "@GWID";
                ParamGWID.Value = tbID.Text;
                cmd.Parameters.Add(ParamGWID);

                SqlParameter ParamChargingTypeID = new SqlParameter();
                ParamChargingTypeID.ParameterName = "@ChargingTypeID";
                ParamChargingTypeID.Value = ddlChargingType.SelectedValue;
                cmd.Parameters.Add(ParamChargingTypeID);

                cmd.Parameters.AddWithValue("@MinPoint", ddlMinPoint.SelectedValue);
                cmd.Parameters.AddWithValue("@MaxDutyCycle", ddlMaxDutyCycle.SelectedValue);
                cmd.Parameters.AddWithValue("@MinDutyCycle", ddlMinDutyCycle.SelectedValue);
                
                if (ddlTransactionNodeID.SelectedValue == "&nbsp;")
                    cmd.Parameters.AddWithValue("@TransactionNodeID", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@TransactionNodeID", ddlTransactionNodeID.SelectedValue);

                cmd.Parameters.AddWithValue("@UseOrganizationPriceList", ddlUseOrganizationPriceList.SelectedValue);
                cmd.Parameters.AddWithValue("@StationHasSwitch", ddlStationHasSwitch.SelectedValue);
                cmd.Parameters.AddWithValue("@DepartureTimeStop", ddlDepartureTimeStop.SelectedValue);
                cmd.Parameters.AddWithValue("@GatewayUsername", tbGatewayUsername.Text);
                cmd.Parameters.AddWithValue("@GatewayPassword", tbGatewayPassword.Text);
                cmd.Parameters.AddWithValue("@GatewayEncryptID", tbGatewayEncryptID.Text);
                cmd.Parameters.AddWithValue("@AllowStopChargeAfterFull", ddlAllowStopChargeAfterFull.SelectedValue);
                cmd.Parameters.AddWithValue("@ActiveChargeCount", ddlActiveChargeCount.SelectedValue);
                cmd.Parameters.AddWithValue("@ZigBeeID", tbZigbeeId.Text);
                cmd.Parameters.AddWithValue("@StopChargeDelayLoopCount", ddlStopChargeDelayLoopCount.SelectedValue);
                cmd.Parameters.AddWithValue("@StopChargeIfNotSubmit", ddlStopChargeIfNotSubmit.SelectedValue);
                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                ShowMessage("Error in btnNewClick: " + ex.Message);
                
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();

            }
            string ParkingLot = string.Empty;

            if (ddlModeParkingLot.SelectedIndex == 0)
            {
            }
            else
                ParkingLot = ddlModeParkingLot.SelectedValue;

  
            PopulategvGateway(ddlModeCity.SelectedValue, ParkingLot, cbShowActivated.Checked);
            //newed = true;
            //Response.Redirect("EditGateway.aspx", true);

            ClearAllTbs();
            gvGateway.SelectedIndex = -1;

            // Enable Service
            btnRestartWindowsService.Enabled = true;
            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            var str = enable == "true"
                ? "Please do not forget to restart Windows Services before leaving this page."
                : "";
            PopUpError("New Information Added. " + str);
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            gvGateway.SelectedIndex = -1;
            ClearAllTbs();
        }

        protected void ClearAlllblError()
        {
            lblIPaddrError.Text = string.Empty;
            lblddlParkingLotNamesError.Text = string.Empty;
            lblddlNetworkIDError.Text = string.Empty;
            lbltbIDError.Text = string.Empty;
            lblddlAlgorithmError.Text = string.Empty;
            lbltbNameError.Text = string.Empty;
            lblddlChargingTypeError.Text = string.Empty;
            //lblddlTransactionNodeIDError.Text = string.Empty;
        }

        protected void ClearAllTbs()
        {
            btnUpdate.Visible = false;
            tbID.Text = string.Empty;
            tbNote.Text = string.Empty;
            ddlNetworkID.SelectedIndex = 0;
            ddlChargingLevel.SelectedIndex = 0;
            ddlPowerSources.SelectedIndex = 0;
            try
            {
                ddlParkingLotNames.SelectedIndex = 0;
            }
            catch
            {
            }
          //  tbChargingLvl.Text = string.Empty;
            tbCurrentThreshold.Text = string.Empty;
            tbInDays.Text = string.Empty;
            tbIPaddr.Text = string.Empty;
            tbLeftCurrentThreshold.Text = string.Empty;
            tbLevel.Text = string.Empty;
            tbMaxCurrent.Text = string.Empty;
            tbMaxStationCurrent.Text = string.Empty;
            tbMaxStationVoltage.Text = string.Empty;
            tbMaxTimesPerDay.Text = string.Empty;
            tbMaxVoltage.Text = string.Empty;
            tbName.Text = string.Empty;
            //tbNetworkID.Text = string.Empty;

            tbPrimaryEmail.Text = string.Empty;
            tbRetrieveInterval.Text = string.Empty;
            tbRetryTime.Text = string.Empty;
            tbSourceCurrent.Text = string.Empty;
            tbSourceVoltage.Text = string.Empty;
            tbTimeOut.Text = string.Empty;
            tbTimeQuantum.Text = string.Empty;
            tbNodeControlDelay.Text = string.Empty;
            ddlEnable.SelectedIndex = 0;
            ddlhasSOC.SelectedIndex = 0;
            ddlAlgorithm.SelectedIndex = 0;
            ddlChargingType.SelectedIndex = 0;
         //   ddlParkingLotNames.SelectedIndex = 0;
            ddlActivate.SelectedIndex = 0;
            ddlControllable.SelectedIndex = 0;
            ddlAggregateControl.SelectedIndex = 0;
            ddlAllowReboot.SelectedIndex = 0;
            ddlMinPoint.SelectedIndex = 0;
            ddlMaxDutyCycle.SelectedIndex = 0;
            ddlMinDutyCycle.SelectedIndex = 0;
            ddlTransactionNodeID.SelectedIndex = 0;
            ddlUseOrganizationPriceList.SelectedIndex = 0;
            ddlStationHasSwitch.SelectedIndex = 0;
            ddlDepartureTimeStop.SelectedIndex = 0;
            tbGatewayUsername.Text = "admin";
            tbGatewayPassword.Text = "admin";
            tbGatewayEncryptID.Text = "Base64";
            ddlAllowStopChargeAfterFull.SelectedIndex = 0;
            ddlActiveChargeCount.SelectedIndex = 0;
            tbZigbeeId.Text = string.Empty;
            ddlStopChargeDelayLoopCount.SelectedIndex = 0;
            ddlStopChargeIfNotSubmit.SelectedIndex = 0;
        }


        protected void btnGo_Click(object sender, EventArgs e)
        {
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
            lblCatchError.Text = string.Empty;
        }

        protected void ddlModeParkingLot_SelectedIndexChanged(object sender, EventArgs e) // Upon DDL MODE PARKING LOT Change.
        {
            ClearAllTbs(); // Clear all Text Boxes when switching select range
            gvGateway.SelectedIndex = -1; // Reset selection of gridview
            ddlParkingLotNames.SelectedIndex = ddlModeParkingLot.SelectedIndex;

            // Hide the btnUpdate since there is nothing to update yet.
            btnUpdate.Visible = false;


            PopulategvGateway(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, cbShowActivated.Checked); // Puts in City GUID, and Parking lot NAME

         }


        #endregion
        #region SQLAccess functions - i.e. ReturnGuidfromCityname
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
                strQuery = "SELECT Id FROM [City] WHERE [Name] = '" + UserCity + "'";
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
        #region Sorting
        protected void gvGatewaySorting(object sender, GridViewSortEventArgs e)
        {
            gvGateway.SelectedIndex = -1;
            ClearAllTbs();
            // DataTable dataTable = gvUserEditor.DataSource as DataTable;
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvGateway.DataSource = dataTable.DefaultView;
                gvGateway.DataBind();
            }
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvGateway.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvGateway.Columns.IndexOf(field);
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
            gvGateway.HeaderRow.Cells[index].Controls.Add(sortImage2);
        }
        void AddSortImage(int columnIndex, GridViewRow headerRow)
        {
            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (gvGateway.SortDirection == SortDirection.Ascending)
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
            foreach (DataControlField c in gvGateway.Columns)
            {
                if (c.SortExpression == SortExpression)
                    break;
                i++;
            }
            return i;
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

        protected void RestartWindowsService()
        {
            // Create an Instance of ServiceController
            ServiceController myService = new ServiceController();
            myService.ServiceName = serviceString;

            // For more info on Service Status, go to: http://msdn.microsoft.com/en-us/library/system.serviceprocess.servicecontrollerstatus.aspx
            string svcStatus = myService.Status.ToString();

            string svcStatusWas = ""; 

            // if the service is not stopped,
            if (svcStatus != "Stopped")
            {
                try
                {
                    // Stop Service
                    myService.Stop();

                    svcStatusWas = ""; 

                    // Wait Until Stopped
                    while (svcStatus != "Stopped")
                    {
                        // Wait 1 second between loops
                        System.Threading.Thread.Sleep(1000);
                        svcStatusWas = svcStatus;

                        myService.Refresh();    // REMEMBER: svcStatus was SET TO myService.Status.ToString above. 
                        // Use the Refresh() Method to refresh the value of myService.Status and 
                        // reassign it to svcStatus
                        svcStatus = myService.Status.ToString();
                    }
                }
                catch (Exception ex)
                {
                    PopUpError("Error at RestartWindowsService1: " + ex.Message + ", Status: " + svcStatus);
                }
            }

            
            // Start the service
            try
            {
                myService.Start();

                // Wait for the startup to finish. "Running" is the string to denote that the service has started.
                while (svcStatus != "Running")
                {
                    // Wait 1 second between loops
                    System.Threading.Thread.Sleep(1000);
                    svcStatusWas = svcStatus;
                    
                    myService.Refresh();    // REMEMBER: svcStatus was SET TO myService.Status.ToString above. 
                    // Use the Refresh() Method to refresh the value of myService.Status and 
                    // reassign it to svcStatus
                    svcStatus = myService.Status.ToString();
                }
            }
            catch (Exception ex2)
            {
                PopUpError("Error at RestartWindowsService2: " + ex2.Message + ", Status: " + svcStatus);
            }
            finally
            {
                PopUpError("Services Restarted");
            }
        }

        protected void btnRestartWindowsService_Click(object sender, EventArgs e)
        {
            btnRestartWindowsService.Enabled = false;
            RestartWindowsService();                       
        }
    }
}