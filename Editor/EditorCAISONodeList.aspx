<%@ Page Title="" Language="C#" MasterPageFile="~/Editor.Master" AutoEventWireup="true" CodeBehind="EditorCAISONodeList.aspx.cs" Inherits="EVEditor.EditorCAISONodeList" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .style1
        {
            width: 400px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Edit CAISO Node List</h2>
    <table>

        <%--1st Row--%>
        <tr>
            <td>By Organization</td>
            <td>
                <asp:DropDownList ID="ddlOrganization" runat="server" AutoPostBack="true" 
                    onselectedindexchanged="ddlOrganizationSelectedIndexChanged">
                </asp:DropDownList>
                <asp:Label ID="lblCatchError" runat="server" 
            ForeColor="Red" Text="" Visible="False"></asp:Label>
                <asp:Button ID="btnHideCatchError" runat="server" 
            CausesValidation="False" onclick="btnHideCatchErrorClick" 
            Text="Hide" Visible="False" />
            </td>
        </tr>
    </table>
    
    <p>
    </p>
    <table>
        <tr>
            <td valign="top">
        <table>
        <tr>
        <td valign="top">
        <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>CAISO Node List</h2>
                </div></div></div>
            <div class="mid-outer"><div class="mid-inner"><div class="mid">

                <asp:GridView ID="gvCAISONodeList" runat="server" CssClass="datatable" 
                     GridLines="Vertical"  OnPageIndexChanging="GvCAISONodeListPaging" onselectedindexchanged="GvCAISONodeListSelectedIndex" 
onrowcreated ="GvCAISONodeListRowCreated" OnSorting="GvCAISONodeListSorting"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="TransactionNodeID" Width="800px"> 
                    <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="TransactionNodeID" HeaderText="Traction Node ID" SortExpression="TransactionNodeID" />
            <asp:BoundField DataField="CityID" HeaderText="CityID" SortExpression="CityID" />
            <asp:BoundField DataField="Name" HeaderText="City" SortExpression="Name" />
            <asp:BoundField DataField="Note" HeaderText="Note" SortExpression="Note"/>
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

            <h2>Selected CAISO Node List</h2></div></div></div>

                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 

         <tr>
            <td nowrap="nowrap">TransactionNode ID</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:TextBox ID="tbTransactionNodeID" runat="server" MaxLength="50" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="vldrtbTransactionNodeID" 
                runat="server" ControlToValidate="tbTransactionNodeID" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
                <%-- end div grid --%>                
             </td>
         </tr>

         <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap" style="width:180px">City</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1">
                <asp:DropDownList ID="ddlCity" runat="server"  ></asp:DropDownList><%--AutoPostBack="False"--%>
                <asp:RequiredFieldValidator ID="vldrddlCity" runat="server" ControlToValidate="ddlCity" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
                </td>
         </tr>

        <tr>
            <td nowrap="nowrap">Note</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td class="style1" >
                <asp:TextBox ID="tbNote" runat="server"></asp:TextBox>

             </td>
         </tr>

     </table>
                    
    </div></div></div> <%--                <asp:Label ID="lblddlCreditCardPaymentError" runat="server" ForeColor="Red" Text=""></asp:Label>--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
       </div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
    <asp:Button ID="btnNew" runat="server" Font-Size="medium" Text="New" visible = "true" onclick="btnNew_Click"/>
    <asp:Button ID="btnUpdate" runat="server" Font-Size="medium" Text="Update" visible = "false" OnClick="btnUpdate_Click" />    
    <asp:Button ID="btnCancel" runat="server" Font-Size="medium" Text="Cancel" visible = "true" OnClick="btnCancel_Click" />
                    <br />
    </td>
    </tr>
    </table>
    <br />
</asp:Content>
