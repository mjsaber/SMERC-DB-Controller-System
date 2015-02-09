<%@ Page Title="Key Encrypter" Language="C#"  AutoEventWireup="true" CodeBehind="KeyEncrypt.aspx.cs" MasterPageFile="~/Site.Master"  Inherits="EVEditor.KeyEncrypt" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .auto-style1
        {
            height: 26px;
        }
    </style>
    </asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <h2> Value Encrypter</h2>
    <h4>After encrypting the key, replace the value in the web.config</h4>
    <table>
        <tr>
            <td>
                Value to Encrypt:
            </td>
            <td>

                <asp:TextBox ID="tbValuePlain" runat="server" Width="500px"></asp:TextBox>

            </td>
        </tr>

        <tr>
            <td class="auto-style1">

                Encrypted Key:</td>
            <td class="auto-style1">

                <asp:TextBox ID="tbValueEncrypted" runat="server" Width="500px"></asp:TextBox>

            </td>
        </tr>
        <tr>
            <td>Regular Value: (For Testing Purposes)</td>
            <td>
                <asp:TextBox ID="tbValueDecrypt" runat="server" Width="500px"></asp:TextBox></td>
        </tr>
        <tr>
            <td><asp:Button ID="btnEncrypt" runat="server" Text="Encrypt values" OnClick="btnEncrypt_Click" /></td>
        </tr>
        
        
    </table>
        
 </asp:Content>