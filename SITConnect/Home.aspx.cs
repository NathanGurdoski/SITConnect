using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SITConnect
{
    public partial class Home : System.Web.UI.Page
    {
        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        byte[] Key;
        byte[] IV;
        byte[] creditNo = null;
        byte[] cvc = null;
        byte[] expDate = null;
        string email = null;
        string fullname = null;
        string PwdSetDateTime = null;
        string sessionTimeOut = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    email = (string)Session["LoggedIn"];
                    displayUserProfile(email);
                }
                else
                {
                    Response.Redirect("Login.aspx", false);
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            int distance = DateTime.Compare(DateTime.Now, DateTime.Parse(PwdSetDateTime).AddMinutes(15));
            if (distance > 0)
            {
                //Force user to change password if it has expired. If user did not close the application properly,
                // he/she will have to "forget password" in order to reset it
                lbl_pwd_expiry.Text = "Your password has expired. Please change your password before logout.";
            }
            else
            {
                Session.Clear();
                Session.Abandon();
                Session.RemoveAll();

                Response.Redirect("Login.aspx", false);
                if (Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                    Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
                }

                if (Request.Cookies["AuthToken"] != null)
                {
                    Response.Cookies["AuthToken"].Value = string.Empty;
                    Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
                }
            }
        }

        protected string decryptData(byte[] cipherText)
        {
            string plainText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                //Create the stremas used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptTransform, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            //Read the decrypted bytes from the decrypting stream
                            //and place them in a string
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return plainText;
        }

        protected void displayUserProfile(string email)
        {
            Console.WriteLine(email);
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT * FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Fname"] != DBNull.Value && reader["Lname"] != DBNull.Value)
                        {
                            fullname = reader["Fname"].ToString() + " " + reader["Lname"].ToString();
                        }
                        if (reader["CreditNo"] != DBNull.Value && reader["CVC"] != DBNull.Value && reader["ExpDate"] != DBNull.Value)
                        {
                            creditNo = Convert.FromBase64String(reader["CreditNo"].ToString());
                            cvc = Convert.FromBase64String(reader["CVC"].ToString());
                            expDate = Convert.FromBase64String(reader["ExpDate"].ToString());
                        }
                        if (reader["IV"] != DBNull.Value)
                        {
                            IV = Convert.FromBase64String(reader["IV"].ToString());
                        }
                        if (reader["Key"] != DBNull.Value)
                        {
                            Key = Convert.FromBase64String(reader["Key"].ToString());
                        }
                        if (reader["PwdSetDateTime"] != DBNull.Value)
                        {
                            PwdSetDateTime = reader["PwdSetDateTime"].ToString();
                        }
                        if (reader["LastLogin"] != DBNull.Value)
                        {
                            sessionTimeOut = reader["LastLogin"].ToString();
                        }

                    }
                    lblSessionTimeOut.Text = DateTime.Parse(sessionTimeOut).AddHours(1).ToString();
                    int distance = DateTime.Compare(DateTime.Now, DateTime.Parse(PwdSetDateTime).AddMinutes(15));
                    if (distance > 0)
                    {
                        lbl_pwd_expiry.Text = "Your password has expired. Please change your password before logout.";
                    }
                    else
                    {
                        lbl_pwd_expiry.Text = "Your password will expire on: " + DateTime.Parse(PwdSetDateTime).AddMinutes(15).ToString();
                    }
                    lblMessage.Text = "Welcome back, " + fullname + "!";
                    lbl_creditNo.Text = decryptData(creditNo);
                    lbl_cvc.Text = decryptData(cvc);
                    lbl_expiry.Text = decryptData(expDate);
                }
            }//try ends here
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            Response.Redirect("ChangePassword.aspx", false);
        }
    }
}