<%@ Page Title="Edit Fleet" Language="C#" MasterPageFile="~/Editor.master" AutoEventWireup="true" CodeBehind="EditFleet.aspx.cs" Inherits="EVEditor.EditFleet" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .style1
        {
            width: 400px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Edit Fleet</h2>
    <table>

        <%--1st Row--%>
        <tr>
            <td>By Organization</td>
            <td>
                <asp:DropDownList ID="ddlOrganization" runat="server" AutoPostBack="true" 
                    onselectedindexchanged="ddlOrganization_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:Label ID="lblCatchError" runat="server" 
            ForeColor="Red" Text="" Visible="False"></asp:Label>
                <asp:Button ID="btnHideCatchError" runat="server" 
            CausesValidation="False" onclick="btnHideCatchError_Click" 
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
                    <h2>Fleet List</h2>
                </div></div></div>
            <div class="mid-outer"><div class="mid-inner"><div class="mid">



                <asp:GridView ID="gvFleet" runat="server" CssClass="datatable" 
                     GridLines="Vertical"  OnPageIndexChanging="gvFleetPaging" onselectedindexchanged="gvFleetSelectedIndex" 
onrowcreated ="gvFleetRowCreated" OnSorting="gvFleetSorting"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="VehicleId" Width="800px">   <%-- onrowcreated="gvPLRowCreated" 
                     OnSorting="gvPLSorting" --%>
                    <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="VehicleId" HeaderText="Vehicle ID" SortExpression="VehicleID" />
            <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName" />
            <asp:BoundField DataField="UserId" HeaderText="UserID" SortExpression="UserId"/>
            <asp:BoundField DataField="ID" HeaderText="EVModileID" SortExpression="ID" />
            <asp:BoundField DataField="EV Info" HeaderText="EV Info" />
            <asp:BoundField DataField="LicenseNo" HeaderText="License No." SortExpression="LicenseNo" />
            <asp:BoundField DataField="PrincipalDriverName" HeaderText="Principal Driver Name" SortExpression="PrincipalDriverName" />

                      </Columns>  
                    <PagerSettings Position="TopAndBottom" /><PagerStyle CssClass="pager-row" /> <RowStyle CssClass="row" Wrap="True" />
            <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="True" /> 
            <SortedAscendingCellStyle BackColor="#FFCC66" Wrap="False" />
            <SortedDescendingCellStyle BackColor="#FFCC66" Wrap="False" />
                </asp:GridView>
                


            </div></div></div> <%-- onrowcreated="gvPLRowCreated" 
                     OnSorting="gvPLSorting" --%>


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
            <td nowrap="nowrap">Vehicle ID</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:TextBox ID="tbVehicleId" runat="server" MaxLength="50" ></asp:TextBox>
                <%-- end div grid --%>
                <br />
                <asp:Label ID="VehicleIdLabel" runat="server" MaxLength="70" Text="If VehicleID doesn't exist in table, click New to generate it. " Font-Size="X-Small"></asp:Label>
             </td>
         </tr>

         <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap" style="width:180px">User Name</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1">
                <asp:DropDownList ID="ddlUserName" runat="server"  ></asp:DropDownList><%--AutoPostBack="False"--%>
                <asp:RequiredFieldValidator ID="vldrddlUserName" runat="server" ControlToValidate="ddlUserName" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
                </td>

         </tr>

        <tr>
            <td nowrap="nowrap">EV Info</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td class="style1" >
                <asp:DropDownList ID="ddlEvInfo" runat="server"></asp:DropDownList>
                <asp:RequiredFieldValidator ID="vldrddlEvInfo" runat="server" ControlToValidate="ddlEvInfo" Display="Dynamic" ErrorMessage="Required Field." ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>

             </td>
         </tr>

        <tr style="background-color: #F4F4F4">
            <td nowrap="nowrap" style="width:180px">License No.</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td class="style1">
            <asp:TextBox ID="tbLicenseNo" runat="server" MaxLength="50" ></asp:TextBox><%--AutoPostBack="False"--%>
                
                </td>

         </tr>

        <tr>
            <td nowrap="nowrap">Principal Driver Name</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td class="style1" >
                <asp:TextBox ID="tbDriverName" runat="server" MaxLength="50"></asp:TextBox>


             </td>
         </tr>


     </table>

    
    </div></div></div> <%--                <asp:Label ID="lblddlCreditCardPaymentError" runat="server" ForeColor="Red" Text=""></asp:Label>--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
       </div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
    <asp:Button ID="btnNew" runat="server" Font-Size="medium" Text="New" visible = "true" onclick="btnNewClick"/>
           <asp:Button ID="btnUpdate" runat="server" Font-Size="medium" Text="Update" visible = "false" OnClick="btnUpdate_Click" />    

           <asp:Button ID="btnCancel" runat="server" Font-Size="medium" Text="Cancel" visible = "true" OnClick="btnCancel_Click" />
           <asp:Button ID="btnDelete" runat="server" Font-Size="medium" Text="Delete" visible = "false" OnClick="btnDelete_Click" /> 
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />

</asp:Content>
