<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Support.aspx.cs" Inherits="SITConnect.Support" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Support</title>
</head>
<body>
    <h2>I did not receive an OTP</h2>
    <ul>
         <li>Please ensure that you have entered a valid email.</li>
         <li>Email has to be a <strong>registered account</strong> of SITConnect</li>
    </ul>

    <p>Still need help? Write an email to us at <strong>sitconnect2021@gmail.com</strong>.<br />
        Our Support Team will get back to you in 2-3 Business Days.
    </p>
    <form id="form1" runat="server">
        <asp:Button ID="btnBack" runat="server" Text="Back to Login" Height="30px" Width="130px" OnClick="btnBack_Click"/>
    </form>
</body>
</html>

