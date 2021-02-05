<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="SITConnect.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Home</title>
</head>
<body>
    <h1>SITConnect Stationery Store</h1>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="False"></asp:Label>
    <p>Session will expire on: <asp:Label ID="lblSessionTimeOut" runat="server" EnableViewState="False"></asp:Label></p>
    <br />
    <form id="form1" runat="server">
    <div>
        <h4>Credit Card Info</h4>
        <p>Credit Card Number:<asp:Label ID="lbl_creditNo" runat="server" EnableViewState="False"></asp:Label>
        </p>
        <p>CVC:<asp:Label ID="lbl_cvc" runat="server" EnableViewState="False"></asp:Label>
        </p>
        <p>Expiry Date:<asp:Label ID="lbl_expiry" runat="server" EnableViewState="False"></asp:Label>
        </p>
    </div>
        <div>
            <asp:Button ID="btnChangePwd" runat="server" Text="Change Password" OnClick="btnChangePwd_Click" />
    <asp:Label ID="lbl_pwd_expiry" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
        </div>
        <div>
            <asp:Button ID="btnLogout" runat="server" Text="Logout" OnClick="btnLogout_Click" />
        </div>
    </form>
</body>
</html>