<%@ Page Title="Edit Charging Box" Language="C#"  AutoEventWireup="true" CodeBehind="EditGateway.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditGateway" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">

        .style1
        {
            width: 400px;
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
                        return "The windows service is restarting. Please Stay on this page for another 10 second. Leave this page will not restart it.";
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
    <h2>Edit Gateway</h2>
        <table>
       <tr>
       <td align="right"> By Organization:</td>           
       <td >
        <asp:DropDownList ID="ddlModeCity" runat="server" AutoPostBack="True" Width="150px" 
                onselectedindexchanged="ddlModeCity_SelectedIndexChanged"></asp:DropDownList>
           </td> <td>
           &nbsp;<asp:Label ID="lblTotalUsers" runat="server" 
                   Font-Bold="True" Font-Size="Medium" Text=""></asp:Label>
               <asp:Label ID="lblCatchError" runat="server" Text="" 
                   ForeColor="Red" Visible="False"></asp:Label>
               <asp:Button ID="btnHideCatchError" runat="server" 
                   Text="Hide" onclick="btnHideCatchError_Click" OnClientClick="DoNotPrompt()"
                   Visible="False" CausesValidation="False" />
           </td>
           <td><asp:Label ID="lblProgress" runat="server" Font-Bold="True" 
                   Font-Size="X-Large" Text=""></asp:Label></td>
           </tr>
       <tr><td align="right">By Parking Lot:</td><td>
        <asp:DropDownList ID="ddlModeParkingLot" runat="server" Width="150px" 
                       AutoPostBack="True" 
                       onselectedindexchanged="ddlModeParkingLot_SelectedIndexChanged"></asp:DropDownList>
           </td>
           <td>
               <asp:Label ID="lblTest" runat="server" Text=""></asp:Label>
               <asp:CheckBox ID="cbShowActivated" runat="server" 
                   AutoPostBack="True" 
                   oncheckedchanged="cbShowActivated_CheckedChanged" />
               <strong>Show Only Activated</strong>
               
               </td>
               <td><asp:Label ID="lblProgressTable" runat="server" Font-Bold="True" 
                   Font-Size="X-Large" Text=""></asp:Label>
                   
                   </td>

                <td><asp:Button ID="btnRestartWindowsService" runat="server" 
                                Font-Size="Large" Text="Restart Windows Services" 
                                visible = "true" CausesValidation="False"
                                onclick="btnRestartWindowsService_Click" onclientclick="DoNotPrompt(); RestartAlert(); this.disabled=true;" 
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
                    <h2>GATEWAY LIST</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvGateway" runat="server" CssClass="datatable" 
                     GridLines="Vertical"
           OnPageIndexChanging="gvGatewayPaging" onrowcreated="gvGatewayRowCreated" 
                     OnSorting="gvGatewaySorting" onselectedindexchanged="gvGatewaySelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="ID" Width="1200px" PageSize="10">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>
            <asp:BoundField DataField="ID" SortExpression="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"/>
            <asp:BoundField DataField="Name" SortExpression="Name" HeaderText="Name"/>
            <asp:BoundField DataField="IP Address" SortExpression="IP Address" HeaderText="IP Address" />                      
            <asp:BoundField DataField="Charging Level" HeaderText="Charging Level" SortExpression="Charging Level" />
            <asp:BoundField DataField="ParkingLotName" HeaderText="Parking Lot Name" SortExpression="ParkingLotName" />
            <asp:BoundField DataField="Network" HeaderText="Network" SortExpression="Network" />
            <asp:BoundField DataField="ChargingName" HeaderText="Charging Algorithm" SortExpression="ChargingName"></asp:BoundField>
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="Max Current" HeaderText="Max Current" SortExpression="Max Current" />
            <asp:BoundField DataField="Max Station Current" HeaderText="Max Station Current" SortExpression="Max Station Current" />
            <asp:BoundField DataField="Max Voltage" HeaderText="Max Voltage" SortExpression="Max Voltage" />
            <asp:BoundField DataField="Max Station Voltage" HeaderText="Max Station Voltage" SortExpression="Max Station Voltage" />
            <asp:BoundField DataField="Source Current" HeaderText="Source Current" SortExpression="Source Current" />
            <asp:BoundField DataField="Source Voltage" HeaderText="Source Voltage" SortExpression="Source Voltage" />
            <asp:BoundField DataField="Retrieve Interval" HeaderText="Retrieve Interval" SortExpression="Retrieve Interval" />            
            <asp:BoundField DataField="Time Quantum" HeaderText="Time Quantum" SortExpression="Time Quantum" />
            <asp:BoundField DataField="Time Out" HeaderText="Time Out" SortExpression="Time Out" />
            <asp:BoundField DataField="Retry Times" HeaderText="Retry Times" SortExpression="Retry Times" />
            <asp:BoundField DataField="CurrentValve" HeaderText="Current Threshold" SortExpression="CurrentValve" />
            <asp:BoundField DataField="NodeControlDelay" HeaderText="Node Control Delay" SortExpression="NodeControlDelay" />
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate" SortExpression="Activate" />
            <asp:CheckBoxField DataField="Enable" HeaderText="Enable" SortExpression="Enable" />
            <asp:CheckBoxField DataField="HasSOC" HeaderText="State of Charge" SortExpression="HasSOC" />
            <asp:CheckBoxField DataField="Controllable" HeaderText="Controllable" SortExpression="Controllable" />
            <asp:CheckBoxField DataField="AggregateControl" HeaderText="Aggregate Control" SortExpression="AggregateControl" />
            <asp:CheckBoxField DataField="AllowReboot" HeaderText="Allow Reboot" SortExpression="AllowReboot" />
            <asp:BoundField DataField="PowerSources" HeaderText="Power Sources" SortExpression="Powersources" />
            <asp:BoundField DataField="InDays" HeaderText="In Days" SortExpression="InDays" />
            <asp:BoundField DataField="MaxTimesInDay" HeaderText="Max Times In Day" SortExpression="MaxTimesInDay" />
            <asp:BoundField DataField="LeftCurrentValve" HeaderText="Left Current Threshold" SortExpression="LeftCurrentValve" />
            <asp:BoundField DataField="ChargingType" HeaderText="Charging Type" SortExpression="ChargingType" />    
            <asp:BoundField DataField="Note" HeaderText="Note" SortExpression="Note" />
            <asp:BoundField DataField="Parking Lot ID" HeaderText="Parking Lot ID" SortExpression="Parking Lot ID" />       
            <asp:BoundField DataField="PrimaryEmail" HeaderText="PrimaryEmail" SortExpression="PrimaryEmail" />              
            <asp:BoundField DataField="ChargingID" HeaderText="ChargingID" SortExpression="ChargingID" />   
            <asp:BoundField DataField="NetworkID" HeaderText="NetworkID" SortExpression="NetworkID" />   
            <asp:BoundField DataField="ChargingTypeID" HeaderText="ChargingTypeID" SortExpression="ChargingTypeID" />
            <asp:BoundField DataField="MinPoint" HeaderText="MinPoint"/>
            <asp:BoundField DataField="MaxDutyCycle" HeaderText="MaxDutyCycle"/>
            <asp:BoundField DataField="MinDutyCycle" HeaderText="MinDutyCycle"/>
            <asp:BoundField DataField="TransactionNodeID" HeaderText="TransactionNodeID"/>
            <asp:BoundField DataField="UseOrganizationPriceList" HeaderText="UseOrganizationPriceList"/>
            <asp:BoundField DataField="StationHasSwitch" HeaderText="StationHasSwitch"/>
            <asp:BoundField DataField="DepartureTimeStop" HeaderText="DepartureTimeStop"/>
            <asp:BoundField DataField="GatewayUsername" HeaderText="GatewayUsername"/>
            <asp:BoundField DataField="GatewayPassword" HeaderText="GatewayPassword"/>
            <asp:BoundField DataField="GatewayEncryptID" HeaderText="GatewayEncryptID"/>
            <asp:BoundField DataField="AllowStopChargeAfterFull" HeaderText="AllowStopChargeAfterFull"/>
            <asp:BoundField DataField="ActiveChargeCount" HeaderText="ActiveChargeCount"/>
            <asp:BoundField DataField="ZigBeeID" HeaderText="ZigBeeID"/>
            <asp:BoundField DataField="StopChargeDelayLoopCount" HeaderText="StopChargeDelayLoopCount" />
            <asp:BoundField DataField="StopChargeIfNotSubmit" HeaderText="StopChargeIfNotSubmit"/>
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
    <h2>SELECTED GATEWAY INFORMATION</h2>
    </div></div></div>

    <div class="mid-outer"><div class="mid-inner"><div class="mid">

    <table style="vertical-align:top"> 
    <tr>
        <td nowrap="nowrap" style="width:180px">ID</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbID" runat="server" ReadOnly="True"></asp:TextBox>
            &nbsp;*Leave Blank if new entry<asp:Label ID="lbltbIDError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td>
        </tr>
    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap"> <asp:Label ID="lblName" runat="server" Text="Name *(Hoverover)" ToolTip="Naming Convention:  PS+(PS Number/Name)+(Station Level)+(Station Number)+(Charging Level).                    For Example: PS8L201LII"></asp:Label>   </td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:TextBox ID="tbName" runat="server"></asp:TextBox>
            <asp:Label ID="lbltbNameError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>
     </tr>
    <tr>
         <td> <asp:Label ID="lblName0" runat="server" 
                Text="Note *(Hoverover)" ToolTip="Naming Convention:  (Parking Lot Number;)+(Floor Level;)+(Voltage Level;)+(Gateway number).                    For Example: Lot 8;Floor 2;120V; 001"></asp:Label>   </td> <%--LeftCurrentValve--%>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbNote" runat="server"></asp:TextBox>
        </td>
     </tr> 
    <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap">IP Address</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:TextBox ID="tbIPaddr" runat="server"></asp:TextBox>
            <asp:RegularExpressionValidator ID="tbIPaddrValidator" 
                runat="server" ControlToValidate="tbIPaddr" 
                ErrorMessage="Invalid IP Address." Display="Dynamic"
               
                    ValidationExpression="(\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b)(:(\b([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-6])\b))?"
                ForeColor="Red"></asp:RegularExpressionValidator>
            <asp:Label ID="lblIPaddrError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>
     </tr>        
    <tr>
         <td>Power Sources</td> 
         <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
         <td class="style1" >
                <asp:DropDownList ID="ddlPowerSources" runat="server">
                    <asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem><asp:ListItem>4</asp:ListItem><asp:ListItem>5</asp:ListItem><asp:ListItem>6</asp:ListItem><asp:ListItem>7</asp:ListItem><asp:ListItem>8</asp:ListItem><asp:ListItem>9</asp:ListItem><asp:ListItem>10</asp:ListItem><asp:ListItem>11</asp:ListItem><asp:ListItem>12</asp:ListItem><asp:ListItem>13</asp:ListItem><asp:ListItem>14</asp:ListItem><asp:ListItem>15</asp:ListItem><asp:ListItem>16</asp:ListItem><asp:ListItem>17</asp:ListItem><asp:ListItem>18</asp:ListItem>
                  <asp:ListItem>19</asp:ListItem>
                  <asp:ListItem>20</asp:ListItem>
                    </asp:DropDownList>
         </td>

     </tr>
    <tr style="background-color: #F4F4F4">
     <td>Network</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1" >
            <asp:DropDownList ID="ddlNetworkID" runat="server">
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="vldrddlNetworkID" 
                runat="server" ControlToValidate="ddlNetworkID" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlNetworkIDError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>
     </tr>
    <tr>
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
    <tr style="background-color: #F4F4F4">
         <td nowrap="nowrap">Parking Lot</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium" >*</td>
         <td class="style1">
             <asp:DropDownList ID="ddlParkingLotNames" runat="server" >
             </asp:DropDownList>

            <asp:RequiredFieldValidator ID="vldrddlParkingLotNames" 
                runat="server" ControlToValidate="ddlParkingLotNames" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlParkingLotNamesError" runat="server" ForeColor="Red" 
                 Text=""></asp:Label>

         </td>
     </tr>

    <tr>
            <td nowrap="nowrap">Algorithm</td>
             <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td nowrap="nowrap" class="style1">
                <asp:DropDownList ID="ddlAlgorithm" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
            <asp:RequiredFieldValidator ID="vldrddlAlgorithm" 
                runat="server" ControlToValidate="ddlAlgorithm" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlAlgorithmError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
            </td>
     </tr>      

    <tr style="background-color: #F4F4F4">
     <td>Enable</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1">
         <asp:DropDownList ID="ddlEnable" runat="server">
            <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td></tr>

    <tr>
     <td>Activate</td> 
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
         <asp:DropDownList ID="ddlActivate" runat="server">
         <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
        </td>
    </tr>

    <tr style="background-color: #F4F4F4">
     <td>State of Charge</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlhasSOC" runat="server">
         <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td>
     </tr>

    <tr>
     <td>Controllable</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlControllable" runat="server">
         <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap">Aggregate Control</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1">
            <asp:DropDownList ID="ddlAggregateControl" runat="server">
         <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td>
     </tr>

    <tr>
     <td>AllowReboot</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlAllowReboot" runat="server">
         <asp:ListItem Value="1">True</asp:ListItem>
            <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
              <td nowrap="nowrap">Level</td>
              <td style="color: #FF0000; font-weight: bold; font-size: medium" > *</td>
              <td class="style1" > 
            <asp:TextBox ID="tbLevel" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="tbLevelValidator" runat="server" Display="Dynamic"
            ControlToValidate="tbLevel" 
                      ErrorMessage="Required Field." ForeColor="Red"></asp:RequiredFieldValidator>
              </td>     
     </tr>
    
    <tr>
         <td nowrap="nowrap">Max Current</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
         <td class="style1" >
            <asp:TextBox ID="tbMaxCurrent" runat="server"></asp:TextBox>
             Amperes
         <asp:RequiredFieldValidator ID="rfvtbMaxCurrent" 
             runat="server" ControlToValidate="tbMaxCurrent" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="tbMaxCurrentValidator" runat="server" 
            ControlToValidate="tbMaxCurrent" ErrorMessage="Must be between 0 - 100." Display="Dynamic"
            MaximumValue="100" MinimumValue="0" Type="Integer" 
                 ForeColor="Red"></asp:RangeValidator>
         </td>     
     </tr>
     
    <tr style="background-color: #F4F4F4">  
             <td nowrap="nowrap">Max Station Current</td>
             <td style="color: #FF0000; font-weight: bold; font-size: medium" >*</td>
             <td class="style1">

            <asp:TextBox ID="tbMaxStationCurrent" runat="server"></asp:TextBox>
                 Amperes
        <asp:RequiredFieldValidator ID="tbMaxStationCurrentValidator" runat="server" Display="Dynamic"
            ControlToValidate="tbMaxStationCurrent" 
                      ErrorMessage="Required Field." 
                  ForeColor="Red"></asp:RequiredFieldValidator>
             </td>     
     </tr>

    <tr>   
             <td nowrap="nowrap">Max Voltage</td>
             <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
             <td class="style1" >
            <asp:TextBox ID="tbMaxVoltage" runat="server"></asp:TextBox>
                 Volts
        <asp:RequiredFieldValidator ID="tbMaxVoltageValidator" runat="server" 
            ControlToValidate="tbMaxVoltage" 
                      ErrorMessage="Required Field." 
                  ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
             </td>
     </tr>

    <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap">Max Station Voltage</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium" >*</td>
        <td class="style1">
            <asp:TextBox ID="tbMaxStationVoltage" runat="server"></asp:TextBox>
            Volts
        <asp:RequiredFieldValidator ID="tbMaxStationVoltageValidator" runat="server" 
            ControlToValidate="tbMaxStationVoltage" 
                      ErrorMessage="Required Field." 
                  ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
         </td>
     </tr>

    <tr>
     <td nowrap="nowrap">Source Current</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium" >*</td>
     <td class="style1">
            <asp:TextBox ID="tbSourceCurrent" runat="server"></asp:TextBox>
            Amperes
        <asp:RequiredFieldValidator ID="tbSourceCurrentValidator" runat="server" 
            ControlToValidate="tbSourceCurrent" 
                      ErrorMessage="Required Field." 
                  ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
         </td>
     </tr>

     
    <tr style="background-color: #F4F4F4">
     <td nowrap="nowrap">Source Voltage</td>
      <td style="color: #FF0000; font-weight: bold; font-size: medium" 
             >*</td>
     <td class="style1">
            <asp:TextBox ID="tbSourceVoltage" runat="server"></asp:TextBox>
            Volts
        <asp:RequiredFieldValidator ID="tbSourceVoltageValidator" runat="server" 
            ControlToValidate="tbSourceVoltage" 
                      ErrorMessage="Required Field." 
                  ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
         </td>
     </tr>
    <tr>
     <td>Retrieve Interval</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbRetrieveInterval" runat="server" 
                Height="22px"></asp:TextBox>
            Seconds&nbsp;
         <asp:RequiredFieldValidator ID="rfvtbRetrieveInterval" 
             runat="server" ControlToValidate="tbRetrieveInterval" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="tbRetrieveIntervalRange" runat="server" 
            ControlToValidate="tbRetrieveInterval" ErrorMessage="Must be between 0-1000" 
            MaximumValue="1000" MinimumValue="0" Type="Integer" 
                ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
         </td></tr>
      
    <tr style="background-color: #F4F4F4">
     <td>Time Quantum</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1">
            <asp:TextBox ID="tbTimeQuantum" runat="server"></asp:TextBox>
            Minutes
         <asp:RequiredFieldValidator ID="rfvtbTimeQuantum" 
             runat="server" ControlToValidate="tbTimeQuantum" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
         </td>
     </tr>
    <tr>
     <td>Time Out</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td class="style1">
            <asp:TextBox ID="tbTimeOut" runat="server"></asp:TextBox>
            Seconds
            <asp:RequiredFieldValidator ID="rfvtbTimeOut" 
             runat="server" ControlToValidate="tbTimeOut" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="tbTimeOutValidator" runat="server" 
            ControlToValidate="tbTimeOut" ErrorMessage="Must be between 0-50." 
            MaximumValue="50" MinimumValue="0" Type="Integer" 
                ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
         </td></tr>
    <tr style="background-color: #F4F4F4">
     <td>Retry Times</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbRetryTime" runat="server"></asp:TextBox>
        &nbsp;<asp:RequiredFieldValidator ID="rfvtbRetryTime" 
             runat="server" ControlToValidate="tbRetryTime" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="tbRetryTimeValidator" runat="server" 
            ControlToValidate="tbRetryTime" ErrorMessage="Must be between 0 - 5." 
            MaximumValue="5" MinimumValue="0" Type="Integer" 
                ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
         </td></tr>

    <tr>
         <td>Current Threshold</td> <%--Current Valve--%>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbCurrentThreshold" runat="server"></asp:TextBox>
                Amperes
             <asp:RequiredFieldValidator ID="rfvtbCurrentThreshold" 
                 runat="server" ControlToValidate="tbCurrentThreshold" 
                 Display="Dynamic" ErrorMessage="Required Field." 
                 ForeColor="Red"></asp:RequiredFieldValidator>
         &nbsp;<asp:RangeValidator ID="tbCurrentThresholdValidator" runat="server" 
                ControlToValidate="tbCurrentThreshold" ErrorMessage="Must be between 0 - 1." 
                MaximumValue="1" MinimumValue="0" Type="Double" 
                    ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
             </td>
      </tr>

    <tr style="background-color: #F4F4F4">
     <td>Node Control Delay</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbNodeControlDelay" runat="server"></asp:TextBox>
            ms
         <asp:RequiredFieldValidator ID="rfvtbNodeControlDelay" 
             runat="server" ControlToValidate="tbNodeControlDelay" 
             Display="Dynamic" ErrorMessage="Required Field." 
             ForeColor="Red"></asp:RequiredFieldValidator>
     &nbsp;<asp:RangeValidator ID="tbNodeControlDelayValidator" runat="server" 
            ControlToValidate="tbNodeControlDelay" ErrorMessage="Must be between 0-10000." 
            MaximumValue="10000" MinimumValue="0" Type="Integer" 
                ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
         </td>
     </tr>
    <tr>
         <td>In Days</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             &nbsp;</td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbInDays" runat="server"></asp:TextBox>
             </td>
     </tr>

    <tr style="background-color: #F4F4F4">
         <td>Max Times per Day</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             &nbsp;</td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbMaxTimesPerDay" runat="server"></asp:TextBox>
             </td>
      </tr>

    <tr>
         <td>Left Current Threshold</td> <%--LeftCurrentValve--%>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             &nbsp;</td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:TextBox ID="tbLeftCurrentThreshold" runat="server"></asp:TextBox>
                Amperes
         </td>

     </tr>

    <tr style="background-color: #F4F4F4">
         <td>Primary Email</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             </td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbPrimaryEmail" runat="server"></asp:TextBox>
                <asp:RegularExpressionValidator
                            ID="RegularExpressionValidator1"
                            RunAt="server"
                            ControlToValidate="tbPrimaryEmail" 
                            ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,3}$"
                            ErrorMessage="Valid Email Address required" 
                    ForeColor="Red" Display="Dynamic" />
         </td>
     </tr>

    <tr>
         <td>Charging Type</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             *</td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:DropDownList ID="ddlChargingType" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
            <asp:RequiredFieldValidator ID="vldrddlChargingType" 
                runat="server" ControlToValidate="ddlChargingType" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlChargingTypeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>

     </tr>

    <tr style="background-color: #F4F4F4">
         <td>Min Point</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             *</td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:DropDownList ID="ddlMinPoint" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
         </td>
     </tr>

    <tr>
         <td>Max Duty Cycle</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             *</td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:DropDownList ID="ddlMaxDutyCycle" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
         <td>Min Duty Cycle</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             *</td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:DropDownList ID="ddlMinDutyCycle" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
         </td>
     </tr>

    <tr>
         <td>TransactionNodeID</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
         <td style="color: #808080; font-weight: normal" class="style1" >
                <asp:DropDownList ID="ddlTransactionNodeID" runat="server" 
                    style="margin-bottom: 0px" >
                </asp:DropDownList>
<%--             <asp:RequiredFieldValidator ID="vldrddlTransactionNodeID" 
                runat="server" ControlToValidate="ddlTransactionNodeID" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
         &nbsp;<asp:Label ID="lblddlTransactionNodeIDError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>--%>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
     <td>Use Org. Price List</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlUseOrganizationPriceList" runat="server">
                <asp:ListItem Value="0">False</asp:ListItem>
                <asp:ListItem Value="1">True</asp:ListItem>
                

         </asp:DropDownList>
         </td>
     </tr>

    <tr>
     <td>Station Has Switch</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlStationHasSwitch" runat="server">
         
         <asp:ListItem Value="1">True</asp:ListItem>
         <asp:ListItem Value="0">False</asp:ListItem>
         </asp:DropDownList>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
     <td>Departure Time Stop</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlDepartureTimeStop" runat="server">
         <asp:ListItem Value="0">False</asp:ListItem>
         <asp:ListItem Value="1">True</asp:ListItem>       
         </asp:DropDownList>
         </td>
     </tr>

    <tr>
     <td>Gateway Username</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:Textbox ID="tbGatewayUsername" runat="server" Text="admin">         
         </asp:Textbox>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
     <td>Gateway Password</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbGatewayPassword" runat="server" Text ="admin">    
         </asp:TextBox>
         </td>
     </tr>
    
    <tr>
     <td>Gateway EncryptID</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:Textbox ID="tbGatewayEncryptID" runat="server" Text="Base64">         
         </asp:Textbox>
         </td>
     </tr>
        
    <tr style="background-color: #F4F4F4">
     <td>Allow Stop Charge After Full</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlAllowStopChargeAfterFull" runat="server">
        <asp:ListItem Value="1">True</asp:ListItem>  
        <asp:ListItem Value="0">False</asp:ListItem>     
         </asp:DropDownList>
         </td>
     </tr>
    
    <tr>
     <td>Active Charge Count</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
     <asp:DropDownList ID="ddlActiveChargeCount" runat="server">
                    <asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem><asp:ListItem>4</asp:ListItem><asp:ListItem>5</asp:ListItem><asp:ListItem>6</asp:ListItem><asp:ListItem>7</asp:ListItem><asp:ListItem>8</asp:ListItem><asp:ListItem>9</asp:ListItem><asp:ListItem>10</asp:ListItem><asp:ListItem>11</asp:ListItem><asp:ListItem>12</asp:ListItem><asp:ListItem>13</asp:ListItem><asp:ListItem>14</asp:ListItem><asp:ListItem>15</asp:ListItem><asp:ListItem>16</asp:ListItem><asp:ListItem>17</asp:ListItem><asp:ListItem>18</asp:ListItem>
                  <asp:ListItem>19</asp:ListItem>
                  <asp:ListItem>20</asp:ListItem>
                    </asp:DropDownList>
         </td>
     </tr>

    <tr style="background-color: #F4F4F4">
     <td>Zigbee ID</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:TextBox ID="tbZigbeeId" runat="server" Text ="">    
         </asp:TextBox>
         </td>
     </tr>

    <tr>
     <td>Stop Charge Delay Loop Count</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
     <asp:DropDownList ID="ddlStopChargeDelayLoopCount" runat="server">
                    <asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem><asp:ListItem>4</asp:ListItem><asp:ListItem>5</asp:ListItem><asp:ListItem>6</asp:ListItem><asp:ListItem>7</asp:ListItem><asp:ListItem>8</asp:ListItem><asp:ListItem>9</asp:ListItem><asp:ListItem>10</asp:ListItem><asp:ListItem>11</asp:ListItem><asp:ListItem>12</asp:ListItem><asp:ListItem>13</asp:ListItem><asp:ListItem>14</asp:ListItem><asp:ListItem>15</asp:ListItem><asp:ListItem>16</asp:ListItem><asp:ListItem>17</asp:ListItem><asp:ListItem>18</asp:ListItem>
                  <asp:ListItem>19</asp:ListItem>
                  <asp:ListItem>20</asp:ListItem>
                    </asp:DropDownList>
         </td>
     </tr>
        
    <tr style="background-color: #F4F4F4">
     <td>Stop Charge If Not Submit</td>
     <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
     <td style="color: #808080; font-weight: normal" class="style1">
            <asp:DropDownList ID="ddlStopChargeIfNotSubmit" runat="server" >
                        <asp:ListItem Value="1">True</asp:ListItem>  
        <asp:ListItem Value="0">False</asp:ListItem>  
         </asp:DropDownList>
         </td>
     </tr>
    <%-- <tr>
         <td>test</td>
         <td style="color: #FF0000; font-weight: bold; font-size: medium">
             &nbsp;</td>
         <td style="color: #808080; font-weight: normal" class="style1">
                <asp:TextBox ID="tbTest" runat="server"></asp:TextBox>
             </td>
     </tr>--%>


     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner"><div class="bottom">                
    </div></div></div> <%--end bottom div x 3--%>
    </div> <%-- end div round--%>
    </div> <%-- end div grid --%>




          
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" OnClientClick="DoNotPrompt()" onclick="btnUpdateClick"/>    
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" CausesValidation="false" OnClientClick="DoNotPrompt()" onclick="btnClearClick"/>       
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" onclick="btnNewClick" OnClientClick="DoNotPrompt()"/>      
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>