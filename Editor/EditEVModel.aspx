<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>
<%@ Page Title="Edit EV Model" Language="C#"  AutoEventWireup="true" CodeBehind="EditEVModel.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditEVModel" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit.HTMLEditor" TagPrefix="cc1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .auto-style1 {
            width: 227px;
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
    <h2>Edit Electric Vehicle Models</h2>
    <p>
        <asp:Label ID="lblCatchError" runat="server" 
            ForeColor="Red" Text="" Visible="False"></asp:Label>
        &nbsp;<asp:Button ID="btnHideCatchError" runat="server" 
            CausesValidation="False" onclick="btnHideCatchError_Click" 
            Text="Hide" Visible="False" />
        <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
        
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
                    <h2>Electric Vehicle List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvEVModel" runat="server" CssClass="datatable" 
                     GridLines="Vertical"
           OnPageIndexChanging="gvEVModelPaging" onrowcreated="gvEVModelRowCreated" 
                     OnSorting="gvEVModelSorting" onselectedindexchanged="gvEVModelSelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="ID" Width="700px" PageSize="10">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID"/>
            <asp:BoundField DataField="Manufacturer" HeaderText="Manufacturer" SortExpression="Manufacturer"/>
            <asp:BoundField DataField="Model" HeaderText="Model" SortExpression="Model"/>
            <asp:BoundField DataField="Level1MaxCurrent" HeaderText="Level 1 Max Current" SortExpression="Level1MaxCurrent" />
            <asp:BoundField DataField="Level1MaxVoltage" HeaderText="Level 1 Max Voltage" SortExpression="Level1MaxVoltage" />
            <asp:BoundField DataField="Level1MaxPower" HeaderText="Level 1 Max Power" SortExpression="Level1MaxPower" />
            <asp:BoundField DataField="Level2MaxCurrent" HeaderText="Level 2 Max Current" SortExpression="Level2MaxCurrent" />
            <asp:BoundField DataField="Level2MaxVoltage" HeaderText="Level 2 Max Voltage" SortExpression="Level2MaxVoltage" />
            <asp:BoundField DataField="Level2MaxPower" HeaderText="Level 2 Max Power" SortExpression="Level2MaxPower" />
            <asp:BoundField DataField="Level3MaxCurrent" HeaderText="Level 3 Max Current" SortExpression="Level3MaxCurrent" />
            <asp:BoundField DataField="Level3MaxVoltage" HeaderText="Level 3 Max Voltage" SortExpression="Level3MaxVoltage" />
            <asp:BoundField DataField="Level3MaxPower" HeaderText="Level 3 Max Power" SortExpression="Level3MaxPower" />
            <asp:BoundField DataField="BatteryCapacity" HeaderText="Battery Capacity" SortExpression="BatteryCapacity" />
            <asp:BoundField DataField="Year" HeaderText="Year" SortExpression="Year" />
            <asp:TemplateField HeaderText="Model Image">
                <ItemTemplate>
                    <asp:Image ID="ModelImage" runat="server" Width="80px" ImageUrl='<%# "~/Editor/ShowImage.ashx?EMID="+ Eval("ID") %>'/>
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
                    <h2>Selected Electric Vehicle Information</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 


    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Manufacturer</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:DropDownList ID="ddlManufacturer" runat="server" Width="225px"></asp:DropDownList>            
        </td>    
        <td>
            <asp:RequiredFieldValidator ID="vldrtbManufacturer" 
                runat="server" ControlToValidate="ddlManufacturer" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
        </td>
    </tr>

     <tr>
        <td nowrap="nowrap">Model</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbModel" runat="server" Width="225px"></asp:TextBox>           
        </td>
        <td>
            <asp:RequiredFieldValidator ID="vldrtbModel" 
                runat="server" ControlToValidate="tbModel" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red"></asp:RequiredFieldValidator>
         </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Level 1 Max Current</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel1MaxCurrent" runat="server" Width="225px"></asp:TextBox>
        </td>   
        <td>Amperes <asp:RangeValidator ID="vldrtbLevel1MaxCurrent" runat="server" 
            ControlToValidate="tbLevel1MaxCurrent" ErrorMessage="Invalid Input" Display="Dynamic"
            MaximumValue="100000" MinimumValue="0" Type="Double" 
                 ForeColor="Red"></asp:RangeValidator> </td>         
     </tr>

     <tr>
        <td nowrap="nowrap">Level 1 Max Voltage</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel1MaxVoltage" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>Volts
            <asp:RangeValidator ID="vldrtbLevel1MaxVoltage" 
                runat="server" ControlToValidate="tbLevel1MaxVoltage" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="100000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td>Level 1 Max Power</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel1MaxPower" runat="server" Width="225px"></asp:TextBox></td>
        <td>Watts
            <asp:RangeValidator ID="vldrtbLevel1MaxPower0" 
                runat="server" ControlToValidate="tbLevel1MaxPower" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="1000000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
     </tr>

     <tr>
        <td nowrap="nowrap">Level 2 Max Current</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel2MaxCurrent" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>Amperes
            <asp:RangeValidator ID="vldrtbLevel2MaxCurrent" 
                runat="server" ControlToValidate="tbLevel2MaxCurrent" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="10000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Level 2 Max Voltage</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel2MaxVoltage" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>Volts <asp:RangeValidator ID="vldrtbLevel2MaxVoltage" 
                runat="server" ControlToValidate="tbLevel2MaxVoltage" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="10000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
     </tr>


     <tr>
        <td>Level 2 Max Power</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel2MaxPower" runat="server" Width="225px"></asp:TextBox></td>
        <td>Watts
            <asp:RangeValidator ID="vldrtbLevel2MaxPower" 
                runat="server" ControlToValidate="tbLevel2MaxPower" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="1000000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
     
     </tr>

    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Level 3 Max Current</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel3MaxCurrent" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>Amperes
            <asp:RangeValidator ID="vldrtbLevel3MaxCurrent" 
                runat="server" ControlToValidate="tbLevel3MaxCurrent" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="10000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
        </td>
    </tr>

    <tr>
        <td nowrap="nowrap" style="width:180px">Level 3 Max Voltage</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel3MaxVoltage" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>Volts
            <asp:RangeValidator ID="vldrtbLevel3MaxVoltage" 
                runat="server" ControlToValidate="tbLevel3MaxVoltage" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="10000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
        </td>
    </tr>  

    <tr style="background-color: #F4F4F4">
        <td>Level 3 Max Power</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbLevel3MaxPower" runat="server" Width="225px"></asp:TextBox></td>
        <td>Watts
            <asp:RangeValidator ID="vldrtbLevel3MaxPower" 
                runat="server" ControlToValidate="tbLevel3MaxPower" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="1000000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
         </td>
    
    </tr>
    
    <tr>
        <td nowrap="nowrap" style="width:180px">Battery Capacity</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:TextBox ID="tbBatteryCapacity" runat="server" Width="225px"></asp:TextBox>
        </td>
        <td>kWh<asp:RangeValidator ID="vldrtbBatteryCapacity" 
                runat="server" ControlToValidate="tbBatteryCapacity" 
                Display="Dynamic" ErrorMessage="Invalid Input" 
                ForeColor="Red" MaximumValue="10000" MinimumValue="0" 
                Type="Double"></asp:RangeValidator>
            
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>Year</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="auto-style1">
            <asp:dropdownlist ID="ddlYear" runat="server" Width="225px"></asp:dropdownlist></td>
        <td>
            <asp:RequiredFieldValidator ID="vldrddlYear" 
                runat="server" ControlToValidate="ddlYear" 
                Display="Dynamic" ErrorMessage="Required Field" 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
        </td>
    
    </tr>

    <tr>
        <td>Location Picture Upload</td>
        <td></td>
        <td class="auto-style1">
            <asp:FileUpload ID="fileUploadLocationPic" runat="server" onchange="ButtonClick()"/> <%-- --%>
        
        <div style="display:none;">
                <asp:Button ID="btnPreviewImage" runat="server" CausesValidation="false" Text="" 
                 onclick="btnPreviewImage_Click"/> <%----%>
                </div>
            
        </td>
        <td>
            <asp:Label ID="lblFileUploadError" runat="server" 
                ForeColor="Red"></asp:Label></td>
    
    </tr>

    <tr style="background-color: #F4F4F4">
        <td>Location Picture</td>
        <td></td>
        <td class="auto-style1">
            <asp:CheckBox ID="cbClearImage" Visible="false" Text="Replace the image with 'No Image'" runat="server" />            
            <br />
            <asp:Image ID="imageEVModel" runat="server" />
            
        </td>
        <td></td>    
    </tr>     
     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




          
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" onclick="btnUpdateClick"/>    
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" CausesValidation="false" onclick="btnClearClick"/>       
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" onclick="btnNewClick"/>      
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>