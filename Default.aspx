<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="RTMC._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h1>
        Welecome to use SMERC Monitoring and Control Center</h1>
    <p>
Your account info

        <asp:GridView ID="gvAccountInfo" runat="server" AutoGenerateColumns="False" 
            onrowdatabound="gvAccountInfo_RowDataBound" BorderStyle="None" 
            GridLines="None" ShowHeader="False">
            <Columns>
                <asp:BoundField HeaderText="Title" DataField="Title" 
                    ItemStyle-HorizontalAlign="Right" >
<ItemStyle HorizontalAlign="Right"></ItemStyle>
                </asp:BoundField>
                <asp:BoundField HeaderText="Content" DataField="Content" 
                    ItemStyle-HorizontalAlign="Left" >
<ItemStyle HorizontalAlign="Left"></ItemStyle>
                </asp:BoundField>
            </Columns>
            <RowStyle VerticalAlign="Top" />
        </asp:GridView>
    </p>
    <br />
</asp:Content>
