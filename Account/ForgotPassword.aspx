<%@ Page Title="Forgot Password" MasterPageFile="~/Site1.master"  Language="C#" 
 AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="RTMC.Account.ForgotPassword" %>

    <asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

    </asp:Content> 
    
    <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <table width ="550">
    <tr>
    <td>
    <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Reset Password</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">   
                                                                           
                    <br />

        Enter Username or Email:<br />
        <asp:TextBox ID="tbEnterUsernameOrEmail" runat="server" Font-Size="Medium" Width="347px"></asp:TextBox>

        <asp:Button ID="btnEmailMe" runat="server" Text="Submit" Font-Bold="True" Font-Size="Medium" OnClick="btnEmailMe_Click" />
            <br />
        <br />
        <asp:Label ID="lblEmail" runat="server" Text="" Visible="False" Font-Size="Medium"></asp:Label>

        <asp:Label ID="lblMessage" runat="server" Text="" 
             ForeColor="Red" Font-Size="Medium"></asp:Label>

                    <br />

        <asp:Label ID="lblOutMessage" runat="server" Text="" Font-Size="Medium"></asp:Label>

                    <br />
                    <br />
                    <asp:Label ID="lblError" runat="server" Text="" Font-Size="Medium"></asp:Label>

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