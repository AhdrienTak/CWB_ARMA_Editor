<?php

if (isset($_POST['user']) && isset($_POST['pass'])) {

	$response = "";
	
	// Check if username exists
	if (is_file('_' . $_POST['user'] . '.txt')) {
		$response = "Error: Username already exists!";
	}
	
	// Otherwise, create the account
	else {
		$newUser = fopen('_' . $_POST['user'] . '.txt', "w");
		fwrite($newUser, $_POST['pass']);
		fclose($newUser);
		$response = "User " . $_POST['user'] . " has been created.";
	}
	
	die($response);
}

else {
	die("Error: Bad form for credential creation.");
}

?>