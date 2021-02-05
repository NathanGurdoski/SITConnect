<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="SITConnect.ChangePassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Edit Profile</title>
    <script>
        function isMatching() {
            var str1 = document.getElementById('<%=tb_new_pwd1.ClientID %>').value;
            var str2 = document.getElementById('<%=tb_new_pwd2.ClientID %>').value;
            var str3 = document.getElementById("lbl_error_complex").innerHTML;
            if (str3 == "OK") {
                if (str2 != str1) {
                    document.getElementById("lbl_error_match").innerHTML = "Passwords do not match.";
                    document.getElementById("lbl_error_match").style.color = "Red";
                    return ("unmatching");
                }
                document.getElementById("lbl_error_match").innerHTML = "OK";
                document.getElementById("lbl_error_match").style.color = "Green";
            }
            else {
                document.getElementById("lbl_error_match").innerHTML = "New password needs to be corrected first.";
                document.getElementById("lbl_error_match").style.color = "Red";
            }
        }
        function validatePwd() {
            var str = document.getElementById('<%=tb_new_pwd1.ClientID %>').value;

            if (str.length < 8) {
                document.getElementById("lbl_error_complex").innerHTML = "(Password Length Must be at least 8 Characters)";
                document.getElementById("lbl_error_complex").style.color = "Red";
                return ("too short");
            }
            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("lbl_error_complex").innerHTML = "(Password require at least 1 number)";
                document.getElementById("lbl_error_complex").style.color = "Red";
                return ("No number");
            }
            else if (str.search(/^(?=.*[a-z])(?=.*[A-Z])/)) {
                document.getElementById("lbl_error_complex").innerHTML = "(Password require at least one lowercase and uppercase character)";
                document.getElementById("lbl_error_complex").style.color = "Red";
                return ("No uppercase or lowercase character");
            }
            else if (str.search(/^(?=.*[!@#$%^&*])/)) {
                document.getElementById("lbl_error_complex").innerHTML = "(Password require at least 1 special character)";
                document.getElementById("lbl_error_complex").style.color = "Red";
                return ("No special character");
            }
            document.getElementById("lbl_error_complex").innerHTML = "OK";
            document.getElementById("lbl_error_complex").style.color = "Green";
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset>
            <legend>Change Password</legend>
                <br/>
                    <p>Enter old password: 
                    <asp:TextBox ID="tb_old_pwd" runat="server" Height="20px" Width="137px" TextMode="Password"></asp:TextBox>
                    </p>
                    <p>Enter new password: 
                    <asp:TextBox ID="tb_new_pwd1" runat="server" Height="20px" Width="137px" TextMode="Password" onkeyup="javascript:validatePwd()"></asp:TextBox>
                    <asp:Label ID="lbl_error_complex" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                    </p>
                    <p>Re-enter new password: 
                    <asp:TextBox ID="tb_new_pwd2" runat="server" Height="20px" Width="137px" TextMode="Password" onkeyup="javascript:isMatching()"></asp:TextBox>
                    <asp:Label ID="lbl_error_match" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                    </p>
                    <asp:Label ID="lbl_error_pwd" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
                <br />
                <p><asp:Button ID="btnCancel" runat="server" Text="Cancel" Height="27px" Width="133px" OnClick="btnCancel_Click"/><asp:Button ID="btnSubmit" runat="server" Text="Submit" Height="27px" Width="133px" OnClick="btnSubmit_Click"/></p>
            </fieldset>
        </div>
    </form>
</body>
</html>
