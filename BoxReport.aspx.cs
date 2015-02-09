using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Security;
using System.Security.Permissions;
using System.Drawing;

namespace RTMC
{
    public partial class BoxReport : System.Web.UI.Page
    {
        protected DateTime dtBefore;
        protected DateTime dtNow;
        protected DateTime dtEndTime;
        protected string strCnn;
        protected string strGatewayID;
        protected string strGatewayName;
        private string strQuery;
        SqlConnection sc = null;
        SqlCommand cmd = null;
        SqlDataAdapter sa;
        DataTable dtStations;
        SqlDataReader sdr = null;
        bool blnUseStationName = true;
        protected string strPlAddress = "";
        private double dblMaxEnergyPM = 2;

        protected string strGetStationIDQuery(string strStationID, string strTableName)
        {
            string strLastPart = " = \'" + strStationID + "\' ";
            if (strTableName.Equals("Station Record"))
            {
                if (blnUseStationName == true)
                {
                    return "StationName" + strLastPart;
                }
                else
                {
                    return "[Station ID]" + strLastPart;
                }
            }
            else if (strTableName.Equals("ChargingRecords"))
            {
                if (blnUseStationName == true)
                {
                    return "StationName" + strLastPart;
                }
                else
                {
                    return "StationID" + strLastPart;
                }
            }
            else if (strTableName.Equals("Station"))
            {
                if (blnUseStationName == true)
                {
                    return "Name" + strLastPart;
                }
                else
                {
                    return "ID" + strLastPart;
                }
            }
            return "";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //For back to previous page
                //this.txtRequestUri.Text = this.Page.Request.UrlReferrer.AbsolutePath.ToString();
                dtNow = DateTime.Now;
                tbBeginingTime.Text = dtNow.ToString("MM/dd/yyyy") + " 00:00:00";
                tbEndTime.Text = dtNow.ToString("MM/dd/yyyy HH:mm:ss");

                strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                strGatewayID = Request.QueryString["ID"];
                strGatewayName = Request.QueryString["GatewayName"];
                strQuery = "SELECT pl.Name + ' Level ' + g.[Level] AS ParkingLot, pl.Address + ', ' + c.Name + ', ' + c.State + pl.[Zip Code] AS Address " +
                           "FROM  Gateway AS g INNER JOIN [Parking Lot] AS pl ON g.[Parking Lot ID] = pl.ID INNER JOIN City AS c ON pl.[City ID] = c.ID " +
                           "WHERE g.ID =@GatewayID";
                try
                {
                    sc = new SqlConnection(strCnn);
                    sc.Open();
                    cmd = new SqlCommand(strQuery, sc);
                    cmd.Parameters.AddWithValue("@GatewayID", strGatewayID);
                    cmd.CommandType = CommandType.Text;
                    sdr = cmd.ExecuteReader();
                    if (sdr.Read())
                    {
                        lblStation.Text = "Report of Charging Box " + strGatewayName + " at ";
                        strPlAddress = sdr["ParkingLot"].ToString() + " " + sdr["Address"];
                        lblPL.Text = strPlAddress;                        
                    }
                    else
                        return;

                }
                catch (Exception ex)
                {
                    return;
                }
                finally
                {
                    if (sdr != null)
                        sdr.Close();
                    sc.Close();
                    cmd.Dispose();
                }
            }
        }

        protected void btnChartGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                strGatewayName = Request["GatewayName"];
            } catch (Exception e1) 
            {
                lblMessage.Text = "No box specified";
                return;
            }
            dtBefore = Convert.ToDateTime(tbBeginingTime.Text);
            dtNow = Convert.ToDateTime(tbEndTime.Text);
            string strErrMsg = "";
            if (dtNow.CompareTo(dtBefore) <= 0)
                strErrMsg += " Beginning time can't later than end time.";
            TimeSpan ts = dtNow - dtBefore;
            if (ts.TotalDays > 365.0)
                strErrMsg += " Beginning time and end time span can't more than 365 days";

            if (strErrMsg != "") {
                lblMessage.Text = strErrMsg;
                rvReport.Visible = false;
                return;
            }
            try
            {

                DataTable dtChargingList = getChargingRecords(tbFilterInvalidData.Checked);

                if (dtChargingList.Rows.Count == 0) {
                    lblMessage.Text = "No Data";

                    rvReport.Visible = false;
                    return;
                }
                List<String> colNames = new List<String> { "SpaceNo", "UserName", "StartTime", "EndTime", "IsEnd", "IsEndedByUser", "ChargingTimes",
                                            "EnergyPrice", "EnergyConsumed", "ChargingCost"};
                DataView dv = dtChargingList.DefaultView;
                dv.Sort = colNames[ddlSortBy.SelectedIndex] + " " + ddlDirection.SelectedValue;
                DataTable dtSortedCL = dv.ToTable();

                rvReport.LocalReport.DataSources.Clear();
                rvReport.LocalReport.DataSources.Add(new
                        Microsoft.Reporting.WebForms.ReportDataSource("dsCR", dtChargingList));
                rvReport.LocalReport.DataSources.Add(new
                        Microsoft.Reporting.WebForms.ReportDataSource("dsSortedCR", dtSortedCL));
                rvReport.DataBind();


                ReportParameter rpHeader = new ReportParameter("rpHeader", strGatewayName);
                ReportParameter rpMiddle = new ReportParameter("rpMiddle", lblPL.Text);
                string strBase = "Charging box report between " + tbBeginingTime.Text + " and " + tbEndTime.Text + " sorted by " +
                                    ddlSortBy.SelectedValue + " in " + ddlDirection.SelectedValue + " order";
                ReportParameter rpBase = new ReportParameter("rpBase", strBase);
                
                //ReportParameter rpOrder = new ReportParameter("rpOrder", colNames[ddlSortBy.SelectedIndex]);
                this.rvReport.LocalReport.SetParameters(new ReportParameter[] { rpHeader, rpBase, rpMiddle});
           


                rvReport.LocalReport.Refresh();

                rvReport.Visible = true;
                lblMessage.Text = "";
            } catch (Exception e1)
            {
                ;
            }
        }

        protected bool bIsValidEnergyPoint(DateTime dtPreTime,
                                double dbPreEnergy,
                                DateTime dtCurTime,
                                double dbCurEnergy,
                                double dbVoltage,
                                double dbCurrent)
        {
            double dbPowerErrorPercentage = 1.05;
            double dbPowerErrorValue = 0.05;
            double dbUpperbound = dbPreEnergy + dbVoltage * dbCurrent *
                                dbPowerErrorPercentage *
                                dtCurTime.Subtract(dtPreTime).TotalSeconds / 3600000 +
                                dbPowerErrorValue;
            return (dbCurEnergy >= dbPreEnergy) &&
                    (dbCurEnergy <= dbUpperbound);
        }

        // Given a time time and the stationID, returns the latest valid main energy 
        // of the station before the time
        public double dbGetCorrectEndMainEnergy(DateTime dtTime, String strStationID)
        {

            string strCnn = System.Web.Configuration.WebConfigurationManager.
                            ConnectionStrings["ApplicationServices"].ConnectionString;
            DataTable dt = new DataTable();
            String query = "select [TimeStamp], [Main Energy] from " +
                "[Station Record] where [Is Successful] = 1 " +
                "and " + strGetStationIDQuery(strStationID, "Station Record") +
                "and timestamp >\'" + dtTime.AddMinutes(-20).ToString() + "\' " +
                "and timestamp <\'" + dtTime.AddMinutes(20).ToString() + "\' ";
            SqlDataAdapter sdr = new SqlDataAdapter(query, strCnn);
            sdr.Fill(dt);
            List<DateTime> timeList = new List<DateTime>();
            List<double> energyList = new List<double>();
            foreach (DataRow dr in dt.Rows)
            {
                timeList.Add((DateTime)dr["TimeStamp"]);
                energyList.Add((double)dr["Main Energy"]);
            }
            List<GraphDataPoint> dps = getValidEnergyPoints(timeList, energyList);
            int index = 0;
            for (int i = 0; i < dps.Count; i++)
            {
                if (dps[i].x <= dtTime)
                    index = i;
                else
                    break;
            }
            if (dps.Count > 0)
                return dps[index].y;
            else
                return -1;
            // find the record in dps which is closest to dtTime
        }
        private List<GraphDataPoint> getValidEnergyPoints(List<DateTime> timeList, List<double> rawEnergyList)
        {
            double firstEnergy = -1;
            DateTime firstTimeStamp = DateTime.Parse("1990-01-01 00:00");
            double secondEnergy = -1;
            DateTime secondTimeStamp = DateTime.Parse("1990-01-01 00:00");
            double thirdEnergy = -1;
            DateTime thirdTimeStamp = DateTime.Parse("1990-01-01 00:00");
            DateTime preTime = DateTime.Parse("1990-01-01 00:00");
            double energyMaxLocal = -1;
            double energyMinLocal = 999999;
            List<GraphDataPoint> energyPoints = new List<GraphDataPoint>();
            bool firstAdded = false;

            for (int i = 0; i < rawEnergyList.Count; i++)
            {
                double energy = rawEnergyList[i];
                DateTime curTime = timeList[i];
                if (firstEnergy < 0)
                {
                    firstEnergy = energy;
                    firstTimeStamp = curTime;
                }
                else if (secondEnergy < 0)
                {
                    secondEnergy = energy;
                    secondTimeStamp = curTime;
                }
                else if (thirdEnergy < 0)
                {
                    thirdEnergy = energy;
                    thirdTimeStamp = curTime;
                }
                else
                {
                    if (firstAdded == false)
                    {
                        if (firstEnergy > secondEnergy && secondEnergy > energy)
                        {
                            firstEnergy = secondEnergy;
                            firstTimeStamp = secondTimeStamp;
                            secondEnergy = thirdEnergy;
                            secondTimeStamp = thirdTimeStamp;
                            thirdEnergy = -1;
                        }
                        else if ((firstEnergy == secondEnergy && secondEnergy > energy) ||
                                  (firstEnergy < secondEnergy && firstEnergy > energy) ||
                                  (firstEnergy == secondEnergy && secondEnergy < energy))
                        {
                            if (withinTimeRange(firstTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(firstTimeStamp, firstEnergy));
                                energyMaxLocal = firstEnergy;
                                energyMinLocal = firstEnergy;
                            }
                            firstAdded = true;
                        }
                        else if ((firstEnergy < secondEnergy && secondEnergy > energy && firstEnergy <= energy) ||
                                  (firstEnergy > secondEnergy && secondEnergy < energy && firstEnergy < energy))
                        {
                            if (withinTimeRange(firstTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(firstTimeStamp, firstEnergy));
                                energyMaxLocal = firstEnergy;
                                energyMinLocal = firstEnergy;
                            }
                            firstAdded = true;
                            secondEnergy = thirdEnergy;
                            secondTimeStamp = thirdTimeStamp;
                            thirdEnergy = -1;
                        }
                        else if ((firstEnergy > secondEnergy && secondEnergy == energy) ||
                                  (firstEnergy > secondEnergy && secondEnergy < energy && firstEnergy >= energy))
                        {
                            if (withinTimeRange(secondTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(secondTimeStamp, secondEnergy));
                                energyMaxLocal = secondEnergy;
                                energyMinLocal = secondEnergy;
                            }
                            firstAdded = true;
                            firstEnergy = secondEnergy;
                            firstTimeStamp = secondTimeStamp;
                            secondEnergy = thirdEnergy;
                            secondTimeStamp = thirdTimeStamp;
                            thirdEnergy = -1;
                        }
                        else if (firstEnergy == secondEnergy && secondEnergy == energy)
                        {
                            if (withinTimeRange(firstTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(firstTimeStamp, firstEnergy));
                                energyMaxLocal = firstEnergy;
                                energyMinLocal = firstEnergy;
                            }
                            firstAdded = true;
                        }
                        else if (firstEnergy < secondEnergy && secondEnergy < energy)
                        {
                            if (withinTimeRange(secondTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(secondTimeStamp, secondEnergy));
                                energyMaxLocal = secondEnergy;
                                energyMinLocal = secondEnergy;
                            }
                            firstAdded = true;
                            firstEnergy = secondEnergy;
                            firstTimeStamp = secondTimeStamp;
                            secondEnergy = thirdEnergy;
                            secondTimeStamp = thirdTimeStamp;
                            thirdEnergy = -1;
                        }
                    }
                    else
                    {
                        if ((firstEnergy == secondEnergy) ||
                            (firstEnergy <= secondEnergy && secondEnergy <= thirdEnergy) ||
                            (firstEnergy < secondEnergy && secondEnergy > thirdEnergy && firstEnergy > thirdEnergy))
                        {
                            if (withinTimeRange(secondTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(secondTimeStamp, secondEnergy));
                                if (secondEnergy > energyMaxLocal)
                                    energyMaxLocal = secondEnergy;
                                if (secondEnergy < energyMinLocal)
                                    energyMinLocal = secondEnergy;
                            }
                            firstEnergy = secondEnergy;
                            firstTimeStamp = secondTimeStamp;
                            secondEnergy = thirdEnergy;
                            secondTimeStamp = thirdTimeStamp;
                            thirdEnergy = -1;
                        }
                        else if ((firstEnergy < secondEnergy && secondEnergy > thirdEnergy && thirdEnergy >= firstEnergy) ||
                                  (firstEnergy > secondEnergy && secondEnergy < thirdEnergy && thirdEnergy >= firstEnergy))
                        {
                            if (withinTimeRange(thirdTimeStamp))
                            {
                                energyPoints.Add(new GraphDataPoint(thirdTimeStamp, thirdEnergy));
                                if (thirdEnergy > energyMaxLocal)
                                    energyMaxLocal = thirdEnergy;
                                if (thirdEnergy < energyMinLocal)
                                    energyMinLocal = thirdEnergy;
                            }
                            firstEnergy = thirdEnergy;
                            firstTimeStamp = thirdTimeStamp;
                            secondEnergy = -1;
                            thirdEnergy = -1;
                        }
                        else
                        {
                            secondEnergy = -1;
                            thirdEnergy = -1;
                        }
                    }
                }
            }
            return energyPoints;
        }

        protected bool withinTimeRange(DateTime dtTime)
        {
            return dtTime >= dtBefore && dtTime <= dtNow;
        }

        protected DataTable getChargingRecords(bool filterInvalid)
        {
            strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            strGatewayID = Request.QueryString["ID"];
            SqlConnection sc = null;
            SqlCommand cmd = null;
            SqlDataAdapter sa;
            SqlDataReader sdr;
            string strQuery = "";
            DataTable dtChargingList = new DataTable();
            try
            {
                sc = new SqlConnection(strCnn);
                sc.Open();                
                strQuery = "SELECT s.[Space No] as SpaceNo, u.UserName, cr.StartTime, cr.EndTime,cr.IsEnd,cr.IsEndedByUser,cr.EnergyPrice, cr.StartMainPower, cr.ChargingTimes, cr.EndMainPower, s.ID, s.Name, g.Name as GatewayName, " +
                                      "cast ((case " +
                                      "when patindex('%[a-z|A-Z]%', reverse([Space No])) = 0 then [Space No] " +
                                      "else right([Space No], patindex('%[a-z|A-Z]%', reverse([Space No])) - 1) " +
                                      "end) as int) as NSpaceNo " +                    
                    "FROM  [station record] as sr inner join " +
                                "station as s on sr.[station id] = s.id inner join " +
                                "gateway as g on s.[gateway id] = g.id inner join " +
                                "chargingrecords as cr on sr.chargingid = cr.id inner join " +
                                "aspnet_users as u on cr.userid = u.userid " +
                            "WHERE cr.StartTime > '" + dtBefore + "' and cr.StartTime < '" + dtNow + "' " +
                            "and s.ID in (select Station.ID from Station inner join Gateway on Station.[Gateway ID] = Gateway.ID where Gateway.ID = " + strGatewayID + ") " +
                            "GROUP BY sr.ChargingID, g.[Level], s.[Space No], u.UserName, cr.StartTime, cr.EndTime, cr.EnergyPrice, cr.StartMainPower, cr.ChargingTimes, cr.IsEnd, cr.EndMainPower, s.ID, cr.IsEndedByUser, s.Name, g.Name " +
                            "ORDER by NSpaceNo, s.[space no],cr.starttime, cr.endtime";

                cmd = new SqlCommand(strQuery, sc);
                sa = new SqlDataAdapter(cmd);
                
                sa.Fill(dtChargingList);
                dtChargingList.Columns.Add("EnergyConsumed", typeof(double));
                dtChargingList.Columns.Add("ChargingCost", typeof(double));
                double dblPowerConsumed = 0;
                decimal dclChargingCost;
                foreach (DataRow dr in dtChargingList.Rows)
                {
                    if (dr["IsEnd"].ToString() == "True")
                    {
                        dblPowerConsumed = double.Parse(dr["EndMainPower"].ToString()) - double.Parse(dr["StartMainPower"].ToString());

                        // Invalid data.
                        if ((dblPowerConsumed < 0 && filterInvalid == true) ||
                            (!dr.IsNull("EndTime") &&
                            dblPowerConsumed > dblMaxEnergyPM * ((DateTime)dr["EndTime"]).Subtract((DateTime)dr["StartTime"]).TotalMinutes && filterInvalid == true))
                        {
                            DateTime dtCurTime = (DateTime)dr["EndTime"];
                            String strStationID = (String)dr["ID"];
                            if (blnUseStationName)
                                strStationID = (String)dr["Name"];
                            double dbEndMainPower = dbGetCorrectEndMainEnergy(dtCurTime, strStationID);
                            dblPowerConsumed = Math.Round(dbEndMainPower - double.Parse(dr["StartMainPower"].ToString()), 4);
                            if (dblPowerConsumed < 0 || dbEndMainPower < 0)
                                dblPowerConsumed = 0;
                        }


                        dr["EnergyConsumed"] = dblPowerConsumed;
                        dclChargingCost = (Convert.ToDecimal(dblPowerConsumed) * decimal.Parse(dr["EnergyPrice"].ToString())) / 100.0m;
                        dr["ChargingCost"] = Convert.ToDouble(dclChargingCost);
                    }
                    else
                    {
                        //Added on 4/30/2012 to fix bugs
                        double dblStartMainPower = double.Parse(dr["StartMainPower"].ToString());
                        string strStationID = dr["ID"].ToString();
                        if (blnUseStationName)
                            strStationID = (String)dr["Name"];
                        strQuery = "SELECT [Main Energy] " +
                                   "FROM  [Station Record] " +
                                   "WHERE Timestamp = (SELECT MAX(Timestamp) AS LastRecordTime FROM   [Station Record] AS sr " +
                                   "WHERE " + strGetStationIDQuery(strStationID, "Station Record") + ")";
                        cmd.Dispose();
                        cmd = new SqlCommand(strQuery, sc);
                        object obj = cmd.ExecuteScalar();
                        if (obj != null)
                        {
                            double dblEndMainPower = double.Parse(obj.ToString());
                            dblPowerConsumed = dblEndMainPower - dblStartMainPower;
                            if (dblPowerConsumed >= 0.0)
                            {
                                dr["EnergyConsumed"] = dblPowerConsumed;
                                dclChargingCost = (Convert.ToDecimal(dblPowerConsumed) * decimal.Parse(dr["EnergyPrice"].ToString())) / 100.0m;
                                dr["ChargingCost"] = Convert.ToDouble(dclChargingCost);
                            }
                            else
                            {
                                if (filterInvalid == true)
                                {
                                    // Invalid data.
                                    if (dr.IsNull("EndTime"))
                                    {
                                        dblPowerConsumed = 0;
                                    }
                                    else
                                    {
                                        DateTime dtCurTime = (DateTime)dr["EndTime"];
                                        String strID = (String)dr["ID"];
                                        double dbEndMainPower = dbGetCorrectEndMainEnergy(dtCurTime, strID);
                                        dblPowerConsumed = Math.Round(dbEndMainPower - double.Parse(dr["StartMainPower"].ToString()), 4);
                                        if (dblPowerConsumed < 0 || dbEndMainPower < 0)
                                            dblPowerConsumed = 0;
                                    }
                                }
 
                                dr["EnergyConsumed"] = dblPowerConsumed;
                                dclChargingCost = (Convert.ToDecimal(dblPowerConsumed) * decimal.Parse(dr["EnergyPrice"].ToString())) / 100.0m;
                                dr["ChargingCost"] = Convert.ToDouble(dclChargingCost);


                                //dr["Energy Consumed"] = "0kWh";
                                //dr["Charging Cost"] = "$0.00";
                            }
                        }
                        else
                        {
                            dr["EnergyConsumed"] = 0;
                            dr["ChargingCost"] = 0.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return dtChargingList;
        }
    }
}