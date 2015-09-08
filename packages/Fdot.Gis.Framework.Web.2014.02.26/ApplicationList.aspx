<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplicationList.aspx.cs"
    Inherits="FDOT.GIS.Web.ApplicationList" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="text-align: center">
        <h2>
            GIS Application Inventory at
            <%=Request.ApplicationPath %></h2>
        <asp:Repeater ID="applicationList" runat="server">
            <HeaderTemplate>
                <table border="1" width="80%" class='application-list'>
                    <tr>
                        <th>
                            Application Name
                        </th>
                        <th>
                            Test Embedded
                        </th>
                        <th>
                            SL Test Mode
                        </th> 
                        <th>
                            Javascript Test
                        </th>
                        <th>
                            Generate Links
                        </th>
                        <th>
                            Authentication Type
                        </th>
                        <th>
                            Authentication Required
                        </th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class='application'>
                    <td>
                        <a href='Default.aspx?appId=<%#Eval("Name") %>'>
                            <%#Eval("Name") %></a>
                    </td>
                    <td>
                        <a href='FDOT.GIS.Client.TestEmbedded.aspx?appId=<%#Eval("Name") %>'>Go</a>
                    </td>
                    <td>
                        <a href='FDOT.GIS.ClientSLTestPage.aspx?appId=<%#Eval("Name") %>'>Test</a>
                    </td>
                    <td>
                        <a href='JavascriptTest.aspx?appId=<%#Eval("Name") %>'>Test</a>
                    </td>
                    <td>
                        <a href='LinkGenerator.aspx?appId=<%#Eval("Name") %>'>Generate Link to
                            <%#Eval("Name") %></a>
                    </td>
                    <td>
                        <%#Eval("AuthenticationType")%>
                    </td>
                    <td>
                        <%#Eval("RequiresAuthentication")%>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <%--        <asp:ListView runat="server" ID="applicationList">
            <LayoutTemplate>
                <div class='application-list'>
                    <asp:PlaceHolder runat="server" ID="itemPlaceholder" />
                </div>
            </LayoutTemplate>
            <ItemTemplate>
                <div class='application'>
                    <a href='FDOT.GIS.ClientTestPage.aspx?appId=<%#Eval("Name") %>'><span>Stand Alone</span>
                        <span>
                            <%#Eval("Name") %></span> </a>&nbsp;&nbsp; <a href='FDOT.GIS.Client.TestEmbedded.aspx?appId=<%#Eval("Name") %>'>
                                <span>Embedded</span> <span>
                                    <%#Eval("Name") %></span> </a>&nbsp;&nbsp; <span>
                                        <%#Eval("AuthenticationType")%></span> &nbsp;&nbsp; <span>Authentication Required:
                                            <%#Eval("RequiresAuthentication")%></span>
                </div>
            </ItemTemplate>
        </asp:ListView>
        --%>
    </div>  
        <div>
                Host name: <%=System.Net.Dns.GetHostEntry("").HostName%>
    </div>
    <div>
        Host name: <%=System.Net.Dns.GetHostEntry("").HostName%>
    </div>
    </form>
</body>
</html>
