<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LinkGenerator.aspx.cs"
    Inherits="FDOT.GIS.Web.LinkGenerator" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <h2>
        Hyperlink Generator for the Framework
    </h2>
    <div style="width: 600px;">
        Virtual Application:
        <%--<asp:TextBox runat="server" ID="txtVirtualAppId"></asp:TextBox>--%>
        <asp:DropDownList runat="server" ID="ddlVirtualAppId"></asp:DropDownList>
        <asp:Button runat="server" OnClick="btnGetLayers_OnClick" ID="btnGetLayers" Text="Get Layers" />
        <br />
        <br />
        <fieldset>
            <legend><strong>Map Extent</strong> </legend>XMax:<asp:TextBox runat="server" ID="XMax" />
            &nbsp;&nbsp;&nbsp; XMin:<asp:TextBox runat="server" ID="XMin" />
            <br />
            <br />
            YMax:<asp:TextBox runat="server" ID="YMax" />
            &nbsp;&nbsp;&nbsp; YMin:<asp:TextBox runat="server" ID="YMin" />
            <br />
            <br />
            <asp:CheckBox runat="server" ID="cbIncludeExtent" Text="Include Extent from Textboxes" />
        </fieldset>
        <br />
        <fieldset>
            <legend><strong>Select a Base Map </strong></legend>
            <asp:RadioButtonList ID="RadioButtonList1" runat="server">
            </asp:RadioButtonList>
        </fieldset>
        <br />
        <fieldset>
            <legend><strong>Select Dynamic layers to be visible </strong></legend>
            <asp:Repeater ID="Repeater1" runat="server" OnItemDataBound="Repeater1_OnItemDataBound">
                <ItemTemplate>
                    <asp:Label runat="server" ID="mapserviceLabel"></asp:Label>
                    <asp:CheckBoxList ID="cbList" runat="server">
                    </asp:CheckBoxList>
                    <br />
                    <br />
                </ItemTemplate>
            </asp:Repeater>
        </fieldset>
        <asp:Button ID="GetLink" runat="server" Text="Generate Link" OnClick="OnGetLink" />
        <br />
        <asp:HyperLink ID="HyperLink1" runat="server"></asp:HyperLink>
    </div>
    </form>
</body>
<script type="text/javascript" src="Scripts/jquery-1.4.3.js"></script>
<script type="text/javascript">

</script>
</html>
