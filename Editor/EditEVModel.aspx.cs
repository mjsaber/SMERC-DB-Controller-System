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
using RTMC;



namespace EVEditor
{
    public partial class EditEVModel : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        string[] strArrRolesToAllow = { "General Administrator" };
        string[] strArrTypesToAllow = {  };
        string[] ColumnsToHide = { "ID" };

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
                Initialize();
            }
        }


        protected void Initialize()
        {
            voidPopulategvEVModel();
            voidPopulateddlMaker();
            voidPopulateddlYear();
        }

        #region Gridview Functions

        protected void voidPopulategvEVModel()
        {
            DataTable DT = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString)) // Populate the gridview according to city selected
            {
                string sqlQuery = "SELECT [ID], Manufacturer, Model, Level1MaxCurrent, Level1MaxVoltage, Level1MaxPower, " +
                                  " Level2MaxCurrent, Level2MaxVoltage, Level2MaxPower, Level3MaxCurrent, Level3MaxVoltage, Level3MaxPower, " + 
                                  " BatteryCapacity, ModelImage, Year "+ 
                                  " FROM [EV Model] ";

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
                            ShowError(ex.Message);
                            return;
                        }
                        if (DT.Rows.Count == 0) // If the SQL Query returned 0 rows, then PopUpMessage for clarity to note there are no Gateways for given selection.
                            PopUpMessage("No Data.");
                    }
                }
            }
            
            Session["data"] = DT;
            gvEVModel.DataSource = Session["data"];
            gvEVModel.DataBind();
        }

        protected void gvEVModelSelectedIndex(object sender, EventArgs e)
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


        protected void gvEVModelPaging(object sender, GridViewPageEventArgs e)
        {
            gvEVModel.SelectedIndex = -1;
            hidecbClearImage();
            ClearAllTbs();
            DataTable dataTable = Session["data"] as DataTable;

            gvEVModel.PageIndex = e.NewPageIndex;
            gvEVModel.DataSource = dataTable;
            gvEVModel.DataBind();
        }

        protected void gvEVModelRowCreated(object sender, GridViewRowEventArgs e)
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

        protected void gvEVModelSorting(object sender, GridViewSortEventArgs e)
        {
            hidecbClearImage();
            DataTable dataTable = Session["data"] as DataTable;
            if (dataTable != null)
            {
                DataView dataView = new DataView(dataTable);
                dataTable.DefaultView.Sort = e.SortExpression + " " + getSortDirectionString(e.SortDirection.ToString());
                gvEVModel.DataSource = dataTable.DefaultView;
                gvEVModel.DataBind();
            }
            gvEVModel.SelectedIndex = -1;
            ClearAllTbs();
            ///////// Add sort arrows
            int index = -1;
            foreach (DataControlField field in gvEVModel.Columns)
            {
                if (field.SortExpression == e.SortExpression)
                {
                    index = gvEVModel.Columns.IndexOf(field);
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
            gvEVModel.HeaderRow.Cells[index].Controls.Add(sortImage2);
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

        #region Helper Functions

        protected void voidPopulateddlYear()
        {
            string strListItemInsert = "Select...";
            ddlYear.Items.AddRange(Enumerable.Range(1980, 71).Select(e => new ListItem(e.ToString())).ToArray());
            ListItem li = new ListItem(strListItemInsert, "-1");
            ddlYear.Items.Insert(0, li);
        }

        protected void voidPopulateddlMaker()
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd;
            DataTable dt = null;
            SqlDataAdapter da;

            try
            {
                string strListItemInsert = "Select...";
                strQuery = "SELECT * FROM [EV Maker] ORDER BY MakerName ";
                cnn.Open();
                cmd = new SqlCommand(strQuery, cnn);
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                dt = new DataTable();
                da.Fill(dt);
                ddlManufacturer.DataSource = dt;
                ddlManufacturer.DataValueField = "MakerName";
                ddlManufacturer.DataTextField = "MakerName";
                ddlManufacturer.DataBind();
                ListItem li = new ListItem(strListItemInsert, "-1");
                ddlManufacturer.Items.Insert(0, li);
                da.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                ShowError("<br> PopulateddlMaker Error: " + ex.Message);
            }
            finally
            {
                if (cnn != null)
                    cnn.Close();
            }

        }
        protected void fillinInfo()
        {
            GridViewRow gvRow;
            try
            {
                gvRow = gvEVModel.Rows[gvEVModel.SelectedIndex];
            }
            catch
            {
                ClearAllTbs();
                return;
            }

            ddlManufacturer.SelectedValue = gvRow.Cells[findGVcolumn("Manufacturer")].Text;
            tbModel.Text = gvRow.Cells[findGVcolumn("Model")].Text; ;
            tbLevel1MaxCurrent.Text = gvRow.Cells[findGVcolumn("Level 1 Max Current")].Text;;
            tbLevel1MaxVoltage.Text = gvRow.Cells[findGVcolumn("Level 1 Max Voltage")].Text;
            tbLevel1MaxPower.Text = gvRow.Cells[findGVcolumn("Level 1 Max Power")].Text; 
            tbLevel2MaxCurrent.Text = gvRow.Cells[findGVcolumn("Level 2 Max Current")].Text; 
            tbLevel2MaxVoltage.Text = gvRow.Cells[findGVcolumn("Level 2 Max Voltage")].Text;
            tbLevel2MaxPower.Text = gvRow.Cells[findGVcolumn("Level 2 Max Power")].Text;
            tbLevel3MaxCurrent.Text = gvRow.Cells[findGVcolumn("Level 3 Max Current")].Text;
            tbLevel3MaxVoltage.Text = gvRow.Cells[findGVcolumn("Level 3 Max Voltage")].Text;
            tbLevel3MaxPower.Text = gvRow.Cells[findGVcolumn("Level 3 Max Power")].Text;
            tbBatteryCapacity.Text = gvRow.Cells[findGVcolumn("Battery Capacity")].Text;
            ddlYear.SelectedValue = gvRow.Cells[findGVcolumn("Year")].Text;
            string strEMID = gvRow.Cells[findGVcolumn("ID")].Text;
            // TO LOAD IMAGE            
            imageEVModel.ImageUrl = "~/Editor/ShowImage.ashx?EMID=" + strEMID;
        }

        protected int findGVcolumn(string Name)
        {
            for (int j = 0; j < gvEVModel.Columns.Count; j++) // Cycle through all Columns of gridview
            {
                if (gvEVModel.Columns[j].HeaderText == Name)
                    return j;
            }
            return -1;
        }

        protected void ClearAllTbs()
        {
            gvEVModel.SelectedIndex = -1;

            btnUpdate.Visible = false;
            ddlManufacturer.SelectedIndex = 0;
            ddlYear.SelectedIndex = 0;
            tbModel.Text = string.Empty;
            tbLevel1MaxCurrent.Text = string.Empty;
            tbLevel1MaxVoltage.Text = string.Empty;
            tbLevel1MaxPower.Text = string.Empty;
            tbLevel2MaxCurrent.Text = string.Empty;
            tbLevel2MaxVoltage.Text = string.Empty;
            tbLevel2MaxPower.Text = string.Empty;
            tbLevel3MaxCurrent.Text = string.Empty;
            tbLevel3MaxVoltage.Text = string.Empty;
            tbLevel3MaxPower.Text = string.Empty;
            tbBatteryCapacity.Text = string.Empty;
            imageEVModel.ImageUrl = string.Empty;
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
        #endregion

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

        #region Update/New Validation CHecks

        protected bool blnUpdateValidation()
        {

            return false;
        }

        #endregion

        #region btnClicks
        protected void btnHideCatchError_Click(object sender, EventArgs e)
        {
            lblCatchError.Visible = false;
            btnHideCatchError.Visible = false;
        }

        protected void btnUpdateClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; SqlDataReader readerProfile = null;
            GridViewRow gvRow = gvEVModel.Rows[gvEVModel.SelectedIndex];
            bool blnFlag = false;

            FileUpload img = (FileUpload)fileUploadLocationPic;
            Byte[] imgByte = null;
            bool blnByteExists = false;

            bool blnPassesImageValidation = true; // Checks that file is an image, and also that it is < 1 MB

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
                blnByteExists = true;

                if (imgByte.Length / 1024f / 1024f > 1)
                {
                    lblFileUploadError.Text += "Size must be less than 1 MB.";
                    blnPassesImageValidation = false;
                }
            }
            else
            {
                // If an image was previewed previously, then add the image to the database.
                if (imageEVModel.ImageUrl.IndexOf("ShowImage") == -1)
                {
                    blnByteExists = true;

                    // Get Path of Image + root path
                    System.Drawing.Image imgPreviewImg = System.Drawing.Image.FromFile(MapPath(System.IO.Path.GetFileName(imageEVModel.ImageUrl.ToLower())));

                    // Convert the image into a byte array, which we will add into the database later.
                    imgByte = imageToByteArray(imgPreviewImg);
                }
            }

            // Replace the current image with the "No Image" picture to the database
            if (cbClearImage.Checked)
            {
                blnByteExists = true;
                System.Drawing.Image imgNoImage = System.Drawing.Image.FromFile(Server.MapPath(strPathToNoImage));

                // Convert Image to a byte array
                imgByte = imageToByteArray(imgNoImage);
            }

            if (!blnPassesImageValidation) // If the image doesn't pass validation, stop function
            {
                PopUpMessage("Error while updating");
                // Stop here if there is an issue.
                return;
            }

            try
            {
                strQuery = "UPDATE [EV Model] SET Manufacturer = @Manufacturer, Model = @Model, Level1MaxCurrent = @Level1MaxCurrent, [Level1MaxVoltage] = @Level1MaxVoltage, [Level1MaxPower] = @Level1MaxPower, [Year] = @Year," +
                           " [Level2MaxCurrent] = @Level2MaxCurrent, [Level2MaxVoltage] = @Level2MaxVoltage, [Level2MaxPower] = @Level2MaxPower, [Level3MaxCurrent] = @Level3MaxCurrent, [Level3MaxVoltage] = @Level3MaxVoltage, [Level3MaxPower] = @Level3MaxPower, " +
                           " [BatteryCapacity] = @BatteryCapacity ";

                if (blnByteExists)
                {
                    strQuery += ", [ModelImage] = @LocImage ";
                }
                strQuery += " WHERE [ID] = @EVID";
                cmd = new SqlCommand(strQuery, cnn);
                cnn.Open();

                if (blnByteExists)
                {
                    SqlParameter ParamLocImage = new SqlParameter();
                    ParamLocImage.ParameterName = "@LocImage";
                    ParamLocImage.Value = imgByte;
                    cmd.Parameters.Add(ParamLocImage);
                }

                SqlParameter ParamManufacturer = new SqlParameter();
                ParamManufacturer.ParameterName = "@Manufacturer";
                ParamManufacturer.Value = ddlManufacturer.SelectedValue;
                cmd.Parameters.Add(ParamManufacturer);

                cmd.Parameters.Add(new SqlParameter("@Year", ddlYear.SelectedValue));

                SqlParameter ParamModel = new SqlParameter();
                ParamModel.ParameterName = "@Model";
                ParamModel.Value = tbModel.Text;
                cmd.Parameters.Add(ParamModel);

                SqlParameter ParamLevel1MaxCurrent = new SqlParameter();
                ParamLevel1MaxCurrent.ParameterName = "@Level1MaxCurrent";
                ParamLevel1MaxCurrent.Value = tbLevel1MaxCurrent.Text;  
                cmd.Parameters.Add(ParamLevel1MaxCurrent);

                SqlParameter ParamLevel1MaxVoltage = new SqlParameter();
                ParamLevel1MaxVoltage.ParameterName = "@Level1MaxVoltage";
                ParamLevel1MaxVoltage.Value = tbLevel1MaxVoltage.Text;  
                cmd.Parameters.Add(ParamLevel1MaxVoltage);

                SqlParameter ParamLevel1MaxPower = new SqlParameter();
                ParamLevel1MaxPower.ParameterName = "@Level1MaxPower";
                ParamLevel1MaxPower.Value = tbLevel1MaxPower.Text;
                cmd.Parameters.Add(ParamLevel1MaxPower);

                SqlParameter ParamLevel2MaxCurrent = new SqlParameter();
                ParamLevel2MaxCurrent.ParameterName = "@Level2MaxCurrent";
                ParamLevel2MaxCurrent.Value = tbLevel2MaxCurrent.Text;
                cmd.Parameters.Add(ParamLevel2MaxCurrent);

                SqlParameter ParamLevel2MaxVoltage = new SqlParameter();
                ParamLevel2MaxVoltage.ParameterName = "@Level2MaxVoltage";
                ParamLevel2MaxVoltage.Value = tbLevel2MaxVoltage.Text;
                cmd.Parameters.Add(ParamLevel2MaxVoltage);

                SqlParameter ParamLevel2MaxPower = new SqlParameter();
                ParamLevel2MaxPower.ParameterName = "@Level2MaxPower";
                ParamLevel2MaxPower.Value = tbLevel2MaxPower.Text;
                cmd.Parameters.Add(ParamLevel2MaxPower);

                SqlParameter ParamLevel3MaxCurrent = new SqlParameter();
                ParamLevel3MaxCurrent.ParameterName = "@Level3MaxCurrent";
                ParamLevel3MaxCurrent.Value = tbLevel3MaxCurrent.Text;  
                cmd.Parameters.Add(ParamLevel3MaxCurrent);

                SqlParameter ParamLevel3MaxVoltage = new SqlParameter();
                ParamLevel3MaxVoltage.ParameterName = "@Level3MaxVoltage";
                ParamLevel3MaxVoltage.Value = tbLevel3MaxVoltage.Text;
                cmd.Parameters.Add(ParamLevel3MaxVoltage);

                SqlParameter ParamLevel3MaxPower = new SqlParameter();
                ParamLevel3MaxPower.ParameterName = "@Level3MaxPower";
                ParamLevel3MaxPower.Value = tbLevel3MaxPower.Text;
                cmd.Parameters.Add(ParamLevel3MaxPower);

                SqlParameter ParamBatteryCapacity = new SqlParameter();
                ParamBatteryCapacity.ParameterName = "@BatteryCapacity";
                ParamBatteryCapacity.Value = tbBatteryCapacity.Text;
                cmd.Parameters.Add(ParamBatteryCapacity);

                SqlParameter ParamEVID = new SqlParameter();
                ParamEVID.ParameterName = "@EVID";
                ParamEVID.Value = gvRow.Cells[findGVcolumn("ID")].Text;
                cmd.Parameters.Add(ParamEVID);

                readerProfile = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                ShowError("Error at btnUpdateClick: " + ex.Message);
                blnFlag = true;
            }
            finally
            {
                if(readerProfile != null)
                    readerProfile.Close();
                if(cnn != null)
                    cnn.Close();
                
            }
            if (blnFlag)
            {
                PopUpMessage("Error");
                return;
            }


            voidPopulategvEVModel();
            ClearAllTbs();
            hidecbClearImage();
            PopUpMessage("Updated");
        }

        protected void btnNewClick(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(connectionString);
            string strQuery;
            SqlCommand cmd; SqlDataReader readerProfile = null;
            
            bool blnByteExists = false;
            bool blnPassesImageValidation = true; // Checks that file is an image, and also that it is < 1 MB
            FileUpload img = (FileUpload)fileUploadLocationPic;
            Byte[] imgByte = null;
            bool blnFlag = false;
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
                
                if(!string.IsNullOrWhiteSpace(imageEVModel.ImageUrl) && imageEVModel.ImageUrl.IndexOf("ShowImage") == -1)
                {                    
                    // Get Path of Image + root path
                    System.Drawing.Image imgPreviewImg = System.Drawing.Image.FromFile(MapPath(System.IO.Path.GetFileName(imageEVModel.ImageUrl.ToLower())));

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
                strQuery = "INSERT INTO [EV Model]( Manufacturer, Model, Level1MaxCurrent, Level1MaxVoltage, Level1MaxPower, Level2MaxCurrent, Level2MaxVoltage, Level2MaxPower, Level3MaxCurrent, Level3MaxVoltage, Level3MaxPower, BatteryCapacity, Year "; 
                           
                if (blnByteExists)
                {
                    strQuery += ", [ModelImage]) ";
                }
                else
                {
                    strQuery += ")";
                }
                strQuery += " VALUES( @Manufacturer, @Model, @Level1MaxCurrent, @Level1MaxVoltage, @Level1MaxPower, @Level2MaxCurrent, @Level2MaxVoltage, @Level2MaxPower, @Level3MaxCurrent, @Level3MaxVoltage, @Level3MaxPower, @BatteryCapacity, @Year ";

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
                    SqlParameter ParamLocImage = new SqlParameter();
                    ParamLocImage.ParameterName = "@LocImage";
                    ParamLocImage.Value = imgByte;
                    cmd.Parameters.Add(ParamLocImage);
                }

                SqlParameter ParamManufacturer = new SqlParameter();
                ParamManufacturer.ParameterName = "@Manufacturer";
                ParamManufacturer.Value = ddlManufacturer.SelectedValue;
                cmd.Parameters.Add(ParamManufacturer);

                cmd.Parameters.Add(new SqlParameter("@Year", ddlYear.SelectedValue));

                SqlParameter ParamModel = new SqlParameter();
                ParamModel.ParameterName = "@Model";
                ParamModel.Value = tbModel.Text;
                cmd.Parameters.Add(ParamModel);

                SqlParameter ParamLevel1MaxCurrent = new SqlParameter();
                ParamLevel1MaxCurrent.ParameterName = "@Level1MaxCurrent";
                ParamLevel1MaxCurrent.Value = tbLevel1MaxCurrent.Text;  
                cmd.Parameters.Add(ParamLevel1MaxCurrent);

                SqlParameter ParamLevel1MaxVoltage = new SqlParameter();
                ParamLevel1MaxVoltage.ParameterName = "@Level1MaxVoltage";
                ParamLevel1MaxVoltage.Value = tbLevel1MaxVoltage.Text;  
                cmd.Parameters.Add(ParamLevel1MaxVoltage);

                SqlParameter ParamLevel1MaxPower = new SqlParameter();
                ParamLevel1MaxPower.ParameterName = "@Level1MaxPower";
                ParamLevel1MaxPower.Value = tbLevel1MaxPower.Text;
                cmd.Parameters.Add(ParamLevel1MaxPower);


                SqlParameter ParamLevel2MaxCurrent = new SqlParameter();
                ParamLevel2MaxCurrent.ParameterName = "@Level2MaxCurrent";
                ParamLevel2MaxCurrent.Value = tbLevel2MaxCurrent.Text;
                cmd.Parameters.Add(ParamLevel2MaxCurrent);

                SqlParameter ParamLevel2MaxVoltage = new SqlParameter();
                ParamLevel2MaxVoltage.ParameterName = "@Level2MaxVoltage";
                ParamLevel2MaxVoltage.Value = tbLevel2MaxVoltage.Text;
                cmd.Parameters.Add(ParamLevel2MaxVoltage);

                SqlParameter ParamLevel2MaxPower = new SqlParameter();
                ParamLevel2MaxPower.ParameterName = "@Level2MaxPower";
                ParamLevel2MaxPower.Value = tbLevel2MaxPower.Text;
                cmd.Parameters.Add(ParamLevel2MaxPower);

                SqlParameter ParamLevel3MaxCurrent = new SqlParameter();
                ParamLevel3MaxCurrent.ParameterName = "@Level3MaxCurrent";
                ParamLevel3MaxCurrent.Value = tbLevel3MaxCurrent.Text;  
                cmd.Parameters.Add(ParamLevel3MaxCurrent);

                SqlParameter ParamLevel3MaxVoltage = new SqlParameter();
                ParamLevel3MaxVoltage.ParameterName = "@Level3MaxVoltage";
                ParamLevel3MaxVoltage.Value = tbLevel3MaxVoltage.Text;
                cmd.Parameters.Add(ParamLevel3MaxVoltage);

                SqlParameter ParamLevel3MaxPower = new SqlParameter();
                ParamLevel3MaxPower.ParameterName = "@Level3MaxPower";
                ParamLevel3MaxPower.Value = tbLevel3MaxPower.Text;
                cmd.Parameters.Add(ParamLevel3MaxPower);

                SqlParameter ParamBatteryCapacity = new SqlParameter();
                ParamBatteryCapacity.ParameterName = "@BatteryCapacity";
                ParamBatteryCapacity.Value = tbBatteryCapacity.Text;
                cmd.Parameters.Add(ParamBatteryCapacity);

                readerProfile = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                ShowError("Error at btnNewClick: " + ex.Message);
                blnFlag = true;
            }
            finally
            {
                if (readerProfile != null)
                    readerProfile.Close();
                if (cnn != null)
                    cnn.Close();
            }

            if (blnFlag)
            {
                PopUpMessage("Error");
                return;
            }

            voidPopulategvEVModel();
            ClearAllTbs();
            hidecbClearImage();
            PopUpMessage("New EV information added.");
        }

        protected void btnClearClick(object sender, EventArgs e)
        {
            ClearAllTbs();
            hidecbClearImage();
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
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
                    imageEVModel.ImageUrl = System.IO.Path.GetFileName(File.FileName).ToLower().ToString();

                }
            }
            else
            {
                PopUpMessage("No Image selected!");
                return;
            }
        }
        #endregion
    }
}