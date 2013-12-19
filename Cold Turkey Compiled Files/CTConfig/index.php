<html>
	<head>
		<script type="text/javascript">
			function prep() {
				var dateVar = new Date()
				var offset = dateVar.getTimezoneOffset();
				document.cookie = "offset="+offset;
				window.location.replace("http://localhost:1990/settings.php");
			}
		</script>
	</head>
	<body onload="prep()">
	</body>
</html>