<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgetPassword.aspx.cs" Inherits="SITConnect.ForgetPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect- Forget Password</title>
    <style type="text/css">
        .auto-style1 {
            margin-top: 0px;
        }
    </style>
    <script src="https://www.google.com/recaptcha/api.js?render=6LebSEsaAAAAAOagHvwuu4Ol6QgV6rogUUwIGmzT"></script>
</head>
<body>
    <h2>Recover my Account</h2>
    <asp:Panel ID="RequestOTP" Visible="true" runat="server">

        <p>Enter the email that you used to register your SITConnect account below.</p>
        <form id="form1" runat="server">
            <div style="align-content: center;">
                <fieldset>
                    <legend>Request OTP</legend>
                    <br />
                    <p>
                        Email:
                        <asp:TextBox ID="tb_email" runat="server" Height="20px" Width="137px"></asp:TextBox>
                    </p>
                    <br />
                    <asp:Label ID="lbl_error_email" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                    <br />
                    <asp:Button ID="btnRequestOTP" runat="server" Text="Send Me an OTP" Height="30px" Width="170px" OnClick="btnRequestOTP_Click" />
                    &nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnBack1" runat="server" Text="Back" Height="30px" Width="110px" OnClick="btnBack1_Click" />
                    <br />
                    <br />
                    <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
                </fieldset>
                <br />
                <br />
                <asp:Button ID="btnShowVerify" runat="server" Text="I have received my OTP" Height="30px" Width="300px" OnClick="btnShowVerify_Click" />
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnNoOTP" runat="server" Text="I did not receive my OTP" Height="30px" Width="300px" OnClick="btnNoOTP_Click" />
            </div>
        </form>
    </asp:Panel>

    <asp:Panel ID="VerifyOTP" Visible="false" runat="server" CssClass="auto-style1">
        <p>OTP will expire after 5 minutes.</p>
        <form id="form2" runat="server">
            <div style="align-content: center;">
                <fieldset>
                    <legend>Verify OTP</legend>
                    <br />
                    <p>
                        OTP:
                        <asp:TextBox ID="tb_otp" runat="server" Height="20px" Width="137px"></asp:TextBox>
                    </p>
                    <br />
                    <asp:Label ID="lbl_error_OTP" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                    <br />
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify OTP" Height="30px" Width="170px" OnClick="btnVerifyOTP_Click" />
                    &nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnBack2" runat="server" Text="Back" Height="30px" Width="110px" OnClick="btnBack2_Click" />
                    <br />
                    <br />
                </fieldset>
                <br />
                <br />
                <asp:Button ID="btnNewReqOTP" runat="server" Text="Request for New OTP" Height="30px" Width="300px" OnClick="btnNewReqOTP_Click" />
            </div>
        </form>
    </asp:Panel>
    
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6LebSEsaAAAAAOagHvwuu4Ol6QgV6rogUUwIGmzT', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>