<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EVMaintenanceRecords.aspx.cs" Inherits="EVEditor.EVMaintenanceRecords" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .style1
        {
            width: 350px;
        }
        .auto-style1
        {
            width: 221px;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Maintenance Records</h2>
    <table>
        <tr>
            <td class="auto-style1">By Organization</td>
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
        <tr>
           <td class="auto-style1">Only Show Unresolved Records </td>
           <td>
               <asp:CheckBox ID="cbShowUnresolvedRecords" runat="server" 
                    AutoPostBack="True" 
                    oncheckedchanged="cbShowUnresolvedRecordsCheckedChanged" />
           </td>    
        </tr>
        <tr>
           <td class="auto-style1">Only Show Unclosed Records </td>
           <td>
               <asp:CheckBox ID="cbShowUnclosedRecords" runat="server" 
                    AutoPostBack="True" 
                    oncheckedchanged="cbShowUnclosedRecordsCheckedChanged" />
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
                    <h2>Maintenance Records</h2>
                    </div></div></div>
                    <div class="mid-outer"><div class="mid-inner"><div class="mid">
                    <asp:GridView ID="gvMaintenanceRecords" runat="server" CssClass="datatable" 
                            GridLines="Vertical"  OnPageIndexChanging="gvMaintenanceRecordsPaging" onselectedindexchanged="gvMaintenanceRecordsSelectedIndex" 
                            onrowcreated ="gvMaintenanceRecordsRowCreated" OnSorting="gvMaintenanceRecordsSorting"
                            AllowSorting="True" AllowPaging="True" CellPadding="0" 
                            BorderWidth="0px" AutoGenerateColumns="False" 
                            DataKeyNames="ID" Width="800px"> 
                        <Columns>
                            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/edit.jpg" ShowSelectButton="True" HeaderText="Select"/>
                            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                            <asp:BoundField DataField="Issue No" HeaderText="Issue No" SortExpression="Issue No" />
                            <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" SortExpression="Timestamp" />
                            <asp:BoundField DataField="UserID" HeaderText="UserID" SortExpression="UserID" />
                            <asp:BoundField DataField="Name" HeaderText="Username" SortExpression="Name" />
                            <asp:BoundField DataField="Feedback" HeaderText="Feedback" SortExpression="Feedback" />
                            <asp:BoundField DataField="MSG" HeaderText="MSG" SortExpression="MSG" />
                            <asp:CheckBoxField DataField="Closed" HeaderText="Closed" SortExpression="Closed" />
                            <asp:CheckBoxField DataField="Resolved" HeaderText="Resolved" SortExpression="Resolved" />
                            <asp:BoundField DataField="Resolved Date" HeaderText="Resolved Date" SortExpression="Resolved Date" />
                            <asp:BoundField DataField="Fixer" HeaderText="FixerID" SortExpression="Fixer" />

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

            <h2>Selected Feedback</h2></div></div></div>
            <div class="mid-outer"><div class="mid-inner"><div class="mid">
            <asp:table runat="server" style="vertical-align:top" Width="679px">
                <asp:TableRow><%-- feedback--%>
                    <asp:tablecell><%-- feedback information--%>
                        <table>
                            <tr>
                                <td nowrap="nowrap">Username</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:Label ID="lblUsername" runat="server" MaxLength="50" ></asp:Label>
           
                                </td>
                            </tr>
                            
                            <tr style="background-color: #F4F4F4">
                                <td nowrap="nowrap">Full Name</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1">
                                    <asp:Label ID="lblFullName" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>
                            
                            <tr>
                                <td nowrap="nowrap">Email</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:Label ID="lblEmail" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>

                            <tr style="background-color: #F4F4F4">
                                <td nowrap="nowrap">Role</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1">
                                    <asp:Label ID="lblRole" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>
                            
                            <tr>
                                <td nowrap="nowrap">Phone No</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:Label ID="lblPhone" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>
                            
                            <tr style="background-color: #F4F4F4">
                                <td nowrap="nowrap">EV Info</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:DropDownList ID="ddlEvInfo" runat="server" MaxLength="50" ></asp:DropDownList>
                                </td>
                            </tr>

                            <tr>
                                <td nowrap="nowrap">Issue No</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:Label ID="lblIssueNo" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>
                            
                            <tr style="background-color: #F4F4F4">
                                <td nowrap="nowrap">Issue Date</td>
                                <td style="color: #FF0000; font-weight: bold; font-size: medium"> </td>
                                <td class="style1" >
                                    <asp:Label ID="lblIssueDate" runat="server" MaxLength="50" ></asp:Label>
                                </td>
                            </tr>

                        </table>                            
                    </asp:TableCell>

                    <asp:TableCell>
                        <table>
                            <tr>
                                <td nowrap="nowrap">Feedback: </td>
                            </tr>
                            <tr style="background-color: #F4F4F4">
                                <td rowspan="5">
                                    <asp:TextBox ID="tbFeedback" TextMode = "MultiLine" Rows = "9" runat="server" Width="350px"></asp:TextBox>
                                </td>                             
                            </tr>
                        </table>
                    </asp:tablecell>                                    
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell></asp:TableCell>
                </asp:TableRow>
            </asp:table>
            <asp:Table ID = "rightSideTable" runat="server">
            </asp:Table>
            <asp:table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:label runat="server">New Response</asp:label>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <asp:TextBox ID="tbNewResponse" runat="server" Width="675px" TextMode="MultiLine" Rows="8"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:label runat="server">New Action</asp:label>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <asp:TextBox ID="tbNewAction" runat="server" Width="675px" TextMode="MultiLine" Rows="8"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>  
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:label runat="server">New Resolution</asp:label>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <asp:TextBox ID="tbNewResolution" runat="server" Width="675px" TextMode="MultiLine" Rows="8"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>  
                <asp:TableRow>
                    <asp:TableCell></asp:TableCell>
                </asp:TableRow>                 
            </asp:table>

            </div></div></div> 
            <div class="bottom-outer"><div class="bottom-inner">
            <div class="bottom"></div></div></div> <%-- End mid div x 3--%>
            </div> <%--end bottom div x 3--%>
            </div> <%-- end div round--%>
            <asp:Button ID="btnSubmit" runat="server" Font-Size="medium" Text="Submit" visible = "false" onclick="btnSubmitClick"/>
            <asp:Button ID = "btnResolveAndClose" runat="server" Font-Size="medium" Text="Resolve and Close" Visible="false" OnClick="btnResolveAndCloseClick"  />  
            <asp:Button ID="btnResolve" runat="server" Font-Size="medium" Text="Resolve" visible = "false" OnClick="btnResolveClick" /> 
            <asp:Button ID="btnClose" runat="server" Font-Size="medium" Text="Close" visible = "false" OnClick="btnCloseClick" />   
            <asp:Button ID="btnCancel" runat="server" Font-Size="medium" Text="Reset" visible = "false" OnClick="btnCancelClick" />
            <asp:Button ID="btnFeedback" runat="server" Font-Size="medium" Text="New Feedback" visible = "true" OnClick="btnFeedbackClick" />
            <br />
            </td>
        </tr>
    </table>
    <br />
    
</asp:Content>
