<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>
<%@ Page Title="Edit Station" Language="C#"  AutoEventWireup="true" CodeBehind="EditStation.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditStation" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .style1
        {
            width: 400px;
        }
        .style2
        {
            height: 34px;
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

<%--Function for RestartWindowsServices--%>
<script language="javascript" type="text/javascript"></script>
<script>
    // Check Browser
    var isFirefox = typeof InstallTrigger !== 'undefined';   // Firefox 1.0+
    // At least Safari 3+: "[object HTMLElementConstructor]"
    var isChrome = !!window.chrome;                          // Chrome 1+
    var isIE = /*@cc_on!@*/false;                            // At least IE6
    // Variable to select when onbeforeunload is called
    function DoNotPrompt() {
        prompt = false;
    }

    function RestartAlert() {
        alert("Windows Services will restart after closing this window.  (Approximately 10 second wait time)");
    }

    function disableBtnRestart() {
        document.getElementById('<%= btnRestartWindowsService.ClientID %>').disabled = true;
    }

    // Check if page is postback or browser exit.
    var IsPostbackVar = document.getElementById('__EVENTTARGET');



    // If IE or Firefox,
    if (isIE || isFirefox) {
        window.onbeforeunload = iEFFpageleave;
        function iEFFpageleave() {
            if (IsPostbackVar && IsPostbackVar.value) {
                // This is a postback,
                // Do not do anything, it will distrupt user experience
            }
            else {
                if (prompt) {
                    if (document.getElementById('<%= btnRestartWindowsService.ClientID %>').disabled) {
                        // If this button is disabled, then nothing was updated
                        // Don't alert or call any function.                                       
                    }
                    else {
                        // Restart Windows Services
                        document.getElementById('<%= btnRestartWindowsService.ClientID %>').click();
                        document.getElementById('<%= btnRestartWindowsService.ClientID %>').disabled = true;
                        return "Windows services will be restarted automatically after either leaving or staying on this page.  (10 second wait time)";
                    } 
                }
                else {
                    // Reset prompt variable
                    prompt = true;
                }
            }
        }
    }


    // If Chrome browser
    if (isChrome) {
        window.onbeforeunload = pageleave;
        function pageleave() {
            if (IsPostbackVar && IsPostbackVar.value) {
                // This is a postback,
                // Do not do anything, it will distrupt user experience
            }
            else {
                if (prompt) {

                    // If the Restart Windoes button is disabled, i.e. there is no restart needed,
                    // Then do not do anything
                    if (document.getElementById('<%= btnRestartWindowsService.ClientID %>').disabled) {
                        // If this button is disabled, then nothing was updated
                        // Don't alert or call any function.

                    }
                    // Otherwise, Give the option to restart the windows services
                    else {
                        document.getElementById('<%= btnRestartWindowsService.ClientID %>').click();
                        document.getElementById('<%= btnRestartWindowsService.ClientID %>').disabled = true;
                        return "Staying on this page will restart windows services, leaving this page will not restart it.";
                    }
                }
                else {
                    // Reset prompt variable
                    prompt = true;
                }
            }
        }
    }
</script>
<%--Start Page layout--%>
    <h2>Edit Charging Station</h2>
    <input type="hidden" id="hiddenvar" runat="server" />
        <table>
       <tr>
       <td align="right" class="style2"> By Organization:</td>           
       <td class="style2" >
        <asp:DropDownList ID="ddlModeCity" runat="server" AutoPostBack="True" Width="150px" 
                onselectedindexchanged="ddlModeCity_SelectedIndexChanged"></asp:DropDownList>
           </td> <td class="style2">
               <asp:Label ID="lblCatchError" runat="server" Text="" 
                   ForeColor="Red" Visible="False"></asp:Label>
           &nbsp;<asp:Button ID="btnHideCatchError" runat="server" OnClientClick="DoNotPrompt()"
                   Text="Hide" onclick="btnHideCatchError_Click" 
                   Visible="False" CausesValidation="False" />
           </td>
           </tr>
       <tr><td align="right">By Parking Lot:</td><td>
        <asp:DropDownList ID="ddlModeParkingLot" runat="server" Width="150px" 
                       AutoPostBack="True" 
                       onselectedindexchanged="ddlModeParkingLot_SelectedIndexChanged"></asp:DropDownList>
           </td>
           <td>
               <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
               <asp:Label ID="lblTotalUsers" runat="server" 
                   Font-Bold="True" Font-Size="Medium" Text=""></asp:Label>
               </td>
               
           </tr>
           <tr><td align="right">By Charging Box:</td>
           <td>
               <asp:DropDownList ID="ddlModeGateway" runat="server" width="150px"
                   
                   onselectedindexchanged="ddlModeGateway_SelectedIndexChanged" 
                   AutoPostBack="True">
               </asp:DropDownList>
               </td>
           <td>
               <asp:CheckBox ID="cbShowActivated" runat="server" 
                   AutoPostBack="True" 
                   oncheckedchanged="cbShowActivated_CheckedChanged" />
               <strong>Show Only Activated</strong>
               <strong>      
       <asp:Button ID="btnRestartWindowsService" runat="server" 
                   Font-Size="Large" Text="Restart Windows Services" 
                   visible = "true" CausesValidation="False"
                   onclick="btnRestartWindowsService_Click" onclientclick="DoNotPrompt(); RestartAlert(); this.disabled = true;" 
                   Enabled="False" UseSubmitBehavior="False"/> 
               </td>
               <td><asp:Label runat="server" ID ="lblRestartWindowsService" Text="*Do not click more than once"></asp:Label></td>
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
                    <h2>Station List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvStation" runat="server" CssClass="datatable" 
                     GridLines="Vertical" 
           OnPageIndexChanging="gvStationPaging" onrowcreated="gvStationRowCreated" 
                     OnSorting="gvStationSorting" onselectedindexchanged="gvStationSelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="ID" Width="1200px">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>     
            <asp:BoundField DataField="ID" SortExpression="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"/>          
            <asp:BoundField DataField="Gateway" SortExpression="Gateway" HeaderText="Charging Box"/>
            <asp:BoundField DataField="Name" SortExpression="Name" HeaderText="Station Name" />                
            
            <asp:BoundField DataField="Manufacturer" HeaderText="Manufacturer" SortExpression="Manufacturer" />
            <asp:BoundField DataField="Charging Level" HeaderText="Charging Level" SortExpression="Charging Level"></asp:BoundField>
            <asp:BoundField DataField="Space No" HeaderText="Space No" SortExpression="Space No" />
            <asp:BoundField DataField="Latitude" HeaderText="Latitude" SortExpression="Latitude" />
            <asp:BoundField DataField="Longitude" HeaderText="Longitude" SortExpression="Longitude" />
            <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" />            
            <asp:BoundField DataField="Note" HeaderText="Note" SortExpression="Note" />     
            <asp:BoundField DataField="Gateway ID" HeaderText="Gateway ID" SortExpression="Gateway ID" />
            <asp:BoundField DataField="Base Value" SortExpression="Base Value" HeaderText="Base Value"/>
            <asp:BoundField DataField="Start Value" SortExpression="Start Value" HeaderText="Start Value"/>
            <asp:BoundField DataField="Relay Channel" SortExpression="Relay Channel" HeaderText="Relay Channel"/>
            <asp:BoundField DataField="PowerSourceNo" HeaderText="Power Source" SortExpression="PowerSource"/>
            <asp:BoundField DataField="ChargingType" HeaderText="Charging Type" SortExpression="ChargingType"/>
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate"  SortExpression="Activate" />
            <asp:CheckBoxField DataField="Controllable" HeaderText="Controllable" SortExpression="Controllable" />
            <asp:CheckBoxField DataField="Enable" HeaderText="Enable" SortExpression="Enable" />
            <asp:CheckBoxField DataField="AC Meter" HeaderText="AC Meter" SortExpression="AC Meter" />
            <asp:BoundField DataField="CreateTime" HeaderText="CreateTime" SortExpression="CreateTime"/>
            <asp:BoundField DataField="ChargingTypeID" HeaderText="ChargingTypeID" SortExpression="ChargingTypeID"/>  
               
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


    <td valign=top>
    <div class="grid">


            <div class="rounded">
                <div class="top-outer"><div class="top-inner"><div class="top">
                    <h2>Selected Station Information</h2></div></div></div>
                <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 

    <tr style="background-color: #F4F4F4">

        <td nowrap="nowrap" style="width:180px">ID</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbID" runat="server" MaxLength="16"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvtbID" 
                runat="server" ControlToValidate="tbID" Display="Dynamic" ForeColor="Red"
                ErrorMessage="Required Field."></asp:RequiredFieldValidator>
            <asp:Label ID="lbltbIDError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        &nbsp;</td> </tr>

     <tr>
        <td nowrap="nowrap"> <asp:Label ID="lblName" runat="server" 
                Text="Name *(Hoverover)" ToolTip="Naming Convention:  PS+(PS Number/Name)+(Station Level)+(Station Number)+(Charging Level).                    For Example: PS09L401LII"></asp:Label>   </td>
         
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:TextBox ID="tbName" runat="server"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvtbName" 
                runat="server" ControlToValidate="tbName" Display="Dynamic" ForeColor="Red"
                ErrorMessage="Required Field."></asp:RequiredFieldValidator>
            <asp:Label ID="lbltbNameError" runat="server" ForeColor="Red" Text=""></asp:Label>
         </td>
     </tr>     
     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap">Charging Box</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
             <asp:DropDownList ID="ddlGateway" runat="server">
             </asp:DropDownList>
            <asp:RequiredFieldValidator ID="vldrddlGateway" 
                runat="server" ControlToValidate="ddlGateway" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlGatewayError" runat="server" ForeColor="Red" Text=""></asp:Label>
         </td>
     </tr>  

     <tr>
        <td nowrap="nowrap" style="width:180px">Space Number</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbSpaceNumber" runat="server" 
                MaxLength="50"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvtbSpaceNumber" Display="Dynamic"
                runat="server" ControlToValidate="tbSpaceNumber" ForeColor="Red"
                ErrorMessage="Required Field."></asp:RequiredFieldValidator>

            <asp:Label ID="lbltbSpaceNumberError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td> </tr>

      <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Latitude (Degrees)</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbLatitude" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator ID="rfvtbLatitude" 
             runat="server" ControlToValidate="tbLatitude" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
            <asp:RangeValidator ID="rvtbLatitude" runat="server" 
            ControlToValidate="tbLatitude" Display="Dynamic" ErrorMessage="Values must be between -90 and 90." MaximumValue="90" 
            MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            <asp:Label ID="lbltbLatitudeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td> </tr>

        <tr>
        <td nowrap="nowrap" style="width:180px">Longitude (Degrees)</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbLongitude" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator ID="rfvtbLongitude" 
             runat="server" ControlToValidate="tbLongitude" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
            &nbsp;<asp:RangeValidator ID="rvtbLongitude" runat="server" 
            ControlToValidate="tbLongitude" ErrorMessage="Values must be between -180 and 180." MaximumValue="180" 
            MinimumValue="-180" ForeColor="Red" Display="Dynamic" Type="Double"></asp:RangeValidator>

            <asp:Label ID="lbltblLongitudeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td> </tr>

        <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Priority</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlPriority" runat="server">
                <asp:ListItem>0</asp:ListItem>
                <asp:ListItem>1</asp:ListItem>
                <asp:ListItem>2</asp:ListItem>
                <asp:ListItem>3</asp:ListItem>
                <asp:ListItem>4</asp:ListItem>
                <asp:ListItem>5</asp:ListItem>
                <asp:ListItem>6</asp:ListItem>
                <asp:ListItem>7</asp:ListItem>
                <asp:ListItem>8</asp:ListItem>
                <asp:ListItem>9</asp:ListItem>
                <asp:ListItem>10</asp:ListItem>
            </asp:DropDownList>
            <asp:Label ID="lbltbPriorityError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td> </tr>

        <tr>
        <td nowrap="nowrap" style="width:180px">Manufacturer</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbManufacturer" runat="server"
                MaxLength="50"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfvtbManufacturer" 
                runat="server" ControlToValidate="tbManufacturer" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red"></asp:RequiredFieldValidator>
            &nbsp;<asp:Label ID="lbltbManufacturerError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td> </tr>


  
     <tr style="background-color: #F4F4F4">
          <td nowrap="nowrap">Charging Level</td>
          <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
          <td class="style1">
              <asp:DropDownList ID="ddlChargingLevel" runat="server">
                  <asp:ListItem>1</asp:ListItem>
                  <asp:ListItem>2</asp:ListItem>
                  <asp:ListItem>3</asp:ListItem>
              </asp:DropDownList>
          </td>
     </tr>
  
     <tr>
     <td>Enable</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1">
         <asp:DropDownList ID="ddlEnable" runat="server">
            <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td></tr>
     </tr>

      <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Controllable</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:DropDownList ID="ddlControllable" runat="server">
                <asp:ListItem Value="1">True</asp:ListItem>
                <asp:ListItem Value="0">False</asp:ListItem>
            </asp:DropDownList>
        </td> </tr>


     <tr>
     <td>Activate</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlActivate" runat="server">
            <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
     </td></tr>

     <tr style="background-color: #F4F4F4">
     <td>Start Value</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:TextBox ID="tbStartValue" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator ID="rfvtbStartValue" 
             runat="server" ControlToValidate="tbStartValue" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="rvtbStartValue" runat="server" 
            ControlToValidate="tbStartValue" 
             ErrorMessage="Numeric values only" MaximumValue="999999" 
            MinimumValue="-99999" ForeColor="Red" Display="Dynamic" 
             Type="Double"></asp:RangeValidator>

     </td></tr>

     <tr>
     <td>Base Value</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:TextBox ID="tbBaseValue" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator ID="rfvtbBaseValue1" 
             runat="server" ControlToValidate="tbBaseValue" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
         &nbsp;<asp:RangeValidator ID="rvtbBaseValue" runat="server" 
            ControlToValidate="tbBaseValue" 
             ErrorMessage="Numeric values only" MaximumValue="999999" 
            MinimumValue="-999999" ForeColor="Red" Display="Dynamic" 
             Type="Double"></asp:RangeValidator>

     </td></tr>

     <tr style="background-color: #F4F4F4">
     <td>Relay Channel</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlRelayChannel" runat="server"></asp:DropDownList>
     </td></tr>
    
     <tr>
     <td>Power Source</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlPowerSource" runat="server"></asp:DropDownList>
     </td></tr>
    
     <tr style="background-color: #F4F4F4">
     <td>Charging Type</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlChargingType" runat="server"></asp:DropDownList>
         <asp:RequiredFieldValidator ID="vldrddlChargingType" 
                runat="server" ControlToValidate="ddlChargingType" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlChargingTypeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
     </td></tr>
    
     <tr>
     <td>AC Meter</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlACMeter" runat="server">
            <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
     </td></tr>



     <tr style="background-color: #F4F4F4">
     <td>Note</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbNote" runat="server" Height="117px" 
                TextMode="MultiLine" Width="276px" MaxLength="200"></asp:TextBox>
         </td></tr>


     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




          
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" OnClientClick="DoNotPrompt()" onclick="btnUpdateClick"/>    
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" OnClientClick="DoNotPrompt()" CausesValidation="false" onclick="btnClearClick"/>       
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" OnClientClick="DoNotPrompt()" onclick="btnNewClick"/>      
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>