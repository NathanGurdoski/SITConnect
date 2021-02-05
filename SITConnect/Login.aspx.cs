using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SITConnect.Registration;

namespace SITConnect
{
    public partial class Login : System.Web.UI.Page
    {

        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        int attempt;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["EmailforOTP"] != null)
            {
                Session.Remove("EmailforOTP");
                if (Session["OTPVerified"] != null && (string)Session["OTPVerified"] == "true")
                {
                    Session.Remove("OTPVerified");
                }
            }

        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            /* User authentication flow:
 
                @Check if both fields are empty.
                - If yes, output error mesg.
                - #If no, check if email is valid.
                    - #If yes, check if account is locked
                        - if yes, output error msg
                        - #if no, check if email has previously been entered before. (i.e. stored in session)
                            -#if yes, *** check if password has expired
                                -if yes: error msg.
                                -#if no: check if password is valid.
                                 - #If yes, check if Captcha returns true
                                    - #If yes, logs in user successfully
                                    - If no, output error msg.
                                 - If no, output error msg. decrement attempt by 1. Check if attempt = 0.
                                    - If yes, lock account.
                                    - If no, repeat @ on btn event.
                            - If no, store new email in session with attempt counter. then go to ***.
                    - If no, output error msg

                Implementation:
                - Refreshing the page will not restart the attempt counter. Only closing the tab/application will do so.
                - Any valid email that is entered with the wrong password will be stored as session data to keep track of the no. of attempts for that account.
                - If account is locked out, don't bother checking for password.
                - Invalid email is not counted as a login attempt.
                - No password but valid email is also not considered login attempt.
                - If user did not change expired password before logout, user will have to forget and reset password in next login.

            */
            string pwd = HttpUtility.HtmlEncode(tb_pwd.Text.ToString().Trim());
            string email = HttpUtility.HtmlEncode(tb_email.Text.ToString().Trim());
            bool loginSuccess = false;
            string errorMsg = null;

            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(email))
            {
                errorMsg = "Please fill up both input fields.";
            }
            else
            {
                if (ValidateEmail(email) == null)
                    errorMsg = "Account does not exist. Please register account first.";
                else
                {
                    if (Session[email] == null)
                    {
                        Session[email] = 3;
                    }

                    string lockoutDate = getLockoutStatus(email);
                    if (!string.IsNullOrEmpty(lockoutDate))
                    {
                        int distance = DateTime.Compare(DateTime.Now, DateTime.Parse(lockoutDate));
                        if (distance < 0)
                            errorMsg = "Account is temporarily locked.";
                        else
                        {
                            errorMsg = "Account lockout has been removed. Please try to login again.";
                            Session[email] = 3;
                            ManageLockAccount(email, "unlock");
                        }
                    }
                    else
                    {
                        string PwdSetDateTime = getPwdSetDateTime(email);
                        int distance = DateTime.Compare(DateTime.Now, DateTime.Parse(PwdSetDateTime).AddMinutes(15));
                        if (distance > 0)
                        {
                            //FIXME: Forget password
                            errorMsg = "Your password has expired. Please select \"Forget Password\" to reset your password.";
                        }
                        else
                        {
                            SHA512Managed hashing = new SHA512Managed();
                            string dbHash = getDBHash(email);
                            string dbSalt = getDBSalt(email);
                            if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                            {
                                string pwdWithSalt = pwd + dbSalt;
                                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                string userHash = Convert.ToBase64String(hashWithSalt);
                                if (!userHash.Equals(dbHash))
                                {
                                    attempt = (int)Session[email];
                                    attempt -= 1;
                                    Session[email] = attempt;
                                    if (attempt <= 0)
                                    {
                                        errorMsg = "You have reached your attempt limit. <br/> Your account has been locked temporarily.";
                                        ManageLockAccount(email, "lock");
                                    }
                                    else
                                        errorMsg = "Wrong password. Please try again. <br/> Login Attempts Left: " + attempt.ToString();
                                    //FIXME: Should I be displaying the same error msg as invalid email? Should I show number of attempts?
                                }
                                else
                                {
                                    if (ValidateCaptcha())
                                        loginSuccess = true;
                                    else
                                        errorMsg = "Captcha unsuccessful";
                                }
                            }

                        }

                    }
                }
            }
            lbl_error_login.Text = errorMsg;

            if (loginSuccess)
            {
                //Implement Session Fixation Prevention
                Session["LoggedIn"] = email;
                //create a new GUID and save into the session
                string guid = Guid.NewGuid().ToString();
                Session["AuthToken"] = guid;

                //now create a new cookie with this guid value
                Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                Response.Redirect("Home.aspx", false);
                updateLastLogin(email);
            }
        }

        protected void ManageLockAccount(string email, string action)
        {

            //Update DateTimeLockout in database to five minutes from now.
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "UPDATE Account SET DateTimeLockout=@DateTime WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            //FIXED: Is this considered automatic lockout? Yes because admin don't have to take action to unlock account.
            if (action == "lock")
            {
                command.Parameters.AddWithValue("@DateTime", DateTime.Now.AddMinutes(5));
            }
            else
            {
                command.Parameters.AddWithValue("@DateTime", DBNull.Value);
            }

            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                command.ExecuteReader();
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
        }

        protected void updateLastLogin(string email)
        {
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "UPDATE Account SET LastLogin=@DateTime WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DateTime", DateTime.Now);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                command.ExecuteReader();
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
        }

        protected string ValidateEmail(string email)
        {
            string e = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT Email FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Email"] != null)
                        {
                            //Email exists in database
                            if (reader["Email"] != DBNull.Value)
                            {
                                e = reader["Email"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex) { throw new Exception(ex.ToString()); }
            finally { connection.Close(); }
            return e;
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

        protected string getLockoutStatus(string email)
        {
            string dt = null;
            SqlConnection connection = new SqlConnection(SITConnectDB);
            string sql = "SELECT DateTimeLockout FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["DateTimeLockout"] != null)
                        {
                            dt = reader["DateTimeLockout"].ToString();
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

        //Google Captcha
        public bool ValidateCaptcha()
        {
            bool result = false;
            //When use submits the recaptcha form, the user gets a response POST parameter
            //captchaResponse consist of the user click pattern. Behaviour analytics! AI :)
            string captchaResponse = Request.Form["g-recaptcha-response"];

            //To send a GET request to Google along with the response and Secret Key.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
                ("https://www.google.com/recaptcha/api/siteverify?secret=6LebSEsaAAAAAMIVH-nAlmqeuHkdx6ILBI0hdV6B &response=" + captchaResponse);

            try
            {
                //Codes to receive the Response in JSON format from Google Server
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        //The response in JSON format
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        //Create jsonObject to handle the response e.g success or Error
                        //Deserialize JSON
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        //Convert the string "False" to bool false or "True" to bool true
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return result;
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registration.aspx", false);
        }

        protected void btnForget_Click(object sender, EventArgs e)
        {
            Response.Redirect("ForgetPassword.aspx", false);
        }

    }
}