<%@ Page Title="Retrieve forgotten Information" MasterPageFile="~/Site1.master"  Language="C#" 
 AutoEventWireup="true" CodeBehind="Retrieve.aspx.cs" Inherits="RTMC.Account.Retrieve" %>

    <asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

    </asp:Content> 
    
    <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <table width ="550">
    <tr>
    <td>
    <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Username and Password Retrieval</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">   
                                                                           
                    <br />
            After retrieving your password, you may change your password 
            by logging in to this website.<br /><br />

        Enter Username or Email below:<br />
        <asp:TextBox ID="tbNameOrEmail" runat="server" 
            ontextchanged="tbNameOrEmail_TextChanged" Font-Size="Medium" Width="347px"></asp:TextBox>
        <asp:Button ID="btnEmailMe" runat="server" 
            onclick="btnEmailMe_Click" Text="Email me" Font-Bold="True" Font-Size="Medium" />
            <br />
        <br />
        <asp:Label ID="lblEmail" runat="server" Text="" Visible="False" Font-Size="Medium"></asp:Label>

        <asp:Label ID="lblMessageText" runat="server" Text="" 
             ForeColor="Red" Font-Size="Medium"></asp:Label>

        <br />
        <br />               
        <asp:HyperLink ID="HyperLink1" runat="server" 
            NavigateUrl="~/Account/Login.aspx" Font-Size="Large">Back to Login</asp:HyperLink>
            </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%> 

       </td>

       </tr>
       </table>

 </asp:Content>