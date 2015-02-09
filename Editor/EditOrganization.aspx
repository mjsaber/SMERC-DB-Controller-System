<%@ Page Title="Edit Organization" Language="C#"  AutoEventWireup="true" CodeBehind="EditOrganization.aspx.cs" MasterPageFile="~/Editor.master"  Inherits="EVEditor.EditOrganization" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">

        .style1
        {
            width: 400px;
        }

    </style>
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Edit Organization</h2>
    <p>
     <asp:CheckBox ID="cbShowActivated" runat="server" 
            AutoPostBack="True" 
            oncheckedchanged="cbShowActivated_CheckedChanged" />
        <strong>Show Only Activated</strong>
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
                    <h2>Organization List</h2>
                </div></div></div>
           
             <div class="mid-outer"><div class="mid-inner"><div class="mid"> 

        <asp:GridView ID="gvOrganization" runat="server" CssClass="datatable" 
                     GridLines="Vertical"
           OnPageIndexChanging="gvOrganizationPaging" onrowcreated="gvOrganizationRowCreated" 
                     OnSorting="gvOrganizationSorting" onselectedindexchanged="gvOrganizationSelectedIndex"
           AllowSorting="True" AllowPaging="True" CellPadding="0" 
                     BorderWidth="0px" AutoGenerateColumns="False" 
                     DataKeyNames="ID" Width="700px" PageSize="10">
        <Columns>
            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>                                  
            <asp:BoundField DataField="Organization" HeaderText="Organization" SortExpression="Organization" />
            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" />
            <asp:BoundField DataField="Latitude" HeaderText="Latitude" SortExpression="Latitude" />
            <asp:BoundField DataField="Longitude" HeaderText="Longitude" SortExpression="Longitude" />
            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />  
            <asp:BoundField DataField="Email Password" HeaderText="Email Password" SortExpression="Email Password"/>              
            <asp:BoundField DataField="Email Host" HeaderText="Email Host" SortExpression="Email Host" />              
            <asp:CheckBoxField DataField="EnableSSL" HeaderText="Enable SSL"  SortExpression="EnableSSL" />    
            <asp:BoundField DataField="Email Port" HeaderText="Email Port" SortExpression="Email Port"/>  
            <asp:CheckBoxField DataField="CO2Index" HeaderText="CO2 Index"  SortExpression="CO2Index" />     
            <asp:BoundField DataField="PriceTransactionNode" HeaderText="Price Transaction Node" SortExpression="PriceTransactionNode"/>       
            <asp:BoundField DataField="EnergyPrice1" HeaderText="Energy Price 1" SortExpression="EnergyPrice1"/>
            <asp:BoundField DataField="EnergyPrice2" HeaderText="Energy Price 2" SortExpression="EnergyPrice2"/>
            <asp:BoundField DataField="EnergyPrice3" HeaderText="Energy Price 3" SortExpression="EnergyPrice3"/>
            <asp:BoundField DataField="EnergyPrice4" HeaderText="Energy Price 4" SortExpression="EnergyPrice4"/>
            <asp:BoundField DataField="EnergyPrice5" HeaderText="Energy Price 5" SortExpression="EnergyPrice5"/>
            <asp:BoundField DataField="EnergyPrice6" HeaderText="Energy Price 6" SortExpression="EnergyPrice6"/>
            <asp:BoundField DataField="EnergyPrice7" HeaderText="Energy Price 7" SortExpression="EnergyPrice7"/>
            <asp:BoundField DataField="EnergyPrice8" HeaderText="Energy Price 8" SortExpression="EnergyPrice8"/>
            <asp:BoundField DataField="EnergyPrice9" HeaderText="Energy Price 9" SortExpression="EnergyPrice9"/>
            <asp:BoundField DataField="EnergyPrice10" HeaderText="Energy Price 10" SortExpression="EnergyPrice10"/>
            <asp:BoundField DataField="EnergyPrice11" HeaderText="Energy Price 11" SortExpression="EnergyPrice11"/>
            <asp:BoundField DataField="EnergyPrice12" HeaderText="Energy Price 12" SortExpression="EnergyPrice12"/>
            <asp:BoundField DataField="EnergyPrice13" HeaderText="Energy Price 13" SortExpression="EnergyPrice13"/>
            <asp:BoundField DataField="EnergyPrice14" HeaderText="Energy Price 14" SortExpression="EnergyPrice14"/>
            <asp:BoundField DataField="EnergyPrice15" HeaderText="Energy Price 15" SortExpression="EnergyPrice15"/>
            <asp:BoundField DataField="EnergyPrice16" HeaderText="Energy Price 16" SortExpression="EnergyPrice16"/>
            <asp:BoundField DataField="EnergyPrice17" HeaderText="Energy Price 17" SortExpression="EnergyPrice17"/>
            <asp:BoundField DataField="EnergyPrice18" HeaderText="Energy Price 18" SortExpression="EnergyPrice18"/>
            <asp:BoundField DataField="EnergyPrice19" HeaderText="Energy Price 19" SortExpression="EnergyPrice19"/>
            <asp:BoundField DataField="EnergyPrice20" HeaderText="Energy Price 20" SortExpression="EnergyPrice20"/>
            <asp:BoundField DataField="EnergyPrice21" HeaderText="Energy Price 21" SortExpression="EnergyPrice21"/>
            <asp:BoundField DataField="EnergyPrice22" HeaderText="Energy Price 22" SortExpression="EnergyPrice22"/>
            <asp:BoundField DataField="EnergyPrice23" HeaderText="Energy Price 23" SortExpression="EnergyPrice23"/>
            <asp:BoundField DataField="EnergyPrice24" HeaderText="Energy Price 24" SortExpression="EnergyPrice24"/>            
            <asp:BoundField DataField="PriceAdjustment1" HeaderText="Price Adjustment 1" SortExpression="PriceAdjustment1"/>
            <asp:BoundField DataField="PriceAdjustment2" HeaderText="Price Adjustment 2" SortExpression="PriceAdjustment2"/>
            <asp:BoundField DataField="PriceAdjustment3" HeaderText="Price Adjustment 3" SortExpression="PriceAdjustment3"/>
            <asp:BoundField DataField="PriceAdjustment4" HeaderText="Price Adjustment 4" SortExpression="PriceAdjustment4"/>
            <asp:BoundField DataField="PriceAdjustment5" HeaderText="Price Adjustment 5" SortExpression="PriceAdjustment5"/>
            <asp:BoundField DataField="PriceAdjustment6" HeaderText="Price Adjustment 6" SortExpression="PriceAdjustment6"/>
            <asp:BoundField DataField="PriceAdjustment7" HeaderText="Price Adjustment 7" SortExpression="PriceAdjustment7"/>
            <asp:BoundField DataField="PriceAdjustment8" HeaderText="Price Adjustment 8" SortExpression="PriceAdjustment8"/>
            <asp:BoundField DataField="PriceAdjustment9" HeaderText="Price Adjustment 9" SortExpression="PriceAdjustment9"/>
            <asp:BoundField DataField="PriceAdjustment10" HeaderText="Price Adjustment 10" SortExpression="PriceAdjustment10"/>
            <asp:BoundField DataField="PriceAdjustment11" HeaderText="Price Adjustment 11" SortExpression="PriceAdjustment11"/>
            <asp:BoundField DataField="PriceAdjustment12" HeaderText="Price Adjustment 12" SortExpression="PriceAdjustment12"/>
            <asp:BoundField DataField="PriceAdjustment13" HeaderText="Price Adjustment 13" SortExpression="PriceAdjustment13"/>
            <asp:BoundField DataField="PriceAdjustment14" HeaderText="Price Adjustment 14" SortExpression="PriceAdjustment14"/>
            <asp:BoundField DataField="PriceAdjustment15" HeaderText="Price Adjustment 15" SortExpression="PriceAdjustment15"/>
            <asp:BoundField DataField="PriceAdjustment16" HeaderText="Price Adjustment 16" SortExpression="PriceAdjustment16"/>
            <asp:BoundField DataField="PriceAdjustment17" HeaderText="Price Adjustment 17" SortExpression="PriceAdjustment17"/>
            <asp:BoundField DataField="PriceAdjustment18" HeaderText="Price Adjustment 18" SortExpression="PriceAdjustment18"/>
            <asp:BoundField DataField="PriceAdjustment19" HeaderText="Price Adjustment 19" SortExpression="PriceAdjustment19"/>
            <asp:BoundField DataField="PriceAdjustment20" HeaderText="Price Adjustment 20" SortExpression="PriceAdjustment20"/>
            <asp:BoundField DataField="PriceAdjustment21" HeaderText="Price Adjustment 21" SortExpression="PriceAdjustment21"/>
            <asp:BoundField DataField="PriceAdjustment22" HeaderText="Price Adjustment 22" SortExpression="PriceAdjustment22"/>
            <asp:BoundField DataField="PriceAdjustment23" HeaderText="Price Adjustment 23" SortExpression="PriceAdjustment23"/>
            <asp:BoundField DataField="PriceAdjustment24" HeaderText="Price Adjustment 24" SortExpression="PriceAdjustment24"/>
            <asp:BoundField DataField="ConsumerKey" HeaderText="Consumer Key" SortExpression="ConsumerKey"/>
            <asp:BoundField DataField="ConsumerSecret" HeaderText="Consumer Secret" SortExpression="ConsumerSecret"/>
            <asp:BoundField DataField="AccessToken" HeaderText="Access Token" SortExpression="AccessToken"/>
            <asp:BoundField DataField="AccessTokenSecret" HeaderText="Access Token Secret" SortExpression="AccessTokenSecret"/>
            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" /> 
            <asp:CheckBoxField DataField="Activate" HeaderText="Activate"  SortExpression="Activate" />
            <asp:BoundField DataField="AllowUserAccountExpiration" HeaderText="Allow User Account Expiration" SortExpression="AllowUserAccountExpiration"/>
            <asp:BoundField DataField="EVUserAccountTypeID" HeaderText="EV User Account Type ID" SortExpression="EVUserAccountTypeID"/>
            <asp:BoundField DataField="RTMCUserAccountTypeID" HeaderText="RTMC User Account Type ID" SortExpression="RTMCUserAccountTypeID"/>
            <asp:BoundField DataField="Level1EnergyRetailAdjustment" HeaderText="Level1 Energy Retail Adjustment" SortExpression="Level1EnergyRetailAdjustment"/>
            <asp:BoundField DataField="Level2EnergyRetailAdjustment" HeaderText="Level2 Energy Retail Adjustment" SortExpression="Level2EnergyRetailAdjustment"/>
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
        <td nowrap="nowrap" style="width:180px">Organization</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbOrganization" runat="server"></asp:TextBox>
            <asp:Label ID="lbltbOrganizationError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td></tr>

     <tr>
        <td nowrap="nowrap">State</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:DropDownList ID="ddlState" runat="server">
            </asp:DropDownList>
            <asp:Label ID="lblddlStateError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">Latitude</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbLatitude" runat="server"></asp:TextBox>
            <asp:RangeValidator ID="rvtbLatitude" runat="server" 
            ControlToValidate="tbLatitude" Display="Dynamic" ErrorMessage="Values must be between -90 and 90." MaximumValue="90" 
            MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            <asp:Label ID="lbltbLatitudeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        &nbsp;</td></tr>

        <tr>
        <td nowrap="nowrap">Longitude</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:TextBox ID="tbLongitude" runat="server"></asp:TextBox>
            <asp:RangeValidator ID="rvtbLongitude" runat="server" 
            ControlToValidate="tbLongitude" ErrorMessage="Values must be between -180 and 180." MaximumValue="180" 
            MinimumValue="-180" ForeColor="Red" Display="Dynamic" Type="Double"></asp:RangeValidator>

            <asp:Label ID="lbltbLongitudeError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
         </td>
     </tr>

     <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap">Activate</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1" >
            <asp:DropDownList ID="ddlActivate" runat="server">
                <asp:ListItem Value="1">True</asp:ListItem>
                <asp:ListItem Value="0">False</asp:ListItem>
            </asp:DropDownList>
         </td>
     </tr>

     <tr>
        <td nowrap="nowrap" style="width:180px">Email</td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbEmail" runat="server" AutoComplete="off"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfltbEmail" 
                runat="server" Display="Dynamic" 
                ErrorMessage="Email Required." ForeColor="Red" 
                ControlToValidate="tbEmail"></asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="reqAuthEmail" 
                RunAt="server" ControlToValidate="tbEmail"
                ErrorMessage="Valid Email Address required" ForeColor="Red" 
                ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,3}$" />
            <asp:Label ID="lbltbEmailError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td></tr>

        <tr style="background-color: #F4F4F4">
        <td nowrap="nowrap" style="width:180px">
            <asp:Label ID="lblPassword" runat="server" 
                Text="Password (Show)*" 
                ToolTip="Check the checkbox to show the password"></asp:Label>
            <asp:CheckBox 
                ID="cbShowPassword" runat="server" 
                oncheckedchanged="cbShowPassword_CheckedChanged" 
                AutoPostBack="True" />
            </td>
        <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
        <td class="style1">
            <asp:TextBox ID="tbPassword" runat="server" AutoComplete="Off"
                TextMode="Password"></asp:TextBox>
           <asp:RequiredFieldValidator ID="rfltbPassword" 
                runat="server" Display="Dynamic" 
                ErrorMessage="Password Required." ForeColor="Red" 
                ControlToValidate="tbPassword"></asp:RequiredFieldValidator>
            <asp:Label ID="lbltbPasswordError" runat="server" ForeColor="Red" 
                Text=""></asp:Label>
        </td></tr>

        <tr>
            <td>Email Host</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>
                <asp:TextBox ID="tbEmailHost" runat="server"></asp:TextBox>
            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>SSL Enabled</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>            
                <asp:DropDownList ID="ddlEnableSSL" runat="server">
                    <asp:ListItem Value="1">True</asp:ListItem>
                    <asp:ListItem Value="0">False</asp:ListItem>
                </asp:DropDownList>            
            </td>
        </tr>

        <tr>
            <td>Email Port</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>
                <asp:TextBox ID="tbEmailPort" runat="server"></asp:TextBox>
            <asp:RangeValidator ID="rvtbEmailPort" runat="server" 
            ControlToValidate="tbEmailPort" Display="Dynamic" 
                    ErrorMessage="Numeric values only" MaximumValue="90000" 
            MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>CO2 Index</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>            
                <asp:DropDownList ID="ddlCO2Index" runat="server">
                    <asp:ListItem Value="1">True</asp:ListItem>
                    <asp:ListItem Value="0">False</asp:ListItem>
                </asp:DropDownList>            
            </td>
        </tr>

        <tr>
            <td>Price Transaction Node</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:DropDownList ID="ddlPriceTransactionNode" runat="server"></asp:DropDownList>
            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 1</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice1" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice1" runat="server" 
                ControlToValidate="tbEnergyPrice1" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 2</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice2" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice2" runat="server" 
                ControlToValidate="tbEnergyPrice2" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 3</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice3" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice3" runat="server" 
                ControlToValidate="tbEnergyPrice3" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 4</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice4" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice4" runat="server" 
                ControlToValidate="tbEnergyPrice4" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 5</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice5" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice5" runat="server" 
                ControlToValidate="tbEnergyPrice5" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 6</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice6" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice6" runat="server" 
                ControlToValidate="tbEnergyPrice6" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 7</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice7" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice7" runat="server" 
                ControlToValidate="tbEnergyPrice7" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 8</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice8" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="rvtbEnergyPrice8" runat="server" 
                ControlToValidate="tbEnergyPrice8" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Energy Price 9</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice9" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator9" runat="server" 
                ControlToValidate="tbEnergyPrice9" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 10</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice10" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator10" runat="server" 
                ControlToValidate="tbEnergyPrice10" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 11</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice11" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator11" runat="server" 
                ControlToValidate="tbEnergyPrice11" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 12</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice12" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator12" runat="server" 
                ControlToValidate="tbEnergyPrice12" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 13</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice13" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator13" runat="server" 
                ControlToValidate="tbEnergyPrice13" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 14</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice14" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator14" runat="server" 
                ControlToValidate="tbEnergyPrice14" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 15</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice15" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator15" runat="server" 
                ControlToValidate="tbEnergyPrice15" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 16</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice16" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator16" runat="server" 
                ControlToValidate="tbEnergyPrice16" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Energy Price 17</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice17" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator17" runat="server" 
                ControlToValidate="tbEnergyPrice17" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 18</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice18" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator18" runat="server" 
                ControlToValidate="tbEnergyPrice18" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 19</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice19" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator19" runat="server" 
                ControlToValidate="tbEnergyPrice19" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 20</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice20" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator20" runat="server" 
                ControlToValidate="tbEnergyPrice20" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 21</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice21" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator21" runat="server" 
                ControlToValidate="tbEnergyPrice21" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 22</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice22" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator22" runat="server" 
                ControlToValidate="tbEnergyPrice22" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Energy Price 23</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice23" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator23" runat="server" 
                ControlToValidate="tbEnergyPrice23" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Energy Price 24</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbEnergyPrice24" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator24" runat="server" 
                ControlToValidate="tbEnergyPrice24" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 1</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment1" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator25" runat="server" 
                ControlToValidate="tbPriceAdjustment1" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 2</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment2" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator26" runat="server" 
                ControlToValidate="tbPriceAdjustment2" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 3</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment3" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator27" runat="server" 
                ControlToValidate="tbPriceAdjustment3" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 4</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment4" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator28" runat="server" 
                ControlToValidate="tbPriceAdjustment4" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 5</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment5" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator29" runat="server" 
                ControlToValidate="tbPriceAdjustment5" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 6</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment6" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator30" runat="server" 
                ControlToValidate="tbPriceAdjustment6" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 7</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment7" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator31" runat="server" 
                ControlToValidate="tbPriceAdjustment7" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 8</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment8" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator32" runat="server" 
                ControlToValidate="tbPriceAdjustment8" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 9</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment9" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator33" runat="server" 
                ControlToValidate="tbPriceAdjustment9" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 10</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment10" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator34" runat="server" 
                ControlToValidate="tbPriceAdjustment10" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 11</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment11" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator35" runat="server" 
                ControlToValidate="tbPriceAdjustment11" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 12</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment12" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator36" runat="server" 
                ControlToValidate="tbPriceAdjustment12" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 13</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment13" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator37" runat="server" 
                ControlToValidate="tbPriceAdjustment13" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 14</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment14" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator38" runat="server" 
                ControlToValidate="tbPriceAdjustment14" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 15</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment15" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator39" runat="server" 
                ControlToValidate="tbPriceAdjustment15" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 16</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment16" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator40" runat="server" 
                ControlToValidate="tbPriceAdjustment16" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 17</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment17" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator41" runat="server" 
                ControlToValidate="tbPriceAdjustment17" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 18</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment18" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator42" runat="server" 
                ControlToValidate="tbPriceAdjustment18" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 19</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment19" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator43" runat="server" 
                ControlToValidate="tbPriceAdjustment19" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 20</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment20" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator44" runat="server" 
                ControlToValidate="tbPriceAdjustment20" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 21</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment21" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator45" runat="server" 
                ControlToValidate="tbPriceAdjustment21" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 22</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment22" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator46" runat="server" 
                ControlToValidate="tbPriceAdjustment22" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Price Adjustment 23</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment23" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator47" runat="server" 
                ControlToValidate="tbPriceAdjustment23" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr>
            <td>Price Adjustment 24</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbPriceAdjustment24" runat="server"></asp:TextBox>        
                <asp:RangeValidator ID="RangeValidator48" runat="server" 
                ControlToValidate="tbPriceAdjustment24" Display="Dynamic" 
                        ErrorMessage="Numeric values only" MaximumValue="90000" 
                MinimumValue="-90" ForeColor="Red" Type="Double"></asp:RangeValidator>

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>First Time Entering<br />
                Twitter Codes?</td>
            <td></td>
            <td>            
                Check to encrypt Values Below:<asp:CheckBox ID="cbTwitterInput" runat="server" />

                <br />
                <br />
                *If it is the initial input of Twitter keys, <br />
                Encrypting is <strong>required</strong> for security purposes.</td>
        </tr>

         <tr>
            <td>Consumer Key</td>
            <td></td>
            <td>            
                <asp:TextBox ID="tbConsumerKey" runat="server" Width="400px"></asp:TextBox>        

            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>Consumer Secret</td>
            <td></td>
            <td>            
                <asp:TextBox ID="tbConsumerSecret" runat="server" Width="400px"></asp:TextBox>        

            </td>
        </tr>

        <tr>
            <td>Access Token</td>
            <td></td>
            <td>            
                <asp:TextBox ID="tbAccessToken" runat="server" Width="400px"></asp:TextBox>        

            </td>
        </tr>

         <tr style="background-color: #F4F4F4">
            <td>Access Token Secret</td>
            <td></td>
            <td>            
                <asp:TextBox ID="tbAccessTokenSecret" runat="server" Width="400px"></asp:TextBox>        

            </td>
        </tr>
        
        <tr>
            <td>Allow User Account Expiration</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>
                <asp:DropDownList ID="ddlAllowUserAccountExpiration" runat="server">
                    <asp:ListItem Value="1">True</asp:ListItem>
                    <asp:ListItem Value="0">False</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>

        <tr style="background-color: #F4F4F4">
            <td>EV User Account Type</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>            
                <asp:DropDownList ID="ddlEVUserAccountType" runat="server">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="vldrEVUserAccountType" 
                runat="server" ControlToValidate="ddlEVUserAccountType" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>            
            </td>
        </tr>

        <tr>
            <td>RTMC User Account Type</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium">*</td>
            <td>
                <asp:DropDownList ID="ddlRTMCUserAccountType" runat="server">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="vldrddlRTMCUserAccountType" 
                runat="server" ControlToValidate="ddlRTMCUserAccountType" 
                Display="Dynamic" ErrorMessage="Required Field." 
                ForeColor="Red" InitialValue="-1"></asp:RequiredFieldValidator>
            </td>
        </tr>
        
        <tr style="background-color: #F4F4F4">
            <td>Level1 Energy Retail Adjustment</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>            
                <asp:TextBox ID="tbLevel1EnergyRetailAdjustment" runat="server" Text="0"></asp:TextBox>      
            </td>
        </tr>

        <tr>
            <td>Level2 Energy Retail Adjustment</td>
            <td style="color: #FF0000; font-weight: bold; font-size: medium"></td>
            <td>
                <asp:TextBox ID="tbLevel2EnergyRetailAdjustment" runat="server" Text="0"></asp:TextBox>
            </td>
        </tr>
     


     </table>

    
    </div></div></div> <%-- End mid div x 3--%>

    <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%--end bottom div x 3--%>
       </div> <%-- end div round--%>
       </div> <%-- end div grid --%>




          
       <asp:Button ID="btnUpdate" runat="server" Font-Size="Large" Text="Update" visible = "false" onclick="btnUpdateClick"/>    
       <asp:Button ID="btnNew" runat="server" Font-Size="Large" Text="New" visible = "true" onclick="btnNewClick"/>      
       <asp:Button ID="Clear" runat="server" Font-Size="Large" Text="Clear" visible = "true" CausesValidation="false" onclick="btnClearClick"/>       
                    <br />
    </td>
    </tr>
    </table>
                      
        <br />
        </asp:Content>