using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using RTMC;
namespace EVEditor
{
    public partial class EditEVMaker : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        // Leave this string blank if you want to allow general TYPES to access the page.
        // But if specific ROLES are desired, then fill the string.
        // For example, if the specific role of "UCLA Administrator" is allowed, but
        // "Pasadena Administrator" is not allowed, then put in "UCLA Administrator below"
        // string[] strArrRolesToAllow = {"UCLA Administrator", "General Administrator" };

        string[] strArrRolesToAllow = { "General Administrator"};
        //  string[] strArrRolesToAllow = { };
        // strArrAllowedTypes are the role types that are allowed.  This is used
        // to help facilitate the page load setup
        string[] strArrTypesToAllow = { "Administrator" };

        // strArrMasterOrgs are the organizations that are allowed full access
        // to all other organizations.
        string[] strArrMasterOrgs = { "General" };

        // listApprovedRoles holds the user's approved roles for this page.
        List<string> listApprovedRoles = new List<string>();

        string[] ColumnsToHide = {};

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
                populateEVMaker();
                
            }
        }

        protected void populateEVMaker()
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview
            {
                string sqlQuery = "SELECT [MakerName] " +
                                    "FROM [EV Maker] ";

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
                            ShowError("Error at populategvFleet: " + ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopUpMessage for clarity
                        {
                            ShowError("No data in this selection");
                        }
                    }
                }

                Session["data"] = DT;
                gvMaker.DataSource = Session["data"];
                gvMaker.DataBind();

            }
        }

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
        {
            lblCatchError.Visible = true;
            lblCatchError.Text = Message;
            btnHideCatchError.Visible = true;
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
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

        protected void gvMakerPaging(object sender, GridViewPageEventArgs e)
        {
            gvMaker.SelectedIndex = -1;

            DataTable dataTable = Session["data"] as DataTable;

            gvMaker.PageIndex = e.NewPageIndex;
            gvMaker.DataSource = dataTable;
            gvMaker.DataBind();
        }

        protected void gvMakerSelectedIndex(object sender, EventArgs e)
        {
            fillInfo();
            //HideError();

            btnUpdate.Visible = true;

        }

        protected void fillInfo()
        {
            GridViewRow gvRow;
            gvRow = gvMaker.Rows[gvMaker.SelectedIndex];


            tbEVMaker.Text = gvRow.Cells[1].Text;
            //findGVcolumn("MakerName")
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvMaker.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvMaker.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void gvMakerRowCreated(object sender, GridViewRowEventArgs e)
        {
            //for (int i = 0; i < ColumnsToHide.Count(); i++)
            //{
            //    if (e.Row.RowType == DataControlRowType.Header)
            //    {
            //        e.Row.Cells[findGVcolumn(ColumnsToHide[i])].Visible = false;
            //    }
            //    if (e.Row.RowType == DataControlRowType.DataRow)
            //    {
            //        e.Row.Cells[findGVcolumn(ColumnsToHide[i])].Visible = false;
            //    }
            //}
        }

        protected void gvMakerSorting(object sender, GridViewSortEventArgs e)
        {
            //DataTable dataTable = Session["data"] as DataTable;
            //if (dataTable != null)
            //{
            //    DataView dataView = new DataView(dataTable);
            //    dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
            //    gvFleet.DataSource = dataTable.DefaultView;
            //    gvFleet.DataBind();
            //}
            //gvFleet.SelectedIndex = -1;
            //clearAll();
            /////////// Add sort arrows
            //int index = -1;
            //foreach (DataControlField field in gvFleet.Columns)
            //{
            //    if (field.SortExpression == e.SortExpression)
            //    {
            //        index = gvFleet.Columns.IndexOf(field);
            //    }
            //}
            //Image sortImage2 = new Image();
            //if (getSortDirectionString1("Ascending") == "ASC")
            //{
            //    sortImage2.ImageUrl = "~/Images/asc.gif";
            //    sortImage2.AlternateText = "Ascending Order";
            //}
            //else
            //{
            //    sortImage2.ImageUrl = "~/Images/desc.gif";
            //    sortImage2.AlternateText = "Descending Order";
            //}
            //// Add the image to the appropriate header cell.            
            //gvFleet.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        protected void clearAll()
        {
            tbEVMaker.Text = string.Empty;
            lblCatchError.Text = string.Empty;
            btnHideCatchError.Visible = false;
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "INSERT INTO [EV Maker] (MakerName) VALUES(@MakerName) ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    cnn.Open();
                    cmd.Parameters.Add(new SqlParameter("@MakerName", tbEVMaker.Text));
                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at NewBottonClick: " + ex.Message);
                    return;
                }
                finally
                {
                    cnn.Close();
                }
            }
            gvMaker.SelectedIndex = -1;
            populateEVMaker();
            PopUpMessage("Information Added");
        }

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                try
                {
                    string strQuery = "UPDATE [EV Maker] SET [MakerName] = @MakerName WHERE [MakerName] = @OldMakerName ";
                    SqlCommand cmd = new SqlCommand(strQuery, cnn);
                    SqlDataReader readerProfile = null;
                    //GridViewRow gvRow = gvFleet.Rows[gvFleet.SelectedIndex];

                    cnn.Open();
                    GridViewRow gvRow = gvMaker.Rows[gvMaker.SelectedIndex];

                    cmd.Parameters.Add(new SqlParameter("@OldMakerName", gvRow.Cells[1].Text));
                    cmd.Parameters.Add(new SqlParameter("@MakerName", tbEVMaker.Text));

                    readerProfile = cmd.ExecuteReader();
                    readerProfile.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Error at btnUpdateClick: " + ex.Message);
                    return;
                }
                finally
                {
                    cnn.Close();
                }

            }
            populateEVMaker();
            PopUpMessage("Information Updated");
        }

        protected void btnCancelClick(object sender, EventArgs e)
        {
            gvMaker.SelectedIndex = -1;
            clearAll();
            btnUpdate.Visible = false;
        }


    }
}