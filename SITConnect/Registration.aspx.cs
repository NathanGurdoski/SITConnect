using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class Registration : System.Web.UI.Page
    {
        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            //Prevent XSS - Sanitize user input using HttpUtility.HtmlEncode (tb_fname);
            if (ValidateRegister(
                HttpUtility.HtmlEncode(tb_fname.Text),
                HttpUtility.HtmlEncode(tb_lname.Text),
                HttpUtility.HtmlEncode(tb_email.Text),
                HttpUtility.HtmlEncode(tb_pwd.Text),
                HttpUtility.HtmlEncode(tb_creditNo.Text),
                HttpUtility.HtmlEncode(tb_cvc.Text),
                HttpUtility.HtmlEncode(tb_expiry.Text)))
            {
                string pwd = tb_pwd.Text.ToString().Trim();
                //Generate random "salt"
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] saltByte = new byte[8];
                //Fills array of bytes with a cryptographically strong sequence of random values.
                rng.GetBytes(saltByte);
                salt = Convert.ToBase64String(saltByte);
                SHA512Managed hashing = new SHA512Managed();
                string pwdWithSalt = pwd + salt;
                byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwd));
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                finalHash = Convert.ToBase64String(hashWithSalt);
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.GenerateKey();
                Key = cipher.Key;
                IV = cipher.IV;
                createAccount();

                Response.Redirect("Login.aspx", false);
            }

        }

        //TODO: Server-side credit validation
        private bool ValidateRegister(string fname, string lname, string email, string pwd, string credit, string cvc, string exp)
        {

            bool result = true;
            //First and Last Name Validation
            if (String.IsNullOrEmpty(fname))
            {
                lbl_error_fname.Text = "Field empty";
                result = false;
            }
            else { lbl_error_fname.Text = ""; }

            if (String.IsNullOrEmpty(lname))
            {
                lbl_error_lname.Text = "Field empty";
                result = false;
            }
            else { lbl_error_lname.Text = ""; }


            //Email Validation
            if (!Regex.IsMatch(email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z"))
            {
                lbl_error_email.Text = "Invalid Email";
                result = false;
            }
            else
            {
                if (ValidateEmail(email) != null)
                {
                    lbl_error_email.Text = "An account with this email has been registered. Please login using the email.";
                    result = false;
                }
                else
                    lbl_error_email.Text = "";
            }

            //Password Validation (Server Side)
            if (!Regex.IsMatch(pwd, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{8,}$"))
            {
                lbl_error_pwd.Text = "Failed to meet password criteria! <br/>" +
                    "Must be at least 8 characters and contain uppercase and lowercase letters, digits " +
                    "and special characters.<br/> Password must also not be the same as email.";
                result = false;
            }
            else { lbl_error_pwd.Text = ""; }

            //Credit Card Validation
            if (!Regex.IsMatch(credit, "^[0-9]{16}$"))
            {
                lbl_error_creditNo.Text = "Invalid Credit No.";
                result = false;
            }
            else { lbl_error_creditNo.Text = ""; }

            if (!Regex.IsMatch(cvc, @"^[0-9]{3}$"))
            {
                lbl_error_cvc.Text = "Invalid CVC";
                result = false;
            }
            else { lbl_error_cvc.Text = ""; }

            if (!Regex.IsMatch(exp, @"(^[0-9]{2}-[0-9]{2}$)|(^[0-9]{2}/[0-9]{2}$)"))
            {
                lbl_error_expiry.Text = "Invalid Expiry Date";
                result = false;
            }
            else { lbl_error_expiry.Text = ""; }

            return result;
        }


        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        public void createAccount()
        {
            //Use try catch blocks to protect against SQLi
            try
            {
                using (SqlConnection con = new SqlConnection(SITConnectDB))
                {
                    //Use SQL Parameters to protect against SQLi
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Account VALUES(@Fname,@Lname,@Email,@PasswordHash,@PasswordSalt," +
                        "@CreditNo,@CVC,@ExpDate,@DateTimeLockout,@IV,@Key,@PwdSetDateTime," +
                        "@FirstGenPwdHash,@FirstGenPwdSalt,@SecGenPwdHash,@SecGenPwdSalt,@LastLogin)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Fname", tb_fname.Text.Trim());
                            cmd.Parameters.AddWithValue("@Lname", tb_lname.Text.Trim());
                            cmd.Parameters.AddWithValue("@Email", tb_email.Text.Trim());
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@CreditNo", Convert.ToBase64String(encryptData(tb_creditNo.Text.Trim())));
                            cmd.Parameters.AddWithValue("@CVC", Convert.ToBase64String(encryptData(tb_cvc.Text.Trim())));
                            cmd.Parameters.AddWithValue("@ExpDate", Convert.ToBase64String(encryptData(tb_expiry.Text.Trim())));
                            cmd.Parameters.AddWithValue("@DateTimeLockout", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@PwdSetDateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("@FirstGenPwdHash", finalHash);
                            cmd.Parameters.AddWithValue("@FirstGenPwdSalt", salt);
                            cmd.Parameters.AddWithValue("@SecGenPwdHash", DBNull.Value);
                            cmd.Parameters.AddWithValue("@SecGenPwdSalt", DBNull.Value);
                            cmd.Parameters.AddWithValue("@LastLogin", DBNull.Value);

                            try
                            {
                                cmd.Connection = con;
                                con.Open();
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.ToString());
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
               plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
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

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx", false);
        }
    }
}