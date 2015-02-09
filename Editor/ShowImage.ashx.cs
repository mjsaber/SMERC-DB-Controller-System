using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;

//namespace RTMC.Editor
namespace EVEditor
{
    /// <summary>
    /// Summary description for ShowImage
    /// </summary>
    public class ShowImage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string PLID = "";
            string EMID = "";
            if (context.Request.QueryString["PLID"] != null)
                PLID = (context.Request.QueryString["PLID"]);
            else if (context.Request.QueryString["EMID"] != null)
                EMID = (context.Request.QueryString["EMID"]);
            else
                throw new ArgumentException("No parameter specified");

            context.Response.ContentType = "image/png";

            if (PLID != string.Empty)
                context.Response.BinaryWrite(ShowEmpImage(PLID));
            else if (EMID != string.Empty)
                context.Response.BinaryWrite(ShowEmImage(EMID));
            
        }

        public byte[] ShowEmpImage(string PLID)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            
            //string conn = ConfigurationManager.ConnectionStrings["EmployeeConnString"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            string sql = "SELECT [ChargingBoxLocationImage] FROM [Parking Lot] WHERE [ID] = @PLID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@PLID", PLID);
            connection.Open();
            object img = cmd.ExecuteScalar();
            try
            {
                //return new MemoryStream((byte[])img);
                return (byte[])img;
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public byte[] ShowEmImage(string EMID)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            //string conn = ConfigurationManager.ConnectionStrings["EmployeeConnString"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            string sql = "SELECT [ModelImage] FROM [EV Model] WHERE [ID] = @EMID";

            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@EMID", EMID);
            connection.Open();
            object img = cmd.ExecuteScalar();
            try
            {
                return (byte[])img;
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}