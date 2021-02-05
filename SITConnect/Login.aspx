<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SITConnect.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Login</title>
    <script src="https://www.google.com/recaptcha/api.js?render=6LebSEsaAAAAAOagHvwuu4Ol6QgV6rogUUwIGmzT"></script>

</head>
<body>
    <h2>Welcome to SITConnect!</h2>
    <form id="form1" runat="server">
        <div>
            <fieldset>
            <legend>Login</legend>
                <br/>
                    <p>Email: 
                    <asp:TextBox ID="tb_email" runat="server" Height="20px" Width="137px"></asp:TextBox>
                    </p>
                    <p>Password: 
                    <asp:TextBox ID="tb_pwd" runat="server" Height="20px" Width="137px" TextMode="Password"></asp:TextBox>
                    </p>
                    <asp:Label ID="lbl_error_login" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                <br />
                <br />
                <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
                <p>
                    <asp:Button ID="btnLogin" runat="server" Text="Login" Height="30px" Width="130px" OnClick="btnLogin_Click"/>
                    &nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnForget" runat="server" Text="Forget Password" Height="30px" Width="150px" OnClick="btnForget_Click"/>
                </p>
                <p><asp:Button ID="btnRegister" runat="server" Text="New User? Register Now " Height="30px" Width="298px" OnClick="btnRegister_Click"/></p>
            </fieldset>
        </div>
    </form>

    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6LebSEsaAAAAAOagHvwuu4Ol6QgV6rogUUwIGmzT', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>

