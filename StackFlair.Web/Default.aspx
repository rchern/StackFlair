<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StackFlair.Web.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>StackFlair</title>
</head>
<body>
    <form id="form1" runat="server">
	    <div>
		<hr><br><br><br><br>
		Select a site:
		<asp:DropDownList runat="server" ID="ddlSites" /> 
		and your numeric user id
		<asp:TextBox runat="server" ID="txtUserID" /> 
		<br />Show global flair:
		<asp:CheckBox runat="server" ID="cbGlobal" />  
		<br />Select a format:
		<asp:RadioButtonList runat="server" ID="rblFormat" RepeatDirection="Horizontal" RepeatLayout="Flow">
			<asp:ListItem Text="Html" Value="html" />
			<asp:ListItem Text="Image" Value="png" />
		</asp:RadioButtonList>
		<br />Select a theme:
		<asp:DropDownList runat="server" ID="ddlThemes">
			<asp:ListItem Text="Default" Value="" />
			<asp:ListItem Text="Glitter" Value="glitter" />
			<asp:ListItem Text="Black" Value="black" />
			<asp:ListItem Text="Hot Dog" Value="hotdog" />
			<asp:ListItem Text="HoLy" Value="holy" />
		</asp:DropDownList>
		<br />Exclude beta SE sites: 
		<asp:CheckBox runat="server" ID="cbBeta" />
		<br /><asp:Button runat="server" ID="btnSubmit" Text="Submit" OnClick="btnSubmit_Click" />
		<br><br><br><br><hr>
	    </div>
    </form>
</body>
</html>
