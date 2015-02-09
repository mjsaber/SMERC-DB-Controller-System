<%@ Page Title="Email Validation" Language="C#"  AutoEventWireup="true" MasterPageFile="../Site.Master" CodeBehind="EmailValidation.aspx.cs" Inherits="EVEditor.EmailValidation"%>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        #TextArea1
        {
            height: 79px;
            width: 218px;
        }
    </style>    
 </asp:Content>


 <asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
 
 <h2>Email Validation</h2>
 <p style="font-size: medium"> This page is used to validate email addresses for new users.</p>
 <table>


    <tr>

        <td>
            <%--lblTest is for testing purposes--%>
        </td>

        <td>
            <asp:Label ID="lblCatchError" runat="server" 
                ForeColor="Red"></asp:Label>
            <asp:Button ID="btnHideCatchError" runat="server" 
                Text="Hide" onclick="btnHideCatchError_Click" />
        </td>

        <td>
            <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
        </td>
    </tr>

    <tr>
        <td style="font-weight: 700">Emails to Validate:</td>
        <td>
            <asp:TextBox ID="tbEmailAddress" runat="server" 
                Height="191px" Width="345px" TextMode="MultiLine"></asp:TextBox>
        
        
        
        </td>
        <td>
            <asp:RegularExpressionValidator ID="regExtbEmailAddress" 
                RunAt="server" ControlToValidate="tbEmailAddress" 
                ErrorMessage="Valid Email required" ForeColor="Red" 
                ValidationExpression="^(\s*,?\s*[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})+\s*$" />
            <br />
                             <asp:RequiredFieldValidator ID="rflEmailAddress" runat="server" ControlToValidate="tbEmailAddress" 
                             ErrorMessage="Required."  ForeColor="Red"></asp:RequiredFieldValidator>
        </td>
    </tr>


    <tr>
        <td></td>
        <td>Separate multiple emails with a 
            comma - &quot; , &quot;</td>
        <td></td>
    
    </tr>
    <tr>

        <td style="font-weight: 700">
            <%--ddlOrganization lists the activated organizations and combined organization--%>
            Select Organization:</td>

        <td style="font-weight: 700">
            <asp:DropDownList ID="ddlOrganization" runat="server">
            </asp:DropDownList>
        </td>

        <td>
            <asp:RequiredFieldValidator ID="vldrddlOrganization" 
                runat="server" ControlToValidate="ddlOrganization" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
        </td>
    </tr>
    <tr>

        <td style="font-weight: 700">
            <%--ddlOrganization lists the activated organizations and combined organization--%>
            Select Account Type:</td>

        <td style="font-weight: 700">
            <asp:DropDownList ID="ddlEVUserAccountType" runat="server">
            </asp:DropDownList>
        </td>

<%--        <td>
            <asp:RequiredFieldValidator ID="vldrddlEVUserAccountType" 
                runat="server" ControlToValidate="ddlEVUserAccountType" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
        </td>--%>
    </tr>

     <tr>
        <td style="font-weight: 700">
            <%--ddlOrganization lists the activated organizations and combined organization--%>
            Select Expiration Window:</td>

        <td style="font-weight: 700">
            <asp:DropDownList ID="ddlEVUserAccountExpirationWindow" runat="server">
            </asp:DropDownList>
        </td>

    </tr>
    
    <tr>
        <td style="font-weight: 700">
            <%--ddlOrganization lists the activated organizations and combined organization--%>
            Select Maximum Vehicles:</td>

        <td style="font-weight: 700">
            <asp:DropDownList ID="ddlMaxVehicles" runat="server">
            </asp:DropDownList>
        </td>

    </tr>

    <tr>
        <td></td>
        <td>
            <asp:Button ID="btnValidateEmails" runat="server" 
                Text="Validate Emails" onclick="btnValidateEmails_Click" />
        </td>
        <td></td>
    </tr>
</table>


 </asp:Content>