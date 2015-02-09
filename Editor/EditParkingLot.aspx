<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>
<%@ Page Title="Edit Parking Lot" Language="C#"  AutoEventWireup="true" CodeBehind="EditParkingLot.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditParkingLot" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit.HTMLEditor" TagPrefix="cc1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        #TextArea1
        {
            height: 79px;
            width: 218px;
        }
        .style1
        {
            width: 556px;
        }
    </style>
    </asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:ToolkitScriptManager ID="ToolkitScriptManager1"
        runat="server">
    </asp:ToolkitScriptManager>
    <script language="javascript" type="text/javascript">
        function ButtonClick() {
            document.getElementById('<%= btnPreviewImage.ClientID %>').click();
        }

    </script>

    <h2>Edit Parking Lot</h2>
    <table>
        <tr>
            <td>By Organization: 
            </td>
            <td>
                <asp:DropDownList ID="ddlModeOrganization" runat="server" 
                    AutoPostBack="true" 
                    onselectedindexchanged="ddlModeOrganization_SelectedIndexChanged"></asp:DropDownList>
        <asp:Label ID="lblCatchError" runat="server" 
            ForeColor="Red" Text="" Visible="False"></asp:Label>
                <asp:Button ID="btnHideCatchError" runat="server" 
            CausesValidation="False" onclick="btnHideCatchError_Click" 
            Text="Hide" Visible="False" />
            </td>
          
        </tr>    
        <tr>
           <td>Show Only Activated: </td>
           <td> Organizations:<asp:CheckBox ID="cbShowActivatedOrgs" runat="server" 
                    AutoPostBack="True" 
                    oncheckedchanged="cbShowActivatedOrgs_CheckedChanged" />
                </td>
          
        </tr>
        <tr>
            <td></td>
            <td>Parking Lots:&nbsp;&nbsp; <asp:CheckBox ID="cbShowActivatedPL" runat="server" AutoPostBack="True" oncheckedchanged="cbShowActivatedPL_CheckedChanged"/></td>
        </tr>
        <tr>
        <td>
            &nbsp;</td>
        <td>
        <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>  
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
                    <h2>Parking Lot List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvPL" runat="server" CssClass="datatable" 
                     GridLines="Vertical"
           OnPageIndexChanging="gvPLPaging" onrowcreated="gvPLRowCreated" 
                     OnSorting="gvPLSorting" onselectedindexchanged="gvPLSelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="ID" Width="800px" PageSize="10">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
            <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" />
            <asp:BoundField DataField="State ID" HeaderText="State ID" SortExpression="State ID" />
            <asp:BoundField DataField="Zip Code" HeaderText="Zip Code" SortExpression="Zip Code" />
            <asp:BoundField DataField="City ID" HeaderText="City ID" SortExpression="City ID" />
            <asp:BoundField DataField="Organization" HeaderText="Organization" SortExpression="Organization" />
            <asp:BoundField DataField="Latitude" HeaderText="Latitude" SortExpression="Latitude" />
            <asp:BoundField DataField="Longitude" HeaderText="Longitude" SortExpression="Longitude" />
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate" SortExpression="Activate" />
            <asp:BoundField DataField ="ChargingBoxLocationDirection" HeaderText ="Location Directions" SortExpression="ChargingBoxLocationDirection" />
            <asp:TemplateField HeaderText="Charging Box Location">
                <ItemTemplate>
                    <asp:Image ID="ChargingBoxLocation" runat="server" width="80px" ImageUrl='<%# "~/Editor/ShowImage.ashx?PLID="+ Eval("ID") %>'/>
                </ItemTemplate>            
            </asp:TemplateField>

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
                    <h2>Selected Parking Lot Information</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 


    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Name</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbName" runat="server" Width="161px" 
                MaxLength="50"></asp:TextBox>            
        &nbsp;<asp:RequiredFieldValidator ID="vldrtbName" 
                runat="server" ControlToValidate="tbName" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
        </td>    
        <td>
            &nbsp;</td>
    </tr>

     <tr>
        <td nowrap="nowrap">Address</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbAddress" runat="server"  Width="161px" 
                MaxLength="50"></asp:TextBox>           
        &nbsp;<asp:RequiredFieldValidator ID="vldrtbAddress" 
                runat="server" ControlToValidate="tbAddress" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
        </td>
        <td>
            &nbsp;</td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Organization</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlOrganization" runat="server"></asp:DropDownList>
        &nbsp;<asp:RequiredFieldValidator ID="vldrddlOrganization" 
                runat="server" ControlToValidate="ddlOrganization" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
        </td>   
        <td>
            &nbsp;</td>         
     </tr>
     
     <tr>
        <td>City</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbCity" runat="server"></asp:TextBox>
         &nbsp;<asp:RequiredFieldValidator ID="vldrtbCity" 
                runat="server" ControlToValidate="tbCity" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
         </td>
        <td>
            &nbsp;</td>
     </tr>
     
     <tr style="background-color: #F4F4F4">
        <td>State</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlState" runat="server">
            </asp:DropDownList>
         &nbsp;<asp:RequiredFieldValidator ID="vldrddlState" 
                runat="server" ControlToValidate="ddlState" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         </td>
        <td>
            &nbsp;</td>
     </tr>

     <tr>
        <td nowrap="nowrap">Zip Code</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbZipCode" runat="server"  Width="161px" 
                MaxLength="10"></asp:TextBox>
            <asp:RequiredFieldValidator ID="vldrtbZipCode" 
                runat="server" ControlToValidate="tbZipCode" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
         &nbsp;<asp:RegularExpressionValidator ID="regextbZipCode" 
                RunAt="server" ControlToValidate="tbZipCode"
                ErrorMessage="Valid Zip Code Required" ForeColor="Red" 
                ValidationExpression="^\d{5}(?:[-\s]\d{4})?$" />
        </td>
        <td>
            &nbsp;</td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap">Latitude</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbLatitude" runat="server"  Width="161px"></asp:TextBox>
        &nbsp;Degrees <asp:RequiredFieldValidator ID="vldrtbLatitude" 
                runat="server" ControlToValidate="tbLatitude" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
         &nbsp;<asp:RangeValidator ID="rvtbLatitude" runat="server" 
                ControlToValidate="tbLatitude" Display="Dynamic" 
                ErrorMessage="Values must be between -90 and 90." 
                ForeColor="Red" MaximumValue="90" MinimumValue="-90" 
                Type="Double"></asp:RangeValidator>
            
         </td>
        <td>&nbsp;</td>
     </tr>

     <tr>
        <td nowrap="nowrap" style="width:180px">Longitude</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbLongitude" runat="server"  Width="161px"></asp:TextBox>
        &nbsp;Degrees <asp:RequiredFieldValidator ID="vldrtbLongitude" 
                runat="server" ControlToValidate="tbLongitude" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
         &nbsp;<asp:RangeValidator ID="rvtbLongitude" runat="server" 
                ControlToValidate="tbLongitude" Display="Dynamic" 
                ErrorMessage="Values must be between -180 and 180." 
                ForeColor="Red" MaximumValue="180" MinimumValue="-180" 
                Type="Double"></asp:RangeValidator>
         </td>
        <td>&nbsp;</td>
     </tr>

    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Activate</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlActivate" runat="server">
            <asp:ListItem Value = "1"> True</asp:ListItem>
            <asp:ListItem Value = "0"> False</asp:ListItem>
            </asp:DropDownList>
        </td>
        <td>&nbsp;</td>
    </tr>    
    <tr>
        <td>Location Directions</td>
        <td></td>
        <td class="style1">
            
            <cc1:Editor ID="tbUpdatedTextbox" Width="457px" Height="524px" runat="server" />
            
            <br />
            
            </td>
        <td></td>
    
    </tr>

    <tr>
        <td>Location Picture Upload</td>
        <td></td>
        <td class="style1">
            <asp:FileUpload ID="fileUploadLocationPic" onchange="ButtonClick()" runat="server" />
        
        <div style="display:none;">
                <asp:Button ID="btnPreviewImage" runat="server" CausesValidation="false" Text="" 
                onclick="btnPreviewImage_Click" />
                </div>
            <asp:Label ID="lblFileUploadError" runat="server" 
                ForeColor="Red"></asp:Label>
        </td>
        <td>
            &nbsp;</td>
    
    </tr>

    <tr>
        <td>Location Picture</td>
        <td></td>
        <td class="style1">
            <asp:CheckBox ID="cbClearImage" Visible="false" Text="Replace the image with 'No Image'" runat="server" />            
            <br />
            <asp:Image ID="imageChargingLocation" runat="server" />
            
        </td>
        <td></td>    
    </tr>

    <tr>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td class="style1">&nbsp;</td>
        <td>&nbsp;</td>    
    </tr>



    </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




          
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" onclick="btnNewClick"/>      
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" CausesValidation="false" onclick="btnClearClick"/>       




          
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" onclick="btnUpdateClick"/>    
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>