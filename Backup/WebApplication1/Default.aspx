<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link href="css/ui-lightness/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <script src="js/jquery-1.4.2.js" type="text/javascript"></script>
    <script src="js/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" action="Default.aspx" method="get">
    <div>
        <input type="text" value='<%=Request["kw"] %>' id="txtKw" name="kw" />
        <script type="text/javascript">
            $("#txtKw").autocomplete({ source: "SearchSug.ashx", select: function(e, ui) {
             $("#txtKw").val(ui.item.value); $("#sb").click(); } });
        </script>
        <input type="submit" id="sb" value="搜索" />      
        <input type="submit" name="btn1" value="搜索2" />      
        <input type="submit" name="btn2" value="搜索3" />      
    </div>
    <asp:Repeater ID="RepeaterResult" runat="server">
        <ItemTemplate><a style="background-color:Red" href='<%#Eval("URL") %>'><%#Eval("Title") %></a><br /><p><%#Eval("Body") %></p></ItemTemplate>
    </asp:Repeater>
    </form>
</body>
</html>
