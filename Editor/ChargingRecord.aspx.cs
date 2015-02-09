using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

using System.Web;
using System.Web.UI;
using System.Web.Security;

using System.Web.UI.WebControls;
using System.Web.Configuration;

using RTMC;

namespace EVEditor
{
    public partial class ChargingRecord : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };

        string[] strArrRolesToAllow = { "General Administrator" };
        //  string[] strArrRolesToAllow = { };
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        string[] strArrTypesToAllow = { "Administrator" };

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };
        string[] ColumnsToHide = { "ID", "UserID", "EnergyPrice", "ChargingAlgorithm", "IsInCharging", "ChargingTimes", "Priority", "StartVoltage", "StartCurrent", "StartPF", "StartActivePower", "StartApparentPower", "EndVoltage", "EndCurrent", "EndPF", "EndActivePower", "EndApparentPower", "LastStartCharging", "LastStartMainPower", "LastStopCharging", "LastStopMainPower", "IsEndedByUser", "ScheduleID", "ChargingCost", "TotalChargingTime", "SOC", "SOCRetrieveTime", "CalculateCO2", "LeaveTime", "OdometerReading", "AggregateControl" };
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
                }
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }
            if (!IsPostBack)
            {
                cbShowIsEnd.Checked = true;
                //cbShowActivatedPL.Checked = true;
                populategvChargingRecord(ddlOrganization.SelectedValue, cbShowIsEnd.Checked);
                populateddlOrganization();
            }
        }
        #region Population Function
        protected void populateddlOrganization()
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
                string strListItemInsert = "All Organizations";
                // Only populate the corresponding organizations associated with the
                // logged in user
                if (blnListhasMasterRole)
                {
                    strQuery = "SELECT ID, Name FROM City";
                    
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

                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();

                da.Fill(dt);

                ddlOrganization.DataSource = dt;
                ddlOrganization.DataValueField = "ID"; // DataValueField contains the GUID of the City
                ddlOrganization.DataTextField = "Name"; // DataTextField contains the Name of the City
                ddlOrganization.DataBind();

                ListItem li = new ListItem(strListItemInsert, "-1");

                ddlOrganization.Items.Insert(0, li);

                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("<br> PopulateddlModeCity Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
        }

        protected void populategvChargingRecord(string strOrgID, bool isChecked)
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                if (strOrgID == "-1")
                {
                    strOrgID = string.Empty;
                }

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

                string sqlQuery = "SELECT cr.[ID], u.UserName, cr.UserID, cr.StationName, cr.StartTime, cr.StartMainPower, cr.IsEnd, cr.EndTime, cr.EndMainPower, cr.EmailAddress, cr.ZipCode, cr.MaxPowerRequired, cr.MaxPowerPriceAccepted, cr.VehicleID, cr.EnergyPrice, cr.ChargingAlgorithm, cr.IsInCharging, cr.ChargingTimes, cr.Priority, cr.StartVoltage, cr.StartCurrent, cr.StartPF, cr.StartActivePower, cr.StartApparentPower, cr.EndVoltage, cr.EndCurrent, cr.EndPF, cr.EndActivePower, cr.EndApparentPower, cr.LastStartCharging, cr.LastStartMainPower, cr.LastStopCharging, cr.LastStopMainPower, cr.IsEndedByUser, cr.ScheduleID, cr.ChargingCost, cr.TotalCharingTime, cr.SOC, cr.SOCRetrieveTime, cr.CalculateCO2, cr.LeaveTime, cr.OdometerReading, cr.AggregateControl " +
                                  " FROM [ChargingRecords] as cr  " +
                                  " INNER JOIN [aspnet_Users] as u ON u.UserId = cr.UserID " +
                                  " INNER JOIN [Station] as s ON s.ID = cr.StationID " +
                                  " INNER JOIN [Gateway] as g ON  g.ID = s.[Gateway ID]" +
                                  " INNER JOIN [Parking Lot] as p ON p.ID = g.[Parking Lot ID]  " +
                                  " INNER JOIN [City] as c ON c.ID = p.[City ID] ";

                // if this user is a Master user
                // the user may access all the information
                if (blnListhasMasterRole)
                {
                    // if an organization is chosen, populate the associated data
                    if (strOrgID != string.Empty)
                    {
                        sqlQuery += " WHERE p.[City ID] = '" + strOrgID + "'";
                    }

                }
                // the user is not a master user.
                // listCityGUID will contain at least one organization GUID.
                else
                {
                    if (strOrgID != string.Empty)
                    {
                        sqlQuery += " WHERE pl.[City ID] = '" + strOrgID + "'";
                    }
                    else
                    {
                        int listCount = listCityGUID.Count;

                        sqlQuery += " WHERE ";

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
                if (isChecked == true)
                {
                    sqlQuery += " AND cr.IsEnd = '0' ";
                }

                // Order the selection by name
                sqlQuery += " ORDER BY cr.StartTime DESC";

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
                            ShowError("Error at populateChargingRecord: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopUpMessage for clarity to note there are no Gateways for given selection.
                        {
                            ShowError("No data in this selection");
                        }
                    }
                }
            }

            Session["data"] = DT;
            gvChargingRecord.DataSource = Session["data"];
            gvChargingRecord.DataBind();
        }
        protected void gvChargingRecordPaging(object sender, GridViewPageEventArgs e)
        {
            gvChargingRecord.SelectedIndex = -1;

            DataTable dataTable = Session["data"] as DataTable;

            gvChargingRecord.PageIndex = e.NewPageIndex;
            gvChargingRecord.DataSource = dataTable;
            gvChargingRecord.DataBind();
        }

        protected void gvChargingRecordSelectedIndex(object sender, EventArgs e)
        {
            HideError();
            fillinInfo();
            btnUpdate.Visible = true;
            btnCloseCharging.Visible = true;

        }
        protected void gvChargingRecordRowCreated(object sender, GridViewRowEventArgs e)
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
        #region Helper Function
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
        protected void ClearAllTbs()
        {
            btnUpdate.Visible = false;
            btnCloseCharging.Visible = false;
            lID.Text = string.Empty;
            lStationName.Text  = string.Empty;
            lUserID.Text = string.Empty;
            tbEnergyPrice.Text = string.Empty;	
            tbChargingAlgorithm.Text=string.Empty;
            tbIsInCharging.Text = string.Empty;
            tbChargingTimes.Text = string.Empty;

            tbStartTime.Text = string.Empty;
            tbStartMainPower.Text = string.Empty;
            
            tbStartVoltage.Text = string.Empty;
            tbStartCurrent.Text = string.Empty;	
            tbStartPF.Text = string.Empty;	
            tbStartActivePower.Text = string.Empty;	
            tbStartApparentPower.Text = string.Empty;
            tbPriority.Text = string.Empty;

            tbIsEnd.Text = string.Empty;
            
            tbEndTime.Text = string.Empty;

            tbEndVoltage.Text = string.Empty;	
            tbEndCurrent.Text = string.Empty;	
            tbEndPF.Text = string.Empty;	
            tbEndActivePower.Text = string.Empty;	
            tbEndApparentPower.Text = string.Empty;	
            tbLastStartMainPower.Text = string.Empty;	
            tbEndMainPower.Text = string.Empty;	
            tbLastStartCharging.Text = string.Empty;	
            tbLastStopCharging.Text = string.Empty;	
            tbLastStopMainPower.Text = string.Empty;	
            tbIsEndedByUser.Text = string.Empty;	
            tbScheduleID.Text = string.Empty;	
            tbChargingCost.Text = string.Empty;	
            tbTotalChargingTime.Text = string.Empty;	
            tbSOC.Text = string.Empty;	
            tbSOCRetrieveTime.Text = string.Empty;
            tbCalculateCO2.Text = string.Empty;
            
            tbEmailAddress.Text = string.Empty;
            tbZipCode.Text = string.Empty;

            tbLeaveTime.Text = string.Empty;
            tbOdometerReading.Text = string.Empty;

            tbMaxPowerRequired.Text = string.Empty;
            tbMaxPowerPriceAccepted.Text = string.Empty;

            tbVehicleID.Text = string.Empty;

            tbAggregateControl.Text = string.Empty;
        }
        protected void HideError()
        {
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void fillinInfo()
        {
            GridViewRow gvRow;
            try
            {
                gvRow = gvChargingRecord.Rows[gvChargingRecord.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                HideError();
                return;
            }

            lID.Text = gvRow.Cells[findGVcolumn("ID")].Text;
            lStationName.Text = gvRow.Cells[findGVcolumn("Station Name")].Text;
            lUserID.Text = gvRow.Cells[findGVcolumn("UserID")].Text;

            tbEnergyPrice.Text = getColumnText(gvRow, "EnergyPrice") ;
            tbChargingAlgorithm.Text = getColumnText(gvRow, "ChargingAlgorithm");
            tbIsInCharging.Text = getColumnText(gvRow, "IsInCharging");
            tbChargingTimes.Text = getColumnText(gvRow, "ChargingTimes");

            tbStartTime.Text = getColumnText(gvRow, "Start Time");
            tbStartMainPower.Text = getColumnText(gvRow, "Start Main Power");

            tbStartVoltage.Text = getColumnText(gvRow, "StartVoltage");
            tbStartCurrent.Text = getColumnText(gvRow, "StartCurrent");
            tbStartPF.Text = getColumnText(gvRow, "StartPF");
            tbStartActivePower.Text = getColumnText(gvRow, "StartActivePower");
            tbStartApparentPower.Text = getColumnText(gvRow, "StartApparentPower");
            tbPriority.Text = getColumnText(gvRow, "Priority");

            tbIsEnd.Text = gvRow.Cells[findGVcolumn("Is End")].Text;
            tbEndTime.Text = getColumnText(gvRow, "End Time");

            tbEndVoltage.Text = getColumnText(gvRow, "EndVoltage");
            tbEndCurrent.Text = getColumnText(gvRow, "EndCurrent");
            tbEndPF.Text = getColumnText(gvRow, "EndPF");
            tbEndActivePower.Text = getColumnText(gvRow, "EndActivePower");
            tbEndApparentPower.Text = getColumnText(gvRow, "EndApparentPower");
            tbLastStartMainPower.Text = getColumnText(gvRow, "LastStartMainPower");

            tbEndMainPower.Text = getColumnText(gvRow, "End Main Power");

            tbLastStartCharging.Text = getColumnText(gvRow, "LastStartCharging");
            tbLastStopCharging.Text = getColumnText(gvRow, "LastStopCharging");
            tbLastStopMainPower.Text = getColumnText(gvRow, "LastStopMainPower");
            tbIsEndedByUser.Text = getColumnText(gvRow, "IsEndedByUser");
            tbScheduleID.Text = getColumnText(gvRow, "ScheduleID");
            tbChargingCost.Text = getColumnText(gvRow, "ChargingCost");
            tbTotalChargingTime.Text = getColumnText(gvRow, "TotalChargingTime");
            tbSOC.Text = getColumnText(gvRow, "SOC");
            tbSOCRetrieveTime.Text = getColumnText(gvRow, "SOCRetrieveTime");
            tbCalculateCO2.Text = getColumnText(gvRow, "CalculateCO2");

            tbEmailAddress.Text = getColumnText(gvRow, "Email Address");
            tbZipCode.Text = getColumnText(gvRow, "Zip Code");

            tbLeaveTime.Text = getColumnText(gvRow, "LeaveTime");
            tbOdometerReading.Text = getColumnText(gvRow, "OdometerReading");

            tbMaxPowerRequired.Text = getColumnText(gvRow, "Max Power Required");
            tbMaxPowerPriceAccepted.Text = getColumnText(gvRow, "Max Power Price Accepted");
            tbVehicleID.Text = getColumnText(gvRow, "Vehicle ID");
            tbAggregateControl.Text = getColumnText(gvRow, "AggregateControl");
           
        }
        protected string getColumnText(GridViewRow gvRow, string column)
        {
            string text = gvRow.Cells[findGVcolumn(column)].Text;
            if (text == "&nbsp;")
                text = "";
            return text;
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
        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvChargingRecord.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvChargingRecord.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }
        #endregion

        protected void ddlOrganization_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllTbs();
            HideError();

            // Reset the gridview selection
            gvChargingRecord.SelectedIndex = -1;

            // Hide the btnUpdate
            btnUpdate.Visible = false;
            btnCloseCharging.Visible = false;

            // Repopulate the gridview with the new settings.
            populategvChargingRecord(ddlOrganization.SelectedValue, cbShowIsEnd.Checked);
        }
        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
        }
        protected void cbShowIsEndCheckedChanged(object sender, EventArgs e)
        {
            ClearAllTbs();
            HideError();

            // Reset the gridview selection
            gvChargingRecord.SelectedIndex = -1;

            // Hide the btnUpdate
            btnUpdate.Visible = false;
            btnCloseCharging.Visible = false;

            // Repopulate the gridview with the new settings.
            populategvChargingRecord(ddlOrganization.SelectedValue, cbShowIsEnd.Checked);
        }
        protected void btnClose_Charging(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;

            SqlCommand cmd; 
            SqlDataReader readerProfile = null;
            GridViewRow gvRow = gvChargingRecord.Rows[gvChargingRecord.SelectedIndex]; // Obtain selected index of gvChargingRecord

            try
            {

                strQuery = "UPDATE [ChargingRecords] SET [EnergyPrice] = @EnergyPrice, [ChargingAlgorithm] = @ChargingAlgorithm, [IsInCharging] = @IsInCharging, [ChargingTimes]=@ChargingTimes, [StartTime] = @StartTime, [StartMainPower] = @StartMainPower, [StartVoltage]=@StartVoltage, [StartCurrent]=@StartCurrent, [StartPF]=@StartPF, [StartActivePower]=@StartActivePower, [StartApparentPower]=@StartApparentPower, [Priority]=@Priority, [IsEnd] = @IsEnd, [EndTime] = @EndTime, [EndVoltage]=@EndVoltage, [EndCurrent]=@EndCurrent, [EndPF]=@EndPF, [EndActivePower]=@EndActivePower, [EndApparentPower]=@EndApparentPower, [EndMainPower] = @EndMainPower, LastStartMainPower=@LastStartMainPower, [LastStartCharging]=@LastStartCharging, [LastStopCharging]=@LastStopCharging, [LastStopMainPower]=@LastStopMainPower, [IsEndedByUser]=@IsEndedByUser, [ScheduleID]=@ScheduleID, [ChargingCost]=@ChargingCost, [TotalCharingTime]=@TotalCharingTime, [SOC]=@SOC, [SOCRetrieveTime]=@SOCRetrieveTime, [CalculateCO2]=@CalculateCO2, [LeaveTime]=@LeaveTime, [OdometerReading]=@OdometerReading, [AggregateControl]=@AggregateControl, "
                    + " [ZipCode] = @ZipCode, [EmailAddress] = @EmailAddress, [MaxPowerRequired] = @MaxPowerRequired, [MaxPowerPriceAccepted] = @MaxPowerPriceAccepted, [VehicleID] = @VehicleID ";

                strQuery += " WHERE [ID] = @ID";
                cmd = new SqlCommand(strQuery, cnn);

                cnn.Open();


                // Normal params.

                SqlParameter ParamEnergyPrice = new SqlParameter();
                ParamEnergyPrice.ParameterName = "@EnergyPrice";
                    ParamEnergyPrice.Value = tbEnergyPrice.Text;
                cmd.Parameters.Add(ParamEnergyPrice);

                SqlParameter ParamChargingAlgorithm = new SqlParameter();
                ParamChargingAlgorithm.ParameterName = "@ChargingAlgorithm";
                    ParamChargingAlgorithm.Value = tbChargingAlgorithm.Text;
                cmd.Parameters.Add(ParamChargingAlgorithm);

                SqlParameter ParamIsInCharging = new SqlParameter();
                ParamIsInCharging.ParameterName = "@IsInCharging";
                ParamIsInCharging.Value = tbIsInCharging.Text;
                cmd.Parameters.Add(ParamIsInCharging);

                SqlParameter ParamChargingTimes = new SqlParameter();
                ParamChargingTimes.ParameterName = "@ChargingTimes";
                ParamChargingTimes.Value = tbChargingTimes.Text;
                cmd.Parameters.Add(ParamChargingTimes);

                SqlParameter ParamStartTime = new SqlParameter();
                ParamStartTime.ParameterName = "@StartTime";
                ParamStartTime.Value = tbStartTime.Text;
                cmd.Parameters.Add(ParamStartTime);

                SqlParameter ParamStartMainPower = new SqlParameter();
                ParamStartMainPower.ParameterName = "@StartMainPower"; 
                ParamStartMainPower.Value = tbStartMainPower.Text;
                cmd.Parameters.Add(ParamStartMainPower);

                SqlParameter ParamStartVoltage = new SqlParameter();
                ParamStartVoltage.ParameterName = "@StartVoltage";
                ParamStartVoltage.Value = tbStartVoltage.Text;
                cmd.Parameters.Add(ParamStartVoltage);

                SqlParameter ParamStartCurrent = new SqlParameter();
                ParamStartCurrent.ParameterName = "@StartCurrent";
                ParamStartCurrent.Value = tbStartCurrent.Text;
                cmd.Parameters.Add(ParamStartCurrent);

                SqlParameter ParamStartPF = new SqlParameter();
                ParamStartPF.ParameterName = "@StartPF";
                ParamStartPF.Value = tbStartPF.Text;
                cmd.Parameters.Add(ParamStartPF);

                SqlParameter ParamStartActivePower = new SqlParameter();
                ParamStartActivePower.ParameterName = "@StartActivePower";
                ParamStartActivePower.Value = tbStartActivePower.Text;
                cmd.Parameters.Add(ParamStartActivePower);

                SqlParameter ParamStartApparentPower = new SqlParameter();
                ParamStartApparentPower.ParameterName = "@StartApparentPower";
                ParamStartApparentPower.Value = tbStartApparentPower.Text;
                cmd.Parameters.Add(ParamStartApparentPower);

                SqlParameter ParamPriority = new SqlParameter();
                ParamPriority.ParameterName = "@Priority";
                ParamPriority.Value = tbPriority.Text;
                cmd.Parameters.Add(ParamPriority);

                SqlParameter ParamIsEnd = new SqlParameter();
                ParamIsEnd.ParameterName = "@IsEnd";
                ParamIsEnd.Value = "True";
                cmd.Parameters.Add(ParamIsEnd);

                SqlParameter ParamEndTime = new SqlParameter();
                ParamEndTime.ParameterName = "@EndTime";
                if (tbEndTime.Text == "")
                    ParamEndTime.Value = DateTime.Now.ToString();
                else
                    ParamEndTime.Value = tbEndTime.Text;
                cmd.Parameters.Add(ParamEndTime);

                SqlParameter ParamEndVoltage = new SqlParameter();
                ParamEndVoltage.ParameterName = "@EndVoltage";
                if (tbEndVoltage.Text == "")
                    ParamEndVoltage.Value = tbStartVoltage.Text;
                else
                    ParamEndVoltage.Value = tbEndVoltage.Text;
                cmd.Parameters.Add(ParamEndVoltage);

                SqlParameter ParamEndCurrent = new SqlParameter();
                ParamEndCurrent.ParameterName = "@EndCurrent";
                if (tbEndCurrent.Text == "")
                    ParamEndCurrent.Value = tbStartCurrent.Text;
                else
                    ParamEndCurrent.Value = tbEndCurrent.Text;
                cmd.Parameters.Add(ParamEndCurrent);

                SqlParameter ParamEndPF = new SqlParameter();
                ParamEndPF.ParameterName = "@EndPF";
                if (tbEndPF.Text == "")
                    ParamEndPF.Value = tbStartPF.Text;
                else
                    ParamEndPF.Value = tbEndPF.Text;
                cmd.Parameters.Add(ParamEndPF);

                SqlParameter ParamEndActivePower = new SqlParameter();
                ParamEndActivePower.ParameterName = "@EndActivePower";
                if (tbEndActivePower.Text == "")
                    ParamEndActivePower.Value = tbStartActivePower.Text;
                else
                    ParamEndActivePower.Value = tbEndActivePower.Text;
                cmd.Parameters.Add(ParamEndActivePower);

                SqlParameter ParamEndApparentPower = new SqlParameter();
                ParamEndApparentPower.ParameterName = "@EndApparentPower";
                if (tbEndApparentPower.Text == "")
                    ParamEndApparentPower.Value = tbStartApparentPower.Text;
                else
                    ParamEndApparentPower.Value = tbEndApparentPower.Text;
                cmd.Parameters.Add(ParamEndApparentPower);

                SqlParameter ParamEndMainPower = new SqlParameter();
                ParamEndMainPower.ParameterName = "@EndMainPower";
                if (tbEndMainPower.Text == "")
                    ParamEndMainPower.Value = tbStartMainPower.Text;
                else
                    ParamEndMainPower.Value = tbEndMainPower.Text;
                cmd.Parameters.Add(ParamEndMainPower);

                SqlParameter ParamLastStartMainPower = new SqlParameter();
                ParamLastStartMainPower.ParameterName = "@LastStartMainPower";
                if (tbLastStartMainPower.Text == "")
                    ParamLastStartMainPower.Value = DBNull.Value;
                else
                    ParamLastStartMainPower.Value = tbLastStartMainPower.Text;
                cmd.Parameters.Add(ParamLastStartMainPower);

                SqlParameter ParamLastStartCharging = new SqlParameter();
                ParamLastStartCharging.ParameterName = "@LastStartCharging";
                if (tbLastStartCharging.Text == "")
                    ParamLastStartCharging.Value = DBNull.Value;
                else
                    ParamLastStartCharging.Value = tbLastStartCharging.Text;
                cmd.Parameters.Add(ParamLastStartCharging);

                SqlParameter ParamLastStopCharging = new SqlParameter();
                ParamLastStopCharging.ParameterName = "@LastStopCharging";
                if (tbLastStopCharging.Text == "")
                    ParamLastStopCharging.Value = DBNull.Value;
                else
                    ParamLastStopCharging.Value = tbLastStopCharging.Text;
                cmd.Parameters.Add(ParamLastStopCharging);

                SqlParameter ParamLastStopMainPower = new SqlParameter();
                ParamLastStopMainPower.ParameterName = "@LastStopMainPower";
                if (tbLastStopMainPower.Text == "")
                    ParamLastStopMainPower.Value = DBNull.Value;
                else
                    ParamLastStopMainPower.Value = tbLastStopMainPower.Text;
                cmd.Parameters.Add(ParamLastStopMainPower);

                SqlParameter ParamIsEndedByUser = new SqlParameter();
                ParamIsEndedByUser.ParameterName = "@IsEndedByUser";
                ParamIsEndedByUser.Value = tbIsEndedByUser.Text;
                cmd.Parameters.Add(ParamIsEndedByUser);

                SqlParameter ParamScheduleID = new SqlParameter();
                ParamScheduleID.ParameterName = "@ScheduleID";
                if (tbScheduleID.Text == "")
                    ParamScheduleID.Value = DBNull.Value;
                else
                    ParamScheduleID.Value = tbScheduleID.Text;
                cmd.Parameters.Add(ParamScheduleID);

                SqlParameter ParamChargingCost = new SqlParameter();
                ParamChargingCost.ParameterName = "@ChargingCost";
                ParamChargingCost.Value = tbChargingCost.Text;
                cmd.Parameters.Add(ParamChargingCost);

                SqlParameter ParamTotalCharingTime = new SqlParameter();
                ParamTotalCharingTime.ParameterName = "@TotalCharingTime";
                ParamTotalCharingTime.Value = tbTotalChargingTime.Text;
                cmd.Parameters.Add(ParamTotalCharingTime);

                SqlParameter ParamSOC = new SqlParameter();
                ParamSOC.ParameterName = "@SOC";
                if (tbSOC.Text == "")
                    ParamSOC.Value = DBNull.Value;
                else
                    ParamSOC.Value = tbSOC.Text;
                cmd.Parameters.Add(ParamSOC);

                SqlParameter ParamSOCRetrieveTime = new SqlParameter();
                ParamSOCRetrieveTime.ParameterName = "@SOCRetrieveTime";
                if (tbSOCRetrieveTime.Text == "")
                    ParamSOCRetrieveTime.Value = DBNull.Value;
                else
                    ParamSOCRetrieveTime.Value = tbSOCRetrieveTime.Text;
                cmd.Parameters.Add(ParamSOCRetrieveTime);

                SqlParameter ParamCalculateCO2 = new SqlParameter();
                ParamCalculateCO2.ParameterName = "@CalculateCO2";
                ParamCalculateCO2.Value = tbCalculateCO2.Text;
                cmd.Parameters.Add(ParamCalculateCO2);

                SqlParameter ParamZipCode = new SqlParameter();
                ParamZipCode.ParameterName = "@ZipCode";
                if (tbZipCode.Text == "")
                    ParamZipCode.Value = DBNull.Value;
                else
                    ParamZipCode.Value = tbZipCode.Text;
                cmd.Parameters.Add(ParamZipCode);

                SqlParameter ParamEmailAddress = new SqlParameter();
                ParamEmailAddress.ParameterName = "@EmailAddress";
                if (tbEmailAddress.Text == "")
                    ParamEmailAddress.Value = DBNull.Value;
                else
                    ParamEmailAddress.Value = tbEmailAddress.Text;
                cmd.Parameters.Add(ParamEmailAddress);

                SqlParameter ParamLeaveTime = new SqlParameter();
                ParamLeaveTime.ParameterName = "@LeaveTime";
                if (tbLeaveTime.Text == "")
                    ParamLeaveTime.Value = DBNull.Value;
                else
                    ParamLeaveTime.Value = tbLeaveTime.Text;
                cmd.Parameters.Add(ParamLeaveTime);

                SqlParameter ParamOdometerReading = new SqlParameter();
                ParamOdometerReading.ParameterName = "@OdometerReading";
                if (tbOdometerReading.Text == "")
                    ParamOdometerReading.Value = DBNull.Value;
                else
                    ParamOdometerReading.Value = tbOdometerReading.Text;
                cmd.Parameters.Add(ParamOdometerReading);

                SqlParameter ParamMaxPowerRequired = new SqlParameter();
                ParamMaxPowerRequired.ParameterName = "@MaxPowerRequired";
                if (tbMaxPowerRequired.Text == "")
                    ParamMaxPowerRequired.Value = DBNull.Value;
                else
                    ParamMaxPowerRequired.Value = tbMaxPowerRequired.Text;
                cmd.Parameters.Add(ParamMaxPowerRequired);

                SqlParameter ParamMaxPowerPriceAccepted = new SqlParameter();
                ParamMaxPowerPriceAccepted.ParameterName = "@MaxPowerPriceAccepted";
                if (tbMaxPowerPriceAccepted.Text == "")
                    ParamMaxPowerPriceAccepted.Value = DBNull.Value;
                else
                ParamMaxPowerPriceAccepted.Value = tbMaxPowerPriceAccepted.Text;
                cmd.Parameters.Add(ParamMaxPowerPriceAccepted);

                SqlParameter ParamVehicleID = new SqlParameter();
                ParamVehicleID.ParameterName = "@VehicleID";
                if (tbVehicleID.Text == "")
                    ParamVehicleID.Value = DBNull.Value;
                else
                    ParamVehicleID.Value = tbVehicleID.Text;
                cmd.Parameters.Add(ParamVehicleID);

                SqlParameter ParamAggregateControl = new SqlParameter();
                ParamAggregateControl.ParameterName = "@AggregateControl";
                ParamAggregateControl.Value = tbAggregateControl.Text;
                cmd.Parameters.Add(ParamAggregateControl);

                SqlParameter ParamID = new SqlParameter();
                ParamID.ParameterName = "@ID";
                ParamID.Value = gvRow.Cells[findGVcolumn("ID")].Text;
                cmd.Parameters.Add(ParamID);

                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                ShowError("Error while Updating: " + ex.Message);
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

            HideError();
            gvChargingRecord.SelectedIndex = -1;
            populategvChargingRecord(ddlOrganization.SelectedValue , cbShowIsEnd.Checked);
            fillinInfo();
            PopUpMessage("Closed");

        }
        
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;

            SqlCommand cmd; 
            SqlDataReader readerProfile = null;
            GridViewRow gvRow = gvChargingRecord.Rows[gvChargingRecord.SelectedIndex]; // Obtain selected index of gvChargingRecord

            try
            {

                strQuery = "UPDATE [ChargingRecords] SET [EnergyPrice] = @EnergyPrice, [ChargingAlgorithm] = @ChargingAlgorithm, [IsInCharging] = @IsInCharging, [ChargingTimes]=@ChargingTimes, [StartTime] = @StartTime, [StartMainPower] = @StartMainPower, [StartVoltage]=@StartVoltage, [StartCurrent]=@StartCurrent, [StartPF]=@StartPF, [StartActivePower]=@StartActivePower, [StartApparentPower]=@StartApparentPower, [Priority]=@Priority, [IsEnd] = @IsEnd, [EndTime] = @EndTime, [EndVoltage]=@EndVoltage, [EndCurrent]=@EndCurrent, [EndPF]=@EndPF, [EndActivePower]=@EndActivePower, [EndApparentPower]=@EndApparentPower, [EndMainPower] = @EndMainPower, LastStartMainPower=@LastStartMainPower, [LastStartCharging]=@LastStartCharging, [LastStopCharging]=@LastStopCharging, [LastStopMainPower]=@LastStopMainPower, [IsEndedByUser]=@IsEndedByUser, [ScheduleID]=@ScheduleID, [ChargingCost]=@ChargingCost, [TotalCharingTime]=@TotalCharingTime, [SOC]=@SOC, [SOCRetrieveTime]=@SOCRetrieveTime, [CalculateCO2]=@CalculateCO2, [LeaveTime]=@LeaveTime, [OdometerReading]=@OdometerReading, [AggregateControl]=@AggregateControl, "
                    + " [ZipCode] = @ZipCode, [EmailAddress] = @EmailAddress, [MaxPowerRequired] = @MaxPowerRequired, [MaxPowerPriceAccepted] = @MaxPowerPriceAccepted, [VehicleID] = @VehicleID ";

                strQuery += " WHERE [ID] = @ID";
                cmd = new SqlCommand(strQuery, cnn);

                cnn.Open();


                // Normal params.

                SqlParameter ParamEnergyPrice = new SqlParameter();
                ParamEnergyPrice.ParameterName = "@EnergyPrice";
                    ParamEnergyPrice.Value = tbEnergyPrice.Text;
                cmd.Parameters.Add(ParamEnergyPrice);

                SqlParameter ParamChargingAlgorithm = new SqlParameter();
                ParamChargingAlgorithm.ParameterName = "@ChargingAlgorithm";
                    ParamChargingAlgorithm.Value = tbChargingAlgorithm.Text;
                cmd.Parameters.Add(ParamChargingAlgorithm);

                SqlParameter ParamIsInCharging = new SqlParameter();
                ParamIsInCharging.ParameterName = "@IsInCharging";
                ParamIsInCharging.Value = tbIsInCharging.Text;
                cmd.Parameters.Add(ParamIsInCharging);

                SqlParameter ParamChargingTimes = new SqlParameter();
                ParamChargingTimes.ParameterName = "@ChargingTimes";
                ParamChargingTimes.Value = tbChargingTimes.Text;
                cmd.Parameters.Add(ParamChargingTimes);

                SqlParameter ParamStartTime = new SqlParameter();
                ParamStartTime.ParameterName = "@StartTime";
                ParamStartTime.Value = tbStartTime.Text;
                cmd.Parameters.Add(ParamStartTime);

                SqlParameter ParamStartMainPower = new SqlParameter();
                ParamStartMainPower.ParameterName = "@StartMainPower"; 
                ParamStartMainPower.Value = tbStartMainPower.Text;
                cmd.Parameters.Add(ParamStartMainPower);

                SqlParameter ParamStartVoltage = new SqlParameter();
                ParamStartVoltage.ParameterName = "@StartVoltage";
                ParamStartVoltage.Value = tbStartVoltage.Text;
                cmd.Parameters.Add(ParamStartVoltage);

                SqlParameter ParamStartCurrent = new SqlParameter();
                ParamStartCurrent.ParameterName = "@StartCurrent";
                ParamStartCurrent.Value = tbStartCurrent.Text;
                cmd.Parameters.Add(ParamStartCurrent);

                SqlParameter ParamStartPF = new SqlParameter();
                ParamStartPF.ParameterName = "@StartPF";
                ParamStartPF.Value = tbStartPF.Text;
                cmd.Parameters.Add(ParamStartPF);

                SqlParameter ParamStartActivePower = new SqlParameter();
                ParamStartActivePower.ParameterName = "@StartActivePower";
                ParamStartActivePower.Value = tbStartActivePower.Text;
                cmd.Parameters.Add(ParamStartActivePower);

                SqlParameter ParamStartApparentPower = new SqlParameter();
                ParamStartApparentPower.ParameterName = "@StartApparentPower";
                ParamStartApparentPower.Value = tbStartApparentPower.Text;
                cmd.Parameters.Add(ParamStartApparentPower);

                SqlParameter ParamPriority = new SqlParameter();
                ParamPriority.ParameterName = "@Priority";
                ParamPriority.Value = tbPriority.Text;
                cmd.Parameters.Add(ParamPriority);

                SqlParameter ParamIsEnd = new SqlParameter();
                ParamIsEnd.ParameterName = "@IsEnd";
                ParamIsEnd.Value = tbIsEnd.Text;
                cmd.Parameters.Add(ParamIsEnd);

                SqlParameter ParamEndTime = new SqlParameter();
                ParamEndTime.ParameterName = "@EndTime";
                if (tbEndTime.Text == "")
                    ParamEndTime.Value = DBNull.Value;
                else
                ParamEndTime.Value = tbEndTime.Text;
                cmd.Parameters.Add(ParamEndTime);

                SqlParameter ParamEndVoltage = new SqlParameter();
                ParamEndVoltage.ParameterName = "@EndVoltage";
                if (tbEndVoltage.Text == "")
                    ParamEndVoltage.Value = DBNull.Value;
                else
                ParamEndVoltage.Value = tbEndVoltage.Text;
                cmd.Parameters.Add(ParamEndVoltage);

                SqlParameter ParamEndCurrent = new SqlParameter();
                ParamEndCurrent.ParameterName = "@EndCurrent";
                if (tbEndCurrent.Text == "")
                    ParamEndCurrent.Value = DBNull.Value;
                else
                ParamEndCurrent.Value = tbEndCurrent.Text;
                cmd.Parameters.Add(ParamEndCurrent);

                SqlParameter ParamEndPF = new SqlParameter();
                ParamEndPF.ParameterName = "@EndPF";
                if (tbEndPF.Text == "")
                    ParamEndPF.Value = DBNull.Value;
                else
                    ParamEndPF.Value = tbEndPF.Text;
                cmd.Parameters.Add(ParamEndPF);

                SqlParameter ParamEndActivePower = new SqlParameter();
                ParamEndActivePower.ParameterName = "@EndActivePower";
                if (tbEndActivePower.Text == "")
                    ParamEndActivePower.Value = DBNull.Value;
                else
                ParamEndActivePower.Value = tbEndActivePower.Text;
                cmd.Parameters.Add(ParamEndActivePower);

                SqlParameter ParamEndApparentPower = new SqlParameter();
                ParamEndApparentPower.ParameterName = "@EndApparentPower";
                if (tbEndApparentPower.Text == "")
                    ParamEndApparentPower.Value = DBNull.Value;
                else
                    ParamEndApparentPower.Value = tbEndApparentPower.Text;
                cmd.Parameters.Add(ParamEndApparentPower);

                SqlParameter ParamEndMainPower = new SqlParameter();
                ParamEndMainPower.ParameterName = "@EndMainPower";
                if (tbEndMainPower.Text == "")
                    ParamEndMainPower.Value = DBNull.Value;
                else
                    ParamEndMainPower.Value = tbEndMainPower.Text;
                cmd.Parameters.Add(ParamEndMainPower);

                SqlParameter ParamLastStartMainPower = new SqlParameter();
                ParamLastStartMainPower.ParameterName = "@LastStartMainPower";
                if (tbLastStartMainPower.Text == "")
                    ParamLastStartMainPower.Value = DBNull.Value;
                else
                    ParamLastStartMainPower.Value = tbLastStartMainPower.Text;
                cmd.Parameters.Add(ParamLastStartMainPower);

                SqlParameter ParamLastStartCharging = new SqlParameter();
                ParamLastStartCharging.ParameterName = "@LastStartCharging";
                if (tbLastStartCharging.Text == "")
                    ParamLastStartCharging.Value = DBNull.Value;
                else
                    ParamLastStartCharging.Value = tbLastStartCharging.Text;
                cmd.Parameters.Add(ParamLastStartCharging);

                SqlParameter ParamLastStopCharging = new SqlParameter();
                ParamLastStopCharging.ParameterName = "@LastStopCharging";
                if (tbLastStopCharging.Text == "")
                    ParamLastStopCharging.Value = DBNull.Value;
                else
                    ParamLastStopCharging.Value = tbLastStopCharging.Text;
                cmd.Parameters.Add(ParamLastStopCharging);

                SqlParameter ParamLastStopMainPower = new SqlParameter();
                ParamLastStopMainPower.ParameterName = "@LastStopMainPower";
                if (tbLastStopMainPower.Text == "")
                    ParamLastStopMainPower.Value = DBNull.Value;
                else
                    ParamLastStopMainPower.Value = tbLastStopMainPower.Text;
                cmd.Parameters.Add(ParamLastStopMainPower);

                SqlParameter ParamIsEndedByUser = new SqlParameter();
                ParamIsEndedByUser.ParameterName = "@IsEndedByUser";
                ParamIsEndedByUser.Value = tbIsEndedByUser.Text;
                cmd.Parameters.Add(ParamIsEndedByUser);

                SqlParameter ParamScheduleID = new SqlParameter();
                ParamScheduleID.ParameterName = "@ScheduleID";
                if (tbScheduleID.Text == "")
                    ParamScheduleID.Value = DBNull.Value;
                else
                    ParamScheduleID.Value = tbScheduleID.Text;
                cmd.Parameters.Add(ParamScheduleID);

                SqlParameter ParamChargingCost = new SqlParameter();
                ParamChargingCost.ParameterName = "@ChargingCost";
                ParamChargingCost.Value = tbChargingCost.Text;
                cmd.Parameters.Add(ParamChargingCost);

                SqlParameter ParamTotalCharingTime = new SqlParameter();
                ParamTotalCharingTime.ParameterName = "@TotalCharingTime";
                ParamTotalCharingTime.Value = tbTotalChargingTime.Text;
                cmd.Parameters.Add(ParamTotalCharingTime);

                SqlParameter ParamSOC = new SqlParameter();
                ParamSOC.ParameterName = "@SOC";
                if (tbSOC.Text == "")
                    ParamSOC.Value = DBNull.Value;
                else
                    ParamSOC.Value = tbSOC.Text;
                cmd.Parameters.Add(ParamSOC);

                SqlParameter ParamSOCRetrieveTime = new SqlParameter();
                ParamSOCRetrieveTime.ParameterName = "@SOCRetrieveTime";
                if (tbSOCRetrieveTime.Text == "")
                    ParamSOCRetrieveTime.Value = DBNull.Value;
                else
                    ParamSOCRetrieveTime.Value = tbSOCRetrieveTime.Text;
                cmd.Parameters.Add(ParamSOCRetrieveTime);

                SqlParameter ParamCalculateCO2 = new SqlParameter();
                ParamCalculateCO2.ParameterName = "@CalculateCO2";
                ParamCalculateCO2.Value = tbCalculateCO2.Text;
                cmd.Parameters.Add(ParamCalculateCO2);

                SqlParameter ParamZipCode = new SqlParameter();
                ParamZipCode.ParameterName = "@ZipCode";
                if (tbZipCode.Text == "")
                    ParamZipCode.Value = DBNull.Value;
                else
                    ParamZipCode.Value = tbZipCode.Text;
                cmd.Parameters.Add(ParamZipCode);

                SqlParameter ParamEmailAddress = new SqlParameter();
                ParamEmailAddress.ParameterName = "@EmailAddress";
                if (tbEmailAddress.Text == "")
                    ParamEmailAddress.Value = DBNull.Value;
                else
                    ParamEmailAddress.Value = tbEmailAddress.Text;
                cmd.Parameters.Add(ParamEmailAddress);

                SqlParameter ParamLeaveTime = new SqlParameter();
                ParamLeaveTime.ParameterName = "@LeaveTime";
                if (tbLeaveTime.Text == "")
                    ParamLeaveTime.Value = DBNull.Value;
                else
                    ParamLeaveTime.Value = tbLeaveTime.Text;
                cmd.Parameters.Add(ParamLeaveTime);

                SqlParameter ParamOdometerReading = new SqlParameter();
                ParamOdometerReading.ParameterName = "@OdometerReading";
                if (tbOdometerReading.Text == "")
                    ParamOdometerReading.Value = DBNull.Value;
                else
                    ParamOdometerReading.Value = tbOdometerReading.Text;
                cmd.Parameters.Add(ParamOdometerReading);

                SqlParameter ParamMaxPowerRequired = new SqlParameter();
                ParamMaxPowerRequired.ParameterName = "@MaxPowerRequired";
                if (tbMaxPowerRequired.Text == "")
                    ParamMaxPowerRequired.Value = DBNull.Value;
                else
                    ParamMaxPowerRequired.Value = tbMaxPowerRequired.Text;
                cmd.Parameters.Add(ParamMaxPowerRequired);

                SqlParameter ParamMaxPowerPriceAccepted = new SqlParameter();
                ParamMaxPowerPriceAccepted.ParameterName = "@MaxPowerPriceAccepted";
                if (tbMaxPowerPriceAccepted.Text == "")
                    ParamMaxPowerPriceAccepted.Value = DBNull.Value;
                else
                ParamMaxPowerPriceAccepted.Value = tbMaxPowerPriceAccepted.Text;
                cmd.Parameters.Add(ParamMaxPowerPriceAccepted);

                SqlParameter ParamVehicleID = new SqlParameter();
                ParamVehicleID.ParameterName = "@VehicleID";
                if (tbVehicleID.Text == "")
                    ParamVehicleID.Value = DBNull.Value;
                else
                    ParamVehicleID.Value = tbVehicleID.Text;
                cmd.Parameters.Add(ParamVehicleID);

                SqlParameter ParamAggregateControl = new SqlParameter();
                ParamAggregateControl.ParameterName = "@AggregateControl";
                ParamAggregateControl.Value = tbAggregateControl.Text;
                cmd.Parameters.Add(ParamAggregateControl);

                SqlParameter ParamID = new SqlParameter();
                ParamID.ParameterName = "@ID";
                ParamID.Value = gvRow.Cells[findGVcolumn("ID")].Text;
                cmd.Parameters.Add(ParamID);

                readerProfile = cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                ShowError("Error while Updating: " + ex.Message);
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

            HideError();
            gvChargingRecord.SelectedIndex = -1;
            populategvChargingRecord(ddlOrganization.SelectedValue , cbShowIsEnd.Checked);
            fillinInfo();
            PopUpMessage("Updated");

        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            gvChargingRecord.SelectedIndex = -1;
            ClearAllTbs();
            HideError();
        }
        
    }
}