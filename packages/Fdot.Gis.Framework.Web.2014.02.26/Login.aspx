<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FDOT.GIS.Web.Login" %>

<%@ Import Namespace="FDOT.GIS.Web" %>
<%@ Register Assembly="FDOT.Enterprise.Architecture.Web.UserControl, PublicKeyToken=a11df4d9086030a8,version=2.0.0.0, culture=neutral"
    Namespace="FDOT.Enterprise.Architecture.Web.UserControl" TagPrefix="DOTLogin" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>
        <%= HttpUtility.HtmlEncode(ApplicationContext.Current.Configuration.Title)%>
        - Login</title>
    <style type="text/css">
        html, body, form
        {
            height: 100%;
            width: 100%;
            overflow: hidden;
            font-family: Arial, Helvetica, Verdana;
        }
        body
        {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            height: 100%;
            text-align: center;
        }
        #footer
        {
            height: 60px;
            width: 100%;
            clear: both;
            border-top: 2px solid black;
        }
        .left-content
        {
            width: 15%;
            height: 100%;
            float: left;
            padding-left: 5px;
            padding-top: 5px;
        }
        .center-content
        {
            width: 68%;
            height: 100%;
            float: left;
            text-align: center;
            vertical-align: middle;
            padding-left: 5px;
            padding-top: 5px;
        }
        .right-content
        {
            width: 15%;
            height: 100%;
            float: right;
            padding-right: 5px;
            padding-top: 5px;
        }
    </style>
</head>
<body>
    <div style="text-align: center; width: 700px; border-width: thin; border-color: Black">
        <div class='left-content'>
            <img alt="FDOT Logo" src='http://tlbstws.dot.state.fl.us/images/dotlogosm.gif' />
        </div>
        <div class='center-content'>
            <h2>
                FDOT Enterprise GIS Framework</h2>
            <h3>
                <%= HttpUtility.HtmlEncode(ApplicationContext.Current.Configuration.Title)%></h3>
        </div>
        <div class='right-content'>
        </div>
        <br />
        <form id="form1" runat="server">
        <div>
            <%--       <asp:Login runat="server" ID="login1" DisplayRememberMe="False" OnLoggedIn="OnLoggedIn" />--%>
            <DOTLogin:CommonLogin ID="loginControl" ShowDisclaimer="true" OnLoggedIn="OnLoggedIn"
                AuthenticationType="ISASecurityandRACFSecurity" runat="server" />
        </div>
        <div id='footer'>
            <div class='left-content'>
                <img src='http://tlbstws.dot.state.fl.us/images/oislogo_sm.jpg' alt='FDOT Office of Information Systems logo' />
            </div>
            <div class='center-content'>
                FLORIDA DEPARTMENT OF TRANSPORTATION<br />
                Report Issues to the Service Desk @ 1-866-955-4357 (HELP) or e-mail: <a href='mailto:FDOT.ServiceDesk@dot.state.fl.us?subject=GIS Framework Issue'>
                    Service Desk</a>
            </div>
            <div class='right-content'>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
