using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SITConnect.Registration;

namespace SITConnect
{
    public partial class ForgetPassword : System.Web.UI.Page
    {
        string SITConnectDB = System.Configuration.ConfigurationManager.ConnectionStrings["SITConnectDB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["OTP"] == null && Session["Timer"] == null && Session["OTPVerified"] != null && Session["EmailforOTP"] != null)
            {
                //User tried to navigate back after OTP was verified. User will have to request for new OTP.
                Session.Remove("EmailforOTP");
                Session.Remove("OTPVerified");
                tb_email.Text = "";
                tb_otp.Text = "";
                RequestOTP.Visible = true;
                VerifyOTP.Visible = false;

            }
        }

        protected void btnBack1_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx", false);
        }

        protected void btnRequestOTP_Click(object sender, EventArgs e)
        {
            string email = HttpUtility.HtmlEncode(tb_email.Text);
            if (string.IsNullOrEmpty(email))
            {
                lbl_error_email.Text = "Field empty";
                lbl_error_email.ForeColor = Color.Red;
            }
            else if (!Regex.IsMatch(tb_email.Text, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z"))
            {
                lbl_error_email.Text = "Email is in incorrect format.";
                lbl_error_email.ForeColor = Color.Red;
            }
            else
            {
                if (ValidateCaptcha())
                {
                    lbl_error_email.Text = "Email confirmed. We will send an email to you if it is a valid account.<br/>Please check your email inbox/ spam folder.<br/>";
                    lbl_error_email.ForeColor = Color.Green;
                    if (ValidateEmail(email) != null)
                    {
                        //Email is valid
                        try
                        {
                            using (MailMessage mail = new MailMessage())
                            {
                                int rand = new Random().Next(100000, 999999);
                                string OTP = rand.ToString();
                                Session["OTP"] = OTP;

                                mail.From = new MailAddress("sitconnet2021@gmail.com");
                                mail.To.Add(email);
                                mail.Subject = "SITConnect - Password Reset Request";
                                mail.Body = "<p style=\"color:#000000;margin:10px 0;padding:0;font-family:Helvetica;font-size:16px;line-height:150%;text-align:left\">" +
                                    "Hey there!<br/><br/>We heard you asked to have your password reset. Please use this OTP: " +
                                    "<strong>" + OTP + "</strong ><br/>" +
                                    "<br/>The OTP will expire in 5 minutes. Do not share this OTP with anyone.<br/>See you in a bit! :-)" +
                                    "<br/><br/>Sincerely,<br/>SITConnect Support Team</p> ";
                                mail.IsBodyHtml = true;

                                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                                {
                                    string strEmail = System.Configuration.ConfigurationManager.AppSettings["Email"];
                                    string strPassword = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];
                                    smtp.Credentials = new NetworkCredential(strEmail, strPassword);
                                    smtp.EnableSsl = true;
                                    smtp.Send(mail);
                                }
                            }
                            Session["Timer"] = DateTime.Now;
                            Session["EmailforOTP"] = email;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    //else
                    //{
                    //    lbl_error_email.Text = "Account Invalid";
                    //}

                    //Session objects Time, EmailforOTP and OTP will not be created if email is invalid
                }
                else
                {
                    lbl_error_email.Text = "Captcha unsuccessful. Please try again.";
                    lbl_error_email.ForeColor = Color.Red;
                }

            }

        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string userOTP = tb_otp.Text;
            if (string.IsNullOrEmpty(userOTP))
            {
                lbl_error_OTP.Text = "Field empty";
            }
            else
            {
                if (Session["Timer"] != null)
                {
                    DateTime timer = DateTime.Parse(Session["Timer"].ToString());
                    if (DateTime.Compare(DateTime.Now, timer.AddMinutes(5)) > 0)
                    {
                        lbl_error_OTP.Text = "OTP has expired. Pls request for a new one." + Session["Timer"].ToString();
                    }
                    else if (userOTP != (string)Session["OTP"])
                    {
                        lbl_error_OTP.Text = "OTP Invalid";
                    }
                    else
                    {
                        lbl_error_OTP.Text = "";
                        Session.Remove("OTP");
                        Session.Remove("Timer");
                        Session["OTPVerified"] = "true";
                        Response.Redirect("ResetPassword.aspx", false);
                    }
                }
                else
                {
                    lbl_error_OTP.Text = "OTP has expired. Pls request for a new one.";
                }
            }

        }

        protected void btnBack2_Click(object sender, EventArgs e)
        {
            VerifyOTP.Visible = false;
            RequestOTP.Visible = true;
            //Naturally Cleared OTP from Session bcoz user wont be able to go to enter otp unless he request for new one, which will reset OTP value.

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

        protected void btnShowVerify_Click(object sender, EventArgs e)
        {
            RequestOTP.Visible = false;
            VerifyOTP.Visible = true;
        }

        protected void btnNoOTP_Click(object sender, EventArgs e)
        {
            Response.Redirect("Support.aspx", false);
        }

        protected void btnNewReqOTP_Click(object sender, EventArgs e)
        {
            string email = (string)Session["EmailforOTP"];
            if (email != null)
            {
                if (ValidateEmail(email) != null)
                {
                    //Email is valid
                    try
                    {
                        using (MailMessage mail = new MailMessage())
                        {
                            int rand = new Random().Next(100000, 999999);
                            string OTP = rand.ToString();
                            Session["OTP"] = OTP;

                            mail.From = new MailAddress("sitconnect2021@gmail.com");
                            mail.To.Add(email);
                            mail.Subject = "SITConnect - Password Reset Request";
                            mail.Body = "<p style=\"color:#000000;margin:10px 0;padding:0;font-family:Helvetica;font-size:16px;line-height:150%;text-align:left\">" +
                                "Hey there!<br/><br/>We heard you asked to have your password reset. Please use this OTP: " +
                                "<strong>" + OTP + "</strong ><br/>" +
                                "<br/>The OTP will expire in 5 minutes. Do not share this OTP with anyone.<br/>See you in a bit! :-)" +
                                "<br/><br/>Sincerely,<br/>SITConnect Support Team</p> ";
                            mail.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                string strEmail = System.Configuration.ConfigurationManager.AppSettings["Email"];
                                string strPassword = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];
                                smtp.Credentials = new NetworkCredential(strEmail, strPassword);
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }
                        Session["Timer"] = DateTime.Now;
                        Session["EmailforOTP"] = email;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                //else
                //{
                //    lbl_error_email.Text = "Account Invalid";
                //}
            }

            else
            {
                //User skipped enter email part
                RequestOTP.Visible = true;
                VerifyOTP.Visible = false;
            }

        }

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
    }
}