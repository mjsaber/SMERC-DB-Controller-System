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
using System.IO;

// 4-4-13 Addition for Loading Images
//using System.Drawing.Imaging;
using System.Drawing;


using RTMC;


namespace EVEditor
{
    public partial class EditParkingLot : System.Web.UI.Page
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


        string[] ColumnsToHide = { "ID", "City ID", "State ID", "Location Directions" };

        // Update this path to the computers location of no image!!
        string strPathToNoImage = "../images/no_img_avail.png";
//@"D:\EV\EV\RTMC\Images\no_img_avail.png"; // URL to the "No Image" - image.
       
        
        
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
                cbShowActivatedOrgs.Checked = true;
                cbShowActivatedPL.Checked = true;
                Initialize(cbShowActivatedOrgs.Checked, cbShowActivatedPL.Checked);                
            }
        }


        protected void Initialize(bool blnActivatedOrg, bool blnActivatedPL)
        {
            voidPopulateddlModeOrganization(blnActivatedOrg);
            voidPopulategvPL(blnActivatedPL, ddlModeOrganization.SelectedValue, blnActivatedOrg);
            voidPopulateddlState();
        }

        #region Gridview functions.  (Populate, Sorting, Paging, etc)

        protected void voidPopulategvPL(bool blnActivated, string strOrgID, bool blnActivatedOrg)
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

                string sqlQuery = "SELECT pl.[ID], pl.Name, pl.Address, pl.[City], s.[State], pl.[State ID], pl.[City ID], pl.[ChargingBoxLocationImage], pl.[ChargingBoxLocationDirection], c.[Name] as Organization, pl.[Zip Code], pl.Latitude, " +
                                  " pl.Longitude, pl.Activate  " +                                  
                                  " FROM [Parking Lot] as pl  " +
                                  " INNER JOIN [State] as s ON s.[Number] = pl.[State ID] "+ 
                                  " INNER JOIN [City] as c ON C.[ID] = pl.[City ID] ";

                // if this user is a Master user
                // the user may access all the information
                if (blnListhasMasterRole)
                {
                    // if an organization is chosen, populate the associated data
                    if (strOrgID != string.Empty)
                    {
                        sqlQuery += " WHERE pl.[City ID] = '" + strOrgID + "'";
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


                // If chosen to show only activated, then choose only activated parking lots
                if (blnActivated)
                    sqlQuery += " AND pl.Activate = 1 ";
                if (blnActivatedOrg)
                    sqlQuery += " AND c.Activate = 1 ";

                // Order the selection by name
                sqlQuery += " ORDER BY pl.Name ASC";

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
                            ShowError("Error at voidPopulategvPL: " + ex.Message);
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
            gvPL.DataSource = Session["data"];
            gvPL.DataBind();
        }
        protected void gvPLSelectedIndex(object sender, EventArgs e)
        {            
            HideError();
            fillinInfo();
            btnUpdate.Visible = true;
            showcbClearImage();
            cbClearImage.Checked = false;
        }

        protected void showcbClearImage()
        {
            cbClearImage.Visible = true;
        }

        protected void hidecbClearImage()
        {
            cbClearImage.Visible = false;
            cbClearImage.Checked = false;
        }

        protected void gvPLPaging(object sender, GridViewPageEventArgs e)
        {
            hidecbClearImage();
            gvPL.SelectedIndex = -1;
                        
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            DataTable dataTable = Session["data"] as DataTable;

            gvPL.PageIndex = e.NewPageIndex;
            gvPL.DataSource = dataTable;
            gvPL.DataBind();
        }

        protected void gvPLRowCreated(object sender, GridViewRowEventArgs e)
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

        protected void gvPLSorting(object sender, GridViewSortEventArgs e)
        {
            hidecbClearImage();
            
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvPL.DataSource = dataTable.DefaultView;
                gvPL.DataBind();
            }
            gvPL.SelectedIndex = -1;
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvPL.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvPL.Columns.IndexOf(field);
                }
            }
            System.Web.UI.WebControls.Image sortImage2 = new System.Web.UI.WebControls.Image();
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
            gvPL.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        #region PopulateFunctions

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
                            ShowError("Error at voidPopulateddlState: " + ex.Message);
                            return;
                        }
                    }
                }
            }

            ddlState.DataSource = DT;
            ddlState.DataValueField = "Number";
            ddlState.DataTextField = "State";
            ddlState.DataBind();

            ListItem li = new ListItem("Select...", "-1");
            ddlState.Items.Insert(0, li);
        }

        protected void voidPopulateddlModeOrganization(bool blnActivatedOnly)
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
                    if (blnActivatedOnly)
                    {
                        strQuery += " WHERE Activate = 1 ";
                        strListItemInsert = "Activated Organizations";
                    }
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
                    if (blnActivatedOnly)
                    {
                        strQuery += " AND Activate = 1 ";
                        strListItemInsert = "Activated Organizations";
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

                ddlModeOrganization.DataSource = dt;
                ddlModeOrganization.DataValueField = "ID"; // DataValueField contains the GUID of the City
                ddlModeOrganization.DataTextField = "Name"; // DataTextField contains the Name of the City
                ddlModeOrganization.DataBind();

                ddlOrganization.DataSource = dt;
                ddlOrganization.DataValueField = "ID"; // DataValueField contains the GUID of the City
                ddlOrganization.DataTextField = "Name"; // DataTextField contains the Name of the City
                ddlOrganization.DataBind();

                ListItem li = new ListItem(strListItemInsert, "-1");
                ListItem li2 = new ListItem(strListItemInsert, "-1");

                ddlModeOrganization.Items.Insert(0, li);
                ddlOrganization.Items.Insert(0, li2);

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

        #endregion

        #region Helper Functions

        protected void fillinInfo()
        {
            GridViewRow gvRow;
            try
            {
                gvRow = gvPL.Rows[gvPL.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                ClearAllErrorLbl();
                ClearImage();
                return;
            }

            tbName.Text = gvRow.Cells[findGVcolumn("Name")].Text;
            tbAddress.Text = gvRow.Cells[findGVcolumn("Address")].Text;

            ddlOrganization.SelectedValue = gvRow.Cells[findGVcolumn("City ID")].Text;
            ddlState.SelectedValue = gvRow.Cells[findGVcolumn("State ID")].Text;
            tbCity.Text = gvRow.Cells[findGVcolumn("City")].Text;
            tbZipCode.Text = gvRow.Cells[findGVcolumn("Zip Code")].Text;
            tbLatitude.Text = gvRow.Cells[findGVcolumn("Latitude")].Text;
            tbLongitude.Text = gvRow.Cells[findGVcolumn("Longitude")].Text;
            string strPLID = gvRow.Cells[findGVcolumn("ID")].Text;
            // TO LOAD IMAGE            
            
            //tbLocationDirections.Text = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Location Directions")].Text);
            tbUpdatedTextbox.Content = Server.HtmlDecode(gvRow.Cells[findGVcolumn("Location Directions")].Text);


            imageChargingLocation.ImageUrl = "~/Editor/ShowImage.ashx?PLID=" + strPLID;

            
            
            CheckBox cbActivate = (CheckBox)gvRow.Cells[findGVcolumn("Activate")].Controls[0];
            if (cbActivate.Checked)
                ddlActivate.SelectedValue = "1";
            else
                ddlActivate.SelectedValue = "0";
            
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

        protected void ClearAllErrorLbl()
        {
            lblFileUploadError.Text = string.Empty;

        }
        protected void ClearAllTbs()
        {
            btnUpdate.Visible = false;
            tbName.Text = string.Empty;
            tbAddress.Text = string.Empty;
            ddlOrganization.SelectedIndex = 0;
            ddlState.SelectedIndex = 0;
            tbCity.Text = string.Empty;
            tbZipCode.Text = string.Empty;
            tbLatitude.Text = string.Empty;
            tbLongitude.Text = string.Empty;
            //tbLocationDirections.Text = string.Empty;
            tbUpdatedTextbox.Content = string.Empty;    
            ddlActivate.SelectedIndex = 0;
            
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvPL.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvPL.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
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

        #endregion

        #region Message Functions.  (Show, Popup, Hide, etc)
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

        #region Button Functions
        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            gvPL.SelectedIndex = -1;
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            hidecbClearImage();
        }

        protected void ClearImage()
        {
            imageChargingLocation.ImageUrl = string.Empty;
            
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery; SqlCommand cmd; SqlDataReader readerProfile = null;

            bool blnByteExists = false;
            bool blnPassesImageValidation = true; // Checks that file is an image, and also that it is < 1 MB

            FileUpload img = (FileUpload)fileUploadLocationPic;
            Byte[] imgByte = null;
            if (img.HasFile && img.PostedFile != null)
            {
                
                string strFileExtension = Path.GetExtension(img.PostedFile.FileName.ToString());
                if (!chkValidExtension(strFileExtension))
                {
                    lblFileUploadError.Text += "'" + strFileExtension + "'" + " Format is not allowed. <br>";

                    blnPassesImageValidation = false;
                }
                //// Create a FILE
                //HttpPostedFile File = fileUploadLocationPic.PostedFile;
                //// Create byte array
                //imgByte = new Byte[File.ContentLength];
                //// force control to load data
                //File.InputStream.Read(imgByte, 0, File.ContentLength);
                imgByte = img.FileBytes;
                blnByteExists = true;
                if (imgByte.Length / 1024f / 1024f > 1)
                {
                    lblFileUploadError.Text += "Image size must be less than 1 MB.";
                    blnPassesImageValidation = false;
                }
            }
            else // Put a "No Image"
            {
                blnByteExists = true;
                
                if(!string.IsNullOrWhiteSpace(imageChargingLocation.ImageUrl) && imageChargingLocation.ImageUrl.IndexOf("ShowImage") == -1)
                {                    
                    // Get Path of Image + root path
                    System.Drawing.Image imgPreviewImg = System.Drawing.Image.FromFile(MapPath(System.IO.Path.GetFileName(imageChargingLocation.ImageUrl.ToLower())));

                    // Convert the image into a byte array, which we will add into the database later.
                   imgByte = imageToByteArray(imgPreviewImg);
                }
                else
                {
                    // Transfer the image to a class type 'Image'
                    System.Drawing.Image imgNoImage = System.Drawing.Image.FromFile(Server.MapPath(strPathToNoImage));
                    
                    // Convert the image to a byte Array
                    imgByte = imageToByteArray(imgNoImage);                    
                }
            }
            // If an image was previewed previously, then add the image to the database.            
            
            if (!blnPassesImageValidation) // If the image doesn't pass validation, stop function
            {
                PopUpMessage("Error while inserting.");
                return;
            }

            try
            {
                strQuery = " INSERT INTO [Parking Lot](Name, Address, City, [State ID], [City ID], [Zip Code], Latitude, Longitude, Activate, ChargingBoxLocationDirection ";

                if (blnByteExists)
                {
                    strQuery += ", [ChargingBoxLocationimage]) ";
                }
                else
                {
                    strQuery += ")";
                }
                strQuery += " VALUES(@plName, @Address, @City, @StateID, @CityID, @ZipCode, @Latitude, @Longitude, @Activate, @ChargingBoxLocationDirection ";

                if (blnByteExists)
                {
                    strQuery += ", @LocImage)";
                }
                else
                {
                    strQuery += ")";
                }

                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();

                if (blnByteExists)
                {
                    //SqlParameter ParamLocImage = new SqlParameter();
                    //ParamLocImage.ParameterName = "@LocImage";
                    //ParamLocImage.Value = imgByte;
                    //cmd.Parameters.Add(ParamLocImage);
                    cmd.Parameters.Add(new SqlParameter("@LocImage", (object)imgByte));
                }

                SqlParameter ParamplName = new SqlParameter();
                ParamplName.ParameterName = "@plName";
                ParamplName.Value = tbName.Text;
                cmd.Parameters.Add(ParamplName);

                SqlParameter ParamAddress = new SqlParameter();
                ParamAddress.ParameterName = "@Address";
                ParamAddress.Value = tbAddress.Text;
                cmd.Parameters.Add(ParamAddress);

                SqlParameter ParamCity = new SqlParameter();
                ParamCity.ParameterName = "@City";
                ParamCity.Value = tbCity.Text;
                cmd.Parameters.Add(ParamCity);

                SqlParameter ParamStateID = new SqlParameter();
                ParamStateID.ParameterName = "@StateID";
                ParamStateID.Value = ddlState.SelectedValue;
                cmd.Parameters.Add(ParamStateID);

                SqlParameter ParamCityID = new SqlParameter();
                ParamCityID.ParameterName = "@CityID";
                ParamCityID.Value = ddlOrganization.SelectedValue;
                cmd.Parameters.Add(ParamCityID);

                SqlParameter ParamZipCode = new SqlParameter();
                ParamZipCode.ParameterName = "@ZipCode";
                ParamZipCode.Value = tbZipCode.Text;
                cmd.Parameters.Add(ParamZipCode);

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
                
                //SqlParameter ParamLocationDirection = new SqlParameter();
                //ParamLocationDirection.ParameterName = "@ChargingBoxLocationDirection";
                //ParamLocationDirection.Value = tbLocationDirections.Text;
                //cmd.Parameters.Add(ParamLocationDirection);

                SqlParameter ParamLocationDirection = new SqlParameter();
                ParamLocationDirection.ParameterName = "@ChargingBoxLocationDirection";
                ParamLocationDirection.Value = tbUpdatedTextbox.Content;
                cmd.Parameters.Add(ParamLocationDirection);

                readerProfile = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                ShowError("Error in btnNewClick: " + ex.Message);
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

            voidPopulategvPL(cbShowActivatedPL.Checked, ddlModeOrganization.SelectedValue, cbShowActivatedOrgs.Checked);
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            gvPL.SelectedIndex = -1;
            hidecbClearImage();
            PopUpMessage("Information added.");
        }

        public bool chkValidExtension(string ext)
        {
            string[] PosterAllowedExtensions = new string[] { ".gif", ".jpeg", ".jpg", ".png", ".GIF", ".JPEG", ".JPG", ".PNG", ".BMP", ".bmp" };
            for (int i = 0; i < PosterAllowedExtensions.Length; i++)
            {
                if (ext == PosterAllowedExtensions[i])
                    return true;
            }
            return false;
        }

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery; SqlCommand cmd; SqlDataReader readerProfile = null;
            GridViewRow gvRow = gvPL.Rows[gvPL.SelectedIndex]; // Obtain selected index of gvPL

            bool blnByteExists = false;
            bool blnPassesImageValidation = true; // Checks that file is an image, and also that it is < 1 MB

            FileUpload img = (FileUpload)fileUploadLocationPic;
            Byte[] imgByte = null;

            //blnByteExists = true;
            string sPath = "C:/Users/Jun/Pictures/car_full.png";



            if (img.HasFile && img.PostedFile != null)
            {
                string strFileExtension = Path.GetExtension(img.PostedFile.FileName.ToString());
                if (!chkValidExtension(strFileExtension))
                {
                    lblFileUploadError.Text += "'" + strFileExtension + "'" + " Format is not allowed. <br>";

                    blnPassesImageValidation = false;
                }
                //// Create a FILE
                //HttpPostedFile File = fileUploadLocationPic.PostedFile;
                //// Create byte array
                //imgByte = new Byte[File.ContentLength];
                //// force control to load data
                //File.InputStream.Read(imgByte, 0, File.ContentLength);
                imgByte = img.FileBytes;

                ////Use FileInfo object to get file size.
                //FileInfo fInfo = new FileInfo(sPath);
                //long numBytes = fInfo.Length;

                ////Open FileStream to read file
                //FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

                ////Use BinaryReader to read file stream into byte array.
                //BinaryReader br = new BinaryReader(fStream);

                ////When you use BinaryReader, you need to supply number of bytes to read from file.
                ////In this case we want to read entire file. So supplying total number of bytes.
                //imgByte = br.ReadBytes((int)numBytes);

                blnByteExists = true;
                if (imgByte.Length / 1024f / 1024f > 1)
                {
                    lblFileUploadError.Text += "Image size must be less than 1 MB.";
                    blnPassesImageValidation = false;
                }

            }
            else // Put a "No Image"
            {
                blnByteExists = true;

                if (!string.IsNullOrWhiteSpace(imageChargingLocation.ImageUrl) && imageChargingLocation.ImageUrl.IndexOf("ShowImage") == -1)
                {
                    // Get Path of Image + root path
                    System.Drawing.Image imgPreviewImg = System.Drawing.Image.FromFile(MapPath(System.IO.Path.GetFileName(imageChargingLocation.ImageUrl.ToLower())));

                    // Convert the image into a byte array, which we will add into the database later.
                    imgByte = imageToByteArray(imgPreviewImg);
                }
                else
                {
                    // Transfer the image to a class type 'Image'
                    System.Drawing.Image imgNoImage = System.Drawing.Image.FromFile(Server.MapPath(strPathToNoImage));

                    // Convert the image to a byte Array
                    imgByte = imageToByteArray(imgNoImage);
                }
            }
            // If an image was previewed previously, then add the image to the database.            

            if (!blnPassesImageValidation) // If the image doesn't pass validation, stop function
            {
                PopUpMessage("Error while inserting.");
                return;
            }
            
            try
            {

                strQuery = "UPDATE [Parking Lot] SET [Name] = @plName, [Address] = @Address, [City] = @City, [State ID] = @StateID, [City ID] = @CityID, "
                    + " [Zip Code] = @ZipCode, [Latitude] = @Latitude, [Longitude] = @Longitude, [Activate] = @Activate, [ChargingBoxLocationDirection] = @ChargingBoxLocationDirection ";

                if (blnByteExists)
                {
                    strQuery+= ", [ChargingBoxLocationimage] = @LocImage ";                    
                }
                strQuery += " WHERE [ID] = @ID";
                 cmd = new SqlCommand(strQuery, cnn); 
                
                cnn.Open();

                if (blnByteExists)
                {
                    //SqlParameter ParamLocImage = new SqlParameter();
                    //ParamLocImage.ParameterName = "@LocImage";
                    //ParamLocImage.Value = imgByte;
                    //cmd.Parameters.Add(ParamLocImage);
                    cmd.Parameters.Add(new SqlParameter("@LocImage", (object)imgByte));
                }

                // Normal params.

                SqlParameter ParamplName = new SqlParameter();
                ParamplName.ParameterName = "@plName";
                ParamplName.Value = tbName.Text;
                cmd.Parameters.Add(ParamplName);

                SqlParameter ParamAddress = new SqlParameter();
                ParamAddress.ParameterName = "@Address";
                ParamAddress.Value = tbAddress.Text;
                cmd.Parameters.Add(ParamAddress);

                SqlParameter ParamCity = new SqlParameter();
                ParamCity.ParameterName = "@City";
                ParamCity.Value = tbCity.Text;
                cmd.Parameters.Add(ParamCity);

                SqlParameter ParamStateID = new SqlParameter();
                ParamStateID.ParameterName = "@StateID";
                ParamStateID.Value = ddlState.SelectedValue;
                cmd.Parameters.Add(ParamStateID);

                SqlParameter ParamCityID = new SqlParameter();
                ParamCityID.ParameterName = "@CityID";
                ParamCityID.Value = ddlOrganization.SelectedValue;
                cmd.Parameters.Add(ParamCityID);

                SqlParameter ParamZipCode = new SqlParameter();
                ParamZipCode.ParameterName = "@ZipCode";
                ParamZipCode.Value = tbZipCode.Text;
                cmd.Parameters.Add(ParamZipCode);

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

                //SqlParameter ParamLocationDirection = new SqlParameter();
                //ParamLocationDirection.ParameterName = "@ChargingBoxLocationDirection";
                //ParamLocationDirection.Value = tbLocationDirections.Text;
                //cmd.Parameters.Add(ParamLocationDirection);

                SqlParameter ParamLocationDirection = new SqlParameter();
                ParamLocationDirection.ParameterName = "@ChargingBoxLocationDirection";
                ParamLocationDirection.Value = tbUpdatedTextbox.Content;
                cmd.Parameters.Add(ParamLocationDirection);

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

            //HideError();
            voidPopulategvPL(cbShowActivatedPL.Checked, ddlModeOrganization.SelectedValue, cbShowActivatedOrgs.Checked);            
            fillinInfo();
            PopUpMessage("Updated");
        }

        #endregion

        #region Postback features. (ddlModeOrganization, cbShowActivated, etc)

        protected void ddlModeOrganization_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear all textboxes, error labels, and the image
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            hidecbClearImage();

            // Reset the gridview selection
            gvPL.SelectedIndex = -1;
            
            // Hide the btnUpdate
            btnUpdate.Visible = false;

            // Repopulate the gridview with the new settings.
            voidPopulategvPL(cbShowActivatedPL.Checked, ddlModeOrganization.SelectedValue, cbShowActivatedOrgs.Checked);
        }
        protected void cbShowActivatedPL_CheckedChanged(object sender, EventArgs e)
        {
            ClearAllTbs();
            ClearAllErrorLbl();
            ClearImage();
            hidecbClearImage();
            gvPL.SelectedIndex = -1;
            voidPopulategvPL(cbShowActivatedPL.Checked, ddlModeOrganization.SelectedValue, cbShowActivatedOrgs.Checked);
        }
        protected void cbShowActivatedOrgs_CheckedChanged(object sender, EventArgs e)
        {
            Initialize(cbShowActivatedOrgs.Checked, cbShowActivatedPL.Checked);
        }
        #endregion        

        protected void btnPreviewImage_Click(object sender, EventArgs e)
        {
            bool blnPassesImageValidation = true; // Checks that file is an image, and also that it is < 1 MB

            FileUpload img = (FileUpload)fileUploadLocationPic;
            Byte[] imgByte = null;
            if (img.HasFile && img.PostedFile != null)
            {
                string strFileExtension = Path.GetExtension(img.PostedFile.FileName.ToString());
                if (!chkValidExtension(strFileExtension))
                {
                    lblFileUploadError.Text += "'" + strFileExtension + "'" + " Format is not allowed. <br>";

                    blnPassesImageValidation = false;
                }
                // Create a FILE
                HttpPostedFile File = fileUploadLocationPic.PostedFile;
                // Create byte array
                imgByte = new Byte[File.ContentLength];
                // force control to load data
                File.InputStream.Read(imgByte, 0, File.ContentLength);

                if (imgByte.Length / 1024f / 1024f > 1)
                {
                    lblFileUploadError.Text += "Image size must be less than 1 MB.";
                    blnPassesImageValidation = false;
                }
                if (!blnPassesImageValidation) // If the image doesn't pass validation, stop function
                {
                    PopUpMessage("Error while previewing.");
                    return;
                }
                else
                {
                    try
                    {
                        File.SaveAs(MapPath(System.IO.Path.GetFileName(File.FileName).ToLower().ToString()));
                    }
                    catch (Exception ex)
                    {

                    }
                    imageChargingLocation.ImageUrl = System.IO.Path.GetFileName(File.FileName).ToLower().ToString();

                }
            }
            else
            {
                PopUpMessage("No Image selected!");
                return;
            }         

        }  
    }
}