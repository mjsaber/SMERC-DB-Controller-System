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
using RTMC;

namespace EVEditor
{
    public class ComboCityClass
    {

        public ComboCityClass(string MainCity, string SubCities, int noOfSubCities)
        {
            _MainCity = MainCity;
            _SubCities = SubCities;
            _noOfSubCities = noOfSubCities;
        }

        private string _SubCities;

        public string SubCities
        {
            get { return _SubCities; }
            set { _SubCities = value; }
        }

        private string _MainCity;

        public string MainCity
        {
            get { return _MainCity; }
            set { _MainCity = value; }
        }

        private int _noOfSubCities;

        public int noOfSubCities
        {
            get { return _noOfSubCities; }
            set { _noOfSubCities = value; }

        }

    }

    public partial class EditCityCombine : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        string[] strArrRolesToAllow = { "General Administrator" };
        string[] strArrTypesToAllow = { };
        string[] ColumnsToHide = {"GUID" };


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

                if (!blnHasPermission(ListOfRoles)) // only continue if the user is a city administrator
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
            }
        }

        protected void Initialize()
        {
            voidPopulategvCombCity(cbShowActivated.Checked);
        }

        #region gvCombCity functions

        protected void voidPopulategvCombCity(bool blnActivate)
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

                /// Creates the gridview with all of the combo cities.
                /// Multiple rows and many tables are used to be able
                /// to finally display the entire combinated city list in a whole format

                strQuery = "SELECT * From CombinatedCity";
                if (blnActivate)
                    strQuery += " Where Activate = 1";
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
                    RepopulateCheckBoxList();
                    PopulateDropDownList();
                    ShowError("Database is empty.  Please populate using Main City and Sub Cities below");

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

                int adder = 0;

                for (int i = 0; i < uniqueGUIDs.Count; i++)
                {
                    strQuery = "SELECT COUNT(*) FROM [CombinatedCity] WHERE ID='" + dt.Rows[adder][0] + "'";
                    if (blnActivate)
                        strQuery += " And Activate = 1";

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
                            //  LengthsOfMainCities.Add(j);
                        }
                    }
                }
                List<ComboCityClass> ComboCityActualNameRelation = new List<ComboCityClass>();
                List<string> ComboCitiesWithCityNames = new List<string>(); // Create a list of all the Actual City names

                for (int i = 0; i < ComboCitiesList.Count; i++)
                {
                    ComboCitiesWithCityNames.Add(CityIdNameRelation[ComboCitiesList[i]]);
                }

                List<List<string>> CityList = new List<List<string>>(); // 2D nest Store all City values

                iterator = 0;
                for (int i = 0; i < LengthsOfMainCities.Count; i++)
                {
                    //
                    // Populate the sublist with the main city and sub city combos
                    //
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
                // Label2.Text = CityList[1][0];
                StringBuilder intarray = new StringBuilder();
                foreach (var sublist in CityList)
                {
                    foreach (var value in sublist)
                    {
                        intarray.Append(value).Append("   ");
                    }
                    intarray.Append("<br>");
                }

                // ATTEMPT: Create datatable and bind city data to it
                DataTable newDT = new DataTable();

                newDT.Columns.Add("Main City", typeof(string));
                newDT.Columns.Add("Sub Cities", typeof(string));
                newDT.Columns.Add("GUID", typeof(string));
                newDT.Columns.Add("Activate", typeof(string));

                string subcities;

                for (int i = 0; i < LengthsOfMainCities.Count; i++)
                {
                    subcities = string.Empty;
                    for (int v = 1; v <= LengthsOfMainCities[i]; v++)
                    {
                        subcities += CityList[i][v];
                        if (v != LengthsOfMainCities[i])
                            subcities += " - ";
                    }

                    newDT.Rows.Add(CityList[i][0], subcities, uniqueGUIDs[i], ActivatedCheck[i]);
                }

                Session["data"] = newDT;
                gvCombCity.DataSource = Session["data"]; // Bind Data
                gvCombCity.DataBind();

                /// ////////////////////////////////////////////////////////// ////////////// ////////////// ////////////// 
                /// END CONSTRUCTING THE COMBO CITY GRID VIEW
                /// ////////////// ////////////// ////////////// ////////////// ////////////// ////////////// ////////////// 

                // Start: Display the rest of the gridviews/ checkboxes
                StringBuilder builder = new StringBuilder();
                foreach (string cc in ComboCitiesList)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(cc).Append(" <br> ");
                }
                string result = builder.ToString();

                // Label1.Text = result; // Show the Combo City IDs, ( ALL OF THE IDs)

                da.Dispose();
                cmd.Dispose();


                /// Display Data for the CheckBox List
                strQuery = "SELECT [Name] FROM [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                cblSubCities.DataSource = dt;
                cblSubCities.DataValueField = "Name";
                cblSubCities.DataTextField = "Name";
                cblSubCities.DataBind();
                da.Dispose();
                cmd.Dispose();
                // End Data for the CheckBox List
                PopulateDropDownList();
            }

            catch (Exception ex)
            {
                ShowError("Error at voidPopulategvCombCity: " + ex.Message);
            }

            finally
            {
                cnn.Close();
            }

            ClearAllTbs();
            ClearAllLabel();
        }
        protected void gvCombCitySelectedIndex(object sender, EventArgs e)
        {
            ClearAllLabel();
            HideError();            
            ClearCheckBox();
            fillinInfo();

            RepopulateCheckBoxList();
            GridViewRow gvRow = gvCombCity.Rows[gvCombCity.SelectedIndex];
            ddlMainCity.SelectedValue = gvRow.Cells[findGVcolumn("Main Organization")].Text;
            RemoveItemFromCheckBoxList();   
  
            string checkString;
            checkString = gvRow.Cells[findGVcolumn("Sub Organizations")].Text;
            for (int j = 0; j < cblSubCities.Items.Count; j++)
            {
                if (-1 != checkString.IndexOf(cblSubCities.Items[j].ToString()))
                {
                    cblSubCities.Items[j].Selected = true;

                }
            }
            voidCreateComboCityString();
        }

        protected void gvCombCityPaging(object sender, GridViewPageEventArgs e)
        {
            gvCombCity.SelectedIndex = -1;

            ClearAllLabel();
            HideError();
            ClearAllTbs();
            ClearCheckBox();
            ddlMainCity.SelectedIndex = -1;

            DataTable dataTable = Session["data"] as DataTable;

            gvCombCity.PageIndex = e.NewPageIndex;
            gvCombCity.DataSource = dataTable;

            gvCombCity.DataBind();
        }

        protected void gvCombCityRowCreated(object sender, GridViewRowEventArgs e)
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

        protected void gvCombCitySorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvCombCity.DataSource = dataTable.DefaultView;
                gvCombCity.DataBind();
            }
            gvCombCity.SelectedIndex = -1;
            ClearAllTbs();
            ClearAllLabel();
            ClearCheckBox();
            ddlMainCity.SelectedIndex = -1;
            HideError();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvCombCity.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvCombCity.Columns.IndexOf(field);
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
            gvCombCity.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        #region PopulateDDL functions
        protected void PopulateDropDownList()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT [Name] FROM [City]";
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlMainCity.DataSource = dt;
                ddlMainCity.DataValueField = "Name";
                ddlMainCity.DataTextField = "Name";
                ddlMainCity.DataBind();

                ListItem li = new ListItem("Select...", "-1");
                ddlMainCity.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at PopulateDropDownList: " + ex.Message);
            }

            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
        }

        protected void RepopulateCheckBoxList()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                strQuery = "SELECT [Name] FROM [City]";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                cblSubCities.DataSource = dt;
                cblSubCities.DataValueField = "Name";
                cblSubCities.DataTextField = "Name";
                cblSubCities.DataBind();
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("Error at RepopulateCheckBoxList: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }
        }

        #endregion



        #region Helper Functions

        protected void fillinInfo()
        {
            GridViewRow gvRow;
            try
            {
                gvRow = gvCombCity.Rows[gvCombCity.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                return;
            }
            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            if (!cbActivate.Checked)
                btnActivate.Visible = true;
            else
                btnDeactivate.Visible = true;

        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvCombCity.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvCombCity.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void ClearAllTbs()
        {
        }

        protected void ClearCheckBox()
        {
            foreach (ListItem li in cblSubCities.Items)
            {
                li.Selected = false;
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
                        return true;
                    }
                }
                for (int k = 0; k < strArrTypesToAllow.Count(); k++)
                {
                    if (RoleList[i].IndexOf(strArrTypesToAllow[k]) != -1)
                        return true;
                }
            }
            return false;
        }


        protected void RefreshPage()
        {
            voidPopulategvCombCity(cbShowActivated.Checked);
            //Page.Response.Redirect(Page.Request.Url.ToString(), true);
        }

        protected void RemoveItemFromCheckBoxList()
        {
            for (int i = 1; i < ddlMainCity.Items.Count; i++)
            {
                if (ddlMainCity.Items[i].Selected)
                {
                    // After Selecting a value from the DDL, remove it from CHECKBOXlist
                    cblSubCities.Items.Remove(ddlMainCity.Items[i]);
                }
            }
        }

        protected void voidCreateComboCityString()
        {
            int numSelected = 0;
            foreach (ListItem li in cblSubCities.Items)
            {
                if (li.Selected)
                {
                    numSelected++;
                }
            }

            lblMainCity.Visible = true;
            lblCombined.Visible = true;
            lblCCdash.Visible = true;
            lblCombined.Text = "";
            for (int i = 1; i < ddlMainCity.Items.Count; i++)
            {
                if (ddlMainCity.Items[i].Selected)
                {
                    lblMainCity.Text += ddlMainCity.Items[i];
                    // After Selecting a value from the DDL, remove it from CHECKBOXlist
                    //      CheckBoxList1.Items.Remove(MainCityDDL.Items[i]);
                }
            }
            lblCCdash.Text = " - ";

            //   CombinedLabel.Text = numSelected.ToString();

            for (int i = 0; i < cblSubCities.Items.Count; i++)
            {
                if (cblSubCities.Items[i].Selected)
                {
                    lblCombined.Text += cblSubCities.Items[i];
                    //if (((i+1)<CheckBoxList1.Items.Count)&&CheckBoxList1.Items[i + 1].Selected) ////// TODO FIX THIS:::::
                    //    CombinedLabel.Text += " - ";
                    if (numSelected > 1)
                    {
                        lblCombined.Text += " - ";
                        numSelected--;
                    }
                }
            }
        }

        protected bool ValidationCheck() // returns 0 for pass, -1 for not pass
        {
            //   RedLabel.Visible = true;
            if (cblSubCities.SelectedIndex == -1)
            {
                ShowError("Erro: Select atleast one city");
                return false;
            }
            if (ddlMainCity.SelectedIndex == 0)
            {
                ShowError("Error: Select a city");
            }
            for (int i = 0; i < gvCombCity.Rows.Count; i++)
            {
                if (gvCombCity.Rows[i].Cells[findGVcolumn("Main Organization")].Text == lblMainCity.Text)
                {
                    if (gvCombCity.Rows[i].Cells[findGVcolumn("Sub Organizations")].Text.Length == lblCombined.Text.Length)
                    {
                        ShowError("Error: Repeat Data in database");
                        return false;
                    }
                }
            }
            return true;
        }

        protected void ClearAllLabel()
        {
            lblCCdash.Text = string.Empty;
            lblCombined.Text = string.Empty;
            lblMainCity.Text = string.Empty;
            btnActivate.Visible = false;
            btnDeactivate.Visible = false;
        }


        #endregion // End Helper Region


        #region ErrorMessage, PopUpMessage, ShowError

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

        protected void ShowError(string Message) // Show an Error (not a pop up) with the Message
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

        #endregion


        #region cbl functions
        protected void cblSubCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAllLabel();
            gvCombCity.SelectedIndex = -1;
            voidCreateComboCityString();
        }
        protected void cbShowActivated_CheckedChanged(object sender, EventArgs e)
        {
            gvCombCity.SelectedIndex = -1;
            voidPopulategvCombCity(cbShowActivated.Checked);
        }
        #endregion


        #region button Clicks.  (btnNewClick, etc.)

        protected void btnNewClick(object sender, EventArgs e)
        {
            if (!ValidationCheck())
            {
                return;
            }

            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;
            string MainCity = "null";

            for (int i = 1; i < ddlMainCity.Items.Count; i++)
            {
                if (ddlMainCity.Items[i].Selected)
                {
                    MainCity = ddlMainCity.Items[i].ToString();
                }
            }

            Guid Guid1 = System.Guid.NewGuid();
            Guid MainCityGuid = new Guid();
            Guid CombinatedCityGuid = new Guid();

            try
            {
                cnn.Open();

                // Obtain MainCity ID
                strQuery = "SELECT ID FROM [City] Where [Name] = '" + MainCity + "'";
                cmd = new SqlCommand(strQuery, cnn);
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                string MainCityID = dt.Rows[0][0].ToString();
                MainCityGuid = new Guid(dt.Rows[0][0].ToString());

                //Label1.Text = MainCityID; // For Viewing purposes only

                cmd.ExecuteNonQuery();
                cmd.Dispose();

                for (int i = 0; i < cblSubCities.Items.Count; i++) // Cycle through the checkbox
                //   for (int i = CheckBoxList1.Items.Count-1; i >=0; i--)
                {                                                   // and look for checked items. (Atleast 1)
                    if (cblSubCities.Items[i].Selected) // If the checkbox list is checked, then
                    {
                        // The code below cycles through the Checkbox list and enters into the SQL database
                        // the different values of the city with the same value of main GUID (ID) so that it can
                        // be retrieved later
                        // CombinedLabel.Text += CheckBoxList1.Items[i];
                        strQuery = "SELECT ID FROM [City] Where [Name] = '" + cblSubCities.Items[i] + "'";
                        cmd = new SqlCommand(strQuery, cnn);
                        da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        dt = new DataTable();
                        da.Fill(dt);
                        string CombCityID = dt.Rows[0][0].ToString();
                        CombinatedCityGuid = new Guid(dt.Rows[0][0].ToString());
                        //  Label1.Text += "<br> " + CombCityID;

                        strQuery = "INSERT INTO CombinatedCity(ID, MainCityID, CombinatedCityID)" +
                        " VALUES ('" + Guid1 + "','" + MainCityID + "','" + CombinatedCityGuid + "')";

                        cmd = new SqlCommand(strQuery, cnn);
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Dispose();
                cnn.Close();
                PopUpMessage("City added.");

                lblCombined.Text = String.Empty;
                RefreshPage();
            }

            catch (Exception ex)
            {
                ShowError("Error at btnNewClick: " + ex.Message);                
            }
            finally
            {
                if(cnn!=null)
                    cnn.Close();
            }
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            ClearAllLabel();
            ddlMainCity.SelectedIndex = -1;
            gvCombCity.SelectedIndex = -1;
            ClearCheckBox();
            voidPopulategvCombCity(cbShowActivated.Checked);
        }

        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            HideError();
        }

        protected void btnActivateClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            GridViewRow gvRow = gvCombCity.Rows[gvCombCity.SelectedIndex];

            try
            {
                // strQuery = "DELETE FROM [EVDemo].[dbo].[CombinatedCity] where ID='" + gvRow.Cells[4].Text + "'";// +CombinedCityGrid.Rows[CombinedCityGrid.SelectedIndex].Cells[4].Text + "'";

                strQuery = "UPDATE [CombinatedCity] SET [Activate] ='1' WHERE ID = '" + gvRow.Cells[findGVcolumn("GUID")].Text + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ShowError("Error at btnActivateClick: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
                return;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            PopUpMessage("Record Activated");
            RefreshPage();
            ClearCheckBox();
            ClearAllLabel();
            gvCombCity.SelectedIndex = -1;
        }

        protected void btnDeactivateClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            GridViewRow gvRow = gvCombCity.Rows[gvCombCity.SelectedIndex];

            try
            {
                // strQuery = "DELETE FROM [EVDemo].[dbo].[CombinatedCity] where ID='" + gvRow.Cells[4].Text + "'";// +CombinedCityGrid.Rows[CombinedCityGrid.SelectedIndex].Cells[4].Text + "'";

                strQuery = "UPDATE [CombinatedCity] SET [Activate] ='0' WHERE ID = '" + gvRow.Cells[findGVcolumn("GUID")].Text + "'";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ShowError("Error at btnDeactivateClick: " + ex.Message);
                if (cnn != null)
                    cnn.Close();
                return;
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }
            PopUpMessage("Record Deactivated");
            RefreshPage();
            ClearCheckBox();
            ClearAllLabel();
            gvCombCity.SelectedIndex = -1;
        }

        #endregion end button click region

        protected void ddlMainCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvCombCity.SelectedIndex = -1;
            HideError();
            ClearAllLabel();
            ClearAllTbs();
            RepopulateCheckBoxList();
            lblCombined.Text = string.Empty;
            RemoveItemFromCheckBoxList();
        }       
    }
}