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
    public partial class ChangePassword : System.Web.UI.Page
    {
        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        static string NewHash;
        static string NewSalt;
        string email = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            //check session is still alive. Can copy from home.
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    email = (string)Session["LoggedIn"];
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            /*Check if current password is 5 min from setDateTime
             * if no: error msg
             * if yes: Check if old p matches currentp in db
                     - if no: error msg
                     - if yes: check if new_p meets pass policy
                        - if no: error msg
                        - if yes: check if new_p matches new_p_reenter
                            - if no: error msg
                            - if yes: check if new_p = old_P
                                - if yes: error msg
                                - if no: check if new_p = gen_1_p or gen_2_p
                                        - if yes: error msg
                                        - if no: check if gen_2_p is null
                                            - if yes: gen_2_p = new_p; then go to #
                                            - if no: check if gen_2_p = current_p
                                                - if yes: go to #
                                                - if  no: gen_1_p = gen_2_p; gen_2_p = current_p; then go to #

                                            #current_p = new_p; perf orm db update. notify user and redirect to home 
                     
                
             */
            string old_p = HttpUtility.HtmlEncode(tb_old_pwd.Text.ToString().Trim());
            string new_p1 = HttpUtility.HtmlEncode(tb_new_pwd1.Text.ToString().Trim());
            string new_p2 = HttpUtility.HtmlEncode(tb_new_pwd2.Text.ToString().Trim());
            bool changeSuccess = false;
            string errorMsg = null;
            string pwdSetDateTime = getPwdSetDateTime(email);
            int distance = DateTime.Compare(DateTime.Now, DateTime.Parse(pwdSetDateTime).AddMinutes(5));
            if (distance < 0)
                errorMsg = "You are not allowed to change your password now.<br/>Password needs to have a minimum age of 5 minutes.";
            else
            {
                if (string.IsNullOrEmpty(old_p) || string.IsNullOrEmpty(new_p1) || string.IsNullOrEmpty(new_p2))
                {
                    errorMsg = "Please fill up all input fields";
                }
                else
                {
                    SHA512Managed hashing = new SHA512Managed();
                    string dbHash = getDBHash(email);
                    string dbSalt = getDBSalt(email);
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string pwdWithSalt = old_p + dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);
                        if (userHash.Equals(dbHash))
                        {
                            //Old password Valid OK
                            //Password Complexity (Server Side) Validation
                            if (Regex.IsMatch(new_p1, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{8,}$"))
                            {
                                //New Password Complexity OK
                                if (new_p2 == new_p1)
                                {
                                    //Password Re-enter OK
                                    if (new_p1 != old_p)
                                    {
                                        //New Password Check Phase 1 OK
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
                                                //New Password Check Phase 2 OK
                                                //Pwd gen1 was either set up during registration or after a password change
                                                //check if Pwd gen 2 is null. If true, pwd is going to be changed for the first time.
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
                                                    else
                                                    {
                                                        //New Password Check Phase 3 OK
                                                        //All Validation successful
                                                        changeSuccess = true;
                                                    }
                                                }
                                                else
                                                {
                                                    //Gen 2 is null. User changes password for first time. Checking is redundant.
                                                    changeSuccess = true;
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
                        else
                        {
                            errorMsg = "Old password is invalid. Please try again";
                        }
                    }
                }

            }

            lbl_error_pwd.Text = errorMsg;
            if (changeSuccess)
            {
                updatePassword(email, new_p1);
                //FIXED: redirect to Home.
                //FIXED: How to alert user? Use bootstrap modal. Refer to EDP.
                Response.Redirect("Home.aspx", false);
            }

        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //go back to Home
            Response.Redirect("Home.aspx", false);
        }

        protected void updatePassword(string email, string newPass)
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

            string dbHashGen1 = getHashSaltByGen(email, "hash", 1);
            string dbSaltGen1 = getHashSaltByGen(email, "salt", 1);
            string dbHashGen2 = getHashSaltByGen(email, "hash", 2);
            string dbSaltGen2 = getHashSaltByGen(email, "salt", 2);

            if (dbSaltGen2 != null && dbSaltGen2.Length > 0 && dbHashGen2 != null && dbHashGen2.Length > 0)
            {
                //Not the 1st time changing. Might be 2nd or 3rd onwards.
                //Need to check current hash and salt.
                string dbHash = getDBHash(email);
                string dbSalt = getDBSalt(email);
                if (!dbHash.Equals(dbHashGen2))
                {
                    //Not the 2nd time. Might be 3rd or 4th onwards.
                    //Overwrite existing values.
                    dbHashGen1 = dbHashGen2;
                    dbSaltGen1 = dbSaltGen2;
                    dbHashGen2 = dbHash;
                    dbSaltGen2 = dbSalt;
                }
            }
            else
            {
                //1st time changing. Overwrite null to new values.
                dbHashGen2 = NewHash;
                dbSaltGen2 = NewSalt;
            }

            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql =
                "UPDATE Account SET PasswordHash=@PasswordHash,PasswordSalt=@PasswordSalt,PwdSetDateTime=@PwdSetDateTime," +
                "FirstGenPwdHash=@FirstGenPwdHash,FirstGenPwdSalt=@FirstGenPwdSalt," +
                "SecGenPwdHash=@SecGenPwdHash,SecGenPwdSalt=@SecGenPwdSalt " +
                "WHERE Email=@Email";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PasswordHash", NewHash);
            command.Parameters.AddWithValue("@PasswordSalt", NewSalt);
            command.Parameters.AddWithValue("@PwdSetDateTime", DateTime.Now);
            command.Parameters.AddWithValue("@FirstGenPwdHash", dbHashGen1);
            command.Parameters.AddWithValue("@FirstGenPwdSalt", dbSaltGen1);
            command.Parameters.AddWithValue("@SecGenPwdHash", dbHashGen2);
            command.Parameters.AddWithValue("@SecGenPwdSalt", dbSaltGen2);
            command.Parameters.AddWithValue("@Email", email);


            try
            {
                connection.Open();
                command.ExecuteReader();
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
        }

        protected string getPwdSetDateTime(string email)
        {
            string dt = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT PwdSetDateTime FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PwdSetDateTime"] != null)
                        {
                            dt = reader["PwdSetDateTime"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
            return dt;
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