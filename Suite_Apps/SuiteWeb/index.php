<?php
header("Last-Modified: " . gmdate("D, d M Y H:i:s") . " GMT");
header("Cache-Control: no-store, no-cache, must-revalidate");
header("Cache-Control: post-check=0, pre-check=0", false);
header("Pragma: no-cache");

require_once("constants/database.php");
require_once("functions/safemysql.class.php");
    
?>
<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <title>Suite: Home</title>
    </head>
    <body>
	<p>Suite Web Content</p>
		<br/>
		<a href="pma/index.php">phpMyAdmin</a><br />
    </body>
</html>
