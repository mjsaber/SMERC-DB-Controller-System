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
 using System.Windows.Forms.VisualStyles;
 using RTMC;
using System.ServiceProcess;

/* TODO:
 *  Find the Phrase "TODO" in this document to see what still must be done in the future
*/


namespace EVEditor
{
    public partial class EditStation : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        string serviceString = WebConfigurationManager.AppSettings["WindowsService"];

        // ColumnsToHide, makes the hiding of columns well visible to programmer.  To remove or add columns to hide, modify the string below
        string[] ColumnsToHide = { "Note", "Gateway ID" ,"ChargingTypeID"};

        string[] strArrRolesToAllow = { "General Administrator" };
        //strArrTypesToAllow is the role types that will be inserted in the string array above ^, into strArrRolesToAllow
        //The "EditGateway" function assumes that only "General Administrators" may have full accessibility to all organizations.
        string[] strArrTypesToAllow = { "Administrator" }; //Make sure there are no white spaces before or after the string. i.e. " Maintainer"


        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.        
        string[] strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        protected enum ExistState
        {
            NotExist,
            Exist,
            ExistButFail
        }

        // Page Load Event
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
                cbShowActivated.Checked = true;
                Initialize();
                tbManufacturer.Text = "SMERC";
            }
        }

        protected void Initialize()
        {
            PopulategvStation(string.Empty, string.Empty, string.Empty, cbShowActivated.Checked); // Initialize the gridview with all data points in the server.
            PopulateddlModeCity();
            PopulateddlModeParkingLot();
            PopulateddlModeGateway();            
            PopulateddlRelayChannel();
            PopulateddlPowerSource();
            PopulateddlChargingType();
            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            if (enable == "true") return;
            btnRestartWindowsService.Visible = false;
            lblRestartWindowsService.Visible = false;
        }

        #region gvStation - Functions

        protected ExistState CheckStationIdNamePairState(string ID, string name)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT [Controllable], [Enable], [Activate] FROM Station WHERE ID = '" + ID + "' AND Name = '" + name +"'";
                var DT = new DataTable();
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
                            lblCatchError.Text += ex.Message;
                            lblCatchError.Visible = true;
                            btnHideCatchError.Visible = true;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopupError for clarity to note there are no Gateways for given selection.
                            return ExistState.NotExist;
                    }
                    var controllable = DT.Rows[0][0].ToString() == "false";
                    var enable = DT.Rows[0][1].ToString() == "true";
                    var activate = DT.Rows[0][2].ToString() == "false";

                    if (!enable)
                        return ExistState.ExistButFail;
                    return ExistState.Exist;
                }
            }
        }

        protected bool IsNameChanged(string ID)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT [Name] FROM Station WHERE ID = '" + ID + "'";
                var DT = new DataTable();
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
                            lblCatchError.Text += ex.Message;
                            lblCatchError.Visible = true;
                            btnHideCatchError.Visible = true;
                        }
                        if (DT.Rows.Count == 0)
                            return true;
                    }
                }
                return DT.Rows[0][0].ToString() != tbName.Text;
            }
        }

        protected void PopulategvStation(string strOrgID, string ParkingLotID, string GatewayID, bool blnActivate) // City is either .Empty (for no City pref), or has a city, to filter by city name
        {
            if (strOrgID == "-1") // This is to account for a empty City Selection.  (If no city is checked, but a parking lot is checked, this if code allows to properly populate the gridview)
                strOrgID = string.Empty;
            if (ParkingLotID == "-1")
                ParkingLotID = string.Empty;
            if (GatewayID == "-1")
                GatewayID = string.Empty;

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
                string sqlQuery = "SELECT DISTINCT g.Name as Gateway, s.Name, s.Controllable, s.Enable, s.Manufacturer, s.[Charging Level], s.[Space No], s.Latitude, s.Longitude, s.Priority, s.Activate, s.Note, s.ID, s.[Gateway ID], s.[Base Value], s.[Start Value], s.[Relay Channel],s.PowerSourceNo, s.CreateTime, s.[AC Meter], ct.ID AS ChargingTypeID, ct.Type AS ChargingType" +
                    " FROM Station AS s INNER JOIN Gateway AS g ON s.[Gateway ID] = g.ID INNER JOIN [ChargingType] AS ct ON ct.ID = s.ChargingTypeID";
                
                // if this user is a Master user
                // the user may access all the information
                if (blnListhasMasterRole)
                {                    
                    if (GatewayID != string.Empty)
                    {
                        sqlQuery += " WHERE g.ID = '" + GatewayID +"'";
                    }
                    else if (ParkingLotID != string.Empty)
                    {
                        sqlQuery += " INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.ID AND pl.ID='" + ParkingLotID + "'";
                    }
                    else if (strOrgID != string.Empty)
                    {
                        sqlQuery += " INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.ID AND pl.[City ID]='" + strOrgID + "'";
                    }
                }
                // the user is not a master user.
                // listCityGUID will contain at least one organization GUID.
                else
                {
                    if (GatewayID != string.Empty)
                    {
                        sqlQuery += " WHERE g.ID ='" + GatewayID +"'";
                    }
                    else if (ParkingLotID != string.Empty)
                    {
                        sqlQuery += " INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.ID AND pl.ID='" + ParkingLotID + "'";
                    }
                    else if (strOrgID != string.Empty)
                    {
                        sqlQuery += " INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.ID AND pl.[City ID]='" + strOrgID + "'";
                    }
                    else
                    {
                        int listCount = listCityGUID.Count;
                        sqlQuery += " INNER JOIN [Parking Lot] as pl ON g.[Parking Lot ID] = pl.ID AND ";
                        
                        for (int i = 0; i < listCount; i++)
                        {
                            sqlQuery += " pl.[City ID] ='" + listCityGUID[i] + "'";
                            if (i < listCount - 1)
                            {
                                sqlQuery += " OR ";
                            }
                        }
                    }
                }

                if (blnActivate)
                {
                    sqlQuery += " AND s.[Activate] = '1'";
                }

                sqlQuery += " ORDER BY s.[Name] ASC";


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
            gvStation.DataSource = Session["data"]; // Source with purposes of sorting.  Session allows to track which way the data is being sorted.
            gvStation.DataBind(); // Bind data

            int intTotalStations = 0;
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                intTotalStations++;
            }
            lblTotalUsers.Text = "Stations in this area: " + intTotalStations;
        }


        protected void gvStationSelectedIndex(object sender, EventArgs e)
        {
            lblCatchError.Text = string.Empty;
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
            ClearAlllblError();

            fillInTxtBoxes();
            btnUpdate.Visible = true;

        }

        protected void gvStationPaging(Object sender, GridViewPageEventArgs e)
        {
            // Save selected values prior to the page change.
            int intddlmodecityindex = ddlModeCity.SelectedIndex;
            int intddlparkingindex = ddlModeParkingLot.SelectedIndex;

            gvStation.SelectedIndex = -1;
            ClearAllTbs();

            DataTable dataTable = Session["data"] as DataTable;
            gvStation.PageIndex = e.NewPageIndex;
            gvStation.DataSource = dataTable;
            gvStation.DataBind();

            ddlModeCity.SelectedIndex = intddlmodecityindex;
            ddlModeParkingLot.SelectedIndex = intddlparkingindex;
        }



        #endregion
        #region PopulateDDL and DDL functions

        protected void PopulateddlRelayChannel()
        {
            ddlRelayChannel.Items.AddRange(Enumerable.Range(0, 49).Select(e => new ListItem(e.ToString())).ToArray());
        }
        protected void PopulateddlPowerSource()
        {
            ddlPowerSource.Items.AddRange(Enumerable.Range(1, 20).Select(e => new ListItem(e.ToString())).ToArray());
        }

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
                    if (blnListhasMasterRole)
                    {
                        strQuery = "SELECT ID, Name FROM [Parking Lot] ORDER BY Name";
                    }
                    else
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
                    
                    cnn.Open();
                    cmd = new SqlCommand(strQuery, cnn);
                    cmd.CommandType = CommandType.Text;
                    da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    dt = new DataTable();

                    da.Fill(dt);

                    ddlModeParkingLot.DataSource = dt; // Fill the Drop Down List
                    ddlModeParkingLot.DataValueField = "ID"; // DataValueFIeld contains the GUID of the Parking Lot
                    ddlModeParkingLot.DataTextField = "Name"; // DataTextField contains the Name of the Parking Lot
                    ddlModeParkingLot.DataBind();

                    ListItem li = new ListItem("All Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                    ddlModeParkingLot.Items.Insert(0, li);

                    da.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    PopUpError("PopulateddlModeParkingLot Error: " + ex.Message);
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
                    strQuery = "SELECT ID, Name FROM [Parking Lot] WHERE [City ID]='" + ddlModeCity.SelectedValue + "'" + " ORDER BY Name ";

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

                        ListItem li = new ListItem("Associated Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);
                    }
                    else
                    {
                        ListItem li = new ListItem("No Parking Lots", "-1"); // Add the Text of "All Parking Lots" to position 0
                        ddlModeParkingLot.Items.Insert(0, li);
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

      



        protected void PopulateddlModeGateway()
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
                    if (ddlModeParkingLot.SelectedValue != "-1")
                    {
                        strQuery += " WHERE pl.ID = '" + ddlModeParkingLot.SelectedValue + "' AND g.Activate= 1 ORDER BY g.Name";
                        strZerothMessage = "Associated Charging Boxes";
                    }
                    // If a city is chosen,                     
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
                    if (ddlModeParkingLot.SelectedValue != "-1")
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
                        strQuery += " WHERE pl.[City ID] ='" + ddlModeCity.SelectedValue + "'";
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

                // Check to see if there are no returned data sets
                bool blnIsEmptyGateway = false;
                if (dt.Rows.Count == 0)
                {
                    blnIsEmptyGateway = true;
                    strZerothMessage = "No Gateways";
                }

                if (!blnIsEmptyGateway)
                {

                    ddlModeGateway.DataSource = dt;
                    ddlModeGateway.DataValueField = "ID"; // DataValueField contains the GUID of the Gateway
                    ddlModeGateway.DataTextField = "Name"; // DataTextField contains the Name of the Gateway
                    ddlModeGateway.DataBind();

                    ddlGateway.DataSource = dt;
                    ddlGateway.DataValueField = "ID"; // DataValueField contains the GUID of the Gateway
                    ddlGateway.DataTextField = "Name"; // DataTextField contains the Name of the Gateway
                    ddlGateway.DataBind();

                    ListItem li2 = new ListItem(strZerothMessage, "-1");
                    ddlModeGateway.Items.Insert(0, li2);

                    ListItem li = new ListItem(strZerothMessage, "-1");
                    ddlGateway.Items.Insert(0, li);
                }
                else
                {
                    ListItem li2 = new ListItem(strZerothMessage, "-1");
                    ddlModeGateway.Items.Insert(0, li2);

                    ListItem li = new ListItem(strZerothMessage, "-1");
                    ddlGateway.Items.Insert(0, li);
                }

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("<br> PopulateddlGateway Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            try
            {
                ddlGateway.SelectedIndex = 0;
            }
            catch
            {
            }
        }

        protected void PopulateddlChargingType()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            ListItem li = new ListItem("Select...","-1");
            try{
                cnn.Open();
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
                ddlChargingType.Items.Insert(0,li);
            }
            catch(Exception ex)
            {
                ShowMessage("Populate Charging Type Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
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


        protected void ddlModeGateway_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllTbs(); // clear all TBs for new select range
            gvStation.SelectedIndex = -1; // Reset gv Selection

            // Hide Any Errors
            HideError();

            // Hide the btnUpdate since there is no selection made yet.
            btnUpdate.Visible = false;
            // Match the selection on both ddls
            ddlGateway.SelectedValue = ddlModeGateway.SelectedValue;

            // Repopulate the gridview based on the new settings.
            PopulategvStation(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlModeGateway.SelectedValue, cbShowActivated.Checked);
        }

        protected void ddlModeCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllTbs();
            gvStation.SelectedIndex = -1;

            // Hide the btnUpdate because no selection is made yet.
            btnUpdate.Visible = false;

            // Hide the error messages.
            HideError();

            // Populate the gridview with the new settings.
            PopulategvStation(ddlModeCity.SelectedValue, string.Empty, string.Empty, cbShowActivated.Checked);

            ddlModeParkingLot.Items.Clear();
            PopulateddlModeParkingLot();
            ddlModeParkingLot.SelectedIndex = 0;
            

            ddlModeGateway.Items.Clear();
            ddlGateway.Items.Clear();
            PopulateddlModeGateway();
            ddlModeGateway.SelectedIndex = 0;
            ddlGateway.SelectedIndex = 0;
            ddlChargingType.SelectedIndex = 0;
            
            //ddlModeGateway.SelectedIndex = 0;
            //ddlGateway.SelectedIndex = 0;
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

        void fillInTxtBoxes()
        {
            GridViewRow gvRow;

            try
            {
                gvRow = gvStation.Rows[gvStation.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                return;
            }
            tbID.Text = gvRow.Cells[findGVcolumn("ID")].Text;
            tbName.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Station Name")].Text);
            tbBaseValue.Text = gvRow.Cells[findGVcolumn("Base Value")].Text;
            tbStartValue.Text = gvRow.Cells[findGVcolumn("Start Value")].Text;
            ddlRelayChannel.SelectedValue = gvRow.Cells[findGVcolumn("Relay Channel")].Text;
            ddlPowerSource.SelectedValue = gvRow.Cells[findGVcolumn("Power Source")].Text;

            try
            {
                ddlGateway.SelectedValue = gvRow.Cells[findGVcolumn("Gateway ID")].Text;
            }
            catch
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "The Gateway, " + Server.HtmlDecode(gvRow.Cells[findGVcolumn("Charging Box")].Text) + ", is de-activated or removed from the database.";
                ddlGateway.SelectedIndex = 0;
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

            tbSpaceNumber.Text = gvRow.Cells[findGVcolumn("Space No")].Text;
            tbLatitude.Text = gvRow.Cells[findGVcolumn("Latitude")].Text;
            tbLongitude.Text = gvRow.Cells[findGVcolumn("Longitude")].Text;
            ddlPriority.SelectedValue = gvRow.Cells[findGVcolumn("Priority")].Text;
            tbManufacturer.Text = gvRow.Cells[findGVcolumn("Manufacturer")].Text;
            ddlChargingLevel.SelectedValue = gvRow.Cells[findGVcolumn("Charging Level")].Text;

            string strNote = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Note")].Text);
            tbNote.Text = string.IsNullOrWhiteSpace(strNote) ? string.Empty : strNote;

            CheckBox cbControllable = (CheckBox)gvRow.Cells[findGVcolumn("Controllable")].Controls[0];
            if (cbControllable.Checked)
                ddlControllable.SelectedValue = "1";
            else
                ddlControllable.SelectedValue = "0";

            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            if (cbActivate.Checked)
                ddlActivate.SelectedValue = "1";
            else
                ddlActivate.SelectedValue = "0";

            CheckBox cbEnabled = (CheckBox)gvRow.Cells[findGVcolumn("Enable")].Controls[0];
            if (cbEnabled.Checked)
                ddlEnable.SelectedValue = "1";
            else
                ddlEnable.SelectedValue = "0";

            CheckBox cbACMeter = (CheckBox)gvRow.Cells[findGVcolumn("AC Meter")].Controls[0];
            if (cbACMeter.Checked)
                ddlACMeter.SelectedValue = "1";
            else
                ddlACMeter.SelectedValue = "0";


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

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvStation.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvStation.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void gvStationRowCreated(object sender, GridViewRowEventArgs e)
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

        #endregion
        #region ValidationChecks


        protected bool blnUpdateGWValidationCheck()
        {
            bool blnPassesValidation = true;

            if (ddlGateway.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlGatewayError.Text = "Select a Gateway.";
            }
            if (ddlChargingType.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlChargingTypeError.Text = "Select a ChargingType.";
            }
            return blnPassesValidation;
        }

        protected bool blnNewGatewayValidCheck()
        {
            bool blnPassesValidation = true;

            if (ddlGateway.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlGatewayError.Text = "Select a Gateway.";
            }
            if (ddlChargingType.SelectedIndex == 0)
            {
                blnPassesValidation = false;
                lblddlChargingTypeError.Text = "Select a ChargingType.";
            }
            return blnPassesValidation;
        }

        #endregion
        #region btn - rbl - Functions

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

        protected bool blnUpdateCheckForChange()
        {
            GridViewRow gvRow = gvStation.Rows[gvStation.SelectedIndex]; // Obtain selected index of gvStation
            string strPrevID = gvRow.Cells[findGVcolumn("ID")].Text;
            string strPrevGateway = gvRow.Cells[findGVcolumn("Charging Box")].Text;
            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            CheckBox cbEnabled = (CheckBox)gvRow.Cells[findGVcolumn("Enable")].Controls[0];
            CheckBox cbControllable = (CheckBox)gvRow.Cells[findGVcolumn("Controllable")].Controls[0];

            int intEnable = 0;
            if (cbEnabled.Checked)
                intEnable = 1;
            else
                intEnable = 0;

            int intActivate = 0;
            if (cbActivate.Checked)
                intActivate = 1;
            else
                intActivate = 0;

            int intControllable = 0;
            if (cbControllable.Checked)
                intControllable = 1;
            else
                intControllable = 0;

            if (ddlActivate.SelectedValue != intActivate.ToString())
                return true;

            if (ddlEnable.SelectedValue != intEnable.ToString())
                return true;

            if (ddlControllable.SelectedValue != intControllable.ToString())
                return true;

            if (strPrevID != tbID.Text)
            {
                return true;
            }

            if (strPrevGateway != ddlGateway.SelectedItem.Text)
            {
                return true;
            }

            return false;
        }


        protected string strReturnUserEmailFromStationID(string strStationID)
        {
            string strCSEmailList = string.Empty;

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT distinct m.[Email] " +
                           " FROM [EVDemo].[dbo].[aspnet_Membership] as m " +
                           " INNER JOIN [EVDemo].[dbo].[ChargingRecords] as cr ON cr.[UserID] = m.[UserId] " +
                           " WHERE cr.[StationID] = '" + strStationID + "'";

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);

                int intdtRows = dt.Rows.Count;

                for (int i = 0; i < intdtRows; i++)
                {
                    strCSEmailList += dt.Rows[i][0].ToString();
                    if (i < intdtRows - 1)
                        strCSEmailList += ", ";
                }

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strReturnUserEmailFromStationID: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strCSEmailList;
        }



        protected List<string> listReturnUserID(string listStationID)
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
                strQuery = "SELECT Distinct [UserID] FROM [ChargingRecords] WHERE [StationID] = '" + listStationID + "'";


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

        protected string strReturnOrgNameOfStation(string strStationID)
        {
            string strCityName = string.Empty;

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            try
            {
                cnn.Open();
                strQuery = "SELECT c.[Name] FROM " +
                           " [City] as c INNER JOIN [Parking Lot] as p ON p.[City ID] = c.[ID] " +
                           " INNER JOIN [Gateway] as g ON g.[Parking Lot ID] = p.[ID] " +
                           " INNER JOIN [Station] as s ON S.[Gateway ID] = g.[ID] WHERE s.[ID] = '" + strStationID + "'";

                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    strCityName = reader["Name"].ToString().Trim();
                }

                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strReturnOrgNameOfStation: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strCityName;
        }

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            ClearAlllblError(); // Clear the lblError Messages

            if (!blnUpdateGWValidationCheck())
            {
                lblCatchError.Text = "<br>Errors shown on right.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                return; // Exit out of function if errors are found.
            }

            GridViewRow gvRow = gvStation.Rows[gvStation.SelectedIndex]; // Obtain selected index of gvStation

            if (blnUpdateCheckForChange())
            {
                string strStationID = gvRow.Cells[findGVcolumn("ID")].Text;
                // List<string> listOfAffectedUsers = new List<string>();
                //listOfAffectedUsers = listReturnUserID(strStationID); // Return a list of the affected users by the station

                string strCSEmailList = strReturnUserEmailFromStationID(strStationID); // Return a Comma Separated Email list for all users associated with the altered station (For SendMail Function)
                string strOrgNameofStation = strReturnOrgNameOfStation(strStationID); // Return the Organization Name associated with the altered Station

                // TODO: 
                // Send mail to all users, notifying that the EV have been stopped charging.
                // User the SendMail Function
                // For example, something like this: 
                // SendMail(strOrganization, strCSEmailList, "Your EV has stopped charging on: " + DateTime.Now.ToString(), "EV Stopped Charging");
            }

            var id = gvRow.Cells[findGVcolumn("ID")].Text;
            var name = tbName.Text;
            //var oldname = tbName.Text;
            var msg = "";
            //var isNameChanged = IsNameChanged(id);
            //if (isNameChanged)
            //{
            //    var existence = CheckStationIdNamePairState(id, name);
            //    switch (existence)
            //    {
            //        case ExistState.Exist:
            //            PopUpError("Station <ID, Name> already existed and used, please use another <ID, Name> pair.");
            //            clearErrorButton();
            //            return;
            //        case ExistState.ExistButFail:
            //            name += "Dup";
            //            msg = "Station <ID, Name> already existed but broken, add suffix 'Dup' at the end of name.";
            //            break;
            //    }
            //}
            
            var cnn = new SqlConnection(connectionString);
            string strQuery;

            SqlCommand cmd; SqlDataReader readerProfile = null;

            try
            {
                strQuery = "UPDATE [Station] SET [Activate] = @Activated, [Gateway ID] = @GatewayID, Controllable = @Controllable, "
                    + " [Enable] = @Enable, [Manufacturer] = @Manufacturer, [Charging Level] = @ChargingLevel, [Space No] = @SpaceNo, "
                    + " [Latitude] = @Latitude, [Longitude] = @Longitude, [Priority] = @Priority, [Base Value] = @BaseValue, [Start Value] = @StartValue, [Relay Channel] = @RelayChannel, [PowerSourceNo] = @PowerSource, [ChargingTypeID] = @ChargingTypeID, [AC Meter] = @ACMeter, [Note] = @Note "
                    + " WHERE [ID] = @ID AND [Name] = @StationName";

                //if (isNameChanged)
                //{
                //    strQuery = "UPDATE [Station] SET [Name] = @StationName, [Activate] = @Activated, [Gateway ID] = @GatewayID, Controllable = @Controllable, "
                //    + " [Enable] = @Enable, [Manufacturer] = @Manufacturer, [Charging Level] = @ChargingLevel, [Space No] = @SpaceNo, "
                //    + " [Latitude] = @Latitude, [Longitude] = @Longitude, [Priority] = @Priority, [Base Value] = @BaseValue, [Start Value] = @StartValue, [Relay Channel] = @RelayChannel, [PowerSourceNo] = @PowerSource, [ChargingTypeID] = @ChargingTypeID, [AC Meter] = @ACMeter, [Note] = @Note "
                //    + " WHERE [ID] = @ID AND [Name] = @StationName";
                //}

                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();

                SqlParameter ParamStationName = new SqlParameter();
                ParamStationName.ParameterName = "@StationName";
                ParamStationName.Value = name;
                cmd.Parameters.Add(ParamStationName);

                //SqlParameter ParamOldStationName = new SqlParameter();
                //ParamOldStationName.ParameterName = "@OldStationName";
                //ParamOldStationName.Value = oldname;
                //cmd.Parameters.Add(ParamOldStationName);

                SqlParameter ParamAct = new SqlParameter();
                ParamAct.ParameterName = "@Activated";
                ParamAct.Value = ddlActivate.SelectedValue;
                cmd.Parameters.Add(ParamAct);

                SqlParameter ParamGatewayID = new SqlParameter();
                ParamGatewayID.ParameterName = "@GatewayID";
                ParamGatewayID.Value = ddlGateway.SelectedValue;
                cmd.Parameters.Add(ParamGatewayID);

                SqlParameter ParamManufacturer = new SqlParameter();
                ParamManufacturer.ParameterName = "@Manufacturer";
                ParamManufacturer.Value = tbManufacturer.Text;
                cmd.Parameters.Add(ParamManufacturer);

                SqlParameter ParamControllable = new SqlParameter();
                ParamControllable.ParameterName = "@Controllable";
                ParamControllable.Value = ddlControllable.SelectedValue;
                cmd.Parameters.Add(ParamControllable);

                SqlParameter ParamChargingLevel = new SqlParameter();
                ParamChargingLevel.ParameterName = "@ChargingLevel";
                ParamChargingLevel.Value = ddlChargingLevel.SelectedValue;
                cmd.Parameters.Add(ParamChargingLevel);

                SqlParameter ParamSpaceNo = new SqlParameter();
                ParamSpaceNo.ParameterName = "@SpaceNo";
                ParamSpaceNo.Value = tbSpaceNumber.Text;
                cmd.Parameters.Add(ParamSpaceNo);

                SqlParameter ParamEnable = new SqlParameter();
                ParamEnable.ParameterName = "@Enable";
                ParamEnable.Value = ddlEnable.SelectedValue;
                cmd.Parameters.Add(ParamEnable);

                SqlParameter ParamLatitude = new SqlParameter();
                ParamLatitude.ParameterName = "@Latitude";
                ParamLatitude.Value = tbLatitude.Text;
                cmd.Parameters.Add(ParamLatitude);

                SqlParameter ParamLongitude = new SqlParameter();
                ParamLongitude.ParameterName = "@Longitude";
                ParamLongitude.Value = tbLongitude.Text;
                cmd.Parameters.Add(ParamLongitude);

                SqlParameter ParamPriority = new SqlParameter();
                ParamPriority.ParameterName = "@Priority";
                ParamPriority.Value = ddlPriority.SelectedValue;
                cmd.Parameters.Add(ParamPriority);

                SqlParameter ParamBaseValue = new SqlParameter();
                ParamBaseValue.ParameterName = "@BaseValue";
                ParamBaseValue.Value = tbBaseValue.Text;
                cmd.Parameters.Add(ParamBaseValue);

                SqlParameter ParamStartValue = new SqlParameter();
                ParamStartValue.ParameterName = "@StartValue";
                ParamStartValue.Value = tbStartValue.Text;
                cmd.Parameters.Add(ParamStartValue);

                SqlParameter ParamRelayChannel = new SqlParameter();
                ParamRelayChannel.ParameterName = "@RelayChannel";
                ParamRelayChannel.Value = ddlRelayChannel.SelectedIndex;
                cmd.Parameters.Add(ParamRelayChannel);

                SqlParameter ParamPowerSource = new SqlParameter();
                ParamPowerSource.ParameterName = "@PowerSource";
                ParamPowerSource.Value = ddlPowerSource.SelectedValue;
                cmd.Parameters.Add(ParamPowerSource);

                SqlParameter ParamNote = new SqlParameter();
                ParamNote.ParameterName = "@Note";
                ParamNote.Value = tbNote.Text;
                cmd.Parameters.Add(ParamNote);

                SqlParameter ParamGWID = new SqlParameter();
                ParamGWID.ParameterName = "@ID";
                ParamGWID.Value = id;
                cmd.Parameters.Add(ParamGWID);

                SqlParameter ParamChargingTypeID = new SqlParameter();
                ParamChargingTypeID.ParameterName = "@ChargingTypeID";
                ParamChargingTypeID.Value = ddlChargingType.SelectedValue;
                cmd.Parameters.Add(ParamChargingTypeID);

                SqlParameter ParamACMeter = new SqlParameter();
                ParamACMeter.ParameterName = "@ACMeter";
                ParamACMeter.Value = ddlACMeter.SelectedValue;
                cmd.Parameters.Add(ParamACMeter);

                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br> Error while Updating: " + ex.Message;
                return;
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }
            string strParkingLot = string.Empty;
            string strGateway = string.Empty;

            PopulategvStation(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlModeGateway.SelectedValue, cbShowActivated.Checked);

            clearErrorButton();
            fillInTxtBoxes();

            // Enable Service
            btnRestartWindowsService.Enabled = true;

            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            var str = enable == "true"
                ? "Please do not forget to restart Windows Services before leaving this page."
                : "";
            PopUpError("Updated. " + str + msg);
        }

        protected string strReturnMainEnergy(string strStationID) // Return the Latest "Main Energy"
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;

            string strLatestMainEnergy = string.Empty;

            try
            {
                cnn.Open();
                strQuery = " SELECT TOP 1 [Main Energy] " +
                           " FROM [Station Record] " +
                           " WHERE [Station ID]= '" + strStationID + "' AND [Is Successful] = '1' " +
                           " ORDER BY [TimeStamp] desc";

                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    strLatestMainEnergy = reader["Main Energy"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strReturnMainEnergy: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strLatestMainEnergy;
        }



        protected string strNewEntryCheck(string strCurrentStationName) // Check for new station name that exists already, if so. then get last station information and email it
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;


            string strLatestStationID = string.Empty;

            try
            {
                cnn.Open();
                strQuery = " SELECT [ID] " +
                           " FROM Station " +
                           " WHERE [Name]= '" + strCurrentStationName + "' " +
                           " ORDER BY [CreateTime] desc";

                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    strLatestStationID = reader["ID"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error at strNewEntryCheck: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            return strLatestStationID; // Passes all tests, i.e. a previous name does not already exist.
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            ClearAlllblError(); // Clear all lbl Error Messages

            if (!blnNewGatewayValidCheck())
            {
                lblCatchError.Text = " <br> Errors shown on right.";
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                return;
            }

            string strPrevId = strNewEntryCheck(tbName.Text);
            if (!string.IsNullOrWhiteSpace(strPrevId))
            {
                tbBaseValue.Text = strReturnMainEnergy(strPrevId);
                // TODO:
                // The StartValue = Meter Energy.  Hardware restrictions do not allow us to do this yet.  
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; SqlDataReader readerProfile = null;

            try
            {
                strQuery = "INSERT INTO [Station](ID, Name, [Gateway ID], [Charging Level], Controllable, Enable, Manufacturer, "
                         + " [Space No], Latitude, Longitude, Priority, Activate, [Base Value], [Start Value], [Relay Channel], [PowerSourceNo], ChargingTypeID, [AC Meter], Note) "
                         + " VALUES(@StationID, @StationName, @GatewayID, @ChargingLevel, @Controllable, @Enable, @Manufacturer, @SpaceNo, "
                         + " @Latitude, @Longitude, @Priority, @Activate, @BaseValue, @StartValue, @RelayChannel, @PowerSource, @ChargingTypeID, @ACMeter, @Note)";

                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();

                SqlParameter ParamStationID = new SqlParameter();
                ParamStationID.ParameterName = "@StationID";
                ParamStationID.Value = tbID.Text;
                cmd.Parameters.Add(ParamStationID);

                SqlParameter ParamGatewayID = new SqlParameter();
                ParamGatewayID.ParameterName = "@GatewayID";
                ParamGatewayID.Value = ddlGateway.SelectedValue;
                cmd.Parameters.Add(ParamGatewayID);

                SqlParameter ParamStationName = new SqlParameter();
                ParamStationName.ParameterName = "@StationName";
                ParamStationName.Value = tbName.Text;
                cmd.Parameters.Add(ParamStationName);

                SqlParameter ParamManufacturer = new SqlParameter();
                ParamManufacturer.ParameterName = "@Manufacturer";
                ParamManufacturer.Value = tbManufacturer.Text;
                cmd.Parameters.Add(ParamManufacturer);

                SqlParameter ParamControllable = new SqlParameter();
                ParamControllable.ParameterName = "@Controllable";
                ParamControllable.Value = ddlControllable.SelectedValue;
                cmd.Parameters.Add(ParamControllable);

                SqlParameter ParamChargingLevel = new SqlParameter();
                ParamChargingLevel.ParameterName = "@ChargingLevel";
                ParamChargingLevel.Value = ddlChargingLevel.SelectedValue;
                cmd.Parameters.Add(ParamChargingLevel);

                SqlParameter ParamSpaceNo = new SqlParameter();
                ParamSpaceNo.ParameterName = "@SpaceNo";
                ParamSpaceNo.Value = tbSpaceNumber.Text;
                cmd.Parameters.Add(ParamSpaceNo);

                SqlParameter ParamEnable = new SqlParameter();
                ParamEnable.ParameterName = "@Enable";
                ParamEnable.Value = ddlEnable.SelectedValue;
                cmd.Parameters.Add(ParamEnable);

                SqlParameter ParamLatitude = new SqlParameter();
                ParamLatitude.ParameterName = "@Latitude";
                ParamLatitude.Value = tbLatitude.Text;
                cmd.Parameters.Add(ParamLatitude);

                SqlParameter ParamLongitude = new SqlParameter();
                ParamLongitude.ParameterName = "@Longitude";
                ParamLongitude.Value = tbLongitude.Text;
                cmd.Parameters.Add(ParamLongitude);

                SqlParameter ParamPriority = new SqlParameter();
                ParamPriority.ParameterName = "@Priority";
                ParamPriority.Value = ddlPriority.SelectedValue;
                cmd.Parameters.Add(ParamPriority);

                SqlParameter ParamActivate = new SqlParameter();
                ParamActivate.ParameterName = "@Activate";
                ParamActivate.Value = ddlActivate.SelectedValue;
                cmd.Parameters.Add(ParamActivate);

                SqlParameter ParamBaseValue = new SqlParameter();
                ParamBaseValue.ParameterName = "@BaseValue";
                ParamBaseValue.Value = tbBaseValue.Text;
                cmd.Parameters.Add(ParamBaseValue);

                SqlParameter ParamStartValue = new SqlParameter();
                ParamStartValue.ParameterName = "@StartValue";
                ParamStartValue.Value = tbStartValue.Text;
                cmd.Parameters.Add(ParamStartValue);

                SqlParameter ParamRelayChannel = new SqlParameter();
                ParamRelayChannel.ParameterName = "@RelayChannel";
                ParamRelayChannel.Value = ddlRelayChannel.SelectedIndex;
                cmd.Parameters.Add(ParamRelayChannel);

                SqlParameter ParamPowerSource = new SqlParameter();
                ParamPowerSource.ParameterName = "@PowerSource";
                ParamPowerSource.Value = ddlPowerSource.SelectedValue;
                cmd.Parameters.Add(ParamPowerSource);

                SqlParameter ParamNote = new SqlParameter();
                ParamNote.ParameterName = "@Note";
                ParamNote.Value = tbNote.Text;
                cmd.Parameters.Add(ParamNote);

                SqlParameter ParamChargingTypeID = new SqlParameter();
                ParamChargingTypeID.ParameterName = "@ChargingTypeID";
                ParamChargingTypeID.Value = ddlChargingType.SelectedValue;
                cmd.Parameters.Add(ParamChargingTypeID);

                SqlParameter ParamACMeter = new SqlParameter();
                ParamACMeter.ParameterName = "@ACMeter";
                ParamACMeter.Value = ddlACMeter.SelectedValue;
                cmd.Parameters.Add(ParamACMeter);

                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                lblCatchError.Text += " <br> Error at btnNewClick: " + ex.Message;
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                return;
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();

            }
            PopulategvStation(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlModeGateway.SelectedValue, cbShowActivated.Checked);


            ClearAllTbs();
            clearErrorButton();
            gvStation.SelectedIndex = -1;

            // Enable Service
            btnRestartWindowsService.Enabled = true;

            var enable = WebConfigurationManager.AppSettings["RestartWindowsService"];
            var str = enable == "true"
                ? "Please do not forget to restart Windows Services before leaving this page."
                : "";
            PopUpError("New Information Added. " + str);
        }

        protected void clearErrorButton()
        {
            lblCatchError.Visible = false;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void btnClearClick(object sender, EventArgs e)
        {

            ClearAllTbs();
            clearErrorButton();
            gvStation.SelectedIndex = -1;
            btnUpdate.Visible = false;
        }

        protected void ClearAlllblError()
        {
            lbltbIDError.Text = string.Empty;
            lbltbNameError.Text = string.Empty;
            lblddlGatewayError.Text = string.Empty;
            lblddlChargingTypeError.Text = string.Empty;
            lbltbSpaceNumberError.Text = string.Empty;
            lbltbLatitudeError.Text = string.Empty;
            lbltblLongitudeError.Text = string.Empty;
            lbltbPriorityError.Text = string.Empty;
            lbltbManufacturerError.Text = string.Empty;
        }

        protected void ClearAllTbs()
        {
            tbID.Text = string.Empty;
            tbName.Text = string.Empty;
            tbBaseValue.Text = string.Empty;
            tbStartValue.Text = string.Empty;
            ddlRelayChannel.SelectedIndex = 0;
            ddlPowerSource.SelectedIndex = 0;
            ddlControllable.SelectedIndex = 0;

            try
            {
                ddlGateway.SelectedIndex = 0;
            }
            catch
            {

            }
            ddlChargingType.SelectedIndex = 0;
            tbSpaceNumber.Text = string.Empty;
            tbLatitude.Text = string.Empty;
            tbLongitude.Text = string.Empty;
            ddlPriority.SelectedIndex = 0;
            tbManufacturer.Text = "SMERC";
            ddlChargingLevel.SelectedIndex = 0;
            ddlEnable.SelectedIndex = 0;
            ddlControllable.SelectedIndex = 0;
            ddlActivate.SelectedIndex = 0;
            tbNote.Text = string.Empty;
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
            gvStation.SelectedIndex = -1; // Reset selection of gridview

            // Hide the btnUpdate since no selection is mad yet.
            btnUpdate.Visible = false;

            // Clear the ddlModeGateway items and repopulate them based on the parking lot selection
            ddlModeGateway.Items.Clear();
            ddlGateway.Items.Clear();
            PopulateddlModeGateway();
            ddlModeGateway.SelectedIndex = 0;
            ddlGateway.SelectedIndex = 0;
            ddlChargingType.SelectedIndex = 0;
            PopulategvStation(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlGateway.SelectedValue, cbShowActivated.Checked);    
        }


        #endregion
        #region Sorting

        protected void gvStationSorting(object sender, GridViewSortEventArgs e)
        {
            // DataTable dataTable = gvUserEditor.DataSource as DataTable;
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvStation.DataSource = dataTable.DefaultView;
                gvStation.DataBind();
            }

            gvStation.SelectedIndex = -1;
            ClearAllTbs();

            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvStation.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvStation.Columns.IndexOf(field);
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
            gvStation.HeaderRow.Cells[index].Controls.Add(sortImage2);
        }
        void AddSortImage(int columnIndex, GridViewRow headerRow)
        {
            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (gvStation.SortDirection == SortDirection.Ascending)
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
            foreach (DataControlField c in gvStation.Columns)
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
        protected string strReturnGuidFromParkingLot(string strParkingLot)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            string strPLGuid = string.Empty;
            try
            {
                strQuery = "SELECT Id FROM [Parking Lot] WHERE [Name] = '" + strParkingLot + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                cmd.Dispose();
                strPLGuid = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                lblCatchError.Visible = true;
                btnHideCatchError.Visible = true;
                lblCatchError.Text += "<br>Error from: strReturnGuidFromParkingLot " + ex.Message;

            }
            finally
            {
                if (cnn != null)
                {
                    cnn.Close();
                }
            }
            return strPLGuid;
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

        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            PopulategvStation(ddlModeCity.SelectedValue, ddlModeParkingLot.SelectedValue, ddlModeGateway.SelectedValue, cbShowActivated.Checked);
        }

        protected void RestartWindowsService()
        {
            // Create an Instance of ServiceController
            ServiceController myService = new ServiceController();
            //myService.ServiceName = "StationControllerAndDataCollector";
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

        // Restart Windows Services
        protected void btnRestartWindowsService_Click(object sender, EventArgs e)
        {
            btnRestartWindowsService.Enabled = false;
            RestartWindowsService();
            
            PopUpError("Services Restarted");
        }

    }

}
