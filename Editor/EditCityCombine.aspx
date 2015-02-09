<%@ Page Title="Edit Combined Organization" Language="C#"  AutoEventWireup="true" CodeBehind="EditCityCombine.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditCityCombine" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">

        .style1
        {
            width: 400px;
        }

    </style>
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Edit Combined Organization</h2>
    <table>
        <tr>
            <td>Show only Activated:&nbsp;
                <asp:CheckBox ID="cbShowActivated" runat="server" AutoPostBack="True" oncheckedchanged="cbShowActivated_CheckedChanged" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblCatchError" runat="server" 
                    ForeColor="Red" Text="" Visible="False"></asp:Label>
                <asp:Button ID="btnHideCatchError" runat="server" CausesValidation="False" onclick="btnHideCatchError_Click" Text="Hide" Visible="False" />
                <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
            </td>
            
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblMainCity" runat="server" Text="" Visible = "false"></asp:Label>
                <asp:Label ID="lblCCdash" runat="server" Text="" Visible = "false"></asp:Label>
                <asp:Label ID="lblCombined" runat="server" Text="" Visible="false"></asp:Label>
            </td>
        </tr>
        
    </table>

        <table>
            <tr>
                <td valign="top">
        <table>
        <tr>
        <td valign="top">
        <div class="grid">
            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Combined Organization List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvCombCity" runat="server" CssClass="datatable" 
                     GridLines="Vertical"
           OnPageIndexChanging="gvCombCityPaging" OnRowCreated="gvCombCityRowCreated"
                     OnSorting="gvCombCitySorting" onselectedindexchanged="gvCombCitySelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     Width="700px">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="Main City" HeaderText="Main Organization" SortExpression="Main City" />
            <asp:BoundField DataField="Sub Cities" HeaderText="Sub Organizations" SortExpression="Sub Cities" />
            <asp:BoundField DataField="GUID" HeaderText="GUID" SortExpression="GUID" />
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate"  SortExpression="Activate"/>
        </Columns>

        <PagerSettings Position="TopAndBottom" /><PagerStyle CssClass="pager-row" /> <RowStyle CssClass="row" Wrap="True" />
        <SelectedRowStyle BackColor="#38CDB9" Font-Bold="True" ForeColor="White" Wrap="True" /> 
            <SortedAscendingCellStyle BackColor="#FFCC66" Wrap="False" />
            <SortedDescendingCellStyle BackColor="#FFCC66" Wrap="False" />
    </asp:GridView> 
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>
    </td>
    </tr>
    </table>
    </td>


    <td valign="top">
                    <div class="grid">


            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Selected Organization Information</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 
    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Main Organization</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlMainCity" runat="server" AutoPostBack="true" 
                onselectedindexchanged="ddlMainCity_SelectedIndexChanged">
            </asp:DropDownList>
         <asp:RequiredFieldValidator ID="rfvddlMainCity" 
             runat="server" ControlToValidate="ddlMainCity" 
             Display="Dynamic" ErrorMessage="Select an Organization" 
             ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
        </td></tr>

     <tr>
        <td nowrap="nowrap">Sub Organizations</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:CheckBoxList ID="cblSubCities" runat="server" 
                AutoPostBack="true" Font-Bold="False"
                
                onselectedindexchanged="cblSubCities_SelectedIndexChanged" 
                BorderColor="Silver" BorderStyle="Outset">
            </asp:CheckBoxList>
         </td>
     </tr>  
     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




             
       <asp:Button ID="btnNew" runat="server" Text="New" visible = "true" onclick="btnNewClick"/>      
       <asp:Button ID="btnActivate" runat="server"  Text="Activate" visible = "false" onclick="btnActivateClick"/>      
       <asp:Button ID="btnDeactivate" runat="server" Text="Deactivate" visible = "false" onclick="btnDeactivateClick"/>      
       <asp:Button ID="btnClear" runat="server" Text="Clear" visible = "true" CausesValidation="false" onclick="btnClearClick"/>       
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>