<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="SITConnect.Registration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>SITConnect - Register</title>

	<script type="text/javascript">
		function clearErrorMsg(id) {
            document.getElementById("lbl_error_" + id.slice(3)).innerHTML = "";
        }

		function validatePwd() {
			var str = document.getElementById('<%=tb_pwd.ClientID %>').value;

			if (str.length < 8) {
				document.getElementById("lbl_error_pwd").innerHTML = "(Password Length Must be at least 8 Characters)";
				document.getElementById("lbl_error_pwd").style.color = "Red";
				return ("too short");
			}
			else if (str.search(/[0-9]/) == -1) {
				document.getElementById("lbl_error_pwd").innerHTML = "(Password require at least 1 number)";
				document.getElementById("lbl_error_pwd").style.color = "Red";
				return ("No number");
			}
			else if (str.search(/^(?=.*[a-z])(?=.*[A-Z])/)) {
				document.getElementById("lbl_error_pwd").innerHTML = "(Password require at least one lowercase and uppercase character)";
				document.getElementById("lbl_error_pwd").style.color = "Red";
				return ("No uppercase or lowercase character");
			}
			else if (str.search(/^(?=.*[!@#$%^&*])/)) {
				document.getElementById("lbl_error_pwd").innerHTML = "(Password require at least 1 special character)";
				document.getElementById("lbl_error_pwd").style.color = "Red";
				return ("No special character");
			}
			document.getElementById("lbl_error_pwd").innerHTML = "";
		}
		
    </script>
</head>
<body>
	<h2>Register New Account</h2>
	<form id="form1" runat="server">
		<div>
			<fieldset>
				<legend>Registration</legend>
				<br />
				<fieldset>
					<legend>Account Particulars</legend>
					<p>First Name: 
					<asp:TextBox ID="tb_fname" runat="server" Height="20px" Width="137px" onkeyup="javascript:clearErrorMsg('tb_fname')"></asp:TextBox>
					<asp:Label ID="lbl_error_fname" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
					<p>Last Name: 
						<asp:TextBox ID="tb_lname" runat="server" Height="20px" Width="137px" onkeyup="javascript:clearErrorMsg('tb_lname')"></asp:TextBox>
					<asp:Label ID="lbl_error_lname" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
					<p>Email: 
					<asp:TextBox ID="tb_email" runat="server" Height="20px" Width="164px" onkeyup="javascript:clearErrorMsg('tb_email')"></asp:TextBox>
					<asp:Label ID="lbl_error_email" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
					<p>Password: 
					<asp:TextBox ID="tb_pwd" runat="server" Height="20px" Width="137px" TextMode="Password" onkeyup="javascript:validatePwd()"></asp:TextBox>
						<asp:Label ID="lbl_error_pwd" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
				</fieldset>
				<br />
				<fieldset>
					<legend>Credit Card Information</legend>
					<p>
						Credit Card Number:  
						<asp:TextBox ID="tb_creditNo" runat="server" Height="20px" Width="143px" onkeyup="javascript:clearErrorMsg('tb_creditNo')"></asp:TextBox>
					<asp:Label ID="lbl_error_creditNo" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
					<p>
						CVC: 
						<asp:TextBox ID="tb_cvc" runat="server" Height="20px" Width="67px" onkeyup="javascript:clearErrorMsg('tb_cvc')"></asp:TextBox>
					<asp:Label ID="lbl_error_cvc" runat="server" ForeColor="Red"></asp:Label>
				&nbsp;Expiry Date: 
						<asp:TextBox ID="tb_expiry" runat="server" Height="20px" Width="88px" onkeyup="javascript:clearErrorMsg('tb_expiry')"></asp:TextBox>
					<asp:Label ID="lbl_error_expiry" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
					</p>
				</fieldset>
				<br />
                <br />
                <p><asp:Button ID="btnRegister" runat="server" Text="Register" Height="27px" Width="133px" OnClick="btnRegister_Click"/></p>
				<p><asp:Button ID="btnLogin" runat="server" Text="Already have an account? Login " Height="27px" Width="284px" OnClick="btnLogin_Click"/></p>
				
			
				
			</fieldset>
		</div>
	</form>
</body>
</html>