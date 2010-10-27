<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StackFlair.Web.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>StackFlair</title>
    <link href="css/reset.css" rel="stylesheet" type="text/css" />
    <%--<link href="css/Default.css" rel="Stylesheet" type="text/css" />--%>
</head>
<body>
    <form id="form1" runat="server">
    <asp:RadioButtonList ID="rblMarital" RepeatDirection="Horizontal" runat="server">
        <asp:ListItem Value="S" Selected="True">Single</asp:ListItem>
        <asp:ListItem Value="M">Married</asp:ListItem>
        <asp:ListItem Value="D">Separated/Divorced</asp:ListItem>
    </asp:RadioButtonList>
    <%--<div id="flair">
        <ul>
            <li>
                    <label for="ddlSites">Select a site:</label>
                    <asp:DropDownList runat="server" ID="ddlSites" DataValueField="key" DataTextField="value" />
            </li>

            <li>
                <label for="txtUserID">Enter your numeric user id:</label>
                <asp:TextBox runat="server" ID="txtUserID" />
            </li>
        
            <li>
                <label for="cbGlobal">Click here to turn off combined flair</label>
                <asp:CheckBox runat="server" ID="cbGlobal" Checked="true" />
            </li>

            <li>
                <label for="rblFormat">Select output format:</label>
                <asp:RadioButtonList runat="server" ID="rblFormat" RepeatDirection="Horizontal" RepeatLayout="Flow">
                    <asp:ListItem Text="Html" Value="html" />
                    <asp:ListItem Text="Image" Value="png" Selected="True" />
                </asp:RadioButtonList>
        
            </li>
        
            <li>
                <label for="cbBeta">Click here to exclude beta sites</label>
                <asp:CheckBox runat="server" ID="cbBeta" />
            </li>
            
            <li>
                <h3>Select a theme:</h3>
                <asp:ListView runat="server" ID="ddlThemes">
                    <LayoutTemplate>
                        <ul id="ddlThemes">
                            <li runat="server" id="itemPlaceHolder"></li>
                        </ul>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <li>
                            <label for='<%# Eval("Name","ddlThemes_{0}") %>'>
                                <asp:Image runat="server" ID="imgTheme" ImageUrl='<%# Eval("Name","/Generate/options/theme={0}/3f0eac82-1801-410d-b334-234c18ddeeeb.png") %>' />
                                <strong><%# Eval("Name") %></strong>
                            </label>
                            <input type="radio" name="ddlThemes" id='<%# Eval("Name","ddlThemes_{0}") %>' value='<%# Eval("Name") %>' />
                        </li>
                    </ItemTemplate>
                </asp:ListView>
            </li>
<br />
            <br />
        
            <li>
                <asp:Button runat="server" ID="btnSubmit" Text="Generate Flair" OnClick="btnSubmit_Click" />
            </li>

            </ul>
            Feature requests welcome, no matter how small or specific!
            <hr />
            Project site: <a href="http://github.com/rchern/StackFlair">http://github.com/rchern/StackFlair</a>
            <hr />
            StackApps site: <a href="http://stackapps.com/q/1567/2286">http://stackapps.com/q/1567/2286</a>
    </div>--%>
    </form>
</body>
</html>
