using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

using System.Web.Security;

namespace RTMC
{
    public partial class _Default : System.Web.UI.Page
    {
        private string strCnn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        private string strQuery;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    //strQuery = "SELECT u.UserName as [User Name: ], m.CreateDate as [Created Time(GMT): ], m.LastLoginDate as [Last Login Time (GMT): ], m.LastPasswordChangedDate as [Last Password Change Time(GMT): ], m.LastLockoutDate as [Last Lockout Time: ], m.FailedPasswordAttemptCount as [Failed Attempt Count: ] " +
                    strQuery = "SELECT u.UserName as [User Name: ], m.LastLoginDate as [Login Time: ],m.CreateDate as [Created Time: ], m.LastPasswordChangedDate as [Last Password Change Time: ], m.LastLockoutDate as [Last Lockout Time: ], m.FailedPasswordAttemptCount as [Failed Attempt Count: ] " +
                               "FROM  aspnet_Membership AS m INNER JOIN aspnet_Users AS u ON m.UserId = u.UserId " +
                               "WHERE u.UserName = @Username";
                    SqlConnection sc = null;
                    SqlCommand cmd = null;
                    SqlDataAdapter sa;
                    DataTable dtAccountInfo = new DataTable();

                    try
                    {
                        sc = new SqlConnection(strCnn);
                        sc.Open();
                        cmd = new SqlCommand(strQuery, sc);
                        cmd.Parameters.AddWithValue("@Username", User.Identity.Name);
                        cmd.CommandType = CommandType.Text;
                        sa = new SqlDataAdapter(cmd);
                        dtAccountInfo = new DataTable();
                        sa.Fill(dtAccountInfo);

                        DataTable dt = new DataTable();
                        dt.Columns.Add("Title", typeof(string));
                        dt.Columns.Add("Content", typeof(string));
                        DataRow dr;
                        foreach (DataColumn dc in dtAccountInfo.Columns)
                        {
                            dr = dt.NewRow();
                            dr["Title"] = dc.ColumnName;
                            if (dc.ColumnName == "Last Lockout Time: ")
                            {
                                DateTime dtLockoutDate = DateTime.Parse(dtAccountInfo.Rows[0][dc.ColumnName].ToString());
                                if (dtLockoutDate.Year < 2010)
                                    dr["Content"] = "No";
                                else
                                    dr["Content"] = ((Convert.ToDateTime(dtAccountInfo.Rows[0][dc.ColumnName])).AddHours(-7)).ToString("MM/dd/yyyy HH:mm:ss");
                            }
                            else if (dc.ColumnName == "Created Time: " || dc.ColumnName == "Last Password Change Time: " || dc.ColumnName == "Login Time: ")
                            {
                                dr["Content"] = ((Convert.ToDateTime(dtAccountInfo.Rows[0][dc.ColumnName])).AddHours(-7)).ToString("MM/dd/yyyy HH:mm:ss");
                            }
                            else
                                dr["Content"] = dtAccountInfo.Rows[0][dc.ColumnName];
                            dt.Rows.Add(dr);
                        }

                        RolePrincipal rp = (RolePrincipal)User;
                        string[] roles = Roles.GetRolesForUser();

                        string str = "";
                        for (int i = 0; i < roles.Length; i++)
                        {
                            switch (roles[i])
                            {
                                case "General Administrator":
                                    str += "Monitor and control all stations in all cities and can charge an EV at any station in the system";
                                    break;
                                case "General Maintainer":
                                    str += "Monitor all stations in all cities but can charge an EV";
                                    break;
                                case "General Operator":
                                    str += "Monitor all stations in all cities and can charge an EV at any station in the system";
                                    break;
                                default:
                                    string[] strs = new string[2];
                                    int intIndex = roles[i].LastIndexOf(' ');
                                    strs[0] = roles[i].Substring(0, intIndex);
                                    strs[1] = roles[i].Substring(intIndex + 1);
                                    //str += roles[i] + ": control all stations";
                                    switch (strs[1])
                                    {
                                        case "Administrator":
                                            str += (i + 1) + ". Monitor and control all stations in " + strs[0] + " and can charge an EV at any station in " + strs[0];
                                            break;
                                        case "Maintainer":
                                            str += (i + 1) + ". Monitor all stations in " + strs[0] + " but can charge an EV";
                                            break;
                                        case "Operator":
                                            str += (i + 1) + ". Monitor all stations in " + strs[0] + " and can charge an EV at any station in " + strs[0];
                                            break;
                                    }
                                    break;

                            }

                            if (i != roles.Length - 1)
                                str += "<br />";
                        }
                        if (roles.Length > 1)
                            str += "<br />It is a combination type";
                        dr = dt.NewRow();
                        dr["Title"] = "Account Type: ";
                        dr["Content"] =  str;
                        dt.Rows.Add(dr);

                        gvAccountInfo.DataSource = dt;
                        gvAccountInfo.DataBind();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        sc.Close();
                        cmd.Dispose();
                    }

                }
            }
        }

        protected void gvAccountInfo_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string decodedText = HttpUtility.HtmlDecode(e.Row.Cells[1].Text);
                e.Row.Cells[1].Text = decodedText;
            }
        }
    }
}
