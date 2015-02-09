<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BoxReport.aspx.cs" Inherits="RTMC.BoxReport" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        <table style="width:100%;">
            <tr>
                <td>                    
                    <asp:Label ID="lblStation" runat="server" Font-Bold="True" Font-Names="Arial" 
                        Font-Size="Large" Text="Label" CssClass="title" BackColor="White" 
                        ForeColor="Black"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblPL" runat="server" Font-Bold="True" Font-Names="Arial" 
                        Font-Size="Large" Text="Label" CssClass="title" BackColor="White" 
                        ForeColor="Black"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    Begining time&nbsp;
                    <asp:TextBox ID="tbBeginingTime" runat="server"></asp:TextBox>
&nbsp; End time&nbsp;
                    <asp:TextBox ID="tbEndTime" runat="server"></asp:TextBox>
&nbsp;
                    <asp:CheckBox ID="tbFilterInvalidData" runat="server" EnableViewState="False" 
                        Text="Filter invalid data" Checked="True" />
                    <asp:Label ID="Label1" runat="server" Text="Sort By"></asp:Label>
                    <asp:DropDownList ID="ddlSortBy" runat="server">
                        <asp:ListItem>Space No</asp:ListItem>
                        <asp:ListItem>User Name</asp:ListItem>
                        <asp:ListItem>Start Time</asp:ListItem>
                        <asp:ListItem>End Time</asp:ListItem>
                        <asp:ListItem>Is End</asp:ListItem>
                        <asp:ListItem>Is Ended By User</asp:ListItem>
                        <asp:ListItem>Energy Price</asp:ListItem>
                        <asp:ListItem>Charging Times</asp:ListItem>                        
                        <asp:ListItem>Energy Consumed</asp:ListItem>
                        <asp:ListItem>Charging Cost</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Label ID="Label2" runat="server" Text="in"></asp:Label>
                    <asp:DropDownList ID="ddlDirection" runat="server">
                        <asp:ListItem>ASC</asp:ListItem>
                        <asp:ListItem>DESC</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Label ID="Label3" runat="server" Text="Order"></asp:Label>
                    </td>
            </tr>
            <tr>
                <td style="font-weight: bold">
                    <asp:Button ID="btnChartGenerate" runat="server" Text="Generate" 
                        onclick="btnChartGenerate_Click" />
                    <asp:Label ID="lblMessage" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
            </tr> 
            <tr>
                <td align="left" style="font-weight: bold">
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnBackButton" runat="server" 
                        Text="Back to Chart and Report Page " PostBackUrl="~/ChartAndReport.aspx"/>
                </td>
            </tr>                
        </table>

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <rsweb:ReportViewer ID="rvReport" runat="server" Font-Names="Verdana" 
        Font-Size="8pt" InteractiveDeviceInfos="(Collection)" 
        WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" 
        Height="14in" Width="9.5in">
        <LocalReport ReportPath="BoxReport.rdlc">
        </LocalReport>
    </rsweb:ReportViewer>
        
</asp:Content>
