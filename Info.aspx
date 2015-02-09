<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Site.master" CodeBehind="Info.aspx.cs" Inherits="EVUser.Info" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        
        Message</h2>
    <p>

        <asp:Label ID="lblErrMsg" runat="server"></asp:Label>
                           
    </p>
</asp:Content>
