<%@ Page Title="Edit ChargingRecord" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChargingRecord.aspx.cs" Inherits="EVEditor.ChargingRecord" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
.style1
{width: 400px;}
    .auto-style1 {
        width: 185px;
    }
</style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<h2>Edit ChargingRecord</h2>
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
<%--2nd Row --%>
<tr>
    <td>Show Enclosed Record</td>
    <td><asp:CheckBox ID="cbShowIsEnd" runat="server" AutoPostBack="True" oncheckedchanged="cbShowIsEndCheckedChanged" /></td>
</tr>
</table>
<p></p>
<table>
    <tr>
        <td valign="top">
            <table>
                <tr>
                    <td valign="top">
                        <div class="grid">
                            <div class="rounded">
                                <div class="top-outer"><div class="top-inner"><div class="top">
                                    <h2>Charging Record</h2>
                                </div>
                            </div>
                        </div>
                        <div class="mid-outer">
                            <div class="mid-inner">
                                <div class="mid">
                                    <asp:GridView ID="gvChargingRecord" runat="server" CssClass="datatable" 
                                    GridLines="Vertical"  OnPageIndexChanging="gvChargingRecordPaging" onselectedindexchanged="gvChargingRecordSelectedIndex" 
                                    onrowcreated =" gvChargingRecordRowCreated" AllowPaging="True" CellPadding="0" 
                                    BorderWidth="0px" AutoGenerateColumns="False" 
                                    DataKeyNames="ID" Width="800px" PageSize="20" >   
                                        <HeaderStyle Wrap="False" />
                                    <Columns>
                                        <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
                                        <asp:BoundField DataField="ID" HeaderText="ID"  />
                                        <asp:BoundField DataField="UserID" HeaderText="UserID"  />
                                        
                                        <asp:BoundField DataField="EnergyPrice" HeaderText="EnergyPrice"  /> 
                                        <asp:BoundField DataField="ChargingAlgorithm" HeaderText="ChargingAlgorithm" />
                                        <asp:BoundField DataField="IsInCharging" HeaderText="IsInCharging"  />
                                        <asp:BoundField DataField="ChargingTimes" HeaderText="ChargingTimes"  />                                                 
                                        <asp:BoundField DataField="Priority" HeaderText="Priority"  />
                                        <asp:BoundField DataField="StartVoltage" HeaderText="StartVoltage"  />
                                        <asp:BoundField DataField="StartCurrent" HeaderText="StartCurrent"  />                                                   
                                        <asp:BoundField DataField="StartPF" HeaderText="StartPF"  />
                                        <asp:BoundField DataField="StartActivePower" HeaderText="StartActivePower"  />
                                        <asp:BoundField DataField="StartApparentPower" HeaderText="StartApparentPower"  />
                                        <asp:BoundField DataField="EndVoltage" HeaderText="EndVoltage"  />
                                        <asp:BoundField DataField="EndCurrent" HeaderText="EndCurrent"  />                                                   
                                        <asp:BoundField DataField="EndPF" HeaderText="EndPF"  />
                                        <asp:BoundField DataField="EndActivePower" HeaderText="EndActivePower"  />
                                        <asp:BoundField DataField="EndApparentPower" HeaderText="EndApparentPower"  />
                                        <asp:BoundField DataField="LastStartCharging" HeaderText="LastStartCharging" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />
                                        <asp:BoundField DataField="LastStartMainPower" HeaderText="LastStartMainPower"  />
                                        <asp:BoundField DataField="LastStopCharging" HeaderText="LastStopCharging" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />
                                        <asp:BoundField DataField="LastStopMainPower" HeaderText="LastStopMainPower"  />                                                   
                                        <asp:BoundField DataField="IsEndedByUser" HeaderText="IsEndedByUser"  />
                                        <asp:BoundField DataField="ScheduleID" HeaderText="ScheduleID"  />
                                        <asp:BoundField DataField="ChargingCost" HeaderText="ChargingCost"  />
                                        <asp:BoundField DataField="TotalCharingTime" HeaderText="TotalChargingTime"  />
                                        <asp:BoundField DataField="SOC" HeaderText="SOC"  />
                                        <asp:BoundField DataField="SOCRetrieveTime" HeaderText="SOCRetrieveTime" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />                                                   
                                        <asp:BoundField DataField="CalculateCO2" HeaderText="CalculateCO2"  />
                                        <asp:BoundField DataField="LeaveTime" HeaderText="LeaveTime" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />
                                        <asp:BoundField DataField="OdometerReading" HeaderText="OdometerReading"  />
                                        <asp:BoundField DataField="AggregateControl" HeaderText="AggregateControl"  />


                                        <asp:BoundField DataField="UserName" HeaderText="User Name" />                                        
                                        <asp:BoundField DataField="StationName" HeaderText="Station Name"  />
                                        <asp:BoundField DataField="StartTime" HeaderText="Start Time" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />
                                        <asp:BoundField DataField="StartMainPower" HeaderText="Start Main Power"  />
                                        <asp:BoundField DataField="IsEnd" HeaderText="Is End"/>
                                        <asp:BoundField DataField="EndTime" HeaderText="End Time" DataFormatString="{0:MM/dd/yyyy HH:mm:ss}" />
                                        <asp:BoundField DataField="EndMainPower" HeaderText="End Main Power" />
                                        <asp:BoundField DataField="EmailAddress" HeaderText="Email Address" />
                                        <asp:BoundField DataField="ZipCode" HeaderText="Zip Code" />
                                        <asp:BoundField DataField="MaxPowerRequired" HeaderText="Max Power Required" />
                                        <asp:BoundField DataField="MaxPowerPriceAccepted" HeaderText="Max Power Price Accepted"/>
                                        <asp:BoundField DataField="VehicleID" HeaderText="Vehicle ID" />
                                        
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
                            <h2>Selected Record Information</h2></div></div></div>
                            <div class="mid-outer"><div class="mid-inner"><div class="mid">
                                <table style="vertical-align:top"> 
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Station Name</td>
                                        <td >
                                            <asp:Label ID="lStationName" runat="server" Text=""></asp:Label>
                                        </td>
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">ID</td>
                                        <td class="style1" >
                                           <asp:Label ID="lID" runat="server" Text=""></asp:Label>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">User ID</td>
                                        <td >
                                            <asp:Label ID="lUserID" runat="server" Text=""></asp:Label>
                                        </td>
                                            
                                    </tr>
                                    
                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Energy Price</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbEnergyPrice" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Charging Algorithm</td>
                                        <td >
                                            <asp:TextBox ID="tbChargingAlgorithm" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Is In Charging</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbIsInCharging" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Charging Times</td>
                                        <td >
                                            <asp:TextBox ID="tbChargingTimes" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Start Time</td>
                                        <td >
                                            <asp:TextBox ID="tbStartTime" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Start Main Power</td>
                                        <td >
                                            <asp:TextBox ID="tbStartMainPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Start Voltage</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbStartVoltage" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Start Current</td>
                                        <td >
                                            <asp:TextBox ID="tbStartCurrent" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Start PF</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbStartPF" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Start Active Power</td>
                                        <td >
                                            <asp:TextBox ID="tbStartActivePower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Start Apparent Power</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbStartApparentPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Priority</td>
                                        <td >
                                            <asp:TextBox ID="tbPriority" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Is End</td>
                                        <td >
                                            <asp:TextBox ID="tbIsEnd" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">End Time</td>
                                        <td >
                                            <asp:TextBox ID="tbEndTime" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">End Voltage</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbEndVoltage" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">End Current</td>
                                        <td >
                                            <asp:TextBox ID="tbEndCurrent" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">End PF</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbEndPF" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">End Active Power</td>
                                        <td >
                                            <asp:TextBox ID="tbEndActivePower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">End Apparent Power</td>
                                        <td class="style1" >
                                           <asp:TextBox ID="tbEndApparentPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Last Start Main Power</td>
                                        <td >
                                            <asp:TextBox ID="tbLastStartMainPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">End Main Power</td>
                                        <td >
                                            <asp:TextBox ID="tbEndMainPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Last Start Charging</td>
                                        <td >
                                            <asp:TextBox ID="tbLastStartCharging" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Last Stop Charging</td>
                                        <td >
                                            <asp:TextBox ID="tbLastStopCharging" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Last Stop Main Power</td>
                                        <td >
                                            <asp:TextBox ID="tbLastStopMainPower" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Is Ended By User</td>
                                        <td >
                                            <asp:TextBox ID="tbIsEndedByUser" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Schedule ID</td>
                                        <td >
                                            <asp:TextBox ID="tbScheduleID" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Charging Cost</td>
                                        <td >
                                            <asp:TextBox ID="tbChargingCost" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Total Charging Time</td>
                                        <td >
                                            <asp:TextBox ID="tbTotalChargingTime" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">SOC</td>
                                        <td >
                                            <asp:TextBox ID="tbSOC" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">SOC Retrieve Time</td>
                                        <td >
                                            <asp:TextBox ID="tbSOCRetrieveTime" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Calculate CO2</td>
                                        <td >
                                            <asp:TextBox ID="tbCalculateCO2" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Email Address</td>
                                        <td >
                                            <asp:TextBox ID="tbEmailAddress" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Zip Code</td>
                                        <td >
                                            <asp:TextBox ID="tbZipCode" runat="server" Width="161px" MaxLength="50" EmptyDataText="There are no crecords."></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Leave Time</td>
                                        <td >
                                            <asp:TextBox ID="tbLeaveTime" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Odometer Reading</td>
                                        <td >
                                            <asp:TextBox ID="tbOdometerReading" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Max Power Required</td>
                                        <td >
                                            <asp:TextBox ID="tbMaxPowerRequired" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Max Power Price Accepted</td>
                                        <td >
                                            <asp:TextBox ID="tbMaxPowerPriceAccepted" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td nowrap="nowrap" class="auto-style1">Vehicle ID</td>
                                        <td >
                                            <asp:TextBox ID="tbVehicleID" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>                                            
                                    </tr>

                                    <tr style="background-color: #F4F4F4">
                                        <td nowrap="nowrap" class="auto-style1">Aggregate Control</td>
                                        <td >
                                            <asp:TextBox ID="tbAggregateControl" runat="server" Width="161px" MaxLength="50"></asp:TextBox>
                                        </td>
                                    </tr>
                                </table>

                                
                            </div></div></div> <%--                <asp:Label ID="lblddlCreditCardPaymentError" runat="server" ForeColor="Red" Text=""></asp:Label>--%>

                            <div class="bottom-outer"><div class="bottom-inner">
                                <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
                            </div> <%--end bottom div x 3--%>
                        </div> <%-- end div round--%>
                        <asp:Button ID="btnUpdate" runat="server" Font-Size="medium" Text="Update" visible = "false" OnClick="btnUpdate_Click" /> 
                        <asp:Button ID="btnCloseCharging" runat="server" Font-Size="medium" Text="Close Charging" visible = "false" OnClick="btnClose_Charging" />   
                        <asp:Button ID="btnCancel" runat="server" Font-Size="medium" Text="Cancel" visible = "true" OnClick="btnCancel_Click" />
                        <br />
                    </td>

                </tr>
            </table>
        </asp:Content>
