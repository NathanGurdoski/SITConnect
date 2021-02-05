using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SITConnect
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        static string NewHash;
        static string NewSalt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["OTPVerified"] != null && (string)Session["OTPVerified"] == "true" && Session["EmailforOTP"] != null)
            {
                lblEmail.Text = (string)Session["EmailforOTP"];
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string new_p1 = HttpUtility.HtmlEncode(tb_new_pwd1.Text.ToString().Trim());
            string new_p2 = HttpUtility.HtmlEncode(tb_new_pwd2.Text.ToString().Trim());
            string errorMsg = null;

            if (string.IsNullOrEmpty(new_p1) || string.IsNullOrEmpty(new_p2))
            {
                errorMsg = "Please fill up all input fields";
            }
            else
            {
                if (Regex.IsMatch(new_p1, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{8,}$"))
                {
                    if (new_p2 == new_p1)
                    {
                        string email = lblEmail.Text;
                        string dbHash = getDBHash(email);
                        string dbSalt = getDBSalt(email);
                        SHA512Managed hashing = new SHA512Managed();

                        if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                        {
                            string pwdWithSalt = new_p1 + dbSalt;
                            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                            string userHash = Convert.ToBase64String(hashWithSalt);
                            if (userHash.Equals(dbHash))
                            {
                                //User actually rememebers password. Tries to reuse the same password because it was expired.
                                errorMsg = "Action not allowed. Old password cannot be reused for up to 2 new password changes.";
                            }
                            else
                            {
                                string dbHashGen1 = getHashSaltByGen(email, "hash", 1);
                                string dbSaltGen1 = getHashSaltByGen(email, "salt", 1);
                                string dbHashGen2 = getHashSaltByGen(email, "hash", 2);
                                string dbSaltGen2 = getHashSaltByGen(email, "salt", 2);

                                if (dbSaltGen1 != null && dbSaltGen1.Length > 0 && dbHashGen1 != null && dbHashGen1.Length > 0)
                                {
                                    pwdWithSalt = new_p1 + dbSaltGen1;
                                    hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                    userHash = Convert.ToBase64String(hashWithSalt);
                                    if (userHash.Equals(dbHashGen1))
                                    {
                                        //Password has been used before
                                        errorMsg = "Action not allowed. Old password cannot be reused for up to 2 new password changes.";
                                    }
                                    else
                                    {
                                        //Pwd gen1 was either set up during registration or after a password change
                                        //check if Pwd gen 2 is null. If true, pwd is going to be forgotten before it had been changed before.
                                        if (dbSaltGen2 != null && dbSaltGen2.Length > 0 && dbHashGen2 != null && dbHashGen2.Length > 0)
                                        {
                                            pwdWithSalt = new_p1 + dbSaltGen2;
                                            hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                            userHash = Convert.ToBase64String(hashWithSalt);
                                            if (userHash.Equals(dbHashGen2))
                                            {
                                                //Password has been used before
                                                errorMsg = "Action not allowed. Old password cannot be reused for up to 2 new password changes.";
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            errorMsg = "Action not allowed. Old password cannot be reused for up to 2 new password changes.";
                        }
                    }
                    else
                    {
                        errorMsg = "New passwords do not match. Please try again.";
                    }
                }
                else
                {
                    errorMsg = "Failed to meet password criteria! <br/> Must be at least 8 characters and contain uppercase and lowercase letters, digits and special characters.";
                }


            }

            if (errorMsg == null)
            {
                resetPwd(lblEmail.Text, new_p1);
                Session.Clear();
                Session.Abandon();
                Session.RemoveAll();
                Response.Redirect("Login.aspx", false);
            }
            else
            {
                lbl_error_pwd.Text = errorMsg;
            }
        }

        protected void resetPwd(string email, string newPass)
        {
            //Generate random "salt"
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];
            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            NewSalt = Convert.ToBase64String(saltByte);
            SHA512Managed hashing = new SHA512Managed();
            string pwdWithSalt = newPass + NewSalt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(newPass));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            NewHash = Convert.ToBase64String(hashWithSalt);

            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "UPDATE Account SET PasswordHash=@PasswordHash,PasswordSalt=@PasswordSalt,PwdSetDateTime=@PwdSetDateTime WHERE Email=@Email";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PasswordHash", NewHash);
            command.Parameters.AddWithValue("@PasswordSalt", NewSalt);
            command.Parameters.AddWithValue("@PwdSetDateTime", DateTime.Now);
            command.Parameters.AddWithValue("@Email", email);


            try
            {
                connection.Open();
                command.ExecuteReader();
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
        }

        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT PasswordHash FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
            return h;
        }

        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT PasswordSalt FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordSalt"] != null)
                        {
                            if (reader["PasswordSalt"] != DBNull.Value)
                            {
                                s = reader["PasswordSalt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
            return s;
        }

        protected string getHashSaltByGen(string email, string item, int gen)
        {
            string r = null;
            string dbHashGen1 = null;
            string dbSaltGen1 = null;
            string dbHashGen2 = null;
            string dbSaltGen2 = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT FirstGenPwdHash, FirstGenPwdSalt, SecGenPwdHash, SecGenPwdSalt FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["FirstGenPwdHash"] != null)
                        {
                            if (reader["FirstGenPwdHash"] != DBNull.Value)
                            {
                                dbHashGen1 = reader["FirstGenPwdHash"].ToString();
                            }
                        }
                        if (reader["FirstGenPwdSalt"] != null)
                        {
                            if (reader["FirstGenPwdSalt"] != DBNull.Value)
                            {
                                dbSaltGen1 = reader["FirstGenPwdSalt"].ToString();
                            }
                        }
                        if (reader["SecGenPwdHash"] != null)
                        {
                            if (reader["SecGenPwdHash"] != DBNull.Value)
                            {
                                dbHashGen2 = reader["SecGenPwdHash"].ToString();
                            }
                        }
                        if (reader["SecGenPwdSalt"] != null)
                        {
                            if (reader["SecGenPwdSalt"] != DBNull.Value)
                            {
                                dbSaltGen2 = reader["SecGenPwdSalt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }

            if (item == "hash" && gen == 1)
            {
                r = dbHashGen1;
            }
            else if (item == "hash" && gen == 2)
            {
                r = dbHashGen2;
            }
            else if (item == "salt" && gen == 1)
            {
                r = dbSaltGen1;
            }
            else if (item == "salt" && gen == 2)
            {
                r = dbSaltGen2;
            }
            return r;
        }
    }
}