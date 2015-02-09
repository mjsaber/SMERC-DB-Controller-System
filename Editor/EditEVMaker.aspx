<%@ Page Title="Edit EVMaker" Language="C#" MasterPageFile="~/Editor.Master" AutoEventWireup="true" CodeBehind="EditEVMaker.aspx.cs" Inherits="EVEditor.EditEVMaker" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .style1
        {
            width: 400px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Edit Maker</h2>
    <table>
        <tr>
            <td>
                <asp:Label ID="lblCatchError" runat="server" 
            ForeColor="Red" Text="" Visible="False"></asp:Label>
                <asp:Button ID="btnHideCatchError" runat="server" 
            CausesValidation="False" onclick="btnHideCatchError_Click" 
            Text="Hide" Visible="False" />
            </td>
        </tr>
        <tr>
            <td valign="top">
        <table>
        <tr>
        <td valign="top">
        <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Maker List</h2>
                </div></div></div>
            <div class="mid-outer"><div class="mid-inner"><div class="mid">



                <asp:GridView ID="gvMaker" runat="server" CssClass="datatable" 
                     GridLines="Vertical"  OnPageIndexChanging="gvMakerPaging" onselectedindexchanged="gvMakerSelectedIndex" 
onrowcreated ="gvMakerRowCreated" OnSorting="gvMakerSorting"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="MakerName" Width="800px">
            <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="MakerName" HeaderText="Maker Name" SortExpression="MakerName" />
            </Columns>  
                    <PagerSettings Position="TopAndBottom" /><PagerStyle CssClass="pager-row" /> <RowStyle CssClass="row" Wrap="True" />
            <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="True" /> 
            <SortedAscendingCellStyle BackColor="#FFCC66" Wrap="False" />
            <SortedDescendingCellStyle BackColor="#FFCC66" Wrap="False" />
                </asp:GridView>
                


            </div></div></div>
    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
       </div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
    </td>
    </tr>
    </table>
    </td>
<td valign="top">
   <div class="grid">


            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">

                    <h2>Selected EV Information</h2></div></div></div>

                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 

         <tr>
            <td nowrap="nowrap">EV Maker</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:TextBox ID="tbEVMaker" runat="server" MaxLength="50" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="vldrtbEVMaker" runat="server" ControlToValidate="tbEVMaker" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red"></asp:RequiredFieldValidator>
             </td>
         </tr>
    </table>

    
    </div></div></div> <%--                <asp:Label ID="lblddlCreditCardPaymentError" runat="server" ForeColor="Red" Text=""></asp:Label>--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
       </div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
    <asp:Button ID="btnNew" runat="server" Font-Size="medium" Text="New" visible = "true" onclick="btnNewClick"/>
    <asp:Button ID="btnUpdate" runat="server" Font-Size="medium" Text="Update" visible = "false" OnClick="btnUpdateClick" />    
    <asp:Button ID="btnCancel" runat="server" Font-Size="medium" Text="Cancel" visible = "true" OnClick="btnCancelClick" />
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />
</asp:Content>
