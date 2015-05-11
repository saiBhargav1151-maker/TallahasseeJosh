Add the following one line to head section of the _Layout.cshtml, or directly in any views that wish to use Ajax Exception Handling:

<script src="@Url.Content("~/Scripts/AjaxExceptionHandling.js")" type="text/javascript"></script>

E.g.
<html>
	<head>
		<script src="@Url.Content("~/Scripts/AjaxExceptionHandling.js")" type="text/javascript"></script>
    </head>
</html>